// #define VALIDATE_AABB_TREE
using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Pathfinding.Collections {
	/// <summary>
	/// Axis Aligned Bounding Box Tree.
	///
	/// Holds a bounding box tree with arbitrary data.
	///
	/// The tree self-balances itself regularly when nodes are added.
	/// </summary>
	public class AABBTree<T> {
		Node[] nodes = new Node[0];
		int root = NoNode;
		readonly Stack<int> freeNodes = new Stack<int>();
		int rebuildCounter = 64;
		const int NoNode = -1;

		struct Node {
			public Bounds bounds;
			public uint flags;
			const uint TagInsideBit = 1u << 30;
			const uint TagPartiallyInsideBit = 1u << 31;
			const uint AllocatedBit = 1u << 29;
			const uint ParentMask = ~(TagInsideBit | TagPartiallyInsideBit | AllocatedBit);
			public const int InvalidParent = (int)ParentMask;
			public bool wholeSubtreeTagged {
				get => (flags & TagInsideBit) != 0;
				set => flags = (flags & ~TagInsideBit) | (value ? TagInsideBit : 0);
			}
			public bool subtreePartiallyTagged {
				get => (flags & TagPartiallyInsideBit) != 0;
				set => flags = (flags & ~TagPartiallyInsideBit) | (value ? TagPartiallyInsideBit : 0);
			}
			public bool isAllocated {
				get => (flags & AllocatedBit) != 0;
				set => flags = (flags & ~AllocatedBit) | (value ? AllocatedBit : 0);
			}
			public bool isLeaf => left == NoNode;
			public int parent {
				get => (int)(flags & ParentMask);
				set => flags = (flags & ~ParentMask) | (uint)value;
			}
			public int left;
			public int right;
			public T value;
		}

		/// <summary>A key to a leaf node in the tree</summary>
		public readonly struct Key {
			internal readonly int value;
			public int node => value - 1;
			public bool isValid => value != 0;
			internal Key(int node) { this.value = node + 1; }
		}

		static float ExpansionRequired (Bounds b, Bounds b2) {
			var union = b;
			union.Encapsulate(b2);
			return union.size.x*union.size.y*union.size.z - b.size.x*b.size.y*b.size.z;
		}

		/// <summary>User data for a node in the tree</summary>
		public T this[Key key] => nodes[key.node].value;

		/// <summary>Bounding box of a given node</summary>
		public Bounds GetBounds (Key key) {
			if (!key.isValid) throw new System.ArgumentException("Key is not valid");
			var node = nodes[key.node];
			if (!node.isAllocated) throw new System.ArgumentException("Key does not point to an allocated node");
			if (!node.isLeaf) throw new System.ArgumentException("Key does not point to a leaf node");
			return node.bounds;
		}

		int AllocNode () {
			if (!freeNodes.TryPop(out int newNodeId)) {
				int prevLength = nodes.Length;
				Memory.Realloc(ref nodes, Mathf.Max(8, nodes.Length*2));
				for (int i = nodes.Length - 1; i >= prevLength; i--) FreeNode(i);
				newNodeId = freeNodes.Pop();
#if VALIDATE_AABB_TREE
				Assert.IsFalse(nodes[newNodeId].isAllocated);
#endif
			}
			return newNodeId;
		}

		void FreeNode (int node) {
			nodes[node].isAllocated = false;
			nodes[node].value = default;
			freeNodes.Push(node);
		}

		/// <summary>
		/// Rebuilds the whole tree.
		///
		/// This can make it more balanced, and thus faster to query.
		/// </summary>
		public void Rebuild () {
			var leaves = new UnsafeSpan<int>(Unity.Collections.Allocator.Temp, nodes.Length);
			int nLeaves = 0;
			for (int i = 0; i < nodes.Length; i++) {
				if (!nodes[i].isAllocated) continue;
				if (nodes[i].isLeaf) leaves[nLeaves++] = i;
				else FreeNode(i);
			}
			root = Rebuild(leaves.Slice(0, nLeaves), Node.InvalidParent);
			rebuildCounter = Mathf.Max(64, nLeaves / 3);
			Validate(root);
		}

		/// <summary>Removes all nodes from the tree</summary>
		public void Clear () {
			for (int i = 0; i < nodes.Length; i++) {
				if (nodes[i].isAllocated) FreeNode(i);
			}
			root = NoNode;
			rebuildCounter = 64;
		}

		struct AABBComparer : IComparer<int> {
			public Node[] nodes;
			public int dim;

			public int Compare(int a, int b) => nodes[a].bounds.center[dim].CompareTo(nodes[b].bounds.center[dim]);
		}

		static int ArgMax (Vector3 v) {
			var m = Mathf.Max(v.x, Mathf.Max(v.y, v.z));
			return m == v.x ? 0: (m == v.y ? 1 : 2);
		}

		int Rebuild (UnsafeSpan<int> leaves, int parent) {
			if (leaves.Length == 0) return NoNode;
			if (leaves.Length == 1) {
				nodes[leaves[0]].parent = parent;
				return leaves[0];
			}

			var bounds = nodes[leaves[0]].bounds;
			for (int i = 1; i < leaves.Length; i++) bounds.Encapsulate(nodes[leaves[i]].bounds);

			leaves.Sort(new AABBComparer { nodes = nodes, dim = ArgMax(bounds.extents) });
			var nodeId = AllocNode();
			nodes[nodeId] = new Node {
				bounds = bounds,
				left = Rebuild(leaves.Slice(0, leaves.Length/2), nodeId),
				right = Rebuild(leaves.Slice(leaves.Length/2), nodeId),
				parent = parent,
				isAllocated = true,
			};
			return nodeId;
		}

		/// <summary>
		/// Moves a node to a new position.
		///
		/// This will update the tree structure to account for the new bounding box.
		/// This is equivalent to removing the node and adding it again with the new bounds, but it preserves the key value.
		/// </summary>
		/// <param name="key">Key to the node to move</param>
		/// <param name="bounds">New bounds of the node</param>
		public void Move (Key key, Bounds bounds) {
			var value = nodes[key.node].value;
			Remove(key);
			var newKey = Add(bounds, value);
			// The first node added after a remove will have the same node index as the just removed node
			Assert.IsTrue(newKey.node == key.node);
		}

		[System.Diagnostics.Conditional("VALIDATE_AABB_TREE")]
		void Validate (int node) {
			if (node == NoNode) return;
			var n = nodes[node];
			Assert.IsTrue(n.isAllocated);
			if (node == root) {
				Assert.AreEqual(Node.InvalidParent, n.parent);
			} else {
				Assert.AreNotEqual(Node.InvalidParent, n.parent);
			}
			if (n.isLeaf) {
				Assert.AreEqual(NoNode, n.right);
			} else {
				Assert.AreNotEqual(NoNode, n.right);
				Assert.AreNotEqual(n.left, n.right);
				Assert.AreEqual(node, nodes[n.left].parent);
				Assert.AreEqual(node, nodes[n.right].parent);
				Assert.IsTrue(math.all((float3)n.bounds.min <= (float3)nodes[n.left].bounds.min + 0.0001f));
				Assert.IsTrue(math.all((float3)n.bounds.max >= (float3)nodes[n.left].bounds.max - 0.0001f));
				Assert.IsTrue(math.all((float3)n.bounds.min <= (float3)nodes[n.right].bounds.min + 0.0001f));
				Assert.IsTrue(math.all((float3)n.bounds.max >= (float3)nodes[n.right].bounds.max - 0.0001f));
				Validate(n.left);
				Validate(n.right);
			}
		}

		public Bounds Remove (Key key) {
			if (!key.isValid) throw new System.ArgumentException("Key is not valid");
			var node = nodes[key.node];
			if (!node.isAllocated) throw new System.ArgumentException("Key does not point to an allocated node");
			if (!node.isLeaf) throw new System.ArgumentException("Key does not point to a leaf node");

			if (key.node == root) {
				root = NoNode;
				FreeNode(key.node);
				return node.bounds;
			}

			// Remove the parent from the tree and replace it with sibling
			var parentToRemoveId = node.parent;
			var parentToRemove = nodes[parentToRemoveId];
			var siblingId = parentToRemove.left == key.node ? parentToRemove.right : parentToRemove.left;
			FreeNode(parentToRemoveId);
			FreeNode(key.node);
			nodes[siblingId].parent = parentToRemove.parent;

			if (parentToRemove.parent == Node.InvalidParent) {
				root = siblingId;
			} else {
				if (nodes[parentToRemove.parent].left == parentToRemoveId) {
					nodes[parentToRemove.parent].left = siblingId;
				} else {
					nodes[parentToRemove.parent].right = siblingId;
				}
			}

			// Rebuild bounding boxes
			var tmpNodeId = nodes[siblingId].parent;
			while (tmpNodeId != Node.InvalidParent) {
				ref var tmpNode = ref nodes[tmpNodeId];
				var bounds = nodes[tmpNode.left].bounds;
				bounds.Encapsulate(nodes[tmpNode.right].bounds);
				tmpNode.bounds = bounds;
				tmpNode.subtreePartiallyTagged = nodes[tmpNode.left].subtreePartiallyTagged | nodes[tmpNode.right].subtreePartiallyTagged;
				tmpNodeId = tmpNode.parent;
			}
			Validate(root);
			return node.bounds;
		}

		public Key Add (Bounds bounds, T value) {
			var newNodeId = AllocNode();

			nodes[newNodeId] = new Node {
				bounds = bounds,
				parent = Node.InvalidParent,
				left = NoNode,
				right = NoNode,
				value = value,
				isAllocated = true,
			};

			if (root == NoNode) {
				root = newNodeId;
				Validate(root);
				return new Key(newNodeId);
			}

			int nodeId = root;
			while (true) {
				var node = nodes[nodeId];

				// We can no longer guarantee that the whole subtree of this node is tagged,
				// as the new node is not tagged
				nodes[nodeId].wholeSubtreeTagged = false;

				if (node.isLeaf) {
					var newInnerId = AllocNode();

					if (node.parent != Node.InvalidParent) {
						if (nodes[node.parent].left == nodeId) nodes[node.parent].left = newInnerId;
						else nodes[node.parent].right = newInnerId;
					}

					bounds.Encapsulate(node.bounds);
					nodes[newInnerId] = new Node {
						bounds = bounds,
						left = nodeId,
						right = newNodeId,
						parent = node.parent,
						isAllocated = true,
					};
					nodes[newNodeId].parent = nodes[nodeId].parent = newInnerId;
					if (root == nodeId) root = newInnerId;

					if (rebuildCounter-- <= 0) Rebuild();
					Validate(root);
					return new Key(newNodeId);
				} else {
					// Inner node
					nodes[nodeId].bounds.Encapsulate(bounds);
					float leftCost = ExpansionRequired(nodes[node.left].bounds, bounds);
					float rightCost = ExpansionRequired(nodes[node.right].bounds, bounds);
					nodeId = leftCost < rightCost ? node.left : node.right;
				}
			}
		}

		/// <summary>Queries the tree for all objects that touch the specified bounds.</summary>
		/// <param name="bounds">Bounding box to search within</param>
		/// <param name="buffer">The results will be added to the buffer</param>
		public void Query(Bounds bounds, List<T> buffer) => QueryNode(root, bounds, buffer);

		void QueryNode (int node, Bounds bounds, List<T> buffer) {
			if (node == NoNode || !bounds.Intersects(nodes[node].bounds)) return;

			if (nodes[node].isLeaf) {
				buffer.Add(nodes[node].value);
			} else {
				// Search children
				QueryNode(nodes[node].left, bounds, buffer);
				QueryNode(nodes[node].right, bounds, buffer);
			}
		}

		/// <summary>Queries the tree for all objects that have been previously tagged using the <see cref="Tag"/> method.</summary>
		/// <param name="buffer">The results will be added to the buffer</param>
		/// <param name="clearTags">If true, all tags will be cleared after this call. If false, the tags will remain and can be queried again later.</param>
		public void QueryTagged(List<T> buffer, bool clearTags = false) => QueryTaggedNode(root, clearTags, buffer);

		void QueryTaggedNode (int node, bool clearTags, List<T> buffer) {
			if (node == NoNode || !nodes[node].subtreePartiallyTagged) return;

			if (clearTags) {
				nodes[node].wholeSubtreeTagged = false;
				nodes[node].subtreePartiallyTagged = false;
			}

			if (nodes[node].isLeaf) {
				buffer.Add(nodes[node].value);
			} else {
				QueryTaggedNode(nodes[node].left, clearTags, buffer);
				QueryTaggedNode(nodes[node].right, clearTags, buffer);
			}
		}

		/// <summary>
		/// Tags a particular object.
		///
		/// Any previously tagged objects stay tagged.
		/// You can retrieve the tagged objects using the <see cref="QueryTagged"/> method.
		/// </summary>
		/// <param name="key">Key to the object to tag</param>
		public void Tag (Key key) {
			if (!key.isValid) throw new System.ArgumentException("Key is not valid");
			if (key.node < 0 || key.node >= nodes.Length) throw new System.ArgumentException("Key does not point to a valid node");
			ref var node = ref nodes[key.node];
			if (!node.isAllocated) throw new System.ArgumentException("Key does not point to an allocated node");
			if (!node.isLeaf) throw new System.ArgumentException("Key does not point to a leaf node");
			node.wholeSubtreeTagged = true;
			int nodeId = key.node;
			while (nodeId != Node.InvalidParent) {
				nodes[nodeId].subtreePartiallyTagged = true;
				nodeId = nodes[nodeId].parent;
			}
		}

		/// <summary>
		/// Tags all objects that touch the specified bounds.
		///
		/// Any previously tagged objects stay tagged.
		/// You can retrieve the tagged objects using the <see cref="QueryTagged"/> method.
		/// </summary>
		/// <param name="bounds">Bounding box to search within</param>
		public void Tag(Bounds bounds) => TagNode(root, bounds);

		bool TagNode (int node, Bounds bounds) {
			if (node == NoNode || nodes[node].wholeSubtreeTagged) return true; // Nothing to do
			if (!bounds.Intersects(nodes[node].bounds)) return false;

			// TODO: Could make this less conservative by propagating info from the child nodes
			nodes[node].subtreePartiallyTagged = true;
			if (nodes[node].isLeaf) return nodes[node].wholeSubtreeTagged = true;
			else return nodes[node].wholeSubtreeTagged = TagNode(nodes[node].left, bounds) & TagNode(nodes[node].right, bounds);
		}
	}
}
