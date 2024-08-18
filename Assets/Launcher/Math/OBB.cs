using System;
using UnityEngine;

namespace LccModel
{
    public class OBB
    {
        public Vector3 pos;
        public Vector3 size;
        public Vector3 xAxis;
        public Vector3 yAxis;
        public Vector3 zAxis;

        public bool IsCollision(OBB box2)
        {
            OBB box1 = this;
            Vector3 RPos;
            RPos = box2.pos - box1.pos;

            return !(
                GetSeparatingPlane(RPos, box1.xAxis, box1, box2) ||
                GetSeparatingPlane(RPos, box1.yAxis, box1, box2) ||
                GetSeparatingPlane(RPos, box1.zAxis, box1, box2) ||

                GetSeparatingPlane(RPos, box2.xAxis, box1, box2) ||
                GetSeparatingPlane(RPos, box2.yAxis, box1, box2) ||
                GetSeparatingPlane(RPos, box2.zAxis, box1, box2) ||

                GetSeparatingPlane(RPos, Vector3.Cross(box1.xAxis, box2.xAxis), box1, box2) ||
                GetSeparatingPlane(RPos, Vector3.Cross(box1.xAxis, box2.yAxis), box1, box2) ||
                GetSeparatingPlane(RPos, Vector3.Cross(box1.xAxis, box2.zAxis), box1, box2) ||

                GetSeparatingPlane(RPos, Vector3.Cross(box1.yAxis, box2.xAxis), box1, box2) ||
                GetSeparatingPlane(RPos, Vector3.Cross(box1.yAxis, box2.yAxis), box1, box2) ||
                GetSeparatingPlane(RPos, Vector3.Cross(box1.yAxis, box2.zAxis), box1, box2) ||

                GetSeparatingPlane(RPos, Vector3.Cross(box1.zAxis, box2.xAxis), box1, box2) ||
                GetSeparatingPlane(RPos, Vector3.Cross(box1.zAxis, box2.yAxis), box1, box2) ||
                GetSeparatingPlane(RPos, Vector3.Cross(box1.zAxis, box2.zAxis), box1, box2));
        }

        /// <summary>
        /// 检查是否有分离轴 SAT算法，如果有的话没碰撞，没有就碰撞了
        /// </summary>
        /// <returns></returns>
        public bool GetSeparatingPlane(Vector3 RPos, Vector3 Plane, OBB box1, OBB box2)
        {
            return Math.Abs(Vector3.Dot(RPos, Plane)) >
                (Math.Abs(Vector3.Dot(box1.xAxis * (box1.size.x / 2), Plane)) +
                Math.Abs(Vector3.Dot(box1.yAxis * (box1.size.y / 2), Plane)) +
                Math.Abs(Vector3.Dot(box1.zAxis * (box1.size.z / 2), Plane)) +
                Math.Abs(Vector3.Dot(box2.xAxis * (box2.size.x / 2), Plane)) +
                Math.Abs(Vector3.Dot(box2.yAxis * (box2.size.y / 2), Plane)) +
                Math.Abs(Vector3.Dot(box2.zAxis * (box2.size.z / 2), Plane)));
        }
    }
}