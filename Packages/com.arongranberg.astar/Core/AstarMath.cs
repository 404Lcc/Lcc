using UnityEngine;
using System.Collections.Generic;
using System;

namespace Pathfinding {
	using Pathfinding.Util;
	using Unity.Mathematics;
	using Unity.Burst;
	using Pathfinding.Collections;
	using Pathfinding.Pooling;

	/// <summary>Contains various spline functions.</summary>
	public static class AstarSplines {
		public static Vector3 CatmullRom (Vector3 previous, Vector3 start, Vector3 end, Vector3 next, float elapsedTime) {
			// References used:
			// p.266 GemsV1
			//
			// tension is often set to 0.5 but you can use any reasonable value:
			// http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
			//
			// bias and tension controls:
			// http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

			float percentComplete = elapsedTime;
			float percentCompleteSquared = percentComplete * percentComplete;
			float percentCompleteCubed = percentCompleteSquared * percentComplete;

			return
				previous * (-0.5F*percentCompleteCubed +
							percentCompleteSquared -
							0.5F*percentComplete) +

				start *
				(1.5F*percentCompleteCubed +
				 -2.5F*percentCompleteSquared + 1.0F) +

				end *
				(-1.5F*percentCompleteCubed +
				 2.0F*percentCompleteSquared +
				 0.5F*percentComplete) +

				next *
				(0.5F*percentCompleteCubed -
				 0.5F*percentCompleteSquared);
		}

		/// <summary>Returns a point on a cubic bezier curve. t is clamped between 0 and 1</summary>
		public static Vector3 CubicBezier (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			t = Mathf.Clamp01(t);
			float t2 = 1-t;
			return t2*t2*t2 * p0 + 3 * t2*t2 * t * p1 + 3 * t2 * t*t * p2 + t*t*t * p3;
		}

		/// <summary>Returns the derivative for a point on a cubic bezier curve. t is clamped between 0 and 1</summary>
		public static Vector3 CubicBezierDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			t = Mathf.Clamp01(t);
			float t2 = 1-t;
			return 3*t2*t2*(p1-p0) + 6*t2*t*(p2 - p1) + 3*t*t*(p3 - p2);
		}

