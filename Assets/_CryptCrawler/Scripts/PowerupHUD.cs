using UnityEngine;
using TMPro;

public class PowerupHUD : MonoBehaviour
{
    private CrawlerController crawler;
    private MeleeAttack melee;

    private GameObject speedRow;
    private GameObject strengthRow;
    private TMP_Text speedText;
    private TMP_Text strengthText;

    void Start()
    {
        crawler = FindAnyObjectByType<CrawlerController>();
        melee = FindAnyObjectByType<MeleeAttack>();

        Transform speedRowTransform = transform.Find("SpeedRow");
        if (speedRowTransform != null)
        {
            speedRow = speedRowTransform.gameObject;
        }

        Transform strengthRowTransform = transform.Find("StrengthRow");
        if (strengthRowTransform != null)
        {
            strengthRow = strengthRowTransform.gameObject;
        }

        if (speedRow != null)
        {
            speedText = speedRow.GetComponentInChildren<TMP_Text>();
        }

        if (strengthRow != null)
        {
            strengthText = strengthRow.GetComponentInChildren<TMP_Text>();
        }

        if (PlayerPrefs.GetInt("ShowPowerupHUD", 1) == 1)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        float speedRemaining = 0f;
        if (crawler != null)
        {
            speedRemaining = crawler.SpeedBoostRemaining;
        }

        float strengthRemaining = 0f;
        if (melee != null)
        {
            strengthRemaining = melee.StrengthBoostRemaining;
        }

        if (speedRow != null)
        {
            if (speedRemaining > 0f)
            {
                speedRow.SetActive(true);
                if (speedText != null)
                {
                    speedText.text = "Speed  " + speedRemaining.ToString("F1") + "s";
                }
            }
            else
            {
                speedRow.SetActive(false);
            }
        }

        if (strengthRow != null)
        {
            if (strengthRemaining > 0f)
            {
                strengthRow.SetActive(true);
                if (strengthText != null)
                {
                    strengthText.text = "Power  " + strengthRemaining.ToString("F1") + "s";
                }
            }
            else
            {
                strengthRow.SetActive(false);
            }
        }
    }
}
