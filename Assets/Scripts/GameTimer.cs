using Mono.Cecil;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [SerializeField]private GameObject playerChest;
    private PlayerChest playerChestScript;
    public static event Action ShowFirstHint;
    private bool showFirstHint;


    private void Awake()
    {
        playerChestScript = playerChest.GetComponent<PlayerChest>();
    }

    private void OnEnable()
    {
        
        MenuManager.onGameStarted += PasstoCoroutine;
    }

    private void PasstoCoroutine()
    {
        
            StartCoroutine(TimerRoutine());
       
    }

    private IEnumerator TimerRoutine()
    {
        Debug.Log("Invoking");
        yield return new WaitForSeconds(5);
        if (!playerChestScript.chestHasBeenOpened)
        {
            ShowFirstHint?.Invoke();
        }
    }

    private void OnDisable()
    {
        MenuManager.onGameStarted -= PasstoCoroutine;
    }
}
