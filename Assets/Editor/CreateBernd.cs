using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public static class CreateBernd
{
    [MenuItem("Tools/Create Bernd (From Max)")]
    public static void CreateBerndFromMax()
    {
        try
        {
            var maxGO = GameObject.Find("Max");
            if (maxGO == null)
            {
                Debug.LogError("CreateBernd: GameObject 'Max' not found in scene.");
                return;
            }

            // Duplicate the entire Max GameObject so sprites/colliders/rigidbody/etc. are preserved
            var berndGO = UnityEngine.Object.Instantiate(maxGO, maxGO.transform.parent);
            berndGO.name = "Bernd";
            // Keep same transform values
            berndGO.transform.position = maxGO.transform.position;
            berndGO.transform.rotation = maxGO.transform.rotation;
            berndGO.transform.localScale = maxGO.transform.localScale;

            // Register created object for undo in editor
            UnityEditor.Undo.RegisterCreatedObjectUndo(berndGO, "Create Bernd");

            // Remove Max-specific AI component(s) from the duplicate
            var maxAI_dup = berndGO.GetComponent<MaxAIController>();
            var maxCtrl_dup = berndGO.GetComponent<MaxController>();
            if (maxAI_dup != null) UnityEditor.Undo.DestroyObjectImmediate(maxAI_dup);
            if (maxCtrl_dup != null) UnityEditor.Undo.DestroyObjectImmediate(maxCtrl_dup);

            // Add BerndAIController
            var berndComp = berndGO.AddComponent<BerndAIController>();

            // Copy serialized fields from the original Max component (if present) into BerndAIController
            var srcComp = maxGO.GetComponent<MaxAIController>() as Component ?? maxGO.GetComponent<MaxController>() as Component;
            if (srcComp != null)
            {
                var srcSO = new SerializedObject(srcComp);
                var dstSO = new SerializedObject(berndComp);
                CopySerializedProperties(srcSO, dstSO);
                dstSO.ApplyModifiedProperties();
            }

            // Preserve tag/layer
            berndGO.tag = maxGO.tag;
            berndGO.layer = maxGO.layer;

            Selection.activeGameObject = berndGO;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Bernd created by duplicating Max; components and inspector values copied.");
        }
        catch (Exception ex)
        {
            Debug.LogError("CreateBernd failed: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    static void CopySerializedProperties(SerializedObject src, SerializedObject dst)
    {
        var it = src.GetIterator();
        bool enter = it.NextVisible(true);
        while (enter)
        {
            if (it.name != "m_Script")
            {
                var dstProp = dst.FindProperty(it.name);
                if (dstProp != null)
                {
                    try
                    {
                        switch (it.propertyType)
                        {
                            case SerializedPropertyType.Integer: dstProp.intValue = it.intValue; break;
                            case SerializedPropertyType.Boolean: dstProp.boolValue = it.boolValue; break;
                            case SerializedPropertyType.Float: dstProp.floatValue = it.floatValue; break;
                            case SerializedPropertyType.String: dstProp.stringValue = it.stringValue; break;
                            case SerializedPropertyType.Color: dstProp.colorValue = it.colorValue; break;
                            case SerializedPropertyType.ObjectReference: dstProp.objectReferenceValue = it.objectReferenceValue; break;
                            case SerializedPropertyType.Enum: dstProp.enumValueIndex = it.enumValueIndex; break;
                            case SerializedPropertyType.Vector2: dstProp.vector2Value = it.vector2Value; break;
                            case SerializedPropertyType.Vector3: dstProp.vector3Value = it.vector3Value; break;
                            case SerializedPropertyType.Vector4: dstProp.vector4Value = it.vector4Value; break;
                            case SerializedPropertyType.Rect: dstProp.rectValue = it.rectValue; break;
                            case SerializedPropertyType.Bounds: dstProp.boundsValue = it.boundsValue; break;
                            case SerializedPropertyType.AnimationCurve: dstProp.animationCurveValue = it.animationCurveValue; break;
                            default:
                                // ignore other types
                                break;
                        }
                    }
                    catch { /* ignore copy failures for individual properties */ }
                }
            }
            enter = it.NextVisible(false);
        }
    }
}
