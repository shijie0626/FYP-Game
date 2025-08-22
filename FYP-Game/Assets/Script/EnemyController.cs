using UnityEngine;

public class EnemyAIWithFade : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform player;
    public float moveSpeed = 3f;
    public float detectionRange = 5f;
    public float stopRange = 1f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float waitTime = 2f;

    [Header("Fade Settings")]
    public SpriteRenderer spriteRenderer;
    public float fadeDuration = 0.5f;

    private int currentPoint = 0;
    private float waitTimer;
    private Rigidbody2D rb;
    private bool chasingPlayer = false;
    private Coroutine fadeRoutine;
    private bool playerInSameRoom = false;
    public Transform groundCheck;  // 地面检查的位置
    public LayerMask groundLayer;  // 定义什么是地面

    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        waitTimer = waitTime;
        SetAlpha(0f); // Start invisible
    }

    private void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (playerInSameRoom && distanceToPlayer <= detectionRange)
            chasingPlayer = true;
        else if (distanceToPlayer > detectionRange * 1.2f)
            chasingPlayer = false;

        if (!playerInSameRoom)
        {
            // Patrol even when invisible
            Patrol();
        }
        else
        {
            if (chasingPlayer)
                ChasePlayer(distanceToPlayer);
            else
                Patrol();
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    private void ChasePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > stopRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPoint];
        Vector2 direction = (targetPoint.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            if (waitTimer <= 0f)
            {
                currentPoint = (currentPoint + 1) % patrolPoints.Length;
                waitTimer = waitTime;
            }
            else
            {
                waitTimer -= Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Room"))
        {
            playerInSameRoom = true;
            StartFade(1f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Room"))
        {
            playerInSameRoom = false;
            StartFade(0f);
        }
    }

    void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeToAlpha(targetAlpha));
    }

    System.Collections.IEnumerator FadeToAlpha(float targetAlpha)
    {
        float startAlpha = spriteRenderer.color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    void SetAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
