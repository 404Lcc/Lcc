using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;
using Pathfinding.Pooling;
using UnityEngine.Assertions;

namespace Pathfinding {
	public class RichPath {
		int currentPart;
		readonly List<RichPathPart> parts = new List<RichPathPart>();

		public Seeker seeker;

		/// <summary>
		/// Transforms points from path space to world space.
		/// If null the identity transform will be used.
		///
		/// This is used when the world position of the agent does not match the
		/// corresponding position on the graph. This is the case in the example
		/// scene called 'Moving'.
		///
		/// See: <see cref="Pathfinding.Examples.LocalSpaceRichAI"/>
		/// </summary>
		public ITransform transform;

		public RichPath () {
			Clear();
		}

		public void Clear () {
			parts.Clear();
			currentPart = 0;
			Endpoint = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		}

		/// <summary>Use this for initialization.</summary>
		/// <param name="seeker">Optionally provide in order to take tag penalties into account. May be null if you do not use a Seeker\</param>
		/// <param name="path">Path to follow</param>
		/// <param name="mergePartEndpoints">If true, then adjacent parts that the path is split up in will
		/// try to use the same start/end points. For example when using a link on a navmesh graph
		/// Instead of first following the path to the center of the node where the link is and then
		/// follow the link, the path will be adjusted to go to the exact point where the link starts
		/// which usually makes more sense.</param>
		/// <param name="simplificationMode">The path can optionally be simplified. This can be a bit expensive for long paths.</param>
		public void Initialize (Seeker seeker, Path path, bool mergePartEndpoints, bool simplificationMode) {
			if (path.error) throw new System.ArgumentException("Path has an error");

			List<GraphNode> nodes = path.path;
			if (nodes.Count == 0) throw new System.ArgumentException("Path traverses no nodes");

			this.seeker = seeker;
			// Release objects back to object pool
			// Yeah, I know, it's casting... but this won't be called much
			for (int i = 0; i < parts.Count; i++) {
				var funnelPart = parts[i] as RichFunnel;
				var specialPart = parts[i] as RichSpecial;
				if (funnelPart != null) ObjectPool<RichFunnel>.Release(ref funnelPart);
				else if (specialPart != null) ObjectPool<RichSpecial>.Release(ref specialPart);
			}

			Clear();

			// Initialize new
			Endpoint = path.vectorPath[path.vectorPath.Count-1];

			//Break path into parts
			for (int i = 0; i < nodes.Count; i++) {
				if (nodes[i] is TriangleMeshNode) {
					var graph = AstarData.GetGraph(nodes[i]) as NavmeshBase;
					if (graph == null) throw new System.Exception("Found a TriangleMeshNode that was not in a NavmeshBase graph");

					RichFunnel f = ObjectPool<RichFunnel>.Claim().Initialize(this, graph);

					f.funnelSimplification = simplificationMode;

					int sIndex = i;
					uint currentGraphIndex = nodes[sIndex].GraphIndex;


					for (; i < nodes.Count; i++) {
						if (nodes[i].GraphIndex != currentGraphIndex && !(nodes[i] is NodeLink3Node)) {
							break;
						}
					}
					i--;

					if (sIndex == 0) {
						f.exactStart = path.vectorPath[0];
					} else {
						f.exactStart = (Vector3)nodes[mergePartEndpoints ? sIndex-1 : sIndex].position;
					}

					if (i == nodes.Count-1) {
						f.exactEnd = path.vectorPath[path.vectorPath.Count-1];
					} else {
						f.exactEnd = (Vector3)nodes[mergePartEndpoints ? i+1 : i].position;
					}

					f.BuildFunnelCorridor(nodes, sIndex, i);

					parts.Add(f);
				} else if (nodes[i] is LinkNode nl) {
					int sIndex = i;
					uint currentGraphIndex = nodes[sIndex].GraphIndex;

					while (i < nodes.Count && nodes[i].GraphIndex == currentGraphIndex) i++;
					i--;

					if (i - sIndex > 1) {
						throw new System.Exception("NodeLink2 path length greater than two (2) nodes. " + (i - sIndex));
					} else if (i - sIndex == 0) {
						// The link is a single node.
						// Just ignore it. It can happen in very rare circumstances with some path types.
						// For example, a RandomPath can stop at the first node of a node link, without including the other end of the link
						continue;
					}
					Assert.AreEqual(nl.linkConcrete, (nodes[i] as LinkNode).linkConcrete);

					RichSpecial rps = ObjectPool<RichSpecial>.Claim().Initialize(nl.linkConcrete.GetTracer(nl));
					parts.Add(rps);
				} else if (!(nodes[i] is PointNode)) {
					// Some other graph type which we do not have support for
					throw new System.InvalidOperationException("The RichAI movment script can only be used on recast/navmesh graphs. A node of type " + nodes[i].GetType().Name + " was in the path.");
				}
			}
		}

