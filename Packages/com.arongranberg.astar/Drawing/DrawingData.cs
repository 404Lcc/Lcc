using UnityEngine;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Rendering;
using System.Diagnostics;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Profiling;
using System.Linq;

namespace Pathfinding.Drawing {
	using Pathfinding.Drawing.Text;
	using Unity.Profiling;

	public static class SharedDrawingData {
		/// <summary>
		/// Same as Time.time, but not updated as frequently.
		/// Used since burst jobs cannot access Time.time.
		/// </summary>
		public static readonly Unity.Burst.SharedStatic<float> BurstTime = Unity.Burst.SharedStatic<float>.GetOrCreate<DrawingManager, BurstTimeKey>(4);

		private class BurstTimeKey {}
	}

	/// <summary>
	/// Used to cache drawing data over multiple frames.
	/// This is useful as a performance optimization when you are drawing the same thing over multiple consecutive frames.
	///
	/// <code>
	/// private RedrawScope redrawScope;
	///
	/// void Start () {
	///     redrawScope = DrawingManager.GetRedrawScope();
	///     using (var builder = DrawingManager.GetBuilder(redrawScope)) {
	///         builder.WireSphere(Vector3.zero, 1.0f, Color.red);
	///     }
	/// }
	///
	/// void OnDestroy () {
	///     redrawScope.Dispose();
	/// }
	/// </code>
	///
	/// See: <see cref="DrawingManager.GetRedrawScope"/>
	/// </summary>
	public struct RedrawScope : System.IDisposable {
		// Stored as a GCHandle to allow storing this struct in an unmanaged ECS component or system
		internal System.Runtime.InteropServices.GCHandle gizmos;
		/// <summary>
		/// ID of the scope.
		/// Zero means no or invalid scope.
		/// </summary>
		internal int id;

		static int idCounter = 1;

		/// <summary>True if the scope has been created</summary>
		public bool isValid => id != 0;

		internal RedrawScope (DrawingData gizmos, int id) {
			this.gizmos = gizmos.gizmosHandle;
			this.id = id;
		}

		internal RedrawScope (DrawingData gizmos) {
			this.gizmos = gizmos.gizmosHandle;
			// Should be enough with 4 billion ids before they wrap around.
			id = idCounter++;
		}

		/// <summary>
		/// Everything rendered with this scope and which is not older than one frame is drawn again.
		/// This is useful if you for some reason cannot draw some items during a frame (e.g. some asynchronous process is modifying the contents)
		/// but you still want to draw the same thing as the last frame to at least draw *something*.
		///
		/// Note: The items age will be reset. So the next frame you can call
		/// this method again to draw the items yet again.
		/// </summary>
		internal void Draw () {
			if (gizmos.IsAllocated) {
				if (gizmos.Target is DrawingData gizmosTarget) gizmosTarget.Draw(this);
			}
		}

		/// <summary>
		/// Stops keeping all previously rendered items alive, and starts a new scope.
		/// Equivalent to first calling Dispose on the old scope and then creating a new one.
		/// </summary>
		public void Rewind () {
			GameObject associatedGameObject = null;
			if (gizmos.IsAllocated) {
				if (gizmos.Target is DrawingData gizmosTarget) associatedGameObject = gizmosTarget.GetAssociatedGameObject(this);
			}
			Dispose();
			this = DrawingManager.GetRedrawScope(associatedGameObject);
		}

		internal void DrawUntilDispose (GameObject associatedGameObject) {
			if (gizmos.Target is DrawingData gizmosTarget) gizmosTarget.DrawUntilDisposed(this, associatedGameObject);
		}

		/// <summary>
		/// Dispose the redraw scope to stop rendering the items.
		///
		/// You must do this when you are done with the scope, even if it was never used to actually render anything.
		/// The items will stop rendering immediately: the next camera to render will not render the items unless kept alive in some other way.
		/// However, items are always rendered at least once.
		/// </summary>
		public void Dispose () {
			if (gizmos.IsAllocated) {
				if (gizmos.Target is DrawingData gizmosTarget) gizmosTarget.DisposeRedrawScope(this);
			}
			gizmos = default;
			id = 0;
		}
	};

	/// <summary>Helper for drawing Gizmos in a performant way</summary>
	public class DrawingData {
		/// <summary>Combines hashes into a single hash value</summary>
		public struct Hasher : IEquatable<Hasher> {
			ulong hash;

			public static Hasher NotSupplied => new Hasher { hash = ulong.MaxValue };

			[System.Obsolete("Use the constructor instead")]
			public static Hasher Create<T>(T init) {
				var h = new Hasher();

				h.Add(init);
				return h;
			}

			/// <summary>
			/// Includes the given data in the final hash.
			/// You can call this as many times as you want.
			/// </summary>
			public void Add<T>(T hash) {
				// Just a regular hash function. The + 12289 is to make sure that hashing zeros doesn't just produce a zero (and generally that hashing one X doesn't produce a hash of X)
				// (with a struct we can't provide default initialization)
				this.hash = (1572869UL * this.hash) ^ (ulong)hash.GetHashCode() + 12289;
			}

			public readonly ulong Hash => hash;

			public override int GetHashCode () {
				return (int)hash;
			}

			public bool Equals (Hasher other) {
				return hash == other.hash;
			}
		}

		internal struct ProcessedBuilderData {
			public enum Type {
				Invalid = 0,
				Static,
				Dynamic,
				Persistent,
			}

			public Type type;
			public BuilderData.Meta meta;
			bool submitted;

			// A single instance of a MeshBuffers struct.
			// This needs to be stored in a NativeArray because we will use it as a pointer
			// and it needs to be guaranteed to stay in the same position in memory.
			public NativeArray<MeshBuffers> temporaryMeshBuffers;
			JobHandle buildJob, splitterJob;
			public List<MeshWithType> meshes;

			public bool isValid {
				get {
					return type != Type.Invalid;
				}
			}

			public struct CapturedState {
				public Matrix4x4 matrix;
				public Color color;
			}

			public struct MeshBuffers {
				public UnsafeAppendBuffer splitterOutput, vertices, triangles, solidVertices, solidTriangles, textVertices, textTriangles, capturedState;
				public Bounds bounds;

				public MeshBuffers(Allocator allocator) {
					splitterOutput = new UnsafeAppendBuffer(0, 4, allocator);
					vertices = new UnsafeAppendBuffer(0, 4, allocator);
					triangles = new UnsafeAppendBuffer(0, 4, allocator);
					solidVertices = new UnsafeAppendBuffer(0, 4, allocator);
					solidTriangles = new UnsafeAppendBuffer(0, 4, allocator);
					textVertices = new UnsafeAppendBuffer(0, 4, allocator);
					textTriangles = new UnsafeAppendBuffer(0, 4, allocator);
					capturedState = new UnsafeAppendBuffer(0, 4, allocator);
					bounds = new Bounds();
				}

				public void Dispose () {
					splitterOutput.Dispose();
					vertices.Dispose();
					triangles.Dispose();
					solidVertices.Dispose();
					solidTriangles.Dispose();
					textVertices.Dispose();
					textTriangles.Dispose();
					capturedState.Dispose();
				}

				static void DisposeIfLarge (ref UnsafeAppendBuffer ls) {
					if (ls.Length*3 < ls.Capacity && ls.Capacity > 1024) {
						var alloc = ls.Allocator;
						ls.Dispose();
						ls = new UnsafeAppendBuffer(0, 4, alloc);
					}
				}

				public void DisposeIfLarge () {
					DisposeIfLarge(ref splitterOutput);
					DisposeIfLarge(ref vertices);
					DisposeIfLarge(ref triangles);
					DisposeIfLarge(ref solidVertices);
					DisposeIfLarge(ref solidTriangles);
					DisposeIfLarge(ref textVertices);
					DisposeIfLarge(ref textTriangles);
					DisposeIfLarge(ref capturedState);
				}
			}

			public unsafe UnsafeAppendBuffer* splitterOutputPtr => & ((MeshBuffers*)temporaryMeshBuffers.GetUnsafePtr())->splitterOutput;

