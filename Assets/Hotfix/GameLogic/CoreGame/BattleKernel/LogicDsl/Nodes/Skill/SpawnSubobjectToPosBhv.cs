using System;
using System.Xml;
using PBConfig;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace LccHotfix
{

    
    public class SpawnSubobjectToPosBhvCfg : ICustomNodeCfg
    {
        // 生成子物体的配置Id
        public IntCfg Tid { get; protected set; }
        
        public PosVarCfg InitPos { get; set;}

        public System.Type NodeType() { return typeof(SpawnSubobjectToPosBhv); }

        public float InitAngle { get; set;} = 0f;
        public float AngleAcc { get; set;} = 0f;
        public int Count { get; set;} = 1;

        public Action<CustomNode, VarEnv> SubobjVarInitFuc { get; set; }

        public SpawnSubobjectToPosBhvCfg(){}

        public SpawnSubobjectToPosBhvCfg(int tid)
        {
            Tid = new IntCfg(tid);
        }
        public SpawnSubobjectToPosBhvCfg(string varTid)
        {
            Tid = new IntCfg(varTid);
        }

        public SpawnSubobjectToPosBhvCfg WithCount(int count)
        {
            this.Count = count;
            return this;
        }
        public SpawnSubobjectToPosBhvCfg WithInitAngle(float angle)
        {
            this.InitAngle = angle;
            return this;
        }
        public SpawnSubobjectToPosBhvCfg WithAngleAcc(float angle)
        {
            this.AngleAcc = angle;
            return this;
        }

        public SpawnSubobjectToPosBhvCfg WithSubobjVar(Action<CustomNode, VarEnv> initVarEnvAction)
        {
            SubobjVarInitFuc = initVarEnvAction;
            return this;
        }
    }
    
    
    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class SpawnSubobjectToPosBhv : BehaviorNode<SpawnSubobjectToPosBhvCfg>
    {
        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
        }
        
        public override void Destroy()
        {
            base.Destroy();
        }

        protected override void OnBegin()
        {
            base.OnBegin();
            Spawn();
        }

        private void Spawn()
        {
            var tid = (uint)_cfg.Tid.GetValue(this);
            var initPosSuccess = _cfg.InitPos.GetVector3(this, out var initPos);
            if (!initPosSuccess)
            {
                return;
            }

            for (int i = 0; i < _cfg.Count; i++)
            {
                var varEnv = this.GetLogicWorld().GetCreationInfo<BattleKernelCreationInfo>().CustomLogicService.NewVarEnv();
                _cfg.SubobjVarInitFuc?.Invoke(this, varEnv);
                var e = this.CreateSubObject(tid, initPos, varEnv);
            }

        }
    }
}
