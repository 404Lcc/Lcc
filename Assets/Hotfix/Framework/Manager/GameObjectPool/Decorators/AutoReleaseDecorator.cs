using UnityEngine;

namespace LccHotfix
{
    //自动释放采取如下策略
    //如果AutoReleaseTime内没有使用这个对象池,则会每帧释放一个对象,直至对象数量与设置的自动释放数量相等为止
    public class AutoReleaseDecorator : GameObjectPoolDecorator
    {
        public const float AutoReleaseTime = 15.0f;
        private float _lastAutoReleaseTime = Time.unscaledTime;

        public AutoReleaseDecorator(IGameObjectPool pool) : base(pool)
        {
        }

        public override GameObjectPoolObject Get()
        {
            _lastAutoReleaseTime = Time.unscaledTime;
            return base.Get();
        }

        public override void Update()
        {
            base.Update();
            if (Pool.Count > PoolSetting.autoRelease && Time.unscaledTime > _lastAutoReleaseTime + AutoReleaseTime)
            {
                ForceRelease();
            }
        }
    }
}