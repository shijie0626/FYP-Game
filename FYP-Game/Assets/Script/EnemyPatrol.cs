using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure Rigidbody2D is kinematic to allow Transform movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.isKinematic = true;
    }

    void Start()
    {
        // Safety check: assign patrol points
        if (pointA == null || pointB == null)
        {
            Debug.LogError(name + ": Patrol points not assigned!");
            enabled = false;
            return;
        }

        // Start at pointA
        transform.position = pointA.position;
        targetPoint = pointB;

        StartCoroutine(Patrol());
    }

    IEnumerator Patrol()
    {
        while (true)
        {
            if (!isDisappeared)
            {
                // Move toward target point
                Vector2 newPos = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);
                Vector2 direction = (targetPoint.position - transform.position).normalized;
                transform.position = newPos;

                // Flip sprite based on horizontal movement
                if (direction.x > 0.01f)
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                else if (direction.x < -0.01f)
                    transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

                // Switch patrol target based on horizontal distance
                if (Mathf.Abs(transform.position.x - targetPoint.position.x) < 0.05f)
                {
                    targetPoint = (targetPoint == pointA) ? pointB : pointA;
                }
            }

            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PresenceBeacon>() != null) return;
        if (!other.CompareTag("Player")) return;

        PlayerToggleWithBeacon toggle = other.GetComponentInParent<PlayerToggleWithBeacon>();
        if (toggle != null && !toggle.IsPlayerActive()) return;

        GameController player = other.GetComponent<GameController>();
        if (player != null)
        {
            player.ApplySlowness(0.5f, 2f);
            player.StartCoroutine(player.DarknessEffect(5f));
        }

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

    // -----------------------------
    // Gizmos for patrol points
    // -----------------------------
    private void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            // Draw spheres at patrol points
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pointA.position, 0.1f);
            Gizmos.DrawSphere(pointB.position, 0.1f);

            // Draw line connecting points
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);

#if UNITY_EDITOR
            // Draw labels above points
            Handles.Label(pointA.position + Vector3.up * 0.2f, "Point A");
            Handles.Label(pointB.position + Vector3.up * 0.2f, "Point B");
#endif
        }
    }
}
