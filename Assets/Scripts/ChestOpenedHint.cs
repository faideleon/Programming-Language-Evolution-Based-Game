using UnityEngine;
using UnityEngine.UI;

public class ChestOpenedHint : MonoBehaviour
{
    [SerializeField] private GameObject chest_UI;
    [SerializeField] private Button backButton;

    private UIDialog hint;

    private void Awake()
    {
        hint = new UIDialog(chest_UI, backButton);
    }

    private void OnEnable()
    {
        PlayerChest.firstKey += showHint;
    }

    private void OnDisable()
    {
        PlayerChest.firstKey -= showHint;
    }

    private void showHint()
    {
        hint.Show();
    }
}
