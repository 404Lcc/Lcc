#if MODULE_ENTITIES
using System.Runtime.InteropServices;
using Pathfinding.Drawing;
using Pathfinding.PID;
using Pathfinding.Util;
using Pathfinding.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pathfinding.ECS {
	[BurstCompile]
	struct DrawGizmosJobUtils {
		[BurstCompile]
		internal static void DrawPath (ref CommandBuilder draw, ref UnsafeSpan<float3> vertices, ref AgentCylinderShape shape) {
			// Some people will set the agent's radius to zero. In that case we just draw the path as a polyline as we have no good reference for how to space the symbols.
			if (shape.radius > 0.01f) {
				var generator = new CommandBuilder.PolylineWithSymbol(CommandBuilder.SymbolDecoration.ArrowHead, shape.radius * 0.5f, shape.radius * 0.0f, shape.radius * 4f, true);
				for (int i = vertices.Length - 1; i >= 0; i--) generator.MoveTo(ref draw, vertices[i]);
			} else {
				for (int i = 0; i < vertices.Length - 1; i++) draw.Line(vertices[i], vertices[i+1]);
			}
		}
	}

	public partial struct JobDrawFollowerGizmos : IJobChunk {
		public CommandBuilder draw;
		public GCHandle entityManagerHandle;
		[ReadOnly]
		public ComponentTypeHandle<LocalTransform> LocalTransformTypeHandleRO;
		[ReadOnly]
		public ComponentTypeHandle<AgentCylinderShape> AgentCylinderShapeHandleRO;
		[ReadOnly]
		public ComponentTypeHandle<MovementSettings> MovementSettingsHandleRO;
		[ReadOnly]
		public ComponentTypeHandle<AgentMovementPlane> AgentMovementPlaneHandleRO;
		// This is actually not read only, because the GetNextCorners function can modify internal state
		// See JobRepairPath.Scheduler.ManagedStateTypeHandleRW for details about why NativeDisableContainerSafetyRestriction is required
		[NativeDisableContainerSafetyRestriction]
		public ComponentTypeHandle<ManagedState> ManagedStateHandleRW;
		[ReadOnly]
		public ComponentTypeHandle<MovementState> MovementStateHandleRO;
		[ReadOnly]
		public ComponentTypeHandle<ResolvedMovement> ResolvedMovementHandleRO;

		[NativeDisableContainerSafetyRestriction]
		public NativeList<float3> scratchBuffer1;
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> scratchBuffer2;

		public void Execute (in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in Unity.Burst.Intrinsics.v128 chunkEnabledMask) {
			if (!scratchBuffer1.IsCreated) scratchBuffer1 = new NativeList<float3>(32, Allocator.Temp);
			if (!scratchBuffer2.IsCreated) scratchBuffer2 = new NativeArray<int>(32, Allocator.Temp);

			unsafe {
				var localTransforms = (LocalTransform*)chunk.GetNativeArray(ref LocalTransformTypeHandleRO).GetUnsafeReadOnlyPtr();
				var agentCylinderShapes = (AgentCylinderShape*)chunk.GetNativeArray(ref AgentCylinderShapeHandleRO).GetUnsafeReadOnlyPtr();
				var movementSettings = (MovementSettings*)chunk.GetNativeArray(ref MovementSettingsHandleRO).GetUnsafeReadOnlyPtr();
				var movementPlanes = (AgentMovementPlane*)chunk.GetNativeArray(ref AgentMovementPlaneHandleRO).GetUnsafeReadOnlyPtr();
				var managedStates = chunk.GetManagedComponentAccessor(ref ManagedStateHandleRW, (EntityManager)entityManagerHandle.Target);
				var movementStates = (MovementState*)chunk.GetNativeArray(ref MovementStateHandleRO).GetUnsafeReadOnlyPtr();
				var resolvedMovement = (ResolvedMovement*)chunk.GetNativeArray(ref ResolvedMovementHandleRO).GetUnsafeReadOnlyPtr();

				for (int i = 0; i < chunk.Count; i++) {
					Execute(ref localTransforms[i], ref movementPlanes[i], ref agentCylinderShapes[i], managedStates[i], ref movementSettings[i], ref movementStates[i], ref resolvedMovement[i]);
				}
			}
		}

		public static readonly UnityEngine.Color VisualRotationColor = Palette.Colorbrewer.Set1.Blue;
		public static readonly UnityEngine.Color UnsmoothedRotation = Palette.Colorbrewer.Set1.Purple;
		public static readonly UnityEngine.Color InternalRotation = Palette.Colorbrewer.Set1.Orange;
		public static readonly UnityEngine.Color TargetInternalRotation = Palette.Colorbrewer.Set1.Yellow;
		public static readonly UnityEngine.Color TargetInternalRotationHint = Palette.Colorbrewer.Set1.Pink;
		public static readonly UnityEngine.Color Path = Palette.Colorbrewer.Set1.Orange;

		public void Execute (ref LocalTransform transform, ref AgentMovementPlane movementPlane, ref AgentCylinderShape shape, ManagedState managedState, ref MovementSettings settings, ref MovementState movementState, ref ResolvedMovement resolvedMovement) {
			if ((settings.debugFlags & PIDMovement.DebugFlags.Funnel) != 0) {
				managedState.pathTracer.DrawFunnel(draw, movementPlane.value);
			}
			if ((settings.debugFlags & PIDMovement.DebugFlags.Rotation) != 0) {
				var p2D = movementPlane.value.ToPlane(transform.Position, out float positionElevation);
				draw.PushMatrix(math.mul(new float4x4(movementPlane.value.rotation, float3.zero), float4x4.Translate(new float3(0, positionElevation, 0))));
				var visualRotation = movementPlane.value.ToPlane(transform.Rotation);
				var unsmoothedRotation = visualRotation - movementState.rotationOffset2;
				var internalRotation = unsmoothedRotation - movementState.rotationOffset;
				var targetInternalRotation = resolvedMovement.targetRotation;
				var targetInternalRotationHint = resolvedMovement.targetRotationHint;
				math.sincos(math.PI*0.5f + new float3(visualRotation, unsmoothedRotation, internalRotation), out var s, out var c);
				draw.xz.ArrowheadArc(p2D, new float2(c.x, s.x), shape.radius * 1.1f, VisualRotationColor);
				draw.xz.ArrowheadArc(p2D, new float2(c.y, s.y), shape.radius * 1.1f, UnsmoothedRotation);
				draw.xz.ArrowheadArc(p2D, new float2(c.z, s.z), shape.radius * 1.1f, InternalRotation);
				math.sincos(math.PI*0.5f + new float2(targetInternalRotation, targetInternalRotationHint), out var s2, out var c2);
				draw.xz.ArrowheadArc(p2D, new float2(c2.x, s2.x), shape.radius * 1.2f, TargetInternalRotation);
				draw.xz.ArrowheadArc(p2D, new float2(c2.y, s2.y), shape.radius * 1.2f, TargetInternalRotationHint);
				draw.PopMatrix();
			}
			if ((settings.debugFlags & PIDMovement.DebugFlags.Path) != 0 && managedState.pathTracer.hasPath) {
				scratchBuffer1.Clear();
				managedState.pathTracer.GetNextCorners(scratchBuffer1, int.MaxValue, ref scratchBuffer2, Allocator.Temp, managedState.pathfindingSettings.traversalProvider, managedState.activePath);
				var span = scratchBuffer1.AsUnsafeSpan();
				draw.PushColor(Path);
				DrawGizmosJobUtils.DrawPath(ref draw, ref span, ref shape);
				draw.PopColor();
			}
		}
	}
}
#endif
