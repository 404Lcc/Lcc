using UnityEngine;
using UnityEditor;

namespace Pathfinding {
	[CustomEditor(typeof(RecastNavmeshModifier))]
	[CanEditMultipleObjects]
	public class RecastNavmeshModifierEditor : EditorBase {
		protected override void Inspector () {
			var modeProp = FindProperty("mode");
			var areaProp = FindProperty("surfaceID");
			var geometrySource = FindProperty("geometrySource");
			var includeInScan = FindProperty("includeInScan");
			var script = target as RecastNavmeshModifier;

			if (areaProp.intValue < 0) {
				areaProp.intValue = 0;
			}

			PropertyField(includeInScan);
			if (!includeInScan.hasMultipleDifferentValues && script.includeInScan == RecastNavmeshModifier.ScanInclusion.AlwaysExclude) {
				EditorGUILayout.HelpBox("This object will be completely ignored by the graph. Even if it would otherwise be included due to its layer or tag.", MessageType.None);
				return;
			}

			PropertyField(geometrySource, "Geometry Source");
			if (!geometrySource.hasMultipleDifferentValues) {
				var geometrySourceValue = (RecastNavmeshModifier.GeometrySource)geometrySource.intValue;
				script.ResolveMeshSource(out var filter, out var coll, out var coll2D);
				switch (geometrySourceValue) {
				case RecastNavmeshModifier.GeometrySource.Auto:
					if (filter != null) {
						EditorGUILayout.HelpBox("Using the attached MeshFilter as a source", MessageType.None);
						if (script.GetComponent<MeshRenderer>() == null) {
							EditorGUILayout.HelpBox("When a MeshFilter is used as a geometry source, a MeshRenderer must also be attached", MessageType.Error);
						}
					} else if (coll != null) {
						EditorGUILayout.HelpBox("Using the attached collider as a source", MessageType.None);
					} else if (coll2D != null) {
						EditorGUILayout.HelpBox("Using the attached 2D collider as a source", MessageType.None);
					} else {
						EditorGUILayout.HelpBox("No MeshFilter or MeshCollider found on this GameObject", MessageType.Error);
					}
					break;
				case RecastNavmeshModifier.GeometrySource.MeshFilter:
					if (filter == null) {
						EditorGUILayout.HelpBox("No MeshFilter found on this GameObject", MessageType.Error);
					} else if (script.GetComponent<MeshRenderer>() == null) {
						EditorGUILayout.HelpBox("When a MeshFilter is used as a geometry source, a MeshRenderer must also be attached", MessageType.Error);
					}
					break;
				case RecastNavmeshModifier.GeometrySource.Collider:
					if (coll == null && coll2D == null) {
						EditorGUILayout.HelpBox("No collider found on this GameObject", MessageType.Error);
					}
					break;
				}
			}

			PropertyField(modeProp, "Surface Type");
			// Note: uses intValue instead of enumValueIndex because the enum does not start from 0.
			var mode = (RecastNavmeshModifier.Mode)modeProp.intValue;
			if (!modeProp.hasMultipleDifferentValues) {
				switch (mode) {
				case RecastNavmeshModifier.Mode.UnwalkableSurface:
					EditorGUILayout.HelpBox("All surfaces on this mesh will be made unwalkable", MessageType.None);
					break;
				case RecastNavmeshModifier.Mode.WalkableSurface:
					EditorGUILayout.HelpBox("All surfaces on this mesh will be walkable", MessageType.None);
					break;
				case RecastNavmeshModifier.Mode.WalkableSurfaceWithSeam:
					EditorGUILayout.HelpBox("All surfaces on this mesh will be walkable and a " +
						"seam will be created between the surfaces on this mesh and the surfaces on other meshes (with a different surface id)", MessageType.None);
					EditorGUI.indentLevel++;
					PropertyField(areaProp, "Surface ID");
					if (!areaProp.hasMultipleDifferentValues && areaProp.intValue < 0) {
						areaProp.intValue = 0;
					}
					EditorGUI.indentLevel--;
					break;
				case RecastNavmeshModifier.Mode.WalkableSurfaceWithTag:
					EditorGUILayout.HelpBox("All surfaces on this mesh will be walkable and the given tag will be applied to them", MessageType.None);
					EditorGUI.indentLevel++;

					EditorGUI.showMixedValue = areaProp.hasMultipleDifferentValues;
					EditorGUI.BeginChangeCheck();
					var newTag = Util.EditorGUILayoutHelper.TagField(new GUIContent("Tag Value"), areaProp.intValue, AstarPathEditor.EditTags);
					if (EditorGUI.EndChangeCheck()) {
						areaProp.intValue = newTag;
					}
					if (!areaProp.hasMultipleDifferentValues && (areaProp.intValue < 0 || areaProp.intValue > GraphNode.MaxTagIndex)) {
						areaProp.intValue = Mathf.Clamp(areaProp.intValue, 0, GraphNode.MaxTagIndex);
					}

					EditorGUI.indentLevel--;
					break;
				}
			}

			var dynamicProp = FindProperty("dynamic");
			PropertyField(dynamicProp, "Dynamic", "Setting this value to false will give better scanning performance, but you will not be able to move the object during runtime");
			if (!dynamicProp.hasMultipleDifferentValues && !dynamicProp.boolValue) {
				EditorGUILayout.HelpBox("This object must not be moved during runtime since 'dynamic' is set to false", MessageType.Info);
			}

			bool solidAlwaysEnabled = true;
			bool solidRelevant = false;
			for (int i = 0; i < targets.Length; i++) {
				script.ResolveMeshSource(out var meshSource, out var collider, out var collider2D);
				bool usesConvexCollider = collider != null && (collider is BoxCollider || collider is SphereCollider || collider is CapsuleCollider || (collider is MeshCollider mc && mc.convex));
				solidAlwaysEnabled &= usesConvexCollider;

				// If the object only has a 2D collider, the solid field doesn't affect anything
				solidRelevant |= meshSource != null || collider != null;
			}

			if (solidRelevant) {
				if (solidAlwaysEnabled) {
					// Forced solid
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.Toggle("Solid", true);
					EditorGUILayout.HelpBox("Convex colliders are always treated as solid", MessageType.Info);
					EditorGUI.EndDisabledGroup();
				} else {
					PropertyField("solid");
				}
			}
		}
	}
}
