using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class KeyItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemID;
    public bool isHiddenItem = false;  // Always false for KeyItem

    [Header("UI Prompt")]
    public GameObject promptUI;
    public Text legacyText;                 // optional, for legacy Text
    public TextMeshProUGUI tmpText;         // optional, for TMP
    public float fadeDuration = 0.3f;

    [Header("Audio")]
    public AudioClip pickupClip;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Visuals")]
    public bool hideOnPickup = true; // hide item when picked up

    private bool playerInRange = false;
    private AudioSource audioSource;
    private Coroutine fadeRoutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (promptUI != null)
            promptUI.SetActive(false);

        SetPromptAlpha(0f);

        // Destroy if already collected
        if (GameController.Instance != null && GameController.Instance.HasItemBeenCollected(itemID))
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            CollectItem();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        FadeInPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        FadeOutPrompt();
    }

    void CollectItem()
    {
        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv != null)
            inv.AddItem(itemID, false);

        if (GameController.Instance != null)
            GameController.Instance.SetCheckpoint(transform.position, itemID);

        FadeOutPrompt();

        if (pickupClip != null)
            audioSource.PlayOneShot(pickupClip, volume);

        if (hideOnPickup)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
            foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
                c.enabled = false;
        }

        if (pickupClip != null)
            StartCoroutine(DestroyAfterSound());
        else
            Destroy(gameObject);
    }

    IEnumerator DestroyAfterSound()
    {
        yield return new WaitForSeconds(pickupClip.length);
        Destroy(gameObject);
    }

    private void FadeInPrompt()
    {
        if (promptUI == null) return;
        promptUI.SetActive(true);
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadePromptAlpha(GetCurrentPromptText(), GetCurrentAlpha(), 1f));
    }

    private void FadeOutPrompt()
    {
        if (promptUI == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadePromptAlpha(GetCurrentPromptText(), GetCurrentAlpha(), 0f));
    }

    private IEnumerator FadePromptAlpha(Graphic textGraphic, float start, float end)
    {
        float t = 0f;
        Color c = textGraphic.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(start, end, t / fadeDuration);
            textGraphic.color = c;
            yield return null;
        }
        c.a = end;
        textGraphic.color = c;

        if (end == 0f)
            promptUI.SetActive(false);
    }

    private Graphic GetCurrentPromptText()
    {
        if (tmpText != null) return tmpText;
        if (legacyText != null) return legacyText;
        return null;
    }

    private float GetCurrentAlpha()
    {
        Graphic g = GetCurrentPromptText();
        if (g != null) return g.color.a;
        return 0f;
    }

    private void SetPromptAlpha(float a)
    {
        Graphic g = GetCurrentPromptText();
        if (g != null)
        {
            Color c = g.color;
            c.a = a;
            g.color = c;
        }
    }
}
