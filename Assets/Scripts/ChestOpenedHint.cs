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
    private string phase4Hint = "You finished 3 phases!\n\nPhase 4 is coming soon";

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
    }

    private void showHint()
    {
        // In phase 2 the chest opens the machine code quiz instead
        if (MachineCodePhase.quizReady == true)
        {
            return;
        }

        hint.Show();
    }
}
