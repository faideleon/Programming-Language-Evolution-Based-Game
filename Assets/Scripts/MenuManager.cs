using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] public Button nextButton;
    [SerializeField] private GameObject menu;
    [SerializeField] private Button cancelbutton;

    // The messages, in the order they are shown
    [SerializeField] private GameObject[] pages;

    public static event Action onGameStarted;

    private UIDialog dialog;

    void Start()
    {
        // If the pages were not set (or left empty) in the Inspector, use the menu's messages
        if (pages == null || pages.Length == 0 || pages[0] == null)
        {
            pages = new GameObject[3];
            pages[0] = menu.transform.Find("HelloMessage").gameObject;
            pages[1] = menu.transform.Find("WelcomeMessage").gameObject;
            pages[2] = menu.transform.Find("StartMessage").gameObject;
        }

        // If the cancel button slot is empty in the Inspector, find it in the menu
        if (cancelbutton == null)
        {
            cancelbutton = menu.transform.Find("LargeBoard/CancelButton").GetComponent<Button>();
        }

        dialog = new UIDialog(menu, pages, nextButton, cancelbutton);
        dialog.pauseGame = true;
        dialog.showBackOnFirstPage = false;
        dialog.onStart += StartGame;

        dialog.Show();
    }

    void StartGame()
    {
        onGameStarted?.Invoke();
    }
}
