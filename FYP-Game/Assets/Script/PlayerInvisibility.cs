using UnityEngine;

public class PlayerInvisibility : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool isInvisible = false;

    private int normalLayer;
    private int invisibleLayer;

    public bool IsInvisible   // 给外部脚本用来查询状态
    {
        get { return isInvisible; }
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 需要在 Unity 里手动创建 "Player" 和 "InvisiblePlayer" 两个 Layer
        normalLayer = LayerMask.NameToLayer("Player");
        invisibleLayer = LayerMask.NameToLayer("InvisiblePlayer");

        if (normalLayer == -1 || invisibleLayer == -1)
        {
            Debug.LogError("请在 Unity Layer 设置中添加 'Player' 和 'InvisiblePlayer' 层！");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInvisibility();
        }
    }

    void ToggleInvisibility()
    {
        isInvisible = !isInvisible;

        // 切换外观
        spriteRenderer.enabled = !isInvisible;

        // 切换层 (隐身时换到 InvisiblePlayer 层)
        gameObject.layer = isInvisible ? invisibleLayer : normalLayer;
    }
}