using LccHotfix;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [MenuTree("引用池查看器", 8)]
    public class ReferencePoolViewerWindow : AEditorWindowBase
    {
        [BoxGroup("基本信息", CenterLabel = true)] [PropertySpace(10)] [HideLabel] [DisplayAsString] [InfoBox("点击下方按钮刷新数据", InfoMessageType.Info)]
        public string info = "引用池统计信息查看器";

        [BoxGroup("过滤设置", CenterLabel = true)] [PropertySpace(10)] [LabelText("只显示有活动的池")] [OnValueChanged("OnFilterChanged")]
        public bool showActiveOnly = false;

        [BoxGroup("过滤设置")] [PropertySpace(10)] [LabelText("搜索类型名称")] [InfoBox("输入类型名称进行搜索", InfoMessageType.None)]
        public string searchTypeName = "";

        [BoxGroup("操作面板", CenterLabel = true)]
        [HorizontalGroup("操作面板/Buttons")]
        [Button("立即刷新", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
        public void RefreshData()
        {
            var allPools = ReferencePool.GetAllReferencePoolInfos();

            // 应用过滤
            ApplyFilters(allPools);

            // 更新显示数据
            UpdateDisplayData();
        }

        [BoxGroup("操作面板")]
        [HorizontalGroup("操作面板/Buttons")]
        [Button("清空数据", ButtonSizes.Large), GUIColor(1f, 0.6f, 0.6f)]
        public void ClearData()
        {
            _poolInfos = null;
            PoolInfos = null;
        }

        [BoxGroup("统计概览", CenterLabel = true)]
        [ShowInInspector]
        [PropertySpace(10)]
        [HideLabel]
        [DisplayAsString]
        public string StatisticsOverview
        {
            get
            {
                if (_poolInfos == null)
                    return "数据未加载... 点击刷新按钮加载数据";

                int totalPools = _poolInfos.Length;
                int totalUsing = _poolInfos.Sum(p => p.UsingReferenceCount);
                int totalUnused = _poolInfos.Sum(p => p.UnusedReferenceCount);
                int totalAcquired = _poolInfos.Sum(p => p.AcquireReferenceCount);
                int totalReleased = _poolInfos.Sum(p => p.ReleaseReferenceCount);
                int totalAdded = _poolInfos.Sum(p => p.AddReferenceCount);
                int totalRemoved = _poolInfos.Sum(p => p.RemoveReferenceCount);

                return $"总池数量: {totalPools}\n" +
                       $"使用中对象: {totalUsing}\n" +
                       $"空闲对象: {totalUnused}\n" +
                       $"总获取次数: {totalAcquired}\n" +
                       $"总归还次数: {totalReleased}\n" +
                       $"总添加次数: {totalAdded}\n" +
                       $"总移除次数: {totalRemoved}";
            }
        }

        [BoxGroup("详细信息", CenterLabel = true)] [PropertySpace(10)] [LabelText("引用池列表")] [ListDrawerSettings]

        public ReferencePoolInfoDisplay[] PoolInfos;

        // 缓存的数据
        private ReferencePoolInfo[] _poolInfos;

        public ReferencePoolViewerWindow()
        {
        }

        public ReferencePoolViewerWindow(EditorWindow editorWindow) : base(editorWindow)
        {
        }

        [OnInspectorGUI]
        private void DrawCustomGUI()
        {
            if (_poolInfos == null)
            {
                Sirenix.Utilities.Editor.SirenixEditorGUI.InfoMessageBox("数据未加载，请点击刷新按钮加载数据");
            }
            else if (_poolInfos.Length == 0)
            {
                Sirenix.Utilities.Editor.SirenixEditorGUI.InfoMessageBox("没有找到匹配的引用池数据");
            }
        }

        private void ApplyFilters(ReferencePoolInfo[] allPools)
        {
            if (allPools == null)
            {
                _poolInfos = null;
                return;
            }

            IEnumerable<ReferencePoolInfo> filtered = allPools;

            // 活动池过滤
            if (showActiveOnly)
            {
                filtered = filtered.Where(p => p.UsingReferenceCount > 0 || p.UnusedReferenceCount > 0);
            }

            // 搜索过滤
            if (!string.IsNullOrEmpty(searchTypeName))
            {
                filtered = filtered.Where(p =>
                    p.Type.FullName?.Contains(searchTypeName, StringComparison.OrdinalIgnoreCase) == true ||
                    p.Type.Name?.Contains(searchTypeName, StringComparison.OrdinalIgnoreCase) == true);
            }

            _poolInfos = filtered.OrderByDescending(p => p.UsingReferenceCount)
                .ThenByDescending(p => p.AcquireReferenceCount)
                .ToArray();
        }

        private void UpdateDisplayData()
        {
            if (_poolInfos == null)
            {
                PoolInfos = null;
                return;
            }

            PoolInfos = _poolInfos.Select(p => new ReferencePoolInfoDisplay
            {
                TypeName = GetTypeDisplayName(p.Type),
                UsingCount = p.UsingReferenceCount,
                UnusedCount = p.UnusedReferenceCount,
                AcquireCount = p.AcquireReferenceCount,
                ReleaseCount = p.ReleaseReferenceCount,
                AddCount = p.AddReferenceCount,
                RemoveCount = p.RemoveReferenceCount,
            }).ToArray();
        }

        private void OnFilterChanged()
        {
            if (_poolInfos != null)
            {
                var allPools = ReferencePool.GetAllReferencePoolInfos();
                ApplyFilters(allPools);
                UpdateDisplayData();
            }
        }

        private string GetTypeDisplayName(Type type)
        {
            if (type == null) return "Unknown";
            return type.FullName ?? type.Name;
        }

        // 为每个引用池信息添加自定义显示
        [System.Serializable]
        public class ReferencePoolInfoDisplay
        {

            [LabelText("类型名称")] [ShowInInspector] public string TypeName { get; set; }


            [LabelText("使用中")] [ShowInInspector] public int UsingCount { get; set; }


            [LabelText("空闲")] [ShowInInspector] public int UnusedCount { get; set; }


            [LabelText("获取/归还")] [ShowInInspector] public string AcquireReleaseRatio => $"{AcquireCount}/{ReleaseCount}";


            [LabelText("添加/移除")] [ShowInInspector] public string AddRemoveRatio => $"{AddCount}/{RemoveCount}";

            [ShowInInspector] public int AcquireCount { get; set; }

            [ShowInInspector] public int ReleaseCount { get; set; }

            [ShowInInspector] public int AddCount { get; set; }

            [ShowInInspector] public int RemoveCount { get; set; }



            // 详细展开信息

            [ShowInInspector]
            public string DetailedInfo
            {
                get
                {
                    return $"类型: {TypeName}\n" +
                           $"使用中: {UsingCount}\n" +
                           $"空闲: {UnusedCount}\n" +
                           $"获取次数: {AcquireCount}\n" +
                           $"归还次数: {ReleaseCount}\n" +
                           $"添加次数: {AddCount}\n" +
                           $"移除次数: {RemoveCount}";
                }
            }


        }
    }
}