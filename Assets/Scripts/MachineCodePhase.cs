using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Phase 2: Machine Code. Put this on the MACHINE_CODE_PHASE object.
// 1. When phase 2 starts (after the phase 1 Wordle), the machine code lesson is shown.
// 2. The player finds the 4 glowing numbers on the map (BitPickup).
// 3. The player sets the 4 lanterns around the statue to those numbers (BinaryLantern).
// 4. When the lanterns are right, the quiz opens at the chest or the statue (MachineCodeQuiz).
// 5. Passing the quiz starts phase 3.
public class MachineCodePhase : MonoBehaviour
{
    // True when the quiz can be taken. The chest uses this to skip its old hint.
    public static bool quizReady = false;

    [Header("Answer")]
    [Tooltip("The last 4 bits of the hidden byte. The first 4 are always 0000. If you change this, also change the last quiz question.")]
    [SerializeField] private string answer = "1011";

    [Header("Lesson popup")]
    [SerializeField] private GameObject lessonPanel;
    [SerializeField] private TMP_Text lessonText;
    [SerializeField] private Button lessonButton;

    [Header("Progress box (top left) and prompt (bottom)")]
    [SerializeField] private GameObject progressBox;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text promptText;

    [Header("Puzzle")]
    [SerializeField] private GameObject hiddenNumbers;
    [SerializeField] private BinaryLantern[] lanterns;

    [Header("Quiz")]
    [SerializeField] private MachineCodeQuiz quiz;
    [SerializeField] private Transform statue;

    private UIDialog dialog;
    private GameObject player;

    private bool puzzleActive = false;
    private bool quizOpen = false;
    private bool phaseTwoDone = false;
    private bool showingFirstLesson = false;
    private bool showingSolvedMessage = false;

    private bool[] bitFound = new bool[4];
    private int bitsFound = 0;
    private float promptTimer = 0f;

    private string[] lessonMessages =
    {
        "PHASE 2: MACHINE CODE. Computers only understand two numbers: 0 and 1. Code made of 0s and 1s is called machine code.",
        "One 0 or 1 is called a bit. 8 bits together make a byte.",
        "Each bit has a value: 8, 4, 2, 1. Add the values where you see a 1. Example: 0101 = 4 + 1 = 5.",
        "Some numbers tell the computer what to do, like add or jump. This kind of number is called an opcode.",
        "PUZZLE: Find the 4 glowing numbers on the map. Then go to the 4 lanterns by the statue and press F to set each lantern to its number.",
    };


    private void OnEnable()
    {
        GameManager.onPhaseChanged += OnPhaseChanged;
        StatueTest.onPlayerReachedStatue += TryOpenQuiz;
        PlayerChest.firstKey += TryOpenQuiz;
        MachineCodeQuiz.onQuizFinished += OnQuizFinished;
        MachineCodeQuiz.onQuizClosed += OnQuizClosed;
    }

    private void OnDisable()
    {
        GameManager.onPhaseChanged -= OnPhaseChanged;
        StatueTest.onPlayerReachedStatue -= TryOpenQuiz;
        PlayerChest.firstKey -= TryOpenQuiz;
        MachineCodeQuiz.onQuizFinished -= OnQuizFinished;
        MachineCodeQuiz.onQuizClosed -= OnQuizClosed;
    }

    // Awake runs before any Start, so everything is ready even if phase 2 starts right away
    private void Awake()
    {
        quizReady = false;
        player = GameObject.FindWithTag("Player");

        // One button: NEXT, and OK on the last page
        dialog = new UIDialog(lessonPanel, lessonText, lessonButton, null, lessonMessages, "NEXT", "BACK", "OK", "CANCEL");
        dialog.onStart += OnDialogClosed;
        dialog.Hide();

        // Everything for phase 2 stays hidden until it starts
        hiddenNumbers.SetActive(false);
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

        // Press L to read the lesson again
        bool lessonCanOpen = puzzleActive && quizOpen == false && lessonPanel.activeSelf == false;
        if (lessonCanOpen && Keyboard.current.lKey.wasPressedThisFrame)
        {
            ShowMessages(lessonMessages);
        }
    }

