using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStepSound : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip walkClip;   // 走路音效
    public AudioClip ladderClip; // 爬梯子音效

    [Header("Settings")]
    public float minSpeed = 0.1f; // 低于这个速度就当作静止

    private AudioSource audioSource;
    private Rigidbody2D rb;
    private AudioClip currentClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;          // ✅ 循环播放
        audioSource.playOnAwake = false;  // ✅ 不要一开始就播

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 velocity = rb.velocity;

        if (Mathf.Abs(velocity.y) > minSpeed)
        {
            // ✅ 只要有 Y 方向速度 → 播放梯子音效
            PlayOrResume(ladderClip);
        }
        else if (Mathf.Abs(velocity.x) > minSpeed)
        {
            // ✅ 只有 X 方向速度 → 播放走路音效
            PlayOrResume(walkClip);
        }
        else
        {
            // ✅ 没有移动 → 暂停当前音效
            PauseIfPlaying();
        }
    }

    private void PlayOrResume(AudioClip clip)
    {
        if (clip == null) return;

        if (currentClip != clip)
        {
            // 换音效 → 立即切换
            currentClip = clip;
            audioSource.clip = currentClip;
            audioSource.Play();
        }
        else if (!audioSource.isPlaying)
        {
            // 继续播放暂停的进度
            audioSource.UnPause();
        }
    }

    private void PauseIfPlaying()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause(); // ✅ 保存进度
        }
    }
}
