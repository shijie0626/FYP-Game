using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComicController : MonoBehaviour
{
    [Header("所有分镜图 (顺序排列)")]
    public Image[] allImages;   // Inspector 里按顺序拖进来

    [Header("按钮")]
    public Button nextButton;        // 普通 Next（翻格/翻页）
    public Button finalNextButton;   // 最后一页专用 Next

    [Header("分页参数")]
    public int imagesPerPage = 4;      // 每页显示多少格
    public float fadeDuration = 0.5f;  // 单格淡入时间
    public float pageFadeDuration = 0.8f; // 整页淡出时间

    private int currentImageIndex = 0; // 当前显示到第几张图
    private int currentPage = 0;       // 当前页号

    void Start()
    {
        // 初始化所有分镜透明
        for (int i = 0; i < allImages.Length; i++)
        {
            SetImageAlpha(allImages[i], 0f);
        }

        // 按钮绑定
        EnsureCanvasGroup(nextButton.gameObject);
        nextButton.onClick.AddListener(OnNextButtonClick);
        nextButton.gameObject.SetActive(true);

        EnsureCanvasGroup(finalNextButton.gameObject);
        finalNextButton.onClick.AddListener(OnFinalNext);
        finalNextButton.gameObject.SetActive(false);
    }

    // —— Next
    void OnNextButtonClick()
    {
        // 还有图片没出现 → 逐格显示
        if (currentImageIndex < allImages.Length)
        {
            StartCoroutine(FadeInImage(allImages[currentImageIndex], fadeDuration));
            currentImageIndex++;

            // 如果是最后一格 → 切换按钮
            if (currentImageIndex == allImages.Length)
            {
                ShowFinalNextButton();
                return;
            }

            // 如果刚好显示满一页 → 下一次点击才翻页
            if (currentImageIndex % imagesPerPage == 0)
            {
                // 先把按钮点击事件改成“翻页”
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(OnPageFlip);
            }
        }
    }

    // —— 翻页（整页渐隐 + 下一页继续出现）
    void OnPageFlip()
    {
        StartCoroutine(FadeOutPageAndContinue());
    }

    IEnumerator FadeOutPageAndContinue()
    {
        // 当前页的起始下标
        int start = currentPage * imagesPerPage;
        int end = Mathf.Min(start + imagesPerPage, allImages.Length);

        // 整页淡出
        for (int i = start; i < end; i++)
        {
            StartCoroutine(FadeOutImage(allImages[i], pageFadeDuration));
        }

        yield return new WaitForSeconds(pageFadeDuration);

        currentPage++;

        // 恢复按钮为“逐格显示”
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNextButtonClick);
    }

    // —— 最后一页：显示最终 Next
    void ShowFinalNextButton()
    {
        nextButton.gameObject.SetActive(false);
        finalNextButton.gameObject.SetActive(true);

        var cg = finalNextButton.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        StartCoroutine(FadeInCanvasGroup(cg, 1.2f));
    }

    void OnFinalNext()
    {
        Debug.Log("最后一页结束：这里可以切场景或进入游戏");
    }

    // ———— 动画工具 ————
    IEnumerator FadeInImage(Image img, float duration)
    {
        float t = 0f;
        Color c = img.color;
        c.a = 0f; img.color = c;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            img.color = c;
            yield return null;
        }
        c.a = 1f; img.color = c;
    }

    IEnumerator FadeOutImage(Image img, float duration)
    {
        float t = 0f;
        Color c = img.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            img.color = c;
            yield return null;
        }
        c.a = 0f; img.color = c;
    }

    IEnumerator FadeInCanvasGroup(CanvasGroup cg, float duration)
    {
        float t = 0f;
        cg.alpha = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    void SetImageAlpha(Image img, float alpha)
    {
        var c = img.color;
        c.a = alpha;
        img.color = c;
    }

    void EnsureCanvasGroup(GameObject go)
    {
        if (go.GetComponent<CanvasGroup>() == null)
            go.AddComponent<CanvasGroup>();
    }
}
