using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HotUpdate.Framework.PbCfg;
using PBConfig;
using UnityEngine;

namespace LccHotfix
{
    #region 委托类型

    /// <summary>
    /// 命中派发伤害前用于调整伤害事件的回调。
    /// </summary>
    public delegate void DamageAdjustFunc(CustomNode node, ref EvtDamage dmg);

    /// <summary>
    /// 命中效果执行前用于调整命中信息的回调。
    /// </summary>
    public delegate void PreHitFunc(CustomNode node, ref HitInfo hitInfo);

    /// <summary>
    /// AOE 或命中流程中对单个目标执行命中效果的回调。
    /// </summary>
    public delegate void NodeHitEffectFunc(CustomNode node, LogicEntity entity, LogicEntity target, HitInfo hitInfo);

    #endregion

    public partial class CustomNode
    {
        #region 世界与上下文获取

        /// <summary>
        /// 获取当前逻辑节点黑板中的 LogicWorld。
        /// </summary>
        public LogicWorld GetLogicWorld()
        {
            var key = CvKey.CV_LogicWorld;
            var world = GetVar<LogicWorld>(key);
            if (world == null)
            {
                this.LogError("GetLogicWorld world == null");
            }

            return world;
        }

        /// <summary>
        /// 获取当前逻辑节点黑板中的 MetaWorld。
        /// </summary>
        public MetaWorld GetMetaWorld()
        {
            var metaWorld = GetVar<MetaWorld>(CvKey.CV_MetaWorld);
            if (metaWorld == null)
            {
                this.LogError("GetMetaWorld metaWorld == null");
            }

            return metaWorld;
        }

        /// <summary>
        /// 判断当前逻辑世界是否已经结束战斗。
        /// </summary>
        public bool GameOver()
        {
            var logicWorld = GetLogicWorld();
            return logicWorld.GameOver;
        }

        /// <summary>
        /// 获取当前逻辑节点黑板中记录的拥有者实体。
        /// </summary>
        public LogicEntity GetOwnerEntity()
        {
            var owner = GetVar<LogicEntity>(CvKey.CV_OwnerEntity);
            if (owner == null)
            {
                this.LogError("GetOwnerEntity owner == null");
            }

            return owner;
        }

        /// <summary>
        /// 获取当前逻辑节点黑板中的拥有者玩家信息。
        /// </summary>
        public IBattlePlayerInfo GetOwnerBattlePlayerInfo()
        {
            var playerInfo = GetVar<IBattlePlayerInfo>(CvKey.CV_OwnerPlayerInfo);
            if (playerInfo == null)
            {
                this.LogError("node.GetOwnerBattlePlayerInfo == null");
            }

            return playerInfo;
        }

        #endregion

        #region 技能创建

        /// <summary>
        /// 创建技能流程黑板，写入目标、伤害类型和攻击次数后挂载技能流程。
        /// </summary>
        public void CreateSkillProcess(LogicEntity entity, int skillTid, LogicEntity target, EDamageType damageType, int curAttackTimes)
        {
            var varEnv = GetLogicWorld().CustomLogicService.NewVarEnv();
            varEnv.WriteVar(CvKey.CV_TargetEid, target.ID);
            varEnv.WriteVar(CvKey.CV_TargetPos, target.position);
            varEnv.WriteVar(CvKey.CV_DamageType, damageType);
            varEnv.WriteVar(CvKey.CV_CurAttackTimes, curAttackTimes);
            if (HasVar<Vector3>(CvKey.CV_LockedSkillDir))
            {
                varEnv.WriteVar(CvKey.CV_LockedSkillDir, GetVar<Vector3>(CvKey.CV_LockedSkillDir));
            }

            CreateSkillProcess(entity, skillTid, varEnv);
        }

