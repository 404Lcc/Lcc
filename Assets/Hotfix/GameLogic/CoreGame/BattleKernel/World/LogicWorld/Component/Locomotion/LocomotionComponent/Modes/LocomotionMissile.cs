using UnityEngine;

namespace LccHotfix
{

    public interface IGetPos
    {
        Vector3 GetPos();
    }
    
    //写法的中间状态，不要学习这个的临时状态，正式写法稍后整理
    public class NodeGetTargetPos : IGetPos
    {
        private CustomNode nodeRef;
        public NodeGetTargetPos(CustomNode node)
        {
            nodeRef = node;
        }
        public Vector3 GetPos()
        {
            var targetEntity = EntityVarCfg.GetEntity(nodeRef, CvKey.CV_TargetEid, false);
            if (targetEntity != null)
            {
                return targetEntity.position;
            }
            return nodeRef.GetVar<Vector3>(CvKey.CV_SbjTargetPos);
        }
    }
    //public delegate Vector3 TargetPosAction( CustomNode node );
    
    /// <summary>
    /// 轻量级导弹追踪运动（性能优先，简洁高效）
    /// 适合需要大量导弹同时存在的场景，实现“漫天乱飞”的追踪效果
    /// </summary>
    public class LocomotionMissileLight : LocomotionBase
    {
        #region 配置参数
        private float _moveSpeed = 8f; // 导弹移动速度
        private float _turnSpeed = 180f; // 转向速度（单位：度/秒），值越大转向越灵敏
        private float _randomness = 0.5f; // 随机偏移强度（0-1），0为精准追踪，1为最大幅度乱飞
        private float _minDistanceToTarget = 0.3f; // 到达目标的判定距离

        private bool _is3DMode = false; // 是否3D模式
        private IGetPos _getTargetPos; // 实时获取目标位置的委托
        #endregion

        #region 内部状态
        private Vector3 _currentDir = Vector3.right; // 当前移动方向
        private Vector3 _targetDriftDir; // 目标随机偏移方向
        private float _driftChangeTimer; // 随机偏移方向切换计时器
        private const float DRIFT_CHANGE_INTERVAL = 0.3f; // 随机偏移方向切换间隔（秒）
        #endregion

        public LocomotionMissileLight()
        {
        }

        #region 外部设置接口
        /// <summary>
        /// 设置动态目标
        /// </summary>
        public void SetDynamicTarget(IGetPos getTargetPos)
        {
            _getTargetPos = getTargetPos;
            IsRuning = true;
            // 初始化随机偏移方向
            _targetDriftDir = GetRandomDirection();
            SetMissileParams(10f,  180f, 0.3f);
        }

        /// <summary>
        /// 设置导弹核心参数
        /// </summary>
        /// <param name="moveSpeed">移动速度</param>
        /// <param name="turnSpeed">转向速度（度/秒）</param>
        /// <param name="randomness">随机偏移强度（0-1）</param>
        public void SetMissileParams(float moveSpeed, float turnSpeed, float randomness)
        {
            _moveSpeed = Mathf.Max(0.1f, moveSpeed);
            _turnSpeed = Mathf.Max(0f, turnSpeed);
            _randomness = Mathf.Clamp01(randomness);
        }

        /// <summary>
        /// 设置场景模式（2D/3D）
        /// </summary>
        public void SetSceneMode(bool is3D)
        {
            _is3DMode = is3D;
            _currentDir = _is3DMode ? Vector3.forward : Vector3.right;
            _targetDriftDir = GetRandomDirection();
        }
        #endregion

        #region 核心更新逻辑
        public override void Update(float dt, LogicEntity entity)
        {
            var comTrans = entity.comTransform;
            var curPosition = comTrans.position;
            var curRotation = comTrans.rotation;

            Vector3 toTarget = Vector3.zero;
            if (_getTargetPos is not null)
            {
                Vector3 targetPos = _getTargetPos.GetPos();
                toTarget = targetPos - curPosition;
            }
            if (!_is3DMode)
            {
                toTarget.z = 0;
            }

            // 到达目标判定
            if (toTarget.magnitude <= _minDistanceToTarget)
            {
                toTarget = _currentDir;
            }

            // 1. 更新随机偏移方向（定期改变，实现“乱飞”效果）
            UpdateRandomDriftDirection(dt);

            // 2. 计算期望方向 = 目标方向 + 随机偏移方向
            Vector3 desiredDir = toTarget.normalized;
            desiredDir = Vector3.Lerp(desiredDir, _targetDriftDir, _randomness);
            desiredDir.Normalize();

            // 3. 平滑转向
            _currentDir = Vector3.RotateTowards(_currentDir, desiredDir, _turnSpeed * Mathf.Deg2Rad * dt, 0f);

            // 4. 更新位置和旋转
            curPosition += _currentDir * _moveSpeed * dt;
            comTrans.SetPosition(curPosition);

            if (_is3DMode)
            {
                curRotation = Quaternion.LookRotation(_currentDir);
                comTrans.SetRotation(curRotation);
            }
            else
            {
                curRotation = Quaternion.FromToRotation(Vector3.right, _currentDir);
                comTrans.SetRotation(curRotation);
            }
        }

        /// <summary>
        /// 定期更新随机偏移方向
        /// </summary>
        private void UpdateRandomDriftDirection(float dt)
        {
            _driftChangeTimer += dt;
            if (_driftChangeTimer >= DRIFT_CHANGE_INTERVAL)
            {
                _targetDriftDir = GetRandomDirection();
                _driftChangeTimer = 0f;
            }
        }

        /// <summary>
        /// 获取一个随机的方向向量
        /// </summary>
        private Vector3 GetRandomDirection()
        {
            if (_is3DMode)
            {
                // 3D空间中的随机方向
                return new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                ).normalized;
            }
            else
            {
                // 2D平面（X-Y）中的随机方向
                float angle = Random.Range(0f, Mathf.PI * 2);
                return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            }
        }

        /// <summary>
        /// 更新导弹旋转
        /// </summary>
        private void UpdateRotation()
        {

        }
        #endregion

        public override bool IsEnd()
        {
            return !IsRuning; // || (_getTargetPos != null && Vector3.Distance(CurPosition, _getTargetPos.Invoke()) <= _minDistanceToTarget);
        }
    }
}
