using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    public TMP_Text healthToggleText;
    public TMP_Text powerupToggleText;

    void Start()
    {
        ShowPanel(mainPanel);
        RefreshToggles();
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
        mainPanel.SetActive(target == mainPanel);
        settingsPanel.SetActive(target == settingsPanel);
        creditsPanel.SetActive(target == creditsPanel);
    }
}
