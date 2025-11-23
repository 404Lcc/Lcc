using System;
using System.Collections.Generic;
using System.Linq;
using LccHotfix;
using Sirenix.OdinInspector;
using UnityEditor;

namespace LccEditor
{
    [Serializable]
    public class ReferencePoolInfoDisplay
    {
        [LabelText("类型名称")]
        [ShowInInspector]
        [ReadOnly]
        public string TypeName { get; set; }

        [LabelText("使用中")]
        [ShowInInspector]
        [ReadOnly]
        public int UsingReferenceCount { get; set; }

        [LabelText("空闲")]
        [ShowInInspector]
        [ReadOnly]
        public int UnusedReferenceCount { get; set; }

        [LabelText("获取/归还")]
        [ShowInInspector]
        [ReadOnly]
        public string AcquireReleaseRatio => $"{AcquireReferenceCount}/{ReleaseReferenceCount}";

        [LabelText("添加/移除")]
        [ShowInInspector]
        [ReadOnly]
        public string AddRemoveRatio => $"{AddReferenceCount}/{RemoveReferenceCount}";

        public int AcquireReferenceCount { get; set; }
        public int ReleaseReferenceCount { get; set; }
        public int AddReferenceCount { get; set; }
        public int RemoveReferenceCount { get; set; }
    }

    [MenuTree("引用池查看器", 9)]
    public class ReferencePoolViewerWindow : AEditorWindowBase
    {
        [PropertySpace(10)] [BoxGroup("引用池统计信息查看器", CenterLabel = true)] [HideLabel] [DisplayAsString]
        public string info = "引用池统计信息查看器";

        [PropertySpace(10)] [BoxGroup("过滤设置", CenterLabel = true)] [LabelText("只显示使用中池")] [OnValueChanged("OnFilterChanged")]
        public bool showOnlyUsing = false;

        [PropertySpace(10)] [BoxGroup("过滤设置")] [LabelText("搜索类型名称")] [OnValueChanged("OnFilterChanged")]
        public string searchTypeName = "";

        public ReferencePoolViewerWindow()
        {
        }

        public ReferencePoolViewerWindow(EditorWindow editorWindow) : base(editorWindow)
        {
        }

        private void OnFilterChanged()
        {
            RefreshData();
        }

        [PropertySpace(10)]
        [BoxGroup("统计概览", CenterLabel = true)]
        [ShowInInspector]
        [HideLabel]
        [EnableIf("@PoolInfoList.Count > 0")]
        public string Desc
        {
            get
            {
                int totalPools = PoolInfoList.Count;
                int totalUsing = PoolInfoList.Sum(x => x.UsingReferenceCount);
                int totalUnused = PoolInfoList.Sum(x => x.UnusedReferenceCount);
                int totalAcquired = PoolInfoList.Sum(x => x.AcquireReferenceCount);
                int totalReleased = PoolInfoList.Sum(x => x.ReleaseReferenceCount);
                int totalAdded = PoolInfoList.Sum(x => x.AddReferenceCount);
                int totalRemoved = PoolInfoList.Sum(x => x.RemoveReferenceCount);

                return $"总池数量: {totalPools} " + $"使用中对象: {totalUsing} " + $"空闲对象: {totalUnused} " + $"总获取次数: {totalAcquired} " + $"总归还次数: {totalReleased} " + $"总添加次数: {totalAdded} " + $"总移除次数: {totalRemoved}";
            }
        }

        [PropertySpace(10)]
        [BoxGroup("详细信息", CenterLabel = true, Order = 1)]
        [LabelText("引用池列表")]
        [ListDrawerSettings(
            IsReadOnly = false,
            Expanded = true,
            ShowItemCount = true,
            ShowPaging = true,
            DraggableItems = true,
            HideAddButton = true,
            HideRemoveButton = true
        )]
        public List<ReferencePoolInfoDisplay> PoolInfoList = new List<ReferencePoolInfoDisplay>();

        [PropertySpace(10)]
        [BoxGroup("操作面板", CenterLabel = true)]
        [HorizontalGroup("操作面板/Buttons")]
        [Button("立即刷新", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
        public void RefreshData()
        {
            var allPools = ReferencePool.GetAllReferencePoolInfos();
            if (allPools != null)
            {
                allPools = Filters(allPools);
                PoolInfoList = allPools.Select(x => new ReferencePoolInfoDisplay
                {
                    TypeName = x.Type.FullName,
                    UsingReferenceCount = x.UsingReferenceCount,
                    UnusedReferenceCount = x.UnusedReferenceCount,
                    AcquireReferenceCount = x.AcquireReferenceCount,
                    ReleaseReferenceCount = x.ReleaseReferenceCount,
                    AddReferenceCount = x.AddReferenceCount,
                    RemoveReferenceCount = x.RemoveReferenceCount,
                }).ToList();
            }
        }

        [PropertySpace(10)]
        [BoxGroup("操作面板")]
        [HorizontalGroup("操作面板/Buttons")]
        [Button("清空数据", ButtonSizes.Large), GUIColor(1f, 0.6f, 0.6f)]
        public void ClearData()
        {
            PoolInfoList.Clear();
        }

        private ReferencePoolInfo[] Filters(ReferencePoolInfo[] allPools)
        {
            IEnumerable<ReferencePoolInfo> filtered = allPools;

            //过滤
            if (showOnlyUsing)
            {
                filtered = filtered.Where(x => x.UsingReferenceCount > 0);
            }

            //过滤
            if (!string.IsNullOrEmpty(searchTypeName))
            {
                filtered = filtered.Where(x => x.Type.FullName.Contains(searchTypeName, StringComparison.OrdinalIgnoreCase));
            }

            return filtered.OrderByDescending(x => x.UsingReferenceCount).ThenByDescending(x => x.AcquireReferenceCount).ToArray();
        }
    }
}