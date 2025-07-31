using System.Collections.Generic;

namespace RVO
{
    /// <summary>
    /// 障碍物图形
    /// </summary>
    public class ObstaclePolygon
    {
        /// <summary>
        /// 顶点
        /// </summary>
        internal IList<Vector2> _vertices;

        /// <summary>
        /// 是否需要删除
        /// </summary>
        internal bool _isNeedDelete;

        /// <summary>
        /// id
        /// </summary>
        public int _id;

        public ObstaclePolygon()
        {
            this._vertices = new List<Vector2>();
        }
    }
}