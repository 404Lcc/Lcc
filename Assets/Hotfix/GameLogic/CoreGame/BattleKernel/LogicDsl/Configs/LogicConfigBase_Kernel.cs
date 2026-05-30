using HotUpdate.Framework.PbCfg;
using PBConfig;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LccHotfix
{
    public partial class LogicConfigBase
    {
        #region 通用节点包装

        /// <summary>
        /// 创建只在节点开始时执行一次的委托节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg BeginCall(NodeParamAction func)
        {
            return new DelegateBhvCfg(func);
        }

        /// <summary>
        /// 创建从节点开始后每帧执行的委托节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg UpdateCall(NodeParamTickAction func)
        {
            return new DelegateBhvCfg(func)
            {
                UpdateCall = true,
                BeginCall = true,
            };
        }

        /// <summary>
        /// 创建每帧累加经过时间到指定变量的委托节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg SaveTickAcc(string timeAccVar)
        {
            return new DelegateBhvCfg((node, dt) => { node.FloatVarAdd(timeAccVar, dt); })
            {
                UpdateCall = true,
                BeginCall = true,
            };
        }

        /// <summary>
        /// 将一个黑板变量读取后写入另一个变量，读取失败时使用默认值。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg CopyVar<T>(string var, string newVar, T defaultV)
        {
            return new DelegateBhvCfg(node => { node.SetVar(newVar, node.GetVar<T>(var, defaultV)); });
        }

        /// <summary>
        /// 创建指定持续时间内每帧执行更新回调的节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FiniteTimeUpdateCfg FiniteTimeUpdate(NodeParamTickAction func, float time)
        {
            return new FiniteTimeUpdateCfg(time).WithUpdate(func);
        }

        #endregion

        #region 伤害

        /// <summary>
        /// 对目标实体变量指定的目标派发伤害事件，实际结算由伤害系统处理。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg MakeDamageTo(string targetIDVar, bool logError = false)
        {
            return new DelegateBhvCfg(node =>
            {
                var entityCfg = new EntityVarCfg(targetIDVar);
                var target = entityCfg.GetEntity(node, logError);
                if (target != null)
                {
                    node.GetLogicWorld()?.DamageEventService?.DispatchDamage(new EvtDamage(node.RootLogic, target));
                }
            });
        }

        #endregion

        #region 子物体生成和子物体初始化与运动

        /// <summary>
        /// 从战斗单位指定挂点生成固定配置 ID 的子物体，并飞向目标位置变量指定的位置。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpawnSubobjectToTargetBhvCfg FighterSpawnSubobjectTo(int tid, string targetPosVar, string bpName)
        {
            return new SpawnSubobjectToTargetBhvCfg(tid) { TargetPos = new PosVarCfg(targetPosVar), InitPos = new PosVarCfg(CvKey.CV_OwnerEntity, bpName) };
        }

        /// <summary>
        /// 从战斗单位指定挂点生成变量配置 ID 的子物体，并飞向目标位置变量指定的位置。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpawnSubobjectToTargetBhvCfg FighterSpawnSubobjectTo(string tid, string targetPosVar, string bpName)
        {
            return new SpawnSubobjectToTargetBhvCfg(tid) { TargetPos = new PosVarCfg(targetPosVar), InitPos = new PosVarCfg(CvKey.CV_OwnerEntity, bpName) };
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
                var boundSize = 0.5f;
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
                    CLHelper.LogError(node, $"Subobject_SetCollider subobjectCfg != null, tid={tid}");
                    return;
                }

                e.ReplaceComBounds(e.comTransform.position, boundSize);
                switch (subobjectCfg.CollisionType)
                {
                    case CollisionType.EhtRaycast:
                        e.AddSubobjectComCollider<SubobjectColliderHandlerBase, RaycastHitAABBMaker>(e.comSubobject.Cfg, out var raycastHelper);
                        raycastHelper.SetDistance(boundSize * 2f);
                        break;
                    default:
                        e.AddSubobjectComCollider<SubobjectColliderHandlerBase, AABBRawHitMaker>(e.comSubobject.Cfg, out var aabbHelper);
                        aabbHelper.SetRadius(boundSize);
                        break;
                }
            });
        }

        /// <summary>
        /// 设置子物体直线运动方向、速度和朝向，目标位置来自 CV_SbjTargetPos。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg Subobject_SetLocomotionStraightDir(float speed = -1f)
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
                var dir = (targetPos - initPos).normalized;
                e.SetComLocomotion<LocomotionStraightDir>(out var locomotion);
                locomotion.SetDir(dir);
                locomotion.SetMoveSpeed(moveSpeed);
                node.SetVar<Vector3>(CvKey.CV_ModeStraightDir, dir);

                var quaternion = Quaternion.FromToRotation(Vector3.right, dir);
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

        #endregion

        #region Buff和Buff生命周期回调

        /// <summary>
        /// 给节点所属战斗单位添加指定逻辑 ID 的 Buff。
        /// </summary>
        protected DelegateBhvCfg AddBuff(int buffLogicID, int maxLvl)
        {
            return new DelegateBhvCfg(node =>
            {
                var ownerEntity = node.GetOwnerEntity();
                if (ownerEntity == null)
                {
                    return;
                }

                var genInfo = node.CreateBuffGenInfoFromUnit(ownerEntity, buffLogicID, maxLvl);
                ownerEntity.AddBuff(genInfo);
            });
        }

        /// <summary>
        /// 创建 Buff 升级或刷新时执行的回调节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandleBuffUpdateBhvCfg Buff_HandleBuffUpgrade(NodeParamAction action)
        {
            return new HandleBuffUpdateBhvCfg(action);
        }

        /// <summary>
        /// 创建 Buff 添加到实体时执行的回调节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandleBuffAddBhvCfg Buff_HandleBuffAdd(NodeParamAction action)
        {
            return new HandleBuffAddBhvCfg(action);
        }

        /// <summary>
        /// 创建 Buff 从实体移除时执行的回调节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandleBuffRemoveBhvCfg Buff_HandleBuffRemove(NodeParamAction action)
        {
            return new HandleBuffRemoveBhvCfg(action);
        }

        /// <summary>
        /// 创建 Buff 在伤害结算前修改伤害上下文或结果的回调节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandleBuffBeforeDmgBhvCfg Buff_HandleBeforeDmg(BuffHandleBeforeDmgAction action)
        {
            return new HandleBuffBeforeDmgBhvCfg(action);
        }

        /// <summary>
        /// 创建 Buff 监听并处理指定实体命令的回调节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandleBuffEntityCmdBhvCfg Buff_HandleEntityCmd(int cmd, NodeParamEntityCmdAction action)
        {
            return new HandleBuffEntityCmdBhvCfg(cmd, action);
        }

        #endregion
    }
}