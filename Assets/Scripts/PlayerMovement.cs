using UnityEngine;
using UnityEngine.InputSystem;

// If this enum is already defined in another script, you can delete this block!
public enum PlayerDirection
{
    NORTH,
    SOUTH,
    EAST,
    WEST
}

public class PlayerMovement : MonoBehaviour
{
    Ray ray = new Ray();
    public float moveSpeed = 5f;
    public PlayerDirection playerDirection;

    // --- NEW VARIABLES FOR BOX GRABBING ---
    public bool isHoldingBox = false;
    public PlayerDirection lockedDirection;
    // --------------------------------------

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        
        // 1. Get raw input from keyboard
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current != null)
        {
            
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY = -1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
        }

        moveInput = new Vector2(moveX, moveY);

        // 2. Diagonal speed normalization (prevents moving faster diagonally)
        if (moveInput.magnitude > 1f)
        {
            moveInput = moveInput.normalized;
        }

        // 3. APPLY THE MOVEMENT LOCK FILTER
        if (isHoldingBox)
        {
            if (lockedDirection == PlayerDirection.NORTH || lockedDirection == PlayerDirection.SOUTH)
            {
                moveInput.x = 0f; // Lock horizontal movement
            }
            else if (lockedDirection == PlayerDirection.EAST || lockedDirection == PlayerDirection.WEST)
            {
                moveInput.y = 0f; // Lock vertical movement
            }
        }
        else
        {
            // 4. Only update the facing direction if we ARE NOT holding a box.
            // This ensures the player doesn't magically turn around if they pull the box backwards!
            UpdateFacingDirection();
        }
    }

    private void FixedUpdate()
    {
        // 5. Apply movement physics in FixedUpdate to prevent camera/collision stuttering
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void UpdateFacingDirection()
    {
        if (moveInput.x > 0) playerDirection = PlayerDirection.EAST;
        else if (moveInput.x < 0) playerDirection = PlayerDirection.WEST;
        else if (moveInput.y > 0) playerDirection = PlayerDirection.NORTH;
        else if (moveInput.y < 0) playerDirection = PlayerDirection.SOUTH;
    }
}