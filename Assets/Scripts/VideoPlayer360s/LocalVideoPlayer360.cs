using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Plays a local 360 video.
/// </summary>
[AddComponentMenu("Video/Local Video Player 360")]
public class LocalVideoPlayer360 : AbstractVideoPlayer360 {
    private static readonly EventHandler PlayOnPrepareCompletedOnce = videoPlayer360 => {
        videoPlayer360.PlayImmediate();
        videoPlayer360.PrepareCompleted -= PlayOnPrepareCompletedOnce;
    };

    private static readonly EventHandler PlayOnSeekCompletedOnce = videoPlayer360 => {
        videoPlayer360.PlayImmediate();
        videoPlayer360.SeekCompleted -= PlayOnSeekCompletedOnce;
    };

    private readonly VideoPlayer[] _videoPlayers = new VideoPlayer[FaceCount];
    private int _prepareCompletedCount;
    private int _seekCompletedCount;

    public override string URL {
        get => base.URL;
        set {
            base.URL = value;
            for (var i = 0; i < FaceCount; ++i) _videoPlayers[i].url = Path.Combine(value, $"Face{i + 1}.mp4");
        }
    }

    public override Renderer[] Renderers {
        get => base.Renderers;
        set {
            if (value.Length != FaceCount) throw new ArgumentException("Invalid number of renderers.");
            base.Renderers = value;
            for (var i = 0; i < FaceCount; ++i) _videoPlayers[i].targetMaterialRenderer = value[i];
        }
    }

    public override float FrameRate => _videoPlayers.First().frameRate;

    public override int FrameCount => Convert.ToInt32(_videoPlayers.First().frameCount);

    public override bool IsPrepared => _videoPlayers.All(it => it.isPrepared);

    public override bool IsPlaying => _videoPlayers.All(it => it.isPlaying);

    public override int Frame {
        get => Convert.ToInt32(_videoPlayers.First().frame);
        set {
            IsBusy = true;
            if (IsPlaying) {
                Pause();
                SeekCompleted += PlayOnSeekCompletedOnce;
            }
            foreach (var videoPlayer in _videoPlayers) videoPlayer.frame = value;
        }
    }

    private void Awake() {
        var videoPlayerObjects = new GameObject[FaceCount];
        for (var i = 0; i < FaceCount; ++i) videoPlayerObjects[i] = new GameObject($"VideoPlayer{i}");
        foreach (var videoPlayerObject in videoPlayerObjects) videoPlayerObject.transform.parent = gameObject.transform;
        for (var i = 0; i < FaceCount; ++i) _videoPlayers[i] = videoPlayerObjects[i].AddComponent<VideoPlayer>();

        foreach (var videoPlayer in _videoPlayers) {
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            videoPlayer.timeReference = VideoTimeReference.InternalTime;

            videoPlayer.prepareCompleted += _ => {
                if (++_prepareCompletedCount < _videoPlayers.Length) return;
                IsBusy = false;
                InvokePrepareCompleted(this);
                _prepareCompletedCount = 0;
            };
            videoPlayer.seekCompleted += _ => {
                if (++_seekCompletedCount < _videoPlayers.Length) return;
                IsBusy = false;
                InvokeSeekCompleted(this);
                _seekCompletedCount = 0;
            };
        }

        if (!String.IsNullOrEmpty(URL)) URL = URL;
        if (Renderers != null) Renderers = Renderers;
    }

    public override void Prepare() {
        IsBusy = true;
        foreach (var videoPlayer in _videoPlayers) videoPlayer.Prepare();
    }

    public override void Play() {
        if (!IsPrepared) {
            PrepareCompleted += PlayOnPrepareCompletedOnce;
            Prepare();
        } else PlayImmediate();
    }

    public override void PlayImmediate() {
        foreach (var videoPlayer in _videoPlayers) videoPlayer.Play();
    }

    public override void Pause() {
        foreach (var videoPlayer in _videoPlayers) videoPlayer.Pause();
    }
}
