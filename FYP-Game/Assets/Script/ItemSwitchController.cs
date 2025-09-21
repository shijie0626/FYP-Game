using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ItemChangeController : MonoBehaviour
{
    [Header("旧物品（初始显示）")]
    public GameObject[] oldItems;

    [Header("新物品（显示在满足条件后）")]
    public GameObject[] newItems;

    [Header("音效")]
    public AudioSource itemAppearAudio;        // 物品切换音效
    public AudioSource happyEnemyAudio1;       // Good Ending 敌人出现音效 1
    public AudioSource happyEnemyAudio2;       // Good Ending 敌人出现音效 2
    public AudioSource tickAudio;              // 倒计时每秒滴答音效

    [Header("文字提示")]
    public Text hintText;
    public string message = "新的物品出现了！";
    public float messageDuration = 2f;

    [Header("倒计时设置")]
    public Text timerText;
    public float countdownTime = 120f;         // 默认倒计时时间

    [Header("Game Over / 视频相关")]
    public CanvasGroup fadePanel;
    public CanvasGroup deathVideoCanvas;
    public VideoPlayer deathVideoPlayer;
    public CanvasGroup gameOverPanel;

    private float remainingTime;
    private bool isCountdownActive = false;

    void Start()
    {
        if (deathVideoPlayer != null)
            deathVideoPlayer.loopPointReached += OnDeathVideoFinished;

        if (gameOverPanel != null)
            gameOverPanel.alpha = 0f;
        if (deathVideoCanvas != null)
            deathVideoCanvas.alpha = 0f;
        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示新物品 + 播放音效 + 提示文字
    /// </summary>
    public void ShowHappyEndingItems()
    {
        SetActiveArray(oldItems, false);
        SetActiveArray(newItems, true);

        if (itemAppearAudio != null) itemAppearAudio.Play();

        if (hintText != null)
        {
            hintText.text = message;
            hintText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideHintText));
            Invoke(nameof(HideHintText), messageDuration);
        }

        // 播放 Good Ending 敌人出现音效
        if (happyEnemyAudio1 != null) happyEnemyAudio1.Play();
        if (happyEnemyAudio2 != null) happyEnemyAudio2.Play();
    }

    /// <summary>
    /// 启动倒计时
    /// </summary>
    public void StartCountdown(float duration)
    {
        remainingTime = duration;
        isCountdownActive = true;

        if (timerText != null)
            timerText.gameObject.SetActive(true);

        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        float tickTimer = 0f;

        while (isCountdownActive && remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (tickAudio != null && tickTimer >= 1f)
            {
                tickAudio.Play();
                tickTimer = 0f;
            }

            if (timerText != null)
            {
                timerText.text = Mathf.Ceil(remainingTime).ToString("F0");

                if (remainingTime <= 60f)
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

        if (tickAudio != null) tickAudio.Stop();
        isCountdownActive = false;

        if (timerText != null) timerText.gameObject.SetActive(false);

        // 倒计时结束 → 播放 Game Over
        TriggerGameOver();
    }

    /// <summary>
    /// 倒计时结束 或 视频结束 → Game Over
    /// </summary>
    private void TriggerGameOver()
    {
        Time.timeScale = 0f;

        if (fadePanel != null) StartCoroutine(FadeCanvas(fadePanel, 1f));

        if (deathVideoPlayer != null && deathVideoCanvas != null)
        {
            deathVideoCanvas.gameObject.SetActive(true);
            deathVideoCanvas.alpha = 1f;
            deathVideoPlayer.Play();
        }
        else if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(true);
            StartCoroutine(FadeCanvas(gameOverPanel, 1f));

            // ✅ 出现 Game Over Panel 时立即停掉所有音效
            StopAllAudio();
        }
    }

    private void OnDeathVideoFinished(VideoPlayer vp)
    {
        if (deathVideoCanvas != null) deathVideoCanvas.alpha = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(true);
            StartCoroutine(FadeCanvas(gameOverPanel, 1f));

            // ✅ 死亡视频播放完出现 Game Over Panel → 停掉所有音效
            StopAllAudio();
        }
    }

    /// <summary>
    /// 停止所有音效
    /// </summary>
    public void StopAllAudio()
    {
        if (tickAudio != null) tickAudio.Stop();
        if (happyEnemyAudio1 != null) happyEnemyAudio1.Stop();
        if (happyEnemyAudio2 != null) happyEnemyAudio2.Stop();
        if (itemAppearAudio != null) itemAppearAudio.Stop();
    }

    private void SetActiveArray(GameObject[] items, bool active)
    {
        if (items == null) return;
        foreach (var obj in items)
        {
            if (obj != null) obj.SetActive(active);
        }
    }

    private void HideHintText()
    {
        if (hintText != null)
            hintText.gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvas(CanvasGroup canvas, float targetAlpha, float duration = 1f)
    {
        if (canvas == null) yield break;

        float startAlpha = canvas.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            canvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        canvas.alpha = targetAlpha;
    }
}