        /// <summary>
        /// 使用已有黑板创建并挂载技能流程，同时向实体发送技能命令。
        /// </summary>
        public void CreateSkillProcess(LogicEntity entity, int skillTid, VarEnv varEnv)
        {
            var skillProcess = CreateSkillProcessLogic(entity, skillTid, varEnv);
            var cmd = new EntityCommand { CmdType = EntityCmdType.Nt_Skill };
            entity.SendCmd(cmd);
            entity.ReplaceComSkillProcess((uint)skillTid, skillProcess);
        }

        /// <summary>
        /// 根据技能配置创建技能逻辑实例，不直接挂载到实体技能流程组件。
        /// </summary>
        public SkillLogic CreateSkillProcessLogic(LogicEntity entity, int skillTid, VarEnv varEnv = null)
        {
            var skillCfg = PbCfg.GetData<TSkillLogic>((uint)skillTid);
            var logicID = skillCfg.LogicID;
            logicID = ResolveSkillLogicID(logicID);

            var svc = GetLogicWorld().CustomLogicService;
            if (varEnv == null)
            {
                varEnv = svc.NewVarEnv();
            }

            FillSkillBaseVarEnv(varEnv, entity, skillCfg);

            var genInfo = svc.NewGenInfo<UnitLogicGenInfo>();
            genInfo.LogicConfigID = logicID;
            genInfo.ConfigContainerName = LogicContainerKey.LogicConfigs_Skill;
            genInfo.PreEnv = varEnv;
            genInfo.SumUnitSource = new UnitSource(entity);
            genInfo.SkillUnitSource = genInfo.SumUnitSource;
            return svc.CreateLogic<SkillLogic>(genInfo);
        }

        /// <summary>
        /// 根据外部技能逻辑重写服务修正技能LogicID。
        /// </summary>
        private int ResolveSkillLogicID(int logicID)
        {
            var world = GetLogicWorld();
            var player = GetOwnerBattlePlayerInfo();
            var fighterCfg = GetVar<TFighter>(CvKey.CV_FigherCfg);
            var newLogicID = world?.SkillLogicOverrideProvider?.ResolveSkillLogicId(player, fighterCfg, logicID) ?? logicID;
            if (newLogicID != logicID)
            {
                CLHelper.LogInfo(this, $"ResolveSkillLogicID 特性修正技能ID logicID:{logicID} -> {newLogicID}");
            }

            return newLogicID;
        }

        #endregion

        #region 子物体创建

        /// <summary>
        /// 根据子物体配置创建子物体实体，解析模型、逻辑 ID 和生命周期。
        /// </summary>
        public LogicEntity CreateSubObject(uint subobjectTid, Vector3 initPos, VarEnv varEnv)
        {
            if (subobjectTid == 0)
            {
                this.LogError("SpawnSubobjectBhv Spawn subobjectTid == 0");
                return null;
            }

            var pbCfg = PbCfg.GetData<TSubobject>(subobjectTid);
            if (pbCfg == null)
            {
                return null;
            }

            string mainResPath = null;
            var goCfg = PbCfg.GetData<TAssetGameObjectModel>(pbCfg.Model);
            if (goCfg != null)
            {
                mainResPath = goCfg.PathName;
            }

            var world = GetLogicWorld();
            var playerInfo = GetOwnerBattlePlayerInfo();
            mainResPath = world?.SubobjectModelOverrideProvider?.ResolveMainModelPath(playerInfo, subobjectTid, mainResPath) ?? mainResPath;

            int subobjectLogicID = (int)pbCfg.LogicID;
            var lifeTime = pbCfg.During;
            if (lifeTime == 0)
            {
                this.LogError($"SpawnSubobjectBhv 请策划确认  pbCfg.During == 0  subobjectTid={subobjectTid}");
                lifeTime = 10f;
            }

            var e = CreateSubobjectEntity(subobjectTid, subobjectLogicID, mainResPath, initPos, varEnv);
            e?.AddComLife(lifeTime);
            return e;
        }