		public Vector3 Endpoint { get; private set; }

		/// <summary>True if we have completed (called NextPart for) the last part in the path</summary>
		public bool CompletedAllParts {
			get {
				return currentPart >= parts.Count;
			}
		}

		/// <summary>True if we are traversing the last part of the path</summary>
		public bool IsLastPart {
			get {
				return currentPart >= parts.Count - 1;
			}
		}

		public void NextPart () {
			currentPart = Mathf.Min(currentPart + 1, parts.Count);
		}

		public RichPathPart GetCurrentPart () {
			if (parts.Count == 0) return null;
			return currentPart < parts.Count ? parts[currentPart] : parts[parts.Count - 1];
		}

		/// <summary>
		/// Replaces the buffer with the remaining path.
		/// See: <see cref="Pathfinding.IAstarAI.GetRemainingPath"/>
		/// </summary>
		public void GetRemainingPath (List<Vector3> buffer, List<PathPartWithLinkInfo> partsBuffer, Vector3 currentPosition, out bool requiresRepath) {
			buffer.Clear();
			buffer.Add(currentPosition);
			requiresRepath = false;
			for (int i = currentPart; i < parts.Count; i++) {
				var part = parts[i];
				if (part is RichFunnel funnel) {
					bool lastCorner;
					var startIndex = buffer.Count;
					if (i != 0) buffer.Add(funnel.exactStart);
					funnel.Update(i == 0 ? currentPosition : funnel.exactStart, buffer, int.MaxValue, out lastCorner, out requiresRepath);
					if (partsBuffer != null) partsBuffer.Add(new PathPartWithLinkInfo(startIndex, buffer.Count-1));
					if (requiresRepath) {
						return;
					}
				} else if (part is RichSpecial rs) {
					// By adding all points above the link will look like just a stright line, which is reasonable
					// The part's start/end indices refer to the last point in previous part and first point in the next part, respectively
					if (partsBuffer != null) partsBuffer.Add(new PathPartWithLinkInfo(buffer.Count-1, buffer.Count, rs.nodeLink));
				}
			}
		}
	}

	public abstract class RichPathPart : IAstarPooledObject {
		public abstract void OnEnterPool();
	}

	public class RichFunnel : RichPathPart {
		readonly List<Vector3> left;
		readonly List<Vector3> right;
		List<TriangleMeshNode> nodes;
		public Vector3 exactStart;
		public Vector3 exactEnd;
		NavmeshBase graph;
		int currentNode;
		Vector3 currentPosition;
		int checkForDestroyedNodesCounter;
		RichPath path;
		int[] triBuffer = new int[3];

		/// <summary>Post process the funnel corridor or not</summary>
		public bool funnelSimplification = true;

		public RichFunnel () {
			left = Pathfinding.Pooling.ListPool<Vector3>.Claim();
			right = Pathfinding.Pooling.ListPool<Vector3>.Claim();
			nodes = new List<TriangleMeshNode>();
			this.graph = null;
		}

		/// <summary>Works like a constructor, but can be used even for pooled objects. Returns this for easy chaining</summary>
		public RichFunnel Initialize (RichPath path, NavmeshBase graph) {
			if (graph == null) throw new System.ArgumentNullException("graph");
			if (this.graph != null) throw new System.InvalidOperationException("Trying to initialize an already initialized object. " + graph);

			this.graph = graph;
			this.path = path;
			return this;
		}

