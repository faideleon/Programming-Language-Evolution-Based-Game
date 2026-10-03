using System.Collections.Generic;
using Cainos.PixelArtTopDown_Basic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrab : MonoBehaviour
{
    private GameObject box;
    private PlayerMovement movement;
    public bool isGrabbing = false;

    private float originalSpeed;

    // The box's weight, remembered while carrying it (the Rigidbody is removed while carried)
    private float boxMass = 10f;
    private float boxDamping = 10f;

    public List<GameObject> nearbyBoxes = new List<GameObject>();

    // How fast the player slides to the middle of the stairs while carrying a box
    [SerializeField] private float stairsCenterSpeed = 3f;
    private StairsLayerTrigger[] stairs;
    private Rigidbody2D rb;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        originalSpeed = movement.moveSpeed;
        stairs = FindObjectsByType<StairsLayerTrigger>();
    }

    private void FixedUpdate()
    {
        if (isGrabbing)
        {
            LineUpWithStairs();
        }
    }

    // While carrying a box the player can only walk in one direction, so they can't
    // step sideways to fit the box through the stairs.
    // So when they carry a box up or down stairs, we slowly slide them to the middle of the stairs.
    private void LineUpWithStairs()
    {
        // Only when walking up or down
        bool walkingUpOrDown = movement.lockedDirection == PlayerDirection.NORTH || movement.lockedDirection == PlayerDirection.SOUTH;
        if (walkingUpOrDown == false || rb.linearVelocity.y == 0)
        {
            return;
        }

        foreach (StairsLayerTrigger stair in stairs)
        {
            // Only stairs that go up and down
            if (stair.direction != StairsLayerTrigger.Direction.South)
            {
                continue;
            }

            Vector2 stairPosition = stair.transform.position;
            float distanceUpDown = Mathf.Abs(rb.position.y - stairPosition.y);
            float distanceSideways = Mathf.Abs(rb.position.x - stairPosition.x);

            // Is the player close to these stairs?
            if (distanceUpDown < 4f && distanceSideways < 1.4f)
            {
                Vector2 newPosition = rb.position;
                newPosition.x = Mathf.MoveTowards(rb.position.x, stairPosition.x, stairsCenterSpeed * Time.fixedDeltaTime);
                rb.position = newPosition;
                return;
            }
        }
    }

    private void Update()
    {
        BoxDirection();

        if (!isGrabbing)
        {
            if (nearbyBoxes.Count > 0) box = TargetBox();
            else box = null;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isGrabbing && box != null)
            {
                // 1. BEFORE WE GRAB: Shoot the raycast to make sure there are no walls
                Vector2 pushDirection = Vector2.zero;
                if (movement.playerDirection == PlayerDirection.NORTH) pushDirection = Vector2.up;
                else if (movement.playerDirection == PlayerDirection.SOUTH) pushDirection = Vector2.down;
                else if (movement.playerDirection == PlayerDirection.WEST) pushDirection = Vector2.left;
                else if (movement.playerDirection == PlayerDirection.EAST) pushDirection = Vector2.right;

                int targetLayer = 1 << gameObject.layer;
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, pushDirection, 1f, targetLayer);

                foreach (RaycastHit2D hit in hits)
                {
                    bool isPartOfBox = hit.collider.transform.IsChildOf(box.transform);
                    bool isPartOfPlayer = hit.collider.transform.IsChildOf(this.transform);

                    // If we hit a solid wall, cancel the grab entirely
                    if (!isPartOfBox && !isPartOfPlayer && !hit.collider.isTrigger)
                    {
                        Debug.Log("Cannot grab box! Wall in the way: " + hit.collider.name);
                        return; // Stop the code right here!
                    }
                }

                // 2. IF THE COAST IS CLEAR: Proceed with the normal grab logic
                isGrabbing = true;
                box.transform.SetParent(this.transform);

                Rigidbody2D boxRB = box.GetComponent<Rigidbody2D>();
                boxMass = boxRB.mass;
                boxDamping = boxRB.linearDamping;
                Destroy(boxRB);

                // Stone boxes are heavy, so the player walks slower with them
                StoneBox stoneBox = box.GetComponent<StoneBox>();
                if (stoneBox != null)
                {
                    movement.moveSpeed = stoneBox.carrySpeed;
                }
                else
                {
                    movement.moveSpeed = 2f;
                }

                movement.isHoldingBox = true;
                movement.lockedDirection = movement.playerDirection;
                SoundManager.PlayGrab();
            }
            else if (isGrabbing && box != null)
            {
                // DROP THE BOX
                Rigidbody2D newRB = box.AddComponent<Rigidbody2D>();
                newRB.bodyType = RigidbodyType2D.Dynamic;
                newRB.gravityScale = 0f;
                newRB.mass = boxMass;
                newRB.linearDamping = boxDamping;
                newRB.freezeRotation = true;

                isGrabbing = false;
                box.transform.SetParent(null);

                movement.moveSpeed = originalSpeed;
                movement.isHoldingBox = false;
                SoundManager.PlayDrop();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PushableBox"))
        {
            if (!nearbyBoxes.Contains(collision.gameObject))
            {
                nearbyBoxes.Add(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PushableBox"))
        {
            nearbyBoxes.Remove(collision.gameObject);
        }
    }

    private GameObject TargetBox()
    {
        float closestDistance = Mathf.Infinity;
        GameObject target = null;

        foreach (GameObject boxy in nearbyBoxes)
        {
            float distance = Vector3.Distance(transform.position, boxy.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = boxy;
            }
        }

        return target;
    }

    void BoxDirection()
    {
        if (box != null && isGrabbing)
        {
            switch (movement.playerDirection)
            {
                case PlayerDirection.SOUTH:
                    box.transform.localPosition = new Vector3(0, -1, 0);
                    break;
                case PlayerDirection.NORTH:
                    box.transform.localPosition = new Vector3(0, 1, 0);
                    break;
                case PlayerDirection.EAST:
                    box.transform.localPosition = new Vector3(1, 0, 0);
                    break;
                case PlayerDirection.WEST:
                    box.transform.localPosition = new Vector3(-1, 0, 0);
                    break;
            }
        }
    }
}