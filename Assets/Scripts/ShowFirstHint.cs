using UnityEngine;
using UnityEngine.UI;

public class ShowFirstHint : MonoBehaviour
{
    [SerializeField] private GameObject gameManager;
    [SerializeField] private GameObject UI;
    [SerializeField] private Button backButton;


    private void Start()
    {
        backButton.onClick.AddListener(() => UI.SetActive(false));
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
        Debug.Log("Invoked");
        UI.SetActive(true);
       
    }
}
