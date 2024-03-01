using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls a 360 video player and interacts with the input and UI.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class VideoPlayer360Controller : MonoBehaviour {
    private const float SeekSeconds = 5f;

    /// <summary>
    /// The screen manager.
    /// </summary>
    [field: SerializeField]
    public ScreenManager ScreenManager { get; set; }

    /// <summary>
    /// The speed of viewport rotation.
    /// </summary>
    [field: SerializeField, Range(0.001f, 2f)]
    public float RotationSpeed { get; set; } = 0.25f;

    private ApplicationManager _appManager;

    private AbstractVideoPlayer360 _videoPlayer360;
    private int _prevFrame;

    private Camera _mainCamera;
    private Vector3 _cameraEulerAngles;

    private void Start() {
        _appManager = ApplicationManager.Instance;
        if (_appManager.PlaybackMode == PlaybackMode.Local)
            _videoPlayer360 = gameObject.AddComponent<LocalVideoPlayer360>();
        else throw new NotImplementedException();
        ScreenManager.InstantiateScreens();

        _videoPlayer360.URL = _appManager.URL;
        _videoPlayer360.Renderers = ScreenManager.Renderers;
        _videoPlayer360.PrepareCompleted += OnInitialized;
        _videoPlayer360.BusyStateChanged += videoPlayer360 => BusyStateChanged?.Invoke(videoPlayer360.IsBusy);
        _videoPlayer360.Play();

        _mainCamera = Camera.main;
        _cameraEulerAngles = _mainCamera!.transform.eulerAngles;
    }

    private void Update() {
        if (!_videoPlayer360.IsBusy && _videoPlayer360.Frame != _prevFrame)
            FrameChanged?.Invoke(_prevFrame = _videoPlayer360.Frame);
    }

    /// <summary>
    /// Invoked when the frame rate is changed.
    /// </summary>
    public event Action<float> FrameRateChanged;

    /// <summary>
    /// Invoked when the frame count is changed.
    /// </summary>
    public event Action<int> FrameCountChanged;

    /// <summary>
    /// Invoked when the busy state is changed.
    /// </summary>
    public event Action<bool> BusyStateChanged;

    /// <summary>
    /// Invoked when the frame is changed.
    /// </summary>
    public event Action<int> FrameChanged;

    /// <summary>
    /// Invoked when the play/pause action is triggered.
    /// </summary>
    public event Action<bool> PlayPauseTriggered;

    /// <summary>
    /// Invoked when the seek backward action is triggered.
    /// </summary>
    public event Action SeekBackwardTriggered;

    /// <summary>
    /// Invoked when the seek forward action is triggered.
    /// </summary>
    public event Action SeekForwardTriggered;

    /// <summary>
    /// Handles the play/pause action.
    /// </summary>
    public void OnPlayPause() {
        if (_videoPlayer360.IsBusy) return;
        var isPlaying = _videoPlayer360.IsPlaying;
        if (isPlaying) _videoPlayer360.Pause();
        else _videoPlayer360.Play();
        PlayPauseTriggered?.Invoke(!isPlaying);
    }

    /// <summary>
    /// Handles the rotate action.
    /// </summary>
    /// <param name="value">The input value.</param>
    public void OnRotate(InputValue value) {
        var delta = value.Get<Vector2>();
        var eulerAngles = _cameraEulerAngles + new Vector3(-delta.y, delta.x, 0f) * RotationSpeed;
        eulerAngles.x = Mathf.Clamp(eulerAngles.x, -90f, 90f);
        _mainCamera.transform.eulerAngles = _cameraEulerAngles = eulerAngles;
    }

    /// <summary>
    /// Handles the seek action.
    /// </summary>
    /// <param name="frame">The index of the target frame.</param>
    public void OnSeek(int frame) {
        if (_videoPlayer360.IsBusy) return;
        _videoPlayer360.Frame = frame;
    }

    /// <summary>
    /// Handles the seek backward action.
    /// </summary>
    public void OnSeekBackward() {
        OnSeek(Math.Clamp(
            _videoPlayer360.Frame - Convert.ToInt32(SeekSeconds * _videoPlayer360.FrameRate),
            0, _videoPlayer360.FrameCount - 1));
        SeekBackwardTriggered?.Invoke();
    }

    /// <summary>
    /// Handles the seek forward action.
    /// </summary>
    public void OnSeekForward() {
        OnSeek(Math.Clamp(
            _videoPlayer360.Frame + Convert.ToInt32(SeekSeconds * _videoPlayer360.FrameRate),
            0, _videoPlayer360.FrameCount - 1));
        SeekForwardTriggered?.Invoke();
    }

    private void OnInitialized(AbstractVideoPlayer360 videoPlayer360) {
        FrameRateChanged?.Invoke(videoPlayer360.FrameRate);
        FrameCountChanged?.Invoke(videoPlayer360.FrameCount);
        videoPlayer360.PrepareCompleted -= OnInitialized;
    }
}
