using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Phase 4: High-Level Languages. Put this on the HIGH_LEVEL_PHASE object.
// It works like phase 2:
// 1. When phase 4 starts (after the assembly Wordle), the lesson is shown.
// 2. The player finds the 4 glowing code words on the map (WordPickup).
// 3. When all 4 are found, the typed quiz opens at the chest or the statue (TypedQuiz).
// 4. Passing the quiz finishes the game.
public class HighLevelPhase : MonoBehaviour
{
    // True when the quiz can be taken. The chest uses this to skip its hint.
    public static bool quizReady = false;

    [Header("Lesson popup")]
    [SerializeField] private GameObject lessonPanel;
    [SerializeField] private TMP_Text lessonText;
    [SerializeField] private Button lessonButton;

    [Header("Progress box (top left) and prompt (bottom)")]
    [SerializeField] private GameObject progressBox;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text promptText;

    [Header("Puzzle")]
    [SerializeField] private GameObject hiddenWords;

    [Header("Quiz")]
    [SerializeField] private TypedQuiz quiz;
    [SerializeField] private Transform statue;

    private UIDialog dialog;
    private GameObject player;

    private bool searching = false;
    private bool quizOpen = false;
    private bool phaseFourDone = false;
    private bool showingFirstLesson = false;
    private bool showingAllFoundMessage = false;

    private int wordsFound = 0;
    private string foundList = "";
    private float promptTimer = 0f;

    private string[] lessonMessages =
    {
        "PHASE 4: HIGH-LEVEL LANGUAGES. Assembly is still hard. High-level languages like Python and Java use words that look like English.",
        "A VARIABLE is a name that stores a value. Example: score = 10",
        "PRINT shows text on the screen. Example: print(\"Hello\")",
        "IF makes a choice. Example: if score > 5, say Good job! A LOOP repeats code many times.",
        "A COMPILER turns high-level code into machine code, so the computer can run it.",
        "PUZZLE: Find the 4 glowing code words on the map. Then open the chest or go to the statue to take the quiz. This time you type the answers!",
    };


    private void OnEnable()
    {
        GameManager.onPhaseChanged += OnPhaseChanged;
        StatueTest.onPlayerReachedStatue += TryOpenQuiz;
        PlayerChest.firstKey += TryOpenQuiz;
        TypedQuiz.onQuizFinished += OnQuizFinished;
        TypedQuiz.onQuizClosed += OnQuizClosed;
    }

    private void OnDisable()
    {
        GameManager.onPhaseChanged -= OnPhaseChanged;
        StatueTest.onPlayerReachedStatue -= TryOpenQuiz;
        PlayerChest.firstKey -= TryOpenQuiz;
        TypedQuiz.onQuizFinished -= OnQuizFinished;
        TypedQuiz.onQuizClosed -= OnQuizClosed;
    }

    // Awake runs before any Start, so everything is ready even if phase 4 starts right away
    private void Awake()
    {
        quizReady = false;
        player = GameObject.FindWithTag("Player");

        // One button: NEXT, and OK on the last page
        dialog = new UIDialog(lessonPanel, lessonText, lessonButton, null, lessonMessages, "NEXT", "BACK", "OK", "CANCEL");
        dialog.onStart += OnDialogClosed;
        dialog.Hide();

        // Everything for phase 4 stays hidden until it starts
        hiddenWords.SetActive(false);
        progressBox.SetActive(false);
        promptText.text = "";
    }

    private void Update()
    {
        // Hide the prompt after a few seconds
        if (promptTimer > 0)
        {
            promptTimer = promptTimer - Time.deltaTime;

            if (promptTimer <= 0)
            {
                promptText.text = "";
            }
        }

        // Press L to read the lesson again (not while typing in the quiz)
        bool lessonCanOpen = (searching || quizReady) && quizOpen == false && lessonPanel.activeSelf == false;
        if (lessonCanOpen && Keyboard.current.lKey.wasPressedThisFrame)
        {
            ShowMessages(lessonMessages);
        }
    }