        /// <summary>
        /// 创建子物体实体并挂载子物体逻辑。
        /// </summary>
        public LogicEntity CreateSubobjectEntity(uint subobjectTid, int subobjectLogicID, string path, Vector3 initPos, VarEnv varEnv)
        {
            var world = GetLogicWorld();
            if (world == null)
            {
                this.LogError("CreateSubobjectEntity Spawn logicWorld == null");
                return null;
            }

            var ownerFighterEntityID = GetVar<long>(CvKey.CV_OwnerFighterEntityID);
            var ownerFighterEntity = world.GetEntityWithComID(ownerFighterEntityID);
            if (ownerFighterEntity == null)
            {
                this.LogError("CreateSubobjectEntity Spawn owner == null");
                return null;
            }

            var playerInfo = GetOwnerBattlePlayerInfo();
            var faction = ownerFighterEntity.comFaction.Faction;
            var e = world.AddEntity(path);
            e.ReplaceComFaction(faction);
            e.AddComAnimation(new MainAnimatorCtrl());
            e.AddComCommandSender(new SubobjectEntityCmdPreHandler());
            e.AddComTransform(initPos, Quaternion.identity, Vector3.one);
            e.AddHolderEntity(ownerFighterEntityID);

            var svc = world.CustomLogicService;
            if (svc == null)
            {
                this.LogError("CreateSubobjectEntity CustomLogicService == null");
                return e;
            }

            if (varEnv == null)
            {
                varEnv = svc.NewVarEnv();
            }

            if (subobjectTid == 0)
            {
                subobjectTid = 1;
            }

            var sourceGenInfo = GetGenInfo<UnitLogicGenInfo>();
            var sumUnitSource = sourceGenInfo?.SkillUnitSource ?? default;
            world.CombatPropertyVolumeProvider?.AddSubobjectVolume(playerInfo, subobjectTid, ref sumUnitSource.Properties);

            var subobjSource = new SubobjectSource(playerInfo, subobjectTid);

            FillSubobjectBaseVarEnv(varEnv, e);
            var genInfo = svc.NewGenInfo<UnitLogicGenInfo>();
            genInfo.LogicConfigID = subobjectLogicID;
            genInfo.ConfigContainerName = LogicContainerKey.LogicConfigs_Subobject;
            genInfo.PreEnv = varEnv;
            genInfo.SkillUnitSource = sourceGenInfo?.SkillUnitSource;
            genInfo.SumUnitSource = sumUnitSource;
            genInfo.SubobjSource = subobjSource;
            var logic = svc.CreateLogic<SubobjectLogic>(genInfo);
            e.AddComSubobject(ownerFighterEntity.ID, 1, (int)subobjectTid, logic);
            return e;
        }

        #endregion

        #region 战斗实体获取

        /// <summary>
        /// 根据黑板中的拥有者战斗实体 ID 获取战斗实体。
        /// </summary>
        public LogicEntity GetFighterEntity()
        {
            var world = GetLogicWorld();
            if (world == null)
            {
                this.LogError("GetFighterEntity world == null");
                return null;
            }

            var fighterEntityID = GetVar<long>(CvKey.CV_OwnerFighterEntityID);
            if (fighterEntityID == 0)
            {
                this.LogError("GetFighterEntity fighterEntityID == 0");
                return null;
            }

            var fighterEntity = world.GetEntityWithComID(fighterEntityID);
            if (fighterEntity == null)
            {
                this.LogError($"GetFighterEntity fighterEntity == null, fighterEntityID={fighterEntityID}");
                return null;
            }

            return fighterEntity;
        }

        #endregion

        #region 黑板环境填充

