using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on the altar.
// Phase 1: bring a WOODEN box. Dropping it on the altar shows the ancient lesson.
// Phase 2: the altar has nothing to do, it tells the player to finish phase 2.
// Phase 3: bring a STONE box. Dropping it on the altar shows the assembly lesson.
// After a lesson, walking to the altar shows that lesson again.
// Every time a lesson ends, the popup tells the player to go to the statue.
public class BoxToAltar : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button button;
    [SerializeField] private AncientLessonScript lesson;
    [SerializeField] private AssemblyLessonScript assemblyLesson;

    private UIDialog dialog;
    private bool woodenBoxPlaced = false;
    private bool stoneBoxPlaced = false;

    private string phase1Message = "Bring a wooden box to the altar to activate the key to solving phase 1. There are 4 phases in total.";
    private string phase1StatueMessage = "Go to the statue with four pillars to take a test to pass phase 1: Ancient Programming Languages.";
    private string phase2Message = "The altar is sleeping. Finish phase 2 first: find the glowing numbers and set the lanterns by the statue.";
    private string phase3Message = "Bring a stone box to the altar to unlock phase 3: Assembly Language. Stone boxes are heavy!";
    private string phase3StatueMessage = "Go to the statue with four pillars to take a test to pass phase 3: Assembly Language.";
    private string doneMessage = "The altar has taught you everything for now. Phase 4 is coming soon!";

    private void Start()
    {
        string[] messages = new string[1];
        messages[0] = phase1Message;

        // No Next button, only Back. The last two texts make the Back button say BACK instead of CANCEL.
        dialog = new UIDialog(panel, text, null, button, messages, "NEXT", "BACK", "START", "BACK");
    }

    private void OnEnable()
    {
        AncientLessonScript.onLessonFinished += OnAncientLessonFinished;
        AssemblyLessonScript.onLessonFinished += OnAssemblyLessonFinished;
    }

    private void OnDisable()
    {
        AncientLessonScript.onLessonFinished -= OnAncientLessonFinished;
        AssemblyLessonScript.onLessonFinished -= OnAssemblyLessonFinished;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // The player reached the altar
        if (collision.gameObject.CompareTag("Player"))
        {
            int phase = GameManager.currentPhase;

            if (phase == 1)
            {
                if (woodenBoxPlaced == false)
                {
                    ShowMessage(phase1Message);
                }
                else
                {
                    // Show the lesson again so they can re-read it
                    dialog.Hide();
                    lesson.Show();
                }
            }
            else if (phase == 2)
            {
                ShowMessage(phase2Message);
            }
            else if (phase == 3)
            {
                if (stoneBoxPlaced == false)
                {
                    ShowMessage(phase3Message);
                }
                else
                {
                    dialog.Hide();
                    assemblyLesson.Show();
                }
            }
            else
            {
                ShowMessage(doneMessage);
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
        if (collision.gameObject.CompareTag("PushableBox") == false)
        {
            return;
        }

        // While the player carries the box, it is a child of the player.
        // It only counts once the box is dropped on the altar.
        bool isBeingCarried = collision.GetComponentInParent<PlayerGrab>() != null;
        if (isBeingCarried == true)
        {
            return;
        }

        bool isStoneBox = collision.GetComponent<StoneBox>() != null;
        int phase = GameManager.currentPhase;

        // Phase 1 needs a wooden box
        if (phase == 1 && woodenBoxPlaced == false && isStoneBox == false)
        {
            woodenBoxPlaced = true;
            dialog.Hide();
            lesson.Show();
        }

        // Phase 3 needs a stone box
        if (phase == 3 && stoneBoxPlaced == false && isStoneBox == true)
        {
            stoneBoxPlaced = true;
            dialog.Hide();
            assemblyLesson.Show();
        }
    }

    private void ShowMessage(string message)
    {
        dialog.SetMessage(0, message);
        dialog.Show();
    }

    private void OnAncientLessonFinished()
    {
        ShowMessage(phase1StatueMessage);
    }

    private void OnAssemblyLessonFinished()
    {
        ShowMessage(phase3StatueMessage);
    }
}
