using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on the altar.
// 1. Before the box is placed, walking to the altar shows a popup: bring a box.
// 2. Dropping a box on the altar shows the lesson.
// 3. After that, walking to the altar shows the lesson again.
// 4. Every time the lesson ends, the popup tells the player to go to the statue.
public class BoxToAltar : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button button;
    [SerializeField] private AncientLessonScript lesson;

    private UIDialog dialog;
    private bool boxPlaced = false;

    private string boxMessage = "Get a box to the altar to activate the key to solving phase 1. There are 4 phases in total.";
    private string statueMessage = "Go to the statue with four pillars to take a test to pass phase 1: Ancient Programming Languages.";

    private void Start()
    {
        string[] messages = new string[1];
        messages[0] = boxMessage;

        // No Next button, only Back. The last two texts make the Back button say BACK instead of CANCEL.
        dialog = new UIDialog(panel, text, null, button, messages, "NEXT", "BACK", "START", "BACK");
    }

    private void OnEnable()
    {
        AncientLessonScript.onLessonFinished += OnLessonFinished;
    }

    private void OnDisable()
    {
        AncientLessonScript.onLessonFinished -= OnLessonFinished;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // The player reached the altar
        if (collision.gameObject.CompareTag("Player"))
        {
            if (boxPlaced == false)
            {
                // No box yet: tell the player to bring one
                dialog.Show();
            }
            else
            {
                // Box is already on the altar: show the lesson again so they can re-read it
                dialog.Hide();
                lesson.Show();
            }
        }

        CheckForBox(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        CheckForBox(collision);
    }

    private void CheckForBox(Collider2D collision)
    {
        if (boxPlaced == true)
        {
            return;
        }

        if (collision.gameObject.CompareTag("PushableBox"))
        {
            // While the player carries the box, it is a child of the player.
            // It only counts once the box is dropped on the altar.
            bool isBeingCarried = collision.GetComponentInParent<PlayerGrab>() != null;

            if (isBeingCarried == false)
            {
                boxPlaced = true;
                dialog.Hide();
                lesson.Show();
            }
        }
    }

    private void OnLessonFinished()
    {
        dialog.SetMessage(0, statueMessage);
        dialog.Show();
    }
}
