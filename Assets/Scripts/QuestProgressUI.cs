using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Optional UI component to display quest progress.
/// Shows which NPCs have been talked to and quest status.
/// </summary>
public class QuestProgressUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text questStatusText;
    [SerializeField] private GameObject questPanel;

    [Header("Display Settings")]
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.Q;

    private void Start()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(showOnStart);
        }
    }

    private void Update()
    {
        // Toggle quest display with Q key
        if (Input.GetKeyDown(toggleKey) && questPanel != null)
        {
            questPanel.SetActive(!questPanel.activeSelf);
        }

        // Update quest text
        UpdateQuestDisplay();
    }

    private void UpdateQuestDisplay()
    {
        if (questStatusText == null || QuestManager.Instance == null)
            return;

        string status = "=== DIE STILLE GLOCKE ===\n\n";

        // Check quest progress
        if (!QuestManager.Instance.talkedToPeter)
        {
            status += "❓ Sprich mit Peter am Haus\n";
        }
        else
        {
            status += "✓ Peter gesprochen\n";

            if (!QuestManager.Instance.talkedToBernd)
            {
                status += "→ Finde Bernd im Osten\n";
            }
            else
            {
                status += "✓ Bernd gesprochen (Hinweis erhalten)\n";

                if (!QuestManager.Instance.talkedToTom)
                {
                    status += "→ Finde Tom auf dem Weg\n";
                }
                else
                {
                    status += "✓ Tom gesprochen\n";

                    if (!QuestManager.Instance.talkedToMax)
                    {
                        status += "→ Gehe zur Burg und sprich mit Max\n";
                    }
                    else
                    {
                        status += "✓ Max gesprochen\n";

                        if (!QuestManager.Instance.bellRevealed)
                        {
                            status += "⚠️ Warte auf Glocken-Enthüllung...\n";
                        }
                        else if (!QuestManager.Instance.bellCollected)
                        {
                            status += "🔔 GLOCKE GEFUNDEN!\n";
                            status += "→ Sammle die Glocke ein!\n";
                        }
                        else
                        {
                            status += "🎉 QUEST ABGESCHLOSSEN! 🎉\n";
                            status += "Die Glocke ist zurück!\n";
                        }
                    }
                }
            }
        }

        status += "\n[Drücke Q zum Umschalten]";
        questStatusText.text = status;
    }
}
