using UnityEngine;

namespace LccHotfix
{
    public class BattleGameplayDemoMono : MonoBehaviour
    {
        [SerializeField]
        private bool _autoStart = true;

        [SerializeField]
        private float _maxDurationSeconds = 60f;

        [SerializeField]
        private bool _autoFinishWhenSideDead = true;

        private ECGameWorld _world;

        private void Start()
        {
            if (_autoStart)
            {
                StartDemo();
            }
        }

        private void Update()
        {
            _world?.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void LateUpdate()
        {
            _world?.LateUpdate();
        }

        private void OnDrawGizmos()
        {
            _world?.Gizmos();
        }

        private void OnDestroy()
        {
            StopDemo();
        }

        [ContextMenu("Start Demo")]
        public void StartDemo()
        {
            StopDemo();

            var creationInfo = new ECGameWorldCreationInfo
            {
                DemoMaxDurationSeconds = _maxDurationSeconds,
                DemoAutoFinishWhenSideDead = _autoFinishWhenSideDead
            };

            _world = ECGameWorld.CreateWorld(creationInfo);
            BattleLog.Debug("BattleGameplayDemoMono started demo world");
        }

        [ContextMenu("Stop Demo")]
        public void StopDemo()
        {
            if (_world == null)
            {
                return;
            }

            _world.DestroyWorlds();
            _world = null;
        }
    }
}
