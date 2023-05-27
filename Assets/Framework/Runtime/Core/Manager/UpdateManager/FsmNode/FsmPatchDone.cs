using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
	/// <summary>
	/// 流程更新完毕
	/// </summary>
	public class FsmPatchDone : IStateNode
	{
		public void OnCreate(StateMachine machine)
		{
		}
		public void OnEnter()
		{
			UpdateEventDefine.PatchStatesChange.Publish("开始游戏！");

            Loader.Instance.Start(UpdateManager.Instance.globalConfig);
        }
		public void OnUpdate()
		{
		}
		public void OnExit()
		{
		}
	}
}