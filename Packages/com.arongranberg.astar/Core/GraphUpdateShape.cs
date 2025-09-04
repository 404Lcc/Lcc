using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Pathfinding {
	/// <summary>
	/// Defines a shape for a Pathfinding.GraphUpdateObject.
	/// The shape consists of a number of points which it can either calculate the convex hull of or use as a polygon directly.
	///
	/// A shape is essentially a 2D shape however it can be rotated arbitrarily.
	/// When a matrix and a list of points is specified in the constructor the matrix decides what direction
	/// is the 'up' direction. When checking if a point is contained in the shape, the point will be projected down
	/// on a plane where the 'up' direction is the normal and then it will check if the shape contains the point.
	///
	/// See: Pathfinding.GraphUpdateObject.shape
	/// </summary>
	public class GraphUpdateShape {
		Vector3[] _points;
		Vector3[] _convexPoints;
		bool _convex;
		Vector3 right = Vector3.right;
		Vector3 forward = Vector3.forward;
		Vector3 up = Vector3.up;
		Vector3 origin;
		public float minimumHeight;

		/// <summary>Shape optimized for burst</summary>
		public struct BurstShape {
			[DeallocateOnJobCompletion]
			NativeArray<Vector3> points;
			float3 origin, right, forward;
			bool containsEverything;

			public BurstShape(GraphUpdateShape scene, Allocator allocator) {
				var pts = scene.convex ? scene._convexPoints : scene._points;

				if (pts == null) points = new NativeArray<Vector3>(0, allocator);
				else points = new NativeArray<Vector3>(pts, allocator);

				origin = scene.origin;
				right = scene.right;
				forward = scene.forward;

				// Speeds up calculations below
				var mgn = scene.right.sqrMagnitude;
				if (mgn > 0) right /= mgn;
				mgn = scene.forward.sqrMagnitude;
				if (mgn > 0) forward /= mgn;
				containsEverything = false;
			}

			/// <summary>Shape that contains everything</summary>
			public static BurstShape Everything => new BurstShape {
				points = new NativeArray<Vector3>(0, Allocator.Persistent),
				origin = float3.zero,
				right = float3.zero,
				forward = float3.zero,
				containsEverything = true,
			};

			public bool Contains (float3 point) {
				if (containsEverything) return true;
				// Transform to local space (shape in the XZ plane)
				point -= origin;
				// Point in local space
				var p = new float3(math.dot(point, right), 0, math.dot(point, forward));

				int j = points.Length-1;
				bool inside = false;

				for (int i = 0; i < points.Length; j = i++) {
					if (((points[i].z <= p.z && p.z < points[j].z) || (points[j].z <= p.z && p.z < points[i].z)) &&
						(p.x < (points[j].x - points[i].x) * (p.z - points[i].z) / (points[j].z - points[i].z) + points[i].x))
						inside = !inside;
				}
				return inside;
			}
		}

		/// <summary>
		/// Gets or sets the points of the polygon in the shape.
		/// These points should be specified in clockwise order.
		/// Will automatically calculate the convex hull if <see cref="convex"/> is set to true
		/// </summary>
		public Vector3[] points {
			get {
				return _points;
			}
			set {
				_points = value;
				if (convex) CalculateConvexHull();
			}
		}

		/// <summary>
		/// Sets if the convex hull of the points should be calculated.
		/// Convex hulls are faster but non-convex hulls can be used to specify more complicated shapes.
		/// </summary>
		public bool convex {
			get {
				return _convex;
			}
			set {
				if (_convex != value && value) {
					CalculateConvexHull();
				}
				_convex = value;
			}
		}

		public GraphUpdateShape () {
		}

		/// <summary>
		/// Construct a shape.
		/// See: <see cref="convex"/>
		/// </summary>
		/// <param name="points">Contour of the shape in local space with respect to the matrix (i.e the shape should be in the XZ plane, the Y coordinate will only affect the bounds)</param>
		/// <param name="convex">If true, the convex hull of the points will be calculated.</param>
		/// <param name="matrix">local to world space matrix for the points. The matrix determines the up direction of the shape.</param>
		/// <param name="minimumHeight">If the points would be in the XZ plane only, the shape would not have a height and then it might not
		/// 		include any points inside it (as testing for inclusion is done in 3D space when updating graphs). This ensures
		/// 		 that the shape has at least the minimum height (in the up direction that the matrix specifies).</param>
		public GraphUpdateShape (Vector3[] points, bool convex, Matrix4x4 matrix, float minimumHeight) {
			this.convex = convex;
			this.points = points;
			origin = matrix.MultiplyPoint3x4(Vector3.zero);
			right = matrix.MultiplyPoint3x4(Vector3.right) - origin;
			up = matrix.MultiplyPoint3x4(Vector3.up) - origin;
			forward = matrix.MultiplyPoint3x4(Vector3.forward) - origin;
			this.minimumHeight = minimumHeight;
		}

		void CalculateConvexHull () {
			_convexPoints = points != null? Polygon.ConvexHullXZ(points) : null;
		}

		/// <summary>World space bounding box of this shape</summary>
		public Bounds GetBounds () {
			return GetBounds(convex ? _convexPoints : points, right, up, forward, origin, minimumHeight);
		}

		public static Bounds GetBounds (Vector3[] points, Matrix4x4 matrix, float minimumHeight) {
			var origin = matrix.MultiplyPoint3x4(Vector3.zero);
			var right = matrix.MultiplyPoint3x4(Vector3.right) - origin;
			var up = matrix.MultiplyPoint3x4(Vector3.up) - origin;
			var forward = matrix.MultiplyPoint3x4(Vector3.forward) - origin;

			return GetBounds(points, right, up, forward, origin, minimumHeight);
		}

		static Bounds GetBounds (Vector3[] points, Vector3 right, Vector3 up, Vector3 forward, Vector3 origin, float minimumHeight) {
			if (points == null || points.Length == 0) return new Bounds();
			float miny = points[0].y, maxy = points[0].y;
			for (int i = 0; i < points.Length; i++) {
				miny = Mathf.Min(miny, points[i].y);
				maxy = Mathf.Max(maxy, points[i].y);
			}
			var extraHeight = Mathf.Max(minimumHeight - (maxy - miny), 0) * 0.5f;
			miny -= extraHeight;
			maxy += extraHeight;

			Vector3 min = right * points[0].x + up * points[0].y + forward * points[0].z;
			Vector3 max = min;
			for (int i = 0; i < points.Length; i++) {
				var p = right * points[i].x + forward * points[i].z;
				var p1 = p + up * miny;
				var p2 = p + up * maxy;
				min = Vector3.Min(min, p1);
				min = Vector3.Min(min, p2);
				max = Vector3.Max(max, p1);
				max = Vector3.Max(max, p2);
			}
			return new Bounds((min+max)*0.5F + origin, max-min);
		}

		public bool Contains (GraphNode node) {
			return Contains((Vector3)node.position);
		}

		public bool Contains (Vector3 point) {
			// Transform to local space (shape in the XZ plane)
			point -= origin;
			var localSpacePoint = new Vector3(Vector3.Dot(point, right)/right.sqrMagnitude, 0, Vector3.Dot(point, forward)/forward.sqrMagnitude);

			if (convex) {
				if (_convexPoints == null) return false;

				for (int i = 0, j = _convexPoints.Length-1; i < _convexPoints.Length; j = i, i++) {
					if (VectorMath.RightOrColinearXZ(_convexPoints[i], _convexPoints[j], localSpacePoint)) return false;
				}
				return true;
			} else {
				return _points != null && Polygon.ContainsPointXZ(_points, localSpacePoint);
			}
		}
	}
}
