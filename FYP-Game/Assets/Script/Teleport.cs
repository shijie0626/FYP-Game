using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform targetPoint;           // 传送目标位置
    public KeyCode teleportKey = KeyCode.E; // 传送按键

    [Header("UI Prompt")]
    public GameObject promptUI;             // 提示 UI (可选)

    private bool playerInRange = false;
    private Transform player;

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void Update()
    {
        // 玩家在范围内 并且 按下E 才能传送
        if (playerInRange && Input.GetKeyDown(teleportKey))
        {
            TeleportPlayer();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.transform;

            if (promptUI != null)
                promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }

    void TeleportPlayer()
    {
        if (targetPoint != null && player != null)
        {
            player.position = targetPoint.position;
        }

        if (promptUI != null)
            promptUI.SetActive(false);
    }
}
