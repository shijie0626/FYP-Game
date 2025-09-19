using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFootstepSound : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip walkClip;       // 地面走路声音
    public AudioClip climbClip;      // 爬梯子声音

    [Header("Ladder Settings")]
    public string ladderTag = "Ladder";

    [Header("Speed Thresholds")]
    public float walkMinSpeed = 0.1f;   // 走路触发最小速度
    public float climbMinSpeed = 0.1f;  // 爬梯触发最小速度

    [Header("Manual Audio Settings")]
    [Range(0.1f, 3f)]
    public float walkPitch = 1f;       // 走路音调
    [Range(0f, 1f)]
    public float walkVolume = 1f;      // 走路音量
    [Range(0.1f, 3f)]
    public float climbPitch = 1f;      // 爬梯音调
    [Range(0f, 1f)]
    public float climbVolume = 1f;     // 爬梯音量

    private AudioSource audioSource;
    private Rigidbody2D rb;
    private bool isOnLadder = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float verticalSpeed = Mathf.Abs(rb.velocity.y);
        float horizontalSpeed = Mathf.Abs(rb.velocity.x);

        if (isOnLadder)
        {
            if (verticalSpeed > climbMinSpeed && !audioSource.isPlaying)
            {
                PlayStep(climbClip, climbPitch, climbVolume);
            }
        }
        else
        {
            if (horizontalSpeed > walkMinSpeed && !audioSource.isPlaying)
            {
                PlayStep(walkClip, walkPitch, walkVolume);
            }
        }
    }

    private void PlayStep(AudioClip clip, float pitch, float volume)
    {
        audioSource.clip = clip;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(ladderTag))
            isOnLadder = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(ladderTag))
            isOnLadder = false;
    }
}
