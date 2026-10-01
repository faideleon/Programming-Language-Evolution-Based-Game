using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    [SerializeField]public Button nextButton;
    [SerializeField]private GameObject menu;
   
    [SerializeField] private Button cancelbutton;

    public static event Action onGameStarted;
    private TMP_Text text;
    public ButtonType buttonType = ButtonType.LOADED;
    
    private TMP_Text [] menuTexts;
    private bool canReturn =false; //checks if the cancel button can return to previous message
    int countNext = 0;//Checks if the save text should appear already

    void Start()
    {
       
        text = nextButton.GetComponentInChildren<TMP_Text>();
        nextButton.onClick.AddListener(StartGame);
        menuTexts = menu.GetComponentsInChildren<TMP_Text>(true);
        cancelbutton.onClick.AddListener(BackGame);
    }

    private void Update()
    {
      
    }

   

    void BackGame()
    {
        if (canReturn)
        {
            if(buttonType == ButtonType.NEXT)
            {
                buttonType = ButtonType.LOADED;
                menuTexts[2].gameObject.SetActive(false);
                menuTexts[1].gameObject.SetActive(true);
                
                canReturn = false;
                countNext=0;
            }else if (buttonType== ButtonType.NEXT2)
            {
                buttonType = ButtonType.NEXT;
                menuTexts[3].gameObject.SetActive(false);
                menuTexts[2].gameObject.SetActive(true);
                countNext--;
                text.text = "NEXT";
            }
        }
    }



    void StartGame()
    {
        
        Time.timeScale = 0f;
        canReturn = true;
            
        
            if (text.text == "NEXT")
            {
                countNext++;    
                buttonType = ButtonType.NEXT;
                showMessage();
                

                     if (countNext == 2) {
                        buttonType= ButtonType.NEXT2;
                        showMessage();
                        text.text = "START";
                        
                        return; 
                    }
                
            }

            else if (text.text == "START")
            {
               
                Time.timeScale = 1f;
                menu.SetActive(false);
                buttonType = ButtonType.START;

            onGameStarted?.Invoke();
            }
       

    }


    void showMessage()
    {
        
        if(buttonType == ButtonType.NEXT)
        {
            menuTexts[1].gameObject.SetActive(false);
            menuTexts[2].gameObject.SetActive(true);
        }
        else if(buttonType == ButtonType.NEXT2)
        {
            menuTexts[2].gameObject.SetActive(false);
            menuTexts[3].gameObject.SetActive(true);
        }
        
        
    }
}


public enum ButtonType {
    LOADED, NEXT, NEXT2, START
}
