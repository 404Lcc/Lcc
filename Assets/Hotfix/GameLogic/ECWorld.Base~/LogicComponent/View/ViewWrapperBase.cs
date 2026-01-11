using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    //EViewCategory：View分类, 由项目定义维度和内容
    public static class EViewCategory
    {
        public const int MainGameObject = 0; // 主场景GameObject对象
        public const int MainUI = 1; // 主UI，Entity的主要表现是UI
        public const int MainFx = 2; // 主特效，主要表现是特效
        public const int Hp = 3; // 血条
        public const int AddedGameObject = 4; // 附加的GameObject

        // 下面属于附属逻辑，生命周期不跟随Main，可以随时增加或者删除
        public const int Range = 100; // 范围
    }


    public class SimpleGameObjectView : IViewWrapper
    {
        protected GameObjectPoolAsyncOperation _gameObject;
        protected Transform _transform;

        public GameObject GameObject
        {
            get { return _gameObject.GameObject; }
        }

        public Transform Transform
        {
            get { return _transform; }
        }

        public int Category { get; private set; }
        public bool IsActive { get; set; }
        public string ViewName { get; set; }
        public string BindPointName { get; set; }

        public bool ClearFx { get; set; }

        public MultChangeBool_AND IsVisible { get; set; }

        private Dictionary<string, Transform> _bpName2Transform = new Dictionary<string, Transform>();

        public SimpleGameObjectView(GameObjectPoolAsyncOperation gameObject, int category)
        {
            _gameObject = gameObject;
            _transform = _gameObject.Transform;
            Category = category;
            IsActive = true;
            IsVisible = new MultChangeBool_AND(true);
        }

        public virtual void DisposeView()
        {
            if (_gameObject == null)
            {
                Debug.LogError($"DisposeView m_gameObject == null: ViewName={ViewName}");
            }

            _gameObject?.Release(ref _gameObject);
            _gameObject = null;
            _bpName2Transform.Clear();
        }

        public virtual void SyncTransform(long entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            _transform.position = position;
            _transform.rotation = rotation;
            _transform.localScale = scale;
        }

        public Transform GetBindPoint(string bpName)
        {
            if (_bpName2Transform.TryGetValue(bpName, out var value))
            {
                return value;
            }

            var bpTrans = ModelBindPointGetter.GetBindPoint(Transform, bpName);
            if (bpTrans == null)
                return Transform;
            _bpName2Transform.Add(bpName, bpTrans);
            return bpTrans;
        }

        public void ModifyVisible(bool visible, int flag)
        {
            IsVisible.AddChange(visible, flag);
            if (_gameObject.IsDone)
            {
                GameObject.SetActive(IsVisible.Value);
            }
        }

        public void RemoveVisible(int flag)
        {
            IsVisible.RemoveChange(flag);
            if (_gameObject.IsDone)
            {
                GameObject.SetActive(IsVisible.Value);
            }
        }
    }

    public class MainUIView : IViewWrapper
    {
        public string UIName;

        public int Category { get; }
        public bool IsActive { get; set; }
        public string ViewName { get; set; }
        public string BindPointName { get; set; }

        public MainUIView(string uiName, int category)
        {
            UIName = uiName;
            Category = category;
            IsActive = true;
        }

        public void SyncTransform(long entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
        }

        public void ModifyVisible(bool visible, int flag)
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