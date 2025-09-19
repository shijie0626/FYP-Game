using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    private bool isPaused = false;

    void Update()
    {
        // 按下 Esc 时切换暂停
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                Pause();
            }
        }
    }

    // 暂停游戏
    public void Pause()
    {
        Debug.Log("游戏暂停");
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    // 继续游戏（按钮调用）
    public void Continue()
    {
        Debug.Log("游戏继续");
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    // 回到主菜单
    public void Home()
    {
        Debug.Log("返回主菜单");
        Time.timeScale = 1f; // 记得恢复时间
        SceneManager.LoadScene("Main Menu");
    }

    // 重新开始
    public void Restart()
    {
        Debug.Log("重新开始当前关卡");
        Time.timeScale = 1f; // 记得恢复时间
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
