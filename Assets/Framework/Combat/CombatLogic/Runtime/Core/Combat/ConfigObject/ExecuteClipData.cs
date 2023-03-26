using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LccModel
{
    public enum ExecuteClipType
    {
        CollisionExecute = 0,
        ActionEvent = 1,
        Animation = 2,
        Audio = 3,
        ParticleEffect = 4,
    }

    [LabelText("执行体目标传入类型")]
    public enum ExecutionTargetInputType
    {
        [LabelText("None")]
        None = 0,
        [LabelText("传入目标实体")]
        Target = 1,
        [LabelText("传入目标点")]
        Point = 2,
    }

    [LabelText("事件类型")]
    public enum FireEventType
    {
        [LabelText("触发筛选敌人")]
        FiltrationTarget = 0,
        [LabelText("触发赋给效果")]
        AssignEffect = 1,
        [LabelText("触发新执行体")]
        TriggerNewExecution = 2,
    }

    [LabelText("碰撞体执行类型")]
    public enum CollisionExecuteType
    {
        [LabelText("脱手执行")]
        OutOfHand = 0,
        [LabelText("执手执行")]
        InHand = 1,
    }
    [LabelText("碰撞体形状")]
    public enum CollisionShape
    {
        [LabelText("圆形")]
        Sphere,
        [LabelText("矩形")]
        Box,
        [LabelText("扇形")]
        Sector,
        [LabelText("自定义")]
        Custom,
    }

    [LabelText("碰撞体执行类型")]
    public enum CollisionMoveType
    {
        [LabelText("固定位置")]
        FixedPosition,
        [LabelText("固定方向")]
        FixedDirection,
        [LabelText("目标飞行")]
        TargetFly,
        [LabelText("朝向飞行")]
        ForwardFly,
        [LabelText("路径飞行")]
        PathFly,
    }

    [LabelText("应用效果")]
    public enum EffectApplyType
    {
        [LabelText("全部效果")]
        AllEffects,
        [LabelText("效果1")]
        Effect1,
        [LabelText("效果2")]
        Effect2,
        [LabelText("效果3")]
        Effect3,

        [LabelText("其他")]
        Other = 100,
    }

    public enum BezierPointType
    {
        Corner,
        Smooth,
        BezierCorner,
    }

    [Serializable]
    public class PathPoint
    {
        public BezierPointType type;
        public Vector3 position;
        [SerializeField]
        Vector3 inTangent;
        [SerializeField]
        Vector3 outTangent;

        public Vector3 InTangent
        {
            get
            {
                if (type == BezierPointType.Corner) return Vector3.zero;
                else return inTangent;
            }
            set
            {
                if (type != BezierPointType.Corner) inTangent = value;
                if (value.sqrMagnitude > 0.001 && type == BezierPointType.Smooth)
                {
                    outTangent = value.normalized * (-1) * outTangent.magnitude;
                }
            }
        }

        public Vector3 OutTangent
        {
            get
            {
                if (type == BezierPointType.Corner) return Vector3.zero;
                if (type == BezierPointType.Smooth)
                {
                    if (inTangent.sqrMagnitude > 0.001)
                    {
                        return inTangent.normalized * (-1) * outTangent.magnitude;
                    }
                }
                return outTangent;
            }
            set
            {
                if (type == BezierPointType.Smooth)
                {
                    if (value.sqrMagnitude > 0.001)
                    {
                        inTangent = value.normalized * (-1) * inTangent.magnitude;
                    }
                    outTangent = value;
                }
                if (type == BezierPointType.BezierCorner) outTangent = value;
            }
        }
    }

    [Serializable]
    public class ActionEventData
    {
        public FireEventType ActionEventType;
        [ShowIf("ActionEventType", FireEventType.AssignEffect)]
        public EffectApplyType EffectApply;
        [ShowIf("ActionEventType", FireEventType.TriggerNewExecution)]
        [LabelText("新执行体")]
        public string NewExecution;
    }
    [Serializable]
    public class CollisionExecuteData
    {
        public CollisionExecuteType ExecuteType;
        public ActionEventData ActionData;

        [Space(10)]
        public CollisionShape Shape;
        [ShowIf("Shape", CollisionShape.Sphere), LabelText("半径")]
        public float Radius;

        [ShowIf("Shape", CollisionShape.Box)]
        public Vector3 Center;
        [ShowIf("Shape", CollisionShape.Box)]
        public Vector3 Size;

        [Space(10)]
        public CollisionMoveType MoveType;

        public string AssetName;


        [ShowIf("MoveType", CollisionMoveType.PathFly)]
        public List<PathPoint> PointList;

        public List<PathPoint> GetPointList()
        {
            var list = new List<PathPoint>();
            foreach (var item in PointList)
            {
                var newPoint = new PathPoint();
                newPoint.position = item.position;
                newPoint.type = item.type;
                newPoint.InTangent = item.InTangent;
                newPoint.OutTangent = item.OutTangent;
                list.Add(newPoint);
            }
            return list;
        }
    }

    public class ExecuteClipData : ScriptableObject
    {
        public float TotalTime { get; set; }
        public float StartTime;
        public float EndTime;

        [ShowInInspector]
        [PropertyOrder(-1)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public ExecuteClipType ExecuteClipType;

        [Space(10)]
        [ShowIf("ExecuteClipType", ExecuteClipType.ActionEvent)]
        public ActionEventData ActionEventData;

        [Space(10)]
        [ShowIf("ExecuteClipType", ExecuteClipType.CollisionExecute)]
        public CollisionExecuteData CollisionExecuteData;

        [Space(10)]
        [ShowIf("ExecuteClipType", ExecuteClipType.Animation)]
        public AnimationData AnimationData;

        [Space(10)]
        [ShowIf("ExecuteClipType", ExecuteClipType.Audio)]
        public AudioData AudioData;

        [Space(10)]
        [ShowIf("ExecuteClipType", ExecuteClipType.ParticleEffect)]
        public ParticleEffectData ParticleEffectData;

        public float Duration { get => (EndTime - StartTime); }

        public ExecuteClipData GetClipTime()
        {
            return this;
        }
    }

    [Serializable]
    public class ParticleEffectData
    {
        public string ParticleEffectName;
    }

    [Serializable]
    public class AnimationData
    {
        public AnimationType AnimationType;
    }

    [Serializable]
    public class AudioData
    {
        public string AudioClipName;
    }
}