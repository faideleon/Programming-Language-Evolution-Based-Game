using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Put this on the statue.
// Phase 1: after the ancient lesson, walking to the statue opens the Wordle (ancient words).
// Phase 3: after the assembly lesson, walking to the statue opens the Wordle (assembly words).
// Solving the Wordle passes the phase. When the Wordle is closed, the next phase starts.
public class StatueTest : MonoBehaviour
{
    [SerializeField] private Color phase1Color = new Color(1f, 0.85f, 0.3f);   // gold
    [SerializeField] private Color phase3Color = new Color(0.55f, 0.85f, 1f);  // light blue
    [SerializeField] private Color finishedColor = new Color(0.6f, 1f, 0.6f);  // green, when the game is finished
    [SerializeField] private float colorSpeed = 2f;

    // Called every time the player walks up to the statue
    public static event Action onPlayerReachedStatue;

    private SpriteRenderer statueSprite;
    private Color targetColor;
    private GameObject player;

    private bool ancientLessonDone = false;
    private bool assemblyLessonDone = false;
    private bool testOpen = false;
    private bool testPassed = false;

    private void Start()
    {
        statueSprite = GetComponent<SpriteRenderer>();
        targetColor = statueSprite.color;
    }

    private void OnEnable()
    {
        GameManager.onPhaseChanged += OnPhaseChanged;
        AncientLessonScript.onLessonFinished += OnAncientLessonFinished;
        AssemblyLessonScript.onLessonFinished += OnAssemblyLessonFinished;
        WordleGame.onWordleFinished += OnWordleFinished;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        GameManager.onPhaseChanged -= OnPhaseChanged;
        AncientLessonScript.onLessonFinished -= OnAncientLessonFinished;
        AssemblyLessonScript.onLessonFinished -= OnAssemblyLessonFinished;
        WordleGame.onWordleFinished -= OnWordleFinished;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Update()
    {
        // Slowly change the statue to its new color after passing a test
        statueSprite.color = Color.Lerp(statueSprite.color, targetColor, colorSpeed * Time.deltaTime);
    }

    // The statue turns green when the whole game is finished
    private void OnPhaseChanged(int phase)
    {
        if (phase == 5)
        {
            targetColor = finishedColor;
        }
    }

    private void OnAncientLessonFinished()
    {
        ancientLessonDone = true;
    }

    private void OnAssemblyLessonFinished()
    {
        assemblyLessonDone = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player = collision.gameObject;
            int phase = GameManager.currentPhase;

            if (phase == 1 && ancientLessonDone == true && testOpen == false)
            {
                OpenTest("Ancient");
            }

            if (phase == 3 && assemblyLessonDone == true && testOpen == false)
            {
                OpenTest("Assembly");
            }

            onPlayerReachedStatue?.Invoke();
        }
    }

    // Opens the Wordle with only the words of one lesson
    private void OpenTest(string category)
    {
        testOpen = true;
        testPassed = false;
        SetPlayerControl(false);

        WordleGame.categoryFilter = category;
        SceneManager.LoadScene("Wordle", LoadSceneMode.Additive);
    }

    private void OnWordleFinished(bool solved)
    {
        if (testOpen == true && solved == true)
        {
            testPassed = true;
            Debug.Log("Phase " + GameManager.currentPhase + " test solved");

            if (GameManager.currentPhase == 1)
            {
                targetColor = phase1Color;
            }
            else
            {
                targetColor = phase3Color;
            }
        }
    }

    // Called when the Wordle is closed (CONTINUE or X)
    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name != "Wordle")
        {
            return;
        }

        testOpen = false;
        SetPlayerControl(true);

        // Passed: start the next phase
        if (testPassed == true)
        {
            testPassed = false;
            GameManager.StartPhase(GameManager.currentPhase + 1);
        }
    }

    // Stops the player from moving, grabbing or opening chests while typing in the Wordle
    private void SetPlayerControl(bool canControl)
    {
        if (player == null)
        {
            return;
        }

        player.GetComponent<PlayerMovement>().enabled = canControl;
        player.GetComponent<PlayerGrab>().enabled = canControl;
        player.GetComponent<PlayerChest>().enabled = canControl;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }
}
