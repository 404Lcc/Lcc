
using UnityEngine;
using UnityEngine.Audio;

namespace LccHotfix
{
    /// <summary>
    /// AudioController 是AudioPlayer的再封装，用来扩展静音状态、淡入淡出等稍复杂的控制
    /// </summary>
    public class AudioController
    {
        public GameObject GameObject { get; private set; } 
        public AudioSource AudioSource { get; private set; }
        public AudioEvent AudioEvent { get; private set; }

        private TimeTransitionFloat _volume = TimeTransitionFloat.NewVolume();
        public float Volume
        {
            get => _volume;
            set
            {
                _volume.Value = value;
                AudioSource.volume = _volume.Value;
            }
        }

        private TimeTransitionFloat _panning = TimeTransitionFloat.NewPanning();
        public float Panning
        {
            get => _panning;
            set
            {
                _panning.Value = value;
                AudioSource.panStereo = _panning.Value;
            }
        }

        public float VolumeTransition
        {
            get => _volume.Transition;
            set => _volume.Transition = value;
        }

        public float PanningTransition
        {
            get => _panning.Transition;
            set => _panning.Transition = value;
        }

        public float Pitch
        {
            get => AudioSource.pitch;
            set => AudioSource.pitch = value;
        }

        public Vector3 Pos
        {
            get
            {
                if (AudioSource.spatialize)
                {
                    return AudioSource.transform.position;
                }
                return Vector3.zero;
            }
            set
            {
                AudioSource.spatialize = true;
                AudioSource.transform.position = value;
            }
        }

        public AudioMixerGroup Mixer
        {
            get => AudioSource.outputAudioMixerGroup;
            set => AudioSource.outputAudioMixerGroup = value;
        }

        public int Priority
        {
            get => AudioSource.priority;
            set => AudioSource.priority = value;
        }

        public bool IsPlaying => AudioSource != null && AudioSource.isPlaying;
        public bool IsPaused { get; private set; }


        public bool IsActive => IsPlaying || IsPaused;

        public AudioController(GameObject obj)
        {
            Init(obj);
        }

        public void Init(GameObject obj)
        {
            GameObject = obj;
            AudioSource = GameObject.GetComponent<AudioSource>();
            IsPaused = false;
        }

        public void SetClip(AudioClip clip)
        {
            AudioSource.clip = clip;
        }
        
        public void SetEvent(AudioEvent audioEvent)
        {
            AudioEvent = audioEvent;
        }

        public void Play(bool isLoop = false, float clipVolume = 1.0f, float clipPanning = 0.0f)
        {
            if (AudioSource.clip is null)
            {
                return;
            }
            AudioSource.loop = isLoop;
            _volume.ForceSet(clipVolume);
            _panning.ForceSet(clipPanning);
            IsPaused = false;
            AudioSource.volume = _volume;
            AudioSource.panStereo = _panning;
            GameObject.SetActive(true);
            AudioSource.Play();
        }

        public void SetLoop(bool isLoop)
        {
            AudioSource.loop = isLoop;
        }

        public void PlayOneShot(AudioClip clip, float clipVolume = 1.0f, float clipPanning = 0.0f)
        {
            AudioSource.loop = false;
            AudioSource.clip = clip;
            _volume.ForceSet(clipVolume);
            _panning.ForceSet(clipPanning);
            IsPaused = false;
            AudioSource.Play();
        }

        public void PlayLoop(AudioClip clip, float clipVolume = 1.0f, float clipPanning = 0.0f)
        {
            AudioSource.loop = true;
            AudioSource.clip = clip;
            _volume.ForceSet(clipVolume);
            _panning.ForceSet(clipPanning);
            IsPaused = false;
            AudioSource.Play();
        }

        public void Stop()
        {
            AudioSource?.Stop();
            IsPaused = false;
        }

        public int GetProgress()
        {
            return AudioSource.timeSamples;
        }

        public void SetProgress(int samples)
        {
            if (samples >= AudioSource.clip.samples)
            {
                if (AudioSource.loop)
                {
                    samples /=AudioSource.clip.samples;
                }
                else
                {
                    Stop();
                    return;
                }
            }
            AudioSource.timeSamples = samples;
        }

        public float GetTime()
        {
            return AudioSource.time;
        }

        public void SetTime(float time)
        {
            if (time >= AudioSource.clip.length)
            {
                if (AudioSource.loop)
                {
                    
                }
                else
                {
                    Stop();
                    return;
                }
            }
            AudioSource.time = time;
        }

        public void Update()
        {
            if (GameObject is null)
            {
                Recycle();
                return;
            }
            if (!GameObject.activeSelf)
            {
                return;
            }

            if (IsPaused)
            {
                return;
            }
            
            _volume.Update();
            _panning.Update();
            if (AudioSource is not null)
            {
                AudioSource.volume = _volume;
                AudioSource.panStereo = _panning;
            }
        }

        public void Pause()
        {
            if (AudioSource is null)
            {
                return;
            }
            IsPaused = true;
            AudioSource.Pause();
        }
        
        public void Resume()
        {
            if (AudioSource is null)
            {
                return;
            }
            IsPaused = false; 
            AudioSource.UnPause();
        }

        public void Clear()
        {
            if (AudioSource is not null)
            {
                AudioSource.spatialize = false;
                AudioSource.transform.position = Vector3.zero;
                AudioSource.outputAudioMixerGroup = null;
                AudioSource.clip = null;
                AudioSource.pitch = 1.0f;
                AudioSource.loop = false;
                AudioSource.Stop();
            }
            AudioEvent = null;
            IsPaused = false;
            GameObject.SetActive(false);
        }

        public void Recycle()
        {
            Clear();
        }
    }
}

