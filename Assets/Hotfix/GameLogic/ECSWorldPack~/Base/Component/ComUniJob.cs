namespace LccHotfix
{
    public class ComUniJob : MetaComponent
    {
        public IFreeChunk FreeChunk { get; protected set; }
        public JobSystems JobSystem { get; protected set; }

        public void CreateJobSystems()
        {
        }

        public void Update()
        {
            if (JobSystem != null && FreeChunk != null)
            {
                JobSystem.Update(FreeChunk);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    public partial class MetaContext
    {
        public ComUniJob ComUniJob
        {
            get { return GetUniqueComponent<ComUniJob>(MetaComponentsLookup.ComUniJob); }
        }

        public bool hasComUniJob
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniJob); }
        }

        public void SetComUniJob()
        {
            var index = MetaComponentsLookup.ComUniJob;
            var component = (ComUniJob)UniqueEntity.CreateComponent(index, typeof(ComUniJob));
            component.CreateJobSystems();
            SetUniqueComponent(index, component);
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex ComUniJobIndex = new ComponentTypeIndex(typeof(ComUniJob));
        public static int ComUniJob => ComUniJobIndex.index;
    }
}