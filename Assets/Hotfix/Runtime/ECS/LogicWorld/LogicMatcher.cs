using Entitas;

namespace LccHotfix
{
    public partial class LogicMatcher
    {
        public static IAllOfMatcher<LogicEntity> AllOf(params int[] indices)
        {
            return Matcher<LogicEntity>.AllOf(indices);
        }

        public static IAllOfMatcher<LogicEntity> AllOf(params IMatcher<LogicEntity>[] matchers)
        {
            return Matcher<LogicEntity>.AllOf(matchers);
        }

        public static IAnyOfMatcher<LogicEntity> AnyOf(params int[] indices)
        {
            return Matcher<LogicEntity>.AnyOf(indices);
        }

        public static IAnyOfMatcher<LogicEntity> AnyOf(params IMatcher<LogicEntity>[] matchers)
        {
            return Matcher<LogicEntity>.AnyOf(matchers);
        }
    }
}