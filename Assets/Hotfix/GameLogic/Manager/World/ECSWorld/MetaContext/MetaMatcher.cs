namespace LccHotfix
{
    public sealed partial class MetaMatcher
    {
        public static Entitas.IAllOfMatcher<MetaEntity> AllOf(params int[] indices)
        {
            return Entitas.Matcher<MetaEntity>.AllOf(indices);
        }

        public static Entitas.IAllOfMatcher<MetaEntity> AllOf(params Entitas.IMatcher<MetaEntity>[] matchers)
        {
            return Entitas.Matcher<MetaEntity>.AllOf(matchers);
        }

        public static Entitas.IAnyOfMatcher<MetaEntity> AnyOf(params int[] indices)
        {
            return Entitas.Matcher<MetaEntity>.AnyOf(indices);
        }

        public static Entitas.IAnyOfMatcher<MetaEntity> AnyOf(params Entitas.IMatcher<MetaEntity>[] matchers)
        {
            return Entitas.Matcher<MetaEntity>.AnyOf(matchers);
        }
    }
}