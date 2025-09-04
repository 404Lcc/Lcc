using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Graphs.Navmesh;
using Pathfinding.Util;
using Pathfinding.Drawing;
using Pathfinding.Collections;

namespace Pathfinding {
	/// <summary>
	/// Overrides navmesh generation settings for a single mesh or collider.
	///
	/// Sometimes you want to tweak the <see cref="RecastGraph"/> on a per-object basis. For example you might want to make some objects completely unwalkable, or you might want to special case some objects to remove them from the navmesh altogether.
	///
	/// You can do this using the <see cref="RecastNavmeshModifier"/> component. Attach it to any object you want to modify and configure the settings as you wish.
	///
	/// Using the <see cref="RecastNavmeshModifier"/> component you can:
	///
	/// - Exclude an object from the graph's scan completely.
	/// - Ensure an object is included in the scan, even if it would normally be excluded.
	/// - Make the surfaces of an object unwalkable.
	/// - Make the surfaces of an object walkable (this is just the default behavior).
	/// - Create seams in the navmesh between adjacent objects.
	/// - Mark the surfaces of an object with a specific tag (see tags) (view in online documentation for working links).
	///
	/// Adding this component to an object will make sure it is included in any recast graphs.
	/// It will be included even if the Rasterize Meshes toggle is set to false.
	///
	/// If you are using the Rasterize Meshes option on the recast graph, and you are updating the graph during runtime,
	/// then disabling it and attaching RecastNavmeshModifiers (with <see cref="dynamic"/> set to false) to the objects you want to rasterize can be good for performance.
	/// This is because it's not possible to find meshes in the scene efficiently in Unity, so every time even a small part of the graph is updated, it has to search all meshes in the scene for ones to rasterize.
	/// This can be slow if you have a lot of meshes in the scene. RecastNavmeshModifiers are stored in a tree for extremely fast lookup (O(log n + k)
	/// compared to O(n) where n is the number of meshes in your scene and k is the number of meshes which should be rasterized, if you know Big-O notation.
	/// But as always, profile before you optimize.
	///
	/// Note: If <see cref="dynamic"/> is false, then the object is not allowed to move. If it does, the internal lookup tree will be incorrect and the graph may not be updated correctly.
	///
	/// See: You can also do similar changes on a per-layer basis using <see cref="RecastGraph.perLayerModifications"/>.
	/// </summary>
	[AddComponentMenu("Pathfinding/Navmesh/RecastNavmeshModifier")]
	[DisallowMultipleComponent]
	#pragma warning disable 618
	[HelpURL("https://arongranberg.com/astar/documentation/stable/recastnavmeshmodifier.html")]
	public class RecastNavmeshModifier : VersionedMonoBehaviour, RecastMeshObj {
	#pragma warning restore 618
		/// <summary>Components are stored in a tree for fast lookups</summary>
		protected static AABBTree<RecastNavmeshModifier> tree = new AABBTree<RecastNavmeshModifier>();

		/// <summary>
		/// Enable if the object will move during runtime.
		///
		/// If disabled, the object will be assumed to stay in the same position, and keep the same size, until the component is disabled or destroyed.
		///
		/// Disabling this will provide a small performance boost when doing graph updates,
		/// as the graph no longer has to check if this RecastNavmeshModifier might have moved.
		///
		/// Even you set dynamic=false, you can disable the component, move the object, and enable it at the new position.
		/// </summary>
		public bool dynamic = true;

		/// <summary>
		/// If true then the mesh will be treated as solid and its interior will be unwalkable.
		/// The unwalkable region will be the minimum to maximum y coordinate in each cell.
		///
		/// If you enable this on a mesh that is actually hollow then the hollow region will also be treated as unwalkable.
		/// </summary>
		public bool solid = false;

		/// <summary>Source of geometry when voxelizing this object</summary>
		public GeometrySource geometrySource = GeometrySource.Auto;

		/// <summary>
		/// Determines if the object should be included in scans or not.
		/// See: <see cref="ScanInclusion"/>
		/// </summary>
		public ScanInclusion includeInScan = ScanInclusion.Auto;

