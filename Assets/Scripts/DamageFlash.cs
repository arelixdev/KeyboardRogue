using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private DamageNumberFloat damageNumberPrefab;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private Vector3 numberOffset = new Vector3(0f, 0.6f, 0f);

    private Health health;
    private Color originalColor;
    private Coroutine flashRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        originalColor = targetRenderer.material.color;
        health.DamageTaken += OnDamageTaken;
    }

    private void OnDestroy()
    {
        health.DamageTaken -= OnDamageTaken;
    }

    private void OnDamageTaken(int amount)
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());

        if (damageNumberPrefab != null)
        {
            DamageNumberFloat number = Instantiate(damageNumberPrefab, transform.position + numberOffset, Quaternion.identity);
            number.SetAmount(amount);
        }
    }

    private IEnumerator FlashRoutine()
    {
        targetRenderer.material.color = flashColor;
        yield return new WaitForSecondsRealtime(flashDuration);
        targetRenderer.material.color = originalColor;
    }
}
