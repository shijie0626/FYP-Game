using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemID;
    public bool isHiddenItem = false;  // Always false for KeyItem

    [Header("UI Prompt")]
    public GameObject promptUI;

    private bool playerInRange = false;

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        // ✅ Destroy this item if already collected
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

        // ✅ If main item → set checkpoint
        if (GameController.Instance != null)
        {
            GameController.Instance.SetCheckpoint(transform.position, itemID);
        }

        if (promptUI != null)
            promptUI.SetActive(false);

        Destroy(gameObject);
    }
}
