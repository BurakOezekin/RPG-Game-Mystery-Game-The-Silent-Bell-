using UnityEngine;

public class BellDrop : MonoBehaviour, Interactable
{
    [SerializeField] GameObject bellPrefab;  // Prefab der Glocke
    [SerializeField] Transform dropPoint;    // Zielposition (wo sie erscheinen soll)

    private bool dropped = false;

    public void Interact()
    {
        if (dropped) return;

        // Prüfen, ob der Spieler die Glocke im Inventar hat
        if (PlayerInventory.Instance.HasItem("Bell"))
        {
            dropped = true;

            // (Optional) Entferne sie aus dem Inventar
            // PlayerInventory.Instance.RemoveItem("Bell");

            // Glocke an Zielposition spawnen
            Instantiate(bellPrefab, dropPoint.position, Quaternion.identity);

            Debug.Log("🔔 Glocke wurde am Turm platziert!");
        }
        else
        {
            Debug.Log("⚠️ Du hast keine Glocke im Inventar!");
        }
    }
}
