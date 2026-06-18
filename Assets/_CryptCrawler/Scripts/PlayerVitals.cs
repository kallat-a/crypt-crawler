using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerVitals : MonoBehaviour
{
    public int maxHealth = 100;
    public Slider healthSlider;
    public AudioClip hurtSFX;
    public float healthBarLerpSpeed = 5f;

    public static bool IsAlive { get; private set; }

    private int currentHealth;
    private float displayedHealth;
    private Animator animator;
    private TMP_Text healthValueText;

    void Start()
    {
        currentHealth = maxHealth;
        displayedHealth = currentHealth;
        IsAlive = true;
        animator = GetComponent<Animator>();

        GameObject hvGo = GameObject.Find("HealthValueText");
        if (hvGo != null)
        {
            healthValueText = hvGo.GetComponent<TMP_Text>();
        }

        if (healthValueText != null)
        {
            if (PlayerPrefs.GetInt("ShowHealthValues", 1) == 1)
            {
                healthValueText.gameObject.SetActive(true);
            }
            else
            {
                healthValueText.gameObject.SetActive(false);
            }
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
        }
        UpdateHealthSlider();
    }

    void Update()
    {
        if (healthSlider == null)
        {
            return;
        }

        // creates a smooth effect when health is added or removed by lerping the health bar to the actual health
        displayedHealth = Mathf.Lerp(displayedHealth, currentHealth, healthBarLerpSpeed * Time.deltaTime);
        healthSlider.value = displayedHealth;
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive)
        {
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthSlider();

        if (hurtSFX != null)
        {
            AudioSource.PlayClipAtPoint(hurtSFX, transform.position);
        }

        if (currentHealth <= 0)
        {
            IsAlive = false;
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive)
        {
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthSlider();
    }

    void Die()
    {
        if (animator != null)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == "die")
                {
                    animator.SetTrigger("die");
                    break;
                }
            }
        }

        DungeonManager manager = FindAnyObjectByType<DungeonManager>();
        if (manager != null)
        {
            manager.LevelLost();
        }
    }

    void UpdateHealthSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.value = displayedHealth;
        }

        if (healthValueText != null && healthValueText.gameObject.activeSelf)
        {
            healthValueText.text = currentHealth + " / " + maxHealth;
        }
    }
}
