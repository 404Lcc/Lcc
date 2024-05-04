using Entitas;

public class IDComponent : ComponentBase
{
    public long id;
}

public partial class LogicEntity
{
    public IDComponent ID => (IDComponent)GetComponent(LogicComponentsLookup.ID);
    public bool HasID => HasComponent(LogicComponentsLookup.ID);

    public void AddID(long newId)
    {
        var index = LogicComponentsLookup.ID;
        var component = (IDComponent)CreateComponent(index, typeof(IDComponent));
        component.id = newId;
        AddComponent(index, component);
    }

    public void ReplaceID(long newId)
    {
        var index = LogicComponentsLookup.ID;
        var component = (IDComponent)CreateComponent(index, typeof(IDComponent));
        component.id = newId;
        ReplaceComponent(index, component);
    }

    public void RemoveID()
    {
        RemoveComponent(LogicComponentsLookup.ID);
    }
}

public partial class LogicMatcher
{
    private static IMatcher<LogicEntity> _matcherID;

    public static IMatcher<LogicEntity> ID
    {
        get
        {
            if (_matcherID == null)
            {
                var matcher = (Matcher<LogicEntity>)Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ID);
                matcher.componentNames = LogicComponentsLookup.componentNames;
                _matcherID = matcher;
            }

            return _matcherID;
        }
    }
}