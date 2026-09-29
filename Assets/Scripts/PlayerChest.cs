using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerChest : MonoBehaviour
{
    public Sprite imgOpened;
    public Sprite imgClosed;

    private bool isOpened = false;
    private GameObject chest;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Chest"))
        {
            // Remember the chest
            chest = collision.gameObject;
        }
    }

    private void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            // 1. MUST check if a chest exists before trying to open or close it!
            if (chest != null)
            {
                if (!isOpened)
                {
                    isOpened = true;
                    chest.GetComponent<SpriteRenderer>().sprite = imgOpened;
                }
                else
                {
                    // 2. Allow the player to manually close it by pressing F again
                    isOpened = false;
                    chest.GetComponent<SpriteRenderer>().sprite = imgClosed;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Chest"))
        {
            // Visually close it if they walk away
            collision.GetComponent<SpriteRenderer>().sprite = imgClosed;

            // 3. Reset everything so the chest works perfectly the next time!
            if (chest == collision.gameObject)
            {
                chest = null;
                isOpened = false;
            }
        }
    }
}