using UnityEngine;

/// <summary>
/// Manages the quest state for "The Silent Bell" quest.
/// Tracks NPC interactions and bell collection progress.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    // Quest progress flags
    public bool talkedToPeter = false;
    public bool talkedToBernd = false;
    public bool talkedToTom = false;
    public bool talkedToMax = false;
    public bool bellRevealed = false;
    public bool bellCollected = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MarkPeterTalked()
    {
        talkedToPeter = true;
        Debug.Log("[QuestManager] Quest started: Player talked to Peter");
    }

    public void MarkBerndTalked()
    {
        talkedToBernd = true;
        Debug.Log("[QuestManager] Player talked to Bernd (false hint given)");
    }

    public void MarkTomTalked()
    {
        talkedToTom = true;
        Debug.Log("[QuestManager] Player talked to Tom");
    }

    public void MarkMaxTalked()
    {
        talkedToMax = true;
        Debug.Log("[QuestManager] Player talked to Max");
    }

    public void RevealBell()
    {
        bellRevealed = true;
        Debug.Log("[QuestManager] Bell has been revealed!");
    }

    public void CollectBell()
    {
        bellCollected = true;
        Debug.Log("[QuestManager] Bell collected! Quest complete!");
    }
}
