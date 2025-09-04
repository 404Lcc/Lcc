using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Pathfinding.Util;
using UnityEngine.Profiling;
using System.Collections.Generic;
using Pathfinding.Jobs;
using Pathfinding.Graphs.Grid.Jobs;
using Pathfinding.Collections;
using Unity.Jobs.LowLevel.Unsafe;

namespace Pathfinding.Graphs.Grid {
	public struct GridGraphNodeData {
		public Allocator allocationMethod;
		public int numNodes;
		/// <summary>
		/// Bounds for the part of the graph that this data represents.
		/// For example if the first layer of a layered grid graph is being updated between x=10 and x=20, z=5 and z=15
		/// then this will be IntBounds(xmin=10, ymin=0, zmin=5, xmax=20, ymax=0, zmax=15)
		/// </summary>
		public IntBounds bounds;
		/// <summary>
		/// Number of layers that the data contains.
		/// For a non-layered grid graph this will always be 1.
		/// </summary>
		public int layers => bounds.size.y;

		/// <summary>
		/// Positions of all nodes.
		///
		/// Data is valid in these passes:
		/// - BeforeCollision: Valid
		/// - BeforeConnections: Valid
		/// - AfterConnections: Valid
		/// - AfterErosion: Valid
		/// - PostProcess: Valid
		/// </summary>
		public NativeArray<Vector3> positions;

		/// <summary>
		/// Bitpacked connections of all nodes.
		///
		/// Connections are stored in different formats depending on <see cref="layeredDataLayout"/>.
		/// You can use <see cref="LayeredGridAdjacencyMapper"/> and <see cref="FlatGridAdjacencyMapper"/> to access connections for the different data layouts.
		///
		/// Data is valid in these passes:
		/// - BeforeCollision: Invalid
		/// - BeforeConnections: Invalid
		/// - AfterConnections: Valid
		/// - AfterErosion: Valid (but will be overwritten)
		/// - PostProcess: Valid
		/// </summary>
		public NativeArray<ulong> connections;

		/// <summary>
		/// Bitpacked connections of all nodes.
		///
		/// Data is valid in these passes:
		/// - BeforeCollision: Valid
		/// - BeforeConnections: Valid
		/// - AfterConnections: Valid
		/// - AfterErosion: Valid
		/// - PostProcess: Valid
		/// </summary>
		public NativeArray<uint> penalties;

		/// <summary>
		/// Tags of all nodes
		///
		/// Data is valid in these passes:
		/// - BeforeCollision: Valid (but if erosion uses tags then it will be overwritten later)
		/// - BeforeConnections: Valid (but if erosion uses tags then it will be overwritten later)
		/// - AfterConnections: Valid (but if erosion uses tags then it will be overwritten later)
		/// - AfterErosion: Valid
		/// - PostProcess: Valid
		/// </summary>
		public NativeArray<int> tags;

		/// <summary>
		/// Normals of all nodes.
		/// If height testing is disabled the normal will be (0,1,0) for all nodes.
		/// If a node doesn't exist (only happens in layered grid graphs) or if the height raycast didn't hit anything then the normal will be (0,0,0).
		///
		/// Data is valid in these passes:
		/// - BeforeCollision: Valid
		/// - BeforeConnections: Valid
		/// - AfterConnections: Valid
		/// - AfterErosion: Valid
		/// - PostProcess: Valid
		/// </summary>
		public NativeArray<float4> normals;

		/// <summary>
		/// Walkability of all nodes before erosion happens.
		///
		/// Data is valid in these passes:
		/// - BeforeCollision: Valid (it will be combined with collision testing later)
		/// - BeforeConnections: Valid
		/// - AfterConnections: Valid
		/// - AfterErosion: Valid
		/// - PostProcess: Valid
		/// </summary>
		public NativeArray<bool> walkable;

		/// <summary>
		/// Walkability of all nodes after erosion happens. This is the final walkability of the nodes.
		/// If no erosion is used then the data will just be copied from the <see cref="walkable"/> array.
		///
		/// Data is valid in these passes:
		/// - BeforeCollision: Invalid
		/// - BeforeConnections: Invalid
		/// - AfterConnections: Invalid
		/// - AfterErosion: Valid
		/// - PostProcess: Valid
		/// </summary>
		public NativeArray<bool> walkableWithErosion;


