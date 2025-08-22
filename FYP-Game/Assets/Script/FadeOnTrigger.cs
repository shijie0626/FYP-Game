using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOnTrigger : MonoBehaviour
{
    public SpriteRenderer targetRenderer;
    public float fadeDuration = 0.5f;

    private Coroutine fadeRoutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ??? -> ?????
            StartFade(0f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ???????????
            PlayerInvisibility invis = other.GetComponent<PlayerInvisibility>();
            if (invis != null && invis.IsInvisible)
            {
                // ????????????? -> ???
                return;
            }

            // ???????? -> ??
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

        // ??????????
        Color finalColor = targetRenderer.color;
        finalColor.a = targetAlpha;
        targetRenderer.color = finalColor;
    }


}