using Pathfinding.Graphs.Grid.Rules;
using UnityEditor;
using UnityEngine;

namespace Pathfinding {
	/// <summary>Editor for the <see cref="RuleTexture"/> rule</summary>
	[CustomGridGraphRuleEditor(typeof(RuleTexture), "Texture")]
	public class RuleTextureEditor : IGridGraphRuleEditor {
		protected static readonly string[] ChannelUseNames = { "None", "Penalty", "Height", "Walkability and Penalty", "Walkability" };

		public void OnInspectorGUI (GridGraph graph, GridGraphRule rule) {
			var target = rule as RuleTexture;

			target.texture = GraphEditor.ObjectField(new GUIContent("Texture"), target.texture, typeof(Texture2D), false, true) as Texture2D;

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Generate Reference")) {
				SaveReferenceTexture(graph);
				EditorUtility.DisplayDialog("Reference texture saved", "A texture has been saved in which every pixel corresponds to one node. The red channel represents if a node is walkable or not. The green channel represents the (normalized) Y coordinate of the nodes.", "Ok");
			}
			GUILayout.EndHorizontal();

			if (target.texture != null) {
				string path = AssetDatabase.GetAssetPath(target.texture);

				if (path != "") {
					var importer = AssetImporter.GetAtPath(path) as TextureImporter;
					if (importer != null && !importer.isReadable) {
						if (GraphEditor.FixLabel("Texture is not readable")) {
							importer.isReadable = true;
							EditorUtility.SetDirty(importer);
							AssetDatabase.ImportAsset(path);
						}
					}
				}
			}

			target.scalingMode = (RuleTexture.ScalingMode)EditorGUILayout.EnumPopup("Scaling Mode", target.scalingMode);
			if (target.scalingMode == RuleTexture.ScalingMode.FixedScale) {
				EditorGUI.indentLevel++;
				target.nodesPerPixel = EditorGUILayout.FloatField("Nodes Per Pixel", target.nodesPerPixel);
				EditorGUI.indentLevel--;
			}

			for (int i = 0; i < 4; i++) {
				char channelName = "RGBA"[i];
				target.channels[i] = (RuleTexture.ChannelUse)EditorGUILayout.Popup("" + channelName, (int)target.channels[i], ChannelUseNames);

				if (target.channels[i] != RuleTexture.ChannelUse.None) {
					EditorGUI.indentLevel++;
					if (target.channels[i] != RuleTexture.ChannelUse.Walkable) {
						target.channelScales[i] = EditorGUILayout.FloatField("Scale", target.channelScales[i]);
					}

					string help = "";
					switch (target.channels[i]) {
					case RuleTexture.ChannelUse.Penalty:
						help = "Penalty goes from 0 to " + target.channelScales[i].ToString("0") + " depending on the " + channelName + " channel value";
						break;
					case RuleTexture.ChannelUse.Position:
						help = "Nodes will have their Y coordinate set to a value between 0 and " + target.channelScales[i].ToString("0") + " depending on the "+channelName+" channel";

						if (graph.collision.heightCheck) {
							EditorGUILayout.HelpBox("Height testing is enabled but the node positions will be overwritten by the texture data. You should disable either height testing or this feature.", MessageType.Error);
						}
						break;
					case RuleTexture.ChannelUse.WalkablePenalty:
						help = "If the "+channelName+" channel is 0, the node is made unwalkable. Otherwise the penalty goes from 0 to " + target.channelScales[i].ToString("0") + " depending on the " + channelName + " channel value";
						break;
					case RuleTexture.ChannelUse.Walkable:
						help = "If the "+channelName+" channel is 0, the node is made unwalkable.";
						break;
					}

					EditorGUILayout.HelpBox(help, MessageType.None);

					if ((target.channels[i] == RuleTexture.ChannelUse.Penalty || target.channels[i] == RuleTexture.ChannelUse.WalkablePenalty) && target.channelScales[i] < 0) {
						EditorGUILayout.HelpBox("Negative penalties are not supported. You can instead raise the penalty of other nodes.", MessageType.Error);
					}

					EditorGUI.indentLevel--;
				}
			}
		}

		static void SaveReferenceTexture (GridGraph graph) {
			if (graph.nodes == null || graph.nodes.Length != graph.width * graph.depth * graph.LayerCount) {
				AstarPath.active.Scan();
			}

			if (graph.nodes.Length < graph.width * graph.depth) {
				Debug.LogError("Couldn't create reference image since nodes.Length < width*depth");
				return;
			}

			if (graph.nodes.Length == 0) {
				Debug.LogError("Couldn't create reference image since the graph is too small (0*0)");
				return;
			}

			if (graph.LayerCount > 1) {
				Debug.LogWarning("Creating reference image for a layered grid graph. Only the first layer will be included in the image.");
			}

			var tex = new Texture2D(graph.width, graph.depth);

			float maxY = float.NegativeInfinity;
			for (int i = 0; i < graph.nodes.Length; i++) {
				Vector3 p = graph.nodes[i] != null? graph.transform.InverseTransform((Vector3)graph.nodes[i].position) : Vector3.zero;
				maxY = p.y > maxY ? p.y : maxY;
			}

			var cols = new Color[graph.width*graph.depth];

			for (int z = 0; z < graph.depth; z++) {
				for (int x = 0; x < graph.width; x++) {
					GraphNode node = graph.nodes[z*graph.width+x];
					if (node != null) {
						float v = node.Walkable ? 1F : 0.0F;
						Vector3 p = graph.transform.InverseTransform((Vector3)node.position);
						float q = p.y / maxY;
						cols[z*graph.width+x] = new Color(v, q, 0);
					} else {
						cols[z*graph.width+x] = new Color(0, 0, 0);
					}
				}
			}
			tex.SetPixels(cols);
			tex.Apply();

			string path = AssetDatabase.GenerateUniqueAssetPath("Assets/gridReference.png");
			System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());

			AssetDatabase.Refresh();
			Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Texture));

			EditorGUIUtility.PingObject(obj);
		}

		public void OnSceneGUI (GridGraph graph, GridGraphRule rule) { }
	}
}
