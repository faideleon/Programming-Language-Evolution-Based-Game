using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Phase 1 lesson about ancient programming languages.
// BoxToAltar calls Show() when the box is placed on the altar.
// When the player presses DONE on the last page, onLessonFinished is called.
public class AncientLessonScript : MonoBehaviour
{
    // Called every time the player finishes the lesson
    public static event Action onLessonFinished;

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button button;

    private UIDialog dialog;

    private void Awake()
    {
        string[] messages = new string[3];
        messages[0] = "Before assembly language, programming required physically configuring hardware through plugboards, toggle switches, and patterned holes punched into paper cards.";
        messages[1] = "Pioneers like Ada Lovelace designed early paper-based algorithms, while mid-century engineers manually routed electrical paths and flipped switches to execute calculations.";
        messages[2] = "These raw binary and mechanical methods work well as vintage game puzzles, where players fix ancient machinery by rewiring patch panels or aligning physical punch cards.";

        // Only a Next button. It says DONE on the last page and closes the lesson.
        dialog = new UIDialog(panel, text, button, null, messages, "NEXT", "BACK", "DONE", "CANCEL");
        dialog.onStart += FinishLesson;

        // The lesson stays hidden until the box is placed on the altar
        dialog.Hide();
    }

    public void Show()
    {
        dialog.Show();
    }

    public void Hide()
    {
        dialog.Hide();
    }

    private void FinishLesson()
    {
        onLessonFinished?.Invoke();
    }
}