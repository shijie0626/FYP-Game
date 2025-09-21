using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // ✅ 新增

public class LevelManager : MonoBehaviour
{
    public void LoadNextLevel(string levelName)
    {
        SceneManager.LoadScene(levelName); // ✅ 新写法
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}