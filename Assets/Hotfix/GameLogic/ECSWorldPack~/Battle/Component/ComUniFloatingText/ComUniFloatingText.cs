using UnityEngine;

namespace LccHotfix
{
    public class ComUniFloatingText : MetaComponent
    {
        public IFloatingText FloatingText;

        public override void PostInitialize(MetaEntity owner)
        {
            base.PostInitialize(owner);

            FloatingText.PostInitialize();
        }

        public override void Dispose()
        {
            base.Dispose();

            FloatingText.Dispose();
        }

        public void Spawn(string text, Vector3 position)
        {
            FloatingText.Spawn(text, position);
        }
    }

    public partial class MetaContext
    {
        public ComUniFloatingText ComUniFloatingText
        {
            get { return GetUniqueComponent<ComUniFloatingText>(MetaComponentsLookup.ComUniFloatingText); }
        }

        public bool hasComUniFloatingText
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniFloatingText); }
        }

        public void SetComUniFloatingText(IFloatingText floatingText)
        {
            var index = MetaComponentsLookup.ComUniFloatingText;
            var component = (ComUniFloatingText)UniqueEntity.CreateComponent(index, typeof(ComUniFloatingText));
            component.FloatingText = floatingText;
            SetUniqueComponent(index, component);
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex ComUniFloatingTextIndex = new ComponentTypeIndex(typeof(ComUniFloatingText));
        public static int ComUniFloatingText => ComUniFloatingTextIndex.index;
    }
}