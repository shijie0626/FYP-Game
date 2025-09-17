using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    private Transform targetPoint;

    [Header("Disappear Settings")]
    public float disappearTime = 3f;
    private bool isDisappeared = false;
    private SpriteRenderer spriteRenderer;
    private Coroutine patrolRoutine;

    void Start()
    {
        targetPoint = pointB;
        spriteRenderer = GetComponent<SpriteRenderer>();
        patrolRoutine = StartCoroutine(Patrol());
    }

    IEnumerator Patrol()
    {
        while (true)
        {
            if (!isDisappeared)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPoint.position,
                    speed * Time.deltaTime
                );

                if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
                {
                    targetPoint = (targetPoint == pointA) ? pointB : pointA;
                }
            }
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ Ignore fake presence beacon
        if (other.GetComponent<PresenceBeacon>() != null) return;

        if (!other.CompareTag("Player")) return;

        // ✅ Check if player is hidden
        PlayerToggleWithBeacon toggle = other.GetComponentInParent<PlayerToggleWithBeacon>();
        if (toggle != null && !toggle.IsPlayerActive())
        {
            return; // ignore hidden player
        }

        // ✅ Apply effects on real player
        GameController player = other.GetComponent<GameController>();
        if (player != null)
        {
            // Apply slowness (50% speed for 2s)
            player.ApplySlowness(0.5f, 2f);

            // Start darkness effect (5s total)
            player.StartCoroutine(player.DarknessEffect(5f));
        }

        // Enemy vanish & reappear
        StartCoroutine(DisappearAndReappear());
    }

    IEnumerator DisappearAndReappear()
    {
        isDisappeared = true;

        // fade out
        yield return StartCoroutine(FadeSprite(1f, 0f, 0.5f));
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(disappearTime);

        // fade in
        spriteRenderer.enabled = true;
        yield return StartCoroutine(FadeSprite(0f, 1f, 0.5f));

        isDisappeared = false;
    }

    IEnumerator FadeSprite(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color c = spriteRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            c.a = Mathf.Lerp(startAlpha, endAlpha, t);
            spriteRenderer.color = c;
            yield return null;
        }

        c.a = endAlpha;
        spriteRenderer.color = c;
    }
}
