using TMPro;
using UnityEngine;

public class DamageNumberFloat : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.7f;
    [SerializeField] private float riseSpeed = 1.2f;

    private TMP_Text label;
    private Color startColor;
    private float timer;

    private void Awake()
    {
        label = GetComponentInChildren<TMP_Text>();
        startColor = label.color;
    }

    public void SetAmount(int amount)
    {
        label.text = "-" + amount;
    }

    private void Update()
    {
        // Temps non-scale: le nombre doit continuer a monter/disparaitre meme si le coup fatal met le jeu en pause (timeScale=0).
        transform.position += Vector3.up * riseSpeed * Time.unscaledDeltaTime;

        timer += Time.unscaledDeltaTime;
        float t = timer / lifetime;
        label.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, t));

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
