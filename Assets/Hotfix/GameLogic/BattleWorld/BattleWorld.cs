using System.Collections.Generic;
using Entitas;
using System.Linq;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class BattleMode : GameModeBase
    {
        public ECSWorld world;
        private StateMachine _fsm;

        public override void Start()
        {
            base.Start();

            _fsm = new StateMachine(this);
            _fsm.AddNode<BattleStart>();
            _fsm.Run<BattleStart>();
        }

        public override void Update()
        {
            base.Update();

            if (_fsm == null)
                return;
            _fsm.Update();
        }



        public override void Release()
        {
            base.Release();

            if (_fsm == null)
                return;
            _fsm = null;
        }
    }

    public class BattleWorldData : IWorldData
    {
        public List<InGamePlayerData> PlayerList { get; private set; }

        public void Init(List<InGamePlayerData> playerList)
        {
            this.PlayerList = playerList;
        }
    }

    public class BattleStart : IStateNode
    {
        public BattleMode mode;

        public void OnCreate(StateMachine machine)
        {
            mode = machine.Owner as BattleMode;
        }

        public void OnEnter()
        {
        }

        public void CreatePlayer()
        {
            foreach (var item in mode.world.GetWorldData<BattleWorldData>().PlayerList)
            {
                var obj = GameUtility.GetObj("Player");
                var entity = EntityUtility.AddEntityWithID<CharacterActorView>(item.PlayerUID, obj);
                var player = entity.GetView<CharacterActorView>(ViewCategory.Actor);
                player.SetPlane(CharacterPlane.XZ);
                player.SetSize(new Vector2(1, 1));

                entity.AddComOwnerPlayer(item);
                entity.AddComTag(TagType.Hero);
                entity.AddComFaction(FactionType.Friend);
                entity.AddComHP(int.MaxValue);
                entity.AddComHero();
                entity.AddComProperty();
                entity.comProperty.Init(int.MaxValue, 100, 3);

                HPView hpView = new HPView();
                hpView.Init(entity, HeadbarType.NormalHP, 0);
                entity.AddView(hpView, ViewCategory.HP);
            }
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }
    }

    public class BattleWorld : ECSWorld
    {
        public override void CreateSystems()
        {
            System = new ECSSystems();
            System.Add(new SysInitializeBattle(this));
            this.AddRender();
            this.AddDeath();
            this.AddBattle();
        }
    }
}