			public void Init (Type type, BuilderData.Meta meta) {
				submitted = false;
				this.type = type;
				this.meta = meta;

				if (meshes == null) meshes = new List<MeshWithType>();
				if (!temporaryMeshBuffers.IsCreated) {
					temporaryMeshBuffers = new NativeArray<MeshBuffers>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
					temporaryMeshBuffers[0] = new MeshBuffers(Allocator.Persistent);
				}
			}

			static int SubmittedJobs = 0;

			public void SetSplitterJob (DrawingData gizmos, JobHandle splitterJob) {
				this.splitterJob = splitterJob;
				if (type == Type.Static) {
					var cameraInfo = new GeometryBuilder.CameraInfo(null);
					unsafe {
						buildJob = GeometryBuilder.Build(gizmos, (MeshBuffers*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(temporaryMeshBuffers), ref cameraInfo, splitterJob);
					}

					SubmittedJobs++;
					// ScheduleBatchedJobs is expensive, so only do it once in a while
					if (SubmittedJobs % 8 == 0) {
						MarkerScheduleJobs.Begin();
						JobHandle.ScheduleBatchedJobs();
						MarkerScheduleJobs.End();
					}
				}
			}

			public void SchedulePersistFilter (int version, int lastTickVersion, float time, int sceneModeVersion) {
				if (type != Type.Persistent) throw new System.InvalidOperationException();

				// If data was from a different game mode then it shouldn't live any longer.
				// E.g. editor mode => game mode
				if (meta.sceneModeVersion != sceneModeVersion) {
					meta.version = -1;
					return;
				}

				// Guarantee that all drawing commands survive at least one frame
				// Don't filter them until they have had the opportunity to be drawn once at least.
				// (they may not actually have been drawn because no cameras may be active)
				if (meta.version < lastTickVersion || submitted) {
					splitterJob.Complete();
					meta.version = version;

					// If the command buffer is empty then this instance should not live longer
					var splitterOutput = temporaryMeshBuffers[0].splitterOutput;
					if (splitterOutput.Length == 0) {
						meta.version = -1;
						return;
					}

					buildJob.Complete();
					unsafe {
						splitterJob = new PersistentFilterJob {
							buffer = &((MeshBuffers*)NativeArrayUnsafeUtility.GetUnsafePtr(temporaryMeshBuffers))->splitterOutput,
							time = time,
						}.Schedule(splitterJob);
					}
				}
			}

			public bool IsValidForCamera (Camera camera, bool allowGizmos, bool allowCameraDefault) {
				if (!allowGizmos && meta.isGizmos) return false;

				if (meta.cameraTargets != null) {
					return meta.cameraTargets.Contains(camera);
				} else {
					return allowCameraDefault;
				}
			}

			public void Schedule (DrawingData gizmos, ref GeometryBuilder.CameraInfo cameraInfo) {
				// The job for Static will already have been scheduled in SetSplitterJob
				if (type != Type.Static) {
					unsafe {
						buildJob = GeometryBuilder.Build(gizmos, (MeshBuffers*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(temporaryMeshBuffers), ref cameraInfo, splitterJob);
					}
				}
			}

			public void BuildMeshes (DrawingData gizmos) {
				if (type == Type.Static && submitted) return;
				buildJob.Complete();
				unsafe {
					GeometryBuilder.BuildMesh(gizmos, meshes, (MeshBuffers*)temporaryMeshBuffers.GetUnsafePtr());
				}
				submitted = true;
			}

			public void CollectMeshes (List<RenderedMeshWithType> meshes) {
				var itemMeshes = this.meshes;
				var customMeshIndex = 0;
				var capturedState = temporaryMeshBuffers[0].capturedState;
				var maxCustomMeshes = capturedState.Length / UnsafeUtility.SizeOf<CapturedState>();

				for (int i = 0; i < itemMeshes.Count; i++) {
					Color color;
					Matrix4x4 matrix;
					int drawOrderIndex;
					if ((itemMeshes[i].type & MeshType.Custom) != 0) {
						UnityEngine.Assertions.Assert.IsTrue(customMeshIndex < maxCustomMeshes);

						// The color and orientation of custom meshes are stored in the captured state array.
						// It is indexed in the same order as the custom meshes in the #meshes list.
						unsafe {
							var state = *((CapturedState*)capturedState.Ptr + customMeshIndex);
							color = state.color;
							matrix = state.matrix;
							customMeshIndex += 1;
						}
						// Custom meshes are rendered *after* all similar builders.
						// In practice this means all custom meshes are drawn after all dynamic items.
						drawOrderIndex = meta.drawOrderIndex + 1;
					} else {
						// All other meshes use default colors and identity matrices
						// since their data is already baked into the vertex colors and positions
						color = Color.white;
						matrix = Matrix4x4.identity;
						drawOrderIndex = meta.drawOrderIndex;
					}
					meshes.Add(new RenderedMeshWithType {
						mesh = itemMeshes[i].mesh,
						type = itemMeshes[i].type,
						drawingOrderIndex = drawOrderIndex,
						color = color,
						matrix = matrix,
					});
				}
			}

			void PoolMeshes (DrawingData gizmos, bool includeCustom) {
				if (!isValid) throw new System.InvalidOperationException();
				var outIndex = 0;
				for (int i = 0; i < meshes.Count; i++) {
					// Custom meshes should only be pooled if the Pool flag is set.
					// Otherwise they are supplied by the user and it's up to them how to handle it.
					if ((meshes[i].type & MeshType.Custom) == 0 || (includeCustom && (meshes[i].type & MeshType.Pool) != 0)) {
						gizmos.PoolMesh(meshes[i].mesh);
					} else {
						// Retain custom meshes
						meshes[outIndex] = meshes[i];
						outIndex += 1;
					}
				}
				meshes.RemoveRange(outIndex, meshes.Count - outIndex);
			}

			public void PoolDynamicMeshes (DrawingData gizmos) {
				if (type == Type.Static && submitted) return;
				PoolMeshes(gizmos, false);
			}

			public void Release (DrawingData gizmos) {
				if (!isValid) throw new System.InvalidOperationException();
				PoolMeshes(gizmos, true);
				// Clear custom meshes too
				meshes.Clear();
				type = Type.Invalid;
				splitterJob.Complete();
				buildJob.Complete();
				var bufs = this.temporaryMeshBuffers[0];
				bufs.DisposeIfLarge();
				this.temporaryMeshBuffers[0] = bufs;
			}

			public void Dispose () {
				if (isValid) throw new System.InvalidOperationException();
				splitterJob.Complete();
				buildJob.Complete();
				if (temporaryMeshBuffers.IsCreated) {
					temporaryMeshBuffers[0].Dispose();
					temporaryMeshBuffers.Dispose();
				}
			}
		}

		internal struct SubmittedMesh {
			public Mesh mesh;
			public bool temporary;
		}

		[BurstCompile]
		internal struct BuilderData : IDisposable {
			public enum State {
				Free,
				Reserved,
				Initialized,
				WaitingForSplitter,
				WaitingForUserDefinedJob,
			}

			public struct Meta {
				public Hasher hasher;
				public RedrawScope redrawScope1;
				public RedrawScope redrawScope2;
				public int version;
				public bool isGizmos;
				/// <summary>Used to invalidate gizmos when the scene mode changes</summary>
				public int sceneModeVersion;
				public int drawOrderIndex;
				public Camera[] cameraTargets;
			}

			public struct BitPackedMeta {
				uint flags;

				const int UniqueIDBitshift = 17;
				const int IsBuiltInFlagIndex = 16;
				const int IndexMask = (1 << IsBuiltInFlagIndex) - 1;
				const int MaxDataIndex = IndexMask;
				public const int UniqueIdMask = (1 << (32 - UniqueIDBitshift)) - 1;


				public BitPackedMeta (int dataIndex, int uniqueID, bool isBuiltInCommandBuilder) {
					// Important to make ensure bitpacking doesn't collide
					if (dataIndex > MaxDataIndex) throw new System.Exception("Too many command builders active. Are some command builders not being disposed?");
					UnityEngine.Assertions.Assert.IsTrue(uniqueID <= UniqueIdMask && uniqueID >= 0);

					flags = (uint)(dataIndex | uniqueID << UniqueIDBitshift | (isBuiltInCommandBuilder ? 1 << IsBuiltInFlagIndex : 0));
				}

				public int dataIndex {
					get {
						return (int)(flags & IndexMask);
					}
				}

				public int uniqueID {
					get {
						return (int)(flags >> UniqueIDBitshift);
					}
				}

				public bool isBuiltInCommandBuilder {
					get {
						return (flags & (1 << IsBuiltInFlagIndex)) != 0;
					}
				}

				public static bool operator== (BitPackedMeta lhs, BitPackedMeta rhs) {
					return lhs.flags == rhs.flags;
				}

				public static bool operator!= (BitPackedMeta lhs, BitPackedMeta rhs) {
					return lhs.flags != rhs.flags;
				}

				public override bool Equals (object obj) {
					if (obj is BitPackedMeta meta) {
						return flags == meta.flags;
					}
					return false;
				}

				public override int GetHashCode () {
					return (int)flags;
				}
			}

			public BitPackedMeta packedMeta;
			public List<SubmittedMesh> meshes;
			public NativeArray<UnsafeAppendBuffer> commandBuffers;
			public State state { get; private set; }
			// TODO?
			public bool preventDispose;
			JobHandle splitterJob;
			JobHandle disposeDependency;
			AllowedDelay disposeDependencyDelay;
			System.Runtime.InteropServices.GCHandle disposeGCHandle;
			public Meta meta;

			public void Reserve (int dataIndex, bool isBuiltInCommandBuilder) {
				if (state != State.Free) throw new System.InvalidOperationException();
				state = BuilderData.State.Reserved;
				packedMeta = new BitPackedMeta(dataIndex, (UniqueIDCounter++) & BitPackedMeta.UniqueIdMask, isBuiltInCommandBuilder);
			}

			static int UniqueIDCounter = 0;

			public void Init (Hasher hasher, RedrawScope frameRedrawScope, RedrawScope customRedrawScope, bool isGizmos, int drawOrderIndex, int sceneModeVersion) {
				if (state != State.Reserved) throw new System.InvalidOperationException();

				meta = new Meta {
					hasher = hasher,
					redrawScope1 = frameRedrawScope,
					redrawScope2 = customRedrawScope,
					isGizmos = isGizmos,
					version = 0, // Will be filled in later
					drawOrderIndex = drawOrderIndex,
					sceneModeVersion = sceneModeVersion,
					cameraTargets = null,
				};

				if (meshes == null) meshes = new List<SubmittedMesh>();
				if (!commandBuffers.IsCreated) {
#if UNITY_2022_3_OR_NEWER
					commandBuffers = new NativeArray<UnsafeAppendBuffer>(JobsUtility.ThreadIndexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
#else
					commandBuffers = new NativeArray<UnsafeAppendBuffer>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
#endif
					for (int i = 0; i < commandBuffers.Length; i++) commandBuffers[i] = new UnsafeAppendBuffer(0, 4, Allocator.Persistent);
				}

				state = State.Initialized;
			}

			public unsafe UnsafeAppendBuffer* bufferPtr {
				get {
					return (UnsafeAppendBuffer*)commandBuffers.GetUnsafePtr();
				}
			}

			[BurstCompile]
			[AOT.MonoPInvokeCallback(typeof(AnyBuffersWrittenToDelegate))]
			unsafe static bool AnyBuffersWrittenTo (UnsafeAppendBuffer* buffers, int numBuffers) {
				bool any = false;

				for (int i = 0; i < numBuffers; i++) {
					any |= buffers[i].Length > 0;
				}
				return any;
			}

			[BurstCompile]
			[AOT.MonoPInvokeCallback(typeof(AnyBuffersWrittenToDelegate))]
			unsafe static void ResetAllBuffers (UnsafeAppendBuffer* buffers, int numBuffers) {
				for (int i = 0; i < numBuffers; i++) {
					buffers[i].Reset();
				}
			}

			unsafe delegate bool AnyBuffersWrittenToDelegate(UnsafeAppendBuffer* buffers, int numBuffers);
			private readonly unsafe static AnyBuffersWrittenToDelegate AnyBuffersWrittenToInvoke = BurstCompiler.CompileFunctionPointer<AnyBuffersWrittenToDelegate>(AnyBuffersWrittenTo).Invoke;
			unsafe delegate void ResetAllBuffersToDelegate(UnsafeAppendBuffer* buffers, int numBuffers);
			private readonly unsafe static ResetAllBuffersToDelegate ResetAllBuffersToInvoke = BurstCompiler.CompileFunctionPointer<ResetAllBuffersToDelegate>(ResetAllBuffers).Invoke;

			public void SubmitWithDependency (System.Runtime.InteropServices.GCHandle gcHandle, JobHandle dependency, AllowedDelay allowedDelay) {
				state = State.WaitingForUserDefinedJob;
				disposeDependency = dependency;
				disposeDependencyDelay = allowedDelay;
				disposeGCHandle = gcHandle;
			}

			public void Submit (DrawingData gizmos) {
				if (state != State.Initialized) throw new System.InvalidOperationException();

#if !UNITY_EDITOR
				if (meta.isGizmos) {
					// Gizmos are never drawn in standalone builds.
					// Draw.Line, and similar draw commands, will already have been removed in standalone builds,
					// but if users use e.g. Draw.editor directly, then the commands will be added to the command buffer.
					// For performance we can just discard the whole buffer here.
					Release();
					return;
				}
#endif


				unsafe {
					// There are about 128 buffers we need to check and it's faster to do that using Burst
					if (meshes.Count == 0 && !AnyBuffersWrittenToInvoke((UnsafeAppendBuffer*)commandBuffers.GetUnsafeReadOnlyPtr(), commandBuffers.Length)) {
						// If no buffers have been written to then simply discard this builder
						Release();
						return;
					}
				}

				meta.version = gizmos.version;

				// Command stream
				// split to static, dynamic and persistent
				// render static
				// render dynamic per camera
				// render persistent per camera
				const int PersistentDrawOrderOffset = 1000000;
				var tmpMeta = meta;
				// Reserve some buffers.
				// We need to set a deterministic order in which things are drawn to avoid flickering.
				// The shaders use the z buffer most of the time, but there are still
				// things which are not order independent.
				// Static stuff is drawn first
				tmpMeta.drawOrderIndex = meta.drawOrderIndex*3 + 0;
				int staticBuffer = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Static, tmpMeta);
				// Dynamic stuff is drawn directly after the static stuff
				// Note that any custom meshes will get this draw order index + 1.
				tmpMeta.drawOrderIndex = meta.drawOrderIndex*3 + 1;
				int dynamicBuffer = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Dynamic, tmpMeta);
				// Persistent stuff is always drawn after everything else
				tmpMeta.drawOrderIndex = meta.drawOrderIndex + PersistentDrawOrderOffset;
				int persistentBuffer = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Persistent, tmpMeta);

				unsafe {
					splitterJob = new StreamSplitter {
						inputBuffers = commandBuffers,
						staticBuffer = gizmos.processedData.Get(staticBuffer).splitterOutputPtr,
						dynamicBuffer = gizmos.processedData.Get(dynamicBuffer).splitterOutputPtr,
						persistentBuffer = gizmos.processedData.Get(persistentBuffer).splitterOutputPtr,
					}.Schedule();
				}

				gizmos.processedData.Get(staticBuffer).SetSplitterJob(gizmos, splitterJob);
				gizmos.processedData.Get(dynamicBuffer).SetSplitterJob(gizmos, splitterJob);
				gizmos.processedData.Get(persistentBuffer).SetSplitterJob(gizmos, splitterJob);

				if (meshes.Count > 0) {
					// Custom meshes may be affected by matrices and colors that are set in the command builders.
					// Matrices may in theory be dynamic per camera (though this functionality is not used at the moment).
					// The Command.CaptureState commands are marked as Dynamic so captured state will be written to
					// the meshBuffers.capturedState array in the #dynamicBuffer.
					var customMeshes = gizmos.processedData.Get(dynamicBuffer).meshes;

					// Copy meshes to render
					for (int i = 0; i < meshes.Count; i++) customMeshes.Add(new MeshWithType { mesh = meshes[i].mesh, type = MeshType.Solid | MeshType.Custom | (meshes[i].temporary ? MeshType.Pool : 0) });
					meshes.Clear();
				}

				// TODO: Allocate 3 output objects and pipe splitter to them

				// Only meshes valid for all cameras have been submitted.
				// Meshes that depend on the specific camera will be submitted just before rendering
				// that camera. Line drawing depends on the exact camera.
				// In particular when drawing circles different number of segments
				// are used depending on the distance to the camera.
				state = State.WaitingForSplitter;
			}

