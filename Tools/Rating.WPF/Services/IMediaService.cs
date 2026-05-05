using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using System;
using System.Collections.Generic;

namespace Rating.WPF.Services
{
    public interface IMediaService : IDisposable
    {
        MediaPlayer MediaPlayer { get; }

        string MediaFilename { get; }
        List<TrackDescription> AudioTracks { get; }
        TrackDescription? SelectedAudioTrack { get; set; }

        void OpenMediaFile();
        void SeekTo(TimeSpan position);
        void SetMute(bool mute);
        void SetPause(bool pause);
    }
}