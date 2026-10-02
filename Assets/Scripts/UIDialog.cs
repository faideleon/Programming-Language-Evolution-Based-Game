using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A panel that shows pages one at a time, with a Next and a Back button.
//
// Next button:  goes to the next page.     On the last page it says START and closes the panel.
// Back button:  goes to the previous page. On the first page it says CANCEL and closes the panel.
//
// Example:
//   UIDialog dialog = new UIDialog(panel, messageText, nextButton, backButton, messages);
//   dialog.onStart += StartGame;
//   dialog.Show();
public class UIDialog
{
    // Called when START is pressed on the last page
    public event Action onStart;
    // Called when CANCEL is pressed on the first page
    public event Action onCancel;

    public GameObject panel;
    public UIButton nextButton;
    public UIButton backButton;

    public int currentPage = 0;
    public int pageCount = 1;

    // Pauses the game while the panel is open
    public bool pauseGame = false;
    // Shows the Back button on the first page
    public bool showBackOnFirstPage = true;

    // Button texts
    public string nextText;
    public string backText;
    public string startText;
    public string cancelText;

    // Way 1: one text object, the text changes on every page
    private TMP_Text messageText;
    private string[] messages;

    // Way 2: one object per page, only the current one is turned on
    private GameObject[] pages;

    private bool isPaused = false;


    // Use this when you have ONE text and a list of messages
    public UIDialog(GameObject panel, TMP_Text messageText, Button next, Button back, string[] messages,
                    string nextText = "NEXT", string backText = "BACK", string startText = "START", string cancelText = "CANCEL")
    {
        this.messageText = messageText;
        this.messages = messages;
        this.pageCount = messages.Length;

        Setup(panel, next, back, nextText, backText, startText, cancelText);
    }

    // Use this when every page is its own object in the scene
    public UIDialog(GameObject panel, GameObject[] pages, Button next, Button back,
                    string nextText = "NEXT", string backText = "BACK", string startText = "START", string cancelText = "CANCEL")
    {
        this.pages = pages;
        this.pageCount = pages.Length;

        Setup(panel, next, back, nextText, backText, startText, cancelText);
    }

    // Use this for a simple popup with only one button that closes it
    public UIDialog(GameObject panel, Button closeButton, string closeText = "BACK")
    {
        this.pageCount = 1;

        Setup(panel, null, closeButton, "NEXT", closeText, "START", closeText);
    }


    private void Setup(GameObject panel, Button next, Button back,
                       string nextText, string backText, string startText, string cancelText)
    {
        this.panel = panel;
        this.nextText = nextText;
        this.backText = backText;
        this.startText = startText;
        this.cancelText = cancelText;

        if (next != null)
        {
            nextButton = new UIButton(next, UIButtonType.Next, nextText, Next);
        }

        if (back != null)
        {
            backButton = new UIButton(back, UIButtonType.Back, backText, Back);
        }

        UpdatePage();
    }


    public void Show()
    {
        if (pauseGame == true && isPaused == false)
        {
            Time.timeScale = 0f;
            isPaused = true;
        }

        panel.SetActive(true);
        currentPage = 0;
        UpdatePage();
    }

    public void Hide()
    {
        if (isPaused == true)
        {
            Time.timeScale = 1f;
            isPaused = false;
        }

        panel.SetActive(false);
    }

    public void Next()
    {
        if (IsLastPage())
        {
            Hide();
            onStart?.Invoke();
        }
        else
        {
            currentPage = currentPage + 1;
            UpdatePage();
        }
    }

    public void Back()
    {
        if (IsFirstPage())
        {
            Hide();
            onCancel?.Invoke();
        }
        else
        {
            currentPage = currentPage - 1;
            UpdatePage();
        }
    }

    // Changes the text of one page, for example SetMessage(0, "New text")
    public void SetMessage(int page, string newText)
    {
        messages[page] = newText;
        UpdatePage();
    }

    public bool IsFirstPage()
    {
        return currentPage == 0;
    }

    public bool IsLastPage()
    {
        return currentPage == pageCount - 1;
    }


    // Shows the current page and sets the button texts
    private void UpdatePage()
    {
        // Way 1: change the text
        if (messageText != null && messages.Length > 0)
        {
            messageText.text = messages[currentPage];
        }

        // Way 2: turn on only the current page
        if (pages != null)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                bool isCurrentPage = (i == currentPage);
                pages[i].SetActive(isCurrentPage);
            }
        }

        // Next button: START on the last page, NEXT on the others
        if (nextButton != null)
        {
            if (IsLastPage())
            {
                nextButton.SetType(UIButtonType.Start, startText);
            }
            else
            {
                nextButton.SetType(UIButtonType.Next, nextText);
            }
        }

        // Back button: CANCEL on the first page, BACK on the others
        if (backButton != null)
        {
            if (IsFirstPage())
            {
                backButton.SetType(UIButtonType.Cancel, cancelText);
                backButton.SetVisible(showBackOnFirstPage);
            }
            else
            {
                backButton.SetType(UIButtonType.Back, backText);
                backButton.SetVisible(true);
            }
        }
    }
}
