using System;
using System.Collections.Generic;

namespace LccModel
{
    public class CombatScriptManager : AObjectBase
    {
        public static CombatScriptManager Instance;

        public Dictionary<string, Type> dict = new Dictionary<string, Type>();

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
        }
        public CombatScript GetScript(string name)
        {
            var script = (CombatScript)Activator.CreateInstance(dict[name]);
            script.InitScript();
            return script;
        }
    }
}