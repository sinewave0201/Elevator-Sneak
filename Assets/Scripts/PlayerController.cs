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

    private Collider2D interactable;

    private float horizontalInput;
    private void Awake(){}

    private void Update()
    {
        //moving logic
        horizontalInput = moveAction.action.ReadValue<Vector2>().x;

        if (horizontalInput != 0f)
        {
            spriteRenderer.flipX = horizontalInput < 0f;
        }

        if (interactAction.action.WasPerformedThisFrame() && interactable != null)//if e is pressed
        {
            Debug.Log("e is pressed");
            if (interactable.CompareTag("Elevator"))
            {
                ElevatorManager em = interactable.GetComponent<ElevatorManager>();
                em.interact();
            }
            
            else if (interactable.CompareTag("Stair"))
            {
                StairManager sm = interactable.GetComponent<StairManager>();
                sm.interact();
            }

            else if (interactable.CompareTag("Interactables"))
            {
                
            }
        }
    }

    //interaction logic
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Elevator") || other.CompareTag("Stair") || 
            other.CompareTag("interactable"))
            interactable = other;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (interactable == other)
            interactable = null;
    }


    private void FixedUpdate()
    {
        body.linearVelocity = new Vector2(horizontalInput * moveSpeed, body.linearVelocityY);
    }
}
