using PBConfig;
using UnityEngine;

namespace LccHotfix
{
    public static class CustomNodeSubobjectExtensions
    {
    #region 子物体创建


        /// <summary>
        /// 根据子物体配置创建子物体实体，解析模型、逻辑 ID 和生命周期。
        /// </summary>
        public static LogicEntity CreateSubObject(this CustomNode self, uint subobjectTid, Vector3 initPos, VarEnv varEnv, int subobjectLogicIDOverride = 0)
        {
            if (subobjectTid == 0)
            {
                self.LogError("CreateSubObject subobjectTid == 0");
                return null;
            }

            var pbCfg = PbCfg.GetData<TSubobject>(subobjectTid);
            if (pbCfg == null)
            {
                self.LogError("CreateSubObject pbCfg == null");
                return null;
            }

            string mainResPath = null;
            var goCfg = PbCfg.GetData<TAssetGameObjectModel>(pbCfg.Model);
            if (goCfg != null)
            {
                mainResPath = goCfg.PathName;
            }

            var world = self.GetLogicWorld();
            var playerInfo = self.GetOwnerBattlePlayerInfo();
            mainResPath = world?.GetCreationInfo<BattleKernelCreationInfo>()?.SubobjectModelOverrideProvider?.ResolveMainModelPath(playerInfo, subobjectTid, mainResPath) ?? mainResPath;

            int subobjectLogicID = subobjectLogicIDOverride > 0 ? subobjectLogicIDOverride : (int)pbCfg.LogicID;
            var lifeTime = pbCfg.During;
            if (lifeTime == 0)
            {
                self.LogError($"CreateSubObject 请策划确认  pbCfg.During == 0  subobjectTid={subobjectTid}");
                lifeTime = 10f;
            }

            var genInfo = self.GetGenInfo<SubobjectGenInfo>(false);
            if (genInfo != null && self.GetOwnerEntity()?.hasComSubobject == true)
            {
                var es = self.CreateSubobjectEntityBySubobject(subobjectTid, subobjectLogicID, mainResPath, initPos, varEnv);
                es?.AddComLife(lifeTime);
                return es;
            }

            var e = self.CreateSubobjectEntity(subobjectTid, subobjectLogicID, mainResPath, initPos, varEnv);
            e?.AddComLife(lifeTime);
            return e;
        }

