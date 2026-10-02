using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Put this on the statue.
// After the lesson, walking to the statue opens the Wordle test (ancient programming words only).
// Solving it completes phase 1 and changes the statue's color.
public class StatueTest : MonoBehaviour
{
    [SerializeField] private Color passedColor = new Color(1f, 0.85f, 0.3f); // gold
    [SerializeField] private float colorSpeed = 2f;

    // Other scripts can listen to this
    public static event Action onPhaseOneComplete;

    private SpriteRenderer statueSprite;
    private GameObject player;

    private bool lessonFinished = false;
    private bool testOpen = false;
    private bool testPassed = false;

    private void Start()
    {
        statueSprite = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        AncientLessonScript.onLessonFinished += OnLessonFinished;
        WordleGame.onWordleFinished += OnWordleFinished;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        AncientLessonScript.onLessonFinished -= OnLessonFinished;
        WordleGame.onWordleFinished -= OnWordleFinished;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Update()
    {
        // Slowly change the statue to the new color after passing
        if (testPassed == true)
        {
            statueSprite.color = Color.Lerp(statueSprite.color, passedColor, colorSpeed * Time.deltaTime);
        }
    }

    private void OnLessonFinished()
    {
        lessonFinished = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (lessonFinished == true && testPassed == false && testOpen == false)
            {
                player = collision.gameObject;
                OpenTest();
            }
        }
    }

    private void OpenTest()
    {
        testOpen = true;
        SetPlayerControl(false);

        // Only use the ancient programming words for phase 1
        WordleGame.categoryFilter = "Ancient";
        SceneManager.LoadScene("Wordle", LoadSceneMode.Additive);
    }

    private void OnWordleFinished(bool solved)
    {
        if (testOpen == true && solved == true)
        {
            testPassed = true;
            Debug.Log("Phase 1 complete: Wordle solved");
            onPhaseOneComplete?.Invoke();
        }
    }

    // Called when the Wordle is closed (CONTINUE or X)
    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "Wordle")
        {
            testOpen = false;
            SetPlayerControl(true);
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
