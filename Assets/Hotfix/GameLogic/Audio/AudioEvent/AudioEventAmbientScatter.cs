using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LccHotfix
{
    [AudioEventAttr]
    public class AudioEventAmbientScatter : AudioEvent
    {
        public float ScatterRate = 0.2f;
        protected List<AudioController> _controllers = new();

        public override void Update()
        {
            base.Update();

            _controllers.RemoveAll(c =>
            {
                if (!c.IsActive)
                {
                    RecycleController(c);
                    return true;
                }
                return false;
            });

            if (Random.Range(0f, 1f) < ScatterRate * Time.deltaTime)
            {
                var controller = GetController(Config.Clips[Random.Range(0, Config.Clips.Count)], "Master/Ambient");
                if (controller is null)
                {
                    return;
                }
                controller.Pitch = 1f + (Random.value * 2f - 1f) * Config.PitchShift;
                controller.Play(false, Config.Volume);
            }
        }
    }
}
