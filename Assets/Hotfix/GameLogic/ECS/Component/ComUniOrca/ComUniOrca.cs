using RVO;
using Unity.Mathematics;

namespace LccHotfix
{
    //https://github.com/aillieo/RVO2-Unity
    public class ComUniOrca : MetaComponent
    {
        public Simulator Simulator { get; private set; }

        public override void PostInitialize(MetaEntity owner)
        {
            base.PostInitialize(owner);

            Simulator = new Simulator();
            Simulator.SetTimeStep(1 / 60f);
            // 设置新增智能体的默认属性。
            // neighborDist新智能体在导航时考虑与其他智能体的默认最大距离（中心点至中心点）。该值越大，模拟运行时间越长；若设置过小，则无法保证模拟的安全性。必须为非负数。
            // maxNeighbors新智能体在导航时考虑的其他智能体默认最大数量。该值越大，模拟运行时间越长；若设置过小，则无法保证模拟的安全性。
            // timeHorizon新智能体通过模拟计算出的速度相对于其他智能体保持安全性的默认最短时间。该值越大，智能体越早对其他智能体的存在作出反应，但其选择速度的自由度会降低。必须为正数。
            // timeHorizonObst新智能体通过模拟计算出的速度相对于障碍物保持安全性的默认最短时间。该值越大，智能体越早对障碍物的存在作出反应，但其选择速度的自由度会降低。必须为正数。
            // radius新智能体的默认半径。必须为非负数。
            // maxSpeed新智能体的默认最大速度。必须为非负数。
            // velocity新智能体的默认初始二维线速度。
            Simulator.SetAgentDefaults(10f, 10, 4f, 4f, 0.5f, 0.2f, new float2(0f, 0f));
        }

        public override void Dispose()
        {
            base.Dispose();

            Simulator.Clear();
            Simulator.Dispose();
            Simulator = null;
        }

        public void DoStep()
        {
            Simulator.DoStep();
        }

        public void EnsureCompleted()
        {
            Simulator.EnsureCompleted();
        }
    }

    public partial class MetaContext
    {
        public MetaEntity ComUniOrcaEntity
        {
            get { return GetGroup(MetaMatcher.ComUniOrca).GetSingleEntity(); }
        }

        public ComUniOrca ComUniOrca
        {
            get { return ComUniOrcaEntity.ComUniOrca; }
        }

        public bool hasComUniOrca
        {
            get { return ComUniOrcaEntity != null; }
        }

        public MetaEntity SetComUniOrca()
        {
            if (hasComUniOrca)
            {
                throw new Entitas.EntitasException("Could not set ComUniOrca!\n" + this + " already has an entity with ComUniOrca!",
                    "You should check if the context already has a ComUniOrcaEntity before setting it or use context.ReplaceComUniOrca().");
            }

            var entity = CreateEntity();
            entity.AddComUniOrca();
            return entity;
        }
    }

    public partial class MetaEntity
    {
        public ComUniOrca ComUniOrca
        {
            get { return (ComUniOrca)GetComponent(MetaComponentsLookup.ComUniOrca); }
        }

        public bool hasComUniOrca
        {
            get { return HasComponent(MetaComponentsLookup.ComUniOrca); }
        }

        public void AddComUniOrca()
        {
            var index = MetaComponentsLookup.ComUniOrca;
            var component = (ComUniOrca)CreateComponent(index, typeof(ComUniOrca));
            AddComponent(index, component);
        }

        public void RemoveComUniOrca()
        {
            RemoveComponent(MetaComponentsLookup.ComUniOrca);
        }
    }

    public sealed partial class MetaMatcher
    {
        static Entitas.IMatcher<MetaEntity> _matcherComUniOrca;

        public static Entitas.IMatcher<MetaEntity> ComUniOrca
        {
            get
            {
                if (_matcherComUniOrca == null)
                {
                    var matcher = (Entitas.Matcher<MetaEntity>)Entitas.Matcher<MetaEntity>.AllOf(MetaComponentsLookup.ComUniOrca);
                    matcher.ComponentNames = MetaComponentsLookup.componentNames;
                    _matcherComUniOrca = matcher;
                }

                return _matcherComUniOrca;
            }
        }
    }

    public static partial class MetaComponentsLookup
    {
        public static int ComUniOrca;
    }
}