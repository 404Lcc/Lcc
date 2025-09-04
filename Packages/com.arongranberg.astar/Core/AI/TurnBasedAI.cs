using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding.Examples {
	/// <summary>Helper script in the example scene 'Turn Based'</summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/turnbasedai.html")]
	public class TurnBasedAI : VersionedMonoBehaviour {
		public int movementPoints = 2;
		public BlockManager blockManager;
		public SingleNodeBlocker blocker;
		public GraphNode targetNode;
		public BlockManager.TraversalProvider traversalProvider;

		void Start () {
			blocker.BlockAtCurrentPosition();
		}

		protected override void Awake () {
			base.Awake();
			// Set the traversal provider to block all nodes that are blocked by a SingleNodeBlocker
			// except the SingleNodeBlocker owned by this AI (we don't want to be blocked by ourself)
			traversalProvider = new BlockManager.TraversalProvider(blockManager, BlockManager.BlockMode.AllExceptSelector, new List<SingleNodeBlocker>() { blocker });
		}
	}
}
