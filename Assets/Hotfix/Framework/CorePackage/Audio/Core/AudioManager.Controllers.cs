using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public partial class AudioManager
    {
        private const int _audioSourceInitCount = 20;
        private readonly List<AudioController> _audioControllers = new();
        private readonly Queue<AudioController> _freeAudioControllers = new();

        private void InitController()
        {
            for (var i = 0; i < _audioSourceInitCount; i++)
            {
                AddController();
            }
        }

        private AudioController AddController()
        {
            var gameObject = new GameObject
            {
                name = "Audio Source"
            };
            gameObject.transform.SetParent(AudioRoot.transform);
            gameObject.AddComponent<AudioSource>();
            gameObject.SetActive(false);
            var audioController = new AudioController(gameObject);
            _audioControllers.Add(audioController);
            return audioController;
        }

        public AudioController GetController(AudioEvent audioEvent, AudioTrack track = null)
        {
            if (_freeAudioControllers.Count == 0)
            {
                return AddController();
            }
            var audioController = _freeAudioControllers.Dequeue();
            _audioControllers.Add(audioController);
            if (track != null)
            {
                audioController.Mixer = track.Mixer;
            }
            audioController.SetEvent(audioEvent);
            return audioController;
        }

        public void RemoveController()
        {
            if (_freeAudioControllers.Count == 0)
            {
                return;
            }
            var controller = _freeAudioControllers.Dequeue();
            controller.Recycle();
            if (IsEditor)
            {
                Object.DestroyImmediate(controller.GameObject);
            }
            else
            {
                Object.Destroy(controller.GameObject);
            }
            
        }

        public void RemoveController(int count)
        {
            if (_freeAudioControllers.Count == 0)
            {
                return;
            }
            for (var i = 0; i < count; i++)
            {
                RemoveController();
            }
        }

        private void UpdateController()
        {
            // 手写压实替代 List.RemoveAll：删除项入空闲池、保留项前移，语义与 RemoveAll 一致，
            // 但不需要把实例方法组转换成委托（那会每次调用分配一个委托对象）
            var count = _audioControllers.Count;
            var write = 0;
            for (var read = 0; read < count; read++)
            {
                var audioController = _audioControllers[read];
                if (audioController.GameObject is null)
                {
                    continue;
                }
                if (audioController.IsActive)
                {
                    audioController.Update();
                    _audioControllers[write++] = audioController;
                    continue;
                }
                _freeAudioControllers.Enqueue(audioController);
            }
            // 截掉尾部空槽，RemoveRange 会顺手清空这些引用
            _audioControllers.RemoveRange(write, count - write);

            if (_freeAudioControllers.Count > _audioSourceInitCount)
            {
                RemoveController(_freeAudioControllers.Count + _audioControllers.Count - _audioSourceInitCount);
            }
        }

        private void DisposeController()
        {
            foreach (var controller in _audioControllers)
            {
                controller.Clear();
            }
            _audioControllers.Clear();
            foreach (var controller in _freeAudioControllers)
            {
                controller.Clear();
            }
            _freeAudioControllers.Clear();
        }

    }
}