        /// <summary>
        /// 填充战斗单位主状态机所需的基础黑板变量。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillMainFsmVarEnv(VarEnv newEnv, LogicEntity e, IBattlePlayerInfo playerInfo, int battleUnitTid, TFighter fighterCfg)
        {
            var env = VarEnvRef; //levelNode、gameModeNode
            env.CopyTo<LogicWorld>(newEnv, CvKey.CV_LogicWorld);
            env.CopyTo<MetaWorld>(newEnv, CvKey.CV_MetaWorld);
            newEnv.WriteVar<LogicEntity>(CvKey.CV_OwnerEntity, e);
            newEnv.WriteVar<long>(CvKey.CV_OwnerFighterEntityID, e.ID);
            newEnv.WriteVar(CvKey.CV_OwnerPlayerInfo, playerInfo);
            newEnv.WriteVar<int>(CvKey.CV_BattleUnitTid, battleUnitTid);
            newEnv.WriteVar<TFighter>(CvKey.CV_FigherCfg, fighterCfg);
        }

        /// <summary>
        /// 填充技能逻辑所需的基础黑板变量。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillSkillBaseVarEnv(VarEnv newEnv, LogicEntity skill_e, TSkillLogic skillCfg)
        {
            var env = VarEnvRef; //mainFSMNode、aiNode
            env.CopyTo<LogicWorld>(newEnv, CvKey.CV_LogicWorld);
            env.CopyTo<MetaWorld>(newEnv, CvKey.CV_MetaWorld);
            newEnv.WriteVar<LogicEntity>(CvKey.CV_OwnerEntity, skill_e);

            var logicID = skillCfg.LogicID;
            var damageRate = skillCfg.DamageRate;
            var subTids = skillCfg.SubobjIds;
            var subTid = subTids?.Count > 0 ? (int)subTids[0] : -1;
            var skillTid = skillCfg.Base.Id;
            newEnv.WriteVar<float>(CvKey.CV_SearchRange, skillCfg.Range);
            newEnv.WriteVar(CvKey.CV_SkillTid, (int)skillTid);
            newEnv.WriteVar(CvKey.CV_SkillDmageRate, damageRate);
            newEnv.WriteVar(CvKey.CV_BaseSpawnSbjTid, subTid);
            newEnv.WriteVar(CvKey.CV_SpawnSbjTid, subTid);
            if (IsDev())
            {
                BattleLog.Debug($"FillSkillBaseVarEnv damageRate={damageRate}, subTid={subTid}, skillTid={skillTid}, logicID={logicID}");
            }

            env.CopyTo<int>(newEnv, CvKey.CV_SkillLevel, false);

            env.CopyTo<long>(newEnv, CvKey.CV_OwnerFighterEntityID);
            env.CopyTo<int>(newEnv, CvKey.CV_BattleUnitTid);
            env.CopyTo<TFighter>(newEnv, CvKey.CV_FigherCfg);
            env.CopyTo<IBattlePlayerInfo>(newEnv, CvKey.CV_OwnerPlayerInfo);

            env.CopyTo<Vector3>(newEnv, CvKey.CV_LockedSkillDir, false); // 手动瞄准方向
            env.CopyTo<Vector3>(newEnv, CvKey.CV_TargetPos, false); // 手动瞄准时 FSM 已设置正确的目标位置
        }

        /// <summary>
        /// 填充子物体逻辑所需的基础黑板变量。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillSubobjectBaseVarEnv(VarEnv newEnv, LogicEntity e)
        {
            var env = VarEnvRef; //skillNode、subobjectNode
            newEnv.WriteVar<LogicEntity>(CvKey.CV_OwnerEntity, e);
            env.CopyTo<LogicWorld>(newEnv, CvKey.CV_LogicWorld);
            env.CopyTo<MetaWorld>(newEnv, CvKey.CV_MetaWorld);

            env.CopyTo<int>(newEnv, CvKey.CV_SkillTid, false);
            env.CopyTo<int>(newEnv, CvKey.CV_SkillLevel, false);
            env.CopyTo<float>(newEnv, CvKey.CV_SkillDmageRate);
            env.CopyTo<float>(newEnv, CvKey.CV_SearchRange, false);
            env.CopyTo<int>(newEnv, CvKey.CV_SkillLevel, false);

            env.CopyTo<long>(newEnv, CvKey.CV_OwnerFighterEntityID);
            env.CopyTo<int>(newEnv, CvKey.CV_BattleUnitTid);
            env.CopyTo<string>(newEnv, CvKey.CV_SbjHitFxResOverride, false);
            env.CopyTo<TFighter>(newEnv, CvKey.CV_FigherCfg);
            env.CopyTo<IBattlePlayerInfo>(newEnv, CvKey.CV_OwnerPlayerInfo);
            env.CopyTo<EDamageType>(newEnv, CvKey.CV_DamageType);
        }

