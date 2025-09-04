using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Pathfinding {
	[CustomEditor(typeof(AnimationLink))]
	public class AnimationLinkEditor : EditorBase {
		protected override void Inspector () {
			var script = target as AnimationLink;

			EditorGUI.BeginDisabledGroup(script.EndTransform == null);
			if (GUILayout.Button("Autoposition Endpoint")) {
				List<Vector3> buffer = Pathfinding.Pooling.ListPool<Vector3>.Claim();
				Vector3 endpos;
				script.CalculateOffsets(buffer, out endpos);
				script.EndTransform.position = endpos;
				Pathfinding.Pooling.ListPool<Vector3>.Release(buffer);
			}
			EditorGUI.EndDisabledGroup();
			base.Inspector();
		}
	}
}
