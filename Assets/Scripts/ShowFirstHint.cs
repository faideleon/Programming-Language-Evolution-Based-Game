using UnityEngine;
using UnityEngine.UI;

public class ShowFirstHint : MonoBehaviour
{
    [SerializeField] private GameObject UI;
    [SerializeField] private Button backButton;

    private UIDialog hint;

    private void Awake()
    {
        hint = new UIDialog(UI, backButton);
    }

    private void OnEnable()
    {
        GameTimer.ShowFirstHint += GameTimer_ShowFirstHint;
    }

    private void OnDisable()
    {
        GameTimer.ShowFirstHint -= GameTimer_ShowFirstHint;
    }

    private void GameTimer_ShowFirstHint()
    {
        hint.Show();
    }
}
