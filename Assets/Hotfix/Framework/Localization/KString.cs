namespace LccHotfix
{
    /// <summary>
    /// 轻量 Format 封装，对齐 trunk KString 调用面。
    /// </summary>
    public static class KString
    {
        public static string Format(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
                return format;
            if (args == null || args.Length == 0)
                return format;
            return string.Format(format, args);
        }

        public static string Format<T>(string format, T arg0)
        {
            return string.Format(format, arg0);
        }

        public static string Format<T1, T2>(string format, T1 arg0, T2 arg1)
        {
            return string.Format(format, arg0, arg1);
        }

        public static string Format<T1, T2, T3>(string format, T1 arg0, T2 arg1, T3 arg2)
        {
            return string.Format(format, arg0, arg1, arg2);
        }

        public static string Format<T1, T2, T3, T4>(string format, T1 arg0, T2 arg1, T3 arg2, T4 arg3)
        {
            return string.Format(format, arg0, arg1, arg2, arg3);
        }

        public static string Format<T1, T2, T3, T4, T5>(string format, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4)
        {
            return string.Format(format, arg0, arg1, arg2, arg3, arg4);
        }

        public static string Format<T1, T2, T3, T4, T5, T6>(string format, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5)
        {
            return string.Format(format, arg0, arg1, arg2, arg3, arg4, arg5);
        }

        public static string Format<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6)
        {
            return string.Format(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static string Format<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7)
        {
            return string.Format(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
    }
}