			public void CheckJobDependency (DrawingData gizmos, bool allowBlocking) {
				if (state == State.WaitingForUserDefinedJob && (disposeDependency.IsCompleted || (allowBlocking && disposeDependencyDelay == AllowedDelay.EndOfFrame))) {
					disposeDependency.Complete();
					disposeDependency = default;
					disposeGCHandle.Free();
					state = State.Initialized;
					Submit(gizmos);
				}
			}

			public void Release () {
				if (state == State.Free) throw new System.InvalidOperationException();
				state = BuilderData.State.Free;
				ClearData();
			}

			void ClearData () {
				// Wait for any jobs that might be running
				// This is important to avoid memory corruption bugs
				disposeDependency.Complete();
				splitterJob.Complete();
				meta = default;
				disposeDependency = default;
				preventDispose = false;
				meshes.Clear();
				unsafe {
					// There are about 128 buffers we need to reset and it's faster to do that using Burst
					ResetAllBuffers((UnsafeAppendBuffer*)commandBuffers.GetUnsafePtr(), commandBuffers.Length);
				}
			}

			public void Dispose () {
				if (state == State.WaitingForUserDefinedJob) {
					disposeDependency.Complete();
					disposeGCHandle.Free();
					// We would call Submit here, but we are deleting the data anyway, so who cares.
					state = State.WaitingForSplitter;
				}

				if (state == State.Reserved || state == State.Initialized || state == State.WaitingForUserDefinedJob) {
					UnityEngine.Debug.LogError("Drawing data is being destroyed, but a drawing instance is still active. Are you sure you have called Dispose on all drawing instances? This will cause a memory leak!");
					return;
				}

				splitterJob.Complete();
				if (commandBuffers.IsCreated) {
					for (int i = 0; i < commandBuffers.Length; i++) {
						commandBuffers[i].Dispose();
					}
					commandBuffers.Dispose();
				}
			}
		}

