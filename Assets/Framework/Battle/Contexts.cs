using Entitas;

public partial class Contexts : IContexts
{
    public const string ID = "ID";
    public LogicWorld Logic { get; set; }

    public IContext[] allContexts => new IContext[] { Logic };

    public Contexts()
    {
        Logic = new LogicWorld();
        InitializeEntityIndices();
    }
    public void InitializeEntityIndices()
    {
        Logic.AddEntityIndex(new PrimaryEntityIndex<LogicEntity, long>(ID, Logic.GetGroup(LogicMatcher.ID), (e, c) => ((IDComponent)c).id));
    }
    public void Reset()
    {
        Logic.Reset();
    }
}