using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBasic : MonoBehaviour
{
    // Permet a un spawner de suivre combien d'ennemis sont encore en vie, quelle que soit la cause de la mort.
    public event Action<EnemyBasic> Removed;

    // Registre partage entre tous les ennemis vivants, pour eviter que deux ennemis se superposent sur la meme case.
    private static readonly List<EnemyBasic> ActiveEnemies = new List<EnemyBasic>();

    [SerializeField] private KeyboardGenerator keyboard;
    [SerializeField] private PlayerKeyboardMover player;
    [SerializeField] private float spawnDelay = 3f;
    [SerializeField] private float moveInterval = 0.5f;
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float heightOffset = 0.53f;
    [SerializeField] private int minSpawnDistance = 3;
    [SerializeField] private float contactRadius = 0.5f;

    private Renderer[] renderers;
    private Health health;
    private Health playerHealth;
    private KeyView currentKey;
    private Vector3 targetPosition;
    private float moveTimer;
    private bool isActive;
    private bool isTouchingPlayer;

    private void Awake()
    {
        if (keyboard == null)
            keyboard = GameManager.Instance.Keyboard;
        if (player == null)
            player = GameManager.Instance.Player;

        health = GetComponent<Health>();
        playerHealth = player.GetComponent<Health>();

        renderers = GetComponentsInChildren<Renderer>();
        SetVisible(false);

        ActiveEnemies.Add(this);
    }

    private void Start()
    {
        Invoke(nameof(Spawn), spawnDelay);
    }

    private void Update()
    {
        if (!isActive)
            return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        CheckPlayerContact();
        if (!isActive)
            return;

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer -= moveInterval;
            MoveToRandomAdjacentKey();
        }
    }

    private void Spawn()
    {
        currentKey = PickSpawnKey();
        targetPosition = GetStandPosition(currentKey);
        transform.position = targetPosition;

        SetVisible(true);
        isActive = true;
    }

    private KeyView PickSpawnKey()
    {
        List<KeyView> candidates = keyboard.Keys.Values
            .Where(key => GridDistance(key, player.CurrentKey) >= minSpawnDistance)
            .ToList();

        if (candidates.Count == 0)
            candidates = keyboard.Keys.Values.ToList();

        return candidates[Random.Range(0, candidates.Count)];
    }

    private void MoveToRandomAdjacentKey()
    {
        List<KeyView> adjacent = keyboard.Keys.Values
            .Where(key => IsAdjacent(key, currentKey))
            .ToList();

        if (adjacent.Count == 0)
            return;

        KeyView candidate = adjacent[Random.Range(0, adjacent.Count)];

        // Une autre ennemi occupe deja cette case: on retarde le deplacement, on retentera au prochain tick.
        if (IsOccupiedByOtherEnemy(candidate))
            return;

        currentKey = candidate;
        targetPosition = GetStandPosition(currentKey);
    }

    private bool IsOccupiedByOtherEnemy(KeyView key)
    {
        foreach (EnemyBasic other in ActiveEnemies)
        {
            if (other != this && other.currentKey == key)
                return true;
        }

        return false;
    }

    private void CheckPlayerContact()
    {
        bool touching = IsTouchingPlayer();

        if (touching && !isTouchingPlayer)
            OnPlayerContact();

        isTouchingPlayer = touching;
    }

    // Contact physique reel (jamais base sur la destination visee, seulement la position actuelle).
    // Le joueur est en dash: il traverse l'ennemi, qui encaisse des degats.
    // Le joueur est arrete: l'ennemi vient de le rattraper, c'est lui qui inflige les degats.
    private void OnPlayerContact()
    {
        if (player.IsMoving)
        {
            int damage = 1 + Mathf.RoundToInt(LevelSession.BonusContactDamage);
            health.TakeDamage(damage);

            if (health.Current <= 0)
            {
                isActive = false;
                Destroy(gameObject);
            }
        }
        else
        {
            isActive = false;
            playerHealth.TakeDamage(1);
            Destroy(gameObject);
        }
    }

    private bool IsTouchingPlayer()
    {
        Vector3 a = transform.position;
        Vector3 b = player.transform.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b) <= contactRadius;
    }

    private static bool IsAdjacent(KeyView a, KeyView b)
    {
        int dRow = Mathf.Abs(a.Row - b.Row);
        int dCol = Mathf.Abs(a.Col - b.Col);
        return dRow + dCol == 1;
    }

    private static int GridDistance(KeyView a, KeyView b)
    {
        return Mathf.Abs(a.Row - b.Row) + Mathf.Abs(a.Col - b.Col);
    }

    private Vector3 GetStandPosition(KeyView key)
    {
        return key.transform.position + Vector3.up * heightOffset;
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers)
            r.enabled = visible;
    }

    private void OnDestroy()
    {
        ActiveEnemies.Remove(this);
        Removed?.Invoke(this);
    }
}