    // ---------------- Starting phase 4 ----------------

    private void OnPhaseChanged(int phase)
    {
        if (phase == 4)
        {
            searching = true;
            hiddenWords.SetActive(true);
            progressBox.SetActive(true);
            UpdateProgress();

            showingFirstLesson = true;
            ShowMessages(lessonMessages);
        }
    }

    // ---------------- Puzzle ----------------

    // Called by WordPickup when the player walks over a word
    public void OnWordFound(string word)
    {
        wordsFound = wordsFound + 1;

        if (foundList == "")
        {
            foundList = word;
        }
        else
        {
            foundList = foundList + ", " + word;
        }

        ShowPrompt("You found " + word + "   (" + wordsFound + "/4 found)");

        // All 4 words found: the quiz is ready
        if (wordsFound == 4)
        {
            searching = false;
            quizReady = true;

            string[] messages = new string[2];
            messages[0] = "You found all 4 code words: " + foundList + ".";
            messages[1] = "Now open the chest or go to the statue to take the quiz. Type your answers!";
            showingAllFoundMessage = true;
            ShowMessages(messages);
        }

        UpdateProgress();
    }

    // ---------------- Quiz ----------------

    private void TryOpenQuiz()
    {
        if (quizReady == false || quizOpen == true)
        {
            return;
        }

        quizOpen = true;
        dialog.Hide();
        SetPlayerControl(false);
        quiz.Open();
    }

    private void OnQuizFinished(bool passed)
    {
        if (passed == true)
        {
            quizReady = false;
            phaseFourDone = true;
            Debug.Log("Phase 4 complete: typed quiz passed");
        }
    }

    private void OnQuizClosed()
    {
        quizOpen = false;
        SetPlayerControl(true);

        if (phaseFourDone == true)
        {
            progressBox.SetActive(false);

            string[] messages = new string[3];
            messages[0] = "Phase 4 complete! You can now read high-level code.";
            messages[1] = "You learned it all: machine code, assembly, and high-level languages.";
            messages[2] = "You escaped the trapped place. Thank you for playing!";
            ShowMessages(messages);

            // Phase 5 means the game is finished
            GameManager.StartPhase(5);
        }
    }

    // ---------------- Text on screen ----------------

    private void ShowMessages(string[] messages)
    {
        dialog.SetMessages(messages);
        dialog.Show();
    }

    // Called when OK is pressed on the last page of a popup
    private void OnDialogClosed()
    {
        if (showingFirstLesson == true)
        {
            showingFirstLesson = false;
            ShowPrompt("Find the 4 glowing code words on the map!");
        }

        if (showingAllFoundMessage == true)
        {
            showingAllFoundMessage = false;

            // If the player is already standing at the statue, start the quiz right away
            float distanceToStatue = Vector2.Distance(player.transform.position, statue.position);
            if (distanceToStatue < 2.5f)
            {
                TryOpenQuiz();
            }
        }
    }

    // Shows a short message at the bottom of the screen for 3 seconds
    public void ShowPrompt(string message)
    {
        promptText.text = message;
        promptTimer = 3f;
    }

    // Updates the box in the top left corner
    private void UpdateProgress()
    {
        string text = "<color=#FFD95A>PHASE 4: HIGH-LEVEL LANGUAGES</color>\n";

        if (quizReady == true)
        {
            text = text + "Words: " + foundList + "\n";
            text = text + "Quiz: open the chest or go to the statue.";
        }
        else
        {
            text = text + "Code words found: " + wordsFound + "/4\n";
            text = text + "Find the glowing words. (L: lesson)";
        }

        progressText.text = text;
    }

    // Stops the player from moving, grabbing or opening chests during the quiz
    private void SetPlayerControl(bool canControl)
    {
        player.GetComponent<PlayerMovement>().enabled = canControl;
        player.GetComponent<PlayerGrab>().enabled = canControl;
        player.GetComponent<PlayerChest>().enabled = canControl;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }
}