		public enum ScanInclusion {
			/// <summary>
			/// Includes or excludes the object as normal based on the recast graph's layer mask and tag mask.
			///
			/// See: <see cref="RecastGraph.mask"/>
			/// </summary>
			Auto,
			/// <summary>This object will be completely ignored by the graph</summary>
			AlwaysExclude,
			/// <summary>This object will always be included when scanning a recast graph, even if it would normally be filtered out</summary>
			AlwaysInclude,
		}

		/// <summary>Source of geometry when voxelizing this object</summary>
		public enum GeometrySource {
			/// <summary>Uses the MeshFilter component on this GameObject if available, otherwise uses the collider</summary>
			Auto,
			/// <summary>Always uses the MeshFilter component on this GameObject</summary>
			MeshFilter,
			/// <summary>Always uses the Collider on this GameObject</summary>
			Collider,
		}

		public enum Mode {
			/// <summary>All surfaces on this mesh will be made unwalkable</summary>
			UnwalkableSurface = 1,
			/// <summary>All surfaces on this mesh will be walkable</summary>
			WalkableSurface,
			/// <summary>All surfaces on this mesh will be walkable and a seam will be created between the surfaces on this mesh and the surfaces on other meshes (with a different surface id)</summary>
			WalkableSurfaceWithSeam,
			/// <summary>All surfaces on this mesh will be walkable and the nodes will be given the specified tag. A seam will be created between the surfaces on this mesh and the surfaces on other meshes (with a different tag or surface id)</summary>
			WalkableSurfaceWithTag,
		}

		/// <summary>
		/// Voxel area for mesh.
		/// This area (not to be confused with pathfinding areas, this is only used when rasterizing meshes for the recast graph) field
		/// can be used to explicitly insert edges in the navmesh geometry or to make some parts of the mesh unwalkable.
		///
		/// When rasterizing the world and two objects with different surface id values are adjacent to each other, a split in the navmesh geometry
		/// will be added between them, characters will still be able to walk between them, but this can be useful when working with navmesh updates.
		///
		/// Navmesh updates which recalculate a whole tile (updatePhysics=True) are very slow So if there are special places
		/// which you know are going to be updated quite often, for example at a door opening (opened/closed door) you
		/// can use surface IDs to create splits on the navmesh for easier updating using normal graph updates (updatePhysics=False).
		/// See the below video for more information.
		///
		/// Video: https://www.youtube.com/watch?v=CS6UypuEMwM
		///
		/// Deprecated: Use <see cref="mode"/> and <see cref="surfaceID"/> instead
		/// </summary>
		[System.Obsolete("Use mode and surfaceID instead")]
		public int area {
			get {
				switch (mode) {
				case Mode.UnwalkableSurface:
					return -1;
				case Mode.WalkableSurface:
				default:
					return 0;
				case Mode.WalkableSurfaceWithSeam:
					return surfaceID;
				case Mode.WalkableSurfaceWithTag:
					return surfaceID;
				}
			}
			set {
				if (value <= -1) mode = Mode.UnwalkableSurface;
				if (value == 0) mode = Mode.WalkableSurface;
				if (value > 0) {
					mode = Mode.WalkableSurfaceWithSeam;
					surfaceID = value;
				}
			}
		}

		/// <summary>
		/// Voxel area for mesh.
		/// This area (not to be confused with pathfinding areas, this is only used when rasterizing meshes for the recast graph) field
		/// can be used to explicitly insert edges in the navmesh geometry or to make some parts of the mesh unwalkable.
		///
		/// When rasterizing the world and two objects with different surface id values are adjacent to each other, a split in the navmesh geometry
		/// will be added between them, characters will still be able to walk between them, but this can be useful when working with navmesh updates.
		///
		/// Navmesh updates which recalculate a whole tile (updatePhysics=True) are very slow So if there are special places
		/// which you know are going to be updated quite often, for example at a door opening (opened/closed door) you
		/// can use surface IDs to create splits on the navmesh for easier updating using normal graph updates (updatePhysics=False).
		/// See the below video for more information.
		///
		/// Video: https://www.youtube.com/watch?v=CS6UypuEMwM
		///
		/// When <see cref="mode"/> is set to Mode.WalkableSurfaceWithTag then this value will be interpreted as a pathfinding tag. See tags (view in online documentation for working links).
		///
		/// Note: This only has an effect if <see cref="mode"/> is set to Mode.WalkableSurfaceWithSeam or Mode.WalkableSurfaceWithTag.
		///
		/// Note: Only non-negative values are valid.
		/// </summary>
		[UnityEngine.Serialization.FormerlySerializedAs("area")]
		public int surfaceID = 1;

