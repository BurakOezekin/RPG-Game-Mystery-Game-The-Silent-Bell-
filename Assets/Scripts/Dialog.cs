using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialog
{
    [SerializeField] List<string> lines;

    [Header("Zeigt ein KI-Eingabefeld (InputField)?")]
    [SerializeField] bool usesAIInput = false; // 👈 wird im Inspector angezeigt

    public List<string> Lines
    {
        get { return lines; }
    }

    public bool UsesAIInput
    {
        get { return usesAIInput; }
    }
}
