using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeterController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    
    private bool hasSpoken = false;

    public void Interact()
    {
        // Mark quest as started
        if (!hasSpoken && QuestManager.Instance != null)
        {
            QuestManager.Instance.MarkPeterTalked();
            hasSpoken = true;
        }
        
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
    }
}
