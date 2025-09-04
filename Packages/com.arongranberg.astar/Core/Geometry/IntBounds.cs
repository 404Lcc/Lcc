using Unity.Mathematics;

namespace Pathfinding {
	/// <summary>
	/// Integer bounding box.
	/// Works almost like UnityEngine.BoundsInt but with a slightly nicer and more efficient api.
	///
	/// Uses an exclusive upper bound (max field).
	/// </summary>
	public struct IntBounds {
		public int3 min, max;

		public IntBounds (int xmin, int ymin, int zmin, int xmax, int ymax, int zmax) {
			min = new int3(xmin, ymin, zmin);
			max = new int3(xmax, ymax, zmax);
		}

		public IntBounds(int3 min, int3 max) {
			this.min = min;
			this.max = max;
		}

		public int3 size => max - min;
		public int volume {
			get {
				var s = size;
				return s.x * s.y * s.z;
			}
		}

		/// <summary>
		/// Returns the intersection bounding box between the two bounds.
		/// The intersection bounds is the volume which is inside both bounds.
		/// If the rects do not have an intersection, an invalid rect is returned.
		/// See: IsValid
		/// </summary>
		public static IntBounds Intersection (IntBounds a, IntBounds b) {
			return new IntBounds(
				math.max(a.min, b.min),
				math.min(a.max, b.max)
				);
		}

		public static bool Intersects (IntBounds a, IntBounds b) {
			return math.all(a.min< b.max & a.max > b.min);
		}

		public IntBounds Offset (int3 offset) {
			return new IntBounds(min + offset, max + offset);
		}

		public bool Contains (IntBounds other) {
			return math.all(other.min >= min & other.max <= max);
		}

		public override string ToString() => "(" + min.ToString() + " <= x < " + max.ToString() + ")";
		public override bool Equals (object _b) {
			var b = (IntBounds)_b;
			return this == b;
		}

		public override int GetHashCode() => min.GetHashCode() ^ (max.GetHashCode() << 2);

		public static bool operator ==(IntBounds a, IntBounds b) => math.all(a.min == b.min & a.max == b.max);

		public static bool operator !=(IntBounds a, IntBounds b) => !(a == b);
	}
}
