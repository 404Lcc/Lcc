using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class FsmInitializeApp : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(1);

            if (Application.isEditor)
                AssetConfig.PlayMode = GameConfig.IsEnablePatcher ? EPlayMode.HostPlayMode : EPlayMode.EditorSimulateMode;
            else
                AssetConfig.PlayMode = GameConfig.IsEnablePatcher ? EPlayMode.HostPlayMode : EPlayMode.OfflinePlayMode;

            TextAsset jStr = Resources.Load<TextAsset>($"Launch/Strings/launch_strings_{GameConfig.AppLanguage}");
            StringTable.Strings = JsonUtility.ToObject<Dictionary<string, string>>(jStr.text);

            ChangeToNextState();
        }

        protected override void ChangeToNextState()
        {
            base.ChangeToNextState();
            _machine.ChangeState<FsmStartSplash>();
        }
    }
}