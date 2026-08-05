using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldMapController : MonoBehaviour
{
    [SerializeField] private MapNodeView nodePrefab;
    [SerializeField] private RectTransform mapRoot;
    [SerializeField] private RectTransform lineRoot;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private ScrollRect mapScrollRect;
    [SerializeField] private GameObject eventPanel;
    [SerializeField] private TMP_Text eventText;
    [SerializeField] private Button eventContinueButton;
    [SerializeField] private GameObject toastPanel;
    [SerializeField] private TMP_Text toastText;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Button newRunButton;
    [SerializeField] private GameObject layoutSelectPanel;
    [SerializeField] private Button azertyButton;
    [SerializeField] private Button qwertyButton;

    [Header("Configuration (ScriptableObjects)")]
    [SerializeField] private DungeonFloorConfig[] floors;
    [SerializeField] private DifficultyCurveConfig difficultyCurve;

    private const float RowSpacing = 150f;
    private const float ColumnSpacing = 260f;
    private const float TopPadding = 80f;
    // Les cases (200x100, pivot en haut) descendent sous leur point d'ancrage: la marge basse
    // doit depasser leur hauteur pour laisser de l'air sous la rangee de depart, sinon elle
    // deborde du contenu scrollable et se retrouve collee/rognee contre le bord du viewport.
    private const float BottomPadding = 150f;

    private readonly List<MapNodeView> spawnedNodes = new List<MapNodeView>();
    private readonly List<GameObject> spawnedLines = new List<GameObject>();

    private void Start()
    {
        LevelSession.Configure(floors, difficultyCurve);
        LevelSession.EnsureMap();

        eventPanel.SetActive(false);
        toastPanel.SetActive(false);
        victoryPanel.SetActive(false);

        newRunButton.onClick.AddListener(OnStartNewRun);
        eventContinueButton.onClick.AddListener(() => eventPanel.SetActive(false));
        azertyButton.onClick.AddListener(() => ChooseLayout(KeyboardLayoutType.AZERTY));
        qwertyButton.onClick.AddListener(() => ChooseLayout(KeyboardLayoutType.QWERTY));

        layoutSelectPanel.SetActive(!KeyboardPreference.HasChosen);

        if (LevelSession.HasWonRun)
        {
            victoryPanel.SetActive(true);
            return;
        }

        if (LevelSession.JustAdvancedFloor)
        {
            LevelSession.JustAdvancedFloor = false;
            ShowToast("Etage " + (LevelSession.CurrentFloorIndex + 1) + " !");
        }

        RefreshMap();
    }

    private void ChooseLayout(KeyboardLayoutType layout)
    {
        KeyboardPreference.Layout = layout;
        layoutSelectPanel.SetActive(false);
    }

    private void RefreshMap()
    {
        foreach (MapNodeView view in spawnedNodes)
            Destroy(view.gameObject);
        spawnedNodes.Clear();

        foreach (GameObject line in spawnedLines)
            Destroy(line);
        spawnedLines.Clear();

        // Le contenu grandit avec le nombre de rangees pour que la carte reste scrollable
        // quelle que soit la hauteur du DungeonFloorConfig courant.
        int maxRow = LevelSession.Map.Count - 1;
        contentRoot.sizeDelta = new Vector2(0f, TopPadding + maxRow * RowSpacing + BottomPadding);

        List<MapNode> available = LevelSession.GetAvailableNodes();

        DrawConnections();

        foreach (List<MapNode> row in LevelSession.Map)
        {
            foreach (MapNode node in row)
            {
                bool isAvailable = available.Contains(node);
                MapNodeView view = Instantiate(nodePrefab, mapRoot);
                RectTransform rt = view.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = GetPosition(node);
                view.Setup(node, isAvailable);
                view.Button.onClick.AddListener(() => OnNodeClicked(node));
                spawnedNodes.Add(view);
            }
        }

        // Vue par defaut: en bas du contenu, sur la position actuelle du joueur.
        if (mapScrollRect != null)
            mapScrollRect.verticalNormalizedPosition = 0f;
    }

    // Position relative au coin superieur du contenu scrollable: la rangee la plus profonde
    // (le boss) est en haut, la rangee de depart en bas, quel que soit le nombre de rangees.
    private Vector2 GetPosition(MapNode node)
    {
        int rowCount = LevelSession.Map[node.Row].Count;
        float totalWidth = (rowCount - 1) * ColumnSpacing;
        float x = node.Column * ColumnSpacing - totalWidth / 2f;
        int maxRow = LevelSession.Map.Count - 1;
        float y = -TopPadding - (maxRow - node.Row) * RowSpacing;
        return new Vector2(x, y);
    }

    private void DrawConnections()
    {
        for (int row = 0; row < LevelSession.Map.Count - 1; row++)
        {
            foreach (MapNode node in LevelSession.Map[row])
            {
                foreach (int targetColumn in node.Connections)
                {
                    MapNode target = LevelSession.Map[row + 1].Find(n => n.Column == targetColumn);
                    if (target != null)
                        DrawLine(GetPosition(node), GetPosition(target));
                }
            }
        }
    }

    private void DrawLine(Vector2 from, Vector2 to)
    {
        GameObject lineGo = new GameObject("Line", typeof(RectTransform), typeof(Image));
        lineGo.transform.SetParent(lineRoot, false);

        Image image = lineGo.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.25f);
        image.raycastTarget = false;

        Vector2 diff = to - from;
        float distance = diff.magnitude;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        RectTransform rt = lineGo.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(distance, 4f);
        rt.anchoredPosition = from + diff / 2f;
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);

        spawnedLines.Add(lineGo);
    }

    private void OnNodeClicked(MapNode node)
    {
        switch (node.Type)
        {
            case MapNodeType.Rest:
                LevelSession.PlayerHp = LevelSession.PlayerMaxHp;
                LevelSession.CompleteNodeImmediately(node);
                ShowToast("Vous vous reposez. PV restaures.");
                RefreshMap();
                break;

            case MapNodeType.Event:
                ResolveEvent(node);
                break;

            default:
                LevelSession.SelectCombatNode(node);
                SceneManager.LoadScene("SampleScene");
                break;
        }
    }

    // Evenements tires dans la liste de l'etage courant (DungeonFloorConfig.possibleEvents).
    private void ResolveEvent(MapNode node)
    {
        LevelSession.CompleteNodeImmediately(node);

        EventDefinition chosenEvent = PickEvent();
        if (chosenEvent != null)
        {
            if (chosenEvent.healAmount > 0)
                LevelSession.PlayerHp = Mathf.Min(LevelSession.PlayerMaxHp, LevelSession.PlayerHp + chosenEvent.healAmount);
            eventText.text = chosenEvent.message;
        }
        else
        {
            eventText.text = "Il ne se passe rien de particulier ici.";
        }

        eventPanel.SetActive(true);
        RefreshMap();
    }

    private EventDefinition PickEvent()
    {
        DungeonFloorConfig floor = LevelSession.CurrentFloorConfig;
        if (floor == null || floor.possibleEvents == null || floor.possibleEvents.Length == 0)
            return null;

        return floor.possibleEvents[Random.Range(0, floor.possibleEvents.Length)];
    }

    private void ShowToast(string message)
    {
        toastText.text = message;
        toastPanel.SetActive(true);
        CancelInvoke(nameof(HideToast));
        Invoke(nameof(HideToast), 2f);
    }

    private void HideToast()
    {
        toastPanel.SetActive(false);
    }

    private void OnStartNewRun()
    {
        LevelSession.StartNewRun();
        victoryPanel.SetActive(false);
        RefreshMap();
    }
}
