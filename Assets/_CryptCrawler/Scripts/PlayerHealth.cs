using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public TMP_Text healthText;

    LevelManager levelManager;

    void Start()
    {
        currentHealth = maxHealth;

        levelManager =
            FindAnyObjectByType<LevelManager>();

        UpdateHealthText();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        UpdateHealthText();

        if (currentHealth <= 0)
        {
            levelManager.LevelLost();
        }
    }

    void UpdateHealthText()
    {
        if (healthText)
        {
            healthText.text =
                "Health: " + currentHealth;
        }

        levelManager.SetHealthText(currentHealth);
    }
}