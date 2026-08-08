using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;

    public int Current { get; private set; }
    public int Max => maxHealth;

    // Montant brut inflige, pour le feedback visuel (flash, nombre flottant). Distinct de 'Damaged' qui donne les PV restants.
    public event Action<int> DamageTaken;
    public event Action<int> Damaged;
    public event Action Died;

    private void Awake()
    {
        Current = maxHealth;
    }

    public void SetCurrent(int value)
    {
        Current = Mathf.Clamp(value, 0, maxHealth);
    }

    public void SetMax(int value)
    {
        maxHealth = Mathf.Max(1, value);
        Current = Mathf.Min(Current, maxHealth);
    }

    public void Heal(int amount)
    {
        if (Current <= 0)
            return;

        Current = Mathf.Min(maxHealth, Current + amount);
        Damaged?.Invoke(Current);
    }

    public void TakeDamage(int amount)
    {
        if (Current <= 0)
            return;

        Current = Mathf.Max(0, Current - amount);
        Debug.Log($"{name} takes {amount} damage -> {Current}/{maxHealth} HP");
        DamageTaken?.Invoke(amount);
        Damaged?.Invoke(Current);

        if (Current == 0)
        {
            Debug.Log($"{name} died.");
            Died?.Invoke();
        }
    }

    // Force la mort et l'evenement Died, meme si Current est deja tombe silencieusement a 0 via
    // SetCurrent (ex: un total agrege par un autre script) : TakeDamage(Current) serait un no-op
    // dans ce cas puisque son garde-fou bloque tout appel une fois Current <= 0.
    public void Kill()
    {
        Current = 0;
        Died?.Invoke();
    }
}
