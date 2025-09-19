using UnityEngine;
using System.Collections;
using TMPro; // For TextMeshProUGUI

public class FadeOnTrigger : MonoBehaviour
{
    [Header("Room Fade Settings")]
    public SpriteRenderer targetRenderer; // the black overlay in front of the room
    public float fadeDuration = 0.5f;

    private Coroutine fadeRoutine;
    private int occupants = 0; // number of "player-like" colliders currently inside

    [Header("Area Name Settings")]
    public string roomName = "Room";
    public CanvasGroup canvasGroup;      // Assign CanvasGroup of the text
    public TextMeshProUGUI areaNameText; // The TMP text in the middle of screen
    public float textFadeDuration = 1f;
    public float displayDuration = 2f;
    private Coroutine textRoutine;

    private void Awake()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f; // start invisible
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // accept either real player or presence beacon tag
        if (other.CompareTag("Player") || other.CompareTag("PlayerBeacon"))
        {
            occupants++;
            if (occupants == 1) // first occupant -> light up the room
            {
                StartFade(0f); // fade overlay to transparent (room lit)
                ShowAreaName();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerBeacon"))
        {
            occupants = Mathf.Max(0, occupants - 1);
            if (occupants == 0) // nobody left -> darken
                StartFade(1f); // fade overlay to opaque (room dark)
        }
    }

    // -------------------- ROOM FADING --------------------
    void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeToAlpha(targetAlpha));
    }

    IEnumerator FadeToAlpha(float targetAlpha)
    {
        float startAlpha = targetRenderer.color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            Color newColor = targetRenderer.color;
            newColor.a = alpha;
            targetRenderer.color = newColor;
            yield return null;
        }

        Color finalColor = targetRenderer.color;
        finalColor.a = targetAlpha;
        targetRenderer.color = finalColor;
    }

    // -------------------- AREA NAME --------------------
    private void ShowAreaName()
    {
        if (canvasGroup == null || areaNameText == null) return;

        areaNameText.text = roomName;

        if (textRoutine != null) StopCoroutine(textRoutine);
        textRoutine = StartCoroutine(FadeTextRoutine());
    }

    private IEnumerator FadeTextRoutine()
    {
        // Fade In
        yield return StartCoroutine(FadeCanvas(0f, 1f, textFadeDuration));

        // Hold
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        yield return StartCoroutine(FadeCanvas(1f, 0f, textFadeDuration));
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
