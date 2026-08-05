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
        health.SetCurrent(LevelSession.PlayerHp);
        health.DamageTaken += OnDamageTaken;
    }

    private void OnDamageTaken(int amount)
    {
        LevelSession.PlayerHp = health.Current;
    }

    private void OnDestroy()
    {
        health.DamageTaken -= OnDamageTaken;
    }
}
