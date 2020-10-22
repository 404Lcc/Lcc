using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace LccModel
{
    public class Manager : Singleton<Manager>
    {
        public Hashtable assemblys = new Hashtable();
        public void InitManager()
        {
            foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assemblys.ContainsKey(item.ManifestModule.Name))
                {
                    assemblys.Add(Path.GetFileNameWithoutExtension(item.ManifestModule.Name), item);
                }
            }
            UIEventManager.Instance.Publish(UIEventType.Launch);
        }
        public Assembly GetAssembly(string name)
        {
            return (Assembly)assemblys[name];
        }
        public Type[] GetAssemblyTypes(string name)
        {
            return GetAssembly(name).GetTypes();
        }
    }
}