using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;

    public int Current { get; private set; }
    public int Max => maxHealth;

    public event Action<int> Damaged;
    public event Action Died;

    private void Awake()
    {
        Current = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (Current <= 0)
            return;

        Current = Mathf.Max(0, Current - amount);
        Debug.Log($"{name} takes {amount} damage -> {Current}/{maxHealth} HP");
        Damaged?.Invoke(Current);

        if (Current == 0)
        {
            Debug.Log($"{name} died.");
            Died?.Invoke();
        }
    }
}
