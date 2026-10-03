using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

// Plays the ending video when the player finishes phase 4.
// The video's own sound is turned off, and our ending music plays instead.
// When the video ends, the last picture stays on screen until the player presses Enter.
// Put this on the ENDING_VIDEO object.
public class EndingVideo : MonoBehaviour
{
    [SerializeField] private GameObject screen;          // the black full-screen panel with the video
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource endingMusic;
    [SerializeField] private TMP_Text continueText;      // "Press Enter to continue"

    private bool waitingToStart = false;
    private bool playing = false;
    private bool finished = false;
    private GameObject player;

    private void Awake()
    {
        // Our own music instead of the video's sound
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        // The video follows the same clock as the music, so they stay together.
        // If the game freezes for a moment, the video skips frames to catch up.
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.DSPTime;
        videoPlayer.skipOnDrop = true;
        videoPlayer.loopPointReached += OnVideoEnded;

        endingMusic.playOnAwake = false;
        endingMusic.loop = false;

        screen.SetActive(false);
        continueText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        HighLevelPhase.onGameFinished += Play;
    }

    private void OnDisable()
    {
        HighLevelPhase.onGameFinished -= Play;
    }

    public void Play()
    {
        player = GameObject.FindWithTag("Player");
        SetPlayerControl(false);
        SoundManager.PauseMusic();

        screen.SetActive(true);
        continueText.gameObject.SetActive(false);

        // Load the video first, so the picture and the music start at the same moment
        videoPlayer.Prepare();
        waitingToStart = true;
        finished = false;
    }

    private void Update()
    {
        if (waitingToStart == true && videoPlayer.isPrepared == true)
        {
            waitingToStart = false;
            playing = true;
            videoPlayer.Play();
            endingMusic.Play();
        }

        if (playing == false)
        {
            return;
        }

        // Safety net: if the music and video still get far apart, move the music back in line
        if (videoPlayer.isPlaying == true && endingMusic.isPlaying == true)
        {
            float difference = Mathf.Abs(endingMusic.time - (float)videoPlayer.time);
            if (difference > 0.5f)
            {
                endingMusic.time = (float)videoPlayer.time;
            }
        }

        bool enterPressed = Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame;
        bool escapePressed = Keyboard.current.escapeKey.wasPressedThisFrame;

        // Enter after the video, or Escape at any time, closes the video
        if ((finished == true && enterPressed) || escapePressed)
        {
            Close();
        }
    }

    private void OnVideoEnded(VideoPlayer source)
    {
        finished = true;
        continueText.gameObject.SetActive(true);
    }

    private void Close()
    {
        playing = false;
        videoPlayer.Stop();
        endingMusic.Stop();
        screen.SetActive(false);

        SoundManager.ResumeMusic();
        SetPlayerControl(true);
    }

    // Stops the player from moving while the video plays
    private void SetPlayerControl(bool canControl)
    {
        player.GetComponent<PlayerMovement>().enabled = canControl;
        player.GetComponent<PlayerGrab>().enabled = canControl;
        player.GetComponent<PlayerChest>().enabled = canControl;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }
}
