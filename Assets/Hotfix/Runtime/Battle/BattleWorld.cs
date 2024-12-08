using Entitas;
using NPOI.SS.Formula.Functions;

namespace LccHotfix
{
    public class BattleWorld : ECSWorld
    {
        public const string ComID = "ComID";
        public const string ComOwnerEntity = "ComOwnerEntity";
        public const string ComTag = "ComTag";
        public const string ComFaction = "ComFaction";
        public const string ComUnityObjectRelated = "ComUnityObjectRelated";

        protected override void Setup()
        {
            base.Setup();

            LogicComponentsLookup.ComID = 0;

            LogicComponentsLookup.componentTypes.Add(typeof(ComID));
        }
        protected override void InitializeEntityIndices()
        {
            base.InitializeEntityIndices();

            LogicContext.AddEntityIndex(new PrimaryEntityIndex<LogicEntity, long>(ComID, LogicContext.GetGroup(LogicMatcher.ComID), (e, c) => ((ComID)c).Value));
        }

        public LogicEntity GetEntityWithComID(long Value)
        {
            return ((PrimaryEntityIndex<LogicEntity, long>)LogicContext.GetEntityIndex(ComID)).GetEntity(Value);
        }

    }
}