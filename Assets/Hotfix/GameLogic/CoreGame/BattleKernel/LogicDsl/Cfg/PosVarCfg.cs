using System;
using UnityEngine;

namespace LccHotfix
{
    public struct PosVarCfg
    {
        public string VarKey { get; private set; }
        public string BindPoint { get; set; }
        public Func<LogicEntity, bool> ClosestPointChecker { get; private set; }
        public string LogStr => $"VarKey:{VarKey}_BindPoint={BindPoint}";
        public bool HasBindPoint => !string.IsNullOrEmpty(BindPoint);

        public PosVarCfg(string varName)
        {
            VarKey = varName;
            BindPoint = null;
            ClosestPointChecker = null;
        }

        public PosVarCfg(string varName, string bindPoint)
        {
            VarKey = varName;
            BindPoint = bindPoint;
            ClosestPointChecker = null;
        }

        public PosVarCfg WithClosestPointOnBounds(Func<LogicEntity, bool> checker)
        {
            ClosestPointChecker = checker;
            return this;
        }

        public bool GetVector3(CustomNode node, out Vector3 pos, bool logError = true)
        {
            if (node.HasVar<Vector3>(VarKey))
            {
                pos = node.GetVar<Vector3>(VarKey);
                return true;
            }

            var entity = EntityVarCfg.GetEntity(node, VarKey, logError);
            if (entity == null)
            {
                if (logError)
                {
                    CLHelper.LogError(node, $"PosVarCfg.GetVector3 entity == null, VarKey={VarKey}");
                }

                pos = Vector3.zero;
                return false;
            }

            if (HasBindPoint)
            {
                var tf = entity.GetMainViewBindTransform(BindPoint);
                if (tf != null)
                {
                    pos = tf.position;
                    return true;
                }
            }

            if (!entity.hasComTransform)
            {
                if (logError)
                {
                    CLHelper.LogError(node, $"PosVarCfg.GetVector3 entity.hasComTransform = false, VarKey={VarKey}");
                }

                pos = Vector3.zero;
                return false;
            }

            if (ClosestPointChecker != null && ClosestPointChecker(entity))
            {
                var owner = node.GetOwnerEntity();
                if (owner != null && owner.hasComTransform)
                {
                    pos = owner.position + owner.position.ClosestDistance(entity, 999);
                    return true;
                }
            }

            pos = entity.position;
            return true;
        }

        public Transform GetTransform(CustomNode node, bool logError = true)
        {
            var entity = EntityVarCfg.GetEntity(node, VarKey, logError);
            if (entity == null)
            {
                if (logError)
                {
                    CLHelper.LogError(node, $"PosVarCfg.GetEntityTF entity == null, VarKey={VarKey}");
                }

                return null;
            }

            if (HasBindPoint)
            {
                var tf = entity.GetMainViewBindTransform(BindPoint);
                if (tf != null)
                {
                    return tf;
                }
            }

            var mainTf = entity.GetEntityTf();
            if (mainTf == null && logError)
            {
                CLHelper.LogError(node, $"PosVarCfg.GetEntityTF mainTf == null, VarKey={VarKey}");
            }

            return mainTf;
        }
    }
}