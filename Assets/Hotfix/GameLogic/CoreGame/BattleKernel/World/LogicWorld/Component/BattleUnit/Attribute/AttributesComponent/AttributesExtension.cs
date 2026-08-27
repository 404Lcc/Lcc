namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 属性读取和修改的快捷扩展。
    /// </summary>
    public static class AttributesExtensionFoundation
    {
        /// <summary>
        /// 获取定点数属性值，读取失败时返回指定错误值。
        /// </summary>
        public static FixPoint GetAttributeFixPoint(this LogicEntity e, int key, FixPoint errorValue = default(FixPoint))
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("e.GetAttributeFixPoint  !HasAttributes");
                return errorValue;
            }
            var rv = errorValue;
            if (e.comAttributes.TryGetValue(key, ref rv))
                return rv;
            else
                return errorValue;
        }

        /// <summary>
        /// 获取 float 属性值，读取失败时返回指定错误值。
        /// </summary>
        public static float GetAttributeFloat(this LogicEntity e, int key, float errorValue = 0)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("e.GetAttribute  !HasAttributes");
                return errorValue;
            }

            var rv = errorValue;
            if (e.comAttributes.TryGetValue(key, ref rv))
                return rv;
            else
                return errorValue;
        }

        /// <summary>
        /// 获取 double 属性值，读取失败时返回指定错误值。
        /// </summary>
        public static double GetAttributeDouble(this LogicEntity e, int key, double errorValue = 0)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("e.GetAttribute  !HasAttributes");
                return errorValue;
            }

            var rv = errorValue;
            if (e.comAttributes.TryGetValue(key, ref rv))
                return rv;
            else
                return errorValue;
        }

        /// <summary>
        /// 获取 int 属性值，读取失败时返回指定错误值。
        /// </summary>
        public static int GetAttributeInt(this LogicEntity e, int key, int errorValue = 0)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("e.GetAttribute  !HasAttributes");
                return errorValue;
            }

            var rv = errorValue;
            if (e.comAttributes.TryGetValue(key, ref rv))
                return rv;
            else
                return errorValue;
        }

        /// <summary>
        /// 获取 bool 属性值，读取失败时返回指定错误值。
        /// </summary>
        public static bool GetAttributeBool(this LogicEntity e, int key, bool errorValue = true)
        {
            var rv = errorValue;
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("e.GetAttribute  !HasAttributes");
                return rv;
            }

            if (!e.hasComAttributes)
                return rv;
            if (e.comAttributes.TryGetValue(key, ref rv))
                return rv;
            else
                return rv;
        }

        /// <summary>
        /// 对 float 属性添加指定 flag 的修改值。
        /// </summary>
        public static void ModifyAttribute(this LogicEntity e, int key, float value, int flag)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("Actor.SetAttribute  !HasAttributes");
                return;
            }

            e.comAttributes.Modify(key, value, flag);
        }

        /// <summary>
        /// 对 int 属性添加指定 flag 的修改值。
        /// </summary>
        public static void ModifyAttribute(this LogicEntity e, int key, int value, int flag)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("Actor.SetAttribute  !HasAttributes");
                return;
            }

            e.comAttributes.Modify(key, value, flag);
        }

        /// <summary>
        /// 对 bool 属性添加指定 flag 的修改值。
        /// </summary>
        public static void ModifyAttribute(this LogicEntity e, int key, bool value, int flag)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("Actor.SetAttribute  !HasAttributes");
                return;
            }

            e.comAttributes.Modify(key, value, flag);
        }

        /// <summary>
        /// 移除指定类型属性上对应 flag 的修改值。
        /// </summary>
        public static void RemoveModify<T>(this LogicEntity e, int key, int flag)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("Actor.RemoveAttribute  !HasAttributes");
                return;
            }

            e.comAttributes.RemoveModify<T>(key, flag);
        }
    }
}
