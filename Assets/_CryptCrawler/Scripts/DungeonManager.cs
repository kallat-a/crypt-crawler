using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class DungeonManager : MonoBehaviour
{
    public enum WinCondition { ClearZombiesThenExit, DefeatBoss }

    [Header("Mode")]
    public WinCondition winCondition = WinCondition.ClearZombiesThenExit;

    [Header("UI")]
    public TMP_Text goldText;
    public TMP_Text objectiveText;
    public TMP_Text messageText;
    public TMP_Text levelText;
    public TMP_Text timerText;
    public GameObject deathPanel;
    public GameObject winPanel;
    public GameObject keyIcon;

    [Header("Level Info")]
    public string levelDisplayName = "Level 1";

    [Header("Key Spawn (Level 1 mode)")]
    public GameObject keyPrefab;
    public Transform keySpawnPoint;

    [Header("Boss mode")]
    public GameObject bossKeyPrefab;

    [Header("Audio")]
    public AudioClip winSFX;
    public AudioClip loseSFX;

    [Header("Flow")]
    public string nextLevelName;
    public string menuSceneName = "MainMenu";
    public bool isLastLevel = false;
    public float winAdvanceDelay = 3f;

    public static bool IsPlaying { get; private set; }

    public bool HasKey { get; private set; }
    public bool BossDead { get; private set; }

    private Vector3 bossDeathPosition;

    private int gold = 0;
    private int zombiesRemaining = 0;
    private bool ended = false;
    private AudioSource audioSource;

    private float sessionTime = 0f;
    private int sessionEnemiesKilled = 0;
    private int sessionBossesKilled = 0;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (IsPlaying)
        {
            sessionTime += Time.deltaTime;
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }
        int totalSeconds = (int)sessionTime;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = minutes + ":" + seconds.ToString("D2");
    }

    void Start()
    {
        Time.timeScale = 1f;
        IsPlaying = true;
        HasKey = false;
        BossDead = false;
        ended = false;
        gold = 0;

        // when the scene is loaded, the volume is reset so we set it to the user's preferred volume
        AudioListener.volume = PlayerPrefs.GetInt("Volume", 100) / 100f;

        if (deathPanel == null)
        {
            deathPanel = FindSceneObject("DeathPanel");
        }
        SetupDeathButtons();
        SetupWinButtons();

        if (levelText != null)
        {
            levelText.text = levelDisplayName;
        }

        UpdateGoldText();

        if (winCondition == WinCondition.ClearZombiesThenExit)
        {
            zombiesRemaining = GameObject.FindGameObjectsWithTag("Enemy").Length;
            SetObjective("Slay the undead. (" + zombiesRemaining + " remaining)");
        }
        else
        {
            SetObjective("Defeat the guardian of the Doors of Heaven.");
        }

        if (messageText != null)
        {
            messageText.enabled = false;
        }
        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }
        SetDeathUi(false);
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldText();
    }


    public void ZombieKilled()
    {
        if (winCondition != WinCondition.ClearZombiesThenExit)
        {
            return;
        }

        zombiesRemaining--;
        sessionEnemiesKilled++;
        if (zombiesRemaining > 0)
        {
            SetObjective("Slay the undead. (" + zombiesRemaining + " remaining)");
        }
        else if (IsPlaying)
        {
            SpawnKey();
        }
    }

    void SpawnKey()
    {
        if (keyPrefab != null && keySpawnPoint != null)
        {
            Instantiate(keyPrefab, keySpawnPoint.position, Quaternion.identity);
        }
        SetObjective("Take the key. Reach the exit gate.");
        ShowMessage("The crypt is cleared. A key has appeared!", 3f);
    }

    public void CollectKey()
    {
        HasKey = true;
        SetObjective("Reach the exit gate.");
        ShowMessage("Key picked up!", 2f);
        if (keyIcon != null)
        {
            keyIcon.SetActive(true);
        }
    }


    public void BossDefeated(Vector3 deathPosition)
    {
        if (winCondition != WinCondition.DefeatBoss)
        {
            return;
        }

        BossDead = true;
        bossDeathPosition = deathPosition;
        sessionEnemiesKilled++;
        sessionBossesKilled++;

        GameObject keyToDrop = keyPrefab;
        if (bossKeyPrefab != null)
        {
            keyToDrop = bossKeyPrefab;
        }
        if (keyToDrop != null)
        {
            Instantiate(keyToDrop, deathPosition + Vector3.up * 0.5f, Quaternion.identity);
        }

        ShowMessage("The guardian falls! Take the key and escape.", 4f);
        SetObjective("Take the key. Reach the exit gate.");
    }


    public void TryExit()
    {
        if (!IsPlaying || ended)
        {
            return;
        }

        if (winCondition == WinCondition.ClearZombiesThenExit)
        {
            if (HasKey)
            {
                LevelBeat();
            }
            else
            {
                ShowMessage("The gate is locked.", 2f);
            }
        }
        else
        {
            if (BossDead && HasKey)
            {
                LevelBeat();
            }
            else if (!BossDead)
            {
                ShowMessage("The guardian still lives.", 2f);
            }
            else
            {
                ShowMessage("The gate is locked. Take the key.", 2f);
            }
        }
    }

    public void LevelBeat()
    {
        if (ended)
        {
            return;
        }
        ended = true;
        IsPlaying = false;

        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        if (buildIndex > PlayerPrefs.GetInt("HighestLevel", 0))
        {
            PlayerPrefs.SetInt("HighestLevel", buildIndex);
        }
        SaveStats();

        PlaySound(winSFX);

        if (isLastLevel)
        {
            HideMessage();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
        }
        else
        {
            ShowMessage("Level Complete!");
            if (!string.IsNullOrEmpty(nextLevelName))
            {
                Invoke(nameof(LoadNextLevel), winAdvanceDelay);
            }
        }
    }

    public void LevelLost()
    {
        if (ended)
        {
            return;
        }
        ended = true;
        IsPlaying = false;

        SaveStats();
        PlaySound(loseSFX);
        HideMessage();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetDeathUi(true);
    }


    public void RestartPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenuPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void PlayAgainPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelName);
    }


    void SaveStats()
    {
        float total = PlayerPrefs.GetFloat("TimePlayed", 0f) + sessionTime;
        PlayerPrefs.SetFloat("TimePlayed", total);

        int enemies = PlayerPrefs.GetInt("EnemiesKilled", 0) + sessionEnemiesKilled;
        PlayerPrefs.SetInt("EnemiesKilled", enemies);

        int bosses = PlayerPrefs.GetInt("BossesKilled", 0) + sessionBossesKilled;
        PlayerPrefs.SetInt("BossesKilled", bosses);

        PlayerPrefs.Save();
    }


    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void SetObjective(string text)
    {
        if (objectiveText != null)
        {
            objectiveText.text = text;
        }
    }

    void ShowMessage(string text, float hideAfter = 0f)
    {
        if (messageText == null)
        {
            return;
        }
        messageText.text = text;
        messageText.enabled = true;

        CancelInvoke(nameof(HideMessage));
        if (hideAfter > 0f)
        {
            Invoke(nameof(HideMessage), hideAfter);
        }
    }

    void HideMessage()
    {
        if (messageText != null)
        {
            messageText.enabled = false;
        }
    }

    void UpdateGoldText()
    {
        if (goldText != null)
        {
            goldText.text = gold.ToString();
        }
    }

    void SetDeathUi(bool active)
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(active);
        }
    }

    void SetupDeathButtons()
    {
        if (deathPanel == null)
        {
            return;
        }

        foreach (UnityEngine.UI.Button button in deathPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            if (button.name == "RestartButton")
            {
                button.onClick.RemoveListener(RestartPressed);
                button.onClick.AddListener(RestartPressed);
            }
            else if (button.name == "MainMenuButton")
            {
                button.onClick.RemoveListener(MainMenuPressed);
                button.onClick.AddListener(MainMenuPressed);
            }
        }
    }

    void SetupWinButtons()
    {
        if (winPanel == null)
        {
            return;
        }

        foreach (UnityEngine.UI.Button button in winPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            if (button.name == "PlayAgainButton")
            {
                button.onClick.RemoveListener(PlayAgainPressed);
                button.onClick.AddListener(PlayAgainPressed);
            }
        }
    }

    GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            GameObject match = FindInChildren(root.transform, objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    GameObject FindInChildren(Transform parent, string objectName)
    {
        if (parent.name == objectName)
        {
            return parent.gameObject;
        }

        foreach (Transform child in parent)
        {
            GameObject match = FindInChildren(child, objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
