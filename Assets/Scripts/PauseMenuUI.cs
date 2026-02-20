using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;   // 在 Inspector 中拖拽赋值
    [SerializeField] private Button quitButton;     // 在 Inspector 中拖拽赋值

    private void Start()
    {
        // 确保按钮已赋值，并且 PauseManager 单例存在
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }
        else
        {
            Debug.LogError("Resume button not assigned in PauseMenuUI");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
        else
        {
            Debug.LogError("Quit button not assigned in PauseMenuUI");
        }
    }

    private void OnResumeClicked()
    {
        // 调用 PauseManager 的继续方法
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.TogglePause(); // 或者直接 ResumeGame()
        }
    }

    private void OnQuitClicked()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.QuitToMainMenu(); // 或 QuitGame()
        }
    }
}
