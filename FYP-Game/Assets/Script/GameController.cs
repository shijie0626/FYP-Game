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
    public LayerMask groundLayer;

    private bool isGrounded;
    private Animator myAnimator;

    [Header("Respawn Settings")]
    public Vector3 RespawnPoint;
    private string currentCheckpointItemID; // track which item set the checkpoint

    [Header("Audio")]
    public AudioSource DeadSound;

    [Header("Darkness Effect")]
    public Image darknessPanel;

    // Slowness state
    private bool isSlowed = false;
    private float baseMoveSpeed;

    // Collected main items
    private HashSet<string> collectedItems = new HashSet<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        myAnimator = GetComponent<Animator>();
        baseMoveSpeed = MoveSpeed;

        // Ensure darkness starts invisible
        if (darknessPanel != null)
        {
            Color c = darknessPanel.color;
            c.a = 0f;
            darknessPanel.color = c;
        }
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        // Handle movement
        Move();
    }

    void Move()
    {
        float moveDirection = Input.GetAxis("Horizontal");

        // Only animate if the player can actually move
        bool canMove = MoveSpeed > 0f;
        myAnimator.SetBool("walk", canMove && moveDirection != 0);

        // Apply movement
        myrigidbody.velocity = new Vector3(moveDirection * MoveSpeed, myrigidbody.velocity.y);

        // Flip sprite only if movement is allowed
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

    // ----------- CHECKPOINT SYSTEM -----------
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

    // ----------- EFFECTS -----------
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

    public System.Collections.IEnumerator DarknessEffect(float duration)
    {
        if (darknessPanel == null) yield break;

        float elapsed = 0f;

        // Fade In Quickly
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / 0.5f);
            SetDarknessAlpha(alpha);
            yield return null;
        }

        // Stay Dark + Pulse
        float darkTime = duration - 2f;
        float timer = 0f;
        while (timer < darkTime)
        {
            float alpha = 0.9f + Mathf.PingPong(Time.time * 0.5f, 0.1f);
            SetDarknessAlpha(alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        // Fade Out Slowly
        elapsed = 0f;
        while (elapsed < 1.5f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / 1.5f);
            SetDarknessAlpha(alpha);
            yield return null;
        }

        SetDarknessAlpha(0f);
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
            myrigidbody.velocity = new Vector3(0, 0, 0);
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

            // Fix momentum when leaving ladder
            myrigidbody.velocity = new Vector2(myrigidbody.velocity.x, 0f);
        }
    }
}
