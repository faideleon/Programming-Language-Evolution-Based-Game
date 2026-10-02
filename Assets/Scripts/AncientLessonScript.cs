using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AncientLessonScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button button;


     private UIDialog dialog;

    private void Start()
    {
        string[] messages = new string[3];
        messages[0] = "Before assembly language, programming required physically configuring hardware through plugboards, toggle switches, and patterned holes punched into paper cards.";
        messages[1] = "ioneers like Ada Lovelace designed early paper-based algorithms, while mid-century engineers manually routed electrical paths and flipped switches to execute calculations.";
        messages[2] = "These raw binary and mechanical methods work well as vintage game puzzles, where players fix ancient machinery by rewiring patch panels or aligning physical punch cards.";

        dialog = new UIDialog(panel, text, button, null, messages);
    }

    
}
