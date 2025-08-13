using UnityEngine;

public class FadeOnTrigger : MonoBehaviour
{
    public SpriteRenderer targetRenderer; // Assign in Inspector
    public float fadeDuration = 0.5f; // Time to fade in/out

    private Coroutine fadeRoutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Fade out when entering
            StartFade(0f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Fade in when leaving
            StartFade(1f);
        }
    }

    void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeToAlpha(targetAlpha));
    }

    System.Collections.IEnumerator FadeToAlpha(float targetAlpha)
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

        // Make sure it finishes exactly at the target alpha
        Color finalColor = targetRenderer.color;
        finalColor.a = targetAlpha;
        targetRenderer.color = finalColor;
    }
}
