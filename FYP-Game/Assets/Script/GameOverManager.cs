using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Video Settings")]
    public VideoPlayer killVideoPlayer;
    public CanvasGroup videoCanvas;

    [Header("UI Settings")]
    public CanvasGroup gameOverPanel;
    public Button restartButton; // assign in inspector

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    [Header("Optional UI Text to Disable on Death")]
    public List<Text> legacyTextsToDisable;          // Legacy UI Text
    public List<TextMeshProUGUI> tmpTextsToDisable;  // TextMeshPro Text

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f; // ensure gameplay starts unpaused

        if (videoCanvas != null)
            videoCanvas.alpha = 0;

        if (gameOverPanel != null)
        {
            gameOverPanel.alpha = 0;
            gameOverPanel.gameObject.SetActive(false);
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    public void KillPlayer(VideoClip clip)
    {
        if (isGameOver) return;
        isGameOver = true;

        // Freeze time immediately
        Time.timeScale = 0f;

        // Disable Legacy UI Texts
        if (legacyTextsToDisable != null)
        {
            foreach (Text txt in legacyTextsToDisable)
            {
                if (txt != null)
                    txt.gameObject.SetActive(false);
            }
        }

        // Disable TextMeshPro Texts
        if (tmpTextsToDisable != null)
        {
            foreach (TextMeshProUGUI tmp in tmpTextsToDisable)
            {
                if (tmp != null)
                    tmp.gameObject.SetActive(false);
            }
        }

        if (killVideoPlayer != null && clip != null)
        {
            killVideoPlayer.clip = clip;
            StartCoroutine(PlayVideoAndShowGameOver());
        }
        else
        {
            ShowGameOverInstant();
        }
    }

    private IEnumerator PlayVideoAndShowGameOver()
    {
        if (videoCanvas != null)
            videoCanvas.alpha = 1f;

        killVideoPlayer.Play();

        // Wait until the video ends
        while (killVideoPlayer.isPlaying || killVideoPlayer.frame < (long)killVideoPlayer.frameCount - 1)
            yield return null;

        if (videoCanvas != null)
            videoCanvas.alpha = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(true);

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime; // unscaled because timeScale = 0
                gameOverPanel.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }

            gameOverPanel.alpha = 1f;
        }
    }

    private void ShowGameOverInstant()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(true);
            gameOverPanel.alpha = 1f;
        }
    }

    // Restart button handler
    public void RestartGame()
    {
        Time.timeScale = 1f; // unfreeze gameplay
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
