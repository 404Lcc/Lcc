using UnityEngine;
using Pathfinding.Drawing;

namespace Pathfinding {
	/// <summary>
	/// Moves an agent in a circle around a point.
	///
	/// This script is intended as an example of how you can make an agent move in a circle.
	/// In a real game, you may want to replace this script with your own custom script that is tailored to your game.
	/// The code in this script is simple enough to copy and paste wherever you need it.
	///
	/// [Open online documentation to see videos]
	///
	/// See: move_in_circle (view in online documentation for working links)
	/// See: <see cref="AIDestinationSetter"/>
	/// See: <see cref="FollowerEntity"/>
	/// See: <see cref="AIPath"/>
	/// See: <see cref="RichAI"/>
	/// See: <see cref="AILerp"/>
	/// </summary>
	[UniqueComponent(tag = "ai.destination")]
	[AddComponentMenu("Pathfinding/AI/Behaviors/MoveInCircle")]
	/// <summary>[MoveInCircle]</summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/moveincircle.html")]
	public class MoveInCircle : VersionedMonoBehaviour {
		/// <summary>Target point to rotate around</summary>
		public Transform target;
		/// <summary>Radius of the circle</summary>
		public float radius = 5;
		/// <summary>Distance between the agent's current position, and the destination it will get. Use a negative value to make the agent move in the opposite direction around the circle.</summary>
		public float offset = 2;

		IAstarAI ai;

		void OnEnable () {
			ai = GetComponent<IAstarAI>();
		}

		void Update () {
			var normal = (ai.position - target.position).normalized;
			var tangent = Vector3.Cross(normal, target.up);

			ai.destination = target.position + normal * radius + tangent * offset;
		}

		public override void DrawGizmos () {
			if (target) Draw.Circle(target.position, target.up, radius, Color.white);
		}
	}
	/// <summary>[MoveInCircle]</summary>
}
