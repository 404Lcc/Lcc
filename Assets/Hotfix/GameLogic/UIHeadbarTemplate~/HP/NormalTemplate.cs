using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 血条模板
    /// </summary>
    public class NormalHPBase : HPBase
    {
        public override void UpdatePos()
        {
            if (!updatePos)
                return;

            if (entity == null)
                return;

            if (!entity.hasComTransform || !entity.hasComView)
                return;

            Vector3 pos = entity.comTransform.position + new Vector3(0, offsetY, 0);
            var uiPos = ClientTools.World2UILocal(pos, this.Transform.parent as RectTransform, Main.CameraService.CurrentCamera, Main.CameraService.UICamera);
            Transform.localPosition = uiPos;
        }
    }
}