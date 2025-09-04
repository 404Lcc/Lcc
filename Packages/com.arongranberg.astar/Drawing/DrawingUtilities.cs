using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;

namespace Pathfinding.Drawing {
	/// <summary>Various high-level utilities that are useful when drawing things</summary>
	public static class DrawingUtilities {
		private static List<Component> componentBuffer = new List<Component>();

		/// <summary>
		/// Bounding box of a GameObject.
		/// Sometimes you want to quickly draw the bounding box of an object. This is not always trivial as the object may have any number of children with colliders and renderers.
		/// You can use this method to calculate the bounding box easily.
		///
		/// The bounding box is calculated based on the colliders and renderers on this object and all its children.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// Draw.WireBox(DrawingUtilities.BoundsFrom(transform), Color.black);
		/// </code>
		///
		/// See: <see cref="BoundsFrom(Transform)"/>
		/// </summary>
		public static Bounds BoundsFrom (GameObject gameObject) {
			return BoundsFrom(gameObject.transform);
		}

		/// <summary>
		/// Bounding box of a Transform.
		/// Sometimes you want to quickly draw the bounding box of an object. This is not always trivial as the object may have any number of children with colliders and renderers.
		/// You can use this method to calculate the bounding box easily.
		///
		/// The bounding box is calculated based on the colliders and renderers on this object and all its children.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// Draw.WireBox(DrawingUtilities.BoundsFrom(transform), Color.black);
		/// </code>
		///
		/// See: <see cref="BoundsFrom(GameObject)"/>
		/// </summary>
		public static Bounds BoundsFrom (Transform transform) {
			transform.gameObject.GetComponents(componentBuffer);
			Bounds bounds = new Bounds(transform.position, Vector3.zero);
			for (int i = 0; i < componentBuffer.Count; i++) {
				var component = componentBuffer[i];
				if (component is Collider coll) bounds.Encapsulate(coll.bounds);
				else if (component is Collider2D coll2D) bounds.Encapsulate(coll2D.bounds);
				else if (component is MeshRenderer rend) bounds.Encapsulate(rend.bounds);
				else if (component is SpriteRenderer rendSprite) bounds.Encapsulate(rendSprite.bounds);
			}
			componentBuffer.Clear();
			var children = transform.childCount;
			for (int i = 0; i < children; i++) bounds.Encapsulate(BoundsFrom(transform.GetChild(i)));
			return bounds;
		}

		/// <summary>
		/// Bounding box which contains all points in the list.
		/// <code>
		/// List<Vector3> points = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 1) };
		/// Draw.WireBox(DrawingUtilities.BoundsFrom(points), Color.black);
		/// </code>
		///
		/// See: <see cref="BoundsFrom(Vector3"/>[])
		/// See: <see cref="BoundsFrom(NativeArray<float3>)"/>
		/// </summary>
		public static Bounds BoundsFrom (List<Vector3> points) {
			if (points.Count == 0) throw new System.ArgumentException("At least 1 point is required");
			Vector3 mn = points[0];
			Vector3 mx = points[0];
			for (int i = 0; i < points.Count; i++) {
				mn = Vector3.Min(mn, points[i]);
				mx = Vector3.Max(mx, points[i]);
			}
			return new Bounds((mx + mn) * 0.5f, (mx - mn) * 0.5f);
		}

		/// <summary>
		/// Bounding box which contains all points in the array.
		/// <code>
		/// List<Vector3> points = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 1) };
		/// Draw.WireBox(DrawingUtilities.BoundsFrom(points), Color.black);
		/// </code>
		///
		/// See: <see cref="BoundsFrom(List<Vector3>)"/>
		/// See: <see cref="BoundsFrom(NativeArray<float3>)"/>
		/// </summary>
		public static Bounds BoundsFrom (Vector3[] points) {
			if (points.Length == 0) throw new System.ArgumentException("At least 1 point is required");
			Vector3 mn = points[0];
			Vector3 mx = points[0];
			for (int i = 0; i < points.Length; i++) {
				mn = Vector3.Min(mn, points[i]);
				mx = Vector3.Max(mx, points[i]);
			}
			return new Bounds((mx + mn) * 0.5f, (mx - mn) * 0.5f);
		}

		/// <summary>
		/// Bounding box which contains all points in the array.
		/// <code>
		/// List<Vector3> points = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 1) };
		/// Draw.WireBox(DrawingUtilities.BoundsFrom(points), Color.black);
		/// </code>
		///
		/// See: <see cref="BoundsFrom(List<Vector3>)"/>
		/// See: <see cref="BoundsFrom(Vector3"/>[])
		/// </summary>
		public static Bounds BoundsFrom (NativeArray<float3> points) {
			if (points.Length == 0) throw new System.ArgumentException("At least 1 point is required");
			float3 mn = points[0];
			float3 mx = points[0];
			for (int i = 0; i < points.Length; i++) {
				mn = math.min(mn, points[i]);
				mx = math.max(mx, points[i]);
			}
			return new Bounds((mx + mn) * 0.5f, (mx - mn) * 0.5f);
		}
	}
}
