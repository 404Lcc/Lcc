using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    //动画状态参数
    //后续特殊字段在项目的 partial 中扩展
    public partial struct AnimationData
    {
        // 特殊指定动作：直接传递动画名称（分层）
        public string SpecAnim_Layer0 { get; set; }
        public string SpecAnim_Layer1 { get; set; }
        public string SpecAnim_Layer2 { get; set; }
        public bool isForceSpecAnim { get; set; }
        public float SpecAnimSpeed_Layer1 { get; set; }

        
        public static bool operator ==(AnimationData lhs, AnimationData rhs)
        {
            return lhs.SpecAnim_Layer0 == rhs.SpecAnim_Layer0
                   && lhs.SpecAnim_Layer1 == rhs.SpecAnim_Layer1
                   && lhs.SpecAnim_Layer2 == rhs.SpecAnim_Layer2
                   && lhs.isForceSpecAnim == rhs.isForceSpecAnim
                   && Mathf.Approximately(lhs.SpecAnimSpeed_Layer1, rhs.SpecAnimSpeed_Layer1);
        }

        public static bool operator !=(AnimationData lhs, AnimationData rhs)
        {
            return !(lhs == rhs);
        }
    }

    public interface IAnimatorCtrl
    {
        void SyncData(ViewComponent view, ref AnimationData data);
    }

    //////////////////////////////////////////////////////////////////////////
    // AnimationComponent: 
    //////////////////////////////////////////////////////////////////////////
    public class AnimationComponent : LogicComponent
    {
        public static readonly AnimationData EmptyData = default;

        //动画的纯数据描述（int、float、bool 等）
        public AnimationData Data;


        //负责解释纯数据描述，传达给具体动画系统的，控制代理
        protected IAnimatorCtrl mCtrl;

        public IAnimatorCtrl Ctrl
        {
            get { return mCtrl; }
        }

        public void SetData(AnimationData data)
        {
            if (Data == data)
            {
                return;
            }
            //BattleLogger.LogDebug($"AnimationComponent SetData {data.SpecAnim_Layer0}");
            Data = data;
            _owner.ReplaceComponent(LogicComponentsLookup.ComAnimation, this);
        }

        public void InitCtrl(IAnimatorCtrl ctrl)
        {
            mCtrl = ctrl;
        }

        public override void DisposeOnRemove()
        {
            Data = EmptyData;
            mCtrl = null;
            base.DisposeOnRemove();
        }
    }


    public partial class LogicEntity
    {
        public AnimationComponent comAnimation
        {
            get { return (AnimationComponent)GetComponent(LogicComponentsLookup.ComAnimation); }
        }

        public bool hasComAnimation
        {
            get { return HasComponent(LogicComponentsLookup.ComAnimation); }
        }


        public void AddComAnimation(IAnimatorCtrl ctrl)
        {
            var index = LogicComponentsLookup.ComAnimation;
            if (!hasComAnimation)
            {
                var component = (AnimationComponent)CreateComponent(index, typeof(AnimationComponent));
                component.InitCtrl(ctrl);
                AddComponent(index, component);
            }
            else
            {
                var component = (AnimationComponent)GetComponent(index);
                component.InitCtrl(ctrl);
                ReplaceComponent(index, component);
            }
        }

        public void SetAnimation(AnimationData data)
        {
            var index = LogicComponentsLookup.ComAnimation;
            if (!hasComAnimation)
            {
                var component = (AnimationComponent)CreateComponent(index, typeof(AnimationComponent));
                component.SetData(data);
                AddComponent(index, component);
            }
            else
            {
                var component = (AnimationComponent)GetComponent(index);
                component.SetData(data);
                ReplaceComponent(index, component);
            }
        }

        public void PlayAnimation(string animStateName, int layerIndex = 0, bool isForce = false,
            float animationSpeed = 1f)
        {
            if (!hasComAnimation)
            {
                BattleLogger.LogWarning($"Entity {ID} has no AnimationComponent, cannot play animation {animStateName}");
                return;
            }

            var newData = comAnimation.Data;
            switch (layerIndex)
            {
                case 0:
                    newData.SpecAnim_Layer0 = animStateName;
                    break;
                case 1:
                    newData.SpecAnim_Layer1 = animStateName;
                    newData.SpecAnimSpeed_Layer1 = Mathf.Max(0.01f, animationSpeed);
                    break;
                case 2:
                    newData.SpecAnim_Layer2 = animStateName;
                    break;
            }
            newData.isForceSpecAnim = isForce;
            comAnimation.SetData(newData);
        }

        public float GetAnimationTime(string animStateName)
        {
            var theView = GetView<IMainAnimatorView>(EViewCategory.MainGameObject);
            if (theView == null)
            {
                return 0f;
            }
            var animator = theView.GetMainAnimator();
            if (animator == null)
            {
                return 0f;
            }

            AnimationClip clip = null;
            var list = animator.runtimeAnimatorController.animationClips;
            foreach (var item in list)
            {
                if (item.name.Contains(animStateName))
                {
                    clip = item;
                    break;
                }
            }
            if (clip != null)
            {
                float evtTime = clip.events.Count() > 0 ? clip.events[0].time : clip.length * 1f;
                return evtTime;
            }

            return 0f;
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComAnimationIndex = new(typeof(AnimationComponent));
        public static int ComAnimation => _ComAnimationIndex.Index;
    }
}
