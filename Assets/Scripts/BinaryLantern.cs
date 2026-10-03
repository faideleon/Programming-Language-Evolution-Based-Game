using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Put this on each of the 4 stone lanterns around the statue.
// Stand next to a lantern and press F to change it between 0 and 1.
public class BinaryLantern : MonoBehaviour
{
    [SerializeField] private MachineCodePhase phase;
    // 1 is the first lantern, 4 is the last one
    [SerializeField] private int lanternNumber = 1;
    // The "#1" and "0" texts next to the lantern
    [SerializeField] private GameObject labels;
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text valueText;

    [SerializeField] private Color offColor = new Color(0.65f, 0.65f, 0.75f);
    [SerializeField] private Color onColor = new Color(1f, 0.9f, 0.55f);

    // The lantern's number: 0 or 1
    public int value = 0;

    // The lantern the player walked up to last. Only this one reacts to F,
    // so two lanterns next to each other never change at the same time.
    private static BinaryLantern selectedLantern;

    private SpriteRenderer lanternSprite;

    private void Awake()
    {
        lanternSprite = GetComponent<SpriteRenderer>();
        numberText.text = "#" + lanternNumber;

        // The labels only appear in phase 2
        labels.SetActive(false);
    }

    // Called by MachineCodePhase when phase 2 starts
    public void ShowLabels()
    {
        labels.SetActive(true);
        Refresh();
    }

    private void Update()
    {
        if (selectedLantern != this || phase.IsPuzzleActive() == false)
        {
            return;
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (phase.AllBitsFound() == false)
            {
                phase.ShowPrompt("Find all 4 numbers first");
                return;
            }

            // Flip between 0 and 1
            if (value == 0)
            {
                value = 1;
            }
            else
            {
                value = 0;
            }

            Refresh();
            SoundManager.PlayLantern();
            phase.OnLanternChanged();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            selectedLantern = this;

            if (phase.IsPuzzleActive())
            {
                phase.ShowPrompt("Press F to change lantern #" + lanternNumber);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // The player left another lantern but is still standing next to this one
        if (collision.gameObject.CompareTag("Player") && selectedLantern == null)
        {
            selectedLantern = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (selectedLantern == this)
            {
                selectedLantern = null;
            }
        }
    }

    // Shows the value and lights up the lantern when it is 1
    private void Refresh()
    {
        valueText.text = value.ToString();

        if (value == 1)
        {
            valueText.color = onColor;
            lanternSprite.color = onColor;
        }
        else
        {
            valueText.color = Color.white;
            lanternSprite.color = offColor;
        }
    }
}
