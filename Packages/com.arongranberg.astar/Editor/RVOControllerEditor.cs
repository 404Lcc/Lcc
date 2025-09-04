using UnityEngine;
using UnityEditor;

namespace Pathfinding.RVO {
	[CustomEditor(typeof(RVOController))]
	[CanEditMultipleObjects]
	public class RVOControllerEditor : EditorBase {
		protected override void Inspector () {
			Section("Shape");
			var ai = (target as MonoBehaviour).GetComponent<IAstarAI>();
			if (ai != null) {
				var drivenStr = "Driven by " + ai.GetType().Name + " component";
				EditorGUILayout.LabelField("Radius", drivenStr);
				if ((target as RVOController).movementPlaneMode == MovementPlane.XZ) {
					EditorGUILayout.LabelField("Height", drivenStr);
					EditorGUILayout.LabelField("Center", drivenStr);
				}
			} else {
				FloatField("radiusBackingField", label: "Radius", min: 0.01f);

				if ((target as RVOController).movementPlaneMode == MovementPlane.XZ) {
					FloatField("heightBackingField", label: "Height", min: 0.01f);
					PropertyField("centerBackingField", label: "Center");
				}
			}

			Section("Avoidance");
			FloatField("agentTimeHorizon", min: 0f);
			FloatField("obstacleTimeHorizon", min: 0f);
			PropertyField("maxNeighbours");
			PropertyField("layer");
			PropertyField("collidesWith");
			PropertyField("priority");
			var rvoController = target as RVOController;
			if (!Mathf.Approximately(rvoController.priorityMultiplier, 1.0f)) {
				EditorGUILayout.HelpBox("Another script is applying an additional multiplier to the priority of " + rvoController.priorityMultiplier.ToString("0.00"), MessageType.None);
			}

			if (rvoController.flowFollowingStrength > 0.01f) {
				EditorGUILayout.HelpBox("Another script is adding flow following behavior. Strength: " + (rvoController.flowFollowingStrength*100).ToString("0") + "%", MessageType.None);
			}

			EditorGUILayout.Separator();
			EditorGUI.BeginDisabledGroup(PropertyField("lockWhenNotMoving"));
			PropertyField("locked");
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.Separator();
			PropertyField("debug");

			bool maxNeighboursLimit = false;

			for (int i = 0; i < targets.Length; i++) {
				var controller = targets[i] as RVOController;
				maxNeighboursLimit |= controller.rvoAgent != null && controller.rvoAgent.NeighbourCount >= controller.rvoAgent.MaxNeighbours;
			}

			if (maxNeighboursLimit) {
				EditorGUILayout.HelpBox("Limit of how many neighbours to consider (Max Neighbours) has been reached. Some nearby agents may have been ignored. " +
					"To ensure all agents are taken into account you can raise the 'Max Neighbours' value at a cost to performance.", MessageType.Warning);
			}

			if (RVOSimulator.active == null && !EditorUtility.IsPersistent(target)) {
				EditorGUILayout.HelpBox("There is no enabled RVOSimulator component in the scene. A single RVOSimulator component is required for local avoidance.", MessageType.Warning);
			}
		}
	}
}
