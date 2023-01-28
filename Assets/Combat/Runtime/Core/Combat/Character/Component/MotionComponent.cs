using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 运动组件，在这里管理战斗实体的移动、跳跃、击飞等运动功能
    /// </summary>
    public sealed class MotionComponent : Component, IUpdate
    {
        public override bool DefaultEnable { get; set; } = true;
        public Vector3 Position { get => GetParent<CombatEntity>().Position; set => GetParent<CombatEntity>().Position = value; }
        public Quaternion Rotation { get => GetParent<CombatEntity>().Rotation; set => GetParent<CombatEntity>().Rotation = value; }
        public bool CanMove { get; set; }
        public GameTimer IdleTimer { get; set; }
        public GameTimer MoveTimer { get; set; }
        public Vector3 MoveVector { get; set; }
        private Vector3 originPos;


        public override void Awake()
        {

        }

        public void RunAI()
        {
            IdleTimer = new GameTimer(RandomHelper.RandomNumber(20, 30) / 10f);
            MoveTimer = new GameTimer(RandomHelper.RandomNumber(20, 40) / 10f);
            IdleTimer.Reset();
            originPos = Position;
        }

        public void Update()
        {
            if (IdleTimer == null)
            {
                return;
            }

            if (IdleTimer.IsRunning)
            {
                IdleTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, IdleFinish);
            }
            else
            {
                if (MoveTimer.IsRunning)
                {
                    MoveTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, MoveFinish);
                    var speed = GetParent<CombatEntity>().GetComponent<AttributeComponent>().MoveSpeed.Value;
                    Position += MoveVector * speed;
                }
            }
        }

        private void IdleFinish()
        {
            var x = RandomHelper.RandomNumber(-20, 20);
            var z = RandomHelper.RandomNumber(-20, 20);
            var vec2 = new Vector2(x, z);
            if (Vector3.Distance(originPos, Position) > 0.1f)
            {
                vec2 = -(Position - originPos);
            }
            vec2.Normalize();
            var right = new Vector2(1, 0);
            var y = VectorAngle(right, vec2);
            Rotation = Quaternion.Euler(0, y, 0);

            MoveVector = new Vector3(vec2.x, 0, vec2.y) / 100f;
            MoveTimer.Reset();
        }

        private void MoveFinish()
        {
            IdleTimer.Reset();
        }

        private float VectorAngle(Vector2 from, Vector2 to)
        {
            var angle = 0f;
            var cross = Vector3.Cross(from, to);
            angle = Vector2.Angle(from, to);
            return cross.z > 0 ? -angle : angle;
        }
    }
}