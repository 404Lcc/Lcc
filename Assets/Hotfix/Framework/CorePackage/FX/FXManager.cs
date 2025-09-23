using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public enum FXType
    {
        Normal,
        LineRenderer,
    }

    public class FXObject : IReference
    {
        public bool active;
        public int id;
        public float during;
        public bool loop;
        public bool ignoreTimeScale;

        public GameObjectPoolObject res;

        public FXType type;

        //跟随节点
        public Transform followTrans;

        //LineRenderer
        public bool follow;
        public LineRenderer lineRenderer;
        public Transform fromTrans;
        public Transform toTrans;

        public void Init(int id, float during, bool ignoreTimeScale, GameObjectPoolObject res, FXType type)
        {
            active = true;
            this.id = id;
            this.during = during;
            this.ignoreTimeScale = ignoreTimeScale;
            this.res = res;
            this.type = type;

            if (during <= 0)
            {
                loop = true;
            }
        }
        public void SetNormal(Transform followTrans)
        {
            this.followTrans = followTrans;
        }

        public void SetLineRenderer(bool follow, LineRenderer lineRenderer, Transform fromTrans, Transform toTrans)
        {
            this.follow = follow;
            this.lineRenderer = lineRenderer;
            this.fromTrans = fromTrans;
            this.toTrans = toTrans;

        }

        public void Update()
        {
            if (res == null)
                return;

            switch (type)
            {
                case FXType.Normal:
                    UpdateNormal();
                    break;
                case FXType.LineRenderer:
                    UpdateLineRenderer();
                    break;
            }
        }

        void UpdateNormal()
        {
            if (followTrans == null)
                return;

            if (!followTrans.gameObject.activeSelf)
            {
                Main.FXService.Release(this);
                return;
            }

            res.GameObject.transform.position = followTrans.position;
            res.GameObject.transform.rotation = followTrans.rotation;
            res.GameObject.transform.localScale = Vector3.one;
        }

        void UpdateLineRenderer()
        {
            if (fromTrans == null)
                return;

            if (toTrans == null)
                return;

            if (!follow)
                return;

            lineRenderer.SetPosition(0, fromTrans.position);
            lineRenderer.SetPosition(1, toTrans.position);
        }

        public void OnRecycle()
        {
            active = false;
            id = 0;
            during = 0;
            loop = false;
            ignoreTimeScale = false;
            if (res != null)
            {
                var obj = res.GameObject;
                //清理一下拖尾
                var renderers = obj.GetComponentsInChildren<TrailRenderer>();
                foreach (var item in renderers)
                {
                    item.Clear();
                }

                Main.GameObjectPoolService.ReleaseObject(res);
            }

            res = null;

            type = FXType.Normal;

            followTrans = null;

            follow = false;
            lineRenderer = null;
            fromTrans = null;
            toTrans = null;
        }
    }


    internal class FXManager : Module, IFXService
    {


        private List<FXObject> _fxList = new List<FXObject>();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            for (int i = 0; i < _fxList.Count; i++)
            {
                if (_fxList[i].loop)
                    continue;

                if (_fxList[i].during > 0)
                {
                    _fxList[i].during -= _fxList[i].ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                }

                _fxList[i].Update();

                if (_fxList[i].during <= 0)
                {
                    Release(_fxList[i]);
                }
            }
        }

        internal override void Shutdown()
        {
            ReleaseAll();
        }

        public FXObject GetFX(int id)
        {
            foreach (var item in _fxList)
            {
                if (item.id == id)
                {
                    return item;
                }
            }
            return null;
        }

        public void Release(int id)
        {
            var fx = GetFX(id);
            Release(fx);
        }

        public void Release(FXObject fx)
        {
            if (fx == null)
                return;

            ReferencePool.Release(fx);
            _fxList.Remove(fx);
        }

        public void ReleaseAll()
        {
            for (int i = 0; i < _fxList.Count; i++)
            {
                Release(_fxList[i]);
            }
            _fxList.Clear();
        }

        public int PlayNormal(string path, Vector3 pos, Quaternion rot, Vector3 scale, float during, bool ignoreTimeScale = false)
        {
            if (string.IsNullOrEmpty(path))
                return -1;

            FXObject fx = GetFX();
            if (fx == null)
                return -1;

            var res = Main.GameObjectPoolService.GetObject(path);
            if (res == null)
                return -1;

            res.GameObject.transform.position = pos;
            res.GameObject.transform.rotation = rot;
            res.GameObject.transform.localScale = scale;

            fx.Init(IdUtility.GenerateId(), during, ignoreTimeScale, res, FXType.Normal);
            return fx.id;
        }

        public int PlayNormal(string path, Transform followTrans, float during, bool unit, bool ignoreTimeScale = false)
        {
            if (string.IsNullOrEmpty(path))
                return -1;

            FXObject fx = GetFX();
            if (fx == null)
                return -1;

            var res = Main.GameObjectPoolService.GetObject(path);
            if (res == null)
                return -1;


            if (unit)
            {
                res.GameObject.transform.parent = null;
            }
            else
            {
                res.GameObject.transform.parent = followTrans;
            }

            res.GameObject.transform.localPosition = Vector3.zero;
            res.GameObject.transform.localRotation = Quaternion.identity;
            res.GameObject.transform.localScale = followTrans.transform.localScale;

            fx.Init(IdUtility.GenerateId(), during, ignoreTimeScale, res, FXType.Normal);

            if (unit)
            {
                fx.SetNormal(followTrans);
            }
            return fx.id;
        }


        public int PlayLineRender(string path, Transform fromTrans, Transform toTrans, float during, bool follow, bool ignoreTimeScale = false)
        {
            if (string.IsNullOrEmpty(path))
                return -1;

            FXObject fx = GetFX();
            if (fx == null)
                return -1;

            var res = Main.GameObjectPoolService.GetObject(path);
            if (res == null)
                return -1;

            LineRenderer lineRenderer = res.GameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                return -1;
            }

            res.GameObject.transform.position = Vector3.zero;
            res.GameObject.transform.rotation = Quaternion.identity;
            res.GameObject.transform.localScale = Vector3.one;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, fromTrans.position);
            lineRenderer.SetPosition(1, toTrans.position);

            fx.Init(IdUtility.GenerateId(), during, ignoreTimeScale, res, FXType.LineRenderer);
            fx.SetLineRenderer(follow, lineRenderer, fromTrans, toTrans);
            return fx.id;
        }

        private FXObject GetFX()
        {
            var fx = ReferencePool.Acquire<FXObject>();
            _fxList.Add(fx);
            return fx;
        }
    }
}