		/// <summary>
		/// True if the data may have multiple layers.
		/// For layered data the nodes are laid out as `data[y*width*depth + z*width + x]`.
		/// For non-layered data the nodes are laid out as `data[z*width + x]` (which is equivalent to the above layout assuming y=0).
		///
		/// This also affects how node connections are stored. You can use <see cref="LayeredGridAdjacencyMapper"/> and <see cref="FlatGridAdjacencyMapper"/> to access
		/// connections for the different data layouts.
		/// </summary>
		public bool layeredDataLayout;

		public void AllocateBuffers (JobDependencyTracker dependencyTracker) {
			Profiler.BeginSample("Allocating buffers");
			// Allocate buffers for jobs
			// Allocating buffers with uninitialized memory is much faster if no jobs assume anything about their contents
			if (dependencyTracker != null) {
				positions = dependencyTracker.NewNativeArray<Vector3>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				normals = dependencyTracker.NewNativeArray<float4>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				connections = dependencyTracker.NewNativeArray<ulong>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				penalties = dependencyTracker.NewNativeArray<uint>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				walkable = dependencyTracker.NewNativeArray<bool>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				walkableWithErosion = dependencyTracker.NewNativeArray<bool>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				tags = dependencyTracker.NewNativeArray<int>(numNodes, allocationMethod, NativeArrayOptions.ClearMemory);
			} else {
				positions = new NativeArray<Vector3>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				normals = new NativeArray<float4>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				connections = new NativeArray<ulong>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				penalties = new NativeArray<uint>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				walkable = new NativeArray<bool>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				walkableWithErosion = new NativeArray<bool>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				tags = new NativeArray<int>(numNodes, allocationMethod, NativeArrayOptions.ClearMemory);
			}
			Profiler.EndSample();
		}

		public void TrackBuffers (JobDependencyTracker dependencyTracker) {
			if (positions.IsCreated) dependencyTracker.Track(positions);
			if (normals.IsCreated) dependencyTracker.Track(normals);
			if (connections.IsCreated) dependencyTracker.Track(connections);
			if (penalties.IsCreated) dependencyTracker.Track(penalties);
			if (walkable.IsCreated) dependencyTracker.Track(walkable);
			if (walkableWithErosion.IsCreated) dependencyTracker.Track(walkableWithErosion);
			if (tags.IsCreated) dependencyTracker.Track(tags);
		}

		public void PersistBuffers (JobDependencyTracker dependencyTracker) {
			dependencyTracker.Persist(positions);
			dependencyTracker.Persist(normals);
			dependencyTracker.Persist(connections);
			dependencyTracker.Persist(penalties);
			dependencyTracker.Persist(walkable);
			dependencyTracker.Persist(walkableWithErosion);
			dependencyTracker.Persist(tags);
		}

		public void Dispose () {
			bounds = default;
			numNodes = 0;
			if (positions.IsCreated) positions.Dispose();
			if (normals.IsCreated) normals.Dispose();
			if (connections.IsCreated) connections.Dispose();
			if (penalties.IsCreated) penalties.Dispose();
			if (walkable.IsCreated) walkable.Dispose();
			if (walkableWithErosion.IsCreated) walkableWithErosion.Dispose();
			if (tags.IsCreated) tags.Dispose();
		}

		public JobHandle Rotate2D (int dx, int dz, JobHandle dependency) {
			var size = bounds.size;
			unsafe {
				var jobs = stackalloc JobHandle[7];
				jobs[0] = positions.Rotate3D(size, dx, dz).Schedule(dependency);
				jobs[1] = normals.Rotate3D(size, dx, dz).Schedule(dependency);
				jobs[2] = connections.Rotate3D(size, dx, dz).Schedule(dependency);
				jobs[3] = penalties.Rotate3D(size, dx, dz).Schedule(dependency);
				jobs[4] = walkable.Rotate3D(size, dx, dz).Schedule(dependency);
				jobs[5] = walkableWithErosion.Rotate3D(size, dx, dz).Schedule(dependency);
				jobs[6] = tags.Rotate3D(size, dx, dz).Schedule(dependency);
				return JobHandleUnsafeUtility.CombineDependencies(jobs, 7);
			}
		}

