using TMPro;
using UnityEngine;

// Put this on each glowing number hidden on the map (there are 4).
// When the player walks over it, the number is collected.
public class BitPickup : MonoBehaviour
{
    [SerializeField] private MachineCodePhase phase;
    // Which lantern this number belongs to (1 to 4)
    [SerializeField] private int lanternNumber = 1;
    [SerializeField] private TMP_Text digitText;
    [SerializeField] private TMP_Text captionText;

    private Vector3 digitStartPosition;

    private void Start()
    {
        // The number to show comes from the answer in MachineCodePhase
        digitText.text = phase.GetBit(lanternNumber);
        captionText.text = "lantern #" + lanternNumber;

        digitStartPosition = digitText.transform.localPosition;
    }

    private void Update()
    {
        // Float up and down a little so the player notices it
        float offset = Mathf.Sin(Time.time * 3f) * 0.1f;
        digitText.transform.localPosition = digitStartPosition + new Vector3(0, offset, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            phase.OnBitFound(lanternNumber);
            gameObject.SetActive(false);
        }
    }
}
