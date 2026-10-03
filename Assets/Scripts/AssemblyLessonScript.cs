using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Phase 3 lesson about assembly language.
// BoxToAltar calls Show() when the stone box is placed on the altar.
// When the player presses DONE on the last page, onLessonFinished is called.
public class AssemblyLessonScript : MonoBehaviour
{
    // Called every time the player finishes the lesson
    public static event Action onLessonFinished;

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button button;

    private UIDialog dialog;

    private void Awake()
    {
        // Every word here can be in the phase 3 Wordle
        string[] messages = new string[5];
        messages[0] = "PHASE 3: ASSEMBLY LANGUAGE. Machine code is hard to read. So people made assembly: short words that stand for machine code.";
        messages[1] = "MOV copies a number into a register. A register is a tiny box inside the computer that holds one number.";
        messages[2] = "ADD adds two numbers. SUB subtracts one number from another.";
        messages[3] = "JMP jumps to another line of code. NOP does nothing. HALT stops the computer.";
        messages[4] = "PUSH puts a number on the STACK, like putting a plate on a pile. POP takes the top plate off again.";

        // Only a Next button. It says DONE on the last page and closes the lesson.
        dialog = new UIDialog(panel, text, button, null, messages, "NEXT", "BACK", "DONE", "CANCEL");
        dialog.onStart += FinishLesson;

        // The lesson stays hidden until the stone box is placed on the altar
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
