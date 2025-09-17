using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Video Settings")]
    public VideoPlayer killVideoPlayer;
    public CanvasGroup videoCanvas;

    [Header("UI Settings")]
    public CanvasGroup gameOverPanel;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

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
        if (videoCanvas != null)
            videoCanvas.alpha = 0;

        if (gameOverPanel != null)
        {
            gameOverPanel.alpha = 0;
            gameOverPanel.gameObject.SetActive(false); // 🚫 stay disabled until video ends
        }
    }

    public void KillPlayer(VideoClip clip)
    {
        if (isGameOver) return;
        isGameOver = true;

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
        // Show video
        videoCanvas.alpha = 1f;
        killVideoPlayer.Play();

        // Wait until the video *really* ends
        while (killVideoPlayer.isPlaying || killVideoPlayer.frame < (long)killVideoPlayer.frameCount - 1)
            yield return null;

        // Now enable + fade in Game Over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(true);

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
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
}
