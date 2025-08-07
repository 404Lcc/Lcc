using System;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace LccModel
{
    public class ResObject : MonoBehaviour
    {
        protected enum LoadState
        {
            None,
            Loading,
            Done,
            Error,
        }

        private event Action<string, Object> _onComplete;

        protected GameObject _bindObject;
        protected string _assetName;

        protected LoadState _state;
        protected float _loadStartTime;

        protected HandleBase _handleBase;
        protected Type _type;
        protected Object _obj;

        public string AssetName => _assetName;
        public bool Loaded => _state == LoadState.Done || _state == LoadState.Error;
        public bool Loading => _state == LoadState.Loading;
        public Type Type => _type;
        public Object Asset => _obj;

        private void OnDestroy()
        {
            _bindObject = null;
            Release();
        }

        public void SetInfo<T>(GameObject bind, string asset, Action<string, Object> onComplete)
        {
            this._bindObject = bind;
            this._assetName = asset;
            this._type = typeof(T);
            this._onComplete = onComplete;
            this._state = LoadState.None;
        }

        public T GetAsset<T>() where T : Object
        {
            if (_obj != null)
                return _obj as T;
            return null;
        }

        private bool Equals(string asset)
        {
            return _assetName == asset;
        }

        #region 加载资源
        protected virtual void Load()
        {
            bool valid = AssetManager.Instance.CheckLocationValid(_assetName);
            if (!valid)
            {
                _state = LoadState.Error;
                LoadEnd();
                return;
            }
            _handleBase = AssetManager.Instance.LoadAssetSync(_assetName, _type);
            _obj = ((AssetHandle)_handleBase).AssetObject;
            if (_obj != null)
            {
                _state = LoadState.Done;
            }
            else
            {
                _state = LoadState.Error;
            }
            LoadEnd();
        }

        protected virtual void StartLoad()
        {
            bool valid = AssetManager.Instance.CheckLocationValid(_assetName);
            if (!valid)
            {
                _state = LoadState.Error;
                LoadEnd();
                return;
            }
            _state = LoadState.Loading;
            _loadStartTime = Time.realtimeSinceStartup;

            _handleBase = AssetManager.Instance.LoadAssetAsync(_assetName, _type);
            ((AssetHandle)_handleBase).Completed += OnCompleted;
        }

        public virtual void LoadEnd()
        {
            if (_bindObject == null)
                return;
            if (_onComplete != null)
            {
                _onComplete(_assetName, _obj);
                _onComplete = null;
            }
        }

        private void OnCompleted(AssetHandle assetHandle)
        {
            _state = LoadState.Done;
            _obj = assetHandle.AssetObject;
            LoadEnd();
        }
        #endregion

        #region 卸载
        public void Release()
        {
            if (_handleBase == null)
                return;
            
            if (_handleBase.IsValid)
            {
                if (_handleBase is AssetHandle assetHandle)
                {
                    assetHandle.Release();
                }
                if (_handleBase is AllAssetsHandle allAssetsHandle)
                {
                    allAssetsHandle.Release();
                }
            }
            _handleBase = null;
            _assetName = null;
            _state = LoadState.None;
            _obj = null;
            _type = null;
            _onComplete = null;
        }
        #endregion

        #region 加载接口
        public static ResObject LoadRes<T>(GameObject loader, string asset) where T : Object
        {
            if (loader != null && !string.IsNullOrEmpty(asset))
            {
                // 别在同一对象上通过同步和异步加载同一资源
                ResObject[] old = loader.GetComponents<ResObject>();
                if (old != null && old.Length > 0)
                {
                    for (int i = 0; i < old.Length; i++)
                    {
                        if (old[i].Equals(asset) && old[i].Loaded)
                        {
                            return old[i];
                        }
                    }
                    for (int i = 0; i < old.Length; i++)
                    {
                        // 空的没用的对象，一般通过复制对象copy过来的
                        if (string.IsNullOrEmpty(old[i]._assetName))
                        {
                            ResObject oldRes = old[i];
                            oldRes.SetInfo<T>(loader, asset, null);
                            oldRes.Load();
                            return oldRes;
                        }
                    }
                }

                ResObject res = loader.AddComponent<ResObject>();
                res.SetInfo<T>(loader, asset, null);
                res.Load();
                return res;
            }
            return null;
        }
        public static ResObject StartLoadRes<T>(GameObject loader, string asset, Action<string, Object> onComplete) where T : Object
        {
            if (loader != null && !string.IsNullOrEmpty(asset))
            {
                ResObject res = null;
                ResObject[] old = loader.GetComponents<ResObject>();
                if (old != null && old.Length > 0)
                {
                    for (int i = 0; i < old.Length; i++)
                    {
                        if (old[i].Equals(asset))
                        {
                            res = old[i];
                            res._onComplete += onComplete;
                            break;
                        }
                    }
                    if (res == null)
                    {
                        for (int i = 0; i < old.Length; i++)
                        {
                            // 空的没用的对象，一般通过复制对象copy过来的
                            if (string.IsNullOrEmpty(old[i]._assetName))
                            {
                                res = old[i];
                                res.SetInfo<T>(loader, asset, onComplete);
                                break;
                            }
                        }
                    }
                }
                if (res == null)
                {
                    res = loader.AddComponent<ResObject>();
                    res.SetInfo<T>(loader, asset, onComplete);
                }
                if (res.Loading)
                    return res;
                res._state = LoadState.None;
                res.StartLoad();
                return res;
            }
            return null;
        }
        #endregion
    }
}