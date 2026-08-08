using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Premier boss du roster. Phase 1: deux poings independants martelent chacun une zone en croix
// du clavier (taille pilotee par BossDefinition.fistZoneRadius), restent vulnerables au sol,
// recommencent ailleurs. Quand l'un des deux meurt, l'autre s'enerve (Phase 2): enchaine 3 zones
// dangereuses d'affilee (le joueur doit esquiver, pas de fenetre de degats pendant le combo) avant
// de redevenir vulnerable, en boucle jusqu'a sa propre mort.
public class FistBossController : BossController
{
    [SerializeField] private float telegraphDuration = 1f;
    [SerializeField] private int fistHp = 10;
    [SerializeField] private float pauseBetweenAttacks = 0.5f;
    [SerializeField] private float comboHitDuration = 0.8f;
    [SerializeField] private float comboPauseBetweenHits = 0.4f;

    // Deux spheres (enfants du prefab) representant chaque poing: se deplacent au centre de la
    // zone qu'elles attaquent, pour que le joueur voie clairement d'ou vient la menace.
    [SerializeField] private Transform[] fistVisuals = new Transform[2];
    [SerializeField] private float fistHoverHeight = 1.2f;

    private readonly bool[] fistAlive = { true, true };
    // Sante persistante de chaque poing: continue d'encaisser les degats d'une attaque a l'autre
    // au lieu d'etre remise a fond a chaque nouvelle zone (portee par le visuel du poing lui-meme).
    private readonly Health[] fistHealths = new Health[2];
    private bool enraged;

    // Reglages fins pilotes par le BossDefinition de la run (LevelSession.Current.chosenBoss),
    // avec des valeurs de repli raisonnables si aucun n'est assigne (ex: tests isoles).
    private float vulnerableDuration = 4f;
    private int zoneRadius = 2;
    private float fistVisualScale = 0.85f;
    private float shockwaveGrowDuration = 1f;
    private float shockwaveMaxRadius = 1.4f;
    private int shockwaveDamage = 1;

    protected override void Awake()
    {
        base.Awake();
        LoadConfig();
        Health.SetMax(fistHp * 2);
        Health.SetCurrent(Health.Max);
        PositionAboveKeyboard();
        ApplyFistVisualScale();
        InitFistHealths();
    }

    private void LoadConfig()
    {
        BossDefinition config = LevelSession.Current?.chosenBoss;
        if (config == null)
            return;

        vulnerableDuration = config.fistVulnerableDuration;
        zoneRadius = config.fistZoneRadius;
        fistVisualScale = config.fistVisualScale;
        shockwaveGrowDuration = config.shockwaveGrowDuration;
        shockwaveMaxRadius = config.shockwaveMaxRadius;
        shockwaveDamage = config.shockwaveDamage;
    }

    private void ApplyFistVisualScale()
    {
        foreach (Transform fist in fistVisuals)
        {
            if (fist != null)
                fist.localScale = Vector3.one * fistVisualScale;
        }
    }

    private void InitFistHealths()
    {
        for (int i = 0; i < fistVisuals.Length && i < fistHealths.Length; i++)
        {
            if (fistVisuals[i] == null)
                continue;

            Health health = fistVisuals[i].gameObject.AddComponent<Health>();
            health.SetMax(fistHp);
            health.SetCurrent(fistHp);
            fistHealths[i] = health;
        }
    }

    // Le total affiche au HUD suit en temps reel la somme des PV restants des poings encore en vie.
    private void Update()
    {
        if (Health.Current <= 0)
            return;

        int total = 0;
        foreach (Health fistHealth in fistHealths)
        {
            if (fistHealth != null)
                total += fistHealth.Current;
        }

        Health.SetCurrent(total);
    }

    // Place le boss et ses deux poings au-dessus du centre du clavier, quelle que soit sa position
    // dans la scene.
    private void PositionAboveKeyboard()
    {
        if (keyboard.Keys.Count == 0)
            return;

        Vector3 sum = Vector3.zero;
        foreach (KeyView key in keyboard.Keys.Values)
            sum += key.transform.position;

        Vector3 center = sum / keyboard.Keys.Count;
        transform.position = center + Vector3.up * 3f;

        if (fistVisuals.Length > 0 && fistVisuals[0] != null)
            fistVisuals[0].position = center + Vector3.up * 3f + Vector3.left * 1.5f;
        if (fistVisuals.Length > 1 && fistVisuals[1] != null)
            fistVisuals[1].position = center + Vector3.up * 3f + Vector3.right * 1.5f;
    }

    private void Start()
    {
        Health.Died += () => Destroy(gameObject, 0.1f);
        StartCoroutine(FistLoop(0));
        StartCoroutine(FistLoop(1));
    }

    private IEnumerator FistLoop(int fistIndex)
    {
        while (fistAlive[fistIndex] && Health.Current > 0)
        {
            if (enraged)
                yield return EnrageCombo(fistIndex);
            else
                yield return NormalSlam(fistIndex);

            yield return new WaitForSeconds(pauseBetweenAttacks);
        }
    }

