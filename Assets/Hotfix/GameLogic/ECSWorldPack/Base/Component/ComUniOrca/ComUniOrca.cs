using RVO;
using Unity.Mathematics;

namespace LccHotfix
{
    //https://github.com/aillieo/RVO2-Unity
    public class ComUniOrca : MetaComponent
    {
        public Simulator simulator;

        public override void Dispose()
        {
            base.Dispose();

            simulator.Clear();
            simulator.Dispose();
            simulator = null;
        }

        public void DoStep()
        {
            simulator.DoStep();
        }

        public void EnsureCompleted()
        {
            simulator.EnsureCompleted();
        }
    }

    public partial class MetaContext
    {
        public ComUniOrca ComUniOrca
        {
            get { return GetUniqueComponent<ComUniOrca>(MetaComponentsLookup.ComUniOrca); }
        }

        public bool hasComUniOrca
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniOrca); }
        }

        public void SetComUniOrca()
        {
            var index = MetaComponentsLookup.ComUniOrca;
            var component = (ComUniOrca)UniqueEntity.CreateComponent(index, typeof(ComUniOrca));
            var simulator = new Simulator();
            simulator.SetTimeStep(1 / 60f);
            // 设置新增智能体的默认属性。
            // neighborDist新智能体在导航时考虑与其他智能体的默认最大距离（中心点至中心点）。该值越大，模拟运行时间越长；若设置过小，则无法保证模拟的安全性。必须为非负数。
            // maxNeighbors新智能体在导航时考虑的其他智能体默认最大数量。该值越大，模拟运行时间越长；若设置过小，则无法保证模拟的安全性。
            // timeHorizon新智能体通过模拟计算出的速度相对于其他智能体保持安全性的默认最短时间。该值越大，智能体越早对其他智能体的存在作出反应，但其选择速度的自由度会降低。必须为正数。
            // timeHorizonObst新智能体通过模拟计算出的速度相对于障碍物保持安全性的默认最短时间。该值越大，智能体越早对障碍物的存在作出反应，但其选择速度的自由度会降低。必须为正数。
            // radius新智能体的默认半径。必须为非负数。
            // maxSpeed新智能体的默认最大速度。必须为非负数。
            // velocity新智能体的默认初始二维线速度。
            simulator.SetAgentDefaults(10f, 10, 4f, 4f, 0.5f, 0.2f, new float2(0f, 0f));
            component.simulator = simulator;
            SetUniqueComponent(index, component);
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex ComUniOrcaIndex = new ComponentTypeIndex(typeof(ComUniOrca));
        public static int ComUniOrca => ComUniOrcaIndex.index;
    }
}