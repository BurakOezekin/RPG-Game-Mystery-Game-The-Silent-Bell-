using UnityEngine;

/// <summary>
/// SUPER SIMPLE: Deactivates this GameObject on start.
/// Use this on the Bell if nothing else works!
/// </summary>
public class HideOnStart : MonoBehaviour
{
    private void Awake()
    {
        // Deactivate this GameObject immediately
        gameObject.SetActive(false);
        Debug.Log($"🔒 {gameObject.name} was DEACTIVATED by HideOnStart");
    }
}
