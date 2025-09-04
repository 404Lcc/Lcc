using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.BurstTriangulator {
	public struct Status {
		// Allow unused fields outside the editor
#pragma warning disable 0414
		int value1, value2, value3, value4;
#pragma warning restore 0414
		public TriangulatorErrorType type;

		/// <summary>
		/// If true, then something went wrong during triangulation.
		/// Check <see cref="type"/> or <see cref="ToString"/>  for more information.
		/// </summary>
		public bool IsError => type != TriangulatorErrorType.Ok;

		public static Status Ok => new Status { type = TriangulatorErrorType.Ok };
		public static Status PositionsLengthLessThan3 (int length) => new Status { value1 = length, type = TriangulatorErrorType.PositionsLengthLessThan3 };
		public static Status PositionsMustBeFinite (int index) => new Status { value1 = index, type = TriangulatorErrorType.PositionsMustBeFinite };
		public static Status ConstraintsLengthNotDivisibleBy2 (int length) => new Status { value1 = length, type = TriangulatorErrorType.ConstraintsLengthNotDivisibleBy2 };
		public static Status DuplicatePosition (int index) => new Status { value1 = index, type = TriangulatorErrorType.DuplicatePosition };
		public static Status DuplicateConstraint (int index1, int index2) => new Status { value1 = index1, value2 = index2, type = TriangulatorErrorType.DuplicateConstraint };
		public static Status ConstraintOutOfBounds (int index, int2 constraint, int positionLength) => new Status { value1 = index, value2 = constraint.x, value3 = constraint.y, value4 = positionLength, type = TriangulatorErrorType.ConstraintOutOfBounds };
		public static Status ConstraintSelfLoop (int index, int2 constraint) => new Status { value1 = index, value2 = constraint.x, value3 = constraint.y, type = TriangulatorErrorType.ConstraintSelfLoop };
		public static Status ConstraintIntersection (int index1, int index2) => new Status { value1 = index1, value2 = index2, type = TriangulatorErrorType.ConstraintIntersection };
		public static Status DegenerateInput => new Status { type = TriangulatorErrorType.DegenerateInput };
		public static Status SloanMaxItersExceeded => new Status { type = TriangulatorErrorType.SloanMaxItersExceeded };
		public static Status IntegersDoNotSupportMeshRefinement => new Status { type = TriangulatorErrorType.IntegersDoNotSupportMeshRefinement };
		public static Status ConstraintArrayLengthMismatch (int constraintLength, int constraintTypeLength) => new Status { value1 = constraintLength, value2 = constraintTypeLength, type = TriangulatorErrorType.ConstraintArrayLengthMismatch };
		public static Status HoleMustBeFinite (int index) => new Status { value1 = index, type = TriangulatorErrorType.HoleMustBeFinite };
		public static Status RedudantHolesArray => new Status { type = TriangulatorErrorType.RedudantHolesArray };
		public static Status ConstraintEdgesMissingForAutoHolesAndBoundary => new Status { type = TriangulatorErrorType.ConstraintEdgesMissingForAutoHolesAndBoundary };
		public static Status ConstraintEdgesMissingForRestoreBoundary => new Status { type = TriangulatorErrorType.ConstraintEdgesMissingForRestoreBoundary };
		public static Status RefinementNotSupportedForCoordinateType => new Status { type = TriangulatorErrorType.RefinementNotSupportedForCoordinateType };
		public static Status SloanMaxItersMustBePositive (int sloanMaxIters) => new Status { type = TriangulatorErrorType.SloanMaxItersMustBePositive, value1 = sloanMaxIters };
		public static Status RefinementThresholdAreaMustBePositive => new Status { type = TriangulatorErrorType.RefinementThresholdAreaMustBePositive };
		public static Status RefinementThresholdAngleOutOfRange => new Status { type = TriangulatorErrorType.RefinementThresholdAngleOutOfRange };

#if UNITY_EDITOR
		internal FixedString512Bytes ToFixedString () {
			switch (type) {
			case TriangulatorErrorType.Ok:
				return "Ok";
			case TriangulatorErrorType.PositionsLengthLessThan3:
				return $"Position array's length must be greater than 3, but was {value1}.";
			case TriangulatorErrorType.PositionsMustBeFinite:
				return $"Positions must be finite, but position at index {value1} is not finite.";
			case TriangulatorErrorType.HoleMustBeFinite:
				return $"Hole must be finite, but hole at index {value1} is not finite.";
			case TriangulatorErrorType.ConstraintsLengthNotDivisibleBy2:
				return $"Input constraint array's length must be divisible by 2, but was {value1}.";
			case TriangulatorErrorType.DuplicatePosition:
				return $"Duplicate position at index {value1}.";
			case TriangulatorErrorType.DuplicateConstraint:
				return $"Constraints at indices {value1} and {value2} are equivalent.";
			case TriangulatorErrorType.ConstraintOutOfBounds:
				return $"Constraint[{value1}] = ({value2}, {value3}) is out of bounds of the positions array (length={value4}).";
			case TriangulatorErrorType.ConstraintSelfLoop:
				return $"Constraint[{value1}] = ({value2}, {value3}) is a self-loop.";
			case TriangulatorErrorType.ConstraintIntersection:
				return $"Constraints at indices {value1} and {value2} intersect.";
			case TriangulatorErrorType.DegenerateInput:
				return "Input is degenerate. It seems to consist only of duplicate or collinear points.";
			case TriangulatorErrorType.SloanMaxItersExceeded:
				return $"Sloan max iterations exceeded! This usually happens when the scale of the input positions is not uniform. Try to pre-process the input data or increase {nameof(TriangulationSettings.SloanMaxIters)}.";
			case TriangulatorErrorType.IntegersDoNotSupportMeshRefinement:
				return "Integer coordinates do not support mesh refinement. Please use float or double coordinates.";
			case TriangulatorErrorType.ConstraintArrayLengthMismatch:
				return $"Constraint type array's length ({value2}) must be exactly half of the constraint array's length ({value1}).";
			case TriangulatorErrorType.RedudantHolesArray:
				return "HoleSeeds buffer is provided, but ConstraintEdges is missing. Using holes requires constrained edges.";
			case TriangulatorErrorType.ConstraintEdgesMissingForAutoHolesAndBoundary:
				return "ConstraintEdges buffer is missing. This is required when using the AutoHolesAndBoundary settings.";
			case TriangulatorErrorType.ConstraintEdgesMissingForRestoreBoundary:
				return "ConstraintEdges buffer is missing. This is required when using the RestoreBoundary settings.";
			case TriangulatorErrorType.RefinementNotSupportedForCoordinateType:
				return "Mesh refinement is not supported for the coordinate type T.";
			case TriangulatorErrorType.SloanMaxItersMustBePositive:
				return $"Sloan max iterations must be positive. But found {value1}.";
			case TriangulatorErrorType.RefinementThresholdAreaMustBePositive:
				return "Refinement threshold area must be positive.";
			case TriangulatorErrorType.RefinementThresholdAngleOutOfRange:
				return "RefinementThresholdAngle must be in the range [0, π / 4]. Note that in the literature, the upper boundary for convergence is approximately π / 6.";
			default:
				return "Unknown error.";
			}
		}
#else
		internal FixedString64Bytes ToFixedString() => "Triangulation error. Run in editor for more info.";
#endif

		public override string ToString () {
			return ToFixedString().ToString();
		}
	}

	public enum TriangulatorErrorType : byte {
		Ok,
		PositionsLengthLessThan3,
		PositionsMustBeFinite,
		ConstraintsLengthNotDivisibleBy2,
		DuplicatePosition,
		DuplicateConstraint,
		ConstraintOutOfBounds,
		ConstraintSelfLoop,
		ConstraintIntersection,
		DegenerateInput,
		SloanMaxItersExceeded,
		IntegersDoNotSupportMeshRefinement,
		ConstraintArrayLengthMismatch,
		HoleMustBeFinite,
		RedudantHolesArray,
		ConstraintEdgesMissingForAutoHolesAndBoundary,
		ConstraintEdgesMissingForRestoreBoundary,
		RefinementNotSupportedForCoordinateType,
		SloanMaxItersMustBePositive,
		RefinementThresholdAreaMustBePositive,
		RefinementThresholdAngleOutOfRange,
	}
}
