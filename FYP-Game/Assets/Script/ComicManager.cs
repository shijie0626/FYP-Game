using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComicController : MonoBehaviour
{
    [Header("所有漫画页 (每页是一个 Panel，里面有若干分镜 Image)")]
    public RectTransform[] pages;

    [Header("按钮")]
    public Button nextButton;        // 普通 Next（翻格/翻页）
    public Button finalNextButton;   // 最后一页专用 Next（所有分镜出现后才显示）
    public Button prevButton;        // Prev 按钮

    [Header("翻页动画参数")]
    public float moveDuration = 0.5f;    // 翻页滑动时间
    public float pageWidth = 1920f;      // 屏幕/页面宽度，用来计算左右滑动距离

    private Vector2 centerPos = Vector2.zero;
    private Vector2 leftPos;
    private Vector2 rightPos;

    private int currentPageIndex = 0;   // 当前页
    private int currentPanelIndex = 0;  // 当前页分镜序号
    private Image[] currentImages;      // 当前页的所有分镜
    private int[] panelProgress;        // 记录每页看到的分镜数量

    void Start()
    {
        // 根据 pageWidth 计算左右位置
        leftPos = new Vector2(-pageWidth, 0);
        rightPos = new Vector2(pageWidth, 0);

        // 初始化所有页的位置：第 0 页在中间，其余在右侧
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].anchoredPosition = (i == 0) ? centerPos : rightPos;
        }

        // 初始化第一页的所有分镜为透明
        if (pages.Length > 0)
        {
            currentImages = pages[0].GetComponentsInChildren<Image>(includeInactive: true);
            foreach (var img in currentImages) SetImageAlpha(img, 0f);
        }

        // 初始化每页的进度
        panelProgress = new int[pages.Length];
        for (int i = 0; i < panelProgress.Length; i++) panelProgress[i] = 0;

        // 按钮绑定 & 初始可见性
        EnsureCanvasGroup(nextButton.gameObject);
        nextButton.onClick.AddListener(OnNextButtonClick);
        nextButton.gameObject.SetActive(true);

        EnsureCanvasGroup(finalNextButton.gameObject);
        finalNextButton.onClick.AddListener(OnFinalNext);
        finalNextButton.gameObject.SetActive(false); // 只有最后一页完结时才出现

        EnsureCanvasGroup(prevButton.gameObject);
        prevButton.onClick.AddListener(OnPrevButtonClick);
        prevButton.gameObject.SetActive(false); // 首页一开始不能点 Prev
    }

    // —— Next
    void OnNextButtonClick()
    {
        if (currentPanelIndex < currentImages.Length)
        {
            ShowNextPanel();
        }
        else
        {
            if (IsLastPage())
            {
                ShowFinalNextButton();
                return;
            }
            OnNextPage();
        }
    }

    // —— Prev
    void OnPrevButtonClick()
    {
        if (currentPanelIndex > 0)
        {
            // 隐藏最后一个已显示的分镜
            currentPanelIndex--;
            SetImageAlpha(currentImages[currentPanelIndex], 0f);
        }
        else
        {
            // 当前页已经是第 0 格，翻到前一页
            OnPrevPage();
        }

        // 首页时禁用 Prev
        prevButton.gameObject.SetActive(!(currentPageIndex == 0 && currentPanelIndex == 0));
    }

    // —— 显示下一个分镜
    void ShowNextPanel()
    {
        StartCoroutine(FadeInImage(currentImages[currentPanelIndex], 1f));
        currentPanelIndex++;

        prevButton.gameObject.SetActive(true);

        if (currentPanelIndex == currentImages.Length && IsLastPage())
        {
            ShowFinalNextButton();
        }
    }

    // —— 翻到下一页
    void OnNextPage()
    {
        int nextPage = currentPageIndex + 1;
        if (nextPage >= pages.Length)
        {
            Debug.Log("所有漫画页展示完毕！");
            nextButton.gameObject.SetActive(false);
            return;
        }

        panelProgress[currentPageIndex] = currentPanelIndex;
        StartCoroutine(SwitchPage(currentPageIndex, nextPage));

        currentPageIndex = nextPage;
        currentImages = pages[currentPageIndex].GetComponentsInChildren<Image>(includeInactive: true);
        currentPanelIndex = Mathf.Clamp(panelProgress[currentPageIndex], 0, currentImages.Length);

        for (int i = 0; i < currentImages.Length; i++)
        {
            SetImageAlpha(currentImages[i], i < currentPanelIndex ? 1f : 0f);
        }

        nextButton.gameObject.SetActive(true);
        finalNextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(true);
    }

    // —— 翻到上一页
    void OnPrevPage()
    {
        int prevPage = currentPageIndex - 1;
        if (prevPage < 0) return;

        panelProgress[currentPageIndex] = currentPanelIndex;
        StartCoroutine(SwitchPage(currentPageIndex, prevPage));

        currentPageIndex = prevPage;
        currentImages = pages[currentPageIndex].GetComponentsInChildren<Image>(includeInactive: true);
        currentPanelIndex = Mathf.Clamp(panelProgress[currentPageIndex], 0, currentImages.Length);

        for (int i = 0; i < currentImages.Length; i++)
        {
            SetImageAlpha(currentImages[i], i < currentPanelIndex ? 1f : 0f);
        }

        prevButton.gameObject.SetActive(!(currentPageIndex == 0 && currentPanelIndex == 0));
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
        Debug.Log("最后一页结束：在这里做收尾动作（例如开始游戏/切场景）");
    }

    // ———— 动画工具 ————
    IEnumerator SwitchPage(int fromIndex, int toIndex)
    {
        float elapsed = 0f;

        Vector2 fromStart = pages[fromIndex].anchoredPosition;
        Vector2 fromTarget = (toIndex > fromIndex) ? leftPos : rightPos;

        Vector2 toStart = (toIndex > fromIndex) ? rightPos : leftPos;
        Vector2 toTarget = centerPos;

        pages[toIndex].anchoredPosition = toStart;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;

            pages[fromIndex].anchoredPosition = Vector2.Lerp(fromStart, fromTarget, t);
            pages[toIndex].anchoredPosition = Vector2.Lerp(toStart, toTarget, t);

            yield return null;
        }

        pages[fromIndex].anchoredPosition = fromTarget;
        pages[toIndex].anchoredPosition = toTarget;
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

    bool IsLastPage() => currentPageIndex == pages.Length - 1;
}
