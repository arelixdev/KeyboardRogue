using UnityEngine;

// Synchronise les PV du joueur avec la run en cours (LevelSession), pour qu'ils persistent entre les niveaux.
public class PlayerRunSync : MonoBehaviour
{
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void Start()
    {
        health.SetMax(LevelSession.PlayerMaxHp);
        health.SetCurrent(LevelSession.PlayerHp);
        // 'Damaged' se declenche sur tout changement de PV (degats ET soin), contrairement a
        // 'DamageTaken' qui ne sert qu'au feedback visuel des degats.
        health.Damaged += OnHpChanged;
    }

    private void OnHpChanged(int current)
    {
        LevelSession.PlayerHp = current;
    }

    private void OnDestroy()
    {
        health.Damaged -= OnHpChanged;
    }
}
