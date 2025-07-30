using System.Collections.Generic;
using System;

namespace LccHotfix
{
    internal class BTScriptManager : Module, IBTScriptService
    {
        private Dictionary<string, BTScript> _scriptDict = new Dictionary<string, BTScript>();

        public BTScriptManager()
        {
            foreach (Type item in Main.CodeTypesService.GetTypes(typeof(BTScriptAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(BTScriptAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    var obj = Activator.CreateInstance(item) as BTScript;
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

        public BTScript GetScript(string name)
        {
            _scriptDict.TryGetValue(name, out var obj);
            return obj;
        }
    }
}