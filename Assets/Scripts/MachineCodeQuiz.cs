using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// One question of the quiz
[Serializable]
public class QuizQuestion
{
    public string question;
    public string[] answers = new string[4];
    // Which answer is right: 0 = first, 1 = second, 2 = third, 3 = fourth
    public int correctAnswer;
    // Shown after the player answers
    public string explanation;

    public QuizQuestion(string question, string answer1, string answer2, string answer3, string answer4, int correctAnswer, string explanation)
    {
        this.question = question;
        this.answers = new string[] { answer1, answer2, answer3, answer4 };
        this.correctAnswer = correctAnswer;
        this.explanation = explanation;
    }
}

// Phase 2 test: a quiz about the machine code lesson.
// Put this on the MACHINE_CODE_QUIZ object. MachineCodePhase calls Open().
public class MachineCodeQuiz : MonoBehaviour
{
    // Called at the end of the quiz. true = passed
    public static event Action<bool> onQuizFinished;
    // Called when the quiz window is closed
    public static event Action onQuizClosed;

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button[] answerButtons = new Button[4];
    [SerializeField] private Button nextButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private int passScore = 5;

    [SerializeField] private Color normalColor = new Color32(70, 72, 76, 255);
    [SerializeField] private Color rightColor = new Color32(83, 141, 78, 255);
    [SerializeField] private Color wrongColor = new Color32(160, 60, 55, 255);

    // Every question comes from the lesson
    [SerializeField] private QuizQuestion[] questions =
    {
        new QuizQuestion("What numbers do computers understand?",
            "Words", "Only 0 and 1", "Only 1 to 10", "Pictures", 1,
            "Computers only understand 0 and 1."),

        new QuizQuestion("What is one 0 or 1 called?",
            "A bit", "A byte", "An opcode", "A lantern", 0,
            "One 0 or 1 is called a bit."),

        new QuizQuestion("How many bits make a byte?",
            "4", "2", "8", "16", 2,
            "8 bits make a byte."),

        new QuizQuestion("Code made of 0s and 1s is called...",
            "English", "A password", "A picture", "Machine code", 3,
            "Code made of 0s and 1s is machine code."),

        new QuizQuestion("What is 0101?",
            "101", "5", "3", "10", 1,
            "0101 = 4 + 1 = 5."),

        new QuizQuestion("A number that tells the computer what to do is called...",
            "An opcode", "A bit", "A byte", "A lantern", 0,
            "It is called an opcode."),

        // If you change the answer in MachineCodePhase, change this question too
        new QuizQuestion("The lanterns say 0000 1011. What number is that?",
            "1011", "13", "11", "16", 2,
            "0000 1011 = 8 + 2 + 1 = 11."),
    };

    private int currentQuestion = 0;
    private int score = 0;
    private bool answered = false;

    private void Awake()
    {
        answerButtons[0].onClick.AddListener(ChooseAnswer1);
        answerButtons[1].onClick.AddListener(ChooseAnswer2);
        answerButtons[2].onClick.AddListener(ChooseAnswer3);
        answerButtons[3].onClick.AddListener(ChooseAnswer4);
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
        if (panel.activeSelf == false)
        {
            return;
        }

        // Keys 1 to 4 also pick an answer
        if (Keyboard.current.digit1Key.wasPressedThisFrame) Choose(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) Choose(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) Choose(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) Choose(3);
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
        QuizQuestion q = questions[currentQuestion];
        answered = false;

        progressText.text = "Question " + (currentQuestion + 1) + " / " + questions.Length + "      Score: " + score;
        questionText.text = q.question;
        feedbackText.text = "Pick an answer (or press 1-4)";
        feedbackText.color = Color.white;
        nextButton.gameObject.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            answerButtons[i].gameObject.SetActive(true);
            answerButtons[i].GetComponent<Image>().color = normalColor;
            answerButtons[i].GetComponentInChildren<TMP_Text>().text = (i + 1) + ".  " + q.answers[i];
        }
    }

    private void ChooseAnswer1() { Choose(0); }
    private void ChooseAnswer2() { Choose(1); }
    private void ChooseAnswer3() { Choose(2); }
    private void ChooseAnswer4() { Choose(3); }

    private void Choose(int answer)
    {
        // Only one answer per question
        if (answered == true || currentQuestion >= questions.Length)
        {
            return;
        }
        answered = true;

        QuizQuestion q = questions[currentQuestion];

        // Always show the right answer in green
        answerButtons[q.correctAnswer].GetComponent<Image>().color = rightColor;

        if (answer == q.correctAnswer)
        {
            score = score + 1;
            feedbackText.text = "Correct! " + q.explanation;
            feedbackText.color = new Color(0.6f, 0.95f, 0.6f);
        }
        else
        {
            answerButtons[answer].GetComponent<Image>().color = wrongColor;
            feedbackText.text = "Not quite. " + q.explanation;
            feedbackText.color = new Color(1f, 0.6f, 0.55f);
        }

        progressText.text = "Question " + (currentQuestion + 1) + " / " + questions.Length + "      Score: " + score;
        nextButton.gameObject.SetActive(true);
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
        for (int i = 0; i < 4; i++)
        {
            answerButtons[i].gameObject.SetActive(false);
        }
        nextButton.gameObject.SetActive(false);

        progressText.text = "Final score: " + score + " / " + questions.Length;

        bool passed = score >= passScore;
        if (passed == true)
        {
            questionText.text = "You passed Phase 2: Machine Code!";
            feedbackText.text = "Great job! You can read machine code.";
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