		/// <summary>Returns the second derivative for a point on a cubic bezier curve. t is clamped between 0 and 1</summary>
		public static Vector3 CubicBezierSecondDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			t = Mathf.Clamp01(t);
			float t2 = 1-t;
			return 6*t2*(p2 - 2*p1 + p0) + 6*t*(p3 - 2*p2 + p1);
		}
	}

	/// <summary>
	/// Various vector math utility functions.
	/// Version: A lot of functions in the Polygon class have been moved to this class
	/// the names have changed slightly and everything now consistently assumes a left handed
	/// coordinate system now instead of sometimes using a left handed one and sometimes
	/// using a right handed one. This is why the 'Left' methods in the Polygon class redirect
	/// to methods named 'Right'. The functionality is exactly the same.
	///
	/// Note the difference between segments and lines. Lines are infinitely
	/// long but segments have only a finite length.
	/// </summary>
	public static class VectorMath {
		/// <summary>
		/// Complex number multiplication.
		/// Returns: a * b
		///
		/// Used to rotate vectors in an efficient way.
		///
		/// See: https://en.wikipedia.org/wiki/Complex_number<see cref="Multiplication_and_division"/>
		/// </summary>
		public static Vector2 ComplexMultiply (Vector2 a, Vector2 b) {
			return new Vector2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
		}

		/// <summary>
		/// Complex number multiplication.
		/// Returns: a * b
		///
		/// Used to rotate vectors in an efficient way.
		///
		/// See: https://en.wikipedia.org/wiki/Complex_number<see cref="Multiplication_and_division"/>
		/// </summary>
		public static float2 ComplexMultiply (float2 a, float2 b) {
			return a.x*b + a.y*new float2(-b.y, b.x);
		}

		/// <summary>
		/// Complex number multiplication.
		/// Returns: a * conjugate(b)
		///
		/// Used to rotate vectors in an efficient way.
		///
		/// See: https://en.wikipedia.org/wiki/Complex_number<see cref="Multiplication_and_division"/>
		/// See: https://en.wikipedia.org/wiki/Complex_conjugate
		/// </summary>
		public static float2 ComplexMultiplyConjugate (float2 a, float2 b) {
			return new float2(a.x * b.x + a.y * b.y, a.y * b.x - a.x * b.y);
		}

		/// <summary>
		/// Complex number multiplication.
		/// Returns: a * conjugate(b)
		///
		/// Used to rotate vectors in an efficient way.
		///
		/// See: https://en.wikipedia.org/wiki/Complex_number<see cref="Multiplication_and_division"/>
		/// See: https://en.wikipedia.org/wiki/Complex_conjugate
		/// </summary>
		public static Vector2 ComplexMultiplyConjugate (Vector2 a, Vector2 b) {
			return new Vector2(a.x * b.x + a.y * b.y, a.y * b.x - a.x * b.y);
		}

		/// <summary>
		/// Returns the closest point on the line.
		/// The line is treated as infinite.
		/// See: ClosestPointOnSegment
		/// See: ClosestPointOnLineFactor
		/// </summary>
		public static Vector3 ClosestPointOnLine (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);
			float dot = Vector3.Dot(point - lineStart, lineDirection);

			return lineStart + (dot*lineDirection);
		}

		/// <summary>
		/// Returns the closest point on the line.
		/// The line is treated as infinite.
		/// See: ClosestPointOnSegment
		/// See: ClosestPointOnLineFactor
		/// </summary>
		public static float3 ClosestPointOnLine (float3 lineStart, float3 lineEnd, float3 point) {
			return lineStart + ClosestPointOnLineFactor(lineStart, lineEnd, point) * (lineEnd - lineStart);
		}

		/// <summary>
		/// Factor along the line which is closest to the point.
		/// Returned value is in the range [0,1] if the point lies on the segment otherwise it just lies on the line.
		/// The closest point can be calculated using (end-start)*factor + start.
		///
		/// See: ClosestPointOnLine
		/// See: ClosestPointOnSegment
		/// </summary>
		public static float ClosestPointOnLineFactor (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			var dir = lineEnd - lineStart;
			float sqrMagn = dir.sqrMagnitude;

			if (sqrMagn <= 0.000001f) return 0;

			return Vector3.Dot(point - lineStart, dir) / sqrMagn;
		}

		/// <summary>
		/// Factor along the line which is closest to the point.
		/// Returned value is in the range [0,1] if the point lies on the segment otherwise it just lies on the line.
		/// The closest point can be calculated using (end-start)*factor + start
		/// </summary>
		public static float ClosestPointOnLineFactor (float3 lineStart, float3 lineEnd, float3 point) {
			var lineDirection = lineEnd - lineStart;
			var sqrMagn = math.dot(lineDirection, lineDirection);
			return math.select(0, math.dot(point - lineStart, lineDirection) / sqrMagn, sqrMagn > 0.000001f);
		}

		/// <summary>
		/// Factor along the line which is closest to the point.
		/// Returned value is in the range [0,1] if the point lies on the segment otherwise it just lies on the line.
		/// The closest point can be calculated using (end-start)*factor + start
		/// </summary>
		public static float ClosestPointOnLineFactor (Int3 lineStart, Int3 lineEnd, Int3 point) {
			var lineDirection = lineEnd - lineStart;
			float magn = lineDirection.sqrMagnitude;

			float closestPoint = (float)Int3.DotLong(point - lineStart, lineDirection);

			if (magn != 0) closestPoint /= magn;

			return closestPoint;
		}

		/// <summary>
		/// Factor of the nearest point on the segment.
		/// Returned value is in the range [0,1] if the point lies on the segment otherwise it just lies on the line.
		/// The closest point can be calculated using (end-start)*factor + start;
		/// </summary>
		public static float ClosestPointOnLineFactor (Vector2Int lineStart, Vector2Int lineEnd, Vector2Int point) {
			var lineDirection = lineEnd - lineStart;
			double magn = (long)lineDirection.x*(long)lineDirection.x + (long)lineDirection.y*(long)lineDirection.y;

			var dirPoint = point - lineStart;
			double closestPoint = (long)dirPoint.x*(long)lineDirection.x + (long)dirPoint.y*(long)lineDirection.y;

			if (magn != 0) closestPoint /= magn;

			return (float)closestPoint;
		}

		/// <summary>
		/// Returns the closest point on the segment.
		/// The segment is NOT treated as infinite.
		/// See: ClosestPointOnLine
		/// See: ClosestPointOnSegmentXZ
		/// </summary>
		public static Vector3 ClosestPointOnSegment (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			var dir = lineEnd - lineStart;
			float sqrMagn = dir.sqrMagnitude;

			if (sqrMagn <= 0.000001) return lineStart;

			float factor = Vector3.Dot(point - lineStart, dir) / sqrMagn;
			return lineStart + Mathf.Clamp01(factor)*dir;
		}

		/// <summary>
		/// Returns the closest point on the segment in the XZ plane.
		/// The y coordinate of the result will be the same as the y coordinate of the point parameter.
		///
		/// The segment is NOT treated as infinite.
		/// See: ClosestPointOnSegment
		/// See: ClosestPointOnLine
		/// </summary>
		public static Vector3 ClosestPointOnSegmentXZ (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			lineStart.y = point.y;
			lineEnd.y = point.y;
			Vector3 fullDirection = lineEnd-lineStart;
			Vector3 fullDirection2 = fullDirection;
			fullDirection2.y = 0;
			float magn = fullDirection2.magnitude;
			Vector3 lineDirection = magn > float.Epsilon ? fullDirection2/magn : Vector3.zero;

			float closestPoint = Vector3.Dot((point-lineStart), lineDirection);
			return lineStart+(Mathf.Clamp(closestPoint, 0.0f, fullDirection2.magnitude)*lineDirection);
		}

		/// <summary>
		/// Returns the approximate shortest squared distance between x,z and the segment p-q.
		/// The segment is not considered infinite.
		/// This function is not entirely exact, but it is about twice as fast as DistancePointSegment2.
		/// TODO: Is this actually approximate? It looks exact.
		/// </summary>
		public static float SqrDistancePointSegmentApproximate (int x, int z, int px, int pz, int qx, int qz) {
			float pqx = (float)(qx - px);
			float pqz = (float)(qz - pz);
			float dx = (float)(x - px);
			float dz = (float)(z - pz);
			float d = pqx*pqx + pqz*pqz;
			float t = pqx*dx + pqz*dz;

			if (d > 0)
				t /= d;
			if (t < 0)
				t = 0;
			else if (t > 1)
				t = 1;

			dx = px + t*pqx - x;
			dz = pz + t*pqz - z;

			return dx*dx + dz*dz;
		}

		/// <summary>
		/// Returns the approximate shortest squared distance between x,z and the segment p-q.
		/// The segment is not considered infinite.
		/// This function is not entirely exact, but it is about twice as fast as DistancePointSegment2.
		/// TODO: Is this actually approximate? It looks exact.
		/// </summary>
		public static float SqrDistancePointSegmentApproximate (Int3 a, Int3 b, Int3 p) {
			float pqx = (float)(b.x - a.x);
			float pqz = (float)(b.z - a.z);
			float dx = (float)(p.x - a.x);
			float dz = (float)(p.z - a.z);
			float d = pqx*pqx + pqz*pqz;
			float t = pqx*dx + pqz*dz;

			if (d > 0)
				t /= d;
			if (t < 0)
				t = 0;
			else if (t > 1)
				t = 1;

			dx = a.x + t*pqx - p.x;
			dz = a.z + t*pqz - p.z;

			return dx*dx + dz*dz;
		}

		/// <summary>
		/// Returns the squared distance between p and the segment a-b.
		/// The line is not considered infinite.
		/// </summary>
		public static float SqrDistancePointSegment (Vector3 a, Vector3 b, Vector3 p) {
			var nearest = ClosestPointOnSegment(a, b, p);

			return (nearest-p).sqrMagnitude;
		}

		/// <summary>
		/// 3D minimum distance between 2 segments.
		/// Input: two 3D line segments S1 and S2
		/// Returns: the shortest squared distance between S1 and S2
		/// </summary>
		public static float SqrDistanceSegmentSegment (Vector3 s1, Vector3 e1, Vector3 s2, Vector3 e2) {
			Vector3 dir1 = e1 - s1;
			Vector3 dir2 = e2 - s2;
			Vector3 startOffset = s1 - s2;
			double dir1sq = Vector3.Dot(dir1, dir1);           // always >= 0
			double b = Vector3.Dot(dir1, dir2);
			double dir2sq = Vector3.Dot(dir2, dir2);           // always >= 0
			double d = Vector3.Dot(dir1, startOffset);
			double e = Vector3.Dot(dir2, startOffset);
			double D = dir1sq*dir2sq - b*b;           // always >= 0
			double sc, sN, sD = D;          // sc = sN / sD, default sD = D >= 0
			double tc, tN, tD = D;          // tc = tN / tD, default tD = D >= 0

			// compute the line parameters of the two closest points
			// D is approximately |dir1|^2|dir2|^2*(1-cos^2 alpha), where alpha is the angle between the lines
			if (D < 0.000001 * dir1sq*dir2sq) { // the lines are almost parallel
				sN = 0.0f;         // force using point P0 on segment S1
				sD = 1.0f;         // to prevent possible division by 0.0 later
				tN = e;
				tD = dir2sq;
			} else {               // get the closest points on the infinite lines
				sN = (b*e - dir2sq*d);
				tN = (dir1sq*e - b*d);
				if (sN < 0.0) {        // sc < 0 => the s=0 edge is visible
					sN = 0.0;
					tN = e;
					tD = dir2sq;
				} else if (sN > sD) { // sc > 1  => the s=1 edge is visible
					sN = sD;
					tN = e + b;
					tD = dir2sq;
				}
			}

			if (tN < 0.0) {            // tc < 0 => the t=0 edge is visible
				tN = 0.0;
				// recompute sc for this edge
				if (-d < 0.0f)
					sN = 0.0f;
				else if (-d > dir1sq)
					sN = sD;
				else {
					sN = -d;
					sD = dir1sq;
				}
			} else if (tN > tD) {    // tc > 1  => the t=1 edge is visible
				tN = tD;
				// recompute sc for this edge
				if ((-d + b) < 0.0f)
					sN = 0;
				else if ((-d + b) > dir1sq)
					sN = sD;
				else {
					sN = (-d +  b);
					sD = dir1sq;
				}
			}

			// finally do the division to get sc and tc
			sc = (Math.Abs(sN) < 0.00001f ? 0.0 : sN / sD);
			tc = (Math.Abs(tN) < 0.00001f ? 0.0 : tN / tD);

			// get the difference of the two closest points
			Vector3 dP = startOffset + ((float)sc * dir1) - ((float)tc * dir2);  // =  S1(sc) - S2(tc)

			return dP.sqrMagnitude;   // return the closest distance
		}

		/// <summary>
		/// Determinant of the 2x2 matrix [c1, c2].
		///
		/// This is useful for many things, like calculating distances between lines and points.
		///
		/// Equivalent to Cross(new float3(c1, 0), new float 3(c2, 0)).z
		/// </summary>
		public static float Determinant (float2 c1, float2 c2) {
			return c1.x*c2.y - c1.y*c2.x;
		}

		/// <summary>Squared distance between two points in the XZ plane</summary>
		public static float SqrDistanceXZ (Vector3 a, Vector3 b) {
			var delta = a-b;

			return delta.x*delta.x+delta.z*delta.z;
		}

		/// <summary>
		/// Signed area of a triangle multiplied by 2.
		/// This will be negative for clockwise triangles and positive for counter-clockwise ones
		/// </summary>
		public static long SignedTriangleAreaTimes2 (int2 a, int2 b, int2 c) {
			return (long)(b.x - a.x) * (long)(c.y - a.y) - (long)(c.x - a.x) * (long)(b.y - a.y);
		}

		/// <summary>
		/// Signed area of a triangle in the XZ plane multiplied by 2.
		/// This will be negative for clockwise triangles and positive for counter-clockwise ones
		/// </summary>
		public static long SignedTriangleAreaTimes2XZ (Int3 a, Int3 b, Int3 c) {
			return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z);
		}

		/// <summary>
		/// Signed area of a triangle in the XZ plane multiplied by 2.
		/// This will be negative for clockwise triangles and positive for counter-clockwise ones.
		/// </summary>
		public static float SignedTriangleAreaTimes2XZ (Vector3 a, Vector3 b, Vector3 c) {
			return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
		}

		/// <summary>
		/// Returns if p lies on the right side of the line a - b.
		/// Uses XZ space. Does not return true if the points are colinear.
		/// </summary>
		public static bool RightXZ (Vector3 a, Vector3 b, Vector3 p) {
			return (b.x - a.x) * (p.z - a.z) - (p.x - a.x) * (b.z - a.z) < -float.Epsilon;
		}

		/// <summary>
		/// Returns if p lies on the right side of the line a - b.
		/// Uses XZ space. Does not return true if the points are colinear.
		/// </summary>
		public static bool RightXZ (Int3 a, Int3 b, Int3 p) {
			return (long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z) < 0;
		}

		/// <summary>
		/// Returns if p lies on the right side of the line a - b.
		/// Does not return true if the points are colinear.
		/// </summary>
		public static bool Right (int2 a, int2 b, int2 p) {
			return (long)(b.x - a.x) * (long)(p.y - a.y) - (long)(p.x - a.x) * (long)(b.y - a.y) < 0;
		}

		/// <summary>
		/// Returns which side of the line a - b that p lies on.
		/// Uses XZ space.
		/// </summary>
		public static Side SideXZ (Int3 a, Int3 b, Int3 p) {
			var s = (long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z);

			return s > 0 ? Side.Left : (s < 0 ? Side.Right : Side.Colinear);
		}

		/// <summary>
		/// Returns if p lies on the right side of the line a - b.
		/// Also returns true if the points are colinear.
		/// </summary>
		public static bool RightOrColinear (Vector2 a, Vector2 b, Vector2 p) {
			return (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y) <= 0;
		}

		/// <summary>
		/// Returns if p lies on the right side of the line a - b.
		/// Also returns true if the points are colinear.
		/// </summary>
		public static bool RightOrColinear (Vector2Int a, Vector2Int b, Vector2Int p) {
			return (long)(b.x - a.x) * (long)(p.y - a.y) - (long)(p.x - a.x) * (long)(b.y - a.y) <= 0;
		}

		/// <summary>
		/// Returns if p lies on the left side of the line a - b.
		/// Uses XZ space. Also returns true if the points are colinear.
		/// </summary>
		public static bool RightOrColinearXZ (Vector3 a, Vector3 b, Vector3 p) {
			return (b.x - a.x) * (p.z - a.z) - (p.x - a.x) * (b.z - a.z) <= 0;
		}

		/// <summary>
		/// Returns if p lies on the left side of the line a - b.
		/// Uses XZ space. Also returns true if the points are colinear.
		/// </summary>
		public static bool RightOrColinearXZ (Int3 a, Int3 b, Int3 p) {
			return (long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z) <= 0;
		}

		/// <summary>
		/// Returns if the points a in a clockwise order.
		/// Will return true even if the points are colinear or very slightly counter-clockwise
		/// (if the signed area of the triangle formed by the points has an area less than or equals to float.Epsilon)
		/// </summary>
		public static bool IsClockwiseMarginXZ (Vector3 a, Vector3 b, Vector3 c) {
			return (b.x-a.x)*(c.z-a.z)-(c.x-a.x)*(b.z-a.z) <= float.Epsilon;
		}

		/// <summary>Returns if the points a in a clockwise order</summary>
		public static bool IsClockwiseXZ (Vector3 a, Vector3 b, Vector3 c) {
			return (b.x-a.x)*(c.z-a.z)-(c.x-a.x)*(b.z-a.z) < 0;
		}

		/// <summary>Returns if the points a in a clockwise order</summary>
		public static bool IsClockwiseXZ (Int3 a, Int3 b, Int3 c) {
			return RightXZ(a, b, c);
		}

		/// <summary>Returns if the points a in a clockwise order</summary>
		public static bool IsClockwise (int2 a, int2 b, int2 c) {
			return Right(a, b, c);
		}

		/// <summary>Returns true if the points a in a clockwise order or if they are colinear</summary>
		public static bool IsClockwiseOrColinearXZ (Int3 a, Int3 b, Int3 c) {
			return RightOrColinearXZ(a, b, c);
		}

		/// <summary>Returns true if the points a in a clockwise order or if they are colinear</summary>
		public static bool IsClockwiseOrColinear (Vector2Int a, Vector2Int b, Vector2Int c) {
			return RightOrColinear(a, b, c);
		}

		/// <summary>Returns if the points are colinear (lie on a straight line)</summary>
		public static bool IsColinear (Vector3 a, Vector3 b, Vector3 c) {
			var lhs = b - a;
			var rhs = c - a;
			// Take the cross product of lhs and rhs
			// The magnitude of the cross product will be zero if the points a,b,c are colinear
			float x = lhs.y * rhs.z - lhs.z * rhs.y;
			float y = lhs.z * rhs.x - lhs.x * rhs.z;
			float z = lhs.x * rhs.y - lhs.y * rhs.x;
			float v = x*x + y*y + z*z;
			float lengthsq = lhs.sqrMagnitude * rhs.sqrMagnitude;

			// Epsilon not chosen with much thought, just that float.Epsilon was a bit too small.
			return v <= math.sqrt(lengthsq) * 0.0001f || lengthsq == 0.0f;
		}

		/// <summary>Returns if the points are colinear (lie on a straight line)</summary>
		public static bool IsColinear (Vector2 a, Vector2 b, Vector2 c) {
			float v = (b.x-a.x)*(c.y-a.y)-(c.x-a.x)*(b.y-a.y);

			// Epsilon not chosen with much thought, just that float.Epsilon was a bit too small.
			return v <= 0.0001f && v >= -0.0001f;
		}

		/// <summary>Returns if the points are colinear (lie on a straight line)</summary>
		public static bool IsColinear (int2 a, int2 b, int2 c) {
			return (long)(b.x - a.x) * (long)(c.y - a.y) - (long)(c.x - a.x) * (long)(b.y - a.y) == 0;
		}

		/// <summary>Returns if the points are colinear (lie on a straight line)</summary>
		public static bool IsColinearXZ (Int3 a, Int3 b, Int3 c) {
			return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z) == 0;
		}

		/// <summary>Returns if the points are colinear (lie on a straight line)</summary>
		public static bool IsColinearXZ (Vector3 a, Vector3 b, Vector3 c) {
			float v = (b.x-a.x)*(c.z-a.z)-(c.x-a.x)*(b.z-a.z);

			// Epsilon not chosen with much thought, just that float.Epsilon was a bit too small.
			return v <= 0.0000001f && v >= -0.0000001f;
		}

		/// <summary>Returns if the points are colinear (lie on a straight line)</summary>
		public static bool IsColinearAlmostXZ (Int3 a, Int3 b, Int3 c) {
			long v = (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z);

			return v > -1 && v < 1;
		}

		/// <summary>
		/// Returns if the line segment start2 - end2 intersects the line segment start1 - end1.
		/// If only the endpoints coincide, the result is undefined (may be true or false).
		/// </summary>
		public static bool SegmentsIntersect (Vector2Int start1, Vector2Int end1, Vector2Int start2, Vector2Int end2) {
			return RightOrColinear(start1, end1, start2) != RightOrColinear(start1, end1, end2) && RightOrColinear(start2, end2, start1) != RightOrColinear(start2, end2, end1);
		}

		/// <summary>
		/// Returns if the line segment start2 - end2 intersects the line segment start1 - end1.
		/// If only the endpoints coincide, the result is undefined (may be true or false).
		///
		/// Note: XZ space
		/// </summary>
		public static bool SegmentsIntersectXZ (Int3 start1, Int3 end1, Int3 start2, Int3 end2) {
			return RightOrColinearXZ(start1, end1, start2) != RightOrColinearXZ(start1, end1, end2) && RightOrColinearXZ(start2, end2, start1) != RightOrColinearXZ(start2, end2, end1);
		}

		/// <summary>
		/// Returns if the two line segments intersects. The lines are NOT treated as infinite (just for clarification)
		/// See: IntersectionPoint
		/// </summary>
		public static bool SegmentsIntersectXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2) {
			Vector3 dir1 = end1-start1;
			Vector3 dir2 = end2-start2;

			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				return false;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float nom2 = dir1.x*(start1.z-start2.z) - dir1.z * (start1.x - start2.x);
			float u = nom/den;
			float u2 = nom2/den;

			if (u < 0F || u > 1F || u2 < 0F || u2 > 1F) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates the intersection points between a "capsule" (segment expanded by a radius), and a line.
		///
		/// Returns: (t1, t2), the intersection points on the form lineStart + lineDir*t. Where t2 >= t1. If t2 < t1 then there are no intersections.
		/// </summary>
		/// <param name="capsuleStart">Center of the capsule's first circle</param>
		/// <param name="capsuleDir">Main axis of the capsule. Must be normalized.</param>
		/// <param name="capsuleLength">Distance betwen the capsule's circle centers.</param>
		/// <param name="lineStart">A point on the line</param>
		/// <param name="lineDir">The (normalized) direction of the line.</param>
		/// <param name="radius">The radius of the circle.</param>
		public static float2 CapsuleLineIntersectionFactors (float2 capsuleStart, float2 capsuleDir, float capsuleLength, float2 lineStart, float2 lineDir, float radius) {
			var cosAlpha = math.dot(capsuleDir, lineDir);
			var sinAlpha = math.sqrt(1.0f - cosAlpha*cosAlpha);
			var tmin = float.PositiveInfinity;
			var tmax = float.NegativeInfinity;

			if (LineCircleIntersectionFactors(lineStart - capsuleStart, lineDir, radius, out float t11, out float t12)) {
				tmin = math.min(tmin, t11);
				tmax = math.max(tmax, t12);
			}
			if (LineCircleIntersectionFactors(lineStart - (capsuleStart + capsuleDir*capsuleLength), lineDir, radius, out float t21, out float t22)) {
				tmin = math.min(tmin, t21);
				tmax = math.max(tmax, t22);
			}

			if (LineLineIntersectionFactor(capsuleStart, capsuleDir, lineStart, lineDir, out float ucenter)) {
				var normal = new float2(-capsuleDir.y, capsuleDir.x);
				var offset = radius * cosAlpha / sinAlpha;
				var side = math.sign(capsuleDir.y*lineDir.x - capsuleDir.x*lineDir.y);
				var ustraight1 = ucenter + offset*side;
				var ustraight2 = ucenter - offset*side;
				if (ustraight1 >= 0 && ustraight1 <= capsuleLength) {
					var p = capsuleStart + capsuleDir * ustraight1 - normal * radius;
					var tstraight1 = math.dot(p - lineStart, lineDir);
					tmin = math.min(tmin, tstraight1);
					tmax = math.max(tmax, tstraight1);
				}
				if (ustraight2 >= 0 && ustraight2 <= capsuleLength) {
					var p = capsuleStart + capsuleDir * ustraight2 + normal * radius;
					var tstraight2 = math.dot(p - lineStart, lineDir);
					tmin = math.min(tmin, tstraight2);
					tmax = math.max(tmax, tstraight2);
				}
			} else {
				// Parallel, or almost parallel.
				// In this case we can just rely on the circle intersection checks.
			}

			return new float2(tmin, tmax);
		}

		/// <summary>
		/// Calculates the point start1 + dir1*t where the two infinite lines intersect.
		/// Returns false if the lines are close to parallel.
		/// </summary>
		public static bool LineLineIntersectionFactor (float2 start1, float2 dir1, float2 start2, float2 dir2, out float t) {
			float den = dir2.y*dir1.x - dir2.x * dir1.y;

			if (math.abs(den) < 0.0001f) {
				t = 0;
				return false;
			}

			float nom = dir2.x*(start1.y-start2.y) - dir2.y*(start1.x-start2.x);
			t = nom/den;
			return true;
		}

		/// <summary>
		/// Calculates the point start1 + dir1*factor1 == start2 + dir2*factor2 where the two infinite lines intersect.
		/// Returns false if the lines are close to parallel.
		/// </summary>
		public static bool LineLineIntersectionFactors (float2 start1, float2 dir1, float2 start2, float2 dir2, out float factor1, out float factor2) {
			float den = dir2.y*dir1.x - dir2.x * dir1.y;

			if (math.abs(den) < 0.0001f) {
				factor1 = factor2 = 0;
				return false;
			}

			float nom1 = dir2.x*(start1.y-start2.y) - dir2.y*(start1.x-start2.x);
			float nom2 = dir1.x*(start1.y-start2.y) - dir1.y*(start1.x - start2.x);
			factor1 = nom1/den;
			factor2 = nom2/den;
			return true;
		}

		/// <summary>
		/// Intersection point between two infinite lines.
		/// Note that start points and directions are taken as parameters instead of start and end points.
		/// Lines are treated as infinite. If the lines are parallel 'start1' will be returned.
		/// Intersections are calculated on the XZ plane.
		///
		/// See: LineIntersectionPointXZ
		/// </summary>
		public static Vector3 LineDirIntersectionPointXZ (Vector3 start1, Vector3 dir1, Vector3 start2, Vector3 dir2) {
			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				return start1;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float u = nom/den;

			return start1 + dir1*u;
		}

		/// <summary>
		/// Intersection point between two infinite lines.
		/// Note that start points and directions are taken as parameters instead of start and end points.
		/// Lines are treated as infinite. If the lines are parallel 'start1' will be returned.
		/// Intersections are calculated on the XZ plane.
		///
		/// See: LineIntersectionPointXZ
		/// </summary>
		public static Vector3 LineDirIntersectionPointXZ (Vector3 start1, Vector3 dir1, Vector3 start2, Vector3 dir2, out bool intersects) {
			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				intersects = false;
				return start1;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float u = nom/den;

			intersects = true;
			return start1 + dir1*u;
		}

		/// <summary>
		/// Returns if the ray (start1, end1) intersects the segment (start2, end2).
		/// false is returned if the lines are parallel.
		/// Only the XZ coordinates are used.
		/// TODO: Double check that this actually works
		/// </summary>
		public static bool RaySegmentIntersectXZ (Int3 start1, Int3 end1, Int3 start2, Int3 end2) {
			Int3 dir1 = end1-start1;
			Int3 dir2 = end2-start2;

			long den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				return false;
			}

			long nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			long nom2 = dir1.x*(start1.z-start2.z) - dir1.z * (start1.x - start2.x);

			//factor1 < 0
			// If both have the same sign, then nom/den < 0 and thus the segment cuts the ray before the ray starts
			if (!(nom < 0 ^ den < 0)) {
				return false;
			}

			//factor2 < 0
			if (!(nom2 < 0 ^ den < 0)) {
				return false;
			}

			if ((den >= 0 && nom2 > den) || (den < 0 && nom2 <= den)) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns the intersection factors for line 1 and line 2. The intersection factors is a distance along the line start - end where the other line intersects it.
		/// <code> intersectionPoint = start1 + factor1 * (end1-start1) </code>
		/// <code> intersectionPoint2 = start2 + factor2 * (end2-start2) </code>
		/// Lines are treated as infinite.
		/// false is returned if the lines are parallel and true if they are not.
		/// Only the XZ coordinates are used.
		/// </summary>
		public static bool LineIntersectionFactorXZ (Int3 start1, Int3 end1, Int3 start2, Int3 end2, out float factor1, out float factor2) {
			Int3 dir1 = end1-start1;
			Int3 dir2 = end2-start2;

			long den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				factor1 = 0;
				factor2 = 0;
				return false;
			}

			long nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			long nom2 = dir1.x*(start1.z-start2.z) - dir1.z * (start1.x - start2.x);

			factor1 = (float)nom/den;
			factor2 = (float)nom2/den;

			return true;
		}

		/// <summary>
		/// Returns the intersection factors for line 1 and line 2. The intersection factors is a distance along the line start - end where the other line intersects it.
		/// <code> intersectionPoint = start1 + factor1 * (end1-start1) </code>
		/// <code> intersectionPoint2 = start2 + factor2 * (end2-start2) </code>
		/// Lines are treated as infinite.
		/// false is returned if the lines are parallel and true if they are not.
		/// Only the XZ coordinates are used.
		/// </summary>
		public static bool LineIntersectionFactorXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out float factor1, out float factor2) {
			Vector3 dir1 = end1-start1;
			Vector3 dir2 = end2-start2;

			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den <= 0.00001f && den >= -0.00001f) {
				factor1 = 0;
				factor2 = 0;
				return false;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float nom2 = dir1.x*(start1.z-start2.z) - dir1.z * (start1.x - start2.x);

			float u = nom/den;
			float u2 = nom2/den;

			factor1 = u;
			factor2 = u2;

			return true;
		}

		/// <summary>
		/// Returns the intersection factor for line 1 with ray 2.
		/// The intersection factors is a factor distance along the line start - end where the other line intersects it.
		/// <code> intersectionPoint = start1 + factor * (end1-start1) </code>
		/// Lines are treated as infinite.
		///
		/// The second "line" is treated as a ray, meaning only matches on start2 or forwards towards end2 (and beyond) will be returned
		/// If the point lies on the wrong side of the ray start, Nan will be returned.
		///
		/// NaN is returned if the lines are parallel.
		/// </summary>
		public static float LineRayIntersectionFactorXZ (Int3 start1, Int3 end1, Int3 start2, Int3 end2) {
			Int3 dir1 = end1-start1;
			Int3 dir2 = end2-start2;

			int den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				return float.NaN;
			}

			int nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			int nom2 = dir1.x*(start1.z-start2.z) - dir1.z * (start1.x - start2.x);

			if ((float)nom2/den < 0) {
				return float.NaN;
			}
			return (float)nom/den;
		}

		/// <summary>
		/// Returns the intersection factor for line 1 with line 2.
		/// The intersection factor is a distance along the line start1 - end1 where the line start2 - end2 intersects it.
		/// <code> intersectionPoint = start1 + intersectionFactor * (end1-start1) </code>.
		/// Lines are treated as infinite.
		/// -1 is returned if the lines are parallel (note that this is a valid return value if they are not parallel too)
		/// </summary>
		public static float LineIntersectionFactorXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2) {
			Vector3 dir1 = end1-start1;
			Vector3 dir2 = end2-start2;

			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				return -1;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float u = nom/den;

			return u;
		}

		/// <summary>Returns the intersection point between the two lines. Lines are treated as infinite. start1 is returned if the lines are parallel</summary>
		public static Vector3 LineIntersectionPointXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2) {
			bool s;

			return LineIntersectionPointXZ(start1, end1, start2, end2, out s);
		}

		/// <summary>Returns the intersection point between the two lines. Lines are treated as infinite. start1 is returned if the lines are parallel</summary>
		public static Vector3 LineIntersectionPointXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out bool intersects) {
			Vector3 dir1 = end1-start1;
			Vector3 dir2 = end2-start2;

			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				intersects = false;
				return start1;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);

			float u = nom/den;

			intersects = true;
			return start1 + dir1*u;
		}

		/// <summary>Returns the intersection point between the two lines. Lines are treated as infinite. start1 is returned if the lines are parallel</summary>
		public static Vector2 LineIntersectionPoint (Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2) {
			bool s;

			return LineIntersectionPoint(start1, end1, start2, end2, out s);
		}

		/// <summary>Returns the intersection point between the two lines. Lines are treated as infinite. start1 is returned if the lines are parallel</summary>
		public static Vector2 LineIntersectionPoint (Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out bool intersects) {
			Vector2 dir1 = end1-start1;
			Vector2 dir2 = end2-start2;

			float den = dir2.y*dir1.x - dir2.x * dir1.y;

			if (den == 0) {
				intersects = false;
				return start1;
			}

			float nom = dir2.x*(start1.y-start2.y)- dir2.y*(start1.x-start2.x);

			float u = nom/den;

			intersects = true;
			return start1 + dir1*u;
		}

		/// <summary>
		/// Returns the intersection point between the two line segments in XZ space.
		/// Lines are NOT treated as infinite. start1 is returned if the line segments do not intersect
		/// The point will be returned along the line [start1, end1] (this matters only for the y coordinate).
		/// </summary>
		public static Vector3 SegmentIntersectionPointXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out bool intersects) {
			Vector3 dir1 = end1-start1;
			Vector3 dir2 = end2-start2;

			float den = dir2.z * dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				intersects = false;
				return start1;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float nom2 = dir1.x*(start1.z-start2.z) - dir1.z*(start1.x-start2.x);
			float u = nom/den;
			float u2 = nom2/den;

			if (u < 0F || u > 1F || u2 < 0F || u2 > 1F) {
				intersects = false;
				return start1;
			}

			intersects = true;
			return start1 + dir1*u;
		}

		/// <summary>
		/// Does the line segment intersect the bounding box.
		/// The line is NOT treated as infinite.
		/// \author Slightly modified code from http://www.3dkingdoms.com/weekly/weekly.php?a=21
		/// </summary>
		public static bool SegmentIntersectsBounds (Bounds bounds, Vector3 a, Vector3 b) {
			// Put segment in box space
			a -= bounds.center;
			b -= bounds.center;

			// Get line midpoint and extent
			var LMid = (a + b) * 0.5F;
			var L = (a - LMid);
			var LExt = new Vector3(Math.Abs(L.x), Math.Abs(L.y), Math.Abs(L.z));

			Vector3 extent = bounds.extents;

			// Use Separating Axis Test
			// Separation vector from box center to segment center is LMid, since the line is in box space
			if (Math.Abs(LMid.x) > extent.x + LExt.x) return false;
			if (Math.Abs(LMid.y) > extent.y + LExt.y) return false;
			if (Math.Abs(LMid.z) > extent.z + LExt.z) return false;
			// Crossproducts of line and each axis
			if (Math.Abs(LMid.y * L.z - LMid.z * L.y) > (extent.y * LExt.z + extent.z * LExt.y)) return false;
			if (Math.Abs(LMid.x * L.z - LMid.z * L.x) > (extent.x * LExt.z + extent.z * LExt.x)) return false;
			if (Math.Abs(LMid.x * L.y - LMid.y * L.x) > (extent.x * LExt.y + extent.y * LExt.x)) return false;
			// No separating axis, the line intersects
			return true;
		}

		/// <summary>
		/// Calculates the two intersection points (point + direction*t) on the line where it intersects with a circle at the origin.
		///
		/// t1 will always be less than or equal to t2 if there are intersections.
		///
		/// Returns false if there are no intersections.
		/// </summary>
		/// <param name="point">A point on the line</param>
		/// <param name="direction">The normalized direction of the line</param>
		/// <param name="radius">The radius of the circle at the origin.</param>
		/// <param name="t1">The first intersection (if any).</param>
		/// <param name="t2">The second intersection (if any).</param>
		public static bool LineCircleIntersectionFactors (float2 point, float2 direction, float radius, out float t1, out float t2) {
			// Distance from the closest point on the line (from the origin) to line.point
			float dot = math.dot(point, direction);
			// Squared distance from the origin to the closest point on the line
			float distanceToLine = math.lengthsq(point) - dot*dot;
			// Calculate the intersection of the line with the circle.
			// This is the squared length of half the chord that intersects the circle.
			float discriminant = radius*radius - distanceToLine;

			if (discriminant < 0.0f) {
				// The line is completely outside the circle
				t1 = float.PositiveInfinity;
				t2 = float.NegativeInfinity;
				return false;
			}

			var sqrtDiscriminant = math.sqrt(discriminant);
			t1 = -dot - sqrtDiscriminant;
			t2 = -dot + sqrtDiscriminant;
			return true;
		}

		/// <summary>
		/// Calculates the two intersection points (lerp(point1, point2, t)) on the segment where it intersects with a circle at the origin.
		///
		/// t1 will always be less than or equal to t2 if there are intersections.
		///
		/// Returns false if there are no intersections.
		/// </summary>
		/// <param name="point1">Start of the segment</param>
		/// <param name="point2">End of the segment</param>
		/// <param name="radiusSq">The squared radius of the circle at the origin.</param>
		/// <param name="t1">The first intersection (if any). Between 0 and 1.</param>
		/// <param name="t2">The second intersection (if any). Between 0 and 1.</param>
		public static bool SegmentCircleIntersectionFactors (float2 point1, float2 point2, float radiusSq, out float t1, out float t2) {
			// Distance from the closest point on the line (from the origin) to line.point
			var dir = point2 - point1;
			var dirSq = math.lengthsq(dir);
			float dot = math.dot(point1, dir) / dirSq;
			// Proportional to the squared distance from the origin to the closest point on the line
			float distanceToLine = math.lengthsq(point1) / dirSq - dot*dot;
			float discriminant = radiusSq/dirSq - distanceToLine;

			if (discriminant < 0.0f) {
				// The line is completely outside the circle
				t1 = float.PositiveInfinity;
				t2 = float.NegativeInfinity;
				return false;
			}

			var sqrtDiscriminant = math.sqrt(discriminant);
			t1 = -dot - sqrtDiscriminant;
			t2 = -dot + sqrtDiscriminant;
			t1 = math.max(0, t1);
			t2 = math.min(1, t2);

			if (t1 >= 1 || t2 <= 0) return false;
			return true;
		}

		/// <summary>
		/// Intersection of a line and a circle.
		/// Returns the greatest t such that segmentStart+t*(segmentEnd-segmentStart) lies on the circle.
		///
		/// In case the line does not intersect with the circle, the closest point on the line
		/// to the circle will be returned.
		///
		/// Note: Works for line and sphere in 3D space as well.
		///
		/// See: http://mathworld.wolfram.com/Circle-LineIntersection.html
		/// See: https://en.wikipedia.org/wiki/Intersection_(Euclidean_geometry)<see cref="A_line_and_a_circle"/>
		/// </summary>
		public static float LineCircleIntersectionFactor (Vector3 circleCenter, Vector3 linePoint1, Vector3 linePoint2, float radius) {
			float segmentLength;
			var normalizedDirection = Normalize(linePoint2 - linePoint1, out segmentLength);
			var dirToStart = linePoint1 - circleCenter;

			var dot = Vector3.Dot(dirToStart, normalizedDirection);
			var discriminant = dot * dot - (dirToStart.sqrMagnitude - radius*radius);

			if (discriminant < 0) {
				// No intersection, pick closest point on segment
				discriminant = 0;
			}

			var t = -dot + Mathf.Sqrt(discriminant);
			// Note: the default value of 1 is important for the PathInterpolator.MoveToCircleIntersection2D
			// method to work properly. Maybe find some better abstraction where this default value is more obvious.
			return segmentLength > 0.00001f ? t / segmentLength : 1f;
		}

		/// <summary>
		/// True if the matrix will reverse orientations of faces.
		///
		/// Scaling by a negative value along an odd number of axes will reverse
		/// the orientation of e.g faces on a mesh. This must be counter adjusted
		/// by for example the recast rasterization system to be able to handle
		/// meshes with negative scales properly.
		///
		/// We can find out if they are flipped by finding out how the signed
		/// volume of a unit cube is transformed when applying the matrix
		///
		/// If the (signed) volume turns out to be negative
		/// that also means that the orientation of it has been reversed.
		///
		/// See: https://en.wikipedia.org/wiki/Normal_(geometry)
		/// See: https://en.wikipedia.org/wiki/Parallelepiped
		/// </summary>
		public static bool ReversesFaceOrientations (Matrix4x4 matrix) {
			var dX = matrix.MultiplyVector(new Vector3(1, 0, 0));
			var dY = matrix.MultiplyVector(new Vector3(0, 1, 0));
			var dZ = matrix.MultiplyVector(new Vector3(0, 0, 1));

			// Calculate the signed volume of the parallelepiped
			var volume = Vector3.Dot(Vector3.Cross(dX, dY), dZ);

			return volume < 0;
		}

		/// <summary>
		/// Normalize vector and also return the magnitude.
		/// This is more efficient than calculating the magnitude and normalizing separately
		/// </summary>
		public static Vector3 Normalize (Vector3 v, out float magnitude) {
			magnitude = v.magnitude;
			// This is the same constant that Unity uses
			if (magnitude > 1E-05f) {
				return v / magnitude;
			} else {
				return Vector3.zero;
			}
		}

		/// <summary>
		/// Normalize vector and also return the magnitude.
		/// This is more efficient than calculating the magnitude and normalizing separately
		/// </summary>
		public static Vector2 Normalize (Vector2 v, out float magnitude) {
			magnitude = v.magnitude;
			// This is the same constant that Unity uses
			if (magnitude > 1E-05f) {
				return v / magnitude;
			} else {
				return Vector2.zero;
			}
		}

		/* Clamp magnitude along the X and Z axes.
		 * The y component will not be changed.
		 */
		public static Vector3 ClampMagnitudeXZ (Vector3 v, float maxMagnitude) {
			float squaredMagnitudeXZ = v.x*v.x + v.z*v.z;

			if (squaredMagnitudeXZ > maxMagnitude*maxMagnitude && maxMagnitude > 0) {
				var factor = maxMagnitude / Mathf.Sqrt(squaredMagnitudeXZ);
				v.x *= factor;
				v.z *= factor;
			}
			return v;
		}

		/* Magnitude in the XZ plane */
		public static float MagnitudeXZ (Vector3 v) {
			return Mathf.Sqrt(v.x*v.x + v.z*v.z);
		}

		/// <summary>
		/// Number of radians that this quaternion rotates around its axis of rotation.
		/// Will be in the range [-PI, PI].
		///
		/// Note: A quaternion of q and -q represent the same rotation, but their axis of rotation point in opposite directions, so the angle will be different.
		/// </summary>
		public static float QuaternionAngle (quaternion rot) {
			return 2 * math.atan2(math.length(rot.value.xyz), rot.value.w);
		}
	}

	/// <summary>
	/// Utility functions for working with numbers and strings.
	///
	/// See: Polygon
	/// See: VectorMath
	/// </summary>
	public static class AstarMath {
		static Unity.Mathematics.Random GlobalRandom = Unity.Mathematics.Random.CreateFromIndex(0);
		static object GlobalRandomLock = new object();

		public static float ThreadSafeRandomFloat () {
			lock (GlobalRandomLock) {
				return GlobalRandom.NextFloat();
			}
		}

		public static float2 ThreadSafeRandomFloat2 () {
			lock (GlobalRandomLock) {
				return GlobalRandom.NextFloat2();
			}
		}

		/// <summary>Converts a non-negative float to a long, saturating at long.MaxValue if the value is too large</summary>
		public static long SaturatingConvertFloatToLong(float v) => v > (float)long.MaxValue ? long.MaxValue : (long)v;

		/// <summary>Maps a value between startMin and startMax to be between targetMin and targetMax</summary>
		public static float MapTo (float startMin, float startMax, float targetMin, float targetMax, float value) {
			return Mathf.Lerp(targetMin, targetMax, Mathf.InverseLerp(startMin, startMax, value));
		}

		/// <summary>
		/// Returns bit number b from int a. The bit number is zero based. Relevant b values are from 0 to 31.
		/// Equals to (a >> b) & 1
		/// </summary>
		static int Bit (int a, int b) {
			return (a >> b) & 1;
		}

		/// <summary>
		/// Returns a nice color from int i with alpha a. Got code from the open-source Recast project, works really well.
		/// Seems like there are only 64 possible colors from studying the code
		/// </summary>
		public static Color IntToColor (int i, float a) {
			int r = Bit(i, 2) + Bit(i, 3) * 2 + 1;
			int g = Bit(i, 1) + Bit(i, 4) * 2 + 1;
			int b = Bit(i, 0) + Bit(i, 5) * 2 + 1;

			return new Color(r*0.25F, g*0.25F, b*0.25F, a);
		}

		/// <summary>
		/// Converts an HSV color to an RGB color.
		/// According to the algorithm described at http://en.wikipedia.org/wiki/HSL_and_HSV
		///
		/// @author Wikipedia
		/// @return the RGB representation of the color.
		/// </summary>
		public static Color HSVToRGB (float h, float s, float v) {
			float r = 0, g = 0, b = 0;

			float Chroma = s * v;
			float Hdash = h / 60.0f;
			float X = Chroma * (1.0f - System.Math.Abs((Hdash % 2.0f) - 1.0f));

			if (Hdash < 1.0f) {
				r = Chroma;
				g = X;
			} else if (Hdash < 2.0f) {
				r = X;
				g = Chroma;
			} else if (Hdash < 3.0f) {
				g = Chroma;
				b = X;
			} else if (Hdash < 4.0f) {
				g = X;
				b = Chroma;
			} else if (Hdash < 5.0f) {
				r = X;
				b = Chroma;
			} else if (Hdash < 6.0f) {
				r = Chroma;
				b = X;
			}

			float Min = v - Chroma;

			r += Min;
			g += Min;
			b += Min;

			return new Color(r, g, b);
		}

		/// <summary>
		/// Calculates the shortest difference between two given angles given in radians.
		///
		/// The return value will be between -pi/2 and +pi/2.
		/// </summary>
		public static float DeltaAngle (float angle1, float angle2) {
			float diff = (angle2 - angle1 + math.PI) % (2*math.PI) - math.PI;
			return math.select(diff, diff + 2*math.PI, diff < -math.PI);
		}
	}

	/// <summary>
	/// Utility functions for working with polygons, lines, and other vector math.
	/// All functions which accepts Vector3s but work in 2D space uses the XZ space if nothing else is said.
	///
	/// Version: A lot of functions in this class have been moved to the VectorMath class
	/// the names have changed slightly and everything now consistently assumes a left handed
	/// coordinate system now instead of sometimes using a left handed one and sometimes
	/// using a right handed one. This is why the 'Left' methods redirect to methods
	/// named 'Right'. The functionality is exactly the same.
	/// </summary>
	[BurstCompile]
	public static class Polygon {
		/// <summary>
		/// Returns if the triangle ABC contains the point p in XZ space.
		/// The triangle vertices are assumed to be laid out in clockwise order.
		/// </summary>
		public static bool ContainsPointXZ (Vector3 a, Vector3 b, Vector3 c, Vector3 p) {
			return VectorMath.IsClockwiseMarginXZ(a, b, p) && VectorMath.IsClockwiseMarginXZ(b, c, p) && VectorMath.IsClockwiseMarginXZ(c, a, p);
		}

		/// <summary>
		/// Returns if the triangle ABC contains the point p.
		/// The triangle vertices are assumed to be laid out in clockwise order.
		/// </summary>
		public static bool ContainsPointXZ (Int3 a, Int3 b, Int3 c, Int3 p) {
			return VectorMath.IsClockwiseOrColinearXZ(a, b, p) && VectorMath.IsClockwiseOrColinearXZ(b, c, p) && VectorMath.IsClockwiseOrColinearXZ(c, a, p);
		}

		/// <summary>
		/// Returns if the triangle ABC contains the point p.
		/// The triangle vertices are assumed to be laid out in clockwise order.
		/// </summary>
		public static bool ContainsPoint (Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int p) {
			return VectorMath.IsClockwiseOrColinear(a, b, p) && VectorMath.IsClockwiseOrColinear(b, c, p) && VectorMath.IsClockwiseOrColinear(c, a, p);
		}

		/// <summary>
		/// Checks if p is inside the polygon.
		/// \author http://unifycommunity.com/wiki/index.php?title=PolyContainsPoint (Eric5h5)
		/// </summary>
		public static bool ContainsPoint (Vector2[] polyPoints, Vector2 p) {
			int j = polyPoints.Length-1;
			bool inside = false;

			for (int i = 0; i < polyPoints.Length; j = i++) {
				if (((polyPoints[i].y <= p.y && p.y < polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y < polyPoints[i].y)) &&
					(p.x < (polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x))
					inside = !inside;
			}
			return inside;
		}

		/// <summary>
		/// Checks if p is inside the polygon (XZ space).
		/// \author http://unifycommunity.com/wiki/index.php?title=PolyContainsPoint (Eric5h5)
		/// </summary>
		public static bool ContainsPointXZ (Vector3[] polyPoints, Vector3 p) {
			int j = polyPoints.Length-1;
			bool inside = false;

			for (int i = 0; i < polyPoints.Length; j = i++) {
				if (((polyPoints[i].z <= p.z && p.z < polyPoints[j].z) || (polyPoints[j].z <= p.z && p.z < polyPoints[i].z)) &&
					(p.x < (polyPoints[j].x - polyPoints[i].x) * (p.z - polyPoints[i].z) / (polyPoints[j].z - polyPoints[i].z) + polyPoints[i].x))
					inside = !inside;
			}
			return inside;
		}

		/// <summary>
		/// Returns if the triangle contains the point p when projected on the movement plane.
		/// The triangle vertices may be clockwise or counter-clockwise.
		///
		/// This method is numerically robust, as in, if the point is contained in exactly one of two adjacent triangles, then this
		/// function will return true for at least one of them (both if the point is exactly on the edge between them).
		/// If it was less numerically robust, it could conceivably return false for both of them if the point was on the edge between them, which would be bad.
		/// </summary>
		[BurstCompile]
		public static bool ContainsPoint (ref int3 aWorld, ref int3 bWorld, ref int3 cWorld, ref int3 pWorld, ref NativeMovementPlane movementPlane) {
			// Extract the coordinate axes of the movement plane
			var m = new float3x3(movementPlane.rotation.value);
			var m2D = math.transpose(new float3x2(m.c0, m.c2));
			return ContainsPoint(ref aWorld, ref bWorld, ref cWorld, ref pWorld, in m2D);
		}

		/// <summary>
		/// Returns if the triangle contains the point p when projected on a plane using the given projection.
		/// The triangle vertices may be clockwise or counter-clockwise.
		///
		/// This method is numerically robust, as in, if the point is contained in exactly one of two adjacent triangles, then this
		/// function will return true for at least one of them (both if the point is exactly on the edge between them).
		/// If it was less numerically robust, it could conceivably return false for both of them if the point was on the edge between them, which would be bad.
		/// </summary>
		public static bool ContainsPoint (ref int3 aWorld, ref int3 bWorld, ref int3 cWorld, ref int3 pWorld, in float2x3 planeProjection) {
			const int QUANTIZATION = 1024;
			var m = new int2x3(planeProjection * QUANTIZATION);
			// Project all the points onto the movement plane using SIMD
			var xs = new int4(aWorld.x, bWorld.x, cWorld.x, pWorld.x);
			var ys = new int4(aWorld.y, bWorld.y, cWorld.y, pWorld.y);
			var zs = new int4(aWorld.z, bWorld.z, cWorld.z, pWorld.z);
			// Subtract the first point from all the other points
			// This ensures that large coordinates will not overflow due to using 32 bits here.
			// Since we multiply all coordinates by QUANTIZATION, and Int3 coordinates are already multiplied by 1000,
			// coordinates would otherwise be liable to start overflowing at unity world coordinates above around 2000.
			// TODO: We could still get bad results if pWorld is very far away from the triangle (about 4000 units).
			xs -= xs.x;
			ys -= ys.x;
			zs -= zs.x;
			// Projected X and Y coordinates
			var px = (xs * m.c0.x + ys * m.c1.x + zs * m.c2.x) / QUANTIZATION;
			var py = (xs * m.c0.y + ys * m.c1.y + zs * m.c2.y) / QUANTIZATION;

			// Do 3 cross products to check if the point is inside the triangle
			var v1 = px.yzx - px.xyz;
			var v2 = py.www - py.xyz;
			var v3 = px.www - px.xyz;
			var v4 = py.yzx - py.xyz;
			long check1 = (long)v1.x * (long)v2.x - (long)v3.x * (long)v4.x;
			long check2 = (long)v1.y * (long)v2.y - (long)v3.y * (long)v4.y;
			long check3 = (long)v1.z * (long)v2.z - (long)v3.z * (long)v4.z;
			// Allow for both clockwise and counter-clockwise triangle layouts.
			// This can be important sometimes on spherical worlds where the "upside-down" triangles
			// will be seen as having the reverse winding order when projected onto a plane.
			// We take care to include points right on the edge of the triangle.
			return (check1 >= 0 & check2 >= 0 & check3 >= 0) | (check1 <= 0 & check2 <= 0 & check3 <= 0);

			// Note: It might be tempting to try to use SIMD-like code for this. But the following requires a lot more instructions, as it turns out.
			// return math.all(new bool3(check1 >= 0, check2 >= 0, check3 >= 0)) || math.all(new bool3(check1 <= 0, check2 <= 0, check3 <= 0));
		}

		public struct BarycentricTriangleInterpolator {
			int2 origin;
			double2x2 barycentricMapping;
			double3 thresholds, linear1, linear2, linear3, ys;

			public BarycentricTriangleInterpolator(Int3 p1, Int3 p2, Int3 p3) {
				double signedTriangleAreaTimes2 = ((double)(p2.z - p3.z)) * (p1.x - p3.x) + ((double)(p3.x - p2.x)) * (p1.z - p3.z);
				var d12 = new double2(p2.x - p1.x, p2.z - p1.z);
				var d23 = new double2(p3.x - p2.x, p3.z - p2.z);
				var d31 = new double2(p1.x - p3.x, p1.z - p3.z);
				var l12 = math.lengthsq(d12);
				var l23 = math.lengthsq(d23);
				var l31 = math.lengthsq(d31);

				// When interpolating a point that is very close to the edge of the triangle (or even outside it)
				// we snap it to the edge of the triangle.
				// This is to improve robustness when there are two adjacent triangles:
				// If the query point is almost exactly on the edge between them, we want
				// SampleY to return the same y coordinate for both triangles.
				// This may not be the case if using pure barycentric interpolation.
				// Due to precision limitations, the integer coordinates may be slightly off
				// from the precise mathematical edge of the triangle.
				// A value of 1 seems to work fine in practice, but we use 2 for a bit of extra margin.
				const double MaxEdgeSnappingDistance = 2;
				// Thresholds for the barycentric coordinates. If they are smaller than this, then the point is very close to the edge of the triangle.
				// This follows from https://en.wikipedia.org/wiki/Barycentric_coordinate_system#Barycentric_coordinates_on_triangles
				// For barycentric coordinates (b1,b2,b3) it holds that
				// b1 = area(p2,p3,p) / area(p1,p2,p3)
				// = (distanceToLine(p2,p3,p)*length(p2,p3)/2) / area(p1,p2,p3)       (since a triangle's area = base*height/2)
				// = distanceToLine(p2,p3,p) * length(p2,p3) / (2*area(p1,p2,p3))
				// If we want to check if distanceToLine is smaller than MaxEdgeSnappingDistance, we get the threshold below
				// for all the barycentric coordinates.
				thresholds = math.sqrt(new double3(l23, l31, l12)) * (MaxEdgeSnappingDistance / math.abs(signedTriangleAreaTimes2));
				origin = new int2(p3.x, p3.z);

				// Linear mapping from 2D space (relative to origin) to barycentric coordinates
				barycentricMapping = new double2x2(
					-d23.y, d23.x,
					-d31.y, d31.x
					) / signedTriangleAreaTimes2;

				ys = new double3(p1.y, p2.y, p3.y);

				// Creates a mapping m(p) from 2D projective space (x,z,1) relative to the origin, to a y coordinate for the point on the line a-b that is closest to (x,z).
				double3 ProjectPointOnLine (Int3 a, Int3 b, double lengthSq, int2 origin) {
					// m1(p) = dot((p + origin) - a, b - a) / lengthSq;
					// = (dot(p, b - a) + dot(origin - a, b - a)) / lengthSq;
					var m1 = new double3(
						b.x - a.x,
						b.z - a.z,
						(double)(origin.x - a.x) * (b.x - a.x) + (double)(origin.y - a.z) * (b.z - a.z)
						) / lengthSq;
					// m2(p) = lerp(a, b, m1(p)) = a + m1(p) * (b - a)
					var m2 = m1 * (b.y - a.y) + new double3(0, 0, a.y);
					return m2;
				}

				linear1 = ProjectPointOnLine(p2, p3, l23, origin);
				linear2 = ProjectPointOnLine(p3, p1, l31, origin);
				linear3 = ProjectPointOnLine(p1, p2, l12, origin);
			}

			public int SampleY (int2 p) {
				p -= origin;

				double2 lambdas = math.mul(this.barycentricMapping, new double2(p.x, p.y));
				double3 lambdas2 = new double3(lambdas.x, lambdas.y, 1 - lambdas.x - lambdas.y);

				if (lambdas2.x < thresholds.x) {
					// Interpolate only along the edge p2-p3 for numerical stability
					return (int)math.round(math.dot(linear1, new double3(p.x, p.y, 1)));
				}
				if (lambdas2.y < thresholds.y) {
					// Interpolate only along the edge p3-p1 for numerical stability
					return (int)math.round(math.dot(linear2, new double3(p.x, p.y, 1)));
				}
				if (lambdas2.z < thresholds.z) {
					// Interpolate only along the edge p1-p2 for numerical stability
					return (int)math.round(math.dot(linear3, new double3(p.x, p.y, 1)));
				}
				return (int)math.round(math.dot(ys, lambdas2));
			}
		}

		/// <summary>
		/// Calculates convex hull in XZ space for the points.
		/// Implemented using the very simple Gift Wrapping Algorithm
		/// which has a complexity of O(nh) where n is the number of points and h is the number of points on the hull,
		/// so it is in the worst case quadratic.
		/// </summary>
		public static Vector3[] ConvexHullXZ (Vector3[] points) {
			if (points.Length == 0) return new Vector3[0];

			var hull = Pathfinding.Pooling.ListPool<Vector3>.Claim();

			int pointOnHull = 0;
			for (int i = 1; i < points.Length; i++) if (points[i].x < points[pointOnHull].x) pointOnHull = i;

			int startpoint = pointOnHull;
			int counter = 0;

			do {
				hull.Add(points[pointOnHull]);
				int endpoint = 0;
				for (int i = 0; i < points.Length; i++) if (endpoint == pointOnHull || !VectorMath.RightOrColinearXZ(points[pointOnHull], points[endpoint], points[i])) endpoint = i;

				pointOnHull = endpoint;

				counter++;
				if (counter > 10000) {
					Debug.LogWarning("Infinite Loop in Convex Hull Calculation");
					break;
				}
			} while (pointOnHull != startpoint);

			var result = hull.ToArray();

			// Return to pool
			Pathfinding.Pooling.ListPool<Vector3>.Release(hull);
			return result;
		}

		/// <summary>
		/// Closest point on the triangle abc to the point p.
		/// See: 'Real Time Collision Detection' by Christer Ericson, chapter 5.1, page 141
		/// </summary>
		public static Vector2 ClosestPointOnTriangle (Vector2 a, Vector2 b, Vector2 c, Vector2 p) {
			// Check if p is in vertex region outside A
			var ab = b - a;
			var ac = c - a;
			var ap = p - a;

			var d1 = Vector2.Dot(ab, ap);
			var d2 = Vector2.Dot(ac, ap);

			// Barycentric coordinates (1,0,0)
			if (d1 <= 0 && d2 <= 0) {
				return a;
			}

			// Check if p is in vertex region outside B
			var bp = p - b;
			var d3 = Vector2.Dot(ab, bp);
			var d4 = Vector2.Dot(ac, bp);

			// Barycentric coordinates (0,1,0)
			if (d3 >= 0 && d4 <= d3) {
				return b;
			}

			// Check if p is in edge region outside AB, if so return a projection of p onto AB
			if (d1 >= 0 && d3 <= 0) {
				var vc = d1 * d4 - d3 * d2;
				if (vc <= 0) {
					// Barycentric coordinates (1-v, v, 0)
					var v = d1 / (d1 - d3);
					return a + ab*v;
				}
			}

			// Check if p is in vertex region outside C
			var cp = p - c;
			var d5 = Vector2.Dot(ab, cp);
			var d6 = Vector2.Dot(ac, cp);

			// Barycentric coordinates (0,0,1)
			if (d6 >= 0 && d5 <= d6) {
				return c;
			}

			// Check if p is in edge region of AC, if so return a projection of p onto AC
			if (d2 >= 0 && d6 <= 0) {
				var vb = d5 * d2 - d1 * d6;
				if (vb <= 0) {
					// Barycentric coordinates (1-v, 0, v)
					var v = d2 / (d2 - d6);
					return a + ac*v;
				}
			}

			// Check if p is in edge region of BC, if so return projection of p onto BC
			if ((d4 - d3) >= 0 && (d5 - d6) >= 0) {
				var va = d3 * d6 - d5 * d4;
				if (va <= 0) {
					var v = (d4 - d3) / ((d4 - d3) + (d5 - d6));
					return b + (c - b) * v;
				}
			}

			return p;
		}

		/// <summary>
		/// Closest point on the triangle abc to the point p when seen from above.
		/// See: 'Real Time Collision Detection' by Christer Ericson, chapter 5.1, page 141
		/// </summary>
		public static Vector3 ClosestPointOnTriangleXZ (Vector3 a, Vector3 b, Vector3 c, Vector3 p) {
			// Check if p is in vertex region outside A
			var ab = new Vector2(b.x - a.x, b.z - a.z);
			var ac = new Vector2(c.x - a.x, c.z - a.z);
			var ap = new Vector2(p.x - a.x, p.z - a.z);

			var d1 = Vector2.Dot(ab, ap);
			var d2 = Vector2.Dot(ac, ap);

			// Barycentric coordinates (1,0,0)
			if (d1 <= 0 && d2 <= 0) {
				return a;
			}

			// Check if p is in vertex region outside B
			var bp = new Vector2(p.x - b.x, p.z - b.z);
			var d3 = Vector2.Dot(ab, bp);
			var d4 = Vector2.Dot(ac, bp);

			// Barycentric coordinates (0,1,0)
			if (d3 >= 0 && d4 <= d3) {
				return b;
			}

			// Check if p is in edge region outside AB, if so return a projection of p onto AB
			var vc = d1 * d4 - d3 * d2;
			if (d1 >= 0 && d3 <= 0 && vc <= 0) {
				// Barycentric coordinates (1-v, v, 0)
				var v = d1 / (d1 - d3);
				return (1-v)*a + v*b;
			}

			// Check if p is in vertex region outside C
			var cp = new Vector2(p.x - c.x, p.z - c.z);
			var d5 = Vector2.Dot(ab, cp);
			var d6 = Vector2.Dot(ac, cp);

			// Barycentric coordinates (0,0,1)
			if (d6 >= 0 && d5 <= d6) {
				return c;
			}

			// Check if p is in edge region of AC, if so return a projection of p onto AC
			var vb = d5 * d2 - d1 * d6;
			if (d2 >= 0 && d6 <= 0 && vb <= 0) {
				// Barycentric coordinates (1-v, 0, v)
				var v = d2 / (d2 - d6);
				return (1-v)*a + v*c;
			}

			// Check if p is in edge region of BC, if so return projection of p onto BC
			var va = d3 * d6 - d5 * d4;
			if ((d4 - d3) >= 0 && (d5 - d6) >= 0 && va <= 0) {
				var v = (d4 - d3) / ((d4 - d3) + (d5 - d6));
				return b + (c - b) * v;
			} else {
				// P is inside the face region. Compute the point using its barycentric coordinates (u, v, w)
				// Note that the x and z coordinates will be exactly the same as P's x and z coordinates
				var denom = 1f / (va + vb + vc);
				var v = vb * denom;
				var w = vc * denom;

				return new Vector3(p.x, (1 - v - w)*a.y + v*b.y + w*c.y, p.z);
			}
		}

		/// <summary>
		/// Closest point on the triangle abc to the point p.
		/// See: 'Real Time Collision Detection' by Christer Ericson, chapter 5.1, page 141
		/// </summary>
		public static float3 ClosestPointOnTriangle (float3 a, float3 b, float3 c, float3 p) {
			ClosestPointOnTriangleByRef(in a, in b, in c, in p, out var output);
			return output;
		}

		/// <summary>
		/// Closest point on the triangle abc to the point p.
		///
		/// Takes arguments by reference to be able to be burst-compiled.
		///
		/// See: 'Real Time Collision Detection' by Christer Ericson, chapter 5.1, page 141
		///
		/// Returns: True if the point is inside the triangle, false otherwise, after the point has been projected on the plane that the triangle is in.
		/// </summary>
		[BurstCompile]
		public static bool ClosestPointOnTriangleByRef (in float3 a, in float3 b, in float3 c, in float3 p, [NoAlias] out float3 output) {
			// Check if p is in vertex region outside A
			var ab = b - a;
			var ac = c - a;
			var ap = p - a;

			var d1 = math.dot(ab, ap);
			var d2 = math.dot(ac, ap);

			// Barycentric coordinates (1,0,0)
			if (d1 <= 0 && d2 <= 0) {
				output = a;
				return false;
			}

			// Check if p is in vertex region outside B
			var bp = p - b;
			var d3 = math.dot(ab, bp);
			var d4 = math.dot(ac, bp);

			// Barycentric coordinates (0,1,0)
			if (d3 >= 0 && d4 <= d3) {
				output = b;
				return false;
			}

			// Check if p is in edge region outside AB, if so return a projection of p onto AB
			var vc = d1 * d4 - d3 * d2;
			if (d1 >= 0 && d3 <= 0 && vc <= 0) {
				// Barycentric coordinates (1-v, v, 0)
				var v = d1 / (d1 - d3);
				output = a + ab * v;
				return false;
			}

			// Check if p is in vertex region outside C
			var cp = p - c;
			var d5 = math.dot(ab, cp);
			var d6 = math.dot(ac, cp);

			// Barycentric coordinates (0,0,1)
			if (d6 >= 0 && d5 <= d6) {
				output = c;
				return false;
			}

			// Check if p is in edge region of AC, if so return a projection of p onto AC
			var vb = d5 * d2 - d1 * d6;
			if (d2 >= 0 && d6 <= 0 && vb <= 0) {
				// Barycentric coordinates (1-v, 0, v)
				var v = d2 / (d2 - d6);
				output = a + ac * v;
				return false;
			}

			// Check if p is in edge region of BC, if so return projection of p onto BC
			var va = d3 * d6 - d5 * d4;
			if ((d4 - d3) >= 0 && (d5 - d6) >= 0 && va <= 0) {
				var v = (d4 - d3) / ((d4 - d3) + (d5 - d6));
				output = b + (c - b) * v;
				return false;
			} else {
				// P is inside the face region. Compute the point using its barycentric coordinates (u, v, w)
				var denom = 1f / (va + vb + vc);
				var v = vb * denom;
				var w = vc * denom;

				// This is equal to: u*a + v*b + w*c, u = va*denom = 1 - v - w;
				output = a + ab * v + ac * w;
				return true;
			}
		}

		/// <summary>
		/// Closest point on the triangle abc to the point p as barycentric coordinates.
		///
		/// See: 'Real Time Collision Detection' by Christer Ericson, chapter 5.1, page 141
		/// </summary>
		public static float3 ClosestPointOnTriangleBarycentric (float2 a, float2 b, float2 c, float2 p) {
			// Check if p is in vertex region outside A
			var ab = b - a;
			var ac = c - a;
			var ap = p - a;

			var d1 = math.dot(ab, ap);
			var d2 = math.dot(ac, ap);

			// Barycentric coordinates (1,0,0)
			if (d1 <= 0 && d2 <= 0) {
				return new float3(1, 0, 0);
			}

			// Check if p is in vertex region outside B
			var bp = p - b;
			var d3 = math.dot(ab, bp);
			var d4 = math.dot(ac, bp);

			// Barycentric coordinates (0,1,0)
			if (d3 >= 0 && d4 <= d3) {
				return new float3(0, 1, 0);
			}

			// Check if p is in edge region outside AB, if so return a projection of p onto AB
			var vc = d1 * d4 - d3 * d2;
			if (d1 >= 0 && d3 <= 0 && vc <= 0) {
				// Barycentric coordinates (1-v, v, 0)
				var v = d1 / (d1 - d3);
				return new float3(1-v, v, 0);
			}

			// Check if p is in vertex region outside C
			var cp = p - c;
			var d5 = math.dot(ab, cp);
			var d6 = math.dot(ac, cp);

			// Barycentric coordinates (0,0,1)
			if (d6 >= 0 && d5 <= d6) {
				return new float3(0, 0, 1);
			}

			// Check if p is in edge region of AC, if so return a projection of p onto AC
			var vb = d5 * d2 - d1 * d6;
			if (d2 >= 0 && d6 <= 0 && vb <= 0) {
				// Barycentric coordinates (1-v, 0, v)
				var v = d2 / (d2 - d6);
				return new float3(1 - v, 0, v);
			}

			// Check if p is in edge region of BC, if so return projection of p onto BC
			var va = d3 * d6 - d5 * d4;
			if ((d4 - d3) >= 0 && (d5 - d6) >= 0 && va <= 0) {
				var v = (d4 - d3) / ((d4 - d3) + (d5 - d6));
				return new float3(0, 1 - v, v);
			} else {
				// P is inside the face region. Compute the point using its barycentric coordinates (u, v, w)
				var denom = 1f / (va + vb + vc);
				var v = vb * denom;
				var w = vc * denom;
				return new float3(1 - v - w, v, w);

				// This is equal to: u*a + v*b + w*c, u = va*denom = 1 - v - w;
				// return a + ab * v + ac * w;
			}
		}

		/// <summary>
		/// Closest point on the triangle abc to the point p as barycentric coordinates.
		///
		/// See: 'Real Time Collision Detection' by Christer Ericson, chapter 5.1, page 141
		/// </summary>
		public static float3 ClosestPointOnTriangleBarycentric (float3 a, float3 b, float3 c, float3 p) {
			// Check if p is in vertex region outside A
			var ab = b - a;
			var ac = c - a;
			var ap = p - a;

			var d1 = math.dot(ab, ap);
			var d2 = math.dot(ac, ap);

			// Barycentric coordinates (1,0,0)
			if (d1 <= 0 && d2 <= 0) {
				return new float3(1, 0, 0);
			}

			// Check if p is in vertex region outside B
			var bp = p - b;
			var d3 = math.dot(ab, bp);
			var d4 = math.dot(ac, bp);

			// Barycentric coordinates (0,1,0)
			if (d3 >= 0 && d4 <= d3) {
				return new float3(0, 1, 0);
			}

			// Check if p is in edge region outside AB, if so return a projection of p onto AB
			var vc = d1 * d4 - d3 * d2;
			if (d1 >= 0 && d3 <= 0 && vc <= 0) {
				// Barycentric coordinates (1-v, v, 0)
				var v = d1 / (d1 - d3);
				return new float3(1-v, v, 0);
			}

			// Check if p is in vertex region outside C
			var cp = p - c;
			var d5 = math.dot(ab, cp);
			var d6 = math.dot(ac, cp);

			// Barycentric coordinates (0,0,1)
			if (d6 >= 0 && d5 <= d6) {
				return new float3(0, 0, 1);
			}

			// Check if p is in edge region of AC, if so return a projection of p onto AC
			var vb = d5 * d2 - d1 * d6;
			if (d2 >= 0 && d6 <= 0 && vb <= 0) {
				// Barycentric coordinates (1-v, 0, v)
				var v = d2 / (d2 - d6);
				return new float3(1 - v, 0, v);
			}

			// Check if p is in edge region of BC, if so return projection of p onto BC
			var va = d3 * d6 - d5 * d4;
			if ((d4 - d3) >= 0 && (d5 - d6) >= 0 && va <= 0) {
				var v = (d4 - d3) / ((d4 - d3) + (d5 - d6));
				return new float3(0, 1 - v, v);
			} else {
				// P is inside the face region. Compute the point using its barycentric coordinates (u, v, w)
				var denom = 1f / (va + vb + vc);
				var v = vb * denom;
				var w = vc * denom;
				return new float3(1 - v - w, v, w);

				// This is equal to: u*a + v*b + w*c, u = va*denom = 1 - v - w;
				// return a + ab * v + ac * w;
			}
		}

		/// <summary>
		/// Closest point on a triangle when one axis is scaled.
		///
		/// Project the triangle onto the plane defined by the projection axis.
		/// Then find the closest point on the triangle in the plane.
		/// Calculate the distance to the closest point in the plane, call that D1.
		/// Convert the closest point into 3D space, and calculate the distance to the
		/// query point along the plane's normal, call that D2.
		/// The final cost for a given point is D1 + D2 * distanceScaleAlongProjectionDirection.
		///
		/// This will form a diamond shape of equivalent cost points around the query point (x).
		/// The ratio of the width of this diamond to the height is equal to distanceScaleAlongProjectionDirection.
		///
		///     ^
		///    / \
		///   /   \
		///  /  x  \
		///  \	  /
		///   \   /
		///    \ /
		///     v
		///
		/// See: <see cref="DistanceMetric.ClosestAsSeenFromAboveSoft(Vector3)"/>
		/// </summary>
		/// <param name="vi1">First vertex of the triangle, in graph space.</param>
		/// <param name="vi2">Second vertex of the triangle, in graph space.</param>
		/// <param name="vi3">Third vertex of the triangle, in graph space.</param>
		/// <param name="projection">Projection parameters that are for example constructed from a movement plane.</param>
		/// <param name="point">Point to find the closest point to.</param>
		/// <param name="closest">Closest point on the triangle to the point.</param>
		/// <param name="sqrDist">Squared cost from the point to the closest point on the triangle.</param>
		/// <param name="distAlongProjection">Distance from the point to the closest point on the triangle along the projection axis.</param>
		[BurstCompile]
		public static void ClosestPointOnTriangleProjected (ref Int3 vi1, ref Int3 vi2, ref Int3 vi3, ref BBTree.ProjectionParams projection, ref float3 point, [NoAlias] out float3 closest, [NoAlias] out float sqrDist, [NoAlias] out float distAlongProjection) {
			var v1 = (float3)vi1;
			var v2 = (float3)vi2;
			var v3 = (float3)vi3;
			var v1proj = math.mul(projection.planeProjection, v1);
			var v2proj = math.mul(projection.planeProjection, v2);
			var v3proj = math.mul(projection.planeProjection, v3);
			// TODO: Can be cached
			var pointProj = math.mul(projection.planeProjection, point);
			var closestBarycentric = ClosestPointOnTriangleBarycentric(v1proj, v2proj, v3proj, pointProj);
			closest = v1*closestBarycentric.x + v2*closestBarycentric.y + v3*closestBarycentric.z;
			var closestProj = v1proj*closestBarycentric.x + v2proj*closestBarycentric.y + v3proj*closestBarycentric.z;
			distAlongProjection = math.abs(math.dot(closest - point, projection.projectionAxis));
			var distInPlane = math.length(closestProj - pointProj);
			if (distInPlane < 0.01f) {
				// If we are very close to being inside the triangle,
				// check if we are actually inside the triangle using a more numerically robust method.
				// If we are, set the in-plane-distance to 0.
				// This is particularly important if distanceScaleAlongProjectionAxis is zero,
				// as otherwise tie breaking may not work due to numerical issues.
				var ci1 = (int3)vi1;
				var ci2 = (int3)vi2;
				var ci3 = (int3)vi3;
				// wow, ugly
				var pi = (int3)(Int3)(Vector3)point;
				if (ContainsPoint(ref ci1, ref ci2, ref ci3, ref pi, in projection.planeProjection)) {
					distInPlane = 0;
				}
			}
			var dist = distInPlane + distAlongProjection*projection.distanceScaleAlongProjectionAxis;
			sqrDist = dist*dist;
		}

		/// <summary>Cached dictionary to avoid excessive allocations</summary>
		static readonly Dictionary<Int3, int> cached_Int3_int_dict = new Dictionary<Int3, int>();

		/// <summary>
		/// Compress the mesh by removing duplicate vertices.
		///
		/// Vertices that differ by only 1 along the y coordinate will also be merged together.
		/// Warning: This function is not threadsafe. It uses some cached structures to reduce allocations.
		/// </summary>
		/// <param name="vertices">Vertices of the input mesh</param>
		/// <param name="triangles">Triangles of the input mesh</param>
		/// <param name="tags">Tags of the input mesh. One for each triangle.</param>
		/// <param name="outVertices">Vertices of the output mesh.</param>
		/// <param name="outTriangles">Triangles of the output mesh.</param>
		/// <param name="outTags">Tags of the output mesh. One for each triangle.</param>
		public static void CompressMesh (List<Int3> vertices, List<int> triangles, List<uint> tags, out Int3[] outVertices, out int[] outTriangles, out uint[] outTags) {
			Dictionary<Int3, int> firstVerts = cached_Int3_int_dict;

			firstVerts.Clear();

			// Use cached array to reduce memory allocations
			int[] compressedPointers = ArrayPool<int>.Claim(vertices.Count);

			// Map positions to the first index they were encountered at
			int count = 0;
			for (int i = 0; i < vertices.Count; i++) {
				// Check if the vertex position has already been added
				// Also check one position up and one down because rounding errors can cause vertices
				// that should end up in the same position to be offset 1 unit from each other
				// TODO: Check along X and Z axes as well?
				int ind;
				if (!firstVerts.TryGetValue(vertices[i], out ind) && !firstVerts.TryGetValue(vertices[i] + new Int3(0, 1, 0), out ind) && !firstVerts.TryGetValue(vertices[i] + new Int3(0, -1, 0), out ind)) {
					firstVerts.Add(vertices[i], count);
					compressedPointers[i] = count;
					vertices[count] = vertices[i];
					count++;
				} else {
					compressedPointers[i] = ind;
				}
			}

			// Create the triangle array or reuse the existing buffer
			outTriangles = new int[triangles.Count];

			// Remap the triangles to the new compressed indices
			for (int i = 0; i < outTriangles.Length; i++) {
				outTriangles[i] = compressedPointers[triangles[i]];
			}

			// Create the vertex array or reuse the existing buffer
			outVertices = new Int3[count];

			for (int i = 0; i < count; i++)
				outVertices[i] = vertices[i];

			ArrayPool<int>.Release(ref compressedPointers);

			outTags = tags.ToArray();
		}

		/// <summary>
		/// Given a set of edges between vertices, follows those edges and returns them as chains and cycles.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="outline">outline[a] = b if there is an edge from a to b.</param>
		/// <param name="hasInEdge">hasInEdge should contain b if outline[a] = b for any key a.</param>
		/// <param name="results">Will be called once for each contour with the contour as a parameter as well as a boolean indicating if the contour is a cycle or a chain (see image).</param>
		public static void TraceContours (Dictionary<int, int> outline, HashSet<int> hasInEdge, System.Action<List<int>, bool> results) {
			// Iterate through chains of the navmesh outline.
			// I.e segments of the outline that are not loops
			// we need to start these at the beginning of the chain.
			// Then iterate over all the loops of the outline.
			// Since they are loops, we can start at any point.
			var obstacleVertices = ListPool<int>.Claim();
			var outlineKeys = ListPool<int>.Claim();

			outlineKeys.AddRange(outline.Keys);
			for (int k = 0; k <= 1; k++) {
				bool cycles = k == 1;
				for (int i = 0; i < outlineKeys.Count; i++) {
					var startIndex = outlineKeys[i];

					// Chains (not cycles) need to start at the start of the chain
					// Cycles can start at any point
					if (!cycles && hasInEdge.Contains(startIndex)) {
						continue;
					}

					var index = startIndex;
					obstacleVertices.Clear();
					obstacleVertices.Add(index);

					while (outline.ContainsKey(index)) {
						var next = outline[index];
						outline.Remove(index);

						obstacleVertices.Add(next);

						// We traversed a full cycle
						if (next == startIndex) break;

						index = next;
					}

					if (obstacleVertices.Count > 1) {
						results(obstacleVertices, cycles);
					}
				}
			}

			ListPool<int>.Release(ref outlineKeys);
			ListPool<int>.Release(ref obstacleVertices);
		}

		/// <summary>Divides each segment in the list into subSegments segments and fills the result list with the new points</summary>
		public static void Subdivide (List<Vector3> points, List<Vector3> result, int subSegments) {
			for (int i = 0; i < points.Count-1; i++)
				for (int j = 0; j < subSegments; j++)
					result.Add(Vector3.Lerp(points[i], points[i+1], j / (float)subSegments));

			result.Add(points[points.Count-1]);
		}
	}
}
