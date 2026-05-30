namespace LccHotfix
{
    public delegate void AttachBreakFunc(LogicEntity entity, ref AttachInfoBase hitInfo);

    public class AttachInfoBase
    {
        public int attachOtherEntityID;
        public LogicEntity attachOtherEntityRef;
        public AttachBreakFunc breakFunc;
    }

    /// 用一个 AttachComponent 组件处理，entity层面的挂接关联
    /// TODO：（未完成） 
    public class AttachComponent : LogicComponent
    {
        public AttachInfoBase Info { get; set; }
    }

    public partial class LogicEntity
    {
        public AttachComponent comAttach
        {
            get { return (AttachComponent)GetComponent(LogicComponentsLookup.ComAttach); }
        }

        public bool hasComAttach
        {
            get { return HasComponent(LogicComponentsLookup.ComAttach); }
        }

        public void SetAttachTo(AttachInfoBase info)
        {
            var index = LogicComponentsLookup.ComAttach;
            var component = (AttachComponent)CreateComponent(index, typeof(AttachComponent));
            component.Info = info;
            ReplaceComponent(index, component);
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComAttachIndex = new(typeof(AttachComponent));
        public static int ComAttach => _ComAttachIndex.Index;
    }
}