namespace Pathfinding {
	/// <summary>
	/// Settings for how an agent searches for paths.
	///
	/// This struct contains information about which graphs the agent can use, which nodes it can traverse, and if any nodes should be easier or harder to traverse.
	///
	/// See: <see cref="FollowerEntity.pathfindingSettings"/>
	/// See: <see cref="Path.UseSettings"/>
	/// </summary>
	[System.Serializable]
	public struct PathRequestSettings {
		/// <summary>
		/// Graphs that this agent can use.
		/// This field determines which graphs will be considered when searching for the start and end nodes of a path.
		/// It is useful in numerous situations, for example if you want to make one graph for small units and one graph for large units, or one graph for people and one graph for ships.
		///
		/// This is a bitmask so if you for example want to make the agent only use graph index 3 then you can set this to:
		/// <code> settings.graphMask = 1 << 3; </code>
		///
		/// See: bitmasks (view in online documentation for working links)
		///
		/// Note that this field only stores which graph indices that are allowed. This means that if the graphs change their ordering
		/// then this mask may no longer be correct.
		///
		/// If you know the name of the graph you can use the <see cref="Pathfinding.GraphMask.FromGraphName"/> method:
		/// <code>
		/// GraphMask mask1 = GraphMask.FromGraphName("My Grid Graph");
		/// GraphMask mask2 = GraphMask.FromGraphName("My Other Grid Graph");
		///
		/// NNConstraint nn = NNConstraint.Walkable;
		///
		/// nn.graphMask = mask1 | mask2;
		///
		/// // Find the node closest to somePoint which is either in 'My Grid Graph' OR in 'My Other Grid Graph'
		/// var info = AstarPath.active.GetNearest(somePoint, nn);
		/// </code>
		///
		/// See: multiple-agent-types (view in online documentation for working links)
		/// </summary>
		public GraphMask graphMask;

		/// <summary>
		/// The penalty for each tag.
		///
		/// If null, all penalties will be treated as zero. Otherwise, the array should always have a length of exactly 32.
		///
		/// [Open online documentation to see images]
		///
		/// See: tags (view in online documentation for working links)
		/// </summary>
		public int[] tagPenalties;

		/// <summary>
		/// The tags which this agent can traverse.
		///
		/// This is a bitmask. Each bit indicates that the agent can traverse nodes with the corresponding tag.
		/// If a bit is not set, the agent will treat it as if it is not traversable.
		///
		/// The default value is -1, which sets all bits, and indicates that the agent can traverse all tags.
		///
		/// [Open online documentation to see images]
		///
		/// See: bitmasks (view in online documentation for working links)
		/// See: tags (view in online documentation for working links)
		/// </summary>
		public int traversableTags;

		/// <summary>
		/// Filters which nodes the agent can traverse, and can also add penalties to each traversed node.
		///
		/// In most common situations, this is left as null (which implies the default traversal provider: <see cref="DefaultITraversalProvider"/>).
		/// But if you need custom pathfinding behavior which cannot be done using the <see cref="graphMask"/>, <see cref="tagPenalties"/> and <see cref="traversableTags"/>, then setting an <see cref="ITraversalProvider"/> is a great option.
		/// It provides you a lot more control over how the pathfinding works.
		///
		/// <code>
		/// followerEntity.pathfindingSettings.traversalProvider = new MyCustomTraversalProvider();
		/// </code>
		///
		/// See: traversal_provider (view in online documentation for working links)
		/// </summary>
		public ITraversalProvider traversalProvider;

		/// <summary>A PathRequestSettings instance with default values for all fields</summary>
		public static PathRequestSettings Default => new PathRequestSettings {
			graphMask = GraphMask.everything,
			tagPenalties = new int[32],
			traversableTags = -1,
			traversalProvider = null,
		};
	}
}
