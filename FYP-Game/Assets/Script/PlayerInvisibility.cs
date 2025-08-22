using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInvisibility : MonoBehaviour
{
    private bool isInvisible = false;  // 实际存储的隐身状态

    public bool IsInvisible   // 外部可以读
    {
        get { return isInvisible; }
    }

    private SpriteRenderer spriteRenderer;
    private int normalLayer;
    private int invisibleLayer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalLayer = LayerMask.NameToLayer("Player");
        invisibleLayer = LayerMask.NameToLayer("InvisiblePlayer");
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

        // 切换层
        gameObject.layer = isInvisible ? invisibleLayer : normalLayer;
    }
}
