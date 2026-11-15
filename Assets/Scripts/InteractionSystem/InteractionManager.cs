using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private InteractionUI interactionUI;
    [SerializeField] private AudioSource SFXplayer;

    private Interaction currentInteraction;
    private PlayerController player; // probably should change this to some data holding class instead of the controller itself later?

    private AudioClip currentSFX;

    private void Awake() {
        player = GetComponent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D c) {
        Interaction interaction = c.GetComponent<Interaction>();
        if (interaction != null && interaction.CanInteract(player)) {
            currentInteraction = interaction;
            if (interactionUI != null) {
                string keyLabel = currentInteraction.InteractionKey.ToString();
                interactionUI.Show(currentInteraction, keyLabel);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D c) {
        Interaction interaction = c.GetComponent<Interaction>();
        if (currentInteraction == interaction) {
            currentInteraction = null;
            if (interactionUI != null) {
                interactionUI.Hide();
            }
        }
    }

    private void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
        {
            if (interactionUI != null)
                interactionUI.Hide();

            return;
        }
        if (currentInteraction == null)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null &&
            keyboard[currentInteraction.InteractionKey].wasPressedThisFrame)
        {
            Interact();
        }
    }

    public void Interact() {
        if (currentInteraction == null) {
            return;
        }
        currentInteraction.Interact(player);

        currentSFX = currentInteraction.interactionSFX;
        if (currentSFX != null)
        {
            SFXplayer.clip = currentSFX;
            SFXplayer.Play();
        }
    }
}