		public void ResizeLayerCount (int layerCount, JobDependencyTracker dependencyTracker) {
			if (layerCount > layers) {
				var oldData = this;
				this.bounds.max.y = layerCount;
				this.numNodes = bounds.volume;
				this.AllocateBuffers(dependencyTracker);
				// Ensure the normals for the upper layers are zeroed out.
				// All other node data in the upper layers can be left uninitialized.
				this.normals.MemSet(float4.zero).Schedule(dependencyTracker);
				this.walkable.MemSet(false).Schedule(dependencyTracker);
				this.walkableWithErosion.MemSet(false).Schedule(dependencyTracker);
				new JobCopyBuffers {
					input = oldData,
					output = this,
					copyPenaltyAndTags = true,
					bounds = oldData.bounds,
				}.Schedule(dependencyTracker);
			}
			if (layerCount < layers) {
				throw new System.ArgumentException("Cannot reduce the number of layers");
			}
		}

		struct LightReader : GridIterationUtilities.ISliceAction {
			public GridNodeBase[] nodes;
			public UnsafeSpan<Vector3> nodePositions;
			public UnsafeSpan<bool> nodeWalkable;

			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			public void Execute (uint outerIdx, uint innerIdx) {
				// The data bounds may have more layers than the existing nodes if a new layer is being added.
				// We can only copy from the nodes that exist.
				if (outerIdx < nodes.Length) {
					var node = nodes[outerIdx];
					if (node != null) {
						nodePositions[innerIdx] = (Vector3)node.position;
						nodeWalkable[innerIdx] = node.Walkable;
						return;
					}
				}

				// Fallback in case the node was null (only happens for layered grid graphs),
				// or if we are adding more layers to the graph, in which case we are outside
				// the bounds of the nodes array.
				nodePositions[innerIdx] = Vector3.zero;
				nodeWalkable[innerIdx] = false;
			}
		}

		public void ReadFromNodesForConnectionCalculations (GridNodeBase[] nodes, Slice3D slice, JobHandle nodesDependsOn, NativeArray<float4> graphNodeNormals, JobDependencyTracker dependencyTracker) {
			bounds = slice.slice;
			numNodes = slice.slice.volume;

			Profiler.BeginSample("Allocating buffers");
			positions = new NativeArray<Vector3>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
			normals = new NativeArray<float4>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
			connections = new NativeArray<ulong>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
			walkableWithErosion = new NativeArray<bool>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
			Profiler.EndSample();

			Profiler.BeginSample("Reading node data");
			var reader = new LightReader {
				nodes = nodes,
				nodePositions = this.positions.AsUnsafeSpan(),
				nodeWalkable = this.walkableWithErosion.AsUnsafeSpan(),
			};
			GridIterationUtilities.ForEachCellIn3DSlice(slice, ref reader);
			Profiler.EndSample();

			ReadNodeNormals(slice, graphNodeNormals, dependencyTracker);
		}

		void ReadNodeNormals (Slice3D slice, NativeArray<float4> graphNodeNormals, JobDependencyTracker dependencyTracker) {
			UnityEngine.Assertions.Assert.IsTrue(graphNodeNormals.IsCreated);
			// Read the normal data from the graphNodeNormals array and copy it to the nodeNormals array.
			// The nodeArrayBounds may have fewer layers than the readBounds if layers are being added.
			// This means we can copy only a subset of the normals.
			// We MemSet the array to zero first to avoid any uninitialized data remaining.
			// TODO: Do clamping in caller
			//var clampedReadBounds = new IntBounds(readBounds.min, new int3(readBounds.max.x, math.min(nodeArrayBounds.y, readBounds.max.y), readBounds.max.z));
			if (dependencyTracker != null) {
				normals.MemSet(float4.zero).Schedule(dependencyTracker);
				new JobCopyRectangle<float4> {
					input = graphNodeNormals,
					output = normals,
					inputSlice = slice,
					outputSlice = new Slice3D(bounds, slice.slice),
				}.Schedule(dependencyTracker);
			} else {
				Profiler.BeginSample("ReadNodeNormals");
				normals.AsUnsafeSpan().FillZeros();
				JobCopyRectangle<float4>.Copy(graphNodeNormals, normals, slice, new Slice3D(bounds, slice.slice));
				Profiler.EndSample();
			}
		}