        /// <summary>
        /// 填充 Buff 逻辑所需的基础黑板变量；当前仍沿用“宿主/来源”混用字段，后续继续收口。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillBuffBaseVarEnv(VarEnv newEnv, LogicEntity e)
        {
            var env = VarEnvRef; //skillNode、subobjectNode
            newEnv.WriteVar<LogicEntity>(CvKey.CV_OwnerEntity, e);
            env.CopyTo<LogicWorld>(newEnv, CvKey.CV_LogicWorld);
            env.CopyTo<MetaWorld>(newEnv, CvKey.CV_MetaWorld);

            env.FillBuffSourceVarEnv(newEnv);
        }

        #endregion

        #region 实体查询与创建

        /// <summary>
        /// 获取指定战斗单位配置 ID 对应的所有实体。
        /// </summary>
        public HashSet<LogicEntity> GetEntitiesWithComBattleUnitTid(int unitTid)
        {
            var logicWorld = GetLogicWorld();
            return logicWorld.GetEntitiesWithComBattleUnitTid(unitTid);
        }


        /// <summary>
        /// 创建带 Transform 和 Animation 组件的普通逻辑实体。
        /// </summary>
        public LogicEntity Create_Entity_Transform_Animation_Entity(string path, Vector3 pos)
        {
            var world = GetLogicWorld();
            if (world == null)
            {
                this.LogError($"CreateSubobjectEntity Spawn logicWorld == null");
                return null;
            }

            var e = world.AddEntity(path);
            e.AddComTransform(pos, Quaternion.identity, Vector3.one);
            e.AddComAnimation(new MainAnimatorCtrl());
            return e;
        }

        /// <summary>
        /// 创建跟随指定拥有者初始位置、并带有移动组件的逻辑实体。
        /// </summary>
        public LogicEntity Create_OwnerEntity_Transform_Locomotion_Entity(LogicEntity ownerEntity, LocomotionBase locomotion, string path)
        {
            var world = GetLogicWorld();
            if (world == null)
            {
                this.LogError($"CreateSubobjectEntity Spawn logicWorld == null");
                return null;
            }

            var e = world.AddEntity(path);
            e.AddHolderEntity(ownerEntity.ID);
            e.AddComTransform(ownerEntity.position, Quaternion.identity, Vector3.one);
            e.SetComLocomotion(locomotion);
            return e;
        }

        #endregion

        #region 单位来源与子物体来源

        /// <summary>
        /// 尝试获取当前逻辑生成信息中的整合单位来源。
        /// </summary>
        public bool GetSumUnitSource(out UnitSource skillUnitSource)
        {
            if (GenInfo is UnitLogicGenInfo theGenInfo)
            {
                skillUnitSource = theGenInfo.SumUnitSource;
                return true;
            }

            skillUnitSource = default;
            return false;
        }

        /// <summary>
        /// 获取当前全部标签身份整合后的参战单位来源信息。
        /// </summary>
        public UnitSource GetSumUnitSource(bool autoCreate = false)
        {
            if (GenInfo is UnitLogicGenInfo theGenInfo)
            {
                return theGenInfo.SumUnitSource;
            }

            if (autoCreate)
            {
                return CreateTempSumUnitSource();
            }

            return default;
        }

