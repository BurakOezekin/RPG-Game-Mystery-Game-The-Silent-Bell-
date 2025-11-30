using UnityEngine;

/// <summary>
/// PUT THIS ON ANY GAMEOBJECT (e.g. GameController or Camera).
/// It will automatically find and hide the bell on scene start.
/// </summary>
public class BellAutoHider : MonoBehaviour
{
    [Header("Automatic Bell Detection")]
    [Tooltip("Sucht automatisch nach GameObjects mit 'Bell' im Namen")]
    public bool autoFindBell = true;
    
    [Tooltip("Oder: Ziehe das Bell GameObject hier rein")]
    public GameObject bellObject;

    private void Awake()
    {
        if (autoFindBell)
        {
            FindAndHideBell();
        }
        else if (bellObject != null)
        {
            HideBell(bellObject);
        }
    }

    private void FindAndHideBell()
    {
        // Method 1: Find by name
        GameObject bell = GameObject.Find("BellItem");
        if (bell == null) bell = GameObject.Find("Bell");
        if (bell == null) bell = GameObject.Find("bell");
        
        // Method 2: Find all objects and search
        if (bell == null)
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("bell"))
                {
                    bell = obj;
                    break;
                }
            }
        }

        if (bell != null)
        {
            HideBell(bell);
        }
        else
        {
            Debug.LogWarning("[BellAutoHider] ⚠️ Keine Glocke gefunden! Stelle sicher, dass das GameObject 'Bell' im Namen hat.");
        }
    }

    private void HideBell(GameObject bell)
    {
        Debug.Log($"[BellAutoHider] Verstecke: {bell.name}");

        // OPTION 1: Deactivate entire GameObject (simplest!)
        bell.SetActive(false);
        Debug.Log($"🔒 {bell.name} wurde DEAKTIVIERT (komplett unsichtbar)");

        // Also mark it in QuestManager
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.bellRevealed = false;
        }
    }
}