		public static GridGraphNodeData ReadFromNodes (GridNodeBase[] nodes, Slice3D slice, JobHandle nodesDependsOn, NativeArray<float4> graphNodeNormals, Allocator allocator, bool layeredDataLayout, JobDependencyTracker dependencyTracker) {
			var nodeData = new GridGraphNodeData {
				allocationMethod = allocator,
				numNodes = slice.slice.volume,
				bounds = slice.slice,
				layeredDataLayout = layeredDataLayout,
			};
			nodeData.AllocateBuffers(dependencyTracker);

			// This is a managed type, we need to trick Unity to allow this inside of a job
			var nodesHandle = System.Runtime.InteropServices.GCHandle.Alloc(nodes);

			var job = new JobReadNodeData {
				nodesHandle = nodesHandle,
				nodePositions = nodeData.positions,
				nodePenalties = nodeData.penalties,
				nodeTags = nodeData.tags,
				nodeConnections = nodeData.connections,
				nodeWalkableWithErosion = nodeData.walkableWithErosion,
				nodeWalkable = nodeData.walkable,
				slice = slice,
			}.ScheduleBatch(nodeData.numNodes, math.max(2000, nodeData.numNodes/16), dependencyTracker, nodesDependsOn);

			dependencyTracker.DeferFree(nodesHandle, job);

			if (graphNodeNormals.IsCreated) nodeData.ReadNodeNormals(slice, graphNodeNormals, dependencyTracker);
			return nodeData;
		}

		public GridGraphNodeData ReadFromNodesAndCopy (GridNodeBase[] nodes, Slice3D slice, JobHandle nodesDependsOn, NativeArray<float4> graphNodeNormals, bool copyPenaltyAndTags, JobDependencyTracker dependencyTracker) {
			var newData = GridGraphNodeData.ReadFromNodes(nodes, slice, nodesDependsOn, graphNodeNormals, allocationMethod, layeredDataLayout, dependencyTracker);
			// Overwrite a rectangle in the center with the data from this object.
			// In the end we will have newly calculated data in the middle and data read from nodes along the borders
			newData.CopyFrom(this, copyPenaltyAndTags, dependencyTracker);
			return newData;
		}

		public void CopyFrom(GridGraphNodeData other, bool copyPenaltyAndTags, JobDependencyTracker dependencyTracker) => CopyFrom(other, IntBounds.Intersection(bounds, other.bounds), copyPenaltyAndTags, dependencyTracker);

		public void CopyFrom (GridGraphNodeData other, IntBounds bounds, bool copyPenaltyAndTags, JobDependencyTracker dependencyTracker) {
			var job = new JobCopyBuffers {
				input = other,
				output = this,
				copyPenaltyAndTags = copyPenaltyAndTags,
				bounds = bounds,
			};
			if (dependencyTracker != null) {
				job.Schedule(dependencyTracker);
			} else {
#if UNITY_2022_2_OR_NEWER
				job.RunByRef();
#else
				job.Run();
#endif
			}
		}

		public JobHandle AssignToNodes (GridNodeBase[] nodes, int3 nodeArrayBounds, IntBounds writeMask, uint graphIndex, JobHandle nodesDependsOn, JobDependencyTracker dependencyTracker) {
			// This is a managed type, we need to trick Unity to allow this inside of a job
			var nodesHandle = System.Runtime.InteropServices.GCHandle.Alloc(nodes);

			// Assign the data to the nodes (in parallel for performance)
			// This will also dirty all nodes, but that is a thread-safe operation.
			var job2 = new JobWriteNodeData {
				nodesHandle = nodesHandle,
				graphIndex = graphIndex,
				nodePositions = positions,
				nodePenalties = penalties,
				nodeTags = tags,
				nodeConnections = connections,
				nodeWalkableWithErosion = walkableWithErosion,
				nodeWalkable = walkable,
				nodeArrayBounds = nodeArrayBounds,
				dataBounds = bounds,
				writeMask = writeMask,
			}.ScheduleBatch(writeMask.volume, math.max(1000, writeMask.volume/16), dependencyTracker, nodesDependsOn);

			dependencyTracker.DeferFree(nodesHandle, job2);
			return job2;
		}
	}

