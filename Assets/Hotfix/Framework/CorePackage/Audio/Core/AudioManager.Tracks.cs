using System.Collections.Generic;
using UnityEngine.Audio;

namespace LccHotfix
{
    public partial class AudioManager
    {
        private AudioAsset<AudioMixer> _mixer;
        public readonly Dictionary<string, AudioTrack> _mixerTracks = new();

        public void InitTrack()
        {
            _mixerTracks.Clear();
            _mixer = AudioAsset<AudioMixer>.Load("GlobalMixer");
            if (_mixer.Asset is null)
            {
                Audio.LogError("[Audio] load GlobalMixer failed !!!");
                return;
            }

            RegisterTrack("Master/Music");
            RegisterTrack("Master/UI");
            RegisterTrack("Master/Combat");
            RegisterTrack("Master/Ambient");

            // if (!MusicOn)
            // {
            //     MusicTrack.SetMute(true);
            // }
            //
            // if (!SoundOn)
            // {
            //     UITrack.SetMute(true);
            //     CombatTrack.SetMute(true);
            // }
            //
            // MixerToSnapshot("Normal", 0);
        }

        public void DisposeTrack()
        {
            _mixerTracks.Clear();
            _mixer?.Unload();
            _mixer = null;
        }

        private void RegisterTrack(string path)
        {
            if (_mixer is null)
            {
                Audio.LogError("[Audio] Mixer is null, cannot register track.");
                return;
            }
            if (_mixerTracks.ContainsKey(path))
            {
                Audio.LogWarning($"[Audio] Track '{path}' is already registered.");
                return;
            }
            var track = new AudioTrack(_mixer, path);
            _mixerTracks.Add(path, track);
            track.SetVolume(1f);
        }

        public AudioTrack GetTrack(string trackName)
        {
            if (trackName is null)
            {
                return null;
            }
            if (_mixerTracks.TryGetValue(trackName, out var track))
            {
                return track;
            }
            return null;
        }

        public void MixerToSnapshot(string trackName, float time)
        {
            var snapshot = _mixer.Asset.FindSnapshot(trackName);
            if (snapshot != null)
            {
                snapshot.TransitionTo(time);
            }
        }
        
        public void Mute(string trackName)
        {
            GetTrack(trackName)?.SetMute(true);
        }

        public void UnMute(string trackName)
        {
            GetTrack(trackName)?.SetMute(false);
        }

        public void MuteAll()
        {
            foreach (var track in _mixerTracks.Values)
            {
                track.SetMute(true);
            }
        }

        public void UnMuteAll()
        {
            foreach (var track in _mixerTracks.Values)
            {
                track.SetMute(false);
            }
        }
    }
}