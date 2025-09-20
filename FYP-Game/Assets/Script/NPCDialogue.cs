using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NPCDialogue : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueUI;
    public Text dialogueText;
    public Image leftPortrait;    // NPC
    public Image rightPortrait;   // Player
    public GameObject promptUI;
    public Image backgroundImage; // Black background (semi-transparent)

    [Header("Default Dialogue Data")]
    [TextArea(2, 5)]
    public string[] normalDialogueLines;
    public bool[] normalIsPlayerSpeaking;

    [Header("Bad Ending Dialogue Data")]
    [TextArea(2, 5)]
    public string[] badEndingDialogueLines;
    public bool[] badEndingIsPlayerSpeaking;

    [Header("Good Ending Dialogue Data")]
    [TextArea(2, 5)]
    public string[] goodEndingDialogueLines;
    public bool[] goodEndingIsPlayerSpeaking;

    [Header("Portraits")]
    public Sprite npcSprite;
    public Sprite playerSprite;

    [Header("Transition Settings")]
    public float transitionDuration = 0.3f;
    public float activeScale = 1.1f;
    public float inactiveScale = 0.9f;
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f);

    [Header("Fade Settings")]
    public float fadeOutDuration = 0.5f;
    public float promptFadeDuration = 0.3f;

    [Header("Quest Settings")]
    public Collider2D[] doorColliders;  // doors to unlock
    public bool unlockAfterDialogue = false;
    public GameObject[] enemiesToActivate;

    [Header("Ending Settings")]
    public List<string> badEndingItems = new List<string> { "Hand", "Doll", "Ring" };
    public List<string> goodEndingHiddenItems = new List<string> { "Newspaper", "Letter" };
    public string badEndingSceneName = "BadEndingScene";

    private bool playerInRange = false;
    private bool isTalking = false;
    private int currentLine = 0;

    private bool usingBadEnding = false;
    private bool usingGoodEnding = false;

    private CanvasGroup dialogueCanvas;
    private CanvasGroup promptCanvas;
    private GameController playerController;
    private PlayerInventory playerInventory;

    private string[] currentDialogue;
    private bool[] currentIsPlayerSpeaking;

    void Start()
    {
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
            dialogueCanvas = dialogueUI.GetComponent<CanvasGroup>() ?? dialogueUI.AddComponent<CanvasGroup>();
            dialogueCanvas.alpha = 0f;
        }

        if (promptUI != null)
        {
            promptCanvas = promptUI.GetComponent<CanvasGroup>() ?? promptUI.AddComponent<CanvasGroup>();
            promptCanvas.alpha = 0f;
            promptUI.SetActive(false);
        }

        if (npcSprite != null) leftPortrait.sprite = npcSprite;
        if (playerSprite != null) rightPortrait.sprite = playerSprite;

        leftPortrait.color = inactiveColor;
        rightPortrait.color = inactiveColor;
        leftPortrait.transform.localScale = Vector3.one * inactiveScale;
        rightPortrait.transform.localScale = Vector3.one * inactiveScale;

        if (backgroundImage != null)
            backgroundImage.color = new Color(0f, 0f, 0f, 0.4f);

        if (enemiesToActivate != null)
        {
            foreach (GameObject enemy in enemiesToActivate)
                if (enemy != null) enemy.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && !isTalking && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
        else if (isTalking && Input.GetKeyDown(KeyCode.E))
        {
            NextLine();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (promptUI != null)
        {
            promptUI.SetActive(true);
            StopCoroutine(nameof(FadePrompt));
            StartCoroutine(FadePrompt(1f));
        }

        playerController = other.GetComponent<GameController>();
        playerInventory = other.GetComponent<PlayerInventory>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (promptUI != null)
        {
            StopCoroutine(nameof(FadePrompt));
            StartCoroutine(FadePrompt(0f, () => promptUI.SetActive(false)));
        }
    }

    void StartDialogue()
    {
        isTalking = true;
        currentLine = 0;

        Time.timeScale = 0f;

        if (playerInventory != null &&
            playerInventory.HasAllHiddenItems(goodEndingHiddenItems) &&
            playerInventory.HasAllMainItems(badEndingItems))
        {
            currentDialogue = goodEndingDialogueLines;
            currentIsPlayerSpeaking = goodEndingIsPlayerSpeaking;
            usingGoodEnding = true;
            usingBadEnding = false;
        }
        else if (playerInventory != null && playerInventory.HasAllMainItems(badEndingItems))
        {
            currentDialogue = badEndingDialogueLines;
            currentIsPlayerSpeaking = badEndingIsPlayerSpeaking;
            usingBadEnding = true;
            usingGoodEnding = false;
        }
        else
        {
            currentDialogue = normalDialogueLines;
            currentIsPlayerSpeaking = normalIsPlayerSpeaking;
            usingBadEnding = usingGoodEnding = false;
        }

        if (dialogueUI != null) dialogueUI.SetActive(true);
        if (promptUI != null)
        {
            StopCoroutine(nameof(FadePrompt));
            StartCoroutine(FadePrompt(0f, () => promptUI.SetActive(false)));
        }

        if (dialogueCanvas != null) dialogueCanvas.alpha = 1f;
        if (playerController != null) playerController.enabled = false;

        ShowLine();
    }

    void NextLine()
    {
        currentLine++;
        if (currentLine >= currentDialogue.Length)
        {
            StartCoroutine(EndDialogue());
        }
        else
        {
            ShowLine();
        }
    }

    void ShowLine()
    {
        dialogueText.text = currentDialogue[currentLine];
        bool playerSpeaking = currentIsPlayerSpeaking[currentLine];
        SetPortraitHighlight(playerSpeaking);
    }

    void SetPortraitHighlight(bool playerSpeaking)
    {
        StopAllCoroutines();
        if (playerSpeaking)
        {
            StartCoroutine(TransitionPortrait(rightPortrait, activeColor, activeScale));
            StartCoroutine(TransitionPortrait(leftPortrait, inactiveColor, inactiveScale));
        }
        else
        {
            StartCoroutine(TransitionPortrait(leftPortrait, activeColor, activeScale));
            StartCoroutine(TransitionPortrait(rightPortrait, inactiveColor, inactiveScale));
        }
    }

    IEnumerator TransitionPortrait(Image img, Color targetColor, float targetScale)
    {
        Color startColor = img.color;
        Vector3 startScale = img.transform.localScale;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionDuration;
            img.color = Color.Lerp(startColor, targetColor, t);
            img.transform.localScale = Vector3.Lerp(startScale, Vector3.one * targetScale, t);
            yield return null;
        }

        img.color = targetColor;
        img.transform.localScale = Vector3.one * targetScale;
    }

    IEnumerator EndDialogue()
    {
        isTalking = false;

        if (dialogueCanvas != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeOutDuration;
                dialogueCanvas.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            dialogueCanvas.alpha = 0f;
        }

        if (dialogueUI != null) dialogueUI.SetActive(false);

        // Unlock doors if needed
        if (unlockAfterDialogue && doorColliders != null)
        {
            foreach (Collider2D door in doorColliders)
                if (door != null) door.enabled = false;
        }

        // 🔴 如果是 Bad Ending，直接跳转
        if (usingBadEnding)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(badEndingSceneName);
            yield break;
        }

        // ✅ 如果是 Good Ending，对接物品切换
        if (usingGoodEnding)
        {
            ItemSwitchController switcher = FindObjectOfType<ItemSwitchController>();
            if (switcher != null)
            {
                switcher.ShowHappyEndingItems();
            }
        }

        if (enemiesToActivate != null)
        {
            foreach (GameObject enemy in enemiesToActivate)
                if (enemy != null) enemy.SetActive(true);
        }

        if (playerController != null) playerController.enabled = true;

        Time.timeScale = 1f;

        if (playerInRange && promptUI != null)
        {
            promptUI.SetActive(true);
            StartCoroutine(FadePrompt(1f));
        }
    }



    IEnumerator FadePrompt(float targetAlpha, System.Action onComplete = null)
    {
        if (promptCanvas == null) yield break;

        float startAlpha = promptCanvas.alpha;
        float elapsed = 0f;

        while (elapsed < promptFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / promptFadeDuration;
            promptCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        promptCanvas.alpha = targetAlpha;
        onComplete?.Invoke();
    }
}
