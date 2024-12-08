using Entitas;

public class IDComponent : LogicComponent
{
    public long id;
}

public partial class LogicEntity
{
    public IDComponent ID => (IDComponent)GetComponent(LogicComponentsLookup.IDComponent);
    public bool HasID => HasComponent(LogicComponentsLookup.IDComponent);

    public void AddID(long newId)
    {
        var index = LogicComponentsLookup.IDComponent;
        var component = (IDComponent)CreateComponent(index, typeof(IDComponent));
        component.id = newId;
        AddComponent(index, component);
    }

    public void ReplaceID(long newId)
    {
        var index = LogicComponentsLookup.IDComponent;
        var component = (IDComponent)CreateComponent(index, typeof(IDComponent));
        component.id = newId;
        ReplaceComponent(index, component);
    }

    public void RemoveID()
    {
        RemoveComponent(LogicComponentsLookup.IDComponent);
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
                var matcher = (Matcher<LogicEntity>)Matcher<LogicEntity>.AllOf(LogicComponentsLookup.IDComponent);
                matcher.ComponentNames = LogicComponentsLookup.componentNames;
                _matcherID = matcher;
            }

            return _matcherID;
        }
    }
}