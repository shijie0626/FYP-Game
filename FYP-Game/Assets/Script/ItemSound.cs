using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(Collider2D))]
public class PickupOnKeySound : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip pickupClip;       // 捡东西音效
    [Range(0f, 1f)]
    public float volume = 1f;          // Inspector 可调音量

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E; // 按键拾取
    public string playerTag = "Player";     // 玩家 Tag

    [Header("UI (Optional)")]
    public GameObject interactionPrompt;    // “按 E 交互”提示

    private AudioSource audioSource;
    private bool playerInRange = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        // 玩家在范围内按下交互键
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            PlayPickupSound();
        }
    }

    private void PlayPickupSound()
    {
        if (pickupClip != null)
            audioSource.PlayOneShot(pickupClip, volume);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }
}
