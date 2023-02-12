using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace LccModel
{
    public class AbilityItemBezierMoveComponent : Component
    {
        public IPosition positionEntity;
        public Vector3 originPosition;
        public float rotateAgree;
        public List<PathPoint> pointList;
        public float duration;
        public float speed  = 0.05f;
        float progress;


        public void DOMove()
        {
            progress = 0.1f;
            var endValue = Evaluate(progress);
            var startPos = positionEntity.Position;
            DOTween.To(() => startPos, (x) => positionEntity.Position = x, endValue, speed).SetEase(Ease.Linear).OnComplete(DOMoveNext);
        }

        private void DOMoveNext()
        {
            if (progress >= 1f)
            {
                return;
            }
            progress += 0.1f;
            progress = Mathf.Min(1f, progress);
            var endValue = Evaluate(progress);
            var startPos = positionEntity.Position;
            DOTween.To(() => startPos, (x) => positionEntity.Position = x, endValue, speed).SetEase(Ease.Linear).OnComplete(DOMoveNext);
        }

        public Vector3 Evaluate(float t, int derivativeOrder = 0)
        {
            if (pointList.Count == 0) return positionEntity.Position;
            if (pointList.Count == 1) return pointList[0].position;

            t = Mathf.Clamp(t, 0, 1f);
            t = t * pointList.Count;
            int segment_index = (int)t;

            if (segment_index + 1 >= pointList.Count)
            {
                var v = pointList[segment_index].position;
                var a = rotateAgree;
                var x = v.x;
                var y = v.z;
                var x1 = x * Mathf.Cos(a) - y * Mathf.Sin(a);
                var y1 = -(y * Mathf.Cos(a) + x * Mathf.Sin(a));

                v = originPosition + new Vector3(x1, v.y, y1);
                return v;
            }
            Vector3[] p = new Vector3[4];
            p[0] = pointList[segment_index].position;
            p[1] = pointList[segment_index].OutTangent + p[0];
            p[3] = pointList[segment_index + 1].position;
            p[2] = pointList[segment_index + 1].InTangent + p[3];

            t = t - segment_index;
            float u = 1 - t;
            if (derivativeOrder < 0) derivativeOrder = 0;
            //原函数
            if (derivativeOrder == 0)
            {
                var v = p[0] * u * u * u + 3 * p[1] * u * u * t + 3 * p[2] * u * t * t + p[3] * t * t * t;

                var a = rotateAgree;
                var x = v.x;
                var y = v.z;
                var x1 = x * Mathf.Cos(a) - y * Mathf.Sin(a);
                var y1 = -(y * Mathf.Cos(a) + x * Mathf.Sin(a));

                v = originPosition + new Vector3(x1, v.y, y1);
                return v;
            }
            return Vector3.zero;
        }
    }
}