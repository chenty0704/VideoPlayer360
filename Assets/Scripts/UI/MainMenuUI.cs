using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The UI for the main menu.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MainMenuUI : MonoBehaviour {
    private const int CursorBlinkIntervalMs = 500;

    private ApplicationManager _appManager;

    private TextField _urlTextField;
    private Label _notification;

    private void Awake() {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _urlTextField = root.Q<TextField>("URLTextField");
        _urlTextField.schedule.Execute(() => _urlTextField.ToggleInClassList("text-field__cursor--hide"))
            .Every(CursorBlinkIntervalMs);
        _notification = root.Q<Label>("Notification");

        _urlTextField.RegisterCallback<NavigationSubmitEvent>(_ => {
            _appManager.URL = _urlTextField.text;
            _appManager.OnPlay();
        }, TrickleDown.TrickleDown);
        _notification.RegisterCallback<TransitionEndEvent>(_ =>
            _notification.RemoveFromClassList("notification--show"));
    }

    private void Start() {
        _appManager = ApplicationManager.Instance;
        _appManager.ErrorReceived += OnErrorReceived;

        if (!String.IsNullOrEmpty(_appManager.URL))
            _urlTextField.SetValueWithoutNotify(_appManager.URL);
    }

    private void OnDestroy() {
        _appManager.ErrorReceived -= OnErrorReceived;
    }

    private void OnErrorReceived(string message) {
        _notification.text = message;
        _notification.RemoveFromClassList("notification--show");
        _notification.schedule.Execute(() => _notification.AddToClassList("notification--show")).StartingIn(5);
    }
}
