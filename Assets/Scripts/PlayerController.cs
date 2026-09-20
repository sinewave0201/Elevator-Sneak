using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;

    [Header("Requirement")]
    [SerializeField] private Rigidbody2D body;
    [SerializeField]private SpriteRenderer spriteRenderer;
    [SerializeField]private InputActionReference moveAction;
    [SerializeField]private InputActionReference interactAction;
    
    private InteractionManager interactionManager;
    private float horizontalInput;
    private void Awake(){
        interactionManager = GetComponent<InteractionManager>();

        Debug.Log("InteractionManager: " + interactionManager);
    }

    private void Update()
    {
        //moving logic
        horizontalInput = moveAction.action.ReadValue<Vector2>().x;

        if (horizontalInput != 0f)
        {
            spriteRenderer.flipX = horizontalInput < 0f;
        }

        if (interactAction.action.WasPerformedThisFrame())//if e is pressed
        {
            Debug.Log("e is pressed");
            interactionManager.Interact(); 
        }
    }

    private void FixedUpdate()
    {
        body.linearVelocity = new Vector2(horizontalInput * moveSpeed, body.linearVelocityY);
    }
}
