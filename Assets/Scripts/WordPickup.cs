using TMPro;
using UnityEngine;

// Put this on each glowing code word hidden on the map (there are 4).
// When the player walks over it, the word is collected.
public class WordPickup : MonoBehaviour
{
    [SerializeField] private HighLevelPhase phase;
    [SerializeField] private string word = "PRINT";
    [SerializeField] private TMP_Text wordText;

    private Vector3 wordStartPosition;

    private void Start()
    {
        wordText.text = word;
        wordStartPosition = wordText.transform.localPosition;
    }

    private void Update()
    {
        // Float up and down a little so the player notices it
        float offset = Mathf.Sin(Time.time * 3f) * 0.1f;
        wordText.transform.localPosition = wordStartPosition + new Vector3(0, offset, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            phase.OnWordFound(word);
            gameObject.SetActive(false);
        }
    }
}
