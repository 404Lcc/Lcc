using System.Collections.Generic;
using PBConfig;
using Vector3 = UnityEngine.Vector3;

namespace LccHotfix
{


public enum HitType
{
    Entity,
    Obstacle,
    EntityPart,
}

public struct HitInfo
{
    public long selfEntityId; //自身entityId
    public long hitEntityID;
    public Vector3 hitPos;
    public Vector3 hitDir;
    public HitType hitType;
}

//支持碰撞间隔，支持范围碰撞，支持单个物体多次碰撞，支持最大碰撞次数
public class SubobjectColliderHandlerBase : ColliderHandler, IEntityColliderCheckActive, IEntityHitEntityHandler
{
    private TSubobject _subobjectCfg;
    private Dictionary<long, int> _hitCount; //碰撞者id 次数
    private int _currentHitCount; //当前碰撞次数
    internal float _hitIntervalTimer; //碰撞计时器


    protected bool HitWithLife => _subobjectCfg.HitWithLife;
    public int MaxHitCount { get; set; }
    protected float HitInterval => _subobjectCfg.HitInterval;
    protected int SingleHitCount => _subobjectCfg.SingleHitCount;

    private float mHitIntervalReduce = 0;

    protected bool IsHitObstacle; //是否碰撞阻挡物
    protected bool IsHitObstacleCounted; //碰撞阻挡物是否计算到次数中
    protected bool IgnoreHitSelf; //是否忽略碰撞自己


    /// <summary>
    /// 子物体碰撞器初始化
    /// </summary>
    /// <param name="subObjCfg"></param>
    public virtual void Init(TSubobject subObjCfg)
    {
        _subobjectCfg = subObjCfg;
        RawHitMaker.SetCapacity(32);

        MaxHitCount = _subobjectCfg.MaxHitCount;
        
        _hitCount = new Dictionary<long, int>();
        _currentHitCount = 0;
        _hitIntervalTimer = 0;
        mHitIntervalReduce = 0;

        IsHitObstacle = true;
        IsHitObstacleCounted = true;
        IgnoreHitSelf = false;
    }


    /// <summary>
    /// 判断自身符合碰撞条件
    /// </summary>
    /// <param name="selfEntity"></param>
    /// <returns></returns>
    public virtual bool IsActiveAsSource(LogicEntity selfEntity)
    {
        if (!selfEntity.hasComID)
            return false;

        if (selfEntity.hasComDeath)
            return false;

        if (CheckMaxHitCountAsSource())
            return false;

        if (_hitIntervalTimer > 0)
            return false;

        return true;
    }
    
    /// <summary>
    /// 检测自身碰撞次数是否达到最大
    /// </summary>
    protected virtual bool CheckMaxHitCountAsSource()
    {
        if (_currentHitCount >= MaxHitCount)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 判断两个entity满足碰撞条件
    /// </summary>
    /// <param name="selfEntity"></param>
    /// <param name="hitEntity"></param>
    /// <returns></returns>
    protected virtual bool Check2EntityHitActive(LogicEntity selfEntity, LogicEntity hitEntity)
    {
        if (selfEntity == null)
            return false;

        if (hitEntity == null)
            return false;

        if (!selfEntity.hasComID)
            return false;

        if (!selfEntity.hasComTransform)
            return false;
        
        if (!selfEntity.hasComFaction)
            return false;

        if (selfEntity.hasComDeath)
            return false;

        if (!hitEntity.hasComID)
            return false;

        if (!hitEntity.hasComTransform)
            return false;

        if (!hitEntity.hasComFaction)
            return false;

        if (hitEntity.hasComDeath)
            return false;

        if (!hitEntity.hasComAttributes)
            return false;

        if (selfEntity.hasComSubobject)
        {
            var comSubobj = selfEntity.comSubobject;
            //排除主人
            if (hitEntity.ID == comSubobj.SpellEntityId)
            {
                return false;
            }

            //排除碰撞白名单
            if (comSubobj.Logic.VarEnvRef.HasVar<bool>(CvKey.CV_HitWhiteList))
            {
                comSubobj.Logic.VarEnvRef.ReadVar(CvKey.CV_HitWhiteList, out List<int> list);
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        if (item == hitEntity.ID)
                        {
                            return false;
                        }
                    }
                }
            }

            //自己作为子物体不能碰隐形
            if (!comSubobj.Cfg.HitCloak && hitEntity.IsCloaked())
            {
                return false;
            }
        }

        //排除自己
        if (hitEntity == selfEntity)
        {
            return false;
        }

        //排除友方 
        if (hitEntity.comFaction.Faction == selfEntity.comFaction.Faction)
        {
            return false;
        }

        //排除不能被碰撞的
        if (!hitEntity.comAttributes.GetValue<bool>(AttributeBool.CanCollision, true))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 生成生数据
    /// </summary>
    /// <param name="selfEntity"></param>
    /// <param name="dt"></param>
    /// <returns></returns>
    public override bool CheckRawHits(LogicEntity selfEntity, float dt)
    {
        //主动碰撞间隔
        if (_hitIntervalTimer > 0)
        {
            _hitIntervalTimer -= dt;
        }

        if (!IsActiveAsSource(selfEntity))
            return false;

        return base.CheckRawHits(selfEntity, dt);
    }


