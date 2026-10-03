using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shows the chest popup every time the chest is opened.
// The hint inside the chest changes when a new phase starts.
public class ChestOpenedHint : MonoBehaviour
{
    [SerializeField] private GameObject chest_UI;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text chestText;

    private UIDialog hint;

    private string phase2Hint = "Phase 2: Machine Code\n\nHint: Find the 4 glowing numbers, then set the lanterns by the statue";
    private string phase3Hint = "Phase 3: Assembly Language\n\nHint: Go to the altar and bring a stone box";
    private string phase4Hint = "Phase 4: High-Level Languages\n\nHint: Find the 4 glowing code words on the map";
    private string endHint = "You finished all 4 phases!\n\nYou are free!";

    private void Awake()
    {
        hint = new UIDialog(chest_UI, backButton);
    }

    private void OnEnable()
    {
        PlayerChest.firstKey += showHint;
        GameManager.onPhaseChanged += OnPhaseChanged;
    }

    private void OnDisable()
    {
        PlayerChest.firstKey -= showHint;
        GameManager.onPhaseChanged -= OnPhaseChanged;
    }

    // Change the hint in the chest for the new phase.
    // Phase 1 uses the text that is already in the chest.
    private void OnPhaseChanged(int phase)
    {
        if (phase == 2)
        {
            chestText.text = phase2Hint;
        }
        else if (phase == 3)
        {
            chestText.text = phase3Hint;
        }
        else if (phase == 4)
        {
            chestText.text = phase4Hint;
        }
        else if (phase == 5)
        {
            chestText.text = endHint;
        }
    }

    private void showHint()
    {
        // In phase 2 and 4 the chest opens the quiz instead
        if (MachineCodePhase.quizReady == true || HighLevelPhase.quizReady == true)
        {
            return;
        }

        hint.Show();
    }
}
