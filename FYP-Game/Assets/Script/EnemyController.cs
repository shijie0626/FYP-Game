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
    public PlayerToggleWithBeacon playerToggle; // 检测玩家是否处于躲藏状态

    [Header("Ghost Kill Settings")]
    public VideoClip killVideoClip; // 🎥 assign different clip for each ghost

    private Rigidbody2D rb;
    private Animator animator;
    private bool chasingPlayer = false;
    private bool isGrounded;
    private Coroutine fadeRoutine;
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

        // 判断玩家是否处于躲藏状态
        bool isPlayerActive = playerToggle != null ? playerToggle.IsPlayerActive() : true;

        if (!isPlayerActive)
        {
            // 玩家在躲藏状态 -> 敌人直接巡逻，不理会玩家
            Patrol();
            if (animator != null)
                animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            return;
        }

        // 玩家不在躲藏 -> 敌人可以追踪
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        chasingPlayer = (distanceToPlayer <= detectionRange);

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

            // Flip sprite
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * moveDirection, transform.localScale.y, 1);

            // Move with velocity only
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
        // Flip sprite
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * moveDirection, transform.localScale.y, 1);

        // 🔹 Clamp patrol within [pointA.x, pointB.x]
        if (pointA != null && pointB != null)
        {
            if (rb.position.x <= Mathf.Min(pointA.position.x, pointB.position.x))
                moveDirection = 1;
            else if (rb.position.x >= Mathf.Max(pointA.position.x, pointB.position.x))
                moveDirection = -1;
        }

        // Move with velocity only
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        // fade out when patrolling
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
            // 只有在玩家不处于躲藏状态时，才能触发死亡
            if (playerToggle != null && playerToggle.IsPlayerActive())
            {
                if (GameOverManager.Instance != null)
                {
                    // Freeze time immediately when ghost catches player
                    Time.timeScale = 0f;
                    GameOverManager.Instance.KillPlayer(killVideoClip);
                }
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
