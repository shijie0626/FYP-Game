using UnityEngine;

public class ItemSwitchController : MonoBehaviour
{
    [Header("旧物品（初始显示的2个）")]
    public GameObject[] oldItems;

    [Header("新物品（Happy Ending后显示的2个）")]
    public GameObject[] newItems;

    void Start()
    {
        // ✅ 游戏开始 → 显示旧物品，隐藏新物品
        SetActiveArray(oldItems, true);
        SetActiveArray(newItems, false);
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
    }

    private void SetActiveArray(GameObject[] items, bool active)
    {
        if (items == null) return;
        foreach (GameObject obj in items)
        {
            if (obj != null) obj.SetActive(active);
        }
    }
}
