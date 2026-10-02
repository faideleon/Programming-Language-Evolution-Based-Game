using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestOpenedHint : MonoBehaviour
{
    [SerializeField] private GameObject chest_UI;
    [SerializeField] private Button backButton;
    
    private void OnEnable()
    {
        backButton.onClick.AddListener(()=>chest_UI.SetActive(false));
        PlayerChest.firstKey += showHint;
    }
    private void OnDisable()
    {
        PlayerChest.firstKey -= showHint;
    }

    private void showHint()
    {
        chest_UI.SetActive(true);
    }
}
