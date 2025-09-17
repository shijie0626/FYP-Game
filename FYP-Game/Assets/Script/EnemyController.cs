using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class EnemyAIWithFade : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform player;
    public float moveSpeed = 3f;
    public float detectionRange = 5f;
    public float stopRange = 1f;

    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    public float waitTime = 0.5f;

    [Header("Fade Settings")]
    public SpriteRenderer spriteRenderer;
    public float fadeDuration = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.1f;

    [Header("Spawn Settings")]
    public Transform spawnPoint;

    [Header("Player Active Reference")]
    public PlayerToggleWithBeacon playerToggle;

    [Header("Ghost Kill Settings")]
    public VideoClip killVideoClip; // 🎥 assign different clip for each ghost

    private Rigidbody2D rb;
    private Animator animator;
    private bool chasingPlayer = false;
    private bool isGrounded;
    private Coroutine fadeRoutine;
    private float waitTimer = 0f;
    private int moveDirection = 1;

    private void Awake()
    {
        if (spawnPoint != null)
            transform.position = spawnPoint.position;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        SetAlpha(0f);
        moveDirection = 1;
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool isPlayerActive = playerToggle != null ? playerToggle.IsPlayerActive() : true;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        chasingPlayer = (isPlayerActive && distanceToPlayer <= detectionRange);

        if (isGrounded)
        {
            if (chasingPlayer)
                ChasePlayer(distanceToPlayer);
            else
                Patrol();
        }

        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }

    private void ChasePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > stopRange)
        {
            moveDirection = player.position.x > transform.position.x ? 1 : -1;
            Vector2 newPos = rb.position + new Vector2(moveDirection * moveSpeed * Time.deltaTime, 0);
            rb.MovePosition(newPos);

            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * moveDirection, transform.localScale.y, 1);
            rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        StartFade(1f);
    }

    private void Patrol()
    {
        Vector2 newPos = rb.position + new Vector2(moveDirection * moveSpeed * Time.deltaTime, 0);
        rb.MovePosition(newPos);

        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * moveDirection, transform.localScale.y, 1);

        if (pointA != null && Mathf.Abs(rb.position.x - pointA.position.x) < 0.1f)
            moveDirection = 1;
        else if (pointB != null && Mathf.Abs(rb.position.x - pointB.position.x) < 0.1f)
            moveDirection = -1;

        if (waitTimer > 0)
            waitTimer -= Time.deltaTime;

        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        StartFade(0f);
    }

    void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeToAlpha(targetAlpha));
    }

    IEnumerator FadeToAlpha(float targetAlpha)
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.KillPlayer(killVideoClip);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (pointA != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(pointA.position, 0.1f);
        }
        if (pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(pointB.position, 0.1f);
        }

        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnPoint.position, 0.15f);
        }
    }
}
