using UnityEngine;
using System.Collections;

public class FadeOnTrigger : MonoBehaviour
{
    public SpriteRenderer targetRenderer; // the black overlay in front of the room
    public float fadeDuration = 0.5f;

    private Coroutine fadeRoutine;
    private int occupants = 0; // number of "player-like" colliders currently inside

    private void OnTriggerEnter2D(Collider2D other)
    {
        // accept either real player or presence beacon tag
        if (other.CompareTag("Player") || other.CompareTag("PlayerBeacon"))
        {
            occupants++;
            if (occupants == 1) // first occupant -> light up the room
                StartFade(0f); // fade overlay to transparent (room lit)
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
}
