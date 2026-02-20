using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    void Awake()
    {
        // 确保单例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 让它在场景切换时不被销毁
    }

    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private string pauseSceneName = "PauseScene"; // 暂停场景的名称

    private bool isPaused = false;
    private bool isSceneLoaded = false; // 标记暂停场景是否已加载

    void Update()
    {
        // 按下暂停键时切换暂停状态
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (!isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        if (!isSceneLoaded)
        {
            // 以叠加模式加载暂停场景
            SceneManager.LoadSceneAsync(pauseSceneName, LoadSceneMode.Additive);
            isSceneLoaded = true;
        }

        // 暂停游戏时间
        Time.timeScale = 0f;
        isPaused = true;

        // 显示鼠标光标（可根据需要）
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        if (isSceneLoaded)
        {
            // 卸载暂停场景
            SceneManager.UnloadSceneAsync(pauseSceneName);
            isSceneLoaded = false;
        }

        // 恢复游戏时间
        Time.timeScale = 1f;
        isPaused = false;

    }

    // 提供给暂停场景中的按钮调用的方法
    public void QuitToMainMenu()
    {
        // 先恢复时间，再切换场景
        Time.timeScale = 1f;
        // 卸载暂停场景（如果还加载着）
        if (isSceneLoaded)
            SceneManager.UnloadSceneAsync(pauseSceneName);
        // 加载主菜单场景
        SceneManager.LoadScene("StartScene");
    }
}
