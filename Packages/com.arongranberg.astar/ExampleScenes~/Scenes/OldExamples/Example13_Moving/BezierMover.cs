using UnityEngine;

namespace Pathfinding.Examples {
	using Pathfinding.Drawing;

	/// <summary>
	/// Moves an object along a spline.
	/// Helper script in the example scene called 'Moving'.
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/beziermover.html")]
	public class BezierMover : VersionedMonoBehaviour {
		public Transform[] points;

		public float speed = 1;
		public float tiltAmount = 1f;
		public float tiltSmoothing = 1.0f;

		float time = 0;
		Vector3 averageCurvature;

		Vector3 Evaluate (float t, out Vector3 derivative, out Vector3 secondDerivative, out Vector3 curvature) {
			int c = points.Length;
			int pt = (Mathf.FloorToInt(t) + c) % c;
			var p0 = points[(pt-1+c)%c].position;
			var p1 = points[pt].position;
			var p2 = points[(pt+1)%c].position;
			var p3 = points[(pt+2)%c].position;
			var tprime = t - Mathf.FloorToInt(t);

			CatmullRomToBezier(p0, p1, p2, p3, out var c0, out var c1, out var c2, out var c3);
			derivative = AstarSplines.CubicBezierDerivative(c0, c1, c2, c3, tprime);
			secondDerivative = AstarSplines.CubicBezierSecondDerivative(c0, c1, c2, c3, tprime);
			curvature = Curvature(derivative, secondDerivative);
			return AstarSplines.CubicBezier(c0, c1, c2, c3, tprime);
		}

		/// <summary>Converts a catmull-rom spline to bezier control points</summary>
		static void CatmullRomToBezier (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, out Vector3 c0, out Vector3 c1, out Vector3 c2, out Vector3 c3) {
			c0 = p1;
			c1 = (-p0 + 6*p1 + 1*p2)*(1/6.0f);
			c2 = (p1 + 6*p2 - p3)*(1/6.0f);
			c3 = p2;
		}

		static Vector3 Curvature (Vector3 derivate, Vector3 secondDerivative) {
			var dx = derivate.magnitude;

			if (dx < 0.000001f) return Vector3.zero;
			return Vector3.Cross(derivate, secondDerivative) / (dx*dx*dx);
		}

		/// <summary>Update is called once per frame</summary>
		void Update () {
			// Move the agent a small distance along the path, according to its speed
			float mn = time;
			float mx = time+1;

			while (mx - mn > 0.0001f) {
				float mid = (mn+mx)/2;

				Vector3 p = Evaluate(mid, out var dummy1, out var dummy2, out var dummy3);
				if ((p-transform.position).sqrMagnitude > (speed*Time.deltaTime)*(speed*Time.deltaTime)) {
					mx = mid;
				} else {
					mn = mid;
				}
			}

			time = (mn+mx)/2;

			transform.position = Evaluate(time, out var derivative, out var dummy, out var curvature);

			averageCurvature = Vector3.Lerp(averageCurvature, curvature, Time.deltaTime);

			// Estimate the acceleration at the current point and use it to tilt the object inwards on the curve
			var centripetalAcceleration = -Vector3.Cross(derivative.normalized, averageCurvature);
			var up = new Vector3(0, 1/(tiltAmount + 0.00001f), 0) + centripetalAcceleration;
			transform.rotation = Quaternion.LookRotation(derivative, up);
		}

		public override void DrawGizmos () {
			if (points != null && points.Length >= 3) {
				for (int i = 0; i < points.Length; i++) if (points[i] == null) return;

				Vector3 pp = Evaluate(0, out var derivative, out var secondDerivative, out var curvature);
				for (int pt = 0; pt < points.Length; pt++) {
					for (int i = 1; i <= 100; i++) {
						var p = Evaluate(pt + (i / 100f), out derivative, out secondDerivative, out curvature);
						Draw.Line(pp, p, Color.white);
						pp = p;
					}
				}
			}
		}
	}
}
