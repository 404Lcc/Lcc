namespace Pathfinding.Util {
	/// <summary>
	/// Represents a part of a path, with optional link information.
	///
	/// A path is divided up into parts, where each part is either a sequence of nodes or an off-mesh link.
	/// If your agent never traverses off-mesh links, the path will always consist of only a single part which is a sequence of nodes.
	/// </summary>
	public struct PathPartWithLinkInfo {
		public PathPartWithLinkInfo(int startIndex, int endIndex, OffMeshLinks.OffMeshLinkTracer linkInfo = default) {
			this.startIndex = startIndex;
			this.endIndex = endIndex;
			this.linkInfo = linkInfo;
		}

		/// <summary>
		/// Index of the first point in the path that this part represents.
		///
		/// For off-mesh links, this will refer to the last point in the part before the off-mesh link.
		/// </summary>
		public int startIndex;
		/// <summary>
		/// Index of the last point in the path that this part represents.
		///
		/// For off-mesh links, this will refer to the first point in the part after the off-mesh link.
		/// </summary>
		public int endIndex;
		/// <summary>The off-mesh link that this part represents. Will contain a null link if this part is not an off-mesh link</summary>
		public OffMeshLinks.OffMeshLinkTracer linkInfo;
		/// <summary>Specifies if this is a sequence of nodes, or an off-mesh link</summary>
		public Funnel.PartType type => linkInfo.link != null ? Funnel.PartType.OffMeshLink : Funnel.PartType.NodeSequence;
	}
}
