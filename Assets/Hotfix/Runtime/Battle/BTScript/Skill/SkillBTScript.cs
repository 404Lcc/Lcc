using System.Collections;
using System.Collections.Generic;
using LccHotfix;
using UnityEngine;

namespace LccHotfix
{
    public class SkillBTScript : ParallelBTScript
    {
        public override void Init()
        {
            base.Init();
            RegisterCallback(BTAction.OnReleaseSkill, OnReleaseSkill);
            RegisterCallback(BTAction.OnReleaseSkillEnd, OnReleaseSkillEnd);
        }

        public virtual void OnReleaseSkill(BTAgent btAgent, object[] args)
        {

        }

        public virtual void OnReleaseSkillEnd(BTAgent btAgent, object[] args)
        {
            btAgent.SetFinish();
        }
    }
}