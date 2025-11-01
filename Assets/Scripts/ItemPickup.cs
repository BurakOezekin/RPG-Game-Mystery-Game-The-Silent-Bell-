using UnityEngine;

public class ItemPickup : MonoBehaviour, Interactable
{
    [SerializeField] string itemName = "Bell";
    private bool picked;

    public void Interact()
    {
        if (picked) return;
        picked = true;

        PlayerInventory.Instance.AddItem(itemName);

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // Item verschwindet (weil es jetzt im Inventar ist)
        gameObject.SetActive(false);

        Debug.Log(itemName + " ins Inventar gelegt!");
    }
}
