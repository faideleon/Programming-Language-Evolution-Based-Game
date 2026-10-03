using System;
using UnityEngine;

// Keeps track of which phase the player is on.
//   Phase 1: Ancient Programming Languages
//   Phase 2: Machine Code
//   Phase 3: Assembly Language
//   Phase 4: coming soon
//
// Other scripts read GameManager.currentPhase, or listen to onPhaseChanged
// to change their messages when a new phase starts.
public class GameManager : MonoBehaviour
{
    public static int currentPhase = 1;
    public static event Action<int> onPhaseChanged;

    [Tooltip("For testing: start the game at this phase (1 = normal start)")]
    [SerializeField] private int startAtPhase = 1;

    private void Awake()
    {
        currentPhase = 1;
    }

    private void Start()
    {
        if (startAtPhase > 1)
        {
            StartPhase(startAtPhase);
        }
    }

    public static void StartPhase(int phase)
    {
        currentPhase = phase;
        Debug.Log("Phase " + phase + " started");
        onPhaseChanged?.Invoke(phase);
    }
}
