using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Level manager for Crypt Crawler.
// Win flow: kill every zombie -> the key spawns at the key spawn point ->
// grab the key -> touch the exit gate -> level complete.
// Lose flow: player dies -> "You Died" + Respawn button; the level only
// reloads when the button is pressed.
[RequireComponent(typeof(AudioSource))]
public class DungeonManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text goldText;
    public TMP_Text objectiveText;
    public TMP_Text messageText;
    public GameObject respawnButton;     // hidden until the player dies

    [Header("Key Spawn")]
    public GameObject keyPrefab;         // KeyPickup prefab
    public Transform keySpawnPoint;      // empty GameObject at room center

    [Header("Audio")]
    public AudioClip winSFX;
    public AudioClip loseSFX;

    [Header("Flow")]
    public string nextLevelName;
    public float winAdvanceDelay = 3f;
    public bool isLastLevel = false;

    public static bool IsPlaying { get; private set; }

    public bool HasKey { get; private set; }

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
        ended = false;
        gold = 0;

        zombiesRemaining = GameObject.FindGameObjectsWithTag("Enemy").Length;

        UpdateGoldText();
        SetObjective("Slay the undead. (" + zombiesRemaining + " remaining)");

        if (messageText != null) messageText.enabled = false;
        if (respawnButton != null) respawnButton.SetActive(false);
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldText();
    }

    // Called by ZombieBehavior when a zombie dies.
    public void ZombieKilled()
    {
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

    // Called by ResourcePickup when the key is collected.
    public void CollectKey()
    {
        HasKey = true;
        SetObjective("Reach the exit gate.");
        ShowMessage("Key picked up!", 2f);
    }

    // Called by ExitGate when the player touches it.
    public void TryExit()
    {
        if (!IsPlaying || ended) return;

        if (HasKey)
        {
            LevelBeat();
        }
        else
        {
            ShowMessage("The gate is locked.", 2f);
        }
    }

    public void LevelBeat()
    {
        if (ended) return;
        ended = true;
        IsPlaying = false;

        PlaySound(winSFX);
        ShowMessage(isLastLevel ? "The Crypt is Conquered!" : "Level Complete!");

        if (!string.IsNullOrEmpty(nextLevelName))
        {
            Invoke(nameof(LoadNextLevel), winAdvanceDelay);
        }
    }

    public void LevelLost()
    {
        if (ended) return;
        ended = true;
        IsPlaying = false;

        PlaySound(loseSFX);
        ShowMessage("You Died");

        if (respawnButton != null)
        {
            respawnButton.SetActive(true);
        }
    }

    // Hooked to the Respawn button's OnClick.
    public void RespawnPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelName);
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
        if (messageText == null) return;

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
}
