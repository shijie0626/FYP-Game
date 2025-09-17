using UnityEngine;

public class HiddenItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemID; // e.g. "newspaper", "letter"

    [Header("UI Image")]
    public GameObject imagePanel; // Panel with letter/newspaper image

    private bool playerInRange = false;
    private bool isReading = false; // ✅ track if currently reading

    void Start()
    {
        if (imagePanel != null)
            imagePanel.SetActive(false); // Hide at start
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isReading)
                ShowImage();
            else
                HideImage();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;

        // ❌ Do NOT auto-close while reading
    }

    void ShowImage()
    {
        if (imagePanel == null) return;

        imagePanel.SetActive(true);
        isReading = true;
        Time.timeScale = 0f; // freeze game

        Debug.Log("Player is reading: " + itemID);

        // Save as collected hidden item
        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv != null)
            inv.AddItem(itemID, true);
    }

    void HideImage()
    {
        if (imagePanel == null) return;

        imagePanel.SetActive(false);
        isReading = false;
        Time.timeScale = 1f; // unfreeze game
    }
}