	public struct GridGraphScanData {
		/// <summary>
		/// Tracks dependencies between jobs to allow parallelism without tediously specifying dependencies manually.
		/// Always use when scheduling jobs.
		/// </summary>
		public JobDependencyTracker dependencyTracker;

		/// <summary>The up direction of the graph, in world space</summary>
		public Vector3 up;

		/// <summary>Transforms graph-space to world space</summary>
		public GraphTransform transform;

		/// <summary>Data for all nodes in the graph update that is being calculated</summary>
		public GridGraphNodeData nodes;

		/// <summary>
		/// Bounds of the data arrays.
		/// Deprecated: Use nodes.bounds or heightHitsBounds depending on if you are using the heightHits array or not
		/// </summary>
		[System.Obsolete("Use nodes.bounds or heightHitsBounds depending on if you are using the heightHits array or not")]
		public IntBounds bounds => nodes.bounds;

		/// <summary>
		/// True if the data may have multiple layers.
		/// For layered data the nodes are laid out as `data[y*width*depth + z*width + x]`.
		/// For non-layered data the nodes are laid out as `data[z*width + x]` (which is equivalent to the above layout assuming y=0).
		///
		/// Deprecated: Use nodes.layeredDataLayout instead
		/// </summary>
		[System.Obsolete("Use nodes.layeredDataLayout instead")]
		public bool layeredDataLayout => nodes.layeredDataLayout;

		/// <summary>
		/// Raycasts hits used for height testing.
		/// This data is only valid if height testing is enabled, otherwise the array is uninitialized (heightHits.IsCreated will be false).
		///
		/// Data is valid in these passes:
		/// - BeforeCollision: Valid (if height testing is enabled)
		/// - BeforeConnections: Valid (if height testing is enabled)
		/// - AfterConnections: Valid (if height testing is enabled)
		/// - AfterErosion: Valid (if height testing is enabled)
		/// - PostProcess: Valid (if height testing is enabled)
		///
		/// Warning: This array does not have the same size as the arrays in <see cref="nodes"/>. It will usually be slightly smaller. See <see cref="heightHitsBounds"/>.
		/// </summary>
		public NativeArray<RaycastHit> heightHits;

		/// <summary>
		/// Bounds for the <see cref="heightHits"/> array.
		///
		/// During an update, the scan data may contain more nodes than we are doing height testing for.
		/// For a few nodes around the update, the data will be read from the existing graph, instead. This is done for performance.
		/// This means that there may not be any height testing information these nodes.
		/// However, all nodes that will be written to will always have height testing information.
		/// </summary>
		public IntBounds heightHitsBounds;

		/// <summary>
		/// Node positions.
		/// Deprecated: Use <see cref="nodes.positions"/> instead
		/// </summary>
		[System.Obsolete("Use nodes.positions instead")]
		public NativeArray<Vector3> nodePositions => nodes.positions;

		/// <summary>
		/// Node connections.
		/// Deprecated: Use <see cref="nodes.connections"/> instead
		/// </summary>
		[System.Obsolete("Use nodes.connections instead")]
		public NativeArray<ulong> nodeConnections => nodes.connections;

		/// <summary>
		/// Node penalties.
		/// Deprecated: Use <see cref="nodes.penalties"/> instead
		/// </summary>
		[System.Obsolete("Use nodes.penalties instead")]
		public NativeArray<uint> nodePenalties => nodes.penalties;

		/// <summary>
		/// Node tags.
		/// Deprecated: Use <see cref="nodes.tags"/> instead
		/// </summary>
		[System.Obsolete("Use nodes.tags instead")]
		public NativeArray<int> nodeTags => nodes.tags;

		/// <summary>
		/// Node normals.
		/// Deprecated: Use <see cref="nodes.normals"/> instead
		/// </summary>
		[System.Obsolete("Use nodes.normals instead")]
		public NativeArray<float4> nodeNormals => nodes.normals;

		/// <summary>
		/// Node walkability.
		/// Deprecated: Use <see cref="nodes.walkable"/> instead
		/// </summary>
		[System.Obsolete("Use nodes.walkable instead")]
		public NativeArray<bool> nodeWalkable => nodes.walkable;

