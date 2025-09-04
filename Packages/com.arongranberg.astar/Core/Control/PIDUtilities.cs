using Unity.Mathematics;

namespace Pathfinding.PID {
	public struct AnglePIDControlOutput2D {
		/// <summary>How much to rotate in a single time-step. In radians.</summary>
		public float rotationDelta;
		public float targetRotation;
		/// <summary>How much to move in a single time-step. In world units.</summary>
		public float2 positionDelta;

		public AnglePIDControlOutput2D(float currentRotation, float targetRotation, float rotationDelta, float moveDistance) {
			var midpointRotation = currentRotation + rotationDelta * 0.5f;
			math.sincos(midpointRotation, out float s, out float c);
			this.rotationDelta = rotationDelta;
			this.positionDelta = new float2(c, s) * moveDistance;
			this.targetRotation = targetRotation;
		}

		public static AnglePIDControlOutput2D WithMovementAtEnd (float currentRotation, float targetRotation, float rotationDelta, float moveDistance) {
			var finalRotation = currentRotation + rotationDelta;
			math.sincos(finalRotation, out float s, out float c);
			return new AnglePIDControlOutput2D {
					   rotationDelta = rotationDelta,
					   targetRotation = targetRotation,
					   positionDelta = new float2(c, s) * moveDistance,
			};
		}
	}
}
