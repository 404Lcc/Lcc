namespace Pathfinding.Graphs.Grid.Rules {
	/// <summary>Common interface for all grid graph rule editors</summary>
	public interface IGridGraphRuleEditor {
		void OnInspectorGUI(GridGraph graph, GridGraphRule rule);
		void OnSceneGUI(GridGraph graph, GridGraphRule rule);
	}
}
