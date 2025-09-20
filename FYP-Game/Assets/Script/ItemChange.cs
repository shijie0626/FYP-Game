using UnityEngine;
using UnityEngine.UI;       // 如果用的是 Unity 自带 UI
// using TMPro;            // 如果你用的是 TextMeshPro 就启用这个

public class ItemSwitchController : MonoBehaviour
{
    [Header("旧物品（初始显示的2个）")]
    public GameObject[] oldItems;

    [Header("新物品（Happy Ending后显示的2个）")]
    public GameObject[] newItems;

    [Header("音效设置")]
    public AudioSource itemAppearAudio;   // 播放新物品出现时的音效

    [Header("文字提示设置")]
    public Text hintText;                 // 普通 UI Text
    // public TMP_Text hintText;          // 如果你用的是 TextMeshPro，请用这个
    public string message = "新的物品出现了！";
    public float messageDuration = 2f;    // 提示持续时间

    void Start()
    {
        // ✅ 游戏开始 → 显示旧物品，隐藏新物品
        SetActiveArray(oldItems, true);
        SetActiveArray(newItems, false);

        if (hintText != null)
            hintText.gameObject.SetActive(false); // 默认隐藏提示
    }

    /// <summary>
    /// 在 Happy Ending 对话触发后调用
    /// </summary>
    public void ShowHappyEndingItems()
    {
        // 隐藏旧的两个
        SetActiveArray(oldItems, false);
        // 显示新的两个
        SetActiveArray(newItems, true);

        // 播放音效
        if (itemAppearAudio != null)
        {
            itemAppearAudio.Play();
        }

        // 显示提示文字
        if (hintText != null)
        {
            hintText.text = message;
            hintText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideHintText)); // 避免多次调用叠加
            Invoke(nameof(HideHintText), messageDuration);
        }
    }

    private void SetActiveArray(GameObject[] items, bool active)
    {
        if (items == null) return;
        foreach (GameObject obj in items)
        {
            if (obj != null) obj.SetActive(active);
        }
    }

    private void HideHintText()
    {
        if (hintText != null)
            hintText.gameObject.SetActive(false);
    }
}