    // ---------------- Starting phase 2 ----------------

    private void OnPhaseChanged(int phase)
    {
        if (phase == 2)
        {
            StartPhaseTwo();
        }
    }

    private void StartPhaseTwo()
    {
        puzzleActive = true;

        hiddenNumbers.SetActive(true);
        progressBox.SetActive(true);
        foreach (BinaryLantern lantern in lanterns)
        {
            lantern.ShowLabels();
        }
        UpdateProgress();

        showingFirstLesson = true;
        ShowMessages(lessonMessages);
    }

    // ---------------- Puzzle ----------------

    // Returns the answer bit for a lantern (1 to 4) as "0" or "1"
    public string GetBit(int lanternNumber)
    {
        return answer[lanternNumber - 1].ToString();
    }

    public bool IsPuzzleActive()
    {
        return puzzleActive;
    }

    public bool AllBitsFound()
    {
        return bitsFound == 4;
    }

    // Called by BitPickup when the player walks over a number
    public void OnBitFound(int lanternNumber)
    {
        bitFound[lanternNumber - 1] = true;
        bitsFound = bitsFound + 1;

        ShowPrompt("Lantern #" + lanternNumber + " = " + GetBit(lanternNumber) + "   (" + bitsFound + "/4 found)");
        UpdateProgress();
    }

    // Called by BinaryLantern every time a lantern is changed
    public void OnLanternChanged()
    {
        // Check if every lantern shows the right number
        for (int i = 0; i < lanterns.Length; i++)
        {
            string lanternValue = lanterns[i].value.ToString();
            if (lanternValue != GetBit(i + 1))
            {
                return;
            }
        }

        // All lanterns are right!
        puzzleActive = false;
        quizReady = true;
        UpdateProgress();

        string[] messages = new string[2];
        messages[0] = "You did it! The lanterns say 0000 " + answer + " = " + AnswerAsNumber() + ".";
        messages[1] = "Now open the chest or go to the statue to take the quiz.";
        showingSolvedMessage = true;
        ShowMessages(messages);
    }

    // "1011" -> 11
    private int AnswerAsNumber()
    {
        return Convert.ToInt32(answer, 2);
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
            phaseTwoDone = true;
            Debug.Log("Phase 2 complete: machine code quiz passed");
        }
    }

    private void OnQuizClosed()
    {
        quizOpen = false;
        SetPlayerControl(true);

        if (phaseTwoDone == true)
        {
            progressBox.SetActive(false);

            string[] messages = new string[2];
            messages[0] = "Phase 2 complete! You can now read machine code.";
            messages[1] = "Next: Assembly Language. Open the chest to see what to do.";
            ShowMessages(messages);

            GameManager.StartPhase(3);
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
            ShowPrompt("Find the 4 glowing numbers on the map!");
        }

        if (showingSolvedMessage == true)
        {
            showingSolvedMessage = false;

            // The lanterns are right next to the statue, so the player is often
            // already standing there. Then start the quiz right away.
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
        // Show the numbers found so far, and ? for the missing ones
        string bits = "";
        for (int i = 0; i < 4; i++)
        {
            if (bitFound[i] == true)
            {
                bits = bits + GetBit(i + 1);
            }
            else
            {
                bits = bits + "?";
            }
        }

        string text = "<color=#FFD95A>PHASE 2: MACHINE CODE</color>\n";

        if (quizReady == true)
        {
            text = text + "Byte: 0000 " + answer + " = " + AnswerAsNumber() + "\n";
            text = text + "Quiz: open the chest or go to the statue.";
        }
        else if (AllBitsFound() == true)
        {
            text = text + "Byte: 0000 " + bits + "   Found: 4/4\n";
            text = text + "Go set the lanterns. (L: lesson)";
        }
        else
        {
            text = text + "Byte: 0000 " + bits + "   Found: " + bitsFound + "/4\n";
            text = text + "Find the glowing numbers. (L: lesson)";
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
