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
    public Button prevButton;        // Prev 按钮

    [Header("分页参数")]
    public int imagesPerPage = 4;   // 每页显示多少格
    public float moveDuration = 0.5f; // 翻页滑动时间
    public float pageWidth = 1920f;   // 页面宽度（用于翻页位移）

    private int currentImageIndex = 0; // 当前显示到第几张图
    private Vector3 pageStartPos;      // 父物体起始位置（翻页时用）

    void Start()
    {
        // 初始化所有分镜透明，并且按页布局
        for (int i = 0; i < allImages.Length; i++)
        {
            SetImageAlpha(allImages[i], 0f);

            int pageIndex = i / imagesPerPage;  // 第几页
            float offsetX = pageIndex * pageWidth;
            // 把每一页的格子排到不同屏幕宽度的位置
            allImages[i].rectTransform.anchoredPosition += new Vector2(offsetX, 0);
        }

        // 保存初始位置
        pageStartPos = transform.localPosition;

        // 按钮绑定
        EnsureCanvasGroup(nextButton.gameObject);
        nextButton.onClick.AddListener(OnNextButtonClick);
        nextButton.gameObject.SetActive(true);

        EnsureCanvasGroup(finalNextButton.gameObject);
        finalNextButton.onClick.AddListener(OnFinalNext);
        finalNextButton.gameObject.SetActive(false);

        EnsureCanvasGroup(prevButton.gameObject);
        prevButton.onClick.AddListener(OnPrevButtonClick);
        prevButton.gameObject.SetActive(false);
    }

    // —— Next
    void OnNextButtonClick()
    {
        if (currentImageIndex < allImages.Length)
        {
            StartCoroutine(FadeInImage(allImages[currentImageIndex], 0.5f));
            currentImageIndex++;

            prevButton.gameObject.SetActive(true);

            // 如果是最后一格，显示 FinalNext
            if (currentImageIndex == allImages.Length)
            {
                ShowFinalNextButton();
                return;
            }
        }

        // 如果刚好翻完一页，做翻页动画
        if (currentImageIndex % imagesPerPage == 0 && currentImageIndex < allImages.Length)
        {
            StartCoroutine(SwitchPage());
        }
    }

    // —— Prev
    void OnPrevButtonClick()
    {
        if (currentImageIndex > 0)
        {
            currentImageIndex--;
            SetImageAlpha(allImages[currentImageIndex], 0f);
        }

        prevButton.gameObject.SetActive(currentImageIndex > 0);
        nextButton.gameObject.SetActive(true);
        finalNextButton.gameObject.SetActive(false);
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
    IEnumerator SwitchPage()
    {
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = startPos + new Vector3(-pageWidth, 0, 0);

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t / moveDuration);
            yield return null;
        }
        transform.localPosition = targetPos;

        pageStartPos = transform.localPosition; // 更新基准位置
    }

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