		/// <summary>
		/// Surface rasterization mode.
		/// See: <see cref="Mode"/>
		/// </summary>
		public Mode mode = Mode.WalkableSurface;

		AABBTree<RecastNavmeshModifier>.Key treeKey;

		void OnEnable () {
			// Clamp area, upper limit isn't really a hard limit, but if it gets much higher it will start to interfere with other stuff
			surfaceID = Mathf.Clamp(surfaceID, 0, 1 << 25);
			if (!treeKey.isValid) {
				treeKey = tree.Add(CalculateBounds(), this);
				if (this.dynamic) BatchedEvents.Add(this, BatchedEvents.Event.Custom, OnUpdate);
			}
		}

		void OnDisable () {
			BatchedEvents.Remove(this);
			var originalBounds = tree.Remove(treeKey);
			treeKey = default;
			if (!this.dynamic) {
				var newBounds = CalculateBounds();
				// When using static baching, the bounds of the object may shrink.
				// In particular, if the object has been rotated, the renderer's bounds will originally use an approximation of the AABB (presumably just the original AABB, but rotated and then axis aligned again),
				// but after static batching, it actually looks at the new mesh (with the rotation baked in), and can generate a more precise AABB (which may be smaller).
				// Therefore we say that it's ok as long as the original bounds contain the new bounds.
				// This is fine, because the tree only needs a bounding box which contains the object. If it's too big, it will just be a bit more conservative.
				// Also expand the original bounding box by a tiny amount to work around floating point errors.
				originalBounds.Expand(0.001f);
				newBounds.Encapsulate(originalBounds);
				if ((newBounds.center - originalBounds.center).sqrMagnitude > 0.01f*0.01f || (newBounds.extents - originalBounds.extents).sqrMagnitude > 0.01f*0.01f) {
					Debug.LogError("The RecastNavmeshModifier has been moved or resized since it was enabled. You should set dynamic to true for moving objects, or disable the component while moving it. The bounds changed from " + originalBounds + " to " + newBounds, this);
				}
			}
		}

		static void OnUpdate (RecastNavmeshModifier[] components, int _) {
			for (int i = 0; i < components.Length; i++) {
				var comp = components[i];
				if (comp != null && comp.transform.hasChanged) {
					var bounds = comp.CalculateBounds();
					if (tree.GetBounds(comp.treeKey) != bounds) tree.Move(comp.treeKey, bounds);
					comp.transform.hasChanged = false;
				}
			}
		}

		/// <summary>Fills the buffer with all RecastNavmeshModifiers which intersect the specified bounds</summary>
		public static void GetAllInBounds (List<RecastNavmeshModifier> buffer, Bounds bounds) {
			// Refreshes the tree if necessary
			BatchedEvents.ProcessEvent<RecastNavmeshModifier>(BatchedEvents.Event.Custom);

			if (!Application.isPlaying) {
				var objs = UnityCompatibility.FindObjectsByTypeSorted<RecastNavmeshModifier>();
				for (int i = 0; i < objs.Length; i++) {
					if (objs[i].enabled) {
						if (bounds.Intersects(objs[i].CalculateBounds())) {
							buffer.Add(objs[i]);
						}
					}
				}
				return;
			} else if (Time.timeSinceLevelLoad == 0) {
				// Is is not guaranteed that all RecastNavmeshModifier OnEnable functions have been called, so if it is the first frame since loading a new level
				// try to initialize all RecastNavmeshModifier objects.
				var objs = UnityCompatibility.FindObjectsByTypeUnsorted<RecastNavmeshModifier>();
				for (int i = 0; i < objs.Length; i++) objs[i].OnEnable();
			}

			tree.Query(bounds, buffer);
		}