        /// <summary>
        /// 创建子物体实体并挂载子物体逻辑。
        /// </summary>
        public static LogicEntity CreateSubobjectEntity(this CustomNode self, uint subobjectTid, int subobjectLogicID, string path, Vector3 initPos, VarEnv varEnv)
        {
            var world = self.GetLogicWorld();
            if (world == null)
            {
                self.LogError("CreateSubobjectEntity Spawn logicWorld == null");
                return null;
            }

            if (world.GameOver)
                return null;

            var ownerFighterEntityID = self.GetOwnerFighterEntityID();
            var ownerFighterEntity = world.GetEntityWithComID(ownerFighterEntityID);
            if (ownerFighterEntity == null)
            {
                self.LogError("CreateSubobjectEntity Spawn owner == null");
                return null;
            }

            var spawnPolicy = world.GetCreationInfo<BattleKernelCreationInfo>()?.SubobjectSpawnPolicy;
            if (spawnPolicy != null && !spawnPolicy.CanSpawnFromOwner(ownerFighterEntity))
                return null;

            var playerInfo = self.GetOwnerBattlePlayerInfo();
            var faction = (playerInfo as InGamePlayerInfo)?.PlayerFaction
                ?? ownerFighterEntity.comFaction.Faction;
            var e = world.AddEntity(path);
            e.ReplaceComFaction(faction);
            e.AddComAnimation(new MainAnimatorCtrl());
            e.AddComCommandSender(new SubobjectEntityCmdPreHandler());
            e.AddComTransform(initPos, Quaternion.identity, Vector3.one);
            e.AddHolderEntity(ownerFighterEntityID);
            if (playerInfo != null)
            {
                e.AddComOwnerPlayer(playerInfo);
            }

            var svc = world.GetCreationInfo<BattleKernelCreationInfo>().CustomLogicService;
            if (svc == null)
            {
                self.LogError("CreateSubobjectEntity CustomLogicService == null");
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

            var sourceGenInfo = self.GetGenInfo<IHasSumUnitSource>(false);
            var skillUnitSource = sourceGenInfo?.SumUnitSource ?? default;
            if (skillUnitSource.FighterEnityId == 0)
            {
                skillUnitSource = new UnitSource(ownerFighterEntity);
            }
            var sumUnitSource = skillUnitSource;
            sumUnitSource.Properties.FillFromSubobjectVolume(playerInfo, subobjectTid);

            var subobjSource = new SubobjectSource(subobjectLogicID, subobjectTid);

            var subobjectCfg = PbCfg.GetData<TSubobject>(subobjectTid);
            self.RootLogic.GetDamageType(out var damageType);
            if (subobjectCfg != null && subobjectCfg.DamageType != TElementType.EetAll)
                damageType = subobjectCfg.DamageType;

            self.FillSubobjectBaseVarEnv(varEnv, e);
            if (varEnv.ReadVar<bool>(CvKey.CV_IsAmmoLastShot, out var isAmmoLastShot) && isAmmoLastShot)
            {
                var lastShotAmplify = (playerInfo as InGamePlayerInfo)?.FeaturesContext?.GlobalAmmoLastShotInGameAmplify ?? 0f;
                if (lastShotAmplify > 0f)
                    sumUnitSource.Properties.InGameAmplify += lastShotAmplify;
            }

            var parentEnv = self.VarEnvRef;
            parentEnv.ReadVar<int>(CvKey.CV_BattleUnitTid, out var battleUnitTid);
            parentEnv.ReadVar<TFighter>(CvKey.CV_FigherCfg, out var fighterCfg);

            var genInfo = FighterSubobjectGenInfo.New(svc,
                metaWorld: self.GetMetaWorld(),
                ownerEntity: e,
                ownerFighterEntityID: ownerFighterEntityID,
                sumUnitSource: sumUnitSource,
                subobjSource: subobjSource,
                battleUnitTid: battleUnitTid,
                fighterCfg: fighterCfg,
                damageType: damageType);
            genInfo.LogicConfigID = subobjectLogicID;
            genInfo.ConfigContainerName = LogicContainerKey.LogicConfigs_Subobject;
            genInfo.PreEnv = varEnv;
            genInfo.BattleSupplyId = self.TryGetBattleSupplyId();
            var logic = svc.CreateLogic<SubobjectLogic>(genInfo);
            e.AddComSubobject(ownerFighterEntity.ID, 1, (int)subobjectTid, logic);
            return e;
        }

        /// <summary>
        /// 由子物体产生子物体：此时原本的单位可能已经不在了，需要从父级子物体获得完整上下文
        /// </summary>
        public static LogicEntity CreateSubobjectEntityBySubobject(this CustomNode self, uint subobjectTid, int subobjectLogicID, string path, Vector3 initPos, VarEnv varEnv)
        {
            var world = self.GetLogicWorld();
            if (world == null)
            {
                self.LogError("CreateSubobjectEntityBySubobject Spawn logicWorld == null");
                return null;
            }

            if (world.GameOver)
                return null;

            var fatherSubobject = self.GetOwnerEntity();
            if (fatherSubobject == null || !fatherSubobject.hasComSubobject)
            {
                self.LogError("CreateSubobjectEntityBySubobject Spawn fatherSubobject failed");
                return null;
            }

            var ownerFighterEntityID = self.GetOwnerFighterEntityID();

            var playerInfo = self.GetOwnerBattlePlayerInfo();
            var faction = (playerInfo as InGamePlayerInfo)?.PlayerFaction
                ?? (fatherSubobject.hasComFaction ? fatherSubobject.comFaction.Faction : EFaction.Invalid);
            var e = world.AddEntity(path);
            e.ReplaceComFaction(faction);
            e.AddComAnimation(new MainAnimatorCtrl());
            e.AddComCommandSender(new SubobjectEntityCmdPreHandler());
            e.AddComTransform(initPos, Quaternion.identity, Vector3.one);
            e.AddHolderEntity(ownerFighterEntityID);
            if (playerInfo != null)
            {
                e.AddComOwnerPlayer(playerInfo);
            }

            var svc = world.GetCreationInfo<BattleKernelCreationInfo>().CustomLogicService;
            if (svc == null)
            {
                self.LogError("CreateSubobjectEntity CustomLogicService == null");
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

            var subobjectCfg = PbCfg.GetData<TSubobject>(subobjectTid);
            self.GetDamageType(out var damageType);
            if (subobjectCfg != null && subobjectCfg.DamageType != TElementType.EetAll)
                damageType = subobjectCfg.DamageType;

            var sourceGenInfo = self.GetGenInfo<IHasSumUnitSource>(false);
            var skillUnitSource = sourceGenInfo?.SumUnitSource ?? default;
            var sumUnitSource = skillUnitSource;
            sumUnitSource.Properties.FillFromSubobjectVolume(playerInfo, subobjectTid);

            var subobjSource = new SubobjectSource(subobjectLogicID, subobjectTid);

            self.FillSubobjectBaseVarEnv(varEnv, e);
            if (varEnv.ReadVar<bool>(CvKey.CV_IsAmmoLastShot, out var isAmmoLastShot) && isAmmoLastShot)
            {
                var lastShotAmplify = (playerInfo as InGamePlayerInfo)?.FeaturesContext?.GlobalAmmoLastShotInGameAmplify ?? 0f;
                if (lastShotAmplify > 0f)
                    sumUnitSource.Properties.InGameAmplify += lastShotAmplify;
            }

            var parentEnv = self.VarEnvRef;
            parentEnv.ReadVar<int>(CvKey.CV_BattleUnitTid, out var battleUnitTid);
            parentEnv.ReadVar<TFighter>(CvKey.CV_FigherCfg, out var fighterCfg);

            var genInfo = SubobjectGenInfo.New(svc,
                metaWorld: self.GetMetaWorld(),
                ownerEntity: e,
                ownerFighterEntityID: ownerFighterEntityID,
                sumUnitSource: sumUnitSource,
                subobjSource: subobjSource,
                damageType: damageType
            );
            genInfo.LogicConfigID = subobjectLogicID;
            genInfo.ConfigContainerName = LogicContainerKey.LogicConfigs_Subobject;
            genInfo.PreEnv = varEnv;
            genInfo.BattleSupplyId = self.TryGetBattleSupplyId();
            var logic = svc.CreateLogic<SubobjectLogic>(genInfo);
            e.AddComSubobject(ownerFighterEntityID, 1, (int)subobjectTid, logic);
            return e;
        }
        #endregion
    }
}
