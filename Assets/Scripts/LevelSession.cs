using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Etat statique de la run en cours, survit au chargement de scene (Map <-> Gameplay).
public static class LevelSession
{
    public const int StartingHp = 3;

    public static DungeonFloorConfig[] Floors { get; private set; }
    public static DifficultyCurveConfig DifficultyCurve { get; private set; }

    public static List<List<MapNode>> Map { get; private set; }
    public static int CurrentFloorIndex { get; private set; }
    public static int CurrentRow { get; private set; } = -1;
    public static int CurrentColumn { get; private set; } = -1;
    public static MapNode PendingNode { get; private set; }
    public static LevelDefinition Current { get; private set; }
    public static bool HasWonRun { get; private set; }

    // Vrai juste apres un changement d'etage, le temps pour l'UI d'afficher un message puis de le consommer.
    public static bool JustAdvancedFloor;

    public static int PlayerHp = StartingHp;
    public static int PlayerMaxHp = StartingHp;

    // Stats globales accumulees via les bonus/malus de fin de combat (Phase 4). Remises a zero a
    // chaque nouvelle run.
    public static float BonusDashSpeed;
    public static float BonusContactDamage;
    public static float BonusEnemySpawnInterval;

    // Touches liees a un effet permanent par le joueur (Heal/Broken), actives pour tous les
    // combats restants de la run.
    public static readonly Dictionary<char, PermanentKeyEffectType> PermanentKeyEffects = new Dictionary<char, PermanentKeyEffectType>();

    // Sequence de touches assignee a chaque sort pour la run en cours (Phase 5): toujours
    // differente d'une run a l'autre, stable du debut a la fin de celle-ci.
    public static Dictionary<SpellDefinition, char[]> SpellSequences { get; private set; } = new Dictionary<SpellDefinition, char[]>();

    // Sorts dont le joueur connait la sequence (revele par un event, ou devine/lance par hasard).
    public static readonly HashSet<SpellDefinition> DiscoveredSpells = new HashSet<SpellDefinition>();

    public static void Configure(DungeonFloorConfig[] floors, DifficultyCurveConfig difficultyCurve)
    {
        Floors = floors;
        DifficultyCurve = difficultyCurve;
    }

    public static void EnsureMap()
    {
        if (Map == null)
            StartNewRun();
    }

    public static void StartNewRun()
    {
        CurrentFloorIndex = 0;
        HasWonRun = false;
        JustAdvancedFloor = false;
        CurrentRow = -1;
        CurrentColumn = -1;
        PendingNode = null;
        Current = null;
        PlayerHp = StartingHp;
        PlayerMaxHp = StartingHp;
        BonusDashSpeed = 0f;
        BonusContactDamage = 0f;
        BonusEnemySpawnInterval = 0f;
        PermanentKeyEffects.Clear();
        DiscoveredSpells.Clear();
        SpellSequences = SpellSystem.AssignSequences(Floors, KeyboardPreference.Layout);
        LogSpellSequences();
        GenerateFloorMap();
    }

    // Debug: liste tous les sorts de la run et leur sequence de touches, decouverts ou non.
    private static void LogSpellSequences()
    {
        if (SpellSequences.Count == 0)
            return;

        string list = string.Join("\n", SpellSequences.Select(kv => $"{kv.Key.spellName}: {string.Join("-", kv.Value)}"));
        Debug.Log($"[Sorts de la run] {SpellSequences.Count} sort(s):\n{list}");
    }

    // Noeuds accessibles depuis la position actuelle (toute la premiere rangee si on vient d'arriver sur l'etage).
    public static List<MapNode> GetAvailableNodes()
    {
        if (CurrentRow == -1)
            return Map[0];

        MapNode current = Map[CurrentRow].First(n => n.Column == CurrentColumn);
        return current.Connections
            .Select(col => Map[CurrentRow + 1].First(n => n.Column == col))
            .ToList();
    }

    // Combat: le niveau est prepare mais pas encore valide tant que la zone n'est pas nettoyee.
    // La profondeur utilisee pour la difficulte cumule les etages precedents: la difficulte continue
    // de grimper d'un etage a l'autre au lieu de repartir de zero.
    public static void SelectCombatNode(MapNode node)
    {
        PendingNode = node;
        int globalDepth = GetGlobalDepth(node.Row);
        Current = DifficultyScaling.Build(node.Type, globalDepth, DifficultyCurve, node.EliteEncounter, node.ChosenBoss);
    }

    public static DungeonFloorConfig CurrentFloorConfig =>
        Floors != null && CurrentFloorIndex >= 0 && CurrentFloorIndex < Floors.Length ? Floors[CurrentFloorIndex] : null;

    private static int GetGlobalDepth(int rowInCurrentFloor)
    {
        int depth = rowInCurrentFloor;
        for (int i = 0; i < CurrentFloorIndex; i++)
            depth += Floors[i].regularRowCount + 2; // +2: rangee Repos + Boss de chaque etage precedent
        return depth;
    }

    public static void CompletePendingNode()
    {
        if (PendingNode == null)
            return;

        PendingNode.Visited = true;
        CurrentRow = PendingNode.Row;
        CurrentColumn = PendingNode.Column;
        bool defeatedBoss = PendingNode.Type == MapNodeType.Boss;
        PendingNode = null;

        if (defeatedBoss)
            AdvanceFloorOrWin();
    }

    // Repos / Event: resolu directement sur la carte, pas de combat.
    public static void CompleteNodeImmediately(MapNode node)
    {
        node.Visited = true;
        CurrentRow = node.Row;
        CurrentColumn = node.Column;
    }

    private static void AdvanceFloorOrWin()
    {
        if (CurrentFloorIndex < Floors.Length - 1)
        {
            CurrentFloorIndex++;
            CurrentRow = -1;
            CurrentColumn = -1;
            GenerateFloorMap();
            JustAdvancedFloor = true;
        }
        else
        {
            HasWonRun = true;
        }
    }

    private static void GenerateFloorMap()
    {
        DungeonFloorConfig config = Floors[Mathf.Clamp(CurrentFloorIndex, 0, Floors.Length - 1)];
        Map = MapGenerator.Generate(config);
    }
}
