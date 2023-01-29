using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 运动组件，在这里管理战斗实体的移动、跳跃、击飞等运动功能
    /// </summary>
    public sealed class MotionComponent : Component, IUpdate
    {
        public override bool DefaultEnable => true;


        public Vector3 Position { get => GetParent<CombatEntity>().Position; set => GetParent<CombatEntity>().Position = value; }
        public Quaternion Rotation { get => GetParent<CombatEntity>().Rotation; set => GetParent<CombatEntity>().Rotation = value; }


        public bool canMove;
        public GameTimer idleTimer;
        public GameTimer moveTimer;
        public Vector3 moveVector;
        private Vector3 originPos;


        public void Update()
        {
            if (idleTimer == null)
            {
                return;
            }

            if (idleTimer.IsRunning)
            {
                idleTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, IdleFinish);
            }
            else
            {
                if (moveTimer.IsRunning)
                {
                    moveTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, MoveFinish);
                    var speed = GetParent<CombatEntity>().GetComponent<AttributeComponent>().MoveSpeed.Value;
                    Position += moveVector * speed;
                }
            }
        }

        private void IdleFinish()
        {
            var x = RandomUtil.RandomNumber(-20, 20);
            var z = RandomUtil.RandomNumber(-20, 20);
            var vec2 = new Vector2(x, z);
            if (Vector3.Distance(originPos, Position) > 0.1f)
            {
                vec2 = -(Position - originPos);
            }
            vec2.Normalize();
            var right = new Vector2(1, 0);
            var y = VectorAngle(right, vec2);
            Rotation = Quaternion.Euler(0, y, 0);

            moveVector = new Vector3(vec2.x, 0, vec2.y) / 100f;
            moveTimer.Reset();
        }

        private void MoveFinish()
        {
            idleTimer.Reset();
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