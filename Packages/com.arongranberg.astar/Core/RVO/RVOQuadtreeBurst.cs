namespace Pathfinding.RVO {
	using UnityEngine;
	using Pathfinding.ECS.RVO;
	using Unity.Burst;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Unity.Collections;
	using Pathfinding.Drawing;

	/// <summary>
	/// Quadtree for quick nearest neighbour search of rvo agents.
	/// See: Pathfinding.RVO.Simulator
	/// </summary>
	public struct RVOQuadtreeBurst {
		const int LeafSize = 16;
		const int MaxDepth = 10;

		NativeArray<int> agents;
		NativeArray<int> childPointers;
		NativeArray<float3> boundingBoxBuffer;
		NativeArray<int> agentCountBuffer;
		NativeArray<float3> agentPositions;
		NativeArray<float> agentRadii;
		NativeArray<float> maxSpeeds;
		NativeArray<float> maxRadius;
		NativeArray<float> nodeAreas;
		MovementPlane movementPlane;

		const int LeafNodeBit = 1 << 30;
		const int BitPackingShift = 15;
		const int BitPackingMask = (1 << BitPackingShift) - 1;
		const int MaxAgents = BitPackingMask;

		/// <summary>
		/// For a given number, contains the index of the first non-zero bit.
		/// Only the values 0 through 15 are used when movementPlane is XZ or XY.
		///
		/// Use bytes instead of ints to save some precious L1 cache memory.
		/// </summary>
		static readonly byte[] ChildLookup = new byte[256];

		static RVOQuadtreeBurst() {
			for (int v = 0; v < 256; v++) {
				for (int i = 0; i < 8; i++) {
					if (((v >> i) & 0x1) != 0) {
						ChildLookup[v] = (byte)i;
						break;
					}
				}
			}
		}

		public Rect bounds {
			get {
				return boundingBoxBuffer.IsCreated ? Rect.MinMaxRect(boundingBoxBuffer[0].x, boundingBoxBuffer[0].y, boundingBoxBuffer[1].x, boundingBoxBuffer[1].y) : new Rect();
			}
		}

		static int InnerNodeCountUpperBound (int numAgents, MovementPlane movementPlane) {
			// Every LeafSize number of nodes can cause a split at most MaxDepth
			// number of times. Each split needs 4 (or 8) units of space.
			// Round the value up by adding LeafSize-1 to the numerator.
			// This is an upper bound. Most likely the tree will contain significantly fewer nodes.
			return ((movementPlane == MovementPlane.Arbitrary ? 8 : 4) * MaxDepth * numAgents + LeafSize-1)/LeafSize;
		}

		public void Dispose () {
			agents.Dispose();
			childPointers.Dispose();
			boundingBoxBuffer.Dispose();
			agentCountBuffer.Dispose();
			maxSpeeds.Dispose();
			maxRadius.Dispose();
			nodeAreas.Dispose();
			agentPositions.Dispose();
			agentRadii.Dispose();
		}

		void Reserve (int minSize) {
			if (!boundingBoxBuffer.IsCreated) {
				boundingBoxBuffer = new NativeArray<float3>(4, Allocator.Persistent);
				agentCountBuffer = new NativeArray<int>(1, Allocator.Persistent);
			}
			// Create a new agent's array. Round up to nearest multiple multiple of 2 to avoid re-allocating often if the agent count slowly increases
			int roundedAgents = math.ceilpow2(minSize);
			Util.Memory.Realloc(ref agents, roundedAgents, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			Util.Memory.Realloc(ref agentPositions, roundedAgents, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			Util.Memory.Realloc(ref agentRadii, roundedAgents, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			Util.Memory.Realloc(ref childPointers, InnerNodeCountUpperBound(roundedAgents, movementPlane), Allocator.Persistent, NativeArrayOptions.ClearMemory);
			Util.Memory.Realloc(ref maxSpeeds, childPointers.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			Util.Memory.Realloc(ref nodeAreas, childPointers.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			Util.Memory.Realloc(ref maxRadius, childPointers.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		}

		public JobBuild BuildJob (NativeArray<float3> agentPositions, NativeArray<AgentIndex> agentVersions, NativeArray<float> agentSpeeds, NativeArray<float> agentRadii, int numAgents, MovementPlane movementPlane) {
			if (numAgents >= MaxAgents) throw new System.Exception("Too many agents. Cannot have more than " + MaxAgents);
			Reserve(numAgents);

			this.movementPlane = movementPlane;

			return new JobBuild {
					   agents = agents,
					   agentVersions = agentVersions,
					   agentPositions = agentPositions,
					   agentSpeeds = agentSpeeds,
					   agentRadii = agentRadii,
					   outMaxSpeeds = maxSpeeds,
					   outMaxRadius = maxRadius,
					   outArea = nodeAreas,
					   outAgentRadii = this.agentRadii, // Will be copied. These are copied so that the quadtree remains in a valid state even after new agents have been added/removed. This is important for the QueryArea method which may be called at any time.
					   outAgentPositions = this.agentPositions, // Will be copied
					   outBoundingBox = boundingBoxBuffer,
					   outAgentCount = agentCountBuffer,
					   outChildPointers = childPointers,
					   numAgents = numAgents,
					   movementPlane = movementPlane,
			};
		}

		[BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
		public struct JobBuild : IJob {
			/// <summary>Length should be greater or equal to agentPositions.Length</summary>
			public NativeArray<int> agents;

			[ReadOnly]
			public NativeArray<float3> agentPositions;

			[ReadOnly]
			public NativeArray<AgentIndex> agentVersions;

			[ReadOnly]
			public NativeArray<float> agentSpeeds;

			[ReadOnly]
			public NativeArray<float> agentRadii;

			/// <summary>Should have size 2</summary>
			[WriteOnly]
			public NativeArray<float3> outBoundingBox;

			/// <summary>Should have size 1</summary>
			[WriteOnly]
			public NativeArray<int> outAgentCount;

			/// <summary>Should have size: InnerNodeCountUpperBound(numAgents)</summary>
			public NativeArray<int> outChildPointers;

			/// <summary>Should have size: InnerNodeCountUpperBound(numAgents)</summary>
			public NativeArray<float> outMaxSpeeds;

			/// <summary>Should have size: InnerNodeCountUpperBound(numAgents)</summary>
			public NativeArray<float> outMaxRadius;

			/// <summary>Should have size: InnerNodeCountUpperBound(numAgents)</summary>
			public NativeArray<float> outArea;

			[WriteOnly]
			public NativeArray<float3> outAgentPositions;

			[WriteOnly]
			public NativeArray<float> outAgentRadii;

			public int numAgents;

			public MovementPlane movementPlane;

			static int Partition (NativeSlice<int> indices, int startIndex, int endIndex, NativeSlice<float> coordinates, float splitPoint) {
				for (int i = startIndex; i < endIndex; i++) {
					if (coordinates[indices[i]] > splitPoint) {
						endIndex--;
						var tmp = indices[i];
						indices[i] = indices[endIndex];
						indices[endIndex] = tmp;
						i--;
					}
				}
				return endIndex;
			}

			void BuildNode (float3 boundsMin, float3 boundsMax, int depth, int agentsStart, int agentsEnd, int nodeOffset, ref int firstFreeChild) {
				if (agentsEnd - agentsStart > LeafSize && depth < MaxDepth) {
					if (movementPlane == MovementPlane.Arbitrary) {
						// Split the node into 8 equally sized (by volume) child nodes
						var xs = new NativeSlice<float3>(agentPositions).SliceWithStride<float>(0);
						var ys = new NativeSlice<float3>(agentPositions).SliceWithStride<float>(4);
						var zs = new NativeSlice<float3>(agentPositions).SliceWithStride<float>(8);

						float3 boundsMid = (boundsMin + boundsMax) * 0.5f;
						int s0 = agentsStart;
						int s8 = agentsEnd;
						int s4 = Partition(agents, s0, s8, xs, boundsMid.x);
						int s2 = Partition(agents, s0, s4, ys, boundsMid.y);
						int s6 = Partition(agents, s4, s8, ys, boundsMid.y);
						int s1 = Partition(agents, s0, s2, zs, boundsMid.z);
						int s3 = Partition(agents, s2, s4, zs, boundsMid.z);
						int s5 = Partition(agents, s4, s6, zs, boundsMid.z);
						int s7 = Partition(agents, s6, s8, zs, boundsMid.z);

						// Note: guaranteed to be large enough
						int childIndex = firstFreeChild;
						outChildPointers[nodeOffset] = childIndex;
						firstFreeChild += 8;

						// x    y     z
						// low  low  low
						// low  low  high
						// low  high low
						// low  high high
						// high low  low
						// high low  high
						// high high low
						// high high high
						var min = boundsMin;
						var mid = boundsMid;
						var max = boundsMax;
						BuildNode(new float3(min.x, min.y, min.z), new float3(mid.x, mid.y, mid.z), depth + 1, s0, s1, childIndex + 0, ref firstFreeChild);
						BuildNode(new float3(min.x, min.y, mid.z), new float3(mid.x, mid.y, max.z), depth + 1, s1, s2, childIndex + 1, ref firstFreeChild);
						BuildNode(new float3(min.x, mid.y, min.z), new float3(mid.x, max.y, mid.z), depth + 1, s2, s3, childIndex + 2, ref firstFreeChild);
						BuildNode(new float3(min.x, mid.y, mid.z), new float3(mid.x, max.y, max.z), depth + 1, s3, s4, childIndex + 3, ref firstFreeChild);
						BuildNode(new float3(mid.x, min.y, min.z), new float3(max.x, mid.y, mid.z), depth + 1, s4, s5, childIndex + 4, ref firstFreeChild);
						BuildNode(new float3(mid.x, min.y, mid.z), new float3(max.x, mid.y, max.z), depth + 1, s5, s6, childIndex + 5, ref firstFreeChild);
						BuildNode(new float3(mid.x, mid.y, min.z), new float3(max.x, max.y, mid.z), depth + 1, s6, s7, childIndex + 6, ref firstFreeChild);
						BuildNode(new float3(mid.x, mid.y, mid.z), new float3(max.x, max.y, max.z), depth + 1, s7, s8, childIndex + 7, ref firstFreeChild);
					} else if (movementPlane == MovementPlane.XY) {
						// Split the node into 4 equally sized (by area) child nodes
						var xs = new NativeSlice<float3>(agentPositions).SliceWithStride<float>(0);
						var ys = new NativeSlice<float3>(agentPositions).SliceWithStride<float>(4);

						float3 boundsMid = (boundsMin + boundsMax) * 0.5f;
						int s0 = agentsStart;
						int s4 = agentsEnd;
						int s2 = Partition(agents, s0, s4, xs, boundsMid.x);
						int s1 = Partition(agents, s0, s2, ys, boundsMid.y);
						int s3 = Partition(agents, s2, s4, ys, boundsMid.y);

						// Note: guaranteed to be large enough
						int childIndex = firstFreeChild;
						outChildPointers[nodeOffset] = childIndex;
						firstFreeChild += 4;

						// x    y
						// low  low
						// low  high
						// high low
						// high high
						BuildNode(new float3(boundsMin.x, boundsMin.y, boundsMin.z), new float3(boundsMid.x, boundsMid.y, boundsMax.z), depth + 1, s0, s1, childIndex + 0, ref firstFreeChild);
						BuildNode(new float3(boundsMin.x, boundsMid.y, boundsMin.z), new float3(boundsMid.x, boundsMax.y, boundsMax.z), depth + 1, s1, s2, childIndex + 1, ref firstFreeChild);
						BuildNode(new float3(boundsMid.x, boundsMin.y, boundsMin.z), new float3(boundsMax.x, boundsMid.y, boundsMax.z), depth + 1, s2, s3, childIndex + 2, ref firstFreeChild);
						BuildNode(new float3(boundsMid.x, boundsMid.y, boundsMin.z), new float3(boundsMax.x, boundsMax.y, boundsMax.z), depth + 1, s3, s4, childIndex + 3, ref firstFreeChild);
					} else {
						// Split the node into 4 equally sized (by area) child nodes
						var xs = new NativeSlice<float3>(agentPositions).SliceWithStride<float>(0);
						var zs = new NativeSlice<float3>(agentPositions).SliceWithStride<float>(8);

						float3 boundsMid = (boundsMin + boundsMax) * 0.5f;
						int s0 = agentsStart;
						int s4 = agentsEnd;
						int s2 = Partition(agents, s0, s4, xs, boundsMid.x);
						int s1 = Partition(agents, s0, s2, zs, boundsMid.z);
						int s3 = Partition(agents, s2, s4, zs, boundsMid.z);

						// Note: guaranteed to be large enough
						int childIndex = firstFreeChild;
						outChildPointers[nodeOffset] = childIndex;
						firstFreeChild += 4;

						// x    z
						// low  low
						// low  high
						// high low
						// high high
						BuildNode(new float3(boundsMin.x, boundsMin.y, boundsMin.z), new float3(boundsMid.x, boundsMax.y, boundsMid.z), depth + 1, s0, s1, childIndex + 0, ref firstFreeChild);
						BuildNode(new float3(boundsMin.x, boundsMin.y, boundsMid.z), new float3(boundsMid.x, boundsMax.y, boundsMax.z), depth + 1, s1, s2, childIndex + 1, ref firstFreeChild);
						BuildNode(new float3(boundsMid.x, boundsMin.y, boundsMin.z), new float3(boundsMax.x, boundsMax.y, boundsMid.z), depth + 1, s2, s3, childIndex + 2, ref firstFreeChild);
						BuildNode(new float3(boundsMid.x, boundsMin.y, boundsMid.z), new float3(boundsMax.x, boundsMax.y, boundsMax.z), depth + 1, s3, s4, childIndex + 3, ref firstFreeChild);
					}
				} else {
					// Bitpack the start and end indices
					outChildPointers[nodeOffset] = agentsStart | (agentsEnd << BitPackingShift) | LeafNodeBit;
				}
			}

			void CalculateSpeeds (int nodeCount) {
				for (int i = nodeCount - 1; i >= 0; i--) {
					if ((outChildPointers[i] & LeafNodeBit) != 0) {
						int startIndex = outChildPointers[i] & BitPackingMask;
						int endIndex = (outChildPointers[i] >> BitPackingShift) & BitPackingMask;
						float speed = 0;
						for (int j = startIndex; j < endIndex; j++) speed = math.max(speed, agentSpeeds[agents[j]]);
						outMaxSpeeds[i] = speed;

						float radius = 0;
						for (int j = startIndex; j < endIndex; j++) radius = math.max(radius, agentRadii[agents[j]]);
						outMaxRadius[i] = radius;

						float area = 0;
						for (int j = startIndex; j < endIndex; j++) area += agentRadii[agents[j]]*agentRadii[agents[j]];
						outArea[i] = area;
					} else {
						// Take the maximum of all child speeds
						// This is guaranteed to have been calculated already because we do the loop in reverse and child indices are always greater than the current index
						int childIndex = outChildPointers[i];
						if (movementPlane == MovementPlane.Arbitrary) {
							// 8 children
							float maxSpeed = 0;
							float maxRadius = 0;
							float area = 0;
							for (int j = 0; j < 8; j++) {
								maxSpeed = math.max(maxSpeed, outMaxSpeeds[childIndex + j]);
								maxRadius = math.max(maxRadius, outMaxSpeeds[childIndex + j]);
								area += outArea[childIndex + j];
							}
							outMaxSpeeds[i] = maxSpeed;
							outMaxRadius[i] = maxRadius;
							outArea[i] = area;
						} else {
							// 4 children
							outMaxSpeeds[i] = math.max(math.max(outMaxSpeeds[childIndex], outMaxSpeeds[childIndex+1]), math.max(outMaxSpeeds[childIndex+2], outMaxSpeeds[childIndex+3]));
							outMaxRadius[i] = math.max(math.max(outMaxRadius[childIndex], outMaxRadius[childIndex+1]), math.max(outMaxRadius[childIndex+2], outMaxRadius[childIndex+3]));

							// Sum of child areas
							outArea[i] = outArea[childIndex] + outArea[childIndex+1] + outArea[childIndex+2] + outArea[childIndex+3];
						}
					}
				}
			}

			public void Execute () {
				float3 mn = float.PositiveInfinity;
				float3 mx = float.NegativeInfinity;
				int existingAgentCount = 0;
				for (int i = 0; i < numAgents; i++) {
					if (agentVersions[i].Valid) {
						agents[existingAgentCount++] = i;
						mn = math.min(mn, agentPositions[i]);
						mx = math.max(mx, agentPositions[i]);
					}
				}

				outAgentCount[0] = existingAgentCount;

				if (existingAgentCount == 0) {
					outBoundingBox[0] = outBoundingBox[1] = float3.zero;
					return;
				}

				outBoundingBox[0] = mn;
				outBoundingBox[1] = mx;

				int firstFreeChild = 1;
				BuildNode(mn, mx, 0, 0, existingAgentCount, 0, ref firstFreeChild);

				CalculateSpeeds(firstFreeChild);

				NativeArray<float3>.Copy(agentPositions, outAgentPositions, numAgents);
				NativeArray<float>.Copy(agentRadii, outAgentRadii, numAgents);
			}
		}

		public struct QuadtreeQuery {
			public float3 position;
			public float speed, timeHorizon, agentRadius;
			public int outputStartIndex, maxCount;
			public RVOLayer layerMask;
			public NativeArray<RVOLayer> layers;
			public NativeArray<int> result;
			public NativeArray<float> resultDistances;
		}

		/// <summary>
		/// A very large distance. Used as a sentinel value in the QueryKNearest method.
		/// We don't use actual infinity, because the code may be compiled using FastMath, which makes the compiler assume that infinities do not exist.
		/// This should be much larger than any distance used in practice.
		/// </summary>
		const float DistanceInfinity = 1e30f;

		public int QueryKNearest (QuadtreeQuery query) {
			if (!agents.IsCreated) return 0;
			float maxRadius = DistanceInfinity;

			for (int i = 0; i < query.maxCount; i++) query.result[query.outputStartIndex + i] = -1;
			for (int i = 0; i < query.maxCount; i++) query.resultDistances[i] = DistanceInfinity;

			QueryRec(ref query, 0, boundingBoxBuffer[0], boundingBoxBuffer[1], ref maxRadius);

			int numFound = 0;
			while (numFound < query.maxCount && query.resultDistances[numFound] < DistanceInfinity) numFound++;
			return numFound;
		}

		void QueryRec (ref QuadtreeQuery query, int treeNodeIndex, float3 nodeMin, float3 nodeMax, ref float maxRadius) {
			// Note: the second agentRadius usage should actually be the radius of the other agents, not this agent
			// Determine the radius that we need to search to take all agents into account
			// but for performance reasons and for simplicity we assume that agents have approximately the same radius.
			// Thus an agent with a very small radius may in some cases detect an agent with a very large radius too late
			// however this effect should be minor.
			var radius = math.min(math.max((maxSpeeds[treeNodeIndex] + query.speed)*query.timeHorizon, query.agentRadius) + query.agentRadius, maxRadius);
			float3 p = query.position;

			if ((childPointers[treeNodeIndex] & LeafNodeBit) != 0) {
				// Leaf node
				int maxCount = query.maxCount;
				int startIndex = childPointers[treeNodeIndex] & BitPackingMask;
				int endIndex = (childPointers[treeNodeIndex] >> BitPackingShift) & BitPackingMask;

				var result = query.result;
				var resultDistances = query.resultDistances;
				for (int j = startIndex; j < endIndex; j++) {
					var agent = agents[j];
					float sqrDistance = math.lengthsq(p - agentPositions[agent]);
					if (sqrDistance < radius*radius && (query.layers[agent] & query.layerMask) != 0) {
						// Close enough

						// Insert the agent into the results list using insertion sort
						for (int k = 0; k < maxCount; k++) {
							if (sqrDistance < resultDistances[k]) {
								// Move the remaining items one step in the array
								for (int q = maxCount - 1; q > k; q--) {
									result[query.outputStartIndex + q] = result[query.outputStartIndex + q-1];
									resultDistances[q] = resultDistances[q-1];
								}
								result[query.outputStartIndex + k] = agent;
								resultDistances[k] = sqrDistance;

								if (k == maxCount - 1) {
									// We reached the end of the array. This means that we just updated the largest distance.
									// We can use this to restrict the future search. We know that no other agent distance we find can be larger than this value.
									maxRadius = math.min(maxRadius, math.sqrt(sqrDistance));
									radius = math.min(radius, maxRadius);
								}
								break;
							}
						}
					}
				}
			} else {
				// Not a leaf node
				int childrenStartIndex = childPointers[treeNodeIndex];

				float3 nodeMid = (nodeMin + nodeMax) * 0.5f;
				if (movementPlane == MovementPlane.Arbitrary) {
					// First visit the child that overlaps the query position.
					// This is important to do first as it usually reduces the maxRadius significantly
					// and thus reduces the number of children we have to search later.
					var mainChildIndex = (p.x < nodeMid.x ? 0 : 4) | (p.y < nodeMid.y ? 0 : 2) | (p.z < nodeMid.z ? 0 : 1);
					{
						var selector = new bool3((mainChildIndex & 0x4) != 0, (mainChildIndex & 0x2) != 0, (mainChildIndex & 0x1) != 0);

						var mn = math.select(nodeMin, nodeMid, selector);
						var mx = math.select(nodeMid, nodeMax, selector);
						QueryRec(ref query, childrenStartIndex + mainChildIndex, mn, mx, ref maxRadius);
						radius = math.min(radius, maxRadius);
					}

					// Visit a child if a cube with sides of length 2*radius (centered at p) touches the child.
					// We calculate this info for all 8 children at the same time.
					// Each child contains three checks, one for each axis.
					// For example for the child which is lower than mid on the x-axis and z-axis, but higher than mid on the y axis
					// the check we want to do looks like: (p.x - radius < nodeMid.x && p.y + radius > nodeMid.y && p.z - radius < nodeMid.z)
					var lessThanMid = p - radius < nodeMid;
					var greaterThanMid = p + radius > nodeMid;
					// If for example lessThanMid.x is false, then we can exclude all 4 children that require that check
					var branch1 = math.select(new int3(0b11110000, 0b11001100, 0b10101010), new int3(0xFF, 0xFF, 0xFF), lessThanMid);
					var branch2 = math.select(new int3(0b00001111, 0b00110011, 0b01010101), new int3(0xFF, 0xFF, 0xFF), greaterThanMid);
					var toVisitByAxis = branch1 & branch2;
					// Combine the checks for each axis
					// Bitmask of which children we want to visit (1 = visit, 0 = don't visit)
					var childrenToVisit = toVisitByAxis.x & toVisitByAxis.y & toVisitByAxis.z;

					childrenToVisit &= ~(1 << mainChildIndex);

					// Loop over all children that we will visit.
					// It's nice with a loop because we will usually only have a single branch.
					while (childrenToVisit != 0) {
						var childIndex = ChildLookup[childrenToVisit];
						var selector = new bool3((childIndex & 0x4) != 0, (childIndex & 0x2) != 0, (childIndex & 0x1) != 0);

						var mn = math.select(nodeMin, nodeMid, selector);
						var mx = math.select(nodeMid, nodeMax, selector);
						QueryRec(ref query, childrenStartIndex + childIndex, mn, mx, ref maxRadius);
						radius = math.min(radius, maxRadius);
						childrenToVisit &= ~(1 << childIndex);
					}
				} else if (movementPlane == MovementPlane.XY) {
					var mainChildIndex = (p.x < nodeMid.x ? 0 : 2) | (p.y < nodeMid.y ? 0 : 1);
					{
						// Note: mx.z will become nodeMid.z which is technically incorrect, but we don't care about the Z coordinate here anyway
						var selector = new bool3((mainChildIndex & 0x2) != 0, (mainChildIndex & 0x1) != 0, false);

						var mn = math.select(nodeMin, nodeMid, selector);
						var mx = math.select(nodeMid, nodeMax, selector);
						QueryRec(ref query, childrenStartIndex + mainChildIndex, mn, mx, ref maxRadius);
						radius = math.min(radius, maxRadius);
					}

					var lessThanMid = p.xy - radius < nodeMid.xy;
					var greaterThanMid = p.xy + radius > nodeMid.xy;

					var v = new bool4(lessThanMid.x & lessThanMid.y, lessThanMid.x & greaterThanMid.y, greaterThanMid.x & lessThanMid.y, greaterThanMid.x & greaterThanMid.y);
					// Build a bitmask of which children to visit
					var childrenToVisit = (v.x ? 1 : 0) | (v.y ? 2 : 0) | (v.z ? 4 : 0) | (v.w ? 8 : 0);
					childrenToVisit &= ~(1 << mainChildIndex);

					// Loop over all children that we will visit.
					// It's nice with a loop because we will usually only have a single branch.
					while (childrenToVisit != 0) {
						var childIndex = ChildLookup[childrenToVisit];
						// Note: mx.z will become nodeMid.z which is technically incorrect, but we don't care about the Z coordinate here anyway
						var selector = new bool3((childIndex & 0x2) != 0, (childIndex & 0x1) != 0, false);

						var mn = math.select(nodeMin, nodeMid, selector);
						var mx = math.select(nodeMid, nodeMax, selector);
						QueryRec(ref query, childrenStartIndex + childIndex, mn, mx, ref maxRadius);
						radius = math.min(radius, maxRadius);
						childrenToVisit &= ~(1 << childIndex);
					}
				} else {
					var mainChildIndex = (p.x < nodeMid.x ? 0 : 2) | (p.z < nodeMid.z ? 0 : 1);
					{
						// Note: mx.y will become nodeMid.y which is technically incorrect, but we don't care about the Y coordinate here anyway
						var selector = new bool3((mainChildIndex & 0x2) != 0, false, (mainChildIndex & 0x1) != 0);

						var mn = math.select(nodeMin, nodeMid, selector);
						var mx = math.select(nodeMid, nodeMax, selector);
						QueryRec(ref query, childrenStartIndex + mainChildIndex, mn, mx, ref maxRadius);
						radius = math.min(radius, maxRadius);
					}

					var lessThanMid = p.xz - radius < nodeMid.xz;
					var greaterThanMid = p.xz + radius > nodeMid.xz;

					var v = new bool4(lessThanMid.x & lessThanMid.y, lessThanMid.x & greaterThanMid.y, greaterThanMid.x & lessThanMid.y, greaterThanMid.x & greaterThanMid.y);
					var childrenToVisit = (v.x ? 1 : 0) | (v.y ? 2 : 0) | (v.z ? 4 : 0) | (v.w ? 8 : 0);
					childrenToVisit &= ~(1 << mainChildIndex);

					while (childrenToVisit != 0) {
						var childIndex = ChildLookup[childrenToVisit];
						// Note: mx.y will become nodeMid.y which is technically incorrect, but we don't care about the Y coordinate here anyway
						var selector = new bool3((childIndex & 0x2) != 0, false, (childIndex & 0x1) != 0);

						var mn = math.select(nodeMin, nodeMid, selector);
						var mx = math.select(nodeMid, nodeMax, selector);
						QueryRec(ref query, childrenStartIndex + childIndex, mn, mx, ref maxRadius);
						radius = math.min(radius, maxRadius);
						childrenToVisit &= ~(1 << childIndex);
					}
				}
			}
		}

		/// <summary>Find the total agent area inside the circle at position with the given radius</summary>
		public float QueryArea (float3 position, float radius) {
			if (!agents.IsCreated || agentCountBuffer[0] == 0) return 0f;
			return math.PI * QueryAreaRec(0, position, radius, boundingBoxBuffer[0], boundingBoxBuffer[1]);
		}

		float QueryAreaRec (int treeNodeIndex, float3 p, float radius, float3 nodeMin, float3 nodeMax) {
			float3 nodeMid = (nodeMin + nodeMax) * 0.5f;
			// Radius of a circle that is guaranteed to cover the entire node
			float nodeRadius = math.length(nodeMax - nodeMid);
			float dist = math.lengthsq(nodeMid - p);
			var maxAgentRadius = maxRadius[treeNodeIndex];
			var thresholdDistance = radius - (nodeRadius + maxAgentRadius);

			if (thresholdDistance > 0 && dist < thresholdDistance*thresholdDistance) {
				// Node is completely inside the circle. Return the precalculated area of all agents inside the node.
				return nodeAreas[treeNodeIndex];
			}

			if (dist > (radius + (nodeRadius + maxAgentRadius))*(radius + (nodeRadius + maxAgentRadius))) {
				return 0;
			}

			if ((childPointers[treeNodeIndex] & LeafNodeBit) != 0) {
				// Leaf node
				// Node is partially inside the circle

				int startIndex = childPointers[treeNodeIndex] & BitPackingMask;
				int endIndex = (childPointers[treeNodeIndex] >> BitPackingShift) & BitPackingMask;

				float k = 0;
				float area = 0;
				for (int j = startIndex; j < endIndex; j++) {
					var agent = agents[j];
					k += agentRadii[agent]*agentRadii[agent];
					float sqrDistance = math.lengthsq(p - agentPositions[agent]);
					float agentRadius = agentRadii[agent];
					if (sqrDistance < (radius + agentRadius)*(radius + agentRadius)) {
						float innerRadius = radius - agentRadius;
						// Slight approximation at the edge of the circle.
						// This is the approximate fraction of the agent that is inside the circle.
						float fractionInside = sqrDistance < innerRadius*innerRadius ? 1.0f : 1.0f - (math.sqrt(sqrDistance) - innerRadius) / (2*agentRadius);
						area += agentRadius*agentRadius * fractionInside;
					}
				}
				return area;
			} else {
				float area = 0;
				// Not a leaf node
				int childIndex = childPointers[treeNodeIndex];
				float radiusWithMargin = radius + maxAgentRadius;
				if (movementPlane == MovementPlane.Arbitrary) {
					bool3 lower = (p - radiusWithMargin) < nodeMid;
					bool3 upper = (p + radiusWithMargin) > nodeMid;
					if (lower[0]) {
						if (lower[1]) {
							if (lower[2]) area += QueryAreaRec(childIndex + 0, p, radius, new float3(nodeMin.x, nodeMin.y, nodeMin.z), new float3(nodeMid.x, nodeMid.y, nodeMid.z));
							if (upper[2]) area += QueryAreaRec(childIndex + 1, p, radius, new float3(nodeMin.x, nodeMin.y, nodeMid.z), new float3(nodeMid.x, nodeMid.y, nodeMax.z));
						}
						if (upper[1]) {
							if (lower[2]) area += QueryAreaRec(childIndex + 2, p, radius, new float3(nodeMin.x, nodeMid.y, nodeMin.z), new float3(nodeMid.x, nodeMax.y, nodeMid.z));
							if (upper[2]) area += QueryAreaRec(childIndex + 3, p, radius, new float3(nodeMin.x, nodeMid.y, nodeMid.z), new float3(nodeMid.x, nodeMax.y, nodeMax.z));
						}
					}
					if (upper[0]) {
						if (lower[1]) {
							if (lower[2]) area += QueryAreaRec(childIndex + 4, p, radius, new float3(nodeMid.x, nodeMin.y, nodeMin.z), new float3(nodeMax.x, nodeMid.y, nodeMid.z));
							if (upper[2]) area += QueryAreaRec(childIndex + 5, p, radius, new float3(nodeMid.x, nodeMin.y, nodeMid.z), new float3(nodeMax.x, nodeMid.y, nodeMax.z));
						}
						if (upper[1]) {
							if (lower[2]) area += QueryAreaRec(childIndex + 6, p, radius, new float3(nodeMid.x, nodeMid.y, nodeMin.z), new float3(nodeMax.x, nodeMax.y, nodeMid.z));
							if (upper[2]) area += QueryAreaRec(childIndex + 7, p, radius, new float3(nodeMid.x, nodeMid.y, nodeMid.z), new float3(nodeMax.x, nodeMax.y, nodeMax.z));
						}
					}
				} else if (movementPlane == MovementPlane.XY) {
					bool2 lower = (p - radiusWithMargin).xy < nodeMid.xy;
					bool2 upper = (p + radiusWithMargin).xy > nodeMid.xy;
					if (lower[0]) {
						if (lower[1]) area += QueryAreaRec(childIndex + 0, p, radius, new float3(nodeMin.x, nodeMin.y, nodeMin.z), new float3(nodeMid.x, nodeMid.y, nodeMax.z));
						if (upper[1]) area += QueryAreaRec(childIndex + 1, p, radius, new float3(nodeMin.x, nodeMid.y, nodeMin.z), new float3(nodeMid.x, nodeMax.y, nodeMax.z));
					}
					if (upper[0]) {
						if (lower[1]) area += QueryAreaRec(childIndex + 2, p, radius, new float3(nodeMid.x, nodeMin.y, nodeMin.z), new float3(nodeMax.x, nodeMid.y, nodeMax.z));
						if (upper[1]) area += QueryAreaRec(childIndex + 3, p, radius, new float3(nodeMid.x, nodeMid.y, nodeMin.z), new float3(nodeMax.x, nodeMax.y, nodeMax.z));
					}
				} else {
					bool2 lower = (p - radiusWithMargin).xz < nodeMid.xz;
					bool2 upper = (p + radiusWithMargin).xz > nodeMid.xz;
					if (lower[0]) {
						if (lower[1]) area += QueryAreaRec(childIndex + 0, p, radius, new float3(nodeMin.x, nodeMin.y, nodeMin.z), new float3(nodeMid.x, nodeMax.y, nodeMid.z));
						if (upper[1]) area += QueryAreaRec(childIndex + 1, p, radius, new float3(nodeMin.x, nodeMin.y, nodeMid.z), new float3(nodeMid.x, nodeMax.y, nodeMax.z));
					}
					if (upper[0]) {
						if (lower[1]) area += QueryAreaRec(childIndex + 2, p, radius, new float3(nodeMid.x, nodeMin.y, nodeMin.z), new float3(nodeMax.x, nodeMax.y, nodeMid.z));
						if (upper[1]) area += QueryAreaRec(childIndex + 3, p, radius, new float3(nodeMid.x, nodeMin.y, nodeMid.z), new float3(nodeMax.x, nodeMax.y, nodeMax.z));
					}
				}
				return area;
			}
		}

		[BurstCompile]
		public struct DebugDrawJob : IJob {
			public CommandBuilder draw;
			[ReadOnly]
			public RVOQuadtreeBurst quadtree;

			public void Execute () {
				quadtree.DebugDraw(draw);
			}
		}

		public void DebugDraw (CommandBuilder draw) {
			if (!agentCountBuffer.IsCreated) return;
			var numAgents = agentCountBuffer[0];
			if (numAgents == 0) return;

			DebugDraw(0, boundingBoxBuffer[0], boundingBoxBuffer[1], draw);
			for (int i = 0; i < numAgents; i++) {
				draw.Cross(agentPositions[agents[i]], 0.5f, Palette.Colorbrewer.Set1.Red);
			}
		}

		void DebugDraw (int nodeIndex, float3 nodeMin, float3 nodeMax, CommandBuilder draw) {
			float3 nodeMid = (nodeMin + nodeMax) * 0.5f;

			draw.WireBox(nodeMid, nodeMax - nodeMin, Palette.Colorbrewer.Set1.Orange);

			if ((childPointers[nodeIndex] & LeafNodeBit) != 0) {
				int startIndex = childPointers[nodeIndex] & BitPackingMask;
				int endIndex = (childPointers[nodeIndex] >> BitPackingShift) & BitPackingMask;

				for (int j = startIndex; j < endIndex; j++) {
					draw.Line(nodeMid, agentPositions[agents[j]], Color.black);
				}
			} else {
				int childIndex = childPointers[nodeIndex];
				if (movementPlane == MovementPlane.Arbitrary) {
					DebugDraw(childIndex + 0, new float3(nodeMin.x, nodeMin.y, nodeMin.z), new float3(nodeMid.x, nodeMid.y, nodeMid.z), draw);
					DebugDraw(childIndex + 1, new float3(nodeMin.x, nodeMin.y, nodeMid.z), new float3(nodeMid.x, nodeMid.y, nodeMax.z), draw);
					DebugDraw(childIndex + 2, new float3(nodeMin.x, nodeMid.y, nodeMin.z), new float3(nodeMid.x, nodeMax.y, nodeMid.z), draw);
					DebugDraw(childIndex + 3, new float3(nodeMin.x, nodeMid.y, nodeMid.z), new float3(nodeMid.x, nodeMax.y, nodeMax.z), draw);
					DebugDraw(childIndex + 4, new float3(nodeMid.x, nodeMin.y, nodeMin.z), new float3(nodeMax.x, nodeMid.y, nodeMid.z), draw);
					DebugDraw(childIndex + 5, new float3(nodeMid.x, nodeMin.y, nodeMid.z), new float3(nodeMax.x, nodeMid.y, nodeMax.z), draw);
					DebugDraw(childIndex + 6, new float3(nodeMid.x, nodeMid.y, nodeMin.z), new float3(nodeMax.x, nodeMax.y, nodeMid.z), draw);
					DebugDraw(childIndex + 7, new float3(nodeMid.x, nodeMid.y, nodeMid.z), new float3(nodeMax.x, nodeMax.y, nodeMax.z), draw);
				} else if (movementPlane == MovementPlane.XY) {
					DebugDraw(childIndex + 0, new float3(nodeMin.x, nodeMin.y, nodeMin.z), new float3(nodeMid.x, nodeMid.y, nodeMax.z), draw);
					DebugDraw(childIndex + 1, new float3(nodeMin.x, nodeMid.y, nodeMin.z), new float3(nodeMid.x, nodeMax.y, nodeMax.z), draw);
					DebugDraw(childIndex + 2, new float3(nodeMid.x, nodeMin.y, nodeMin.z), new float3(nodeMax.x, nodeMid.y, nodeMax.z), draw);
					DebugDraw(childIndex + 3, new float3(nodeMid.x, nodeMid.y, nodeMin.z), new float3(nodeMax.x, nodeMax.y, nodeMax.z), draw);
				} else {
					DebugDraw(childIndex + 0, new float3(nodeMin.x, nodeMin.y, nodeMin.z), new float3(nodeMid.x, nodeMax.y, nodeMid.z), draw);
					DebugDraw(childIndex + 1, new float3(nodeMin.x, nodeMin.y, nodeMid.z), new float3(nodeMid.x, nodeMax.y, nodeMax.z), draw);
					DebugDraw(childIndex + 2, new float3(nodeMid.x, nodeMin.y, nodeMin.z), new float3(nodeMax.x, nodeMax.y, nodeMid.z), draw);
					DebugDraw(childIndex + 3, new float3(nodeMid.x, nodeMin.y, nodeMid.z), new float3(nodeMax.x, nodeMax.y, nodeMax.z), draw);
				}
			}
		}
	}
}