    /// <summary>
    /// 处理生数据 触发碰撞
    /// </summary>
    /// <param name="selfEntity"></param>
    /// <param name="rawHits"></param>
    /// <param name="dt"></param>
    public override void HandleRawHits(LogicEntity selfEntity, RawHit[] rawHits, float dt)
    {
        foreach (var rawHit in rawHits)
        {
            var hitCollider = rawHit.Collider;
            var hitPoint = rawHit.Point;
            if (hitCollider == null)
                continue;

            //todo 暂时不处理碰撞障碍物
            // if (IsHitObstacle && hitCollider.tag == "Obstacle")
            // {
            //     HandleHitObstacle(selfEntity, hitPoint);
            //     continue;
            // }

            var hitEntity = hitCollider;
            if (!Check2EntityHitActive(selfEntity, hitEntity))
                continue;

            //todo 暂时不判断isPart
            //处理 A碰B
            HandleHitEntity(selfEntity, hitEntity, hitPoint, false);

            //处理 B碰A
            if (hitEntity.hasComCollider && !IgnoreHitSelf)
            {
                var handler = hitEntity.comCollider.handler;
                if (handler == null)
                {
                    continue;
                }

                if (handler is IEntityColliderCheckActive active && handler is IEntityHitEntityHandler hitEntityHandler)
                {
                    if (active.IsActiveAsSource(hitEntity))
                    {
                        hitEntityHandler.HandleHitEntity(hitEntity, selfEntity, hitPoint, false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 碰撞其他entity
    /// </summary>
    /// <param name="selfEntity"></param>
    /// <param name="hitEntity"></param>
    /// <param name="hitPoint"></param>
    /// <param name="isPart"></param>
    public virtual void HandleHitEntity(LogicEntity selfEntity, LogicEntity hitEntity, Vector3 hitPoint, bool isPart)
    {
        if (!Check2EntityHitActive(selfEntity, hitEntity))
        {
            return;
        }

        _hitIntervalTimer = HitInterval - mHitIntervalReduce;
        if (BattleLogger.IsDebugEnabled)
            BattleLogger.LogDebug($"[碰撞冷却] selfEntity={selfEntity.ID}, hitEntity={hitEntity.ID}, 碰撞间隔={HitInterval}s, 间隔减少={mHitIntervalReduce}s, 实际冷却={_hitIntervalTimer:F3}s, 当前碰撞次数={_currentHitCount}/{MaxHitCount}");

        HitInfo hitInfo = new HitInfo();
        hitInfo.hitPos = hitPoint;
        hitInfo.hitDir = hitEntity.comTransform.position - selfEntity.comTransform.position;
        hitInfo.hitEntityID = hitEntity.ID;
        hitInfo.hitType = isPart ? HitType.EntityPart : HitType.Entity;
        if (!TryAddHitCount(hitInfo))
            return;
        if (selfEntity.hasComSubobject)
        {
            SendHitCommand(selfEntity, hitInfo);
        }
    }

    /// <summary>
    /// 碰撞障碍物
    /// </summary>
    /// <param name="selfEntity"></param>
    /// <param name="hitPoint"></param>
    protected virtual void HandleHitObstacle(LogicEntity selfEntity, Vector3 hitPoint)
    {
        HitInfo hitInfo = new HitInfo();
        hitInfo.hitEntityID = -1;
        hitInfo.hitPos = hitPoint;
        hitInfo.hitDir = selfEntity.comTransform.rotation * Vector3.right;
        hitInfo.hitType = HitType.Obstacle;
        if (!TryAddHitCount(hitInfo))
            return;
        if (selfEntity.hasComSubobject)
        {
            SendHitCommand(selfEntity, hitInfo);
        }
    }

    /// <summary>
    /// 记录碰撞次数
    /// </summary>
    /// <param name="selfEntity"></param>
    /// <param name="hitInfo"></param>
    /// <param name="hitEntity"></param>
    protected virtual bool TryAddHitCount(HitInfo hitInfo)
    {
        var hitEntityID = hitInfo.hitEntityID;
        if (_currentHitCount >= MaxHitCount)
        {
            return false;
        }
        if (hitEntityID >= 0)
        {
            if (_hitCount.ContainsKey(hitEntityID))
            {
                var hitCount = _hitCount[hitEntityID];
                if (hitCount >= SingleHitCount)
                    return false;

                _hitCount[hitEntityID]++;
            }
            else
            {
                _hitCount.Add(hitEntityID, 1);
            }

            _currentHitCount++;
        }

        if (hitEntityID == -1 && IsHitObstacle && IsHitObstacleCounted)
        {
            _currentHitCount++;
        }

        return true;
    }

    public void ReduceHitInterval(float reduceNum)
    {
        mHitIntervalReduce += reduceNum;
    }
    
    public void ReduceHitIntervalPercent(float percent)
    {
        mHitIntervalReduce += HitInterval * percent;
    }


    /// <summary>
    /// 发送碰撞命令
    /// </summary>
    /// <param name="selfEntity"></param>
    /// <param name="hitInfo"></param>
    /// <param name="hitEntity"></param>
    /// <param name="hitEntityID"></param>
    protected void SendHitCommand(LogicEntity selfEntity, HitInfo hitInfo)
    {
        var comSubobject = selfEntity.comSubobject;
        hitInfo.selfEntityId = selfEntity.ID;
        var cmd = new EntityCommand()
        {
            CmdType = EntityCmdType.Nt_ColliderHit,
            EntityID = selfEntity.ID,
            V0 = HitWithLife,
            V1 = MaxHitCount - _currentHitCount,
            HitInfo = hitInfo,
        };
        selfEntity.SendCmd(cmd);
    }

    /// <summary>
    /// 回收
    /// </summary>
    public override void OnRecycle()
    {
        base.OnRecycle();

        _subobjectCfg = null;

        _hitCount = new Dictionary<long, int>();
        _currentHitCount = 0;
        _hitIntervalTimer = 0;

        IsHitObstacle = true;
        IsHitObstacleCounted = true;
        IgnoreHitSelf = false;
    }
}
}
