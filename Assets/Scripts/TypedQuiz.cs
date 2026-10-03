using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// One question of the typed quiz
[Serializable]
public class TypedQuestion
{
    public string question;
    // Every answer that counts as right (big or small letters don't matter)
    public string[] rightAnswers;
    // Shown after the player answers
    public string explanation;

    public TypedQuestion(string question, string[] rightAnswers, string explanation)
    {
        this.question = question;
        this.rightAnswers = rightAnswers;
        this.explanation = explanation;
    }
}

// Phase 4 test: the player types the answers.
// Put this on the HIGH_LEVEL_QUIZ object. HighLevelPhase calls Open().
// Press Enter to answer, then Enter again for the next question.
public class TypedQuiz : MonoBehaviour
{
    // Called at the end of the quiz. true = passed
    public static event Action<bool> onQuizFinished;
    // Called when the quiz window is closed
    public static event Action onQuizClosed;

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button answerButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private int passScore = 5;

    // Every question comes from the lesson
    [SerializeField] private TypedQuestion[] questions =
    {
        new TypedQuestion("A name that stores a value, like score = 10, is called a ...",
            new string[] { "variable", "a variable" },
            "It is called a variable."),

        new TypedQuestion("Which word shows text on the screen?",
            new string[] { "print" },
            "PRINT shows text on the screen."),

        new TypedQuestion("Which word makes a choice?",
            new string[] { "if" },
            "IF makes a choice."),

        new TypedQuestion("Code that repeats many times is called a ...",
            new string[] { "loop", "a loop" },
            "It is called a loop."),

        new TypedQuestion("What turns high-level code into machine code?",
            new string[] { "compiler", "a compiler", "the compiler" },
            "A compiler turns high-level code into machine code."),

        new TypedQuestion("Name one high-level language from the lesson.",
            new string[] { "python", "java" },
            "Python and Java are high-level languages."),
    };

    private int currentQuestion = 0;
    private int score = 0;
    private bool answered = false;

    private void Awake()
    {
        answerButton.onClick.AddListener(CheckAnswer);
        nextButton.onClick.AddListener(NextQuestion);
        retryButton.onClick.AddListener(StartQuiz);
        continueButton.onClick.AddListener(Close);
        closeButton.onClick.AddListener(Close);

        panel.SetActive(false);
    }

    public void Open()
    {
        panel.SetActive(true);
        StartQuiz();
    }

    public void Close()
    {
        panel.SetActive(false);
        onQuizClosed?.Invoke();
    }

    private void Update()
    {
        if (panel.activeSelf == false || currentQuestion >= questions.Length)
        {
            return;
        }

        // Enter answers the question, then Enter again goes to the next one
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            if (answered == false)
            {
                CheckAnswer();
            }
            else
            {
                NextQuestion();
            }
        }
    }

    private void StartQuiz()
    {
        currentQuestion = 0;
        score = 0;
        retryButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        TypedQuestion q = questions[currentQuestion];
        answered = false;

        progressText.text = "Question " + (currentQuestion + 1) + " / " + questions.Length + "      Score: " + score;
        questionText.text = q.question;
        feedbackText.text = "Type your answer and press Enter";
        feedbackText.color = Color.white;

        answerInput.gameObject.SetActive(true);
        answerButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(false);

        // Empty the box and put the cursor in it, so the player can type right away
        answerInput.text = "";
        answerInput.interactable = true;
        answerInput.ActivateInputField();
    }

    private void CheckAnswer()
    {
        if (answered == true)
        {
            return;
        }

        // Nothing typed yet
        string typed = answerInput.text.Trim().ToLower();
        if (typed == "")
        {
            feedbackText.text = "Type an answer first!";
            answerInput.ActivateInputField();
            return;
        }

        answered = true;
        answerInput.interactable = false;
        answerButton.gameObject.SetActive(false);

        TypedQuestion q = questions[currentQuestion];

        if (IsRightAnswer(typed, q))
        {
            score = score + 1;
            SoundManager.PlayCorrect();
            feedbackText.text = "Correct! " + q.explanation;
            feedbackText.color = new Color(0.6f, 0.95f, 0.6f);
        }
        else
        {
            SoundManager.PlayWrong();
            feedbackText.text = "Not quite. " + q.explanation;
            feedbackText.color = new Color(1f, 0.6f, 0.55f);
        }

        progressText.text = "Question " + (currentQuestion + 1) + " / " + questions.Length + "      Score: " + score;
        nextButton.gameObject.SetActive(true);
    }

    private bool IsRightAnswer(string typed, TypedQuestion q)
    {
        foreach (string rightAnswer in q.rightAnswers)
        {
            if (typed == rightAnswer.ToLower())
            {
                return true;
            }
        }
        return false;
    }

    private void NextQuestion()
    {
        currentQuestion = currentQuestion + 1;

        if (currentQuestion < questions.Length)
        {
            ShowQuestion();
        }
        else
        {
            ShowResult();
        }
    }

    private void ShowResult()
    {
        answerInput.gameObject.SetActive(false);
        answerButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);

        progressText.text = "Final score: " + score + " / " + questions.Length;

        bool passed = score >= passScore;
        if (passed == true)
        {
            questionText.text = "You passed Phase 4: High-Level Languages!";
            feedbackText.text = "Great job! You can read high-level code.";
            feedbackText.color = new Color(0.95f, 0.8f, 0.35f);
        }
        else
        {
            questionText.text = "Not yet! You need " + passScore + " right answers to pass.";
            feedbackText.text = "Read the lesson again (press L) and try again.";
            feedbackText.color = new Color(1f, 0.6f, 0.55f);
            retryButton.gameObject.SetActive(true);
        }

        continueButton.gameObject.SetActive(true);
        onQuizFinished?.Invoke(passed);
    }
}