        /// <summary>
        /// 基于当前拥有者战斗实体临时创建单位来源信息。
        /// </summary>
        public UnitSource CreateTempSumUnitSource()
        {
            var world = GetVar<LogicWorld>(CvKey.CV_LogicWorld);
            var attackerID = GetVar<long>(CvKey.CV_OwnerFighterEntityID);
            if (world == null)
            {
                this.LogError($"node.GetUnitSource world == null");
                return default;
            }

            var e_attacker = world.GetEntityWithComID(attackerID);
            if (e_attacker == null)
            {
                this.LogError($"node.GetUnitSource e_attacker == null, attackerID={attackerID}");
                return default;
            }

            return new UnitSource(e_attacker);
        }

        /// <summary>
        /// 尝试获取当前逻辑生成信息中的子物体来源。
        /// </summary>
        public bool GetSubobjectSource(out SubobjectSource sbjSource)
        {
            if (GenInfo is UnitLogicGenInfo theGenInfo)
            {
                sbjSource = theGenInfo.SubobjSource.GetValueOrDefault();
                return theGenInfo.SubobjSource != null;
            }

            sbjSource = default;
            return false;
        }

        #endregion

        #region 实体变量查询

        /// <summary>
        /// 通过黑板变量配置获取实体，死亡实体返回 null。
        /// </summary>
        public LogicEntity GetEntityByVar(string varKey)
        {
            var cfg = new EntityVarCfg(varKey);
            var entity = cfg.GetEntity(this, false);
            if (entity.IsDead())
            {
                return null;
            }

            return entity;
        }

        #endregion

        #region AOE命中处理

        /// <summary>
        /// 以中心点和范围构造 AABB，并对范围内目标执行 AOE 命中效果。
        /// </summary>
        public void MakeAoeEffect_InAABB(LogicEntity entity, float aoeRange, Vector2 aoeCenterPos, NodeHitEffectFunc executeEffectFunc)
        {
            if (aoeRange <= 0)
            {
                this.LogError($"MakeAoeEffect_InAABB AoeRange({aoeRange}) <= 0");
                return;
            }

            var aoeAABB = new AABB(aoeCenterPos, aoeRange);
            MakeAoeEffect_InAABB(entity, aoeAABB, executeEffectFunc);
        }

        /// <summary>
        /// 对指定 AABB 范围内可碰撞的目标实体执行 AOE 命中效果。
        /// </summary>
        public void MakeAoeEffect_InAABB(LogicEntity entity, AABB aabb, NodeHitEffectFunc executeEffectFunc)
        {
            var world = entity?.OwnerWorld;
            if (world?.TargetQueryService == null)
            {
                return;
            }

            world.TargetQueryService.RangeAttackableTargetBatchAction(world, entity, entity.position, 999f, item =>
            {
                if (!item.IsValid())
                {
                    return true;
                }

                if (!item.CanCollider())
                {
                    return true;
                }

                var comBounds = item.comBounds;
                var bounds = comBounds.GetBounds();
                if (aabb.Collision(bounds))
                {
                    var aoeHitInfo = new HitInfo { hitPos = item.position, hitEntityID = item.ID };
                    executeEffectFunc(this, entity, item, aoeHitInfo);
                }

                return true;
            });
        }

        #endregion

        #region 单位属性与特性

        /// <summary>
        /// 累加当前整合单位来源属性中的局内增幅值。
        /// </summary>
        public void AddSumProperty_InGameAmplify(float inGameAmplify)
        {
            var theGenInfo = GetGenInfo<UnitLogicGenInfo>();
            if (theGenInfo == null)
            {
                this.LogError("AddSumProperty_InGameAmplify theGenInfo == null");
                return;
            }

            theGenInfo.SumUnitSource.Properties.InGameAmplify += inGameAmplify;
        }

