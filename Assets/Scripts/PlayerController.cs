using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;

    [Header("Requirement")]
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactAction;

    private InteractionManager interactionManager;
    private Collider2D interactable;
    private InputAction dialogueAction;

    private float horizontalInput;

    private void Awake()
    {
        interactionManager = GetComponent<InteractionManager>();
        dialogueAction = interactAction.asset.FindAction("PC-Default/Dialogue", true);

        Debug.Log("InteractionManager: " + interactionManager);
    }

    private void OnEnable()
    {
        dialogueAction?.Enable();
    }

    private void OnDisable()
    {
        dialogueAction?.Disable();
    }

    private void Update()
    {
        bool dialoguePressed = dialogueAction.WasPerformedThisFrame();

        // F advances or closes dialogue and blocks movement/other interactions while it is open.
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
        {
            horizontalInput = 0f;

            if (dialoguePressed)
                DialogueManager.Instance.AdvanceOrClose();

            return;
        }

        horizontalInput = moveAction.action.ReadValue<Vector2>().x;

        if (horizontalInput != 0f)
        {
            spriteRenderer.flipX = horizontalInput < 0f;
        }

        // F is reserved for speaking.
        if (dialoguePressed && interactable != null)
        {
            DialogueTrigger dialogueTrigger = interactable.GetComponent<DialogueTrigger>();

            if (dialogueTrigger != null)
                dialogueTrigger.Interact();
        }

        if (interactAction.action.WasPerformedThisFrame())//if e is pressed
        {
            Debug.Log("e is pressed");

            if (interactionManager != null)
                interactionManager.Interact();
        }
    }

    //interaction logic
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<DialogueTrigger>() != null)
            interactable = other;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (interactable == other)
        {
            interactable = null;

            if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
                DialogueManager.Instance.CloseDialogue();
        }
    }

    private void FixedUpdate()
    {
        body.linearVelocity = new Vector2(horizontalInput * moveSpeed, body.linearVelocityY);
    }
}
