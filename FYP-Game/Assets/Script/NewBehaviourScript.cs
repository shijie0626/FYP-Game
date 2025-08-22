using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInvisibility : MonoBehaviour
{
    private Renderer[] renderers;  // 保存玩家身上的所有Renderer
    private bool isInvisible = false;

    void Start()
    {
        // 找到玩家身上所有 Renderer (例如 SpriteRenderer, MeshRenderer)
        renderers = GetComponentsInChildren<Renderer>();
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

        foreach (Renderer r in renderers)
        {
            r.enabled = !isInvisible; // true = 显示, false = 隐藏
        }
    }
}
