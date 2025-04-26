using Entitas;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    public class BattleGameMode : GameModeBase
    {
        public BattleGameModeState data;

        public override void Init(GameModeState state)
        {
            base.Init(state);

            data = state as BattleGameModeState;
        }

        public override void Start()
        {
            base.Start();

        }

        public override void Release()
        {
            base.Release();

            GameObject.Destroy(data.Map);
        }
    }

    public class BattleGameModeState : GameModeState
    {
        public GameObject Map { get; private set; }

        public void Init(GameObject map)
        {
            this.Map = map;
        }
    }

    public class BattleWorld : ECSWorld
    {
        protected override void Setup()
        {
            base.Setup();

            //基础
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

            //战斗部分
            LogicComponentsLookup.ComLocomotion = 5;
            LogicComponentsLookup.ComCollider = 6;
            LogicComponentsLookup.ComView = 7;
            LogicComponentsLookup.ComTransform = 8;
            LogicComponentsLookup.ComFSM = 9;
            LogicComponentsLookup.ComLife = 10;
            LogicComponentsLookup.ComDeath = 11;
            LogicComponentsLookup.ComProperty = 12;
            LogicComponentsLookup.ComHP = 13;
            LogicComponentsLookup.ComSkills = 14;
            LogicComponentsLookup.ComSkillProcess = 15;
            LogicComponentsLookup.ComBuffs = 16;
            LogicComponentsLookup.ComSubobject = 17;
            LogicComponentsLookup.ComControl = 18;
            LogicComponentsLookup.componentTypeList.Add(typeof(ComLocomotion));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComCollider));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComView));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComTransform));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComFSM));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComLife));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComDeath));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComProperty));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComHP));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComSkills));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComSkillProcess));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComBuffs));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComSubobject));
            LogicComponentsLookup.componentTypeList.Add(typeof(ComControl));

            MetaComponentsLookup.ComUniDamage = 1;
            MetaComponentsLookup.componentTypeList.Add(typeof(ComUniDamage));
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
            mode.Init(_gameModeState);
            MetaContext.SetComUniGameMode(mode);

            MetaContext.SetComUniDamage(new DamageBase());
        }

        protected override void InitSystem()
        {
            base.InitSystem();

            //控制
            System.Add(new SysControl(this));

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