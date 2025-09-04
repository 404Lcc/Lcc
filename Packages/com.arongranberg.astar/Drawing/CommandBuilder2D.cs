using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Pathfinding.Drawing.CommandBuilder;

namespace Pathfinding.Drawing {
	/// <summary>
	/// 2D wrapper for a <see cref="CommandBuilder"/>.
	///
	/// <code>
	/// var p1 = new Vector2(0, 1);
	/// var p2 = new Vector2(5, 7);
	///
	/// // Draw it in the XY plane
	/// Draw.xy.Line(p1, p2);
	///
	/// // Draw it in the XZ plane
	/// Draw.xz.Line(p1, p2);
	/// </code>
	///
	/// See: 2d-drawing (view in online documentation for working links)
	/// See: <see cref="Draw.xy"/>
	/// See: <see cref="Draw.xz"/>
	/// </summary>
	public partial struct CommandBuilder2D {
		/// <summary>The wrapped command builder</summary>
		private CommandBuilder draw;
		/// <summary>True if drawing in the XY plane, false if drawing in the XZ plane</summary>
		bool xy;

		static readonly float3 XY_UP = new float3(0, 0, 1);
		static readonly float3 XZ_UP = new float3(0, 1, 0);
		static readonly quaternion XY_TO_XZ_ROTATION =  quaternion.RotateX(-math.PI*0.5f);
		static readonly quaternion XZ_TO_XZ_ROTATION =  quaternion.identity;
		static readonly float4x4 XZ_TO_XY_MATRIX = new float4x4(new float4(1, 0, 0, 0), new float4(0, 0, 1, 0), new float4(0, 1, 0, 0), new float4(0, 0, 0, 1));

		public CommandBuilder2D(CommandBuilder draw, bool xy) {
			this.draw = draw;
			this.xy = xy;
		}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (float2 a, float2 b) {
			draw.Reserve<LineData>();
			// Add(Command.Line);
			// Add(new LineData { a = a, b = b });

			// The code below is equivalent to the commented out code above.
			// But drawing lines is the most common operation so it needs to be really fast.
			// Having this hardcoded improves line rendering performance by about 8%.
			unsafe {
				var buffer = draw.buffer;
				var bufferSize = buffer->Length;
				var newLen = bufferSize + 4 + 24;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				UnityEngine.Assertions.Assert.IsTrue(newLen <= buffer->Capacity);
#endif
				var ptr = (byte*)buffer->Ptr + bufferSize;
				*(Command*)ptr = Command.Line;
				var lineData = (LineData*)(ptr + 4);
				if (xy) {
					lineData->a = new float3(a, 0);
					lineData->b = new float3(b, 0);
				} else {
					lineData->a = new float3(a.x, 0, a.y);
					lineData->b = new float3(b.x, 0, b.y);
				}
				buffer->Length = newLen;
			}
		}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (float2 a, float2 b, Color color) {
			draw.Reserve<Color32, LineData>();
			// Add(Command.Line);
			// Add(new LineData { a = a, b = b });

			// The code below is equivalent to the commented out code above.
			// But drawing lines is the most common operation so it needs to be really fast.
			// Having this hardcoded improves line rendering performance by about 8%.
			unsafe {
				var buffer = draw.buffer;
				var bufferSize = buffer->Length;
				var newLen = bufferSize + 4 + 24 + 4;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				UnityEngine.Assertions.Assert.IsTrue(newLen <= buffer->Capacity);
#endif
				var ptr = (byte*)buffer->Ptr + bufferSize;
				*(Command*)ptr = Command.Line | Command.PushColorInline;
				*(uint*)(ptr + 4) = CommandBuilder.ConvertColor(color);
				var lineData = (LineData*)(ptr + 8);
				if (xy) {
					lineData->a = new float3(a, 0);
					lineData->b = new float3(b, 0);
				} else {
					lineData->a = new float3(a.x, 0, a.y);
					lineData->b = new float3(b.x, 0, b.y);
				}
				buffer->Length = newLen;
			}
		}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (float3 a, float3 b) {
			draw.Line(a, b);
		}

