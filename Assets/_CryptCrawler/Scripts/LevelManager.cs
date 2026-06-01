using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class LevelManager : MonoBehaviour
{
    public static bool IsPlaying { get; private set; }

    public string nextLevel;

    public TMP_Text healthText;
    public TMP_Text enemiesText;
    public TMP_Text messageText;

    public GameObject nextButton;

    public AudioClip winSFX;
    public AudioClip loseSFX;

    public bool isFinalLevel = false;

    AudioSource audioSource;

    private int enemiesRemaining;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        IsPlaying = true;

        enemiesRemaining =
            GameObject.FindGameObjectsWithTag("Enemy").Length;

        UpdateEnemyText();

        if (messageText)
        {
            messageText.enabled = false;
        }
    }

    void Update()
    {
        if (!IsPlaying)
            return;
    }

    public void SetHealthText(int currentHealth)
    {
        if (healthText)
        {
            healthText.text = "Health: " + currentHealth;
        }
    }

    void UpdateEnemyText()
    {
        if (enemiesText)
        {
            enemiesText.text =
                "Enemies: " + enemiesRemaining;
        }
    }

    public void EnemyKilled()
    {
        enemiesRemaining--;

        UpdateEnemyText();

        if (enemiesRemaining <= 0)
        {
            LevelBeat();
        }
    }

    public void LevelBeat()
    {
        IsPlaying = false;

        PlaySoundClip(winSFX);

        if (isFinalLevel)
        {
            DisplayGameMessage("GAME COMPLETED");

            if (nextButton)
            {
                nextButton
                    .GetComponentInChildren<TMP_Text>()
                    .text = "START OVER";
            }
        }
        else
        {
            DisplayGameMessage("LEVEL COMPLETE");
        }

        if (nextButton)
        {
            nextButton.SetActive(true);
        }
    }

    public void LevelLost()
    {
        IsPlaying = false;

        PlaySoundClip(loseSFX);

        DisplayGameMessage("GAME OVER");

        Invoke(nameof(ReloadSameScene), 2f);
    }

    void PlaySoundClip(AudioClip clip)
    {
        if (clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void DisplayGameMessage(string message)
    {
        if (messageText)
        {
            messageText.text = message;
            messageText.enabled = true;
        }
    }

    void ReloadSameScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadNextLevel()
    {
        if (isFinalLevel)
        {
            SceneManager.LoadScene("Level1");
            return;
        }

        if (nextLevel.Length > 0)
        {
            LoadSceneByName(nextLevel);
        }
        else
        {
            Debug.LogWarning(
                "No nextLevel specified."
            );
        }
    }
}