		public override void OnEnterPool () {
			left.Clear();
			right.Clear();
			nodes.Clear();
			graph = null;
			currentNode = 0;
			checkForDestroyedNodesCounter = 0;
		}

		public TriangleMeshNode CurrentNode {
			get {
				var node = nodes[currentNode];
				if (!node.Destroyed) {
					return node;
				}
				return null;
			}
		}

		/// <summary>
		/// Build a funnel corridor from a node list slice.
		/// The nodes are assumed to be of type TriangleMeshNode.
		/// </summary>
		/// <param name="nodes">Nodes to build the funnel corridor from</param>
		/// <param name="start">Start index in the nodes list</param>
		/// <param name="end">End index in the nodes list, this index is inclusive</param>
		public void BuildFunnelCorridor (List<GraphNode> nodes, int start, int end) {
			//Make sure start and end points are on the correct nodes
			exactStart = (nodes[start] as MeshNode).ClosestPointOnNode(exactStart);
			exactEnd = (nodes[end] as MeshNode).ClosestPointOnNode(exactEnd);

			left.Clear();
			right.Clear();
			left.Add(exactStart);
			right.Add(exactStart);

			this.nodes.Clear();

			if (funnelSimplification) {
				List<GraphNode> tmp = ListPool<GraphNode>.Claim(end-start);

				var tagPenalties = path.seeker != null ? path.seeker.tagPenalties : Path.ZeroTagPenalties;
				var traversableTags = path.seeker != null ? path.seeker.traversableTags : -1;

				Funnel.Simplify(new Funnel.PathPart {
					startIndex = start,
					endIndex = end,
					startPoint = exactStart,
					endPoint = exactEnd,
					type = Funnel.PartType.NodeSequence,
				}, graph, nodes, tmp, tagPenalties, traversableTags);

				if (this.nodes.Capacity < tmp.Count) this.nodes.Capacity = tmp.Count;

				for (int i = 0; i < tmp.Count; i++) {
					// Guaranteed to be TriangleMeshNodes since they are all in the same graph
					var node = tmp[i] as TriangleMeshNode;
					if (node != null) this.nodes.Add(node);
				}

				ListPool<GraphNode>.Release(ref tmp);
			} else {
				if (this.nodes.Capacity < end-start) this.nodes.Capacity = (end-start);
				for (int i = start; i <= end; i++) {
					// Guaranteed to be TriangleMeshNodes since they are all in the same graph
					var node = nodes[i] as TriangleMeshNode;
					if (node != null) this.nodes.Add(node);
				}
			}

			for (int i = 0; i < this.nodes.Count-1; i++) {
				if (this.nodes[i].GetPortal(this.nodes[i+1], out var leftP, out var rightP)) {
					left.Add(leftP);
					right.Add(rightP);
				} else {
					// Can happen in case custom connections have been added
					left.Add((Vector3)this.nodes[i].position);
					right.Add((Vector3)this.nodes[i].position);
					left.Add((Vector3)this.nodes[i+1].position);
					right.Add((Vector3)this.nodes[i+1].position);
				}
			}

			left.Add(exactEnd);
			right.Add(exactEnd);
		}

		/// <summary>
		/// Split funnel at node index splitIndex and throw the nodes up to that point away and replace with prefix.
		/// Used when the AI has happened to get sidetracked and entered a node outside the funnel.
		/// </summary>
		void UpdateFunnelCorridor (int splitIndex, List<TriangleMeshNode> prefix) {
			nodes.RemoveRange(0, splitIndex);
			nodes.InsertRange(0, prefix);

			left.Clear();
			right.Clear();
			left.Add(exactStart);
			right.Add(exactStart);

			for (int i = 0; i < nodes.Count-1; i++) {
				if (nodes[i].GetPortal(nodes[i+1], out var leftP, out var rightP)) {
					left.Add(leftP);
					right.Add(rightP);
				}
			}

			left.Add(exactEnd);
			right.Add(exactEnd);
		}

