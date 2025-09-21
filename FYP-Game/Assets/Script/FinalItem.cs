using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinalItem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interactionPrompt; // "Press E to interact"
    public GameObject inputPanel;        // Panel containing input field
    public InputField inputField;        // Input field for typing
    public Text feedbackText;            // Feedback text

    [Header("Settings")]
    public string secretWord = "Robinson Edward";
    public string happyEndingScene = "HappyEnding";

    private bool playerInRange = false;
    private GameController playerController;  // Reference to player
    private float originalMoveSpeed;          // Store original speed

    private NPCDialogue npcDialogue;

    void Start()
    {
        interactionPrompt.SetActive(false);
        inputPanel.SetActive(false);
        feedbackText.text = "";
        feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, 0);

        npcDialogue = FindObjectOfType<NPCDialogue>();
    }

    void Update()
    {
        // Open panel with E only if player is in range and not typing
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !inputField.isFocused)
        {
            OpenInputPanel();
        }

        // Submit input if Enter is pressed
        if (inputPanel.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            CheckInput();
        }
    }

    private void OpenInputPanel()
    {
        inputPanel.SetActive(true);
        inputField.text = "";
        inputField.ActivateInputField();
        feedbackText.text = "";
        feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, 0);

        // Freeze player movement
        if (playerController != null)
        {
            originalMoveSpeed = playerController.MoveSpeed;
            playerController.MoveSpeed = 0f;
        }

        // Show timer if NPCDialogue exists
        if (npcDialogue != null && npcDialogue.timerText != null)
            npcDialogue.timerText.gameObject.SetActive(true);
    }

    private void CloseInputPanel()
    {
        inputPanel.SetActive(false);
        feedbackText.text = "";

        // Restore movement
        if (playerController != null)
            playerController.MoveSpeed = originalMoveSpeed;
    }

    public void CheckInput()
    {
        if (inputField.text.Trim().ToLower() == secretWord.ToLower())
        {
            CloseInputPanel();

            // Stop NPCDialogue timer
            if (npcDialogue != null)
                npcDialogue.CompleteGoodEnding();

            // Load happy ending
            SceneManager.LoadScene(happyEndingScene);
        }
        else
        {
            StartCoroutine(ShowFeedback("Nothing happens...", 1f));
        }
    }

    private IEnumerator ShowFeedback(string message, float duration)
    {
        feedbackText.text = message;

        // Fade in
        for (float t = 0; t <= 1f; t += Time.deltaTime * 2f)
        {
            feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, t);
            yield return null;
        }

        yield return new WaitForSeconds(duration);

        // Fade out
        for (float t = 1f; t >= 0f; t -= Time.deltaTime * 2f)
        {
            feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, t);
            yield return null;
        }

        inputField.text = "";
        inputField.ActivateInputField();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            interactionPrompt.SetActive(true);
            playerController = other.GetComponent<GameController>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            CloseInputPanel();
            interactionPrompt.SetActive(false);
        }
    }
}
