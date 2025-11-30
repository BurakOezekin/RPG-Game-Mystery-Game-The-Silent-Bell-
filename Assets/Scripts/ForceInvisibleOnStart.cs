using UnityEngine;

/// <summary>
/// HELPER SCRIPT: Forces a GameObject to be invisible on start.
/// Use this as a BACKUP on the Bell GameObject if it's still visible.
/// This script runs before BellItem and ensures complete invisibility.
/// </summary>
[DefaultExecutionOrder(-100)] // Execute BEFORE other scripts
public class ForceInvisibleOnStart : MonoBehaviour
{
    [Header("Force Invisibility Settings")]
    [SerializeField] private bool disableChildrenToo = true;
    [SerializeField] private bool showDebugLog = true;

    private void Awake()
    {
        MakeInvisible();
    }

    private void Start()
    {
        // Double-check in Start as well
        MakeInvisible();
    }

    private void MakeInvisible()
    {
        if (showDebugLog)
        {
            Debug.Log($"[ForceInvisibleOnStart] Making {gameObject.name} invisible...");
        }

        // Disable all SpriteRenderers
        var spriteRenderers = disableChildrenToo 
            ? GetComponentsInChildren<SpriteRenderer>(true) 
            : GetComponents<SpriteRenderer>();
        
        foreach (var sr in spriteRenderers)
        {
            sr.enabled = false;
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
            
            if (showDebugLog)
            {
                Debug.Log($"  ✓ Disabled SpriteRenderer on {sr.gameObject.name}");
            }
        }

        // Disable all MeshRenderers
        var meshRenderers = disableChildrenToo 
            ? GetComponentsInChildren<MeshRenderer>(true) 
            : GetComponents<MeshRenderer>();
        
        foreach (var mr in meshRenderers)
        {
            mr.enabled = false;
            
            if (showDebugLog)
            {
                Debug.Log($"  ✓ Disabled MeshRenderer on {mr.gameObject.name}");
            }
        }

        // Disable all Colliders
        var colliders2D = disableChildrenToo 
            ? GetComponentsInChildren<Collider2D>(true) 
            : GetComponents<Collider2D>();
        
        foreach (var col in colliders2D)
        {
            col.enabled = false;
            
            if (showDebugLog)
            {
                Debug.Log($"  ✓ Disabled Collider2D on {col.gameObject.name}");
            }
        }

        // Disable 3D colliders too (just in case)
        var colliders3D = disableChildrenToo 
            ? GetComponentsInChildren<Collider>(true) 
            : GetComponents<Collider>();
        
        foreach (var col in colliders3D)
        {
            col.enabled = false;
            
            if (showDebugLog)
            {
                Debug.Log($"  ✓ Disabled Collider on {col.gameObject.name}");
            }
        }

        if (showDebugLog)
        {
            Debug.Log($"[ForceInvisibleOnStart] {gameObject.name} is now COMPLETELY INVISIBLE ✓");
        }
    }

    // Optional: Make it visible again (called by BellItem when revealing)
    public void MakeVisible()
    {
        if (showDebugLog)
        {
            Debug.Log($"[ForceInvisibleOnStart] Making {gameObject.name} visible...");
        }

        // Re-enable all SpriteRenderers
        var spriteRenderers = disableChildrenToo 
            ? GetComponentsInChildren<SpriteRenderer>(true) 
            : GetComponents<SpriteRenderer>();
        
        foreach (var sr in spriteRenderers)
        {
            sr.enabled = true;
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        // Re-enable all MeshRenderers
        var meshRenderers = disableChildrenToo 
            ? GetComponentsInChildren<MeshRenderer>(true) 
            : GetComponents<MeshRenderer>();
        
        foreach (var mr in meshRenderers)
        {
            mr.enabled = true;
        }

        // Re-enable all Colliders
        var colliders2D = disableChildrenToo 
            ? GetComponentsInChildren<Collider2D>(true) 
            : GetComponents<Collider2D>();
        
        foreach (var col in colliders2D)
        {
            col.enabled = true;
        }
    }
}