		internal struct BuilderDataContainer : IDisposable {
			BuilderData[] data;

			public int memoryUsage {
				get {
					int sum = 0;
					if (data != null) {
						for (int i = 0; i < data.Length; i++) {
							var cmds = data[i].commandBuffers;
							for (int j = 0; j < cmds.Length; j++) {
								sum += cmds[j].Capacity;
							}
							unsafe {
								sum += data[i].commandBuffers.Length * sizeof(UnsafeAppendBuffer);
							}
						}
					}
					return sum;
				}
			}


			public BuilderData.BitPackedMeta Reserve (bool isBuiltInCommandBuilder) {
				if (data == null) data = new BuilderData[1];
				for (int i = 0; i < data.Length; i++) {
					if (data[i].state == BuilderData.State.Free) {
						data[i].Reserve(i, isBuiltInCommandBuilder);
						return data[i].packedMeta;
					}
				}

				var newData = new BuilderData[data.Length * 2];
				data.CopyTo(newData, 0);
				data = newData;
				return Reserve(isBuiltInCommandBuilder);
			}

			public void Release (BuilderData.BitPackedMeta meta) {
				data[meta.dataIndex].Release();
			}

			public bool StillExists (BuilderData.BitPackedMeta meta) {
				int index = meta.dataIndex;

				if (data == null || index >= data.Length) return false;
				return data[index].packedMeta == meta;
			}

			public ref BuilderData Get (BuilderData.BitPackedMeta meta) {
				int index = meta.dataIndex;

				if (data[index].state == BuilderData.State.Free) throw new System.ArgumentException("Data is not reserved");
				if (data[index].packedMeta != meta) throw new System.ArgumentException("This command builder has already been disposed");
				return ref data[index];
			}

			public void DisposeCommandBuildersWithJobDependencies (DrawingData gizmos) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) data[i].CheckJobDependency(gizmos, false);
				MarkerAwaitUserDependencies.Begin();
				for (int i = 0; i < data.Length; i++) data[i].CheckJobDependency(gizmos, true);
				MarkerAwaitUserDependencies.End();
			}