    private IEnumerator NormalSlam(int fistIndex)
    {
        List<KeyView> zone = PickZoneAround(PickRandomCenter());
        MoveFistTo(fistIndex, zone[0]);

        BossAttackZone attack = SpawnZone(zone, fistIndex);
        bool destroyed = false;
        attack.DestroyedByPlayer += () => destroyed = true;
        yield return StartCoroutine(attack.RunVulnerable(telegraphDuration, vulnerableDuration, fistHealths[fistIndex], shockwaveGrowDuration, shockwaveMaxRadius, shockwaveDamage));

        if (destroyed)
            OnFistDefeated(fistIndex);
    }

    // Combo de 3 frappes au total: les 2 premieres dangereuses (Hazard), la 3eme s'arrete en
    // fenetre vulnerable pour laisser le joueur riposter (au lieu d'une 4eme frappe separee).
    private IEnumerator EnrageCombo(int fistIndex)
    {
        for (int i = 0; i < 2; i++)
        {
            List<KeyView> zone = PickZoneAround(PickRandomCenter());
            MoveFistTo(fistIndex, zone[0]);

            BossAttackZone hit = SpawnZone(zone, fistIndex);
            yield return StartCoroutine(hit.RunHazard(telegraphDuration, comboHitDuration, shockwaveGrowDuration, shockwaveMaxRadius, shockwaveDamage));
            yield return new WaitForSeconds(comboPauseBetweenHits);
        }

        List<KeyView> restZone = PickZoneAround(PickRandomCenter());
        MoveFistTo(fistIndex, restZone[0]);

        BossAttackZone rest = SpawnZone(restZone, fistIndex);
        bool destroyed = false;
        rest.DestroyedByPlayer += () => destroyed = true;
        yield return StartCoroutine(rest.RunVulnerable(telegraphDuration, vulnerableDuration, fistHealths[fistIndex], shockwaveGrowDuration, shockwaveMaxRadius, shockwaveDamage));

        if (destroyed)
            OnFistDefeated(fistIndex);
    }

    private void MoveFistTo(int fistIndex, KeyView key)
    {
        if (fistIndex >= fistVisuals.Length || fistVisuals[fistIndex] == null)
            return;

        fistVisuals[fistIndex].position = key.transform.position + Vector3.up * fistHoverHeight;
    }

    private void OnFistDefeated(int fistIndex)
    {
        fistAlive[fistIndex] = false;

        if (fistIndex < fistVisuals.Length && fistVisuals[fistIndex] != null)
            fistVisuals[fistIndex].gameObject.SetActive(false);

        bool anyAlive = fistAlive.Any(alive => alive);

        // Le total (Update()) a deja pu ramener Health.Current a 0 silencieusement avant qu'on
        // arrive ici (SetCurrent ne declenche pas Died) : Kill() garantit que Died se declenche
        // quand meme, au lieu de TakeDamage(0) qui serait un no-op dans ce cas.
        if (!anyAlive)
        {
            Health.Kill();
        }
        else
        {
            // Le poing survivant repart a fond en passant en Phase 2: l'enrage est une seconde
            // vie, pas juste la suite de la premiere.
            int survivorIndex = fistIndex == 0 ? 1 : 0;
            if (fistHealths[survivorIndex] != null)
                fistHealths[survivorIndex].SetCurrent(fistHealths[survivorIndex].Max);

            enraged = true;
        }
    }

    private BossAttackZone SpawnZone(List<KeyView> keys, int fistIndex)
    {
        GameObject go = new GameObject("BossAttackZone");
        BossAttackZone zone = go.AddComponent<BossAttackZone>();
        zone.Setup(keys, player, player.GetComponent<Health>(), fistVisuals[fistIndex]);
        return zone;
    }

    private KeyView PickRandomCenter()
    {
        List<KeyView> allKeys = keyboard.Keys.Values.ToList();
        return allKeys[Random.Range(0, allKeys.Count)];
    }

    // Zone en croix: la touche centrale (toujours zone[0]) + jusqu'a 'zoneRadius' touches dans
    // chacune des 4 directions (haut/bas/gauche/droite). Deterministe: pas de tirage aleatoire sur
    // quelles touches voisines sont incluses, contrairement a l'ancienne version.
    private List<KeyView> PickZoneAround(KeyView center)
    {
        List<KeyView> zone = new List<KeyView> { center };
        AddDirection(zone, center, -1, 0);
        AddDirection(zone, center, 1, 0);
        AddDirection(zone, center, 0, -1);
        AddDirection(zone, center, 0, 1);
        return zone;
    }

    private void AddDirection(List<KeyView> zone, KeyView center, int dRow, int dCol)
    {
        for (int step = 1; step <= zoneRadius; step++)
        {
            KeyView key = FindKeyAt(center.Row + dRow * step, center.Col + dCol * step);
            if (key == null)
                break; // hors clavier dans cette direction: inutile de continuer plus loin

            zone.Add(key);
        }
    }

    private KeyView FindKeyAt(int row, int col)
    {
        foreach (KeyView key in keyboard.Keys.Values)
        {
            if (key.Row == row && key.Col == col)
                return key;
        }

        return null;
    }
}
