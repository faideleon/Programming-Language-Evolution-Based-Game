using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrab : MonoBehaviour
{
    private GameObject box;
    private PlayerMovement movement;
    public bool isGrabbing = false;

    private float originalSpeed;

    public List<GameObject> nearbyBoxes = new List<GameObject>();

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        originalSpeed = movement.moveSpeed;
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

                Destroy(box.GetComponent<Rigidbody2D>());
                movement.moveSpeed = 2f;

                movement.isHoldingBox = true;
                movement.lockedDirection = movement.playerDirection;
            }
            else if (isGrabbing && box != null)
            {
                // DROP THE BOX
                Rigidbody2D newRB = box.AddComponent<Rigidbody2D>();
                newRB.bodyType = RigidbodyType2D.Dynamic;
                newRB.gravityScale = 0f;
                newRB.mass = 10f;
                newRB.linearDamping = 10f;
                newRB.freezeRotation = true;

                isGrabbing = false;
                box.transform.SetParent(null);

                movement.moveSpeed = originalSpeed;
                movement.isHoldingBox = false;
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