using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Represents the playback mode of the video.
/// </summary>
public enum PlaybackMode {
    /// <summary>
    /// Plays a local video.
    /// </summary>
    Local,
    /// <summary>
    /// Streams a video over the Internet.
    /// </summary>
    Streaming
}

/// <summary>
/// Manages configurations and scene transitions.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class ApplicationManager : Singleton<ApplicationManager> {
    /// <summary>
    /// The folder or HTTP URL of the video.
    /// </summary>
    [field: SerializeField]
    public string URL { get; set; }

    private int _prevScreenWidth = 1920, _prevScreenHeight = 1080;

    /// <summary>
    /// The display scaling of the screen.
    /// </summary>
    public float DisplayScaling { get; private set; }

    /// <summary>
    /// The playback mode of the video.
    /// </summary>
    public PlaybackMode PlaybackMode => !URL.StartsWith("https://") ? PlaybackMode.Local : PlaybackMode.Streaming;

    protected override void Awake() {
        base.Awake();

        DisplayScaling = Screen.dpi / 96f;
    }

    /// <summary>
    /// Invoked when the full screen state is changed.
    /// </summary>
    public event Action<bool> FullScreenChanged;

    /// <summary>
    /// Invoked when an error is received.
    /// </summary>
    public event Action<string> ErrorReceived;

    /// <summary>
    /// Handles the play action.
    /// </summary>
    public void OnPlay() {
        if (String.IsNullOrEmpty(URL))
            ErrorReceived?.Invoke("Please specify a folder or HTTP URL.");
        else if (PlaybackMode == PlaybackMode.Local && !Directory.Exists(URL))
            ErrorReceived?.Invoke("The specified folder does not exist.");
        else SceneManager.LoadScene("VideoPlayer360");
    }

    /// <summary>
    /// Handles the toggle full screen action.
    /// </summary>
    public void OnToggleFullScreen() {
        if (!Screen.fullScreen) {
            (_prevScreenWidth, _prevScreenHeight) = (Screen.width, Screen.height);
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true);
            FullScreenChanged?.Invoke(true);
        } else {
            Screen.SetResolution(_prevScreenWidth, _prevScreenHeight, false);
            FullScreenChanged?.Invoke(false);
        }
    }

    /// <summary>
    /// Handles the back to main menu action.
    /// </summary>
    public void OnBackToMainMenu() {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        SceneManager.LoadScene("MainMenu");
    }
}
