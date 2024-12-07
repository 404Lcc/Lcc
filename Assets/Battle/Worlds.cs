using Entitas;

public partial class Worlds
{
    public const string ID = "ID";
    public LogicWorld Logic { get; set; }

    public IContext[] allContexts { get; set; }

    public Worlds()
    {
        Logic = new LogicWorld();
        allContexts = new IContext[] { Logic };
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