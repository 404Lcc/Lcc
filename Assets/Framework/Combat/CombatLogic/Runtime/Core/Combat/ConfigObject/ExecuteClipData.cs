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

    [LabelText("ִ����Ŀ�괫������")]
    public enum ExecutionTargetInputType
    {
        [LabelText("None")]
        None = 0,
        [LabelText("����Ŀ��ʵ��")]
        Target = 1,
        [LabelText("����Ŀ���")]
        Point = 2,
    }

    [LabelText("�¼�����")]
    public enum FireEventType
    {
        [LabelText("����ɸѡ����")]
        FiltrationTarget = 0,
        [LabelText("��������Ч��")]
        AssignEffect = 1,
        [LabelText("������ִ����")]
        TriggerNewExecution = 2,
    }

    [LabelText("��ײ��ִ������")]
    public enum CollisionExecuteType
    {
        [LabelText("����ִ��")]
        OutOfHand = 0,
        [LabelText("ִ��ִ��")]
        InHand = 1,
    }
    [LabelText("��ײ����״")]
    public enum CollisionShape
    {
        [LabelText("Բ��")]
        Sphere,
        [LabelText("����")]
        Box,
        [LabelText("����")]
        Sector,
        [LabelText("�Զ���")]
        Custom,
    }

    [LabelText("��ײ��ִ������")]
    public enum CollisionMoveType
    {
        [LabelText("�̶�λ��")]
        FixedPosition,
        [LabelText("�̶�����")]
        FixedDirection,
        [LabelText("Ŀ�����")]
        TargetFly,
        [LabelText("�������")]
        ForwardFly,
        [LabelText("·������")]
        PathFly,
    }

    [LabelText("Ӧ��Ч��")]
    public enum EffectApplyType
    {
        [LabelText("ȫ��Ч��")]
        AllEffects,
        [LabelText("Ч��1")]
        Effect1,
        [LabelText("Ч��2")]
        Effect2,
        [LabelText("Ч��3")]
        Effect3,

        [LabelText("����")]
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
        [LabelText("��ִ����")]
        public string NewExecution;
    }
    [Serializable]
    public class CollisionExecuteData
    {
        public CollisionExecuteType ExecuteType;
        public ActionEventData ActionData;

        [Space(10)]
        public CollisionShape Shape;
        [ShowIf("Shape", CollisionShape.Sphere), LabelText("�뾶")]
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