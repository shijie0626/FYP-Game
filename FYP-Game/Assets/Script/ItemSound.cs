using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class KeyItemPickupSound : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip pickupClip;   // 捡到物品音效
    [Range(0f, 1f)]
    public float volume = 1f;      // 音量

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void PlayPickupSound()
    {
        if (pickupClip != null)
        {
            audioSource.PlayOneShot(pickupClip, volume);
        }
    }
}