        /// <summary>
        /// 根据拥有者韧性计算 Buff 持续时间倍率，并限制在合理区间。
        /// </summary>
        public float GetTenacityBuffDurationRate()
        {
            var ownerEntity = GetOwnerEntity();
            var ownerUnitSource = new UnitSource(ownerEntity);
            var tenacity = ownerUnitSource.Properties.Tenacity;
            if (tenacity == 0f)
            {
                return 1f;
            }

            var rate = (float)(10000f - tenacity) / 10000f;
            return Mathf.Clamp(rate, 0.2f, 2f);
        }

        /// <summary>
        /// 获取当前全部标签身份整合后的属性快照。
        /// </summary>
        public PropertySnapshot GetSumProperties()
        {
            var unitSource = GetSumUnitSource();
            return unitSource.Properties;
        }

        /// <summary>
        /// 获取当前全部标签身份整合后的特性数据。
        /// </summary>
        public UnitSkillFeature GetSumFeature()
        {
            var unitSource = GetSumUnitSource();
            return unitSource.Properties.Feature;
        }

        #endregion

        #region Buff生成

        /// <summary>
        /// 给目标创建一个 BuffGenInfo，来源继承当前节点的单位来源。
        /// </summary>
        public BuffGenInfo CreateBuffGenInfoFromUnit(LogicEntity target, int buffLogicID, int maxLvl = 1)
        {
            var sourceGenInfo = GetGenInfo<UnitLogicGenInfo>(false);
            if (sourceGenInfo == null && IsDev())
            {
                CLHelper.LogInfo(this, "MakeBuffGenInfo node.UnitLogicGenInfo != null");
            }

            var genInfo = target.CreateBuffGenInfo(buffLogicID, maxLvl);
            if (genInfo == null)
            {
                return null;
            }

            genInfo.SumUnitSource = sourceGenInfo?.SumUnitSource ?? CreateTempSumUnitSource();
            genInfo.SkillUnitSource = sourceGenInfo?.SkillUnitSource;
            genInfo.SubobjSource = sourceGenInfo?.SubobjSource;
            return genInfo;
        }

        #endregion

        #region 黑板变量运算

        /// <summary>
        /// 对黑板中的整型变量做加法并返回新值。
        /// </summary>
        public int IntVarAdd(string key, int add)
        {
            var v = GetVar<int>(key, 0);
            SetVar<int>(key, v + add);
            return v + add;
        }

        /// <summary>
        /// 对黑板中的浮点变量做加法并返回新值。
        /// </summary>
        public float FloatVarAdd(string key, float add)
        {
            var v = GetVar<float>(key, 0f);
            SetVar<float>(key, v + add);
            return v + add;
        }

        /// <summary>
        /// 对黑板中的整型变量按倍率相乘，变量不存在时返回 false。
        /// </summary>
        public bool IntVarMult(string key, float rate)
        {
            if (!HasVar<int>(key))
            {
                this.LogError($"node.IntVarMult 未找到黑板变量 key={key}");
                return false;
            }

            var v = GetVar<int>(key);
            SetVar<int>(key, (int)(v * rate));
            return true;
        }

        /// <summary>
        /// 对黑板中的浮点变量按倍率相乘，变量不存在时返回 false。
        /// </summary>
        public bool FloatVarMult(string key, float rate, bool debugLog = false)
        {
            if (!HasVar<float>(key))
            {
                this.LogError($"node.FloatVarMult 未找到黑板变量 key={key}");
                return false;
            }

            var v = GetVar<float>(key);
            SetVar<float>(key, v * rate);
            if (debugLog)
            {
                this.LogError($"FloatVarMult v({v}) * ({rate}) -> ({v * rate})");
            }

            return true;
        }

        #endregion

        #region 调试辅助

        /// <summary>
        /// 判断当前是否为开发日志模式。
        /// </summary>
        public bool IsDev()
        {
            return BattleLog.IsDebugEnabled;
        }

        #endregion
    }
}
