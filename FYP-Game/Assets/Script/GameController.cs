using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Header("Movement Settings")]
    public float MoveSpeed = 5f;
    public Rigidbody2D myrigidbody;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded;
    private Animator myAnimator;

    [Header("Respawn Settings")]
    public Vector3 RespawnPoint;
    private string currentCheckpointItemID;

    [Header("Audio")]
    public AudioSource DeadSound;

    [Header("Darkness Effect")]
    public Image darknessPanel;

    private bool isSlowed = false;
    private float baseMoveSpeed;

    private HashSet<string> collectedItems = new HashSet<string>();

    private Coroutine darknessCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        myAnimator = GetComponent<Animator>();
        baseMoveSpeed = MoveSpeed;

        if (darknessPanel != null)
        {
            Color c = darknessPanel.color;
            c.a = 0f;
            darknessPanel.color = c;
            darknessPanel.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        Move();
    }

    void Move()
    {
        float moveDirection = Input.GetAxis("Horizontal");
        bool canMove = MoveSpeed > 0f;
        myAnimator.SetBool("walk", canMove && moveDirection != 0);
        myrigidbody.velocity = new Vector3(moveDirection * MoveSpeed, myrigidbody.velocity.y);

        if (canMove)
        {
            if (moveDirection > 0.01f)
                transform.localScale = Vector3.one;
            else if (moveDirection < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "DeadZone")
        {
            Respawn();
            FindObjectOfType<Health>().currentHealth -= 1;
            if (DeadSound != null) DeadSound.Play();
        }
    }

    public void SetCheckpoint(Vector3 position, string itemID)
    {
        RespawnPoint = position;
        currentCheckpointItemID = itemID;
        if (!collectedItems.Contains(itemID))
            collectedItems.Add(itemID);
    }

    public bool HasItemBeenCollected(string itemID)
    {
        return collectedItems.Contains(itemID);
    }

    private void Respawn()
    {
        transform.position = RespawnPoint;
    }

    public void ApplySlowness(float slowMultiplier, float duration)
    {
        if (!isSlowed)
            StartCoroutine(SlowEffect(slowMultiplier, duration));
    }

    private System.Collections.IEnumerator SlowEffect(float slowMultiplier, float duration)
    {
        isSlowed = true;
        MoveSpeed = baseMoveSpeed * slowMultiplier;

        yield return new WaitForSeconds(duration);

        MoveSpeed = baseMoveSpeed;
        isSlowed = false;
    }

    // ---------- Darkness Effect ----------
    public void StartDarknessEffect(float duration)
    {
        // Stop any existing effect
        StopDarknessEffect();

        darknessCoroutine = StartCoroutine(DarknessEffect(duration));
    }

    public void StopDarknessEffect()
    {
        if (darknessCoroutine != null)
        {
            StopCoroutine(darknessCoroutine);
            darknessCoroutine = null;
        }

        if (darknessPanel != null)
        {
            SetDarknessAlpha(0f);
            darknessPanel.gameObject.SetActive(false);
        }
    }

    public System.Collections.IEnumerator DarknessEffect(float duration)
    {
        if (darknessPanel == null) yield break;

        darknessPanel.gameObject.SetActive(true);

        try
        {
            float elapsed = 0f;

            // Fade In
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                SetDarknessAlpha(Mathf.Lerp(0f, 1f, elapsed / 0.5f));
                yield return null;
            }

            // Stay Dark + Pulse
            float darkTime = Mathf.Max(0f, duration - 1f);
            float timer = 0f;
            while (timer < darkTime)
            {
                SetDarknessAlpha(0.9f + Mathf.PingPong(Time.time * 0.1f, 0.1f));
                timer += Time.deltaTime;
                yield return null;
            }

            // Fade Out
            elapsed = 0f;
            while (elapsed < 1.5f)
            {
                elapsed += Time.deltaTime;
                SetDarknessAlpha(Mathf.Lerp(1f, 0f, elapsed / 1.5f));
                yield return null;
            }
        }
        finally
        {
            SetDarknessAlpha(0f);
            darknessPanel.gameObject.SetActive(false);
            darknessCoroutine = null;
        }
    }

    private void SetDarknessAlpha(float alpha)
    {
        if (darknessPanel == null) return;
        Color c = darknessPanel.color;
        c.a = alpha;
        darknessPanel.color = c;
    }

    // ----------- LADDER -----------
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Ladder")
        {
            myrigidbody.gravityScale = 0;
            myAnimator.SetBool("walk", false);
        }

        if (Input.GetAxisRaw("Vertical") > 0 && other.tag == "Ladder")
        {
            myrigidbody.velocity = new Vector3(0, MoveSpeed, 0);
            myAnimator.SetBool("isClimb", true);
            myAnimator.SetBool("down", false);
        }
        else if (Input.GetAxisRaw("Vertical") < 0 && other.tag == "Ladder")
        {
            myrigidbody.velocity = new Vector3(0, -MoveSpeed, 0);
            myAnimator.SetBool("isClimb", false);
            myAnimator.SetBool("down", true);
        }
        else if (other.tag == "Ladder")
        {
            myrigidbody.velocity = Vector3.zero;
        }

        if (Input.GetAxisRaw("Vertical") == 0 && other.tag == "Ladder")
        {
            myAnimator.SetBool("isClimb", false);
            myAnimator.SetBool("down", false);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Ladder")
        {
            myrigidbody.gravityScale = 1f;
            myAnimator.SetBool("isClimb", false);
            myAnimator.SetBool("down", false);
            myrigidbody.velocity = new Vector2(myrigidbody.velocity.x, 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