		/// <summary>
		/// Draws a circle.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Circle(float3,float,float,float)"/>
		/// See: <see cref="Arc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the circle or arc.</param>
		/// <param name="radius">Radius of the circle or arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		public void Circle (float2 center, float radius, float startAngle = 0f, float endAngle = 2 * math.PI) {
			Circle(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), radius, startAngle, endAngle);
		}

		/// <summary>
		/// Draws a circle.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="CommandBuilder.Circle(float3,float3,float)"/>
		/// See: <see cref="Arc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the circle or arc.</param>
		/// <param name="radius">Radius of the circle or arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		public void Circle (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * math.PI) {
			if (xy) {
				draw.PushMatrix(XZ_TO_XY_MATRIX);
				draw.CircleXZInternal(new float3(center.x, center.z, center.y), radius, startAngle, endAngle);
				draw.PopMatrix();
			} else {
				draw.CircleXZInternal(center, radius, startAngle, endAngle);
			}
		}

		/// <summary>\copydocref{SolidCircle(float3,float,float,float)}</summary>
		public void SolidCircle (float2 center, float radius, float startAngle = 0f, float endAngle = 2 * math.PI) {
			SolidCircle(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), radius, startAngle, endAngle);
		}

		/// <summary>
		/// Draws a disc.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.SolidCircle(float3,float3,float)"/>
		/// See: <see cref="SolidArc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the disc or solid arc.</param>
		/// <param name="radius">Radius of the disc or solid arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		public void SolidCircle (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * math.PI) {
			if (xy) draw.PushMatrix(XZ_TO_XY_MATRIX);
			draw.SolidCircleXZInternal(xy ? new float3(center.x, center.z, center.y) : center, radius, startAngle, endAngle);
			if (xy) draw.PopMatrix();
		}

		/// <summary>
		/// Draws a wire pill in 2D.
		///
		/// <code>
		/// Draw.xy.WirePill(new float2(-0.5f, -0.5f), new float2(0.5f, 0.5f), 0.5f, color);
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WirePill(float2,float2,float,float)"/>
		/// </summary>
		/// <param name="a">Center of the first circle of the capsule.</param>
		/// <param name="b">Center of the second circle of the capsule.</param>
		/// <param name="radius">Radius of the capsule.</param>
		public void WirePill (float2 a, float2 b, float radius) {
			WirePill(a, b - a, math.length(b - a), radius);
		}

		/// <summary>
		/// Draws a wire pill in 2D.
		///
		/// <code>
		/// Draw.xy.WirePill(new float2(-0.5f, -0.5f), new float2(1, 1), 1, 0.5f, color);
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WirePill(float2,float2,float)"/>
		/// </summary>
		/// <param name="position">Center of the first circle of the capsule.</param>
		/// <param name="direction">The main axis of the capsule. Does not have to be normalized. If zero, a circle will be drawn.</param>
		/// <param name="length">Length of the main axis of the capsule, from circle center to circle center. If zero, a circle will be drawn.</param>
		/// <param name="radius">Radius of the capsule.</param>
		public void WirePill (float2 position, float2 direction, float length, float radius) {
			direction = math.normalizesafe(direction);

			if (radius <= 0) {
				Line(position, position + direction * length);
			} else if (length <= 0 || math.all(direction == 0)) {
				Circle(position, radius);
			} else {
				float4x4 m;
				if (xy) {
					m = new float4x4(
						new float4(direction, 0, 0),
						new float4(math.cross(new float3(direction, 0), XY_UP), 0),
						new float4(0, 0, 1, 0),
						new float4(position, 0, 1)
						);
				} else {
					m = new float4x4(
						new float4(direction.x, 0, direction.y, 0),
						new float4(0, 1, 0, 0),
						new float4(math.cross(new float3(direction.x, 0, direction.y), XZ_UP), 0),
						new float4(position.x, 0, position.y, 1)
						);
				}
				draw.PushMatrix(m);
				Circle(new float2(0, 0), radius, 0.5f * math.PI, 1.5f * math.PI);
				Line(new float2(0, -radius), new float2(length, -radius));
				Circle(new float2(length, 0), radius, -0.5f * math.PI, 0.5f * math.PI);
				Line(new float2(0, radius), new float2(length, radius));
				draw.PopMatrix();
			}
		}

		/// <summary>\copydocref{CommandBuilder.Polyline(List<Vector3>,bool)}</summary>
		[BurstDiscard]
		public void Polyline (List<Vector2> points, bool cycle = false) {
			for (int i = 0; i < points.Count - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Count > 1) Line(points[points.Count - 1], points[0]);
		}

		/// <summary>\copydocref{CommandBuilder.Polyline(Vector3[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (Vector2[] points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>\copydocref{CommandBuilder.Polyline(float3[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (float2[] points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>\copydocref{CommandBuilder.Polyline(NativeArray<float3>,bool)}</summary>
		public void Polyline (NativeArray<float2> points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>
		/// Draws a 2D cross.
		///
		/// <code>
		/// Draw.xz.Cross(float3.zero, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.Cross"/>
		/// </summary>
		public void Cross (float2 position, float size = 1) {
			size *= 0.5f;
			Line(position - new float2(size, 0), position + new float2(size, 0));
			Line(position - new float2(0, size), position + new float2(0, size));
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// The rectangle will be oriented along the rotation's X and Z axes.
		///
		/// <code>
		/// Draw.xz.WireRectangle(new Vector3(0f, 0, 0), new Vector2(1, 1), Color.black);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// This is identical to <see cref="Draw.WirePlane(float3,quaternion,float2)"/>, but this name is added for consistency.
		///
		/// See: <see cref="Draw.WirePolygon"/>
		/// </summary>
		public void WireRectangle (float3 center, float2 size) {
			draw.WirePlane(center, xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, size);
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// This is particularly useful when combined with <see cref="InScreenSpace"/>.
		///
		/// <code>
		/// using (Draw.InScreenSpace(Camera.main)) {
		///     Draw.xy.WireRectangle(new Rect(10, 10, 100, 100), Color.black);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.WireRectangle(float3,quaternion,float2)"/>
		/// See: <see cref="Draw.WirePolygon"/>
		/// </summary>
		public void WireRectangle (Rect rect) {
			float2 min = rect.min;
			float2 max = rect.max;

			Line(new float2(min.x, min.y), new float2(max.x, min.y));
			Line(new float2(max.x, min.y), new float2(max.x, max.y));
			Line(new float2(max.x, max.y), new float2(min.x, max.y));
			Line(new float2(min.x, max.y), new float2(min.x, min.y));
		}

		/// <summary>
		/// Draws a solid rectangle.
		/// This is particularly useful when combined with <see cref="InScreenSpace"/>.
		///
		/// Behind the scenes this is implemented using <see cref="Draw.SolidPlane"/>.
		///
		/// <code>
		/// using (Draw.InScreenSpace(Camera.main)) {
		///     Draw.xy.SolidRectangle(new Rect(10, 10, 100, 100), Color.black);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WireRectangle"/>
		/// See: <see cref="Draw.WireRectangle(float3,quaternion,float2)"/>
		/// See: <see cref="Draw.SolidBox"/>
		/// </summary>
		public void SolidRectangle (Rect rect) {
			draw.SolidPlane(xy ? new float3(rect.center.x, rect.center.y, 0.0f) : new float3(rect.center.x, 0, rect.center.y), xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, new float2(rect.width, rect.height));
		}

		/// <summary>
		/// Draws a grid of lines.
		///
		/// <code>
		/// Draw.xz.WireGrid(Vector3.zero, new int2(3, 3), new float2(1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.WireGrid"/>
		/// </summary>
		/// <param name="center">Center of the grid</param>
		/// <param name="cells">Number of cells of the grid. Should be greater than 0.</param>
		/// <param name="totalSize">Total size of the grid along the X and Z axes.</param>
		public void WireGrid (float2 center, int2 cells, float2 totalSize) {
			draw.WireGrid(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, cells, totalSize);
		}

		/// <summary>
		/// Draws a grid of lines.
		///
		/// <code>
		/// Draw.xz.WireGrid(Vector3.zero, new int2(3, 3), new float2(1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.WireGrid"/>
		/// </summary>
		/// <param name="center">Center of the grid</param>
		/// <param name="cells">Number of cells of the grid. Should be greater than 0.</param>
		/// <param name="totalSize">Total size of the grid along the X and Z axes.</param>
		public void WireGrid (float3 center, int2 cells, float2 totalSize) {
			draw.WireGrid(center, xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, cells, totalSize);
		}
	}
}
