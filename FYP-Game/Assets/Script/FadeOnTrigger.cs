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
            StartFade(0f); // 进入房间 -> block 渐隐
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 检查玩家是否隐身
            PlayerInvisibility invis = other.GetComponent<PlayerInvisibility>();
            if (invis != null && invis.IsInvisible)
            {
                // 玩家只是隐身，不算真正离开
                return;
            }

            // 真正离开房间 -> 渐显
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

        // 确保最后到目标透明度
        Color finalColor = targetRenderer.color;
        finalColor.a = targetAlpha;
        targetRenderer.color = finalColor;
    }
}