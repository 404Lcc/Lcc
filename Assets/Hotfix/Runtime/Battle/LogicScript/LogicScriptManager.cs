using System.Collections.Generic;
using System;

namespace LccHotfix
{
    internal class LogicScriptManager : Module
    {
        private Dictionary<string, LogicScript> _scriptDict = new Dictionary<string, LogicScript>();

        public LogicScriptManager()
        {
            foreach (Type item in CodeTypesManager.Instance.GetTypes(typeof(LogicScriptAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(LogicScriptAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    var obj = Activator.CreateInstance(item) as LogicScript;
                    if (obj == null)
                    {
                        continue;
                    }
                    obj.Init();
                    _scriptDict.Add(item.Name, obj);
                }
            }
        }
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            foreach (var item in _scriptDict.Values)
            {
                item.Dispose();
            }
            _scriptDict.Clear();
        }

        public LogicScript GetScript(string name)
        {
            _scriptDict.TryGetValue(name, out var obj);
            return obj;
        }
    }
}