using PBConfig;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LccHotfix
{
    public partial class LogicConfigBase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandleSubobjectHitCmdCfg HandleSubobjectHit(string path, float during = 2f)
        {
            return new HandleSubobjectHitCmdCfg(path) { During = during, };
        }

        /// <summary>
        /// 从战斗单位指定挂点生成固定配置 ID 的子物体，并飞向目标位置变量指定的位置。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpawnSubobjectToTargetBhvCfg FighterSpawnSubobjectTo(int tid, string targetPosVar, string initBindPoint, string targetBindPoint = null)
        {
            var targetPos = string.IsNullOrEmpty(targetBindPoint)
                ? new PosVarCfg(targetPosVar)
                : new PosVarCfg(targetPosVar, targetBindPoint);
            return new SpawnSubobjectToTargetBhvCfg(tid) { TargetPos = targetPos, InitPos = new PosVarCfg(CvKey.CV_OwnerEntity, initBindPoint) };
        }

        /// <summary>
        /// 从战斗单位指定挂点生成变量配置 ID 的子物体，并飞向目标位置变量指定的位置。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpawnSubobjectToTargetBhvCfg FighterSpawnSubobjectTo(string tid, string targetPosVar, string initBindPoint, string targetBindPoint = null)
        {
            var targetPos = string.IsNullOrEmpty(targetBindPoint)
                ? new PosVarCfg(targetPosVar)
                : new PosVarCfg(targetPosVar, targetBindPoint);
            return new SpawnSubobjectToTargetBhvCfg(tid) { TargetPos = targetPos, InitPos = new PosVarCfg(CvKey.CV_OwnerEntity, initBindPoint) };
        }

        /// <summary>
        /// 从指定初始位置变量生成子物体，并飞向目标位置变量指定的位置。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpawnSubobjectToTargetBhvCfg SpawnSubobjectTo(int tid, string targetPosVar, string initPosVar)
        {
            return new SpawnSubobjectToTargetBhvCfg(tid) { TargetPos = new PosVarCfg(targetPosVar), InitPos = new PosVarCfg(initPosVar) };
        }

        /// <summary>
        /// 在指定位置变量处生成子物体，不额外指定飞行目标。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpawnSubobjectToPosBhvCfg SpawnSubobjectFixedPos(string tid, string initPosVar)
        {
            return new SpawnSubobjectToPosBhvCfg(tid) { InitPos = new PosVarCfg(initPosVar) };
        }

        /// <summary>
        /// 在战斗单位指定挂点处生成子物体，不额外指定飞行目标。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpawnSubobjectToPosBhvCfg FighterSpawnSubobjectFixedPos(string tid, string bpName)
        {
            return new SpawnSubobjectToPosBhvCfg(tid) { InitPos = new PosVarCfg(CvKey.CV_OwnerEntity, bpName) };
        }

        /// <summary>
        /// 创建子物体碰撞初始化节点，由调用方传入具体碰撞初始化逻辑。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg Subobject_InitCollider(NodeParamAction actionSetCollider)
        {
            return BeginCall(node => { actionSetCollider?.Invoke(node); });
        }

        /// <summary>
        /// 根据 TSubobject 配置初始化子物体 Bounds、碰撞器和命中特效变量。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg Subobject_InitColliderByPbCfg()
        {
            return BeginCall(node =>
            {
                var e = node.GetOwnerEntity();
                var boundSize = 0f;
                var tid = e.comSubobject.ConfigId;
                var subobjectCfg = PbCfg.GetData<TSubobject>((uint)tid);
                if (subobjectCfg != null)
                {
                    if (subobjectCfg.Radius != 0)
                    {
                        boundSize = subobjectCfg.Radius;
                    }

                    node.SetVar<int>(CvKey.CV_SbjHitFxRes, (int)subobjectCfg.HitFx);
                }
                else
                {
                    CLogger.LogError(node, $"Subobject_SetCollider subobjectCfg != null, tid={tid}");
                    return;
                }

                e.ReplaceComBounds(e.comTransform.position, boundSize);

                //先全走HitMakerUnityPhysics，试运行一段时间没问题看改配置表。ai别动
                switch (subobjectCfg.CollisionType)
                {
                    case CollisionType.EhtNone:
                        break;
                    case CollisionType.EhtAabb:
                        //HitMakerLogicPhysics
                        e.AddSubobjectComCollider<SubobjectColliderHandlerBase, HitMakerUnityPhysics>(e.comSubobject.Cfg, out var hitMakerLogic);
                        break;
                    case CollisionType.EhtRaycast3D:
                        e.AddSubobjectComCollider<SubobjectColliderHandlerBase, HitMakerUnityPhysics>(e.comSubobject.Cfg, out var hitMakerUnity);
                        break;
                }

            });
        }

        /// <summary>
        /// 设置子物体直线运动方向、速度和朝向，目标位置来自 CV_SbjTargetPos。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg Subobject_SetLocomotionStraightDir(float speed = -1f, bool isLevel = true)
        {
            return InitializeCall(node =>
            {
                var e = node.GetOwnerEntity();
                if (speed > 0)
                {
                    node.SetVar<float>(CvKey.CV_SbjMoveSpeed, speed);
                }

                var moveSpeed = node.GetVar<float>(CvKey.CV_SbjMoveSpeed, 10f);
                var targetPos = node.GetVar<Vector3>(CvKey.CV_SbjTargetPos);
                var initPos = e.position;
                var rawDir = targetPos - initPos;
                var dir = rawDir;
                if (isLevel)
                {
                    dir.y = 0;
                }
                if (dir.sqrMagnitude > 0.0001f)
                    dir = dir.normalized;
                else if (rawDir.sqrMagnitude > 0.0001f)
                    dir = rawDir.normalized;
                else
                    dir = e.comTransform.rotation * Vector3.forward;
                e.SetComLocomotion<LocomotionStraightDir>(out var locomotion);
                locomotion.SetDir(dir);
                locomotion.SetMoveSpeed(moveSpeed);
                node.SetVar<Vector3>(CvKey.CV_ModeStraightDir, dir);

                var quaternion = Quaternion.LookRotation(dir);
                e.SetQuaternion(quaternion);
            });
        }

        /// <summary>
        /// 创建子物体运动初始化节点，由调用方传入具体运动初始化逻辑。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg Subobject_SetLocomotion(NodeParamAction actionSetLocomotion)
        {
            return BeginCall(node => { actionSetLocomotion?.Invoke(node); });
        }
    }
}
