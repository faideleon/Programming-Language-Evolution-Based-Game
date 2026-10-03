using UnityEngine;

// Plays the background music and all sound effects.
// Put this on the SOUND_MANAGER object and drag the sounds in the Inspector.
//
// Other scripts play a sound with one line, for example:
//   SoundManager.PlayPickup();
public class SoundManager : MonoBehaviour
{
    // The one SoundManager in the scene, so other scripts can use it
    private static SoundManager instance;

    [Header("Music")]
    [SerializeField] private AudioClip music;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.3f;

    [Header("Sound effects")]
    [SerializeField] [Range(0f, 1f)] private float effectsVolume = 0.7f;
    [SerializeField] private AudioClip pickup;
    [SerializeField] private AudioClip grab;
    [SerializeField] private AudioClip drop;
    [SerializeField] private AudioClip lantern;
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip correct;
    [SerializeField] private AudioClip wrong;
    [SerializeField] private AudioClip chestOpen;
    [SerializeField] private AudioClip phaseComplete;

    private AudioSource musicSource;
    private AudioSource effectsSource;

    private void Awake()
    {
        instance = this;

        // One speaker for the music (loops forever) and one for the sound effects
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();

        effectsSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        GameManager.onPhaseChanged += OnPhaseChanged;
    }

    private void OnDisable()
    {
        GameManager.onPhaseChanged -= OnPhaseChanged;
    }

    // A little tune every time a new phase starts
    private void OnPhaseChanged(int phase)
    {
        PlayPhaseComplete();
    }

    // ---------------- Music ----------------

    // Used by the ending video, which has its own music
    public static void PauseMusic()  { if (instance != null) instance.musicSource.Pause(); }
    public static void ResumeMusic() { if (instance != null) instance.musicSource.UnPause(); }

    // ---------------- Sounds other scripts can play ----------------

    // (if there is no SoundManager in the scene, nothing happens)
    public static void PlayPickup()        { if (instance != null) instance.PlayEffect(instance.pickup); }
    public static void PlayGrab()          { if (instance != null) instance.PlayEffect(instance.grab); }
    public static void PlayDrop()          { if (instance != null) instance.PlayEffect(instance.drop); }
    public static void PlayLantern()       { if (instance != null) instance.PlayEffect(instance.lantern); }
    public static void PlayClick()         { if (instance != null) instance.PlayEffect(instance.click); }
    public static void PlayCorrect()       { if (instance != null) instance.PlayEffect(instance.correct); }
    public static void PlayWrong()         { if (instance != null) instance.PlayEffect(instance.wrong); }
    public static void PlayChestOpen()     { if (instance != null) instance.PlayEffect(instance.chestOpen); }
    public static void PlayPhaseComplete() { if (instance != null) instance.PlayEffect(instance.phaseComplete); }

    private void PlayEffect(AudioClip clip)
    {
        if (clip != null)
        {
            effectsSource.PlayOneShot(clip, effectsVolume);
        }
    }
}
