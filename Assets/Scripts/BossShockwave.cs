using System.Collections;
using UnityEngine;

// Onde de choc au sol: apparait au point de chute d'un poing et grandit jusqu'a un rayon max sur
// une duree donnee. Contrairement au check instantane par touche (qui ne voit que les cases
// teintees), elle couvre un vrai rayon physique et continue de menacer le joueur pendant sa
// croissance, pas seulement a l'instant de l'impact.
public class BossShockwave : MonoBehaviour
{
    private static readonly Color ShockwaveColor = new Color(1f, 0.35f, 0.1f, 0.5f);

    private Transform player;
    private Health playerHealth;
    private float maxRadius;
    private float growDuration;
    private int damage;
    private bool hasHitPlayer;

    public void Setup(Vector3 originWorldPos, Transform targetPlayer, Health targetPlayerHealth, float radius, float duration, int dmg)
    {
        transform.position = originWorldPos;
        player = targetPlayer;
        playerHealth = targetPlayerHealth;
        maxRadius = radius;
        growDuration = Mathf.Max(0.01f, duration);
        damage = dmg;

        BuildVisual();
        StartCoroutine(Grow());
    }

    private void BuildVisual()
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "ShockwaveVisual";
        visual.transform.SetParent(transform, false);
        visual.transform.localScale = Vector3.zero;

        Collider col = visual.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        Renderer renderer = visual.GetComponent<Renderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        renderer.material = new Material(shader) { color = ShockwaveColor };
    }

    private IEnumerator Grow()
    {
        Transform visual = transform.GetChild(0);
        float timer = 0f;

        while (timer < growDuration)
        {
            float radius = Mathf.Lerp(0f, maxRadius, timer / growDuration);
            visual.localScale = Vector3.one * radius * 2f;
            CheckHit(radius);

            timer += Time.deltaTime;
            yield return null;
        }

        visual.localScale = Vector3.one * maxRadius * 2f;
        CheckHit(maxRadius);

        Destroy(gameObject);
    }

    private void CheckHit(float radius)
    {
        if (hasHitPlayer || player == null)
            return;

        Vector3 a = transform.position;
        Vector3 b = player.position;
        a.y = 0f;
        b.y = 0f;

        if (Vector3.Distance(a, b) <= radius)
        {
            hasHitPlayer = true;
            playerHealth.TakeDamage(damage);
        }
    }
}
