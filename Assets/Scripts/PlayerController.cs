using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;

    [Header("Requirement")]
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Transform characterVisual;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private Animator mcAnimator;
    [SerializeField] private InputActionReference dialogueAdvanceAction;

    private Vector3 characterVisualScale;

    private float horizontalInput;

    private void Awake()
    {
        mcAnimator.SetBool("isWalking", false);
        //in charge of fliping
        characterVisualScale = characterVisual.localScale;
    }

    private void OnEnable()
    {
        dialogueAdvanceAction?.action.Enable();
    }

    private void OnDisable()
    {
        dialogueAdvanceAction?.action.Disable();
    }

    private void Update()
    {
        bool dialoguePressed = dialogueAdvanceAction != null && dialogueAdvanceAction.action.WasPerformedThisFrame();


        // F advances or closes dialogue and blocks movement/other interactions while it is open.
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
        {
            horizontalInput = 0f;

            if (dialoguePressed)
                DialogueManager.Instance.AdvanceOrClose();

            return;
        }

        horizontalInput = moveAction.action.ReadValue<Vector2>().x;

        if (horizontalInput == 0f)
        {
            //not moving: update animation
            mcAnimator.SetBool("isWalking", false);
        }
        if (horizontalInput != 0f)
        {
            //moving: update animation
            mcAnimator.SetBool("isWalking", true);
            Vector3 scale = characterVisualScale;
            scale.x = Mathf.Abs(characterVisualScale.x)
                    * (horizontalInput < 0f ? 1f : -1f);

            characterVisual.localScale = scale;
        }
    }
    private void FixedUpdate()
    {
        body.linearVelocity = new Vector2(horizontalInput * moveSpeed, body.linearVelocityY);
    }
}
