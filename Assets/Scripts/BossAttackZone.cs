using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackZoneMode
{
    Hazard,
    Vulnerable,
}

// Primitive reutilisable pour les attaques de boss ciblant des touches: telegraphe -> phase active
// (dangereuse ou vulnerable) -> resolution. Concue pour etre reutilisee par n'importe quel futur
// boss, pas seulement le premier.
//
// Le highlight des touches et les degats infliges AU JOUEUR par simple contact ont ete retires
// temporairement (un nouveau visuel/systeme de degats sera fait plus tard) : seule l'onde de choc
// au sol (BossShockwave, mode Vulnerable) inflige encore des degats au joueur. En revanche, pendant
// la fenetre Vulnerable, le joueur peut infliger des degats au poing, mais seulement en dashant a
// travers lui (pas en s'approchant/restant a cote) : meme logique que "dasher a travers un ennemi".
public class BossAttackZone : MonoBehaviour
{
    private List<KeyView> targetKeys;
    private AttackZoneMode mode;
    private PlayerKeyboardMover player;
    private Health playerHealth;
    private Health targetHealth; // uniquement en mode Vulnerable: fournie par l'appelant, PAS creee ici (persiste entre les attaques).
    private Transform fistTransform; // position reelle du poing (pas la case): c'est ce qu'il faut dasher a travers.
    private bool wasHitting;
    private bool destroyedEarly;

    public event Action Completed; // fin normale (duree ecoulee)
    public event Action DestroyedByPlayer; // mode Vulnerable uniquement: la cible est tombee a 0 PV avant la fin

    public void Setup(List<KeyView> keys, PlayerKeyboardMover targetPlayer, Health targetPlayerHealth, Transform targetFistTransform)
    {
        targetKeys = keys;
        player = targetPlayer;
        playerHealth = targetPlayerHealth;
        fistTransform = targetFistTransform;
    }

    public IEnumerator RunHazard(float telegraphDuration, float activeDuration)
    {
        mode = AttackZoneMode.Hazard;
        yield return new WaitForSeconds(telegraphDuration);
        if (this == null)
            yield break;

        yield return RunActivePhase(activeDuration);

        Completed?.Invoke();
        Destroy(gameObject);
    }

    // 'health' est la sante persistante du poing (vit sur le boss, pas sur cette zone): elle
    // continue d'encaisser les degats d'une attaque a l'autre au lieu d'etre remise a fond a chaque fois.
    // Les parametres shockwave* pilotent l'onde de choc au sol declenchee au moment de l'impact.
    public IEnumerator RunVulnerable(float telegraphDuration, float activeDuration, Health health, float shockwaveGrowDuration, float shockwaveMaxRadius, int shockwaveDamage)
    {
        mode = AttackZoneMode.Vulnerable;
        targetHealth = health;
        yield return new WaitForSeconds(telegraphDuration);
        if (this == null)
            yield break;

        SpawnShockwave(shockwaveGrowDuration, shockwaveMaxRadius, shockwaveDamage);

        yield return RunActivePhase(activeDuration);

        if (destroyedEarly)
            DestroyedByPlayer?.Invoke();
        else
            Completed?.Invoke();

        Destroy(gameObject);
    }

    private IEnumerator RunActivePhase(float activeDuration)
    {
        float timer = 0f;
        while (timer < activeDuration)
        {
            if (mode == AttackZoneMode.Vulnerable)
            {
                if (targetHealth.Current <= 0)
                {
                    destroyedEarly = true;
                    yield break;
                }

                CheckDashThroughFist();
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    // Fenetre de riposte: le joueur inflige des degats au poing uniquement s'il dashe A TRAVERS lui
    // (en mouvement + proche), pas en s'approchant/restant a cote pendant qu'il est immobile.
    private void CheckDashThroughFist()
    {
        bool hitting = player.IsMoving && IsPlayerNearFist();
        if (hitting && !wasHitting)
            targetHealth.TakeDamage(1);

        wasHitting = hitting;
    }

    private bool IsPlayerNearFist()
    {
        Vector3 a = fistTransform.position;
        Vector3 b = player.transform.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b) <= 0.8f;
    }

    // L'onde nait au centre de la zone (targetKeys[0], toujours la case d'impact) et grandit en
    // temps reel pendant que le joueur peut encore etre en train d'y entrer.
    private void SpawnShockwave(float growDuration, float maxRadius, int damage)
    {
        Vector3 origin = targetKeys[0].transform.position;
        GameObject go = new GameObject("BossShockwave");
        BossShockwave shockwave = go.AddComponent<BossShockwave>();
        shockwave.Setup(origin, player.transform, playerHealth, maxRadius, growDuration, damage);
    }
}
