namespace LccHotfix
{
    public partial class LogicWorld
    {
        public IGizmoService GizmoService { get; private set; }

        public void SetGizmoService(IGizmoService gizmoService)
        {
            GizmoService = gizmoService;
        }
    }
}
