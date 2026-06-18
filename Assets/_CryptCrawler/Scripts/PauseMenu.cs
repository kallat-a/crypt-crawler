using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    private bool isPaused = false;

    void Awake()
    {
        SetupButton("ResumeButton", Resume);
        SetupButton("RestartButton", RestartLevel);
        SetupButton("MainMenuButton", GoToMainMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else if (DungeonManager.IsPlaying)
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        isPaused = true;
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void SetupButton(string buttonName, UnityEngine.Events.UnityAction action)
    {
        if (pausePanel == null)
        {
            return;
        }

        foreach (Button button in pausePanel.GetComponentsInChildren<Button>(true))
        {
            if (button.name != buttonName)
            {
                continue;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
            return;
        }
    }
}