			public void ReleaseAllUnused () {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].state == BuilderData.State.WaitingForSplitter) {
						data[i].Release();
					}
				}
			}

			public void Dispose () {
				if (data != null) {
					for (int i = 0; i < data.Length; i++) data[i].Dispose();
				}
				// Ensures calling Dispose multiple times is a NOOP
				data = null;
			}
		}

		internal struct ProcessedBuilderDataContainer {
			ProcessedBuilderData[] data;
			Dictionary<ulong, List<int> > hash2index;
			Stack<int> freeSlots;
			Stack<List<int> > freeLists;

			public bool isEmpty => data == null || freeSlots.Count == data.Length;

			public int memoryUsage {
				get {
					int sum = 0;
					if (data != null) {
						for (int i = 0; i < data.Length; i++) {
							var bufs = data[i].temporaryMeshBuffers;
							for (int j = 0; j < bufs.Length; j++) {
								var psum = 0;
								psum += bufs[j].textVertices.Capacity;
								psum += bufs[j].textTriangles.Capacity;
								psum += bufs[j].solidVertices.Capacity;
								psum += bufs[j].solidTriangles.Capacity;
								psum += bufs[j].vertices.Capacity;
								psum += bufs[j].triangles.Capacity;
								psum += bufs[j].capturedState.Capacity;
								psum += bufs[j].splitterOutput.Capacity;
								sum += psum;
								UnityEngine.Debug.Log(i + ":" + j + " " + psum);
							}
						}
					}
					return sum;
				}
			}

			public int Reserve (ProcessedBuilderData.Type type, BuilderData.Meta meta) {
				if (data == null) {
					data = new ProcessedBuilderData[0];
					freeSlots = new Stack<int>();
					freeLists = new Stack<List<int> >();
					hash2index = new Dictionary<ulong, List<int> >();
				}
				if (freeSlots.Count == 0) {
					var newData = new ProcessedBuilderData[math.max(4, data.Length*2)];
					data.CopyTo(newData, 0);
					for (int i = data.Length; i < newData.Length; i++) freeSlots.Push(i);
					data = newData;
				}
				int index = freeSlots.Pop();
				data[index].Init(type, meta);
				if (!meta.hasher.Equals(Hasher.NotSupplied)) {
					List<int> ls;
					if (!hash2index.TryGetValue(meta.hasher.Hash, out ls)) {
						if (freeLists.Count == 0) freeLists.Push(new List<int>());
						ls = hash2index[meta.hasher.Hash] = freeLists.Pop();
					}
					ls.Add(index);
				}
				return index;
			}

			public ref ProcessedBuilderData Get (int index) {
				if (!data[index].isValid) throw new System.ArgumentException();
				return ref data[index];
			}

			void Release (DrawingData gizmos, int i) {
				var h = data[i].meta.hasher.Hash;

				if (!data[i].meta.hasher.Equals(Hasher.NotSupplied)) {
					if (hash2index.TryGetValue(h, out var ls)) {
						ls.Remove(i);
						if (ls.Count == 0) {
							freeLists.Push(ls);
							hash2index.Remove(h);
						}
					}
				}
				data[i].Release(gizmos);
				freeSlots.Push(i);
			}

			public void SubmitMeshes (DrawingData gizmos, Camera camera, int versionThreshold, bool allowGizmos, bool allowCameraDefault) {
				if (data == null) return;
				MarkerSchedule.Begin();
				var cameraInfo = new GeometryBuilder.CameraInfo(camera);
				int c = 0;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version >= versionThreshold && data[i].IsValidForCamera(camera, allowGizmos, allowCameraDefault)) {
						c++;
						data[i].Schedule(gizmos, ref cameraInfo);
					}
				}

				MarkerSchedule.End();

				// Ensure all jobs start to be executed on the worker threads now
				JobHandle.ScheduleBatchedJobs();

				MarkerBuild.Begin();
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version >= versionThreshold && data[i].IsValidForCamera(camera, allowGizmos, allowCameraDefault)) {
						data[i].BuildMeshes(gizmos);
					}
				}
				MarkerBuild.End();
			}

			/// <summary>
			/// Remove any existing dynamic meshes since we know we will not need them after this frame.
			/// We do not remove custom meshes or static ones because those may be kept between frames and cameras.
			/// </summary>
			public void PoolDynamicMeshes (DrawingData gizmos) {
				if (data == null) return;
				MarkerPool.Begin();
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid) {
						data[i].PoolDynamicMeshes(gizmos);
					}
				}
				MarkerPool.End();
			}

			public void CollectMeshes (int versionThreshold, List<RenderedMeshWithType> meshes, Camera camera, bool allowGizmos, bool allowCameraDefault) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version >= versionThreshold && data[i].IsValidForCamera(camera, allowGizmos, allowCameraDefault)) {
						data[i].CollectMeshes(meshes);
					}
				}
			}

			public void FilterOldPersistentCommands (int version, int lastTickVersion, float time, int sceneModeVersion) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].type == ProcessedBuilderData.Type.Persistent) {
						data[i].SchedulePersistFilter(version, lastTickVersion, time, sceneModeVersion);
					}
				}
			}

			public bool SetVersion (Hasher hasher, int version) {
				if (data == null) return false;

				if (hash2index.TryGetValue(hasher.Hash, out var indices)) {
					UnityEngine.Assertions.Assert.IsTrue(indices.Count > 0);
					for (int id = 0; id < indices.Count; id++) {
						var i = indices[id];
						UnityEngine.Assertions.Assert.IsTrue(data[i].isValid);
						UnityEngine.Assertions.Assert.AreEqual(data[i].meta.hasher.Hash, hasher.Hash);
						data[i].meta.version = version;
					}
					return true;
				} else {
					return false;
				}
			}

			public bool SetVersion (RedrawScope scope, int version) {
				if (data == null) return false;
				bool found = false;

				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && (data[i].meta.redrawScope1.id == scope.id || data[i].meta.redrawScope2.id == scope.id)) {
						data[i].meta.version = version;
						found = true;
					}
				}
				return found;
			}

			public bool SetCustomScope (Hasher hasher, RedrawScope scope) {
				if (data == null) return false;

				if (hash2index.TryGetValue(hasher.Hash, out var indices)) {
					UnityEngine.Assertions.Assert.IsTrue(indices.Count > 0);
					for (int id = 0; id < indices.Count; id++) {
						var i = indices[id];
						UnityEngine.Assertions.Assert.IsTrue(data[i].isValid);
						UnityEngine.Assertions.Assert.AreEqual(data[i].meta.hasher.Hash, hasher.Hash);
						data[i].meta.redrawScope2 = scope;
					}
					return true;
				} else {
					return false;
				}
			}

			public void ReleaseDataOlderThan (DrawingData gizmos, int version) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version < version) {
						Release(gizmos, i);
					}
				}
			}

			public void ReleaseAllWithHash (DrawingData gizmos, Hasher hasher) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.hasher.Hash == hasher.Hash) {
						Release(gizmos, i);
					}
				}
			}

			public void Dispose (DrawingData gizmos) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid) Release(gizmos, i);
					data[i].Dispose();
				}
				// Ensures calling Dispose multiple times is a NOOP
				data = null;
			}
		}

		[System.Flags]
		internal enum MeshType {
			Solid = 1 << 0,
			Lines = 1 << 1,
			Text = 1 << 2,
			// Set if the mesh is not a built-in mesh. These may have non-identity matrices set.
			Custom = 1 << 3,
			// If set for a custom mesh, the mesh will be pooled.
			// This is used for temporary custom meshes that are created by ALINE
			Pool = 1 << 4,
			BaseType = Solid | Lines | Text,
		}

		internal struct MeshWithType {
			public Mesh mesh;
			public MeshType type;
		}

		internal struct RenderedMeshWithType {
			public Mesh mesh;
			public MeshType type;
			public int drawingOrderIndex;
			// May only be set to non-white if type contains MeshType.Custom
			public Color color;
			// May only be set to a non-identity matrix if type contains MeshType.Custom
			public Matrix4x4 matrix;
		}

		internal BuilderDataContainer data;
		internal ProcessedBuilderDataContainer processedData;
		List<RenderedMeshWithType> meshes = new List<RenderedMeshWithType>();
		List<Mesh> cachedMeshes = new List<Mesh>();
		List<Mesh> stagingCachedMeshes = new List<Mesh>();
#if USE_RAW_GRAPHICS_BUFFERS
		List<Mesh> stagingCachedMeshesDelay = new List<Mesh>();
#endif
		int lastTimeLargestCachedMeshWasUsed = 0;
		internal SDFLookupData fontData;
		int currentDrawOrderIndex = 0;

		/// <summary>
		/// Incremented every time the editor goes from play mode -> edit mode, or edit mode -> play mode.
		/// Used to ensure that no WithDuration scopes survive this transition.
		///
		/// Normally it is not important, but when Unity's enter play mode settings have reload domain disabled
		/// then it can become important since this manager will survive the transition.
		/// </summary>
		internal int sceneModeVersion = 0;

		/// <summary>
		/// Slightly adjusted scene mode version.
		/// This takes into account `Application.isPlaying` too. It is possible for <see cref="sceneModeVersion"/> to be modified
		/// and then some gizmos are drawn before the actual play mode change happens (with the old Application.isPlaying) mode.
		///
		/// More precisely, what could happen without this adjustment is
		/// 1. EditorApplication.playModeStateChanged (PlayModeStateChange.ExitingPlayMode) fires which increments sceneModeVersion.
		/// 2. A final update loop runs with Application.isPlaying = true.
		/// 3. During this loop, a new command builder is created with the new sceneModeVersion and Application.isPlaying=true and is drawn to using a WithDuration scope.
		/// 4. The play mode changes to editor mode.
		/// 5. The WithDuration scope survives!
		///
		/// We cannot increment sceneModeVersion on PlayModeStateChange.ExitedPlayMode (not Exiting) instead, because some gizmos which we want to keep may
		/// be drawn before that event fires. Yay, Unity is so helpful.
		/// </summary>
		int adjustedSceneModeVersion {
			get {
				return sceneModeVersion + (Application.isPlaying ? 1000 : 0);
			}
		}

		internal int GetNextDrawOrderIndex () {
			currentDrawOrderIndex++;
			return currentDrawOrderIndex;
		}

		internal void PoolMesh (Mesh mesh) {
			// Note: clearing the mesh here will deallocate the vertex/index buffers
			// This is not good for performance as it will have to be allocated again (likely with the same size) in the next frame
			//mesh.Clear();
			stagingCachedMeshes.Add(mesh);
		}

		void SortPooledMeshes () {
			// TODO: Is accessing the vertex count slow?
			cachedMeshes.Sort((a, b) => b.vertexCount - a.vertexCount);
		}

		internal Mesh GetMesh (int desiredVertexCount) {
			if (cachedMeshes.Count > 0) {
				// Do a binary search to find the smallest cached mesh which is larger or equal to the desired vertex count
				// TODO: We should actually compare the byte size of the vertex buffer, not the number of vertices because
				// the vertex size can change depending on the mesh attribute layout.
				int mn = 0;
				int mx = cachedMeshes.Count;
				while (mx > mn + 1) {
					int mid = (mn+mx)/2;
					if (cachedMeshes[mid].vertexCount < desiredVertexCount) {
						mx = mid;
					} else {
						mn = mid;
					}
				}

				var res = cachedMeshes[mn];
				if (mn == 0) lastTimeLargestCachedMeshWasUsed = version;
				cachedMeshes.RemoveAt(mn);
				return res;
			} else {
				var mesh = new Mesh {
					hideFlags = HideFlags.DontSave
				};
				mesh.MarkDynamic();
				return mesh;
			}
		}

		internal void LoadFontDataIfNecessary () {
			if (fontData.material == null) {
				var font = DefaultFonts.LoadDefaultFont();
				fontData.Dispose();
				fontData = new SDFLookupData(font);
			}
		}

		static float CurrentTime {
			get {
				return Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
			}
		}

		static void UpdateTime () {
			// Time.time cannot be accessed in the job system, so create a global variable which *can* be accessed.
			// It's not updated as frequently, but it's only used for the WithDuration method, so it should be ok
			SharedDrawingData.BurstTime.Data = CurrentTime;
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// <code>
		/// // Create a new CommandBuilder
		/// using (var draw = DrawingManager.GetBuilder()) {
		///     // Use the exact same API as the global Draw class
		///     draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		/// </code>
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.
		/// If false, it will only be rendered in the editor when gizmos are enabled.</param>
		public CommandBuilder GetBuilder (bool renderInGame = false) {
			UpdateTime();
			return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, default, !renderInGame, false, adjustedSceneModeVersion);
		}

		internal CommandBuilder GetBuiltInBuilder (bool renderInGame = false) {
			UpdateTime();
			return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, default, !renderInGame, true, adjustedSceneModeVersion);
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.</param>
		public CommandBuilder GetBuilder (RedrawScope redrawScope, bool renderInGame = false) {
			UpdateTime();
			return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, redrawScope, !renderInGame, false, adjustedSceneModeVersion);
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.</param>
		public CommandBuilder GetBuilder (Hasher hasher, RedrawScope redrawScope = default, bool renderInGame = false) {
			// The user is going to rebuild the data with the given hash
			// Let's clear the previous data with that hash since we know it is not needed any longer.
			// Do not do this if a hash is not given.
			if (!hasher.Equals(Hasher.NotSupplied)) DiscardData(hasher);
			UpdateTime();
			return new CommandBuilder(this, hasher, frameRedrawScope, redrawScope, !renderInGame, false, adjustedSceneModeVersion);
		}

		/// <summary>Material to use for surfaces</summary>
		public Material surfaceMaterial;

		/// <summary>Material to use for lines</summary>
		public Material lineMaterial;

		/// <summary>Material to use for text</summary>
		public Material textMaterial;

		public DrawingSettings.Settings settings;

		public DrawingSettings.Settings settingsRef {
			get {
				if (settings == null) {
					settings = DrawingSettings.DefaultSettings;
				}

				return settings;
			}
		}

		public int version { get; private set; } = 1;
		int lastTickVersion;
		int lastTickVersion2;
		Dictionary<int, GameObject> persistentRedrawScopes = new Dictionary<int, GameObject>();
