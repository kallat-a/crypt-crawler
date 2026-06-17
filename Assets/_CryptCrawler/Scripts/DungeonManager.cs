using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Level manager for Crypt Crawler.
//
// Win conditions (set per level via winCondition):
//  - ClearZombiesThenExit (Level 1): kill all zombies -> key spawns -> grab
//    key -> reach exit gate -> level complete.
//  - DefeatBoss (Level 2): kill the boss -> the key drops at the boss's
//    location and the exit gate unlocks -> grab key -> reach exit -> you win.
//
// Lose flow: player dies -> "You Died" with a Restart button and a Main Menu
// button (the level only reloads when Restart is pressed).
// Final win: shows a win panel with a Play Again button (-> main menu).
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
    public GameObject deathPanel;        // holds Restart + Main Menu buttons
    public GameObject winPanel;          // holds Play Again button (final level)

    [Header("Key Spawn (Level 1 mode)")]
    public GameObject keyPrefab;
    public Transform keySpawnPoint;

    [Header("Boss mode")]
    public GameObject bossKeyPrefab;     // key dropped when the boss dies (can reuse KeyPickup)

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

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        IsPlaying = true;
        HasKey = false;
        BossDead = false;
        ended = false;
        gold = 0;

        UpdateGoldText();

        if (winCondition == WinCondition.ClearZombiesThenExit)
        {
            zombiesRemaining = GameObject.FindGameObjectsWithTag("Enemy").Length;
            SetObjective("Slay the undead. (" + zombiesRemaining + " remaining)");
        }
        else // DefeatBoss
        {
            SetObjective("Defeat the guardian of the Doors of Heaven.");
        }

        if (messageText != null) messageText.enabled = false;
        if (deathPanel != null) deathPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldText();
    }

    // ---------- Level 1 (clear zombies) path ----------

    // Called by ZombieBehavior when a NON-summoned zombie dies. Summoned boss
    // zombies must NOT call this (they'd never let the count reach zero / are
    // irrelevant in boss mode).
    public void ZombieKilled()
    {
        if (winCondition != WinCondition.ClearZombiesThenExit) return;

        zombiesRemaining--;
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
    }

    // ---------- Level 2 (defeat boss) path ----------

    // Called by BossBehavior on death, passing where the boss fell.
    public void BossDefeated(Vector3 deathPosition)
    {
        if (winCondition != WinCondition.DefeatBoss) return;

        BossDead = true;
        bossDeathPosition = deathPosition;

        // Drop the key where the boss fell; the gate is now unlocked.
        GameObject keyToDrop = bossKeyPrefab != null ? bossKeyPrefab : keyPrefab;
        if (keyToDrop != null)
        {
            Instantiate(keyToDrop, deathPosition + Vector3.up * 0.5f, Quaternion.identity);
        }

        ShowMessage("The guardian falls! Take the key and escape.", 4f);
        SetObjective("Take the key. Reach the exit gate.");
    }

    // ---------- Shared exit ----------

    // Called by ExitGate when the player touches it.
    public void TryExit()
    {
        if (!IsPlaying || ended) return;

        if (winCondition == WinCondition.ClearZombiesThenExit)
        {
            if (HasKey) LevelBeat();
            else ShowMessage("The gate is locked.", 2f);
        }
        else // DefeatBoss: gate unlocks once the boss is dead AND key collected
        {
            if (BossDead && HasKey) LevelBeat();
            else if (!BossDead) ShowMessage("The guardian still lives.", 2f);
            else ShowMessage("The gate is locked. Take the key.", 2f);
        }
    }

    public void LevelBeat()
    {
        if (ended) return;
        ended = true;
        IsPlaying = false;

        PlaySound(winSFX);

        if (isLastLevel)
        {
            ShowMessage("You Win!");
            if (winPanel != null) winPanel.SetActive(true);
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
        if (ended) return;
        ended = true;
        IsPlaying = false;

        PlaySound(loseSFX);
        ShowMessage("You Died");

        if (deathPanel != null) deathPanel.SetActive(true);
    }

    // ---------- Button hooks ----------

    public void RestartPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenuPressed()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public void PlayAgainPressed()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelName);
    }

    // ---------- UI helpers ----------

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
        if (objectiveText != null) objectiveText.text = text;
    }

    void ShowMessage(string text, float hideAfter = 0f)
    {
        if (messageText == null) return;
        messageText.text = text;
        messageText.enabled = true;

        CancelInvoke(nameof(HideMessage));
        if (hideAfter > 0f) Invoke(nameof(HideMessage), hideAfter);
    }

    void HideMessage()
    {
        if (messageText != null) messageText.enabled = false;
    }

    void UpdateGoldText()
    {
        if (goldText != null) goldText.text = gold.ToString();
    }
}
