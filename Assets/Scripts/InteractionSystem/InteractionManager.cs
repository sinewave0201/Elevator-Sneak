using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private InteractionUI interactionUI;

    private Interaction currentInteraction;
    private bool hasInteracted;
    private PlayerController player; // probably should change this to some data holding class instead of the controller itself later?

    private void Awake() {
        player = GetComponent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D c) {
        Interaction interaction = c.GetComponent<Interaction>();
        if (interaction != null && interaction.CanInteract(player)) {
            currentInteraction = interaction;
            hasInteracted = false;
            if (interactionUI != null) {
                interactionUI.Show(interaction);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D c) {
        Interaction interaction = c.GetComponent<Interaction>();
        if (currentInteraction == interaction) {
            currentInteraction = null;
            hasInteracted = false;
            if (interactionUI != null) {
                interactionUI.Hide();
            }
        }
    }

    public void Interact() {
        if (currentInteraction == null || hasInteracted) {
            return;
        }
        hasInteracted = true;
        currentInteraction.Interact(player);
    }
}
