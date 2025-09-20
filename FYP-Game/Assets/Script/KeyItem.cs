using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class KeyItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemID;
    public bool isHiddenItem = false;  // Always false for KeyItem

    [Header("UI Prompt")]
    public GameObject promptUI;

    [Header("Audio")]
    public AudioClip pickupClip;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Visuals")]
    public bool hideOnPickup = true; // ✅ 是否在拾取时立即隐藏物品

    private bool playerInRange = false;
    private AudioSource audioSource;

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // ✅ 如果物品已经被收集过 → 直接销毁
        if (GameController.Instance != null && GameController.Instance.HasItemBeenCollected(itemID))
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            CollectItem();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        if (promptUI != null)
            promptUI.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void CollectItem()
    {
        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv != null)
        {
            inv.AddItem(itemID, false);
        }

        // ✅ 如果是关键物品 → 设置存档点
        if (GameController.Instance != null)
        {
            GameController.Instance.SetCheckpoint(transform.position, itemID);
        }

        if (promptUI != null)
            promptUI.SetActive(false);

        // 🔊 播放音效
        if (pickupClip != null)
        {
            audioSource.PlayOneShot(pickupClip, volume);
        }

        // ✅ 隐藏视觉效果（Renderer + Collider）
        if (hideOnPickup)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
            foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
            {
                c.enabled = false;
            }
        }

        // 延迟销毁物品
        if (pickupClip != null)
        {
            StartCoroutine(DestroyAfterSound());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyAfterSound()
    {
        yield return new WaitForSeconds(pickupClip.length);
        Destroy(gameObject);
    }
}
