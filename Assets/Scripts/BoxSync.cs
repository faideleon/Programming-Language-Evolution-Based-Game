using UnityEngine;

public class BoxSync : MonoBehaviour
{
    private SpriteRenderer[] allSpriteRenderers;
    private GameObject player;
    private SpriteRenderer playerSpriteRenderer;

    private bool canInteract;

    private void Awake()
    {
        // Find all the sprite renderers on the box when the game starts
        allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        // As long as the player is touching the box, sync the layers continuously.
        // This ensures that the exact microsecond the player steps on the stairs and 
        // changes layers, the box comes with them!
        if (canInteract && player != null && playerSpriteRenderer != null)
        {
            SyncWithPlayer();
        }
    }

    void SyncWithPlayer()
    {
        // 1. Sync the Parent's Physics Layer
        gameObject.layer = player.layer;

        // 2. Sync all Children's Physics Layers (CRITICAL if your collider is on a child)
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            child.gameObject.layer = player.layer;
        }

        // 3. Sync Sorting Layers (Visuals)
        string playerSortingLayer = playerSpriteRenderer.sortingLayerName;
        foreach (SpriteRenderer srs in allSpriteRenderers)
        {
            if (srs.sortingLayerName != playerSortingLayer)
            {
                srs.sortingLayerName = playerSortingLayer;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            canInteract = true;
            player = collision.gameObject;

            // Cache the sprite renderer so we don't have to use GetComponent every frame
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
        }

        if (collision.gameObject.CompareTag("TriggerStairs"))
        {
            if (player != null && playerSpriteRenderer != null)
            {
                SyncWithPlayer();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            canInteract = false;
            player = null;
            playerSpriteRenderer = null; // Clear the cache
        }

        if (collision.gameObject.CompareTag("TriggerStairs"))
        {
            if (player != null && playerSpriteRenderer != null)
            {
                SyncWithPlayer();
            }
        }
    }
}