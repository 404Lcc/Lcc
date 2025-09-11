using UnityEngine;

namespace LccHotfix
{
    public class UIHeadbarTemplate : ItemBase
    {
        public LogicEntity entity;
        public float offsetY;
        public bool updatePos;

        public virtual void Init(LogicEntity entity, float offsetY)
        {
            this.entity = entity;
            this.offsetY = offsetY;
            updatePos = true;
        }

        public virtual void Update()
        {
            UpdatePos();
        }

        public virtual void UpdatePos()
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

        public override void Hide()
        {
            base.Hide();

            entity = null;
        }
    }
}