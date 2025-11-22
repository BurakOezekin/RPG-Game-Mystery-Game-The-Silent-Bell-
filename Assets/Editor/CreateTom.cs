using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Helper functions for tom creation

public static class CreateTom
{
    [MenuItem("Tools/Create Tom (From Player)")]
    public static void CreateTomFromPlayer()
    {
        try
        {
            // Try to find a GameObject with PlayerController
            var players = UnityEngine.Object.FindObjectsOfType<PlayerController>();
            GameObject srcGO = null;
            if (players != null && players.Length > 0)
            {
                srcGO = players[0].gameObject;
            }
            else
            {
                // fallback: try to find by name 'Player'
                srcGO = GameObject.Find("Player");
            }

            if (srcGO == null)
            {
                Debug.LogError("CreateTom: No GameObject with PlayerController (or named 'Player') found in scene.");
                return;
            }

            // Duplicate the object to preserve children/graphics/colliders
            var tomGO = UnityEngine.Object.Instantiate(srcGO, srcGO.transform.parent);
            tomGO.name = "Tom";

            // Set start position as requested
            tomGO.transform.position = new Vector3(-13f, 8f, 0f);
            tomGO.transform.rotation = srcGO.transform.rotation;
            tomGO.transform.localScale = srcGO.transform.localScale;

            // Copy layer and tag from source so collision masks behave the same.
            // Prefer the Player's configured `interactableLayer` (first bit) if available.
            try {
                tomGO.tag = srcGO.tag;
                var srcPlayer = srcGO.GetComponent<PlayerController>();
                int layerToUse = srcGO.layer;
                if (srcPlayer != null)
                {
                    int mask = srcPlayer.interactableLayer.value;
                    int first = GetFirstLayerFromMask(mask);
                    if (first >= 0) layerToUse = first;
                }
                tomGO.layer = layerToUse;
                // also set layer recursively on children so colliders on child objects are on same layer
                SetLayerRecursively(tomGO, layerToUse);
            } catch { }

            UnityEditor.Undo.RegisterCreatedObjectUndo(tomGO, "Create Tom");

            // Remove any existing AI components that could conflict (Max/Tom/Bernd style)
            var bernd = tomGO.GetComponent<BerndAIController>(); if (bernd != null) UnityEditor.Undo.DestroyObjectImmediate(bernd);
            var maxAI = tomGO.GetComponent<MaxAIController>(); if (maxAI != null) UnityEditor.Undo.DestroyObjectImmediate(maxAI);

            // Ensure Animator exists on Tom
            var srcAnimator = srcGO.GetComponent<Animator>();
            var dstAnimator = tomGO.GetComponent<Animator>();
            if (dstAnimator == null) dstAnimator = tomGO.AddComponent<Animator>();

            if (srcAnimator != null && srcAnimator.runtimeAnimatorController != null)
            {
                dstAnimator.runtimeAnimatorController = srcAnimator.runtimeAnimatorController;
                Debug.Log("CreateTom: Assigned Player's RuntimeAnimatorController to Tom.");
            }
            else
            {
                Debug.LogWarning("CreateTom: Player has no RuntimeAnimatorController assigned. You may need to assign an Animator Controller to Tom manually.");
            }

            // Add TomAIController (or replace if exists)
            var tomAI = tomGO.GetComponent<TomAIController>();
            if (tomAI == null) tomAI = tomGO.AddComponent<TomAIController>();

            // Configure the waypoints on the TomAIController via SerializedObject so it'll serialize into the prefab
            var so = new SerializedObject(tomAI);
            var wayProp = so.FindProperty("waypoints");
            if (wayProp != null)
            {
                wayProp.arraySize = 3;
                wayProp.GetArrayElementAtIndex(0).vector3Value = new Vector3(-13f, 8f, 0f);
                wayProp.GetArrayElementAtIndex(1).vector3Value = new Vector3(-3f, 8f, 0f);
                wayProp.GetArrayElementAtIndex(2).vector3Value = new Vector3(-3f, 17f, 0f);
            }

            // Movement settings: reasonable defaults
            var speedProp = so.FindProperty("speed"); if (speedProp != null) speedProp.floatValue = 2f;
            var waitProp = so.FindProperty("waitTime"); if (waitProp != null) waitProp.floatValue = 1f;
            var startProp = so.FindProperty("startOnAwake"); if (startProp != null) startProp.boolValue = true;

            so.ApplyModifiedProperties();

            // If the scene object has no collider but has a SpriteRenderer, try to size a BoxCollider2D on the Sprite's GameObject
            var existingCol = tomGO.GetComponentInChildren<Collider2D>();
            if (existingCol == null)
            {
                var spriteR = tomGO.GetComponentInChildren<SpriteRenderer>();
                if (spriteR != null)
                {
                    var targetGO = spriteR.gameObject;
                    var box = Undo.AddComponent<BoxCollider2D>(targetGO);
                    box.isTrigger = false;
                    // Attempt to size collider to sprite bounds (local sprite bounds)
                    try
                    {
                        if (spriteR.sprite != null)
                        {
                            var spriteBounds = spriteR.sprite.bounds;
                            // size in local space of the sprite object
                            Vector2 size = spriteBounds.size;
                            // account for the sprite transform scale
                            Vector3 ls = spriteR.transform.lossyScale;
                            size = Vector2.Scale(size, new Vector2(ls.x, ls.y));
                            box.size = size;
                            box.offset = spriteBounds.center;
                        }
                    }
                    catch { }
                }
            }

            // Create Prefab folder if needed
            string prefabFolder = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            string prefabPath = prefabFolder + "/Tom.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(tomGO, prefabPath, InteractionMode.UserAction);
            if (prefab != null)
            {
                Debug.Log("CreateTom: Prefab created at " + prefabPath);
            }
            else
            {
                Debug.LogError("CreateTom: Failed to create prefab at " + prefabPath);
            }

            Selection.activeGameObject = tomGO;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        catch (Exception ex)
        {
            Debug.LogError("CreateTom failed: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    [MenuItem("Tools/Ensure Tom Physics")]
    public static void EnsureTomPhysics()
    {
        try
        {
            var tomGO = GameObject.Find("Tom");
            if (tomGO == null)
            {
                Debug.LogError("Ensure Tom Physics: No GameObject named 'Tom' found in the scene.");
                return;
            }

            // Try to match Tom's layer to the Player's layer so Player's layer masks detect Tom
            var players = UnityEngine.Object.FindObjectsOfType<PlayerController>();
            if (players != null && players.Length > 0)
            {
                var src = players[0].gameObject;
                try { SetLayerRecursively(tomGO, src.layer); tomGO.tag = src.tag; } catch { }
            }

            // Add a BoxCollider2D if missing (so players can't walk through Tom)
            var col = tomGO.GetComponent<Collider2D>();
            if (col == null)
            {
                var box = Undo.AddComponent<BoxCollider2D>(tomGO);
                box.isTrigger = false;
                Debug.Log("Ensure Tom Physics: Added BoxCollider2D to Tom.");
            }

            // If a prefab exists, update it too (ensure BoxCollider2D present)
            string prefabPath = "Assets/Prefabs/Tom.prefab";
            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset != null)
            {
                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                bool changed = false;
                var pCol = root.GetComponentInChildren<Collider2D>();
                if (pCol == null)
                {
                    var spriteR = root.GetComponentInChildren<SpriteRenderer>();
                    if (spriteR != null)
                    {
                        var box = spriteR.gameObject.AddComponent<BoxCollider2D>();
                        box.isTrigger = false;
                        try
                        {
                            if (spriteR.sprite != null)
                            {
                                var spriteBounds = spriteR.sprite.bounds;
                                Vector2 size = spriteBounds.size;
                                Vector3 ls = spriteR.transform.lossyScale;
                                size = Vector2.Scale(size, new Vector2(ls.x, ls.y));
                                box.size = size;
                                box.offset = spriteBounds.center;
                            }
                        }
                        catch { }
                        changed = true;
                    }
                    else
                    {
                        // fallback: add box on root
                        var box2 = root.AddComponent<BoxCollider2D>();
                        box2.isTrigger = false;
                        changed = true;
                    }
                }
                // Also match prefab layers to Player's interactable layer (if present)
                if (players != null && players.Length > 0)
                {
                    var src = players[0].gameObject;
                    try {
                        var srcPlayer = players[0];
                        int mask = srcPlayer.interactableLayer.value;
                        int first = GetFirstLayerFromMask(mask);
                        int layerToUse = first >= 0 ? first : src.layer;
                        SetLayerRecursively(root, layerToUse);
                        root.tag = src.tag;
                        changed = true;
                    } catch { }
                }
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                    Debug.Log("Ensure Tom Physics: Updated prefab at " + prefabPath);
                }
                PrefabUtility.UnloadPrefabContents(root);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        catch (Exception ex)
        {
            Debug.LogError("EnsureTomPhysics failed: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    static void SetLayerRecursively(GameObject go, int layer)
    {
        if (go == null) return;
        try
        {
            go.layer = layer;
        }
        catch { }
        foreach (Transform t in go.transform)
        {
            SetLayerRecursively(t.gameObject, layer);
        }
    }

    // return first layer index from a LayerMask int value, or -1 if none
    static int GetFirstLayerFromMask(int mask)
    {
        if (mask == 0) return -1;
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0) return i;
        }
        return -1;
    }
}
