using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    // As: EViewCategory：View分类, 由项目定义维度和内容
    public static class EViewCategory
    {
        public const int MainGameObject = 0; // 主场景GameObject对象
        public const int MainUI = 1; // 主UI，Entity的主要表现是UI
        public const int MainFx = 2; // 主特效，主要表现是特效
        public const int Hp = 3; // 血条
        public const int AddedGameObject = 4; // 附加的GameObject
        public const int RecordSubObjrct = 5; // 只为了单独入池
        public const int RadarIcon = 6; // 雷达图标

        // 下面属于附属逻辑，生命周期不跟随Main，可以随时增加或者删除
        public const int Range = 100; // 范围
    }

    /// 测试最简 View
    public class SimpleGameObjectView : IViewWrapper
    {
        protected GameObjectHandle m_gameObject;

        public GameObject GameObject
        {
            get { return m_gameObject.GameObject; }
        }

        protected Transform m_transform;

        public Transform Transform
        {
            get { return m_transform; }
        }

        public SimpleGameObjectView(GameObjectHandle gameObject, int category)
        {
            m_gameObject = gameObject;
            m_transform = m_gameObject.Transform;
            Category = category;
            IsVisible = new MultChangeBool_AND(true);
        }

        public int Category { get; private set; }

        public string ViewName { get; set; }
        public string BindPointName { get; set; }

        public bool ClearFx { get; set; }

        public MultChangeBool_AND IsVisible { get; set; }

        private Dictionary<string, Transform> mBpName2Transform = new Dictionary<string, Transform>();

        public virtual void DisposeView()
        {
            if (m_gameObject == null)
            {
                UnityEngine.Debug.LogError($"DisposeView m_gameObject == null: ViewName={ViewName}");
            }

            m_gameObject?.Release(ref m_gameObject);
            m_gameObject = null;
            mBpName2Transform.Clear();
        }

        public virtual void SyncTransform(long entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            m_transform.position = position;
            m_transform.rotation = rotation;
            m_transform.localScale = scale;
        }

        public Transform GetBindPoint(string bpName)
        {
            if (mBpName2Transform.TryGetValue(bpName, out var value))
            {
                return value;
            }

            var bpTrans = ModelBindPointGetter.GetBindPoint(Transform, bpName);
            if (bpTrans == null)
                return Transform;
            mBpName2Transform.Add(bpName, bpTrans);
            return bpTrans;
        }

        public void ModifyVisible(bool visible, int flag)
        {
            IsVisible.AddChange(visible, flag);
            if (m_gameObject.IsDone)
            {
                GameObject.SetActive(IsVisible.Value);
            }
        }

        public void RemoveVisible(int flag)
        {
            IsVisible.RemoveChange(flag);
            if (m_gameObject.IsDone)
            {
                GameObject.SetActive(IsVisible.Value);
            }
        }
    }

    public class MainUIView : IViewWrapper
    {
        public string UIName;

        public int Category { get; }
        public string ViewName { get; set; }
        public string BindPointName { get; set; }

        public MainUIView(string uiName, int category)
        {
            UIName = uiName;
            Category = category;
        }

        public void SyncTransform(long entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
        }

        public void ModifyVisible(bool visible, int flag)
        {

        }

        public void RemoveVisible(int flag)
        {

        }

        public void HideView()
        {

        }

        public void DisposeView()
        {
            UIName = "";
        }
    }
}