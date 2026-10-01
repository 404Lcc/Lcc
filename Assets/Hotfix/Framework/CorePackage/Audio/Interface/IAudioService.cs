using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public interface IAudioService : IService
    {
        bool IsEditor { get; }

        IAudioStereo Stereo { get; }

        AudioMetaConfig Meta { get; }

        void Init();

        void Register(bool isEditor = false);

        void Play(string bankName, string eventName);

        void Play(string eventName, Vector3 pos);

        void Pause();

        void Resume();

        void Clean();

        AudioEvent CreateEvent(string bankName, string eventName);

#if UNITY_EDITOR
        AudioEvent CreateEvent(AudioEventConfig config);
#endif

        List<AudioEvent> GetEvents(string eventType);

        AudioEvent GetEventById(int eventId);

        void DestroyEvent(AudioEvent audioEvent);

        void DestroyEvent(int eventId);

        void DestroyAllEvent();

        int NewEventId();

        void LoadBank(string bankName);

        IEnumerator LoadBankAsync(string bankName, Action callback = null);

        void UnloadBank(string bankName);

        void InitTrack();

        void DisposeTrack();

        AudioTrack GetTrack(string trackName);

        void MixerToSnapshot(string trackName, float time);

        void Mute(string trackName);

        void UnMute(string trackName);

        void MuteAll();

        void UnMuteAll();

        void SetStereo(IAudioStereo stereo);

        AudioController GetController(AudioEvent audioEvent, AudioTrack track = null);

        void RemoveController();

        void RemoveController(int count);

        List<string> GetAllEventTypes();
    }
}