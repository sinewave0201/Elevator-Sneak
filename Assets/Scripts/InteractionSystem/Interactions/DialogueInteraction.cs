
using UnityEngine;
using System.Collections.Generic;

public class DialogueInteraction : Interaction
{
    [SerializeField] private DialogueTrigger dialogueTrigger;

    public override bool CanInteract(PlayerController player)
    {
        return base.CanInteract(player)
            && dialogueTrigger != null
            && DialogueManager.Instance != null
            && !DialogueManager.Instance.IsOpen;
    }

    public override void Interact(PlayerController player)
    {
        if (dialogueTrigger == null)
        {
            Debug.LogError(
                "DialogueInteraction has no DialogueTrigger assigned.",
                this);
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogError(
                "No DialogueManager exists in the current scene.",
                this);
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueTrigger, OnCompletion);
    }

    public override void OnCompletion(List<ProgressFlag> pendingFlags)
    {
        PlayerDataManager manager = PlayerDataManager.Instance;

        if (manager == null || manager.Progress == null)
        {
            Debug.LogError("Player progress manager is unavailable.", this);
            return;
        }

        foreach (ProgressFlag p in pendingFlags) {
            manager.Progress.SetFlag(p);
        }
    }
}
