using UnityEngine;
using UnityEditor;

/// <summary>
/// Unity Menu items to quickly hide/show the bell in the scene
/// </summary>
public class BellEditorMenu
{
    [MenuItem("Tools/Bell Quest/Hide Bell in Scene 🔒")]
    public static void HideAllBells()
    {
        BellItem[] bells = Object.FindObjectsOfType<BellItem>();
        
        if (bells.Length == 0)
        {
            Debug.LogWarning("⚠️ No Bell objects found in scene! Make sure BellItem.cs is attached to the bell GameObject.");
            return;
        }

        foreach (BellItem bell in bells)
        {
            GameObject bellObj = bell.gameObject;
            
            // Disable all renderers
            SpriteRenderer[] renderers = bellObj.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in renderers)
            {
                sr.enabled = false;
                Color c = sr.color;
                c.a = 0f;
                sr.color = c;
            }

            // Disable all colliders
            Collider2D[] colliders = bellObj.GetComponentsInChildren<Collider2D>(true);
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            EditorUtility.SetDirty(bellObj);
            Debug.Log($"🔒 {bellObj.name} is now HIDDEN!");
        }

        Debug.Log($"✅ Hidden {bells.Length} bell(s) in the scene!");
    }

    [MenuItem("Tools/Bell Quest/Show Bell in Scene 👁 (For Testing)")]
    public static void ShowAllBells()
    {
        BellItem[] bells = Object.FindObjectsOfType<BellItem>();
        
        if (bells.Length == 0)
        {
            Debug.LogWarning("⚠️ No Bell objects found in scene!");
            return;
        }

        foreach (BellItem bell in bells)
        {
            GameObject bellObj = bell.gameObject;
            
            // Enable all renderers
            SpriteRenderer[] renderers = bellObj.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in renderers)
            {
                sr.enabled = true;
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }

            // Enable all colliders
            Collider2D[] colliders = bellObj.GetComponentsInChildren<Collider2D>(true);
            foreach (var col in colliders)
            {
                col.enabled = true;
            }

            EditorUtility.SetDirty(bellObj);
            Debug.Log($"👁 {bellObj.name} is now VISIBLE!");
        }

        Debug.Log($"✅ Showed {bells.Length} bell(s) in the scene!");
    }

    [MenuItem("Tools/Bell Quest/Find Bell Objects 🔍")]
    public static void FindBells()
    {
        BellItem[] bells = Object.FindObjectsOfType<BellItem>();
        
        if (bells.Length == 0)
        {
            Debug.LogWarning("⚠️ No Bell objects found in scene! Add BellItem.cs component to your bell GameObject.");
            return;
        }

        Debug.Log($"🔍 Found {bells.Length} bell object(s):");
        foreach (BellItem bell in bells)
        {
            Debug.Log($"  • {bell.gameObject.name} at position {bell.transform.position}");
            Selection.activeGameObject = bell.gameObject;
        }
    }
}
