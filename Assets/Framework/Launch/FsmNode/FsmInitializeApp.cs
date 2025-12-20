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

            {
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.orientation = ScreenOrientation.Portrait;
            }
            

            {
                AppConfig.AppVersion = Application.version;
            }


            {
                if (Application.isEditor)
                    AssetConfig.PlayMode = PatchConfig.IsEnablePatcher ? EPlayMode.HostPlayMode : EPlayMode.EditorSimulateMode;
                else
                    AssetConfig.PlayMode = PatchConfig.IsEnablePatcher ? EPlayMode.HostPlayMode : EPlayMode.OfflinePlayMode;
            }

            {
                TextAsset jStr = Resources.Load<TextAsset>($"Launch/Strings/launch_strings_{"todo 语言"}");
                StringTable.Strings = JsonUtility.ToObject<Dictionary<string, string>>(jStr.text);
            }

            ChangeToNextState();
        }
    }
}