using System;
using System.Collections.Generic;

namespace HotUpdate.Framework.PbCfg
{
    public static class PbCfg
    {
        private static readonly Dictionary<Type, Dictionary<uint, object>> DataMap = new Dictionary<Type, Dictionary<uint, object>>();

        public static T GetData<T>(uint id)
        {
            if (DataMap.TryGetValue(typeof(T), out var typedData) && typedData.TryGetValue(id, out var data))
            {
                return (T)data;
            }

            return default;
        }

        public static T GetData<T>(int id)
        {
            if (id < 0)
            {
                return default;
            }

            return GetData<T>((uint)id);
        }

        public static void RegisterData<T>(uint id, T data)
        {
            if (data is null)
            {
                return;
            }

            var type = typeof(T);
            if (!DataMap.TryGetValue(type, out var typedData))
            {
                typedData = new Dictionary<uint, object>();
                DataMap.Add(type, typedData);
            }

            typedData[id] = data;
        }

        public static void RegisterData<T>(IEnumerable<T> dataList, Func<T, uint> idGetter)
        {
            if (dataList is null || idGetter is null)
            {
                return;
            }

            foreach (var data in dataList)
            {
                if (data is null)
                {
                    continue;
                }

                RegisterData(idGetter(data), data);
            }
        }

        public static void Clear()
        {
            DataMap.Clear();
        }
    }
}
