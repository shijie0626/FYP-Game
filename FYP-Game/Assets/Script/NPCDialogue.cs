using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

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

    [Header("Good Ending Time Trial Settings")]
    public GameObject[] goodEndingEnemies; // enemies spawned for time trial
    public float goodEndingTimeLimit = 60f; // adjustable in inspector
    public Text timerText; // UI Text showing countdown
    public VideoPlayer deathVideoPlayer; // Death scene video
    public CanvasGroup deathVideoCanvas; // Video canvas
    public CanvasGroup gameOverPanel; // GameOver UI panel
    public CanvasGroup fadePanel; // Fullscreen black panel for fade out
    public Button restartButton; // Restart button

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

    private float remainingTime;
    private bool isTimeTrialActive = false;

    [Header("Z Lock Settings")]
    public float videoZ = -5f;       // above gameplay
    public float gameOverZ = -4f;    // above video

    void Start()
    {
        Time.timeScale = 1f; // Ensure time is running at start

        // Setup dialogue UI
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
            dialogueCanvas = dialogueUI.GetComponent<CanvasGroup>() ?? dialogueUI.AddComponent<CanvasGroup>();
            dialogueCanvas.alpha = 0f;
        }

        // Setup prompt UI
        if (promptUI != null)
        {
            promptCanvas = promptUI.GetComponent<CanvasGroup>() ?? promptUI.AddComponent<CanvasGroup>();
            promptCanvas.alpha = 0f;
            promptUI.SetActive(false);
        }

        // Set portraits
        if (npcSprite != null) leftPortrait.sprite = npcSprite;
        if (playerSprite != null) rightPortrait.sprite = playerSprite;

        leftPortrait.color = inactiveColor;
        rightPortrait.color = inactiveColor;
        leftPortrait.transform.localScale = Vector3.one * inactiveScale;
        rightPortrait.transform.localScale = Vector3.one * inactiveScale;

        // Background color
        if (backgroundImage != null)
            backgroundImage.color = new Color(0f, 0f, 0f, 0.4f);

        // Disable enemies at start
        if (enemiesToActivate != null)
            foreach (GameObject enemy in enemiesToActivate)
                if (enemy != null) enemy.SetActive(false);

        if (goodEndingEnemies != null)
            foreach (GameObject enemy in goodEndingEnemies)
                if (enemy != null) enemy.SetActive(false);

        if (deathVideoCanvas != null)
            deathVideoCanvas.alpha = 0f;

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.alpha = 0f;

        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.gameObject.SetActive(true);
        }

        if (deathVideoPlayer != null)
            deathVideoPlayer.loopPointReached += OnDeathVideoFinished;

        // Setup restart button
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    void Update()
    {
        if (playerInRange && !isTalking && Input.GetKeyDown(KeyCode.E))
            StartDialogue();
        else if (isTalking && Input.GetKeyDown(KeyCode.Space))
            NextLine();
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

        Time.timeScale = 0f; // freeze gameplay

        // Determine which dialogue to use
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

        ShowLine();
    }

    void NextLine()
    {
        currentLine++;
        if (currentLine >= currentDialogue.Length)
            StartCoroutine(EndDialogue());
        else
            ShowLine();
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

        // Fade out dialogue UI
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

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        // Unlock doors
        if (unlockAfterDialogue && doorColliders != null)
        {
            foreach (Collider2D door in doorColliders)
                if (door != null) door.enabled = false;
        }

        // Restore gameplay time
        Time.timeScale = 1f;

        // Bad Ending
        if (usingBadEnding)
        {
            SceneManager.LoadScene(badEndingSceneName);
            yield break;
        }

        // Good Ending Time Trial
        if (usingGoodEnding)
        {
            // Disable previous enemies
            if (enemiesToActivate != null)
                foreach (GameObject enemy in enemiesToActivate)
                    if (enemy != null) enemy.SetActive(false);

            // Activate good ending enemies
            if (goodEndingEnemies != null)
                foreach (GameObject enemy in goodEndingEnemies)
                    if (enemy != null) enemy.SetActive(true);

            remainingTime = goodEndingTimeLimit;
            isTimeTrialActive = true;

            if (timerText != null)
                timerText.gameObject.SetActive(true);

            StartCoroutine(GoodEndingTimer());
        }
        else
        {
            // Normal dialogue finished: activate first batch enemies
            if (enemiesToActivate != null)
            {
                foreach (GameObject enemy in enemiesToActivate)
                    if (enemy != null) enemy.SetActive(true);
            }
        }
    }

    IEnumerator GoodEndingTimer()
    {
        while (isTimeTrialActive && remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = Mathf.Ceil(remainingTime).ToString("F0");
                if (remainingTime <= 10f)
                {
                    float alpha = Mathf.PingPong(Time.time * 2f, 1f);
                    timerText.color = new Color(1f, 0f, 0f, alpha);
                }
                else
                {
                    timerText.color = Color.white;
                }
            }

            yield return null;
        }

        if (remainingTime <= 0f)
        {
            isTimeTrialActive = false;
            if (timerText != null)
                timerText.gameObject.SetActive(false);

            // Freeze gameplay and fade out
            Time.timeScale = 0f;
            if (fadePanel != null)
                yield return StartCoroutine(FadeCanvas(fadePanel, 1f));

            // Play death video
            if (deathVideoPlayer != null && deathVideoCanvas != null)
            {
                deathVideoCanvas.gameObject.SetActive(true);
                deathVideoCanvas.alpha = 1f;
                deathVideoPlayer.Play();
            }
            else
            {
                // fallback if no video
                if (gameOverPanel != null)
                {
                    gameOverPanel.gameObject.SetActive(true);
                    StartCoroutine(FadeCanvas(gameOverPanel, 1f));
                }
            }
        }
    }

    void OnDeathVideoFinished(VideoPlayer vp)
    {
        if (deathVideoCanvas != null)
            deathVideoCanvas.alpha = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(true);
            StartCoroutine(FadeCanvas(gameOverPanel, 1f));
        }
    }

    public void CompleteGoodEnding()
    {
        isTimeTrialActive = false;
        if (timerText != null)
            timerText.gameObject.SetActive(false);

        SceneManager.LoadScene("HappyEnding");
    }

    // Restart button handler
    public void RestartGame()
    {
        Time.timeScale = 1f; // Reset time scale
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    IEnumerator FadeCanvas(CanvasGroup cg, float targetAlpha)
    {
        float startAlpha = cg.alpha;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        cg.alpha = targetAlpha;
    }
}
