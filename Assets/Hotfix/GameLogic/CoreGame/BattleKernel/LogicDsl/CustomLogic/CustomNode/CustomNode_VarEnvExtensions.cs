namespace LccHotfix
{
    public static class CustomNodeVarEnvExtensions
    {
        #region 黑板变量运算

        /// <summary>
        /// 对黑板中的整型变量做加法并返回新值。
        /// </summary>
        public static int IntVarAdd(this CustomNode self, string key, int add)
        {
            var v = self.GetVar<int>(key, 0);
            self.SetVar<int>(key, v + add);
            return v + add;
        }

        /// <summary>
        /// 对黑板中的浮点变量做加法并返回新值。
        /// </summary>
        public static float FloatVarAdd(this CustomNode self, string key, float add)
        {
            var v = self.GetVar<float>(key, 0f);
            self.SetVar<float>(key, v + add);
            return v + add;
        }

        /// <summary>
        /// 对黑板中的整型变量按倍率相乘，变量不存在时返回 false。
        /// </summary>
        public static bool IntVarMult(this CustomNode self, string key, float rate)
        {
            if (!self.HasVar<int>(key))
            {
                self.LogError($"node.IntVarMult 未找到黑板变量 key={key}");
                return false;
            }

            var v = self.GetVar<int>(key);
            self.SetVar<int>(key, (int)(v * rate));
            return true;
        }

        /// <summary>
        /// 对黑板中的浮点变量按倍率相乘，变量不存在时返回 false。
        /// </summary>
        public static bool FloatVarMult(this CustomNode self, string key, float rate, bool debugLog = false)
        {
            if (!self.HasVar<float>(key))
            {
                self.LogError($"node.FloatVarMult 未找到黑板变量 key={key}");
                return false;
            }

            var v = self.GetVar<float>(key);
            self.SetVar<float>(key, v * rate);
            if (debugLog)
            {
                self.LogError($"FloatVarMult v({v}) * ({rate}) -> ({v * rate})");
            }

            return true;
        }

        #endregion
    }
}
