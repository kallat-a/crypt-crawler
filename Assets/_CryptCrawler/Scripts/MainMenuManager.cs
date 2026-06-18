using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject storyPanel;

    public TMP_Text healthToggleText;
    public TMP_Text powerupToggleText;
    public TMP_Text volumeText;
    public TMP_Text statsText;

    void Start()
    {
        ShowPanel(mainPanel);
        RefreshToggles();
        ApplyVolume();
        RefreshStats();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Level1_3rd_Person");
    }

    public void OpenSettings()
    {
        ShowPanel(settingsPanel);
    }

    public void OpenCredits()
    {
        ShowPanel(creditsPanel);
    }

    public void OpenStory()
    {
        ShowPanel(storyPanel);
    }

    public void Back()
    {
        ShowPanel(mainPanel);
    }

    public void ToggleHealthValues()
    {
        if (PlayerPrefs.GetInt("ShowHealthValues", 1) == 1)
        {
            PlayerPrefs.SetInt("ShowHealthValues", 0);
        }
        else
        {
            PlayerPrefs.SetInt("ShowHealthValues", 1);
        }
        PlayerPrefs.Save();
        RefreshToggles();
    }

    public void TogglePowerupHUD()
    {
        if (PlayerPrefs.GetInt("ShowPowerupHUD", 1) == 1)
        {
            PlayerPrefs.SetInt("ShowPowerupHUD", 0);
        }
        else
        {
            PlayerPrefs.SetInt("ShowPowerupHUD", 1);
        }
        PlayerPrefs.Save();
        RefreshToggles();
    }

    public void CycleVolume()
    {
        int current = PlayerPrefs.GetInt("Volume", 100);
        int next = 100;
        if (current == 100)
        {
            next = 25;
        }
        else if (current == 25)
        {
            next = 50;
        }
        else if (current == 50)
        {
            next = 75;
        }
        PlayerPrefs.SetInt("Volume", next);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    void ApplyVolume()
    {
        int vol = PlayerPrefs.GetInt("Volume", 100);
        AudioListener.volume = vol / 100f;
        if (volumeText != null)
        {
            volumeText.text = "Volume: " + vol + "%";
        }
    }

    void RefreshStats()
    {
        if (statsText == null)
        {
            return;
        }

        int highestLevel = PlayerPrefs.GetInt("HighestLevel", 0);
        float timePlayed = PlayerPrefs.GetFloat("TimePlayed", 0f);
        int enemiesKilled = PlayerPrefs.GetInt("EnemiesKilled", 0);
        int bossesKilled = PlayerPrefs.GetInt("BossesKilled", 0);

        string levelName = "None";
        if (highestLevel == 1)
        {
            levelName = "Level 1";
        }
        else if (highestLevel >= 2)
        {
            levelName = "Level 2";
        }

        int totalSeconds = (int)timePlayed;
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        string timeStr;
        if (hours > 0)
        {
            timeStr = hours + "h " + minutes + "m";
        }
        else if (minutes > 0)
        {
            timeStr = minutes + "m " + seconds + "s";
        }
        else
        {
            timeStr = seconds + "s";
        }

        statsText.text = "Furthest: " + levelName
            + "   Time: " + timeStr
            + "   Enemies: " + enemiesKilled
            + "   Bosses: " + bossesKilled;
    }

    void RefreshToggles()
    {
        if (healthToggleText != null)
        {
            if (PlayerPrefs.GetInt("ShowHealthValues", 1) == 1)
            {
                healthToggleText.text = "Health Values: ON";
            }
            else
            {
                healthToggleText.text = "Health Values: OFF";
            }
        }

        if (powerupToggleText != null)
        {
            if (PlayerPrefs.GetInt("ShowPowerupHUD", 1) == 1)
            {
                powerupToggleText.text = "Powerup HUD: ON";
            }
            else
            {
                powerupToggleText.text = "Powerup HUD: OFF";
            }
        }
    }

    void ShowPanel(GameObject target)
    {
        if (target == mainPanel)
        {
            mainPanel.SetActive(true);
        }
        else
        {
            mainPanel.SetActive(false);
        }

        if (target == settingsPanel)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            settingsPanel.SetActive(false);
        }

        if (target == creditsPanel)
        {
            creditsPanel.SetActive(true);
        }
        else
        {
            creditsPanel.SetActive(false);
        }

        if (target == storyPanel)
        {
            storyPanel.SetActive(true);
        }
        else
        {
            storyPanel.SetActive(false);
        }
    }
}
