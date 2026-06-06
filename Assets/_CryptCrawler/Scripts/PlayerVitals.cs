using UnityEngine;
using UnityEngine.UI;

// Player health for Crypt Crawler: HP, damage, healing, health bar UI.
// On death: plays the knight's death animation (if a "die" trigger exists on
// the Animator) and tells the DungeonManager to run the level-lost flow.
public class PlayerVitals : MonoBehaviour
{
    public int maxHealth = 100;
    public Slider healthSlider;
    public AudioClip hurtSFX;

    public static bool IsAlive { get; private set; }

    private int currentHealth;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        IsAlive = true;
        animator = GetComponent<Animator>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
        }
        UpdateHealthSlider();
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;

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
        if (!IsAlive) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthSlider();
    }

    void Die()
    {
        // Play the death animation if the Animator has a "die" trigger.
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
            healthSlider.value = currentHealth;
        }
    }
}
