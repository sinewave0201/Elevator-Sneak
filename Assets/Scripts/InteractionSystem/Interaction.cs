using UnityEngine;

// base class for all interactions
// probably should not be passing playerController and instead some
// sort of player data object
public abstract class Interaction : MonoBehaviour
{
    [SerializeField] private string interactionPrompt = "Interact";
    [SerializeField] private InteractionRequirement requirement;

    public string InteractionPrompt => interactionPrompt;

    public virtual bool CanInteract(PlayerController player)
    {
        return requirement == null || requirement.IsMet(player);
    }

    public abstract void Interact(PlayerController player);
}