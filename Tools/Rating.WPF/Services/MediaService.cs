using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Rating.WPF.Services
{
    public class MediaService : IMediaService
    {
        private readonly LibVLC _libVLC;

        public MediaPlayer MediaPlayer { get; private set; }

        public string MediaFilename { get; private set; }

        public List<TrackDescription> AudioTracks { get; private set; } = new();

        private TrackDescription? _selectedAudioTrack;
        public TrackDescription? SelectedAudioTrack
        {
            get => _selectedAudioTrack;
            set
            {
                _selectedAudioTrack = value;
                if (value != null)
                    MediaPlayer.SetAudioTrack(value.Value.Id);
            }
        }

        public MediaService()
        {
            Core.Initialize();
            _libVLC = new LibVLC();
        }

        public void OpenMediaFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Video/audio Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.mp3;*.wav;*.flac"
            };

            if (dialog.ShowDialog() != true)
                return;

            // Dispose previous player
            MediaPlayer?.Stop();
            MediaPlayer?.Dispose();

            // Create new Media object
            var media = new Media(_libVLC, new Uri(dialog.FileName));

            // Create new MediaPlayer EXACTLY like your old code
            MediaPlayer = new MediaPlayer(media);

            // Attach Playing event EXACTLY like your old code
            MediaPlayer.Playing += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AudioTracks = MediaPlayer.AudioTrackDescription.ToList();
                });
            };

            MediaPlayer.SetSpu(-1);
            MediaPlayer.Play();

            MediaFilename = dialog.FileName;
        }

        public void SeekTo(TimeSpan position)
        {
            if (MediaPlayer?.Media != null)
                MediaPlayer.Time = (long)position.TotalMilliseconds;
        }

        public void SetMute(bool mute)
        {
            if (MediaPlayer?.Media != null)
                MediaPlayer.Mute = mute;
        }

        public void SetPause(bool pause)
        {
            if (MediaPlayer?.Media != null)
            {
                if (pause)
                    MediaPlayer.Pause();
                else
                    MediaPlayer.Play();
            }
        }

        public void Dispose()
        {
            MediaPlayer?.Stop();
            MediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }
    }
}