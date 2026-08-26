using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace LccHotfix
{
    public class SpawnSubobjectToTargetBhvCfg : ICustomNodeCfg
    {
        // 生成子物体的配置Id
        public IntCfg Tid { get; protected set; }

        public PosVarCfg TargetPos { get; set; }

        public PosVarCfg InitPos { get; set; }

        public System.Type NodeType()
        {
            return typeof(SpawnSubobjectToTargetBhv);
        }

        public FloatCfg InitAngle { get; set; }
        public FloatCfg AngleAcc { get; set; }
        public bool AngleSigned { get; set; }
        public IntCfg Count { get; set; } = new IntCfg(1);


        //并排齐射
        public IntCfg MultiAbreastCount { get; set; } //并排额外数量
        public float MultiAbreastDistance { get; set; } //并排间距

        public SpawnSubobjectToTargetBhvCfg(int tid)
        {
            Tid = new IntCfg(tid);
        }

        public SpawnSubobjectToTargetBhvCfg(string varTid)
        {
            Tid = new IntCfg(varTid);
        }

        public SpawnSubobjectToTargetBhvCfg SetCount(int count)
        {
            this.Count.ResetDefaultValue(count);
            return this;
        }

        public SpawnSubobjectToTargetBhvCfg SetCount(string countVar)
        {
            this.Count.SetVarID(countVar);
            return this;
        }

        public SpawnSubobjectToTargetBhvCfg WithInitAngle(float angle)
        {
            this.InitAngle = new FloatCfg(angle);
            return this;
        }

        public SpawnSubobjectToTargetBhvCfg WithInitAngle(string angleVar)
        {
            this.InitAngle = new FloatCfg(angleVar);
            return this;
        }

        public SpawnSubobjectToTargetBhvCfg WithAngleAcc(float angleAcc)
        {
            this.AngleAcc = new FloatCfg(angleAcc);
            return this;
        }

        public SpawnSubobjectToTargetBhvCfg WithAngleAcc(string angleAccVar)
        {
            this.AngleAcc = new FloatCfg(angleAccVar);
            return this;
        }

        public SpawnSubobjectToTargetBhvCfg WithAngleSigned(bool signed)
        {
            this.AngleSigned = signed;
            return this;
        }

        //并排的子弹
        public SpawnSubobjectToTargetBhvCfg WithMultiAbreast(string multiVar, float distance)
        {
            MultiAbreastCount = new IntCfg(multiVar);
            MultiAbreastDistance = distance;
            return this;
        }

        public SpawnSubobjectToTargetBhvCfg WithMulti(int multi, float distance)
        {
            MultiAbreastCount = new IntCfg(multi);
            MultiAbreastDistance = distance;
            return this;
        }

        public Action<CustomNode, VarEnv> SubobjVarInitFuc { get; set; }

        public SpawnSubobjectToTargetBhvCfg WithSubobjVar(Action<CustomNode, VarEnv> initVarEnvAction)
        {
            SubobjVarInitFuc = initVarEnvAction;
            return this;
        }

        public float InitPosPullToCenterDistance { get; set; }

        public SpawnSubobjectToTargetBhvCfg WithInitPosPullToCenter(float distance)
        {
            InitPosPullToCenterDistance = distance;
            return this;
        }
    }


    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class SpawnSubobjectToTargetBhv : BehaviorNode<SpawnSubobjectToTargetBhvCfg>
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
            bool success = _cfg.TargetPos.GetVector3(this, out var targetPos, false);
            if (!success)
            {
                return;
            }

            var tid = (uint)_cfg.Tid.GetValue(this);
            var initPosSuccess = _cfg.InitPos.GetVector3(this, out var initPos);
            if (!initPosSuccess)
            {
                return;
            }

            // sunxy: 不要这样强行拉到同一水平面，还有抛物线弹道呢
            //targetPos.y = initPos.y;

            if (_cfg.InitAngle != null)
            {
                var initAngle = _cfg.InitAngle.GetValue(this);
                targetPos = RotateTargetPosition(initPos, targetPos, initAngle);
            }

            var targetEntity = EntityVarCfg.GetEntity(this, _cfg.TargetPos.VarKey, false);
            long targetEntityID = 0;
            if (targetEntity != null)
            {
                targetEntityID = targetEntity.ID;
            }


            var multiAbreastCountAdd = _cfg.MultiAbreastCount?.GetValue(this) ?? 0;
            var multiDistance = _cfg.MultiAbreastDistance;
            var count = _cfg.Count.GetValue(this);
            var angleAcc = 0f;
            if (_cfg.AngleAcc != null)
            {
                angleAcc = _cfg.AngleAcc.GetValue(this);
            }

            for (int i = 0; i < count; i++)
            {
                var dir = targetPos - initPos;

                if (multiAbreastCountAdd == 0)
                {
                    var varEnv = NewSubobjVarEnv(targetPos, targetEntityID);
                    var e = this.CreateSubObject(tid, initPos, varEnv);
                }
                else
                {
                    // 并排多发处理
                    var multiNormalDir = GetGroundSideDir(dir); // 发射方向的 XZ 横向量
                    var abreastCount = multiAbreastCountAdd + 1;
                    for (var n = 0; n < abreastCount; n++)
                    {
                        var spawnOffset = multiNormalDir * (multiDistance * ((abreastCount - 1) / 2.0f - n));
                        var varEnvMulti = NewSubobjVarEnv(targetPos + spawnOffset, targetEntityID);
                        var e = this.CreateSubObject(tid, initPos + spawnOffset, varEnvMulti);
                    }
                }

                if (Mathf.Abs(angleAcc) > 0.0001f)
                {
                    int sign = 1;
                    if (_cfg.AngleSigned)
                    {
                        sign = count % 2 == 0 ? -1 : 1;
                    }

                    targetPos = RotateTargetPosition(initPos, targetPos, sign * angleAcc);
                }
            }

        }

        private VarEnv NewSubobjVarEnv(Vector3 targetPos, long targetEntityID)
        {
            var varEnv = Main.CustomLogicService.NewVarEnv();
            varEnv.WriteVar(CvKey.CV_SbjTargetPos, targetPos);
            if (targetEntityID != 0)
            {
                varEnv.WriteVar<long>(CvKey.CV_TargetEid, targetEntityID);
            }

            if (_cfg.SubobjVarInitFuc != null)
            {
                _cfg.SubobjVarInitFuc(this, varEnv);
            }

            return varEnv;
        }


        public Vector3 RotateTargetPosition(Vector3 initPos, Vector3 targetPos, float angle)
        {
            // 计算从initPos指向targetPos的方向向量
            Vector3 dir = targetPos - initPos;
            // 创建围绕Y轴（Vector3.up）旋转的四元数
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            // 旋转方向向量
            Vector3 rotatedDir = rotation * dir;
            // 计算新的目标位置
            Vector3 newTargetPos = initPos + rotatedDir;

            return newTargetPos;
        }

        private static Vector3 GetGroundSideDir(Vector3 dir)
        {
            var forward = new Vector3(dir.x, 0f, dir.z);
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            return Vector3.Cross(Vector3.up, forward.normalized).normalized;
        }
    }
}