		/// <summary>
		/// Node walkability with erosion.
		/// Deprecated: Use <see cref="nodes.walkableWithErosion"/> instead
		/// </summary>
		[System.Obsolete("Use nodes.walkableWithErosion instead")]
		public NativeArray<bool> nodeWalkableWithErosion => nodes.walkableWithErosion;

		public void SetDefaultPenalties (uint initialPenalty) {
			nodes.penalties.MemSet(initialPenalty).Schedule(dependencyTracker);
		}

		public void SetDefaultNodePositions (GraphTransform transform) {
			new JobNodeGridLayout {
				graphToWorld = transform.matrix,
				bounds = nodes.bounds,
				nodePositions = nodes.positions,
			}.Schedule(dependencyTracker);
		}

		public JobHandle HeightCheck (GraphCollision collision, float nodeWidth, int maxHits, IntBounds recalculationBounds, NativeArray<int> outLayerCount, float characterHeight, Allocator allocator) {
			// For some reason the physics code crashes when allocating raycastCommands with UninitializedMemory, even though I have verified that every
			// element in the array is set to a well defined value before the physics code gets to it... Mysterious.
			var cellCount = recalculationBounds.size.x * recalculationBounds.size.z;

			heightHits = dependencyTracker.NewNativeArray<RaycastHit>(cellCount * maxHits, allocator, NativeArrayOptions.ClearMemory);
			heightHitsBounds = recalculationBounds;

			// Due to floating point inaccuracies we don't want the rays to end *exactly* at the base of the graph
			// The rays may or may not hit colliders with the exact same y coordinate.
			// We extend the rays a bit to ensure they always hit
			const float RayLengthMargin = 0.01f;
			var raycastOffset = up * collision.fromHeight;
			var raycastDirection = -up * (collision.fromHeight + RayLengthMargin);

			if (collision.thickRaycast) {
				if (maxHits > 1) {
					throw new System.NotImplementedException("Thick raycasts are not supported for layered grid graphs");
				}

				var raycastCommands = dependencyTracker.NewNativeArray<SpherecastCommand>(cellCount, allocator, NativeArrayOptions.ClearMemory);

				new JobPrepareGridRaycastThick {
					graphToWorld = transform.matrix,
					bounds = recalculationBounds,
					physicsScene = Physics.defaultPhysicsScene,
					raycastOffset = raycastOffset,
					raycastDirection = raycastDirection,
					raycastMask = collision.heightMask,
					raycastCommands = raycastCommands,
					radius = collision.thickRaycastDiameter * nodeWidth * 0.5f,
				}.Schedule(dependencyTracker);

				dependencyTracker.ScheduleBatch(raycastCommands, heightHits, 2048);

				new JobClampHitToRay {
					hits = heightHits,
					commands = raycastCommands,
				}.Schedule(dependencyTracker);

				outLayerCount[0] = 1;
				return default;
			} else {
				// Common case
				var raycastCommands = dependencyTracker.NewNativeArray<RaycastCommand>(cellCount, allocator, NativeArrayOptions.ClearMemory);

				var prepareJob = new JobPrepareGridRaycast {
					graphToWorld = transform.matrix,
					bounds = recalculationBounds,
					physicsScene = Physics.defaultPhysicsScene,
					raycastOffset = raycastOffset,
					raycastDirection = raycastDirection,
					raycastMask = collision.heightMask,
					raycastCommands = raycastCommands,
				}.Schedule(dependencyTracker);

				if (maxHits > 1) {
					// Skip this distance between each hit.
					// It is pretty arbitrarily chosen, but it must be lower than characterHeight.
					// If it would be set too low then many thin colliders stacked on top of each other could lead to a very large number of hits
					// that will not lead to any walkable nodes anyway.
					float minStep = characterHeight * 0.5f;
					var dependency = new JobRaycastAll(raycastCommands, heightHits, Physics.defaultPhysicsScene, maxHits, allocator, dependencyTracker, minStep).Schedule(prepareJob);

					dependency = new JobMaxHitCount {
						hits = heightHits,
						maxHits = maxHits,
						layerStride = cellCount,
						maxHitCount = outLayerCount,
					}.Schedule(dependency);

					return dependency;
				} else {
					dependencyTracker.ScheduleBatch(raycastCommands, heightHits, 2048);
					outLayerCount[0] = 1;
					return default;
				}
			}
		}

		public void CopyHits (IntBounds recalculationBounds) {
			// Copy the hit points and normals to separate arrays
			// Ensure the normals for the upper layers are zeroed out.
			nodes.normals.MemSet(float4.zero).Schedule(dependencyTracker);
			new JobCopyHits {
				hits = heightHits,
				points = nodes.positions,
				normals = nodes.normals,
				slice = new Slice3D(nodes.bounds, recalculationBounds),
			}.Schedule(dependencyTracker);
		}

		public void CalculateWalkabilityFromHeightData (bool useRaycastNormal, bool unwalkableWhenNoGround, float maxSlope, float characterHeight) {
			new JobNodeWalkability {
				useRaycastNormal = useRaycastNormal,
				unwalkableWhenNoGround = unwalkableWhenNoGround,
				maxSlope = maxSlope,
				up = up,
				nodeNormals = nodes.normals,
				nodeWalkable = nodes.walkable,
				nodePositions = nodes.positions.Reinterpret<float3>(),
				characterHeight = characterHeight,
				layerStride = nodes.bounds.size.x*nodes.bounds.size.z,
			}.Schedule(dependencyTracker);
		}

		public IEnumerator<JobHandle> CollisionCheck (GraphCollision collision, IntBounds calculationBounds) {
			if (collision.type == ColliderType.Ray && !collision.use2D) {
				var collisionCheckResult = dependencyTracker.NewNativeArray<bool>(nodes.numNodes, nodes.allocationMethod, NativeArrayOptions.UninitializedMemory);
				collision.JobCollisionRay(nodes.positions, collisionCheckResult, up, nodes.allocationMethod, dependencyTracker);
				nodes.walkable.BitwiseAndWith(collisionCheckResult).WithLength(nodes.numNodes).Schedule(dependencyTracker);
				return null;

// Before Unity 6000.1, these features compile, but they will cause memory corruption in some cases, due to a bug in Unity
#if UNITY_2022_2_OR_NEWER && UNITY_6000_1_OR_NEWER
			} else if (collision.type == ColliderType.Capsule && !collision.use2D) {
				var collisionCheckResult = dependencyTracker.NewNativeArray<bool>(nodes.numNodes, nodes.allocationMethod, NativeArrayOptions.UninitializedMemory);
				collision.JobCollisionCapsule(nodes.positions, collisionCheckResult, up, nodes.allocationMethod, dependencyTracker);
				nodes.walkable.BitwiseAndWith(collisionCheckResult).WithLength(nodes.numNodes).Schedule(dependencyTracker);
				return null;
			} else if (collision.type == ColliderType.Sphere && !collision.use2D) {
				var collisionCheckResult = dependencyTracker.NewNativeArray<bool>(nodes.numNodes, nodes.allocationMethod, NativeArrayOptions.UninitializedMemory);
				collision.JobCollisionSphere(nodes.positions, collisionCheckResult, up, nodes.allocationMethod, dependencyTracker);
				nodes.walkable.BitwiseAndWith(collisionCheckResult).WithLength(nodes.numNodes).Schedule(dependencyTracker);
				return null;
#endif
			} else {
				// This part can unfortunately not be jobified yet
				return new JobCheckCollisions {
						   nodePositions = nodes.positions,
						   collisionResult = nodes.walkable,
						   collision = collision,
				}.ExecuteMainThreadJob(dependencyTracker);
			}
		}