#if ALINE_TRACK_REDRAW_SCOPE_LEAKS
		Dictionary<int, String> persistentRedrawScopeInfos = new Dictionary<int, String>();
#endif
		internal System.Runtime.InteropServices.GCHandle gizmosHandle;

		public RedrawScope frameRedrawScope;

		public GameObject GetAssociatedGameObject (RedrawScope scope) {
			if (persistentRedrawScopes.TryGetValue(scope.id, out var go)) return go;
			return null;
		}

		struct Range {
			public int start;
			public int end;
		}

		Dictionary<Camera, Range> cameraVersions = new Dictionary<Camera, Range>();

		internal static readonly ProfilerMarker MarkerScheduleJobs = new ProfilerMarker("ScheduleJobs");
		internal static readonly ProfilerMarker MarkerAwaitUserDependencies = new ProfilerMarker("Await user dependencies");
		internal static readonly ProfilerMarker MarkerSchedule = new ProfilerMarker("Schedule");
		internal static readonly ProfilerMarker MarkerBuild = new ProfilerMarker("Build");
		internal static readonly ProfilerMarker MarkerPool = new ProfilerMarker("Pool");
		internal static readonly ProfilerMarker MarkerRelease = new ProfilerMarker("Release");
		internal static readonly ProfilerMarker MarkerBuildMeshes = new ProfilerMarker("Build Meshes");
		internal static readonly ProfilerMarker MarkerCollectMeshes = new ProfilerMarker("Collect Meshes");
		internal static readonly ProfilerMarker MarkerSortMeshes = new ProfilerMarker("Sort Meshes");
		internal static readonly ProfilerMarker LeakTracking = new ProfilerMarker("RedrawScope Leak Tracking");

		void DiscardData (Hasher hasher) {
			processedData.ReleaseAllWithHash(this, hasher);
		}

		internal void OnChangingPlayMode () {
			sceneModeVersion++;

#if UNITY_EDITOR
			// If we are in the editor, we schedule a callback to check if any RedrawScope objects were not disposed.
			// OnChangingPlayMode will run before the scene is destroyed. So we know that any persistent redraw scopes
			// that are alive right now should be destroyed soon.
			// We wait a few updates to allow the scene to be destroyed before we check for leaks.
			// EditorApplication.delayCall may be called before the scene has actually been destroyed.
			// Usually it has, but in particular if the user double-clicks the play button to start and then immediately
			// stop the game, then it may run before the scene has been destroyed.
			var shouldBeDestroyed = this.persistentRedrawScopes.Keys.ToArray();
			UnityEditor.EditorApplication.CallbackFunction checkLeaks = null;
			int remainingFrames = 2;
			checkLeaks = () => {
				if (remainingFrames > 0) {
					remainingFrames--;
					return;
				}
				UnityEditor.EditorApplication.delayCall -= checkLeaks;
				int leaked = 0;
				foreach (var v in shouldBeDestroyed) {
					if (persistentRedrawScopes.ContainsKey(v)) leaked++;
				}
				if (leaked > 0) {
#if ALINE_TRACK_REDRAW_SCOPE_LEAKS
					UnityEngine.Debug.LogError(leaked + " RedrawScope objects were not disposed. Make sure to dispose them when you are done with them, otherwise this will lead to a memory leak and potentially a performance issue.");
					foreach (var v in shouldBeDestroyed) {
						if (persistentRedrawScopes.ContainsKey(v)) {
							UnityEngine.Debug.LogError("RedrawScope leaked. Allocated from:\n" + persistentRedrawScopeInfos[v]);
						}
					}
#else
					UnityEngine.Debug.LogError(leaked + " RedrawScope objects were not disposed. Make sure to dispose them when you are done with them, otherwise this will lead to a memory leak and potentially a performance issue.\nEnable ALINE_TRACK_REDRAW_SCOPE_LEAKS in the scripting define symbols to track the leaks more accurately.");
#endif
					foreach (var v in shouldBeDestroyed) {
						persistentRedrawScopes.Remove(v);
#if ALINE_TRACK_REDRAW_SCOPE_LEAKS
						persistentRedrawScopeInfos.Remove(v);
#endif
					}
				}
			};
			UnityEditor.EditorApplication.delayCall += checkLeaks;
#endif
		}

		/// <summary>
		/// Schedules the meshes for the specified hash to be drawn.
		/// Returns: False if there is no cached mesh for this hash, you may want to
		///  submit one in that case. The draw command will be issued regardless of the return value.
		/// </summary>
		public bool Draw (Hasher hasher) {
			if (hasher.Equals(Hasher.NotSupplied)) throw new System.ArgumentException("Invalid hash value");
			return processedData.SetVersion(hasher, version);
		}

		/// <summary>
		/// Schedules the meshes for the specified hash to be drawn.
		/// Returns: False if there is no cached mesh for this hash, you may want to
		///  submit one in that case. The draw command will be issued regardless of the return value.
		///
		/// This overload will draw all meshes within the specified redraw scope.
		/// Note that if they had been drawn with another redraw scope earlier they will be removed from that scope.
		/// </summary>
		public bool Draw (Hasher hasher, RedrawScope scope) {
			if (hasher.Equals(Hasher.NotSupplied)) throw new System.ArgumentException("Invalid hash value");
			if (scope.isValid) processedData.SetCustomScope(hasher, scope);
			return processedData.SetVersion(hasher, version);
		}

		/// <summary>Schedules all meshes that were drawn the last frame with this redraw scope to be drawn again</summary>
		internal void Draw (RedrawScope scope) {
			if (scope.isValid) processedData.SetVersion(scope, version);
		}

		internal void DrawUntilDisposed (RedrawScope scope, GameObject associatedGameObject) {
			if (scope.isValid) {
				Draw(scope);
				persistentRedrawScopes.Add(scope.id, associatedGameObject);
#if ALINE_TRACK_REDRAW_SCOPE_LEAKS && UNITY_EDITOR
				LeakTracking.Begin();
				persistentRedrawScopeInfos[scope.id] = new System.Diagnostics.StackTrace().ToString();
				LeakTracking.End();
#endif
			}
		}

		internal void DisposeRedrawScope (RedrawScope scope) {
			if (scope.isValid) {
				processedData.SetVersion(scope, -1);
				persistentRedrawScopes.Remove(scope.id);
#if ALINE_TRACK_REDRAW_SCOPE_LEAKS && UNITY_EDITOR
				persistentRedrawScopeInfos.Remove(scope.id);
#endif
			}
		}

		void RefreshRedrawScopes () {
#if UNITY_EDITOR && UNITY_2020_1_OR_NEWER
			var currentStage = UnityEditor.SceneManagement.StageUtility.GetCurrentStage();
			var isInNonMainStage = currentStage != UnityEditor.SceneManagement.StageUtility.GetMainStage();
#endif
			foreach (var scope in persistentRedrawScopes) {
#if UNITY_EDITOR && UNITY_2020_1_OR_NEWER
				// True if the scene is in isolation mode (e.g. focusing on a single prefab) and this object is not part of that sub-stage
				var disabledDueToIsolationMode = isInNonMainStage && scope.Value != null && UnityEditor.SceneManagement.StageUtility.GetStage(scope.Value) != currentStage;
				if (disabledDueToIsolationMode) continue;
#endif
				processedData.SetVersion(new RedrawScope(this, scope.Key), version);
			}
		}

		void CleanupOldCameras () {
			// Remove cameras that have not been used for a while, to avoid memory leaks.
			// We keep them for a few frames for debugging purposes.
			foreach (var item in cameraVersions) {
				if (item.Value.end < lastTickVersion - 10) {
					cameraVersions.Remove(item.Key);
					// Break to avoid modifying the collection while iterating over it
					// In the rare case that multiple cameras needed to be removed, we can continue removing them next frame.
					break;
				}
			}
		}

		public void TickFramePreRender () {
			data.DisposeCommandBuildersWithJobDependencies(this);
			// Remove persistent commands that have timed out.
			// When not playing then persistent commands are never drawn twice
			processedData.FilterOldPersistentCommands(version, lastTickVersion, CurrentTime, adjustedSceneModeVersion);
			CleanupOldCameras();

			RefreshRedrawScopes();

			// All cameras rendered between the last tick and this one will have
			// a version that is at least lastTickVersion + 1.
			// However the user may want to reuse meshes from the previous frame (see Draw(Hasher)).
			// This requires us to keep data from one more frame and thus we use lastTickVersion2 + 1
			// TODO: One frame should be enough, right?
			processedData.ReleaseDataOlderThan(this, lastTickVersion2 + 1);
			lastTickVersion2 = lastTickVersion;
			lastTickVersion = version;
			currentDrawOrderIndex = 0;

			// Pooled meshes from two frames ago can now be used.
#if USE_RAW_GRAPHICS_BUFFERS
			// One would think that pooled meshes from only one frame ago can be used.
			// And yes, Unity will allow this, but the GPU may still be working on the meshes from the previous frame.
			// Therefore, when we try to write to the raw mesh vertex buffers Unity will block until the previous
			// frame's GPU work is done, which may take a long time.
			// Using "double buffering" for the meshes that are updated every frame is more efficient.
			// When we use simplified methods for setting the vertex/index data we don't have to do this
			// because Unity seems to manage an upload buffer or something for us.
			cachedMeshes.AddRange(stagingCachedMeshesDelay);
			// Move stagingCachedMeshes to stagingCachedMeshesDelay, and make stagingCachedMeshes an empty list.
			stagingCachedMeshesDelay.Clear();
			var tmp = stagingCachedMeshesDelay;
			stagingCachedMeshesDelay = stagingCachedMeshes;
			stagingCachedMeshes = tmp;
#else
			cachedMeshes.AddRange(stagingCachedMeshes);
			stagingCachedMeshes.Clear();
#endif
			SortPooledMeshes();

			// If the largest cached mesh hasn't been used in a while, then remove it to free up the memory
			if (version - lastTimeLargestCachedMeshWasUsed > 60 && cachedMeshes.Count > 0) {
				Mesh.DestroyImmediate(cachedMeshes[0]);
				cachedMeshes.RemoveAt(0);
				lastTimeLargestCachedMeshWasUsed = version;
			}

			// TODO: Filter cameraVersions to avoid memory leak
		}

		public void PostRenderCleanup () {
			MarkerRelease.Begin();
			data.ReleaseAllUnused();
			MarkerRelease.End();
			version++;
		}

		static int MeshCompareByDrawingOrder (RenderedMeshWithType a, RenderedMeshWithType b) {
			// Extract if the meshes are Solid/Lines/Text
			var ta = (int)a.type & 0x7;
			var tb = (int)b.type & 0x7;
			return ta != tb ? ta - tb : a.drawingOrderIndex - b.drawingOrderIndex;
		}

		static readonly System.Comparison<RenderedMeshWithType> meshSorter = MeshCompareByDrawingOrder;

		// Temporary array, cached to avoid allocations
		Plane[] frustrumPlanes = new Plane[6];
		// Temporary block, cached to avoid allocations
		MaterialPropertyBlock customMaterialProperties = new MaterialPropertyBlock();

		int totalMemoryUsage => this.data.memoryUsage + this.processedData.memoryUsage;

		void LoadMaterials () {
			// Make sure the material references are correct

			// Note: When importing the package for the first time the asset database may not be up to date and Resources.Load may return null.

			if (surfaceMaterial == null) {
				surfaceMaterial = Resources.Load<Material>("aline_surface");
			}
			if (lineMaterial == null) {
				lineMaterial = Resources.Load<Material>("aline_outline");
			}
			if (fontData.material == null) {
				var font = DefaultFonts.LoadDefaultFont();
				fontData.Dispose();
				fontData = new SDFLookupData(font);
			}
		}

		public DrawingData() {
			gizmosHandle = System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.Weak);
			LoadMaterials();
		}

		static int CeilLog2 (int x) {
			// Should use `math.ceillog2` whenever we next raise the minimum compatible version of the mathematics package.
			// This variant is prone to floating point errors.
			return (int)math.ceil(math.log2(x));
		}

		/// <summary>
		/// Wrapper for different kinds of commands buffers.
		///
		/// Annoyingly, they all use a CommandBuffer in the end, but the universal render pipeline wraps it in a RasterCommandBuffer,
		/// and it's not possible to get the underlaying CommandBuffer.
		/// </summary>
		public struct CommandBufferWrapper {
			public CommandBuffer cmd;
#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
			public bool allowDisablingWireframe;
			public RasterCommandBuffer cmd2;
#endif

#if UNITY_2023_1_OR_NEWER
			public void SetWireframe (bool enable) {
				if (cmd != null) {
					cmd.SetWireframe(enable);
				}
#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
				else if (cmd2 != null) {
					if (allowDisablingWireframe) cmd2.SetWireframe(enable);
				}
#endif
			}
#endif

			public void DrawMesh (Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass, MaterialPropertyBlock properties) {
				if (cmd != null) {
					cmd.DrawMesh(mesh, matrix, material, submeshIndex, shaderPass, properties);
				}
#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
				else if (cmd2 != null) {
					cmd2.DrawMesh(mesh, matrix, material, submeshIndex, shaderPass, properties);
				}
#endif
			}
		}

		/// <summary>Call after all <see cref="Draw"/> commands for the frame have been done to draw everything.</summary>
		/// <param name="allowCameraDefault">Indicates if built-in command builders and custom ones without a custom CommandBuilder.cameraTargets should render to this camera.</param>
		public void Render (Camera cam, bool allowGizmos, CommandBufferWrapper commandBuffer, bool allowCameraDefault) {
			// Early out when there's nothing to render
			if (processedData.isEmpty) return;

			LoadMaterials();

			// Warn if the materials could not be found
			if (surfaceMaterial == null || lineMaterial == null) {
				// Note that when the package is installed Unity may start rendering things and call this method before it has initialized the Resources folder with the materials.
				// We don't want to throw exceptions in that case because once the import finishes everything will be good.
				// UnityEngine.Debug.LogWarning("Looks like you just installed ALINE. The ALINE package will start working after the next script recompilation.");
				return;
			}

			if (!cameraVersions.TryGetValue(cam, out Range cameraRenderingRange)) {
				cameraRenderingRange = new Range { start = int.MinValue, end = int.MinValue };
			}

			// Check if the last time the camera was rendered
			// was during the current frame.
			if (cameraRenderingRange.end > lastTickVersion) {
				// In some cases a camera is rendered multiple times per frame.
				// In this case we just extend the end of the drawing range up to the current version.
				// The reasoning is that all times the camera is rendered in a frame
				// all things should be drawn.
				// If we did update the start of the range then things would only be drawn
				// the first time the camera was rendered in the frame.

				// Sometimes the scene view will be rendered twice in a single frame
				// due to some internal Unity tooltip code.
				// Without this fix the scene view camera may end up showing no gizmos
				// for a single frame.
				cameraRenderingRange.end = version + 1;
			} else {
				// This is the common case: the previous time the camera was rendered
				// it rendered all versions lower than cameraRenderingRange.end.
				// So now we start by rendering from that version.
				cameraRenderingRange = new Range  { start = cameraRenderingRange.end, end = version + 1 };
			}

			// Don't show anything rendered before the last frame.
			// If the camera has been turned off for a while and then suddenly starts rendering again
			// we want to make sure that we don't render meshes from multiple frames.
			// This happens often in the unity editor as the scene view and game view often skip
			// rendering many frames when outside of play mode.
			cameraRenderingRange.start = Mathf.Max(cameraRenderingRange.start, lastTickVersion2 + 1);

			var settings = settingsRef;

#if UNITY_2023_1_OR_NEWER
			bool skipDueToWireframe = false;
			commandBuffer.SetWireframe(false);
#else
			// If GL.wireframe is enabled (the Wireframe mode in the scene view settings)
			// then I have found no way to draw gizmos in a good way.
			// It's best to disable gizmos altogether to avoid drawing wireframe versions of gizmo meshes.
			bool skipDueToWireframe = GL.wireframe;
#endif

			if (!skipDueToWireframe) {
				MarkerBuildMeshes.Begin();
				processedData.SubmitMeshes(this, cam, cameraRenderingRange.start, allowGizmos, allowCameraDefault);
				MarkerBuildMeshes.End();
				MarkerCollectMeshes.Begin();
				meshes.Clear();
				processedData.CollectMeshes(cameraRenderingRange.start, meshes, cam, allowGizmos, allowCameraDefault);
				processedData.PoolDynamicMeshes(this);
				MarkerCollectMeshes.End();

				// Early out if nothing is being rendered
				if (meshes.Count > 0) {
					MarkerSortMeshes.Begin();
					// Note that a stable sort is required as some meshes may have the same sorting index
					// but those meshes will have a consistent ordering between them in the list
					meshes.Sort(meshSorter);
					MarkerSortMeshes.End();

					var planes = frustrumPlanes;
					GeometryUtility.CalculateFrustumPlanes(cam, planes);

					int colorID = Shader.PropertyToID("_Color");
					int colorFadeID = Shader.PropertyToID("_FadeColor");
					var solidBaseColor = new Color(1, 1, 1, settings.solidOpacity);
					var solidFadeColor = new Color(1, 1, 1, settings.solidOpacityBehindObjects);
					var lineBaseColor = new Color(1, 1, 1, settings.lineOpacity);
					var lineFadeColor = new Color(1, 1, 1, settings.lineOpacityBehindObjects);
					var textBaseColor = new Color(1, 1, 1, settings.textOpacity);
					var textFadeColor = new Color(1, 1, 1, settings.textOpacityBehindObjects);

					// The meshes list is already sorted as first surfaces, then lines, then text
					for (int i = 0; i < meshes.Count;) {
						int meshEndIndex = i+1;
						var tp = meshes[i].type & MeshType.BaseType;
						while (meshEndIndex < meshes.Count && (meshes[meshEndIndex].type & MeshType.BaseType) == tp) meshEndIndex++;

						Material mat;
						customMaterialProperties.Clear();
						switch (tp) {
						case MeshType.Solid:
							mat = surfaceMaterial;
							customMaterialProperties.SetColor(colorID, solidBaseColor);
							customMaterialProperties.SetColor(colorFadeID, solidFadeColor);
							break;
						case MeshType.Lines:
							mat = lineMaterial;
							customMaterialProperties.SetColor(colorID, lineBaseColor);
							customMaterialProperties.SetColor(colorFadeID, lineFadeColor);
							break;
						case MeshType.Text:
							mat = fontData.material;
							customMaterialProperties.SetColor(colorID, textBaseColor);
							customMaterialProperties.SetColor(colorFadeID, textFadeColor);
							break;
						default:
							throw new System.InvalidOperationException("Invalid mesh type");
						}

						for (int pass = 0; pass < mat.passCount; pass++) {
							for (int j = i; j < meshEndIndex; j++) {
								var mesh = meshes[j];
								if ((mesh.type & MeshType.Custom) != 0) {
									// This mesh type may have a matrix set. So we need to handle that
									if (GeometryUtility.TestPlanesAABB(planes, TransformBoundingBox(mesh.matrix, mesh.mesh.bounds))) {
										// Custom meshes may have different colors
										customMaterialProperties.SetColor(colorID, solidBaseColor * mesh.color);
										commandBuffer.DrawMesh(mesh.mesh, mesh.matrix, mat, 0, pass, customMaterialProperties);
										customMaterialProperties.SetColor(colorID, solidBaseColor);
									}
								} else if (GeometryUtility.TestPlanesAABB(planes, mesh.mesh.bounds)) {
									// This mesh is drawn with an identity matrix
									commandBuffer.DrawMesh(mesh.mesh, Matrix4x4.identity, mat, 0, pass, customMaterialProperties);
								}
							}
						}

						i = meshEndIndex;
					}

					meshes.Clear();
				}
			}

			cameraVersions[cam] = cameraRenderingRange;
		}

		/// <summary>Returns a new axis aligned bounding box that contains the given bounding box after being transformed by the matrix</summary>
		static Bounds TransformBoundingBox (Matrix4x4 matrix, Bounds bounds) {
			var mn = bounds.min;
			var mx = bounds.max;
			// Create the bounding box from the bounding box of the transformed
			// 8 points of the original bounding box.
			var newBounds = new Bounds(matrix.MultiplyPoint(mn), Vector3.zero);

			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mn.x, mn.y, mx.z)));

			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mn.x, mx.y, mn.z)));
			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mn.x, mx.y, mx.z)));

			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mx.x, mn.y, mn.z)));
			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mx.x, mn.y, mx.z)));

			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mx.x, mx.y, mn.z)));
			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mx.x, mx.y, mx.z)));
			return newBounds;
		}

		/// <summary>
		/// Destroys all cached meshes.
		/// Used to make sure that no memory leaks happen in the Unity Editor.
		/// </summary>
		public void ClearData () {
			gizmosHandle.Free();
			data.Dispose();
			processedData.Dispose(this);

			for (int i = 0; i < cachedMeshes.Count; i++) {
				Mesh.DestroyImmediate(cachedMeshes[i]);
			}
			cachedMeshes.Clear();

			UnityEngine.Assertions.Assert.IsTrue(meshes.Count == 0);
			fontData.Dispose();
		}
	}
}
