using UnityEngine;

namespace LccHotfix
{
    public class UniMapComponent : MetaComponent
    {
        private GameObjectHandle _map;
        private Camera _camera;
        public bool IsDone => _map.IsDone;

        public void Init(string mapName)
        {
            _map = Main.GameObjectPoolService.GetObjectAsync(mapName, (operation) =>
            {
                var obj = operation.GameObject.transform;
                _camera = obj.Find("Camera").GetComponent<Camera>();
                operation.Transform.position = Vector3.zero;
                Main.CameraService.AddOverlayCamera(_camera);
                Main.CameraService.SetCurrentCamera(_camera);
            });
        }

        public override void DisposeOnRemove()
        {
            if (_map.IsDone)
            {
                _camera.gameObject.GetComponent<UIAnimManager>().StopAnim();
                Main.CameraService.RemoveOverlayCamera(_camera);
                Main.CameraService.SetCurrentCamera(null);
            }

            _map.Release(ref _map);

            base.DisposeOnRemove();
        }

        public Transform GetBindPoint(string bpName)
        {
            if (!IsDone)
                return null;
            var bpTrans = _map.Transform.Find(bpName);
            return bpTrans;
        }

        internal Transform GetPlayerPoint() => GetBindPoint("Player");

        public Vector3 GetPlayerPosition()
        {
            var playerBPTrans = GetPlayerPoint();
            return playerBPTrans.transform.position;
        }
    }

    public partial class MetaWorld
    {
        public UniMapComponent comUniMap
        {
            get { return GetUniqueComponent<UniMapComponent>(MetaComponentsLookup.ComUniMap); }
        }

        public bool hasComUniMap
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniMap); }
        }

        public void SetComUniMap(string mapName)
        {
            var index = MetaComponentsLookup.ComUniMap;
            var component = (UniMapComponent)UniqueEntity.CreateComponent(index, typeof(UniMapComponent));
            component.Init(mapName);
            SetUniqueComponent(index, component);
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex _ComUniMapIndex = new(typeof(UniMapComponent));
        public static int ComUniMap => _ComUniMapIndex.Index;
    }
}