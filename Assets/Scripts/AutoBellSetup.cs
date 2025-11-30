using UnityEngine;

/// <summary>
/// Automatic setup script that finds and fixes the Bell object on first run.
/// This will automatically replace ItemPickup with BellItem and hide the bell.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class AutoBellSetup : MonoBehaviour
{
    private void Awake()
    {
        SetupBellAutomatically();
    }

    private void SetupBellAutomatically()
    {
        // Find all objects with "Bell" in the name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("bell"))
            {
                Debug.Log($"[AutoBellSetup] Found potential bell object: {obj.name}");
                
                // Check if it has ItemPickup instead of BellItem
                ItemPickup oldPickup = obj.GetComponent<ItemPickup>();
                BellItem bellItem = obj.GetComponent<BellItem>();
                
                if (oldPickup != null && bellItem == null)
                {
                    Debug.Log($"[AutoBellSetup] Converting {obj.name} from ItemPickup to BellItem...");
                    
                    // Destroy old component
                    Destroy(oldPickup);
                    
                    // Add new component
                    bellItem = obj.AddComponent<BellItem>();
                    
                    Debug.Log($"✅ {obj.name} now has BellItem component!");
                }
                
                // Make sure the bell is deactivated
                if (obj.activeSelf)
                {
                    obj.SetActive(false);
                    Debug.Log($"🔒 {obj.name} has been DEACTIVATED (hidden)");
                }
                
                // Also disable renderer as backup
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = false;
                    Color c = sr.color;
                    c.a = 0f;
                    sr.color = c;
                }
                
                // Disable collider
                Collider2D col = obj.GetComponent<Collider2D>();
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }
        
        Debug.Log("[AutoBellSetup] ✅ Bell setup complete!");
    }
}