		/// <summary>True if any node in the path is destroyed</summary>
		bool CheckForDestroyedNodes () {
			// Loop through all nodes and check if they are destroyed
			// If so, we really need a recalculation of our path quickly
			// since there might be an obstacle blocking our path after
			// a graph update or something similar
			for (int i = 0, t = nodes.Count; i < t; i++) {
				if (nodes[i].Destroyed) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Approximate distance (as the crow flies) to the endpoint of this path part.
		/// See: <see cref="exactEnd"/>
		/// </summary>
		public float DistanceToEndOfPath {
			get {
				var currentNode = CurrentNode;
				Vector3 closestOnNode = currentNode != null? currentNode.ClosestPointOnNode(currentPosition) : currentPosition;
				return (exactEnd - closestOnNode).magnitude;
			}
		}

		/// <summary>
		/// Clamps the position to the navmesh and repairs the path if the agent has moved slightly outside it.
		/// You should not call this method with anything other than the agent's position.
		/// </summary>
		public Vector3 ClampToNavmesh (Vector3 position) {
			if (path.transform != null) position = path.transform.InverseTransform(position);
			UnityEngine.Assertions.Assert.IsFalse(float.IsNaN(position.x));
			ClampToNavmeshInternal(ref position);
			if (path.transform != null) position = path.transform.Transform(position);
			UnityEngine.Assertions.Assert.IsFalse(float.IsNaN(position.x));
			return position;
		}

		/// <summary>
		/// Find the next points to move towards and clamp the position to the navmesh.
		///
		/// Returns: The position of the agent clamped to make sure it is inside the navmesh.
		/// </summary>
		/// <param name="position">The position of the agent.</param>
		/// <param name="buffer">Will be filled with up to numCorners points which are the next points in the path towards the target.</param>
		/// <param name="numCorners">See buffer.</param>
		/// <param name="lastCorner">True if the buffer contains the end point of the path.</param>
		/// <param name="requiresRepath">True if nodes along the path have been destroyed and a path recalculation is necessary.</param>
		public Vector3 Update (Vector3 position, List<Vector3> buffer, int numCorners, out bool lastCorner, out bool requiresRepath) {
			if (path.transform != null) position = path.transform.InverseTransform(position);
			UnityEngine.Assertions.Assert.IsFalse(float.IsNaN(position.x));

			lastCorner = false;
			requiresRepath = false;

			// Only check for destroyed nodes every 10 frames
			if (checkForDestroyedNodesCounter >= 10) {
				checkForDestroyedNodesCounter = 0;
				requiresRepath |= CheckForDestroyedNodes();
			} else {
				checkForDestroyedNodesCounter++;
			}

			bool nodesDestroyed = ClampToNavmeshInternal(ref position);

			currentPosition = position;

			if (nodesDestroyed) {
				// Some nodes on the path have been destroyed
				// we need to recalculate the path immediately
				requiresRepath = true;
				lastCorner = false;
				buffer.Add(position);
			} else if (!FindNextCorners(position, currentNode, buffer, numCorners, out lastCorner)) {
				Debug.LogError("Failed to find next corners in the path");
				buffer.Add(position);
			}

			if (path.transform != null) {
				for (int i = 0; i < buffer.Count; i++) {
					buffer[i] = path.transform.Transform(buffer[i]);
				}

				position = path.transform.Transform(position);
				UnityEngine.Assertions.Assert.IsFalse(float.IsNaN(position.x));
			}

			return position;
		}

		/// <summary>Cached object to avoid unnecessary allocations</summary>
		static Queue<TriangleMeshNode> navmeshClampQueue = new Queue<TriangleMeshNode>();
		/// <summary>Cached object to avoid unnecessary allocations</summary>
		static List<TriangleMeshNode> navmeshClampList = new List<TriangleMeshNode>();
		/// <summary>Cached object to avoid unnecessary allocations</summary>
		static Dictionary<TriangleMeshNode, TriangleMeshNode> navmeshClampDict = new Dictionary<TriangleMeshNode, TriangleMeshNode>();

		/// <summary>
		/// Searches for the node the agent is inside.
		/// This will also clamp the position to the navmesh
		/// and repair the funnel cooridor if the agent moves slightly outside it.
		///
		/// Returns: True if nodes along the path have been destroyed so that a path recalculation is required
		/// </summary>
		bool ClampToNavmeshInternal (ref Vector3 position) {
			var previousNode = nodes[currentNode];

			if (previousNode.Destroyed) {
				return true;
			}

			// Check if we are in the same node as we were in during the last frame and otherwise do a more extensive search
			if (previousNode.ContainsPoint(position)) {
				return false;
			}

			// This part of the code is relatively seldom called
			// Most of the time we are still on the same node as during the previous frame

			var que = navmeshClampQueue;
			var allVisited = navmeshClampList;
			var parent = navmeshClampDict;
			previousNode.TemporaryFlag1 = true;
			parent[previousNode] = null;
			que.Enqueue(previousNode);
			allVisited.Add(previousNode);

			float bestDistance = float.PositiveInfinity;
			Vector3 bestPoint = position;
			TriangleMeshNode bestNode = null;

			while (que.Count > 0) {
				var node = que.Dequeue();

				// Snap to the closest point in XZ space (keep the Y coordinate)
				// If we would have snapped to the closest point in 3D space, the agent
				// might slow down when traversing slopes
				var closest = node.ClosestPointOnNodeXZ(position);
				var dist = VectorMath.MagnitudeXZ(closest - position);

				// Check if this node is any closer than the previous best node.
				// Allow for a small margin to both avoid floating point errors and to allow
				// moving past very small local minima.
				if (dist <= bestDistance * 1.05f + 0.001f) {
					if (dist < bestDistance) {
						bestDistance = dist;
						bestPoint = closest;
						bestNode = node;
					}

					for (int i = 0; i < node.connections.Length; i++) {
						if (!node.connections[i].isOutgoing) continue;
						var neighbour = node.connections[i].node as TriangleMeshNode;
						if (neighbour != null && !neighbour.TemporaryFlag1) {
							neighbour.TemporaryFlag1 = true;
							parent[neighbour] = node;
							que.Enqueue(neighbour);
							allVisited.Add(neighbour);
						}
					}
				}
			}

			UnityEngine.Assertions.Assert.IsNotNull(bestNode);

			for (int i = 0; i < allVisited.Count; i++) allVisited[i].TemporaryFlag1 = false;
			allVisited.ClearFast();

			var closestNodeInPath = nodes.IndexOf(bestNode);

			// Move the x and z coordinates of the chararacter but not the y coordinate
			// because the navmesh surface may not line up with the ground
			position.x = bestPoint.x;
			position.z = bestPoint.z;

			// Check if the closest node
			// was on the path already or if we need to adjust it
			if (closestNodeInPath == -1) {
				// Reuse this list, because why not.
				var prefix = navmeshClampList;

				while (closestNodeInPath == -1) {
					prefix.Add(bestNode);
					bestNode = parent[bestNode];
					closestNodeInPath = nodes.IndexOf(bestNode);
				}

				// We have found a node containing the position, but it is outside the funnel
				// Recalculate the funnel to include this node
				exactStart = position;
				UpdateFunnelCorridor(closestNodeInPath, prefix);

				prefix.ClearFast();

				// Restart from the first node in the updated path
				currentNode = 0;
			} else {
				currentNode = closestNodeInPath;
			}

			parent.Clear();
			// Do a quick check to see if the next node in the path has been destroyed
			// If that is the case then we should plan a new path immediately
			return currentNode + 1 < nodes.Count && nodes[currentNode+1].Destroyed;
		}

		/// <summary>
		/// Fill wallBuffer with all navmesh wall segments close to the current position.
		/// A wall segment is a node edge which is not shared by any other neighbour node, i.e an outer edge on the navmesh.
		/// </summary>
		public void FindWalls (List<Vector3> wallBuffer, float range) {
			FindWalls(currentNode, wallBuffer, currentPosition, range);
		}

		void FindWalls (int nodeIndex, List<Vector3> wallBuffer, Vector3 position, float range) {
			if (range <= 0) return;

			bool negAbort = false;
			bool posAbort = false;

			range *= range;

			//Looping as 0,-1,1,-2,2,-3,3,-4,4 etc. Avoids code duplication by keeping it to one loop instead of two
			for (int i = 0; !negAbort || !posAbort; i = i < 0 ? -i : -i-1) {
				if (i < 0 && negAbort) continue;
				if (i > 0 && posAbort) continue;

				if (i < 0 && nodeIndex+i < 0) {
					negAbort = true;
					continue;
				}

				if (i > 0 && nodeIndex+i >= nodes.Count) {
					posAbort = true;
					continue;
				}

				TriangleMeshNode prev = nodeIndex+i-1 < 0 ? null : nodes[nodeIndex+i-1];
				TriangleMeshNode node = nodes[nodeIndex+i];
				TriangleMeshNode next = nodeIndex+i+1 >= nodes.Count ? null : nodes[nodeIndex+i+1];

				if (node.Destroyed) {
					break;
				}

				var dir = node.ClosestPointOnNodeXZ(position)-position;
				dir.y = 0;
				if (dir.sqrMagnitude > range) {
					if (i < 0) negAbort = true;
					else posAbort = true;
					continue;
				}

				for (int j = 0; j < 3; j++) triBuffer[j] = 0;

				for (int j = 0; j < node.connections.Length; j++) {
					var other = node.connections[j].node as TriangleMeshNode;
					if (other == null) continue;

					int va = -1;
					for (int a = 0; a < 3; a++) {
						for (int b = 0; b < 3; b++) {
							if (node.GetVertex(a) == other.GetVertex((b+1) % 3) && node.GetVertex((a+1) % 3) == other.GetVertex(b)) {
								va = a;
								a = 3;
								break;
							}
						}
					}
					if (va == -1) {
						//No direct connection
					} else {
						triBuffer[va] = other == prev || other == next ? 2 : 1;
					}
				}

				for (int j = 0; j < 3; j++) {
					//Tribuffer values
					// 0 : Navmesh border, outer edge
					// 1 : Inner edge, to node inside funnel
					// 2 : Inner edge, to node outside funnel
					if (triBuffer[j] == 0) {
						//Add edge to list of walls
						wallBuffer.Add((Vector3)node.GetVertex(j));
						wallBuffer.Add((Vector3)node.GetVertex((j+1) % 3));
					}
				}
			}

			if (path.transform != null) {
				for (int i = 0; i < wallBuffer.Count; i++) {
					wallBuffer[i] = path.transform.Transform(wallBuffer[i]);
				}
			}
		}

		bool FindNextCorners (Vector3 origin, int startIndex, List<Vector3> funnelPath, int numCorners, out bool lastCorner) {
			lastCorner = false;

			if (left == null) throw new System.Exception("left list is null");
			if (right == null) throw new System.Exception("right list is null");
			if (funnelPath == null) throw new System.ArgumentNullException("funnelPath");

			if (left.Count != right.Count) throw new System.ArgumentException("left and right lists must have equal length");

			int diagonalCount = left.Count;

			if (diagonalCount == 0) throw new System.ArgumentException("no diagonals");

			if (diagonalCount-startIndex < 3) {
				//Direct path
				funnelPath.Add(left[diagonalCount-1]);
				lastCorner = true;
				return true;
			}

#if ASTARDEBUG
			for (int i = startIndex; i < left.Count-1; i++) {
				Debug.DrawLine(left[i], left[i+1], Color.red);
				Debug.DrawLine(right[i], right[i+1], Color.magenta);
				Debug.DrawRay(right[i], Vector3.up, Color.magenta);
			}
			for (int i = 0; i < left.Count; i++) {
				Debug.DrawLine(right[i], left[i], Color.cyan);
			}
#endif

			//Remove identical vertices
			while (left[startIndex+1] == left[startIndex+2] && right[startIndex+1] == right[startIndex+2]) {
				//System.Console.WriteLine ("Removing identical left and right");
				//left.RemoveAt (1);
				//right.RemoveAt (1);
				startIndex++;

				if (diagonalCount-startIndex <= 3) {
					return false;
				}
			}

			Vector3 swPoint = left[startIndex+2];
			if (swPoint == left[startIndex+1]) {
				swPoint = right[startIndex+2];
			}


			//Test
			while (VectorMath.IsColinearXZ(origin, left[startIndex+1], right[startIndex+1]) || VectorMath.RightOrColinearXZ(left[startIndex+1], right[startIndex+1], swPoint) == VectorMath.RightOrColinearXZ(left[startIndex+1], right[startIndex+1], origin)) {
#if ASTARDEBUG
				Debug.DrawLine(left[startIndex+1], right[startIndex+1], new Color(0, 0, 0, 0.5F));
				Debug.DrawLine(origin, swPoint, new Color(0, 0, 0, 0.5F));
#endif
				//left.RemoveAt (1);
				//right.RemoveAt (1);
				startIndex++;

				if (diagonalCount-startIndex < 3) {
					//Debug.Log ("#2 " + left.Count + " - " + startIndex + " = " + (left.Count-startIndex));
					//Direct path
					funnelPath.Add(left[diagonalCount-1]);
					lastCorner = true;
					return true;
				}

				swPoint = left[startIndex+2];
				if (swPoint == left[startIndex+1]) {
					swPoint = right[startIndex+2];
				}
			}


			//funnelPath.Add (origin);

			Vector3 portalApex = origin;
			Vector3 portalLeft = left[startIndex+1];
			Vector3 portalRight = right[startIndex+1];

			int apexIndex = startIndex+0;
			int rightIndex = startIndex+1;
			int leftIndex = startIndex+1;

			for (int i = startIndex+2; i < diagonalCount; i++) {
				if (funnelPath.Count >= numCorners) {
					return true;
				}

				if (funnelPath.Count > 2000) {
					Debug.LogWarning("Avoiding infinite loop. Remove this check if you have this long paths.");
					break;
				}

				Vector3 pLeft = left[i];
				Vector3 pRight = right[i];

				/*Debug.DrawLine (portalApex,portalLeft,Color.red);
				 * Debug.DrawLine (portalApex,portalRight,Color.yellow);
				 * Debug.DrawLine (portalApex,left,Color.cyan);
				 * Debug.DrawLine (portalApex,right,Color.cyan);*/

				if (VectorMath.SignedTriangleAreaTimes2XZ(portalApex, portalRight, pRight) >= 0) {
					if (portalApex == portalRight || VectorMath.SignedTriangleAreaTimes2XZ(portalApex, portalLeft, pRight) <= 0) {
						portalRight = pRight;
						rightIndex = i;
					} else {
						funnelPath.Add(portalLeft);
						portalApex = portalLeft;
						apexIndex = leftIndex;

						portalLeft = portalApex;
						portalRight = portalApex;

						leftIndex = apexIndex;
						rightIndex = apexIndex;

						i = apexIndex;

						continue;
					}
				}

				if (VectorMath.SignedTriangleAreaTimes2XZ(portalApex, portalLeft, pLeft) <= 0) {
					if (portalApex == portalLeft || VectorMath.SignedTriangleAreaTimes2XZ(portalApex, portalRight, pLeft) >= 0) {
						portalLeft = pLeft;
						leftIndex = i;
					} else {
						funnelPath.Add(portalRight);
						portalApex = portalRight;
						apexIndex = rightIndex;

						portalLeft = portalApex;
						portalRight = portalApex;

						leftIndex = apexIndex;
						rightIndex = apexIndex;

						i = apexIndex;

						continue;
					}
				}
			}

			lastCorner = true;
			funnelPath.Add(left[diagonalCount-1]);

			return true;
		}
	}

	public struct FakeTransform {
		public Vector3 position;
		public Quaternion rotation;
	}

	public class RichSpecial : RichPathPart {
		public OffMeshLinks.OffMeshLinkTracer nodeLink;
		public FakeTransform first => new FakeTransform { position = nodeLink.relativeStart, rotation = nodeLink.isReverse ? nodeLink.link.end.rotation : nodeLink.link.start.rotation };
		public FakeTransform second => new FakeTransform { position = nodeLink.relativeEnd, rotation = nodeLink.isReverse ? nodeLink.link.start.rotation : nodeLink.link.end.rotation };
		public bool reverse => nodeLink.isReverse;

		public override void OnEnterPool () {
			nodeLink = default;
		}

		/// <summary>Works like a constructor, but can be used even for pooled objects. Returns this for easy chaining</summary>
		public RichSpecial Initialize (OffMeshLinks.OffMeshLinkTracer nodeLink) {
			this.nodeLink = nodeLink;
			return this;
		}
	}
}
