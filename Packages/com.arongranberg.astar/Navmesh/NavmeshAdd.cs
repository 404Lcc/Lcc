using UnityEngine;

namespace Pathfinding {
	using Pathfinding.Drawing;
	using Pathfinding.Pooling;
	using Pathfinding.Collections;

	/// <summary>
	/// Adds new geometry to a recast graph.
	///
	/// This component will add new geometry to a recast graph similar
	/// to how a NavmeshCut component removes it.
	///
	/// There are quite a few limitations to this component though.
	/// This navmesh geometry will not be connected to the rest of the navmesh
	/// in the same tile unless very exactly positioned so that the
	/// triangles line up exactly.
	/// It will be connected to neighbouring tiles if positioned so that
	/// it lines up with the tile border.
	///
	/// This component has a few very specific use-cases.
	/// For example if you have a tiled recast graph
	/// this component could be used to add bridges
	/// in that world.
	/// You would create a NavmeshCut object cutting out a hole for the bridge.
	/// then add a NavmeshAdd object which fills that space.
	/// Make sure NavmeshCut.CutsAddedGeom is disabled on the NavmeshCut, otherwise it will
	/// cut away the NavmeshAdd object.
	/// Then you can add links between the added geometry and the rest of the world, preferably using NodeLink3.
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/navmeshadd.html")]
	public class NavmeshAdd : NavmeshClipper {
		public enum MeshType {
			Rectangle,
			CustomMesh
		}

		public MeshType type;

		/// <summary>
		/// Custom mesh to use.
		/// The contour(s) of the mesh will be extracted.
		/// If you get the "max perturbations" error when cutting with this, check the normals on the mesh.
		/// They should all point in the same direction. Try flipping them if that does not help.
		/// </summary>
		public Mesh mesh;

		/// <summary>Cached vertices</summary>
		Vector3[] verts;

		/// <summary>Cached triangles</summary>
		int[] tris;

		/// <summary>Size of the rectangle</summary>
		public Vector2 rectangleSize = new Vector2(1, 1);

		public float meshScale = 1;

		public Vector3 center;

		/// <summary>
		/// Includes rotation and scale in calculations.
		/// This is slower since a lot more matrix multiplications are needed but gives more flexibility.
		/// </summary>
		[UnityEngine.Serialization.FormerlySerializedAsAttribute("useRotation")]
		public bool useRotationAndScale;

		/// <summary>
		/// Distance between positions to require an update of the navmesh.
		/// A smaller distance gives better accuracy, but requires more updates when moving the object over time,
		/// so it is often slower.
		/// </summary>
		[Tooltip("Distance between positions to require an update of the navmesh\nA smaller distance gives better accuracy, but requires more updates when moving the object over time, so it is often slower.")]
		public float updateDistance = 0.4f;

		/// <summary>
		/// How many degrees rotation that is required for an update to the navmesh.
		/// Should be between 0 and 180.
		/// </summary>
		[Tooltip("How many degrees rotation that is required for an update to the navmesh. Should be between 0 and 180.")]
		public float updateRotationDistance = 10;

		/// <summary>cached transform component</summary>
		protected Transform tr;

		/// <summary>
		/// Returns true if this object has moved so much that it requires an update.
		/// When an update to the navmesh has been done, call NotifyUpdated to be able to get
		/// relavant output from this method again.
		/// </summary>
		public override bool RequiresUpdate (GridLookup<NavmeshClipper>.Root previousState) {
			return (tr.position-previousState.previousPosition).sqrMagnitude > updateDistance*updateDistance || (useRotationAndScale && (Quaternion.Angle(previousState.previousRotation, tr.rotation) > updateRotationDistance));
		}

		/// <summary>
		/// Forces this navmesh add to update the navmesh.
		///
		/// This update is not instant, it is done the next time it is checked if it needs updating.
		///
		/// If there's no AstarPath component in the scene, this method will do nothing.
		///
		/// See: <see cref="NavmeshUpdates.updateInterval"/>
		/// See: <see cref="NavmeshUpdates.ForceUpdate"/>
		/// </summary>
		public override void ForceUpdate () {
			if (AstarPath.active != null) AstarPath.active.navmeshUpdates.ForceUpdateAround(this);
		}

		protected override void Awake () {
			base.Awake();
			tr = transform;
		}

		/// <summary>Internal method to notify the NavmeshAdd that it has just been used to update the navmesh</summary>
		public override void NotifyUpdated (GridLookup<NavmeshClipper>.Root previousState) {
			previousState.previousPosition = tr.position;

			if (useRotationAndScale) {
				previousState.previousRotation = tr.rotation;
			}
		}

		public Vector3 Center {
			get {
				return tr.position + (useRotationAndScale ? tr.TransformPoint(center) : center);
			}
		}