		/// <summary>
		/// Resolves the geometry source that is to be used.
		/// Will output either a MeshFilter, a Collider, or a 2D collider, never more than one.
		/// If all are null, then no geometry could be found.
		///
		/// See: <see cref="geometrySource"/>
		/// </summary>
		public void ResolveMeshSource (out MeshFilter meshFilter, out Collider collider, out Collider2D collider2D) {
			meshFilter = null;
			collider = null;
			collider2D = null;
			switch (geometrySource) {
			case GeometrySource.Auto:
				if (TryGetComponent<MeshRenderer>(out _) && TryGetComponent<MeshFilter>(out meshFilter) && meshFilter.sharedMesh != null) return;
				if (TryGetComponent<Collider>(out collider)) return;
				TryGetComponent<Collider2D>(out collider2D);
				break;
			case GeometrySource.MeshFilter:
				TryGetComponent<MeshFilter>(out meshFilter);
				break;
			case GeometrySource.Collider:
				if (TryGetComponent<Collider>(out collider)) return;
				TryGetComponent<Collider2D>(out collider2D);
				break;
			default:
				throw new System.ArgumentOutOfRangeException();
			}
		}

		/// <summary>Calculates and returns the bounding box containing all geometry to be rasterized</summary>
		private Bounds CalculateBounds () {
			ResolveMeshSource(out var filter, out var coll, out var coll2D);

			if (coll != null) {
				return coll.bounds;
			} else if (coll2D != null) {
				return coll2D.bounds;
			} else if (filter != null) {
				if (TryGetComponent<MeshRenderer>(out var rend)) {
					return rend.bounds;
				} else {
					Debug.LogError("Cannot use a MeshFilter as a geomtry source without a MeshRenderer attached to the same GameObject.", this);
					return new Bounds(Vector3.zero, Vector3.one);
				}
			} else {
				Debug.LogError("Could not find an attached mesh source", this);
				return new Bounds(Vector3.zero, Vector3.one);
			}
		}

		protected override void OnUpgradeSerializedData (ref Serialization.Migrations migrations, bool unityThread) {
			if (migrations.TryMigrateFromLegacyFormat(out var legacyVersion)) {
				#pragma warning disable 618
				if (legacyVersion == 1) area = surfaceID;
				#pragma warning restore 618
				if (legacyVersion <= 2) includeInScan = ScanInclusion.AlwaysInclude;
				// Mode.ExcludeFromGraph was changed to ScanInclusion.AlwaysExclude
				if (mode == (Mode)0) includeInScan = ScanInclusion.AlwaysExclude;
			}
		}


		#region Compatibility
		bool RecastMeshObj.dynamic { get => dynamic; set => dynamic = value; }
		bool RecastMeshObj.solid { get => solid; set => solid = value; }
		GeometrySource RecastMeshObj.geometrySource { get => geometrySource; set => geometrySource = value; }
		ScanInclusion RecastMeshObj.includeInScan { get => includeInScan; set => includeInScan = value; }
		int RecastMeshObj.surfaceID { get => surfaceID; set => surfaceID = value; }
		Mode RecastMeshObj.mode { get => mode; set => mode = value; }
		#endregion
	}

	/// <summary>
	/// Explicit mesh object for recast graphs.
	/// Deprecated: Has been renamed to <see cref="RecastNavmeshModifier"/>.
	/// </summary>
	[System.Obsolete("Has been renamed to RecastNavmeshModifier")]
	public interface RecastMeshObj {
		bool enabled { get; set; }
		bool dynamic { get; set; }
		bool solid { get; set; }
		RecastNavmeshModifier.GeometrySource geometrySource { get; set; }
		RecastNavmeshModifier.ScanInclusion includeInScan { get; set; }
		int surfaceID { get; set; }
		RecastNavmeshModifier.Mode mode { get; set; }
	}
}
