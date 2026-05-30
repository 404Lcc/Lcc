using System;

namespace LccHotfix
{
    public class SetVarOnceWithFuncCfg : ICustomNodeCfg
    {
        public string Key;
        public NodeParamAction Action;

        public SetVarOnceWithFuncCfg(string key, NodeParamAction action)
        {
            Key = key;
            Action = action;
        }
        
        public System.Type NodeType() { return typeof(SetVarOnceWithFunc); }
    }

    public class SetVarOnceWithFunc : BehaviorNode<SetVarOnceWithFuncCfg>
    {
        protected override void OnBegin()
        {
            base.OnBegin();
            var key = _cfg.Key;
            if (HasVar<bool>(key))
            {
                if (!GetVar<bool>(key))
                {
                    SetVarWithFunc(key);
                }
            }
            else
            {
                SetVarWithFunc(key);
            }
        }

        private void SetVarWithFunc(string key)
        {
            SetVar<bool>(key, true);
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
