using UnityEngine;

namespace LccHotfix
{
    public class DemoMono : MonoBehaviour
    {
        [SerializeField]
        private bool _autoStart = true;

        [SerializeField]
        private float _maxDurationSeconds = 60f;

        [SerializeField]
        private bool _autoFinishWhenSideDead = true;

        private DemoWorld _world;

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

            var creationInfo = new DemoWorldCreationInfo
            {
                DemoMaxDurationSeconds = _maxDurationSeconds,
                DemoAutoFinishWhenSideDead = _autoFinishWhenSideDead
            };

            _world = DemoWorld.CreateWorld(creationInfo);
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
