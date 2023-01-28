using System;

namespace LccModel
{
    public class Component : AObjectBase
    {
        public virtual bool DefaultEnable { get; set; } = true;
        private bool enable = false;
        public bool Enable
        {
            get
            {
                return enable;
            }
            set
            {
                if (enable == value) return;
                enable = value;
                if (enable)
                {
                    OnEnable();
                }
                else
                {
                    OnDisable();
                }
            }
        }

        public virtual void OnEnable()
        {

        }

        public virtual void OnDisable()
        {

        }

        public override void Awake()
        {
            base.Awake();
            Enable = DefaultEnable;
        }
        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            Enable = DefaultEnable;
        }
        public override void Awake<P1, P2>(P1 p1, P2 p2)
        {
            base.Awake(p1, p2);

            Enable = DefaultEnable;
        }

        public override void Awake<P1, P2, P3>(P1 p1, P2 p2, P3 p3)
        {
            base.Awake(p1, p2, p3);

            Enable = DefaultEnable;
        }

        public override void Awake<P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4)
        {
            base.Awake(p1, p2, p3, p4);

            Enable = DefaultEnable;

        }

        #region ÊÂ¼þ
        public T Publish<T>(T TEvent) where T : class
        {
            ((Entity)Parent).Publish(TEvent);
            return TEvent;
        }
        public void Subscribe<T>(Action<T> action) where T : class
        {
            ((Entity)Parent).Subscribe(action);
        }

        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            ((Entity)Parent).UnSubscribe(action);
        }
        #endregion
    }
}