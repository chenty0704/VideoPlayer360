using System;
using UnityEngine;

/// <summary>
/// Represents an abstract 360 video player.
/// </summary>
public abstract class AbstractVideoPlayer360 : MonoBehaviour {
    /// <summary>
    /// Delegate type for all parameterless events.
    /// </summary>
    public delegate void EventHandler(AbstractVideoPlayer360 videoPlayer360);

    /// <summary>
    /// The number of faces on a cubemap.
    /// </summary>
    protected const int FaceCount = 6;

    /// <summary>
    /// The folder or HTTP URL of the video.
    /// </summary>
    [field: SerializeField]
    public virtual string URL { get; set; }

    /// <summary>
    /// The renderers that will receive images.
    /// </summary>
    [field: SerializeField]
    public virtual Renderer[] Renderers { get; set; }

    private bool _isBusy;

    /// <summary>
    /// The frame rate of the video.
    /// </summary>
    public abstract float FrameRate { get; }

    /// <summary>
    /// The length of the video in seconds.
    /// </summary>
    public float LengthSeconds => FrameCount / FrameRate;

    /// <summary>
    /// The number of frames in the video.
    /// </summary>
    public abstract int FrameCount { get; }

    /// <summary>
    /// Whether the 360 video player is busy.
    /// </summary>
    public bool IsBusy {
        get => _isBusy;
        protected set {
            if (value == _isBusy) return;
            _isBusy = value;
            BusyStateChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Whether the video has been successfully prepared.
    /// </summary>
    public abstract bool IsPrepared { get; }

    /// <summary>
    /// Whether the video is being played.
    /// </summary>
    public abstract bool IsPlaying { get; }

    /// <summary>
    /// The time of the current frame in seconds.
    /// </summary>
    public float Seconds {
        get => Frame / FrameRate;
        set => Frame = Convert.ToInt32(value / FrameRate);
    }

    /// <summary>
    /// The index of the current frame.
    /// </summary>
    public abstract int Frame { get; set; }

    /// <summary>
    /// Invoked when playback preparation is completed.
    /// </summary>
    public event EventHandler PrepareCompleted;

    /// <summary>
    /// Invoked when a seek is completed.
    /// </summary>
    public event EventHandler SeekCompleted;

    /// <summary>
    /// Invoked when the busy state is changed.
    /// </summary>
    public event EventHandler BusyStateChanged;

    /// <summary>
    /// Initiates playback preparation.
    /// </summary>
    public abstract void Prepare();

    /// <summary>
    /// Starts playback after proper playback preparation.
    /// </summary>
    public abstract void Play();

    /// <summary>
    /// Starts playback immediately.
    /// </summary>
    public abstract void PlayImmediate();

    /// <summary>
    /// Pauses playback.
    /// </summary>
    public abstract void Pause();

    /// <summary>
    /// Invokes the prepare completed event.
    /// </summary>
    protected void InvokePrepareCompleted(AbstractVideoPlayer360 videoPlayer360) {
        PrepareCompleted?.Invoke(videoPlayer360);
    }

    /// <summary>
    /// Invokes the seek completed event.
    /// </summary>
    protected void InvokeSeekCompleted(AbstractVideoPlayer360 videoPlayer360) {
        SeekCompleted?.Invoke(videoPlayer360);
    }
}
