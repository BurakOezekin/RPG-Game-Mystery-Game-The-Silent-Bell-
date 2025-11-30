using UnityEngine;
using UnityEditor;

/// <summary>
/// Unity Editor script to automatically hide the Bell in the Scene view.
/// This runs in the Editor, not just at runtime.
/// </summary>
[InitializeOnLoad]
public static class BellSetupEditor
{
    static BellSetupEditor()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        OnHierarchyChanged(); // Run immediately
    }

    private static void OnHierarchyChanged()
    {
        // Find all BellItem components in the scene
        BellItem[] bells = Object.FindObjectsOfType<BellItem>();
        
        foreach (BellItem bell in bells)
        {
            if (bell != null)
            {
                HideBellInEditor(bell.gameObject);
            }
        }
    }

    private static void HideBellInEditor(GameObject bellObject)
    {
        if (bellObject == null) return;

        bool madeChanges = false;

        // Disable all SpriteRenderers
        SpriteRenderer[] renderers = bellObject.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            if (sr.enabled)
            {
                sr.enabled = false;
                Color c = sr.color;
                c.a = 0f;
                sr.color = c;
                madeChanges = true;
            }
        }

        // Disable all Colliders
        Collider2D[] colliders = bellObject.GetComponentsInChildren<Collider2D>(true);
        foreach (var col in colliders)
        {
            if (col.enabled)
            {
                col.enabled = false;
                madeChanges = true;
            }
        }

        if (madeChanges)
        {
            EditorUtility.SetDirty(bellObject);
            Debug.Log($"[Editor] Made {bellObject.name} invisible in the Editor!");
        }
    }
}

/// <summary>
/// Custom Inspector button to manually hide the bell
/// </summary>
[CustomEditor(typeof(BellItem))]
public class BellItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BellItem bell = (BellItem)target;

        GUILayout.Space(10);
        
        if (GUILayout.Button("🔒 HIDE BELL NOW (Editor)", GUILayout.Height(30)))
        {
            HideBell(bell.gameObject);
        }

        if (GUILayout.Button("👁 SHOW BELL (For Testing)", GUILayout.Height(30)))
        {
            ShowBell(bell.gameObject);
        }
    }

    private void HideBell(GameObject bellObject)
    {
        // Disable all renderers
        SpriteRenderer[] renderers = bellObject.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            sr.enabled = false;
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        // Disable all colliders
        Collider2D[] colliders = bellObject.GetComponentsInChildren<Collider2D>(true);
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        EditorUtility.SetDirty(bellObject);
        Debug.Log("🔒 Bell is now HIDDEN in the Editor!");
    }

    private void ShowBell(GameObject bellObject)
    {
        // Enable all renderers
        SpriteRenderer[] renderers = bellObject.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            sr.enabled = true;
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        // Enable all colliders
        Collider2D[] colliders = bellObject.GetComponentsInChildren<Collider2D>(true);
        foreach (var col in colliders)
        {
            col.enabled = true;
        }

        EditorUtility.SetDirty(bellObject);
        Debug.Log("👁 Bell is now VISIBLE in the Editor!");
    }
}
