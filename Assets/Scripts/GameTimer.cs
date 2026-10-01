using Mono.Cecil;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static event Action ShowFirstHint;
    private bool showFirstHint;
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
        yield return new WaitForSeconds(20);
        ShowFirstHint?.Invoke();
    }

    private void OnDisable()
    {
        MenuManager.onGameStarted -= PasstoCoroutine;
    }
}
