using System.Collections.Generic;
using UnityEngine.UI;

namespace LccHotfix
{
    public class UIBattlePanel : UIElementBase, ICoroutine
    {
        public override void OnConstruct()
        {
            base.OnConstruct();

            LayerID = UILayerID.Main;
            IsFullScreen = true;
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override void OnShow(object[] paramsList)
        {
            base.OnShow(paramsList);
        }
    }
}