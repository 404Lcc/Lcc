using System.Reflection;
using Mirror;

namespace LccModel
{
    public class MirrorUnitCtrl : NetworkBehaviour
    {
        public override void OnStartClient()
        {
            base.OnStartClient();

            DontDestroyOnLoad(gameObject);

            var obj = HotfixFunc.CallProperty("LccHotfix", "Main", "MirrorService", BindingFlags.Static | BindingFlags.Public);
            HotfixFunc.CallMethod("LccHotfix", "MirrorManager", "AddUnit", BindingFlags.Public | BindingFlags.Instance, new object[] { this }, obj);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            DontDestroyOnLoad(gameObject);

            var obj = HotfixFunc.CallProperty("LccHotfix", "Main", "MirrorService", BindingFlags.Static | BindingFlags.Public);
            HotfixFunc.CallMethod("LccHotfix", "MirrorManager", "RemoveUnit", BindingFlags.Public | BindingFlags.Instance, new object[] { this }, obj);
        }
    }
}