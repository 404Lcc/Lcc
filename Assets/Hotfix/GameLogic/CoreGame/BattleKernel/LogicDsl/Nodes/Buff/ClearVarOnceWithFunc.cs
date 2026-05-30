using System;

namespace LccHotfix
{
    
    public class ClearVarOnceWithFuncCfg : ICustomNodeCfg
    {
        public string Key;
        public NodeParamAction Action;

        public ClearVarOnceWithFuncCfg(string key, NodeParamAction action)
        {
            Key = key;
            Action = action;
        }
        
        public System.Type NodeType() { return typeof(ClearVarOnceWithFunc); }
    }

    public class ClearVarOnceWithFunc : BehaviorNode<ClearVarOnceWithFuncCfg>
    {
        protected override void OnBegin()
        {
            base.OnBegin();
            var key = _cfg.Key;
            if (HasVar<bool>(key))
            {
                if (GetVar<bool>(key))
                {
                    SetVarWithFunc(key);
                }
            }
        }

        private void SetVarWithFunc(string key)
        {
            SetVar<bool>(key, false);
            if (_cfg.Action != null)
                _cfg.Action(this);
        }

        public override void Destroy()
        {
            var key = _cfg.Key;
            ClearVar<bool>(key);
            base.Destroy();
        }
    }
}
