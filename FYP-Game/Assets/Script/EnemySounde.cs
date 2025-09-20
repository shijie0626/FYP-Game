using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyChaseAudio : MonoBehaviour
{
    [Header("追逐音乐设置")]
    public AudioClip chaseMusic;   // 鬼发现玩家时播放的音乐
    [Range(0f, 1f)]
    public float volume = 1f;

    private AudioSource audioSource;
    private bool isPlaying = false; // 避免重复播放

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true; // 追逐音乐一般是循环播放
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// 鬼检测到玩家时调用
    /// </summary>
    public void StartChaseMusic()
    {
        if (!isPlaying && chaseMusic != null)
        {
            audioSource.clip = chaseMusic;
            audioSource.volume = volume;
            audioSource.Play();
            isPlaying = true;
        }
    }

    /// <summary>
    /// 玩家逃脱 / 鬼失去目标时调用
    /// </summary>
    public void StopChaseMusic()
    {
        if (isPlaying)
        {
            audioSource.Stop();
            isPlaying = false;
        }
    }
}
