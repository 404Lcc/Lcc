using Entitas;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    public class BattleGameMode : GameModeBase
    {
        public BattleModeState data;

        public override void Init(GameModeState state)
        {
            base.Init(state);

            data = state as BattleModeState;
        }

        public override void Start()
        {
            base.Start();

        }

        public override void Release()
        {
            base.Release();

            GameObject.Destroy(data.map);
            data.map = null;
        }
    }
    public class BattleModeState : GameModeState
    {
        public GameObject map;
    }
    public class BattleWorld : ECSWorld
    {
        protected override void Setup()
        {
            base.Setup();

            LogicComponentsLookup.ComID = 0;
            LogicComponentsLookup.ComTag = 1;
            LogicComponentsLookup.ComFaction = 2;
            LogicComponentsLookup.ComOwnerEntity = 3;
            LogicComponentsLookup.ComUnityObjectRelated = 4;

            LogicComponentsLookup.componentTypeList.Add(typeof(ComID));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComTag));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComFaction));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComOwnerEntity));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComUnityObjectRelated));

            MetaComponentsLookup.ComUniGameMode = 0;
            MetaComponentsLookup.componentTypeList.Add(typeof(ComUniGameMode));
        }
        protected override void InitializeEntityIndices()
        {
            base.InitializeEntityIndices();

            LogicContext.AddEntityIndex(new PrimaryEntityIndex<LogicEntity, long>(typeof(ComID).Name, LogicContext.GetGroup(LogicMatcher.ComID), (e, c) => ((ComID)c).id));
            LogicContext.AddEntityIndex(new EntityIndexEnum<LogicEntity, TagType>(typeof(ComTag).Name, LogicContext.GetGroup(LogicMatcher.ComTag), (e, c) => ((ComTag)c).tag));
            LogicContext.AddEntityIndex(new EntityIndex<LogicEntity, FactionType>(typeof(ComFaction).Name, LogicContext.GetGroup(LogicMatcher.ComFaction), (e, c) => ((ComFaction)c).faction));
            LogicContext.AddEntityIndex(new EntityIndex<LogicEntity, int>(typeof(ComOwnerEntity).Name, LogicContext.GetGroup(LogicMatcher.ComOwnerEntity), (e, c) => ((ComOwnerEntity)c).ownerEntityID));
            LogicContext.AddEntityIndex(new GroupEntityIndex<LogicEntity, int>(typeof(ComUnityObjectRelated).Name, LogicContext.GetGroup(LogicMatcher.ComUnityObjectRelated), (e, c) => ((ComUnityObjectRelated)c).gameObjectInstanceID.Keys.ToArray()));
        }

        protected override void InitComponent()
        {
            base.InitComponent();

            var mode = new BattleGameMode();

            var state = new BattleModeState();
            state.gameModeType = GameModeType.Battle;
            state.map = null;

            mode.Init(state);
            MetaContext.SetComUniGameMode(mode);

            MetaContext.SetComUniDamage(new DamageBase());
        }
        protected override void InitSystem()
        {
            base.InitSystem();

            //技能
            System.Add(new SysSkillCD(this));
            System.Add(new SysSkillProcess(this));

            //buff
            System.Add(new SysBuffs(this));

            //子物体
            System.Add(new SysSubobject(this));

            //状态机
            System.Add(new SysFSM(this));

            //移动
            System.Add(new SysLocomotion(this));
            //碰撞
            System.Add(new SysCollision(this));

            //显示
            System.Add(new SysViewUpdate(this));

            //生命周期
            System.Add(new SysLife(this));
            System.Add(new SysDeathProcess(this));


            //游戏模式
            System.Add(new SysGameMode(this));
        }
    }
}