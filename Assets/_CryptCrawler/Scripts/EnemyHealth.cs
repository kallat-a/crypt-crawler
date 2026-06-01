using UnityEngine;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public TMP_Text healthText;
    int currentHealth;

    EnemyBehavior enemyBehavior;

    void Start()
    {
        currentHealth = maxHealth;
        enemyBehavior =
            GetComponent<EnemyBehavior>();
        UpdateHealthText();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateHealthText();

        if (currentHealth <= 0)
        {
            enemyBehavior.DestroyEnemy();
        }
    }



    void UpdateHealthText()
    {
        if (healthText)
        {
            healthText.text = "Health: " + currentHealth.ToString();
        }
    }
}