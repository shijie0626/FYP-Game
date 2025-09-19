using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class HidingSpotTMP : MonoBehaviour
{
    [Header("Input")]
    public KeyCode hideKey = KeyCode.E;
    public float toggleCooldown = 0.5f; // time between hide/unhide swaps

    [Header("References")]
    public PlayerToggleWithBeacon playerToggle;
    public TextMeshProUGUI promptText;
    public string playerTag = "Player";

    [Header("Fade Settings")]
    public float fadeDuration = 0.3f;

    private bool playerInside = false;
    private bool playerHidingHere = false;
    private Coroutine fadeRoutine;
    private bool canToggle = true;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (playerToggle == null)
            playerToggle = FindObjectOfType<PlayerToggleWithBeacon>();

        if (promptText != null)
            SetTextAlpha(0f); // start hidden
    }

    void Update()
    {
        if (playerToggle == null || !canToggle) return;

        // Player inside + not hidden
        if (playerInside && playerToggle.IsPlayerActive())
        {
            if (Input.GetKeyDown(hideKey))
            {
                playerToggle.HidePlayer();
                playerHidingHere = true;
                FadeOutPrompt();
                StartCoroutine(StartCooldown());
            }
        }
        // Player hidden here
        else if (playerHidingHere && !playerToggle.IsPlayerActive())
        {
            if (Input.GetKeyDown(hideKey))
            {
                playerToggle.ShowPlayer();
                playerHidingHere = false;

                if (playerInside) FadeInPrompt();
                StartCoroutine(StartCooldown());
            }
        }
    }

    private IEnumerator StartCooldown()
    {
        canToggle = false;
        yield return new WaitForSeconds(toggleCooldown);
        canToggle = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;

        if (playerToggle != null && playerToggle.IsPlayerActive())
            FadeInPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;

        if (!playerHidingHere) FadeOutPrompt();
    }

    private void FadeInPrompt()
    {
        if (promptText == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTextAlpha(promptText, promptText.color.a, 1f));
    }

    private void FadeOutPrompt()
    {
        if (promptText == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTextAlpha(promptText, promptText.color.a, 0f));
    }

    private IEnumerator FadeTextAlpha(TextMeshProUGUI tmp, float start, float end)
    {
        float t = 0f;
        Color c = tmp.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(start, end, t / fadeDuration);
            tmp.color = c;
            yield return null;
        }
        c.a = end;
        tmp.color = c;
    }

    private void SetTextAlpha(float a)
    {
        if (promptText == null) return;
        Color c = promptText.color;
        c.a = a;
        promptText.color = c;
    }
}