		public void Connections (float maxStepHeight, bool maxStepUsesSlope, IntBounds calculationBounds, NumNeighbours neighbours, bool cutCorners, bool use2D, bool useErodedWalkability, float characterHeight) {
			var job = new JobCalculateGridConnections {
				maxStepHeight = maxStepHeight,
				maxStepUsesSlope = maxStepUsesSlope,
				graphToWorld = transform.matrix,
				bounds = calculationBounds.Offset(-nodes.bounds.min),
				arrayBounds = nodes.bounds.size,
				neighbours = neighbours,
				use2D = use2D,
				cutCorners = cutCorners,
				nodeWalkable = (useErodedWalkability ? nodes.walkableWithErosion : nodes.walkable).AsUnsafeSpanNoChecks(),
				nodePositions = nodes.positions.AsUnsafeSpanNoChecks(),
				nodeNormals = nodes.normals.AsUnsafeSpanNoChecks(),
				nodeConnections = nodes.connections.AsUnsafeSpanNoChecks(),
				characterHeight = characterHeight,
				layeredDataLayout = nodes.layeredDataLayout,
			};

			if (dependencyTracker != null) {
				job.ScheduleBatch(calculationBounds.size.z, 20, dependencyTracker);
			} else {
				job.RunBatch(calculationBounds.size.z);
			}

			// For single layer graphs this will have already been done in the JobCalculateGridConnections job
			// but for layered grid graphs we need to handle things differently because the data layout is different.
			// It needs to be done after all axis aligned connections have been calculated.
			if (nodes.layeredDataLayout) {
				var job2 = new JobFilterDiagonalConnections {
					slice = new Slice3D(nodes.bounds, calculationBounds),
					neighbours = neighbours,
					cutCorners = cutCorners,
					nodeConnections = nodes.connections.AsUnsafeSpanNoChecks(),
				};
				if (dependencyTracker != null) {
					job2.ScheduleBatch(calculationBounds.size.z, 20, dependencyTracker);
				} else {
					job2.RunBatch(calculationBounds.size.z);
				}
			}
		}

		public void Erosion (NumNeighbours neighbours, int erodeIterations, IntBounds erosionWriteMask, bool erosionUsesTags, int erosionStartTag, int erosionTagsPrecedenceMask) {
			if (!nodes.layeredDataLayout) {
				new JobErosion<FlatGridAdjacencyMapper> {
					bounds = nodes.bounds,
					writeMask = erosionWriteMask,
					neighbours = neighbours,
					nodeConnections = nodes.connections,
					erosion = erodeIterations,
					nodeWalkable = nodes.walkable,
					outNodeWalkable = nodes.walkableWithErosion,
					nodeTags = nodes.tags,
					erosionUsesTags = erosionUsesTags,
					erosionStartTag = erosionStartTag,
					erosionTagsPrecedenceMask = erosionTagsPrecedenceMask,
				}.Schedule(dependencyTracker);
			} else {
				new JobErosion<LayeredGridAdjacencyMapper> {
					bounds = nodes.bounds,
					writeMask = erosionWriteMask,
					neighbours = neighbours,
					nodeConnections = nodes.connections,
					erosion = erodeIterations,
					nodeWalkable = nodes.walkable,
					outNodeWalkable = nodes.walkableWithErosion,
					nodeTags = nodes.tags,
					erosionUsesTags = erosionUsesTags,
					erosionStartTag = erosionStartTag,
					erosionTagsPrecedenceMask = erosionTagsPrecedenceMask,
				}.Schedule(dependencyTracker);
			}
		}

		public void AssignNodeConnections (GridNodeBase[] nodes, int3 nodeArrayBounds, IntBounds writeBounds) {
			var bounds = this.nodes.bounds;
			var writeDataOffset = writeBounds.min - bounds.min;
			var nodeConnections = this.nodes.connections.AsUnsafeReadOnlySpan();
			for (int y = 0; y < writeBounds.size.y; y++) {
				var yoffset = (y + writeBounds.min.y)*nodeArrayBounds.x*nodeArrayBounds.z;
				for (int z = 0; z < writeBounds.size.z; z++) {
					var zoffset = yoffset + (z + writeBounds.min.z)*nodeArrayBounds.x + writeBounds.min.x;
					var zoffset2 = (y+writeDataOffset.y)*bounds.size.x*bounds.size.z + (z+writeDataOffset.z)*bounds.size.x + writeDataOffset.x;
					for (int x = 0; x < writeBounds.size.x; x++) {
						var node = nodes[zoffset + x];
						var dataIdx = zoffset2 + x;
						var conn = nodeConnections[dataIdx];

						if (node == null) continue;

						if (node is LevelGridNode lgnode) {
							lgnode.SetAllConnectionInternal(conn);
						} else {
							var gnode = node as GridNode;
							gnode.SetAllConnectionInternal((int)conn);
						}
					}
				}
			}
		}
	}
}
