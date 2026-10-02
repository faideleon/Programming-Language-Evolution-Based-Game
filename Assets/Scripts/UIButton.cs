using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public enum UIButtonType
{
    Next,
    Back,
    Cancel,
    Start
}

// Wraps a Unity Button so you can set its type, text and click action from code.
//
// Example:
//   UIButton myButton = new UIButton(nextButton, UIButtonType.Next, "NEXT", OnNextClicked);
public class UIButton
{
    public Button button;
    public TMP_Text label;
    public UIButtonType type;

    public UIButton(Button button, UIButtonType type, string text, UnityAction onClick)
    {
        this.button = button;
        this.label = button.GetComponentInChildren<TMP_Text>(true);
        this.type = type;

        SetText(text);

        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }
    }

    // Changes the type and the text, for example NEXT -> START
    public void SetType(UIButtonType newType, string newText)
    {
        type = newType;
        SetText(newText);
    }

    public void SetText(string newText)
    {
        // Some buttons are only an image and have no text
        if (label != null)
        {
            label.text = newText;
        }
    }

    public void SetVisible(bool visible)
    {
        button.gameObject.SetActive(visible);
    }
}
