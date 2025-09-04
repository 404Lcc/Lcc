using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {
	using Pathfinding.Drawing;
	using Pathfinding.Graphs.Navmesh;
	using Pathfinding.Jobs;
	using Pathfinding.Serialization;
	using Pathfinding.Sync;
	using Pathfinding.Pooling;
	using Unity.Jobs;

	/// <summary>
	/// Stores a set of navmesh tiles which can be placed on a recast graph.
	///
	/// This component is used to store chunks of a <see cref="RecastGraph"/> to a file and then be able to efficiently load them and place them on an existing recast graph.
	/// A typical use case is if you have a procedurally generated level consisting of multiple rooms, and scanning the graph after the level has been generated
	/// is too expensive. In this scenario, each room can have its own NavmeshPrefab component which stores the navmesh for just that room, and then when the
	/// level is generated all the NavmeshPrefab components will load their tiles and place them on the recast graph, joining them together at the seams.
	///
	/// Since this component works on tiles, the size of a NavmeshPrefab must be a multiple of the graph's tile size.
	/// The tile size of a recast graph is determined by multiplying the <see cref="RecastGraph.cellSize"/> with the tile size in voxels (<see cref="RecastGraph.editorTileSize"/>).
	/// When a NavmeshPrefab is placed on a recast graph, it will load the tiles into the closest spot (snapping the position and rotation).
	/// The NavmeshPrefab will even resize the graph to make it larger in case you want to place a NavmeshPrefab outside the existing bounds of the graph.
	///
	/// <b>Usage</b>
	///
	/// - Attach a NavmeshPrefab component to a GameObject (typically a prefab) that you want to store the navmesh for.
	/// - Make sure you have a RecastGraph elsewhere in the scene with the same settings that you use for the game.
	/// - Adjust the bounding box to fit your game object. The bounding box should be a multiple of the tile size of the recast graph.
	/// - In the inspector, click the "Scan" button to scan the graph and store the navmesh as a file, referenced by the NavmeshPrefab component.
	/// - Make sure the rendered navmesh looks ok in the scene view.
	/// - In your game, instantiate a prefab with the NavmeshComponent. It will automatically load its stored tiles and place them on the first recast graph in the scene.
	///
	/// If you have multiple recast graphs you may not want it to always use the first recast graph.
	/// In that case you can set the <see cref="applyOnStart"/> field to false and call the <see cref="Apply(RecastGraph)"/> method manually.
	///
	/// <b>Accounting for borders</b>
	///
	/// When scanning a recast graph (and by extension a NavmeshPrefab), a margin is always added around parts of the graph the agent cannot traverse.
	/// This can become problematic when scanning individual chunks separate from the rest of the world, because each one will have a small border of unwalkable space.
	/// The result is that when you place them on a recast graph, they will not be able to connect to each other.
	/// [Open online documentation to see images]
	/// One way to solve this is to scan the prefab together with a mesh that is slightly larger than the prefab, extending the walkable surface enough
	/// so that no border is added. In the image below, this mesh is displayed in white. It can be convenient to make this an invisible collider on the prefab
	/// that is excluded from physics, but is included in the graph's rasterization layer mask.
	/// [Open online documentation to see images]
	/// Now that the border has been removed, the chunks can be placed next to each other and be able to connect.
	/// [Open online documentation to see images]
	///
	/// <b>Loading tiles into a graph</b>
	///
	/// If <see cref="applyOnStart"/> is true, the tiles will be loaded into the first recast graph in the scene when the game starts.
	/// If the recast graph is not scanned, it will be initialized with empty tiles and then the tiles will be loaded into it.
	/// So if your world is made up entirely of NavmeshPrefabs, you can skip scanning for performance by setting A* Inspector -> Settings -> Scan On Awake to false.
	///
	/// You can also apply a NavmeshPrefab to a graph manually by calling the <see cref="Apply(RecastGraph)"/> method.
	///
	/// Note: A navmesh prefab will fully replace the tiles within its bounding box. You cannot stack multiple navmesh prefabs on top of each other
	/// to make e.g. a building with multiple floors.
	///
	/// See: <see cref="RecastGraph"/>
	/// See: <see cref="TileMeshes"/>
	/// </summary>
	[AddComponentMenu("Pathfinding/Navmesh Prefab")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/navmeshprefab.html")]
	public class NavmeshPrefab : VersionedMonoBehaviour {
		/// <summary>Reference to the serialized tile data</summary>
		public TextAsset serializedNavmesh;

		/// <summary>
		/// If true, the tiles stored in this prefab will be loaded and applied to the first recast graph in the scene when this component is enabled.
		/// If false, you will have to call the <see cref="Apply(RecastGraph)"/> method manually.
		///
		/// If this component is disabled and then enabled again, the tiles will be reloaded.
		/// </summary>
		public bool applyOnStart = true;

		/// <summary>
		/// If true, the tiles that this prefab loaded into the graph will be removed when this component is disabled or destroyed.
		/// If false, the tiles will remain in the graph.
		/// </summary>
		public bool removeTilesWhenDisabled = true;

		/// <summary>
		/// Bounding box for the navmesh to be stored in this prefab.
		/// Should be a multiple of the tile size of the associated recast graph.
		///
		/// See:
		/// See: <see cref="RecastGraph.TileWorldSizeX"/>
		/// </summary>
		public Bounds bounds = new Bounds(Vector3.zero, new Vector3(10, 10, 10));

		bool startHasRun = false;

		protected override void Reset () {
			base.Reset();
			AstarPath.FindAstarPath();
			if (AstarPath.active != null && AstarPath.active.data.recastGraph != null) {
				var graph = AstarPath.active.data.recastGraph;
				// Make the default bounds be 1x1 tiles in the graph
				bounds = new Bounds(Vector3.zero, new Vector3(graph.TileWorldSizeX, graph.forcedBoundsSize.y, graph.TileWorldSizeZ));
			}
		}

#if UNITY_EDITOR
		public override void DrawGizmos () {
			using (Draw.WithMatrix(Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one))) {
				Draw.WireBox(bounds.center, bounds.size);
			}

			if (!Application.isPlaying && serializedNavmesh != null) {
				var path = UnityEditor.AssetDatabase.GetAssetPath(serializedNavmesh);
				var lastEditTime = System.IO.File.GetLastWriteTimeUtc(Application.dataPath + "/../" + path);
				lastEditTime.ToBinary();
				// Hash the metadata to avoid somewhat expensive deserialization and drawing every frame.
				var hasher = new Pathfinding.Drawing.DrawingData.Hasher();
				hasher.Add(lastEditTime);
				hasher.Add(transform.position);
				hasher.Add(transform.rotation);
				hasher.Add(bounds);

				// Draw a new mesh if the metadata has changed
				if (!Pathfinding.Drawing.DrawingManager.instance.gizmos.Draw(hasher)) {
					var builder = Pathfinding.Drawing.DrawingManager.GetBuilder(hasher);

					var tileMeshes = TileMeshes.Deserialize(serializedNavmesh.bytes);

					var center = transform.position + transform.rotation * bounds.center;
					var corner = center - transform.rotation*bounds.extents;
					var tileWorldSize = tileMeshes.tileWorldSize;
					var graphToWorldSpace = Matrix4x4.TRS(corner, transform.rotation, Vector3.one);

					var vertexCount = 0;
					var trisCount = 0;
					for (int i = 0; i < tileMeshes.tileMeshes.Length; i++) {
						vertexCount += tileMeshes.tileMeshes[i].verticesInTileSpace.Length;
						trisCount += tileMeshes.tileMeshes[i].triangles.Length;
					}

					var colors = ArrayPool<Color>.Claim(vertexCount);
					var vertices = ArrayPool<Vector3>.Claim(vertexCount);
					var triangles = ArrayPool<int>.Claim(trisCount);
					vertexCount = 0;
					trisCount = 0;

					using (builder.WithColor(AstarColor.SolidColor)) {
						for (int z = 0; z < tileMeshes.tileRect.Height; z++) {
							for (int x = 0; x < tileMeshes.tileRect.Width; x++) {
								var tile = tileMeshes.tileMeshes[x + z*tileMeshes.tileRect.Width];

								var tileToWorldSpace = graphToWorldSpace * Matrix4x4.Translate(new Vector3(x * tileWorldSize.x, 0, z * tileWorldSize.y));
								var startVertex = vertexCount;
								for (int j = 0; j < tile.triangles.Length; trisCount++, j++) {
									triangles[trisCount] = tile.triangles[j] + startVertex;
								}
								for (int j = 0; j < tile.verticesInTileSpace.Length; vertexCount++, j++) {
									colors[vertexCount] = AstarColor.SolidColor;
									vertices[vertexCount] = tileToWorldSpace.MultiplyPoint3x4((Vector3)tile.verticesInTileSpace[j]);
								}

								for (int i = 0; i < tile.triangles.Length; i += 3) {
									builder.Line(vertices[startVertex + tile.triangles[i+0]], vertices[startVertex + tile.triangles[i+1]]);
									builder.Line(vertices[startVertex + tile.triangles[i+1]], vertices[startVertex + tile.triangles[i+2]]);
									builder.Line(vertices[startVertex + tile.triangles[i+2]], vertices[startVertex + tile.triangles[i+0]]);
								}
							}
						}
					}

					builder.SolidMesh(vertices, triangles, colors, vertexCount, trisCount);
					ArrayPool<Color>.Release(ref colors);
					ArrayPool<Vector3>.Release(ref vertices);
					ArrayPool<int>.Release(ref triangles);

					builder.Dispose();
				}
			}
		}
#endif

		/// <summary>
		/// Moves and rotates this object so that it is aligned with tiles in the first recast graph in the scene
		///
		/// See: SnapToClosestTileAlignment(RecastGraph)
		/// </summary>
		[ContextMenu("Snap to closest tile alignment")]
		public void SnapToClosestTileAlignment () {
			AstarPath.FindAstarPath();
			if (AstarPath.active != null && AstarPath.active.data.recastGraph != null) {
				SnapToClosestTileAlignment(AstarPath.active.data.recastGraph);
			}
		}

		/// <summary>
		/// Applies the navmesh stored in this prefab to the first recast graph in the scene.
		///
		/// See: <see cref="Apply(RecastGraph)"/> for more details.
		/// </summary>
		[ContextMenu("Apply here")]
		public void Apply () {
			AstarPath.FindAstarPath();
			if (AstarPath.active != null && AstarPath.active.data.recastGraph != null) {
				var graph = AstarPath.active.data.recastGraph;
				Apply(graph);
			}
		}

		/// <summary>Moves and rotates this object so that it is aligned with tiles in the given graph</summary>
		public void SnapToClosestTileAlignment (RecastGraph graph) {
			// Calculate a new tile layout, because the graph may not be scanned yet (especially if this code runs outside of play mode)
			var tileLayout = new TileLayout(graph);
			SnapToGraph(tileLayout, transform.position, transform.rotation, bounds, out IntRect tileRect, out int snappedRotation, out float yOffset);
			var graphSpaceBounds = tileLayout.GetTileBoundsInGraphSpace(tileRect.xmin, tileRect.ymin, tileRect.Width, tileRect.Height);
			var centerInGraphSpace = new Vector3(graphSpaceBounds.center.x, yOffset, graphSpaceBounds.center.z);
#if UNITY_EDITOR
			if (!Application.isPlaying) UnityEditor.Undo.RecordObject(transform, "Snap to closest tile alignment");
#endif
			transform.rotation = Quaternion.Euler(graph.rotation) * Quaternion.Euler(0, snappedRotation * 90, 0);
			transform.position = tileLayout.transform.Transform(centerInGraphSpace) + transform.rotation*(-bounds.center + new Vector3(0, bounds.extents.y, 0));

#if UNITY_EDITOR
			if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(transform);
#endif
		}

		/// <summary>
		/// Rounds the size of the <see cref="bounds"/> to the closest multiple of the tile size in the graph, ensuring that the bounds cover at least 1x1 tiles.
		/// The new bounds has the same center and size along the y-axis.
		/// </summary>
		public void SnapSizeToClosestTileMultiple (RecastGraph graph) {
			this.bounds = SnapSizeToClosestTileMultiple(graph, this.bounds);
		}

		/// <summary>Start is called before the first frame update</summary>
		void Start () {
			startHasRun = true;
			if (applyOnStart && serializedNavmesh != null && AstarPath.active != null && AstarPath.active.data.recastGraph != null) Apply(AstarPath.active.data.recastGraph);
		}

		void OnEnable () {
			if (startHasRun && applyOnStart && serializedNavmesh != null && AstarPath.active != null && AstarPath.active.data.recastGraph != null) Apply(AstarPath.active.data.recastGraph);
		}

		void OnDisable () {
			if (removeTilesWhenDisabled && serializedNavmesh != null && AstarPath.active != null) {
				var pos = transform.position;
				var rot = transform.rotation;
				AstarPath.active.AddWorkItem(ctx => {
					var graph = AstarPath.active.data.recastGraph;
					if (graph != null) {
						SnapToGraph(new TileLayout(graph), pos, rot, bounds, out IntRect tileRect, out int rotation, out float yOffset);
						graph.ClearTiles(tileRect);
					}
				});
			}
		}

		/// <summary>
		/// Rounds the size of the bounds to the closest multiple of the tile size in the graph, ensuring that the bounds cover at least 1x1 tiles.
		/// The returned bounds has the same center and size along the y-axis as the input.
		/// </summary>
		public static Bounds SnapSizeToClosestTileMultiple (RecastGraph graph, Bounds bounds) {
			var tileSize = Mathf.Max(graph.editorTileSize * graph.cellSize, 0.001f);
			var tiles = new Vector2(bounds.size.x / tileSize, bounds.size.z / tileSize);
			var roundedTiles = new Vector2Int(Mathf.Max(1, Mathf.RoundToInt(tiles.x)), Mathf.Max(1, Mathf.RoundToInt(tiles.y)));
			return new Bounds(
				bounds.center,
				new Vector3(
					roundedTiles.x * tileSize,
					bounds.size.y,
					roundedTiles.y * tileSize
					)
				);
		}

		public static void SnapToGraph (TileLayout tileLayout, Vector3 position, Quaternion rotation, Bounds bounds, out IntRect tileRect, out int snappedRotation, out float yOffset) {
			var rotInGraphSpace = tileLayout.transform.InverseTransformVector(rotation * Vector3.right);
			// Snap to increments of 90 degrees
			snappedRotation = -Mathf.RoundToInt(Mathf.Atan2(rotInGraphSpace.z, rotInGraphSpace.x) / (0.5f*Mathf.PI));
			var snappedRotationQ = Quaternion.Euler(0, snappedRotation * 90, 0);
			var localToGraph = tileLayout.transform.inverseMatrix * Matrix4x4.TRS(position + snappedRotationQ * bounds.center, snappedRotationQ, Vector3.one);
			var cornerInGraphSpace1 = localToGraph.MultiplyPoint3x4(-bounds.extents);
			var cornerInGraphSpace2 = localToGraph.MultiplyPoint3x4(bounds.extents);
			var minInGraphSpace = Vector3.Min(cornerInGraphSpace1, cornerInGraphSpace2);
			var tileCoordinatesF = Vector3.Scale(minInGraphSpace, new Vector3(1.0f/tileLayout.TileWorldSizeX, 1, 1.0f/tileLayout.TileWorldSizeZ));
			var tileCoordinates = new Vector2Int(Mathf.RoundToInt(tileCoordinatesF.x), Mathf.RoundToInt(tileCoordinatesF.z));
			var boundsSizeInGraphSpace = new Vector2(bounds.size.x, bounds.size.z);
			if (((snappedRotation % 2) + 2) % 2 == 1) Util.Memory.Swap(ref boundsSizeInGraphSpace.x, ref boundsSizeInGraphSpace.y);
			var w = Mathf.Max(1, Mathf.RoundToInt(boundsSizeInGraphSpace.x / tileLayout.TileWorldSizeX));
			var h = Mathf.Max(1, Mathf.RoundToInt(boundsSizeInGraphSpace.y / tileLayout.TileWorldSizeZ));
			tileRect = new IntRect(
				tileCoordinates.x,
				tileCoordinates.y,
				tileCoordinates.x + w - 1,
				tileCoordinates.y + h - 1
				);

			yOffset = minInGraphSpace.y;
		}

		/// <summary>
		/// Applies the navmesh stored in this prefab to the given graph.
		/// The loaded tiles will be placed at the closest valid spot to this object's current position.
		/// Some rounding may occur because the tiles need to be aligned to the graph's tile boundaries.
		///
		/// If the recast graph is not scanned, it will be initialized with empty tiles and then the tiles in this prefab will be loaded into it.
		///
		/// If the recast graph is too small and the tiles would have been loaded out of bounds, the graph will first be resized to fit.
		/// If you have a large graph, this resizing can be a somewhat expensive operation.
		///
		/// See: <see cref="NavmeshPrefab.SnapToClosestTileAlignment()"/>
		/// </summary>
		public void Apply (RecastGraph graph) {
			if (serializedNavmesh == null) throw new System.InvalidOperationException("Cannot Apply NavmeshPrefab because no serialized data has been set");

			AstarPath.active.AddWorkItem(() => {
				UnityEngine.Profiling.Profiler.BeginSample("NavmeshPrefab.Apply");
				SnapToGraph(new TileLayout(graph), transform.position, transform.rotation, bounds, out IntRect tileRect, out int rotation, out float yOffset);

				var tileMeshes = TileMeshes.Deserialize(serializedNavmesh.bytes);
				tileMeshes.Rotate(rotation);
				if (tileMeshes.tileRect.Width != tileRect.Width || tileMeshes.tileRect.Height != tileRect.Height) {
					throw new System.Exception("NavmeshPrefab has been scanned with a different size than it is right now (or with a different graph). Expected to find " + tileRect.Width + "x" + tileRect.Height + " tiles, but found " + tileMeshes.tileRect.Width + "x" + tileMeshes.tileRect.Height);
				}
				tileMeshes.tileRect = tileRect;
				graph.ReplaceTiles(tileMeshes, yOffset);
				UnityEngine.Profiling.Profiler.EndSample();
			});
		}

		/// <summary>Scans the navmesh using the first recast graph in the scene, and returns a serialized byte representation</summary>
		public byte[] Scan () {
			// Make sure this method works even when called in the editor outside of play mode.
			AstarPath.FindAstarPath();
			if (AstarPath.active == null || AstarPath.active.data.recastGraph == null) throw new System.InvalidOperationException("There's no recast graph in the scene. Add one if you want to scan this navmesh prefab.");
			return Scan(AstarPath.active.data.recastGraph);
		}

		/// <summary>Scans the navmesh and returns a serialized byte representation</summary>
		public byte[] Scan (RecastGraph graph) {
			// Schedule the jobs asynchronously, but immediately wait for them to finish
			var result = ScanAsync(graph).Complete();
			var data = result.data;
			// Dispose of all the unmanaged memory
			result.Dispose();
			return data;
		}

		/// <summary>
		/// Scans the navmesh asynchronously and returns a promise of a byte representation.
		///
		/// TODO: Maybe change this method to return a <see cref="TileMeshes"/> object instead?
		/// </summary>
		public Promise<SerializedOutput> ScanAsync (RecastGraph graph) {
			var arena = new DisposeArena();

			// First configure the rasterization settings by copying them from the recast graph,
			// but changing which region we are interested in.
			var tileLayout = new TileLayout(
				new Bounds(transform.position + transform.rotation * bounds.center, bounds.size),
				transform.rotation,
				graph.cellSize,
				graph.editorTileSize,
				graph.useTiles
				);
			// Disable cropping to the graph's exact bounds, since this can lead to a 1 voxel border on the +X and +Z edges of the prefab,
			// due to the cropping being conservative, to ensure the nodes are strictly inside the bounds.
			tileLayout.graphSpaceSize.x = float.PositiveInfinity;
			tileLayout.graphSpaceSize.z = float.PositiveInfinity;
			var buildSettings = RecastBuilder.BuildTileMeshes(graph, tileLayout, new IntRect(0, 0, tileLayout.tileCount.x - 1, tileLayout.tileCount.y - 1));
			var scene = this.gameObject.scene;
			buildSettings.collectionSettings.physicsScene = scene.GetPhysicsScene();
			buildSettings.collectionSettings.physicsScene2D = scene.GetPhysicsScene2D();

			// Schedule the jobs asynchronously
			var tileMeshesPromise = buildSettings.Schedule(arena);
			var output = new SerializedOutput {
				promise = tileMeshesPromise,
				arena = arena,
			};
			var serializeJob = new SerializeJob {
				tileMeshesPromise = tileMeshesPromise,
				output = output,
			}.ScheduleManaged(tileMeshesPromise.handle);

			return new Promise<SerializedOutput>(serializeJob, output);
		}

		public class SerializedOutput : IProgress, System.IDisposable {
			public Promise<TileBuilder.TileBuilderOutput> promise;
			public byte[] data;
			public DisposeArena arena;

			public float Progress => promise.Progress;

			public void Dispose () {
				// Dispose of all the unmanaged memory
				promise.Dispose();
				arena.DisposeAll();
			}
		}

		struct SerializeJob : IJob {
			public Promise<TileBuilder.TileBuilderOutput> tileMeshesPromise;
			public SerializedOutput output;

			public void Execute () {
				// Note: Assumes that the tileMeshesPromise has already completed
				var tileMeshes = tileMeshesPromise.GetValue();
				// Serialize the data to a byte array
				output.data = tileMeshes.tileMeshes.ToManaged().Serialize();
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// Saves the given data to the <see cref="serializedNavmesh"/> field, or creates a new file if none exists.
		///
		/// A new file will be created if <see cref="serializedNavmesh"/> is null.
		/// If this object is part of a prefab, the file name will be based on the prefab's name.
		///
		/// Warning: This method is only available in the editor.
		///
		/// Warning: You should only pass valid serialized tile data to this function.
		///
		/// See: <see cref="Scan"/>
		/// See: <see cref="ScanAsync"/>
		/// </summary>
		public void SaveToFile (byte[] data) {
			string path;
			if (serializedNavmesh != null) {
				// If we already have a file, just overwrite it
				path = UnityEditor.AssetDatabase.GetAssetPath(serializedNavmesh);
			} else {
				// Otherwise create a new file.
				// If this is a prefab, base the name on the prefab's name.
				System.IO.Directory.CreateDirectory(Application.dataPath + "/Tiles");
				var name = "tiles";
				var prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this);
				if (prefabPath != null && prefabPath != "") {
					name = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
				}
				name = name.Replace("/", "_").Replace("\\", "_").Replace(".", "_").Replace("__", "_");
				path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Tiles/" + name + ".bytes");
			}
			var fullPath = Application.dataPath + "/../" + path;
			WriteFileSomewhatAtomic(fullPath, data);

			UnityEditor.AssetDatabase.Refresh();
			serializedNavmesh = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
			// Required if we do this in edit mode
			UnityEditor.EditorUtility.SetDirty(this);
		}

		static void WriteFileSomewhatAtomic (string path, byte[] data) {
			// Ensure the temp path is likely on the same disk as the final path.
			// This is not necessarily true for the system wide "get temp path" function.
			var tempPath = Application.dataPath + "/../Temp/tmp_" + System.Guid.NewGuid().ToString() + ".bytes";
			System.IO.File.WriteAllBytes(tempPath, data);
			try {
				// Unfortunately the Move + overwrite operation doesn't exist until .net core 3.0.
				// So we have to delete the target file before moving to it.
				if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
				System.IO.File.Move(tempPath, path);
			} catch {
				if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
			}
		}

		/// <summary>
		/// Scans the navmesh and saves it to the <see cref="serializedNavmesh"/> field.
		/// A new file will be created if <see cref="serializedNavmesh"/> is null.
		/// If this object is part of a prefab, the file name will be based on the prefab's name.
		///
		/// Note: This method is only available in the editor.
		/// </summary>
		public void ScanAndSaveToFile () {
			SaveToFile(Scan());
		}
#endif

		protected override void OnUpgradeSerializedData (ref Migrations migrations, bool unityThread) {
			migrations.TryMigrateFromLegacyFormat(out var _);
			if (migrations.AddAndMaybeRunMigration(1 << 0)) {
				removeTilesWhenDisabled = false;
			}
			base.OnUpgradeSerializedData(ref migrations, unityThread);
		}
	}
}