		/// <summary>
		/// Rebuild the internal mesh representation.
		///
		/// Use this if you have changed any settings during runtime.
		/// </summary>
		[ContextMenu("Rebuild Mesh")]
		public void RebuildMesh () {
			if (type == MeshType.CustomMesh) {
				if (mesh == null) {
					verts = null;
					tris = null;
				} else {
					verts = mesh.vertices;
					tris = mesh.triangles;
				}
			} else { // Rectangle
				if (verts == null || verts.Length != 4 || tris == null || tris.Length != 6) {
					verts = new Vector3[4];
					tris = new int[6];
				}

				tris[0] = 0;
				tris[1] = 1;
				tris[2] = 2;
				tris[3] = 0;
				tris[4] = 2;
				tris[5] = 3;

				verts[0] =  new Vector3(-rectangleSize.x*0.5f, 0, -rectangleSize.y*0.5f);
				verts[1] =  new Vector3(rectangleSize.x*0.5f, 0, -rectangleSize.y*0.5f);
				verts[2] =  new Vector3(rectangleSize.x*0.5f, 0,  rectangleSize.y*0.5f);
				verts[3] =  new Vector3(-rectangleSize.x*0.5f, 0,  rectangleSize.y*0.5f);
			}
		}

		/// <summary>
		/// Bounds in XZ space after transforming using the *inverse* transform of the inverseTransform parameter.
		/// The transformation will typically transform the vertices to graph space and this is used to
		/// figure out which tiles the add intersects.
		/// </summary>
		public override Rect GetBounds (Pathfinding.Util.GraphTransform inverseTransform, float radiusMargin) {
			if (this.verts == null) RebuildMesh();
			var verts = Pathfinding.Pooling.ArrayPool<Int3>.Claim(this.verts != null? this.verts.Length : 0);
			int[] tris;
			GetMesh(ref verts, out tris, out var _, inverseTransform);

			Rect r = new Rect();
			for (int i = 0; i < tris.Length; i++) {
				var p = (Vector3)verts[tris[i]];
				if (i == 0) {
					r = new Rect(p.x, p.z, 0, 0);
				} else {
					r.xMax = System.Math.Max(r.xMax, p.x);
					r.yMax = System.Math.Max(r.yMax, p.z);
					r.xMin = System.Math.Min(r.xMin, p.x);
					r.yMin = System.Math.Min(r.yMin, p.z);
				}
			}

			Pathfinding.Pooling.ArrayPool<Int3>.Release(ref verts);
			return r;
		}

		/// <summary>Copy the mesh to the vertex and triangle buffers after the vertices have been transformed using the inverse of the inverseTransform parameter.</summary>
		/// <param name="vbuffer">Assumed to be either null or an array which has a length of zero or a power of two. If this mesh has more
		///  vertices than can fit in the buffer then the buffer will be pooled using Pathfinding.Pooling.ArrayPool.Release and
		///  a new sufficiently large buffer will be taken from the pool.</param>
		/// <param name="tbuffer">This will be set to the internal triangle buffer. You must not modify this array.</param>
		/// <param name="vertexCount">This will be set to the number of vertices in the vertex buffer.</param>
		/// <param name="inverseTransform">All vertices will be transformed using the #Pathfinding.GraphTransform.InverseTransform method.
		///  This is typically used to transform from world space to graph space.</param>
		public void GetMesh (ref Int3[] vbuffer, out int[] tbuffer, out int vertexCount, Pathfinding.Util.GraphTransform inverseTransform = null) {
			if (verts == null) RebuildMesh();

			if (verts == null) {
				tbuffer = ArrayPool<int>.Claim(0);
				vertexCount = 0;
				return;
			}

			if (vbuffer == null || vbuffer.Length < verts.Length) {
				if (vbuffer != null) ArrayPool<Int3>.Release(ref vbuffer);
				vbuffer = ArrayPool<Int3>.Claim(verts.Length);
			}
			tbuffer = tris;
			vertexCount = verts.Length;

			if (useRotationAndScale) {
				Matrix4x4 m = Matrix4x4.TRS(tr.position + center, tr.rotation, tr.localScale * meshScale);

				for (int i = 0; i < verts.Length; i++) {
					var v = m.MultiplyPoint3x4(verts[i]);
					if (inverseTransform != null) v = inverseTransform.InverseTransform(v);
					vbuffer[i] = (Int3)v;
				}
			} else {
				Vector3 voffset = tr.position + center;
				for (int i = 0; i < verts.Length; i++) {
					var v = voffset + verts[i]*meshScale;
					if (inverseTransform != null) v = inverseTransform.InverseTransform(v);
					vbuffer[i] = (Int3)v;
				}
			}
		}

		public static readonly Color GizmoColor = new Color(154.0f/255, 35.0f/255, 239.0f/255);

#if UNITY_EDITOR
		public static Int3[] gizmoBuffer;

		public override void DrawGizmos () {
			if (tr == null) tr = transform;

			GetMesh(ref gizmoBuffer, out var tbuffer, out var _);

			for (int i = 0; i < tbuffer.Length; i += 3) {
				var v1 = (Vector3)gizmoBuffer[tbuffer[i+0]];
				var v2 = (Vector3)gizmoBuffer[tbuffer[i+1]];
				var v3 = (Vector3)gizmoBuffer[tbuffer[i+2]];

				Draw.Line(v1, v2, GizmoColor);
				Draw.Line(v2, v3, GizmoColor);
				Draw.Line(v3, v1, GizmoColor);
			}
		}
#endif
	}
}
