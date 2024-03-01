using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The UI for a video player.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class VideoPlayerUI : MonoBehaviour {
    private const int PanelTimeoutMs = 2_000;
    private const int LoadingOverlayDelayMs = 100;

    /// <summary>
    /// The 360 video player controller.
    /// </summary>
    [field: SerializeField]
    public VideoPlayer360Controller Controller { get; set; }

    [field: SerializeField, HideInInspector] private VectorImage playIcon;
    [field: SerializeField, HideInInspector] private VectorImage pauseIcon;
    [field: SerializeField, HideInInspector] private VectorImage enterFullScreenIcon;
    [field: SerializeField, HideInInspector] private VectorImage exitFullScreenIcon;

    private ApplicationManager _appManager;

    private VisualElement _panel;

    private SliderInt _timeSlider;
    private VisualElement _timeSliderDragger;
    private VisualElement _timeSliderFill;
    private Label _timeSliderOverlay;

    private Button _playPauseButton;
    private Label _timeLabel;
    private Button _fullScreenButton;

    private VisualElement _playPauseOverlay;
    private VisualElement _playPauseOverlayIcon;
    private VisualElement _seekForwardOverlay;
    private VisualElement _seekBackwardOverlay;
    private VisualElement _loadingOverlay;
    private VisualElement _progressOverlay;

    private IVisualElementScheduledItem _hidePanel;
    private IVisualElementScheduledItem _showLoadingOverlay;

    private float _frameRate;
    private int _frameCount;

    private void Awake() {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _panel = root.Q<VisualElement>("Panel");

        _timeSlider = _panel.Q<SliderInt>("TimeSlider");
        _timeSliderDragger = _timeSlider.Q<VisualElement>("unity-dragger");
        _timeSliderFill = new VisualElement {pickingMode = PickingMode.Ignore};
        _timeSliderFill.AddToClassList("time-slider__fill");
        _timeSliderDragger.Add(_timeSliderFill);
        _timeSliderOverlay = _timeSlider.Q<Label>("Overlay");

        var actionRow = _panel.Q<VisualElement>("ActionRow");
        var leftGroup = actionRow.Q<VisualElement>("LeftGroup");
        var rightGroup = actionRow.Q<VisualElement>("RightGroup");
        _playPauseButton = leftGroup.Q<Button>("PlayPauseButton");
        _timeLabel = leftGroup.Q<Label>("TimeLabel");
        _fullScreenButton = rightGroup.Q<Button>("FullScreenButton");

        var actionOverlays = root.Q<VisualElement>("ActionOverlays");
        _playPauseOverlay = actionOverlays.Q<VisualElement>("PlayPauseOverlay");
        _playPauseOverlayIcon = _playPauseOverlay.Q<VisualElement>("Icon");
        _seekForwardOverlay = actionOverlays.Q<VisualElement>("SeekForwardOverlay");
        _seekBackwardOverlay = actionOverlays.Q<VisualElement>("SeekBackwardOverlay");
        _loadingOverlay = root.Q<VisualElement>("LoadingOverlay");
        _progressOverlay = _loadingOverlay.Q<VisualElement>("ProgressOverlay");

        _panel.RegisterCallback<MouseEnterEvent>(_ => {
            _hidePanel?.Pause();
            _panel.style.opacity = 1f;
        });
        _panel.RegisterCallback<MouseLeaveEvent>(_ =>
            _hidePanel = _panel.schedule.Execute(() => _panel.style.opacity = 0f).StartingIn(PanelTimeoutMs));
        _hidePanel = _panel.schedule.Execute(() => _panel.style.opacity = 0f).StartingIn(PanelTimeoutMs);

        _timeSlider.RegisterCallback<ChangeEvent<int>>(evt => Controller.OnSeek(evt.newValue));
        _timeSlider.RegisterCallback<MouseEnterEvent>(_ => _timeSliderOverlay.style.opacity = 1f);
        _timeSlider.RegisterCallback<MouseMoveEvent>(evt => {
            var frame = evt.localMousePosition.x * _appManager.DisplayScaling / 1900f * _frameCount;
            _timeSliderOverlay.text = FormatTime(frame / _frameRate);
            _timeSliderOverlay.style.translate = new Translate(evt.localMousePosition.x - 30f, 0f);
        });
        _timeSlider.RegisterCallback<MouseOutEvent>(_ => _timeSliderOverlay.style.opacity = 0f);

        _playPauseButton.RegisterCallback<ClickEvent>(_ => Controller.OnPlayPause());
        _fullScreenButton.RegisterCallback<ClickEvent>(_ => _appManager.OnToggleFullScreen());
        OnFullScreenChanged(Screen.fullScreen);

        foreach (var overlay in new[] {_playPauseOverlay, _seekForwardOverlay, _seekBackwardOverlay})
            overlay.RegisterCallback<TransitionEndEvent>(_ => overlay.RemoveFromClassList("action-overlay--show"));
        _progressOverlay.RegisterCallback<TransitionEndEvent>(_ => {
            _progressOverlay.RemoveFromClassList("progress-overlay--rotate");
            _progressOverlay.schedule.Execute(() =>
                _progressOverlay.AddToClassList("progress-overlay--rotate")).StartingIn(5);
        });

        Controller.FrameRateChanged += OnFrameRateChanged;
        Controller.FrameCountChanged += OnFrameCountChanged;
        Controller.BusyStateChanged += OnBusyStateChanged;
        Controller.FrameChanged += OnFrameChanged;
        Controller.PlayPauseTriggered += OnPlayPause;
        Controller.SeekBackwardTriggered += OnSeekBackward;
        Controller.SeekForwardTriggered += OnSeekForward;
    }

    private void Start() {
        _appManager = ApplicationManager.Instance;
        _appManager.FullScreenChanged += OnFullScreenChanged;
    }

    private void OnDestroy() {
        _appManager.FullScreenChanged -= OnFullScreenChanged;
    }

    private void OnFullScreenChanged(bool fullScreen) {
        _fullScreenButton.style.backgroundImage =
            new StyleBackground(fullScreen ? exitFullScreenIcon : enterFullScreenIcon);
    }

    private void OnFrameRateChanged(float frameRate) {
        _frameRate = frameRate;
    }

    private void OnFrameCountChanged(int frameCount) {
        _frameCount = frameCount;
        var text = _timeLabel.text;
        var index = text.LastIndexOf(' ');
        _timeLabel.text = text[..(index + 1)] + FormatTime(_frameCount / _frameRate);
        _timeSlider.highValue = _frameCount - 1;
    }

    private void OnBusyStateChanged(bool isBusy) {
        if (isBusy) {
            _showLoadingOverlay = _loadingOverlay.schedule.Execute(() => {
                _loadingOverlay.style.display = DisplayStyle.Flex;
                _progressOverlay.AddToClassList("progress-overlay--rotate");
            }).StartingIn(LoadingOverlayDelayMs);
        } else {
            _showLoadingOverlay.Pause();
            _loadingOverlay.style.display = DisplayStyle.None;
            _progressOverlay.RemoveFromClassList("progress-overlay--rotate");
        }
    }

    private void OnFrameChanged(int frame) {
        var text = _timeLabel.text;
        var index = text.IndexOf(' ');
        _timeLabel.text = FormatTime(frame / _frameRate) + text[index..];
        _timeSlider.SetValueWithoutNotify(frame);
    }

    private void OnPlayPause(bool isPlaying) {
        _playPauseButton.style.backgroundImage = new StyleBackground(isPlaying ? pauseIcon : playIcon);
        _playPauseOverlayIcon.style.backgroundImage = new StyleBackground(isPlaying ? playIcon : pauseIcon);
        _playPauseOverlayIcon.style.translate = new Translate(isPlaying ? 2.5f : 0f, 0f);
        ShowActionOverlay(_playPauseOverlay);
    }

    private void OnSeekBackward() {
        ShowActionOverlay(_seekBackwardOverlay);
    }

    private void OnSeekForward() {
        ShowActionOverlay(_seekForwardOverlay);
    }

    private static string FormatTime(float seconds) {
        var value = Convert.ToInt32(seconds);
        int min = value / 60, sec = value % 60;
        return $"{min}:{sec:D2}";
    }

    private static void ShowActionOverlay(VisualElement overlay) {
        overlay.RemoveFromClassList("action-overlay--show");
        overlay.schedule.Execute(() => overlay.AddToClassList("action-overlay--show")).StartingIn(5);
    }
}
