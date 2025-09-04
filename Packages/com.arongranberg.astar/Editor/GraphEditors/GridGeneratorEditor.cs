using UnityEngine;
using UnityEditor;
using Pathfinding.Serialization;
using System.Collections.Generic;

namespace Pathfinding {
	using Pathfinding.Graphs.Grid;
	using Pathfinding.Graphs.Grid.Rules;
	using Pathfinding.Util;

	[CustomGraphEditor(typeof(GridGraph), "Grid Graph")]
	public class GridGraphEditor : GraphEditor {
		[JsonMember]
		public bool locked = true;

		[JsonMember]
		public bool showExtra;

		GraphTransform savedTransform;
		Vector2 savedDimensions;
		float savedNodeSize;

		public bool isMouseDown;

		[JsonMember]
		public GridPivot pivot;

		/// <summary>
		/// Shows the preview for the collision testing options.
		///
		/// [Open online documentation to see images]
		///
		/// On the left you can see a top-down view of the graph with a grid of nodes.
		/// On the right you can see a side view of the graph. The white line at the bottom is the base of the graph, with node positions indicated using small dots.
		/// When using 2D physics, only the top-down view is visible.
		///
		/// The green shape indicates the shape that will be used for collision checking.
		/// </summary>
		[JsonMember]
		public bool collisionPreviewOpen;

		[JsonMember]
		public int selectedTilemap;

		/// <summary>Cached gui style</summary>
		static GUIStyle lockStyle;

		/// <summary>Cached gui style</summary>
		static GUIStyle gridPivotSelectBackground;

		/// <summary>Cached gui style</summary>
		static GUIStyle gridPivotSelectButton;

		public GridGraphEditor() {
			// Default value depends on if the game is running or not. Make it hidden in play mode by default.
			collisionPreviewOpen = !Application.isPlaying;
		}

		public override void OnInspectorGUI (NavGraph target) {
			var graph = target as GridGraph;

			DrawFirstSection(graph);
			Separator();
			DrawMiddleSection(graph);
			Separator();
			DrawCollisionEditor(graph, graph.collision);
			DrawRules(graph);

			Separator();
			DrawLastSection(graph);
		}

		bool IsHexagonal (GridGraph graph) {
			return graph.neighbours == NumNeighbours.Six && graph.uniformEdgeCosts;
		}

		bool IsIsometric (GridGraph graph) {
			if (IsHexagonal(graph)) return false;
			if (graph.aspectRatio != 1) return true;
			return graph.isometricAngle != 0;
		}

		bool IsAdvanced (GridGraph graph) {
			if (graph.inspectorGridMode == InspectorGridMode.Advanced) return true;
			// Weird configuration
			return (graph.neighbours == NumNeighbours.Six) != graph.uniformEdgeCosts;
		}

		InspectorGridMode DetermineGridType (GridGraph graph) {
			bool hex = IsHexagonal(graph);
			bool iso = IsIsometric(graph);
			bool adv = IsAdvanced(graph);

			if (adv || (hex && iso)) return InspectorGridMode.Advanced;
			if (hex) return InspectorGridMode.Hexagonal;
			if (iso) return InspectorGridMode.IsometricGrid;
			return graph.inspectorGridMode;
		}

		void DrawInspectorMode (GridGraph graph) {
			graph.inspectorGridMode = DetermineGridType(graph);
			var newMode = (InspectorGridMode)EditorGUILayout.EnumPopup("Shape", (System.Enum)graph.inspectorGridMode);
			if (newMode != graph.inspectorGridMode) graph.SetGridShape(newMode);
		}

		protected virtual void Draw2DMode (GridGraph graph) {
			graph.is2D = EditorGUILayout.Toggle(new GUIContent("2D"), graph.is2D);
		}

		GUIContent[] hexagonSizeContents = {
			new GUIContent("Hexagon Width", "Distance between two opposing sides on the hexagon"),
			new GUIContent("Hexagon Diameter", "Distance between two opposing vertices on the hexagon"),
			new GUIContent("Node Size", "Raw node size value, this doesn't correspond to anything particular on the hexagon."),
		};

		static List<GridLayout> cachedSceneGridLayouts;
		static float cachedSceneGridLayoutsTimestamp = -float.PositiveInfinity;

		static string GetPath (Transform current) {
			if (current.parent == null)
				return current.name;
			return GetPath(current.parent) + "/" + current.name;
		}

		void DrawTilemapAlignment (GridGraph graph) {
			if (cachedSceneGridLayouts == null || Time.realtimeSinceStartup - cachedSceneGridLayoutsTimestamp > 5f) {
				var tilemaps = UnityCompatibility.FindObjectsByTypeSorted<UnityEngine.GridLayout>();
				List<GridLayout> layouts = new List<GridLayout>(tilemaps);
				for (int i = 0; i < tilemaps.Length; i++) {
					if (tilemaps[i] is UnityEngine.Tilemaps.Tilemap tilemap) layouts.Remove(tilemap.layoutGrid);
				}
				cachedSceneGridLayouts = layouts;
				cachedSceneGridLayoutsTimestamp = Time.realtimeSinceStartup;
			}
			for (int i = cachedSceneGridLayouts.Count - 1; i >= 0; i--) {
				if (cachedSceneGridLayouts[i] == null) cachedSceneGridLayouts.RemoveAt(i);
			}

			if (cachedSceneGridLayouts.Count > 0) {
				GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Align to tilemap");

				var tilemapNames = new GUIContent[cachedSceneGridLayouts.Count+1];
				tilemapNames[0] = new GUIContent("Select...");
				for (int i = 0; i < cachedSceneGridLayouts.Count; i++) tilemapNames[i+1] = new GUIContent(GetPath(cachedSceneGridLayouts[i].transform).Replace("/", "\u2215"));

				var originalIndent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				selectedTilemap = EditorGUILayout.Popup(selectedTilemap, tilemapNames);
				selectedTilemap = Mathf.Clamp(selectedTilemap, 0, tilemapNames.Length - 1);

				EditorGUI.BeginDisabledGroup(selectedTilemap <= 0 || selectedTilemap - 1 >= cachedSceneGridLayouts.Count);
				if (GUILayout.Button("Align To")) {
					graph.AlignToTilemap(cachedSceneGridLayouts[selectedTilemap - 1]);
				}
				EditorGUI.EndDisabledGroup();
				EditorGUI.indentLevel = originalIndent;

				GUILayout.EndHorizontal();
			}
		}

		void DrawFirstSection (GridGraph graph) {
			float prevRatio = graph.aspectRatio;

			DrawInspectorMode(graph);

			Draw2DMode(graph);
			DrawTilemapAlignment(graph);

			var normalizedPivotPoint = NormalizedPivotPoint(graph, pivot);
			var worldPoint = graph.CalculateTransform().Transform(normalizedPivotPoint);
			int newWidth, newDepth;

			DrawWidthDepthFields(graph, out newWidth, out newDepth);

			EditorGUI.BeginChangeCheck();
			float newNodeSize;
			if (graph.inspectorGridMode == InspectorGridMode.Hexagonal) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical();
				graph.inspectorHexagonSizeMode = (InspectorGridHexagonNodeSize)EditorGUILayout.EnumPopup(new GUIContent("Hexagon Dimension"), graph.inspectorHexagonSizeMode);
				float hexagonSize = GridGraph.ConvertNodeSizeToHexagonSize(graph.inspectorHexagonSizeMode, graph.nodeSize);
				hexagonSize = (float)System.Math.Round(hexagonSize, 5);
				newNodeSize = GridGraph.ConvertHexagonSizeToNodeSize(graph.inspectorHexagonSizeMode, EditorGUILayout.FloatField(hexagonSizeContents[(int)graph.inspectorHexagonSizeMode], hexagonSize));
				EditorGUILayout.EndVertical();
				if (graph.inspectorHexagonSizeMode != InspectorGridHexagonNodeSize.NodeSize) GUILayout.Box("", AstarPathEditor.astarSkin.FindStyle(graph.inspectorHexagonSizeMode == InspectorGridHexagonNodeSize.Diameter ? "HexagonDiameter" : "HexagonWidth"));
				EditorGUILayout.EndHorizontal();
			} else {
				newNodeSize = EditorGUILayout.FloatField(new GUIContent("Node size", "The size of a single node. The size is the side of the node square in world units"), graph.nodeSize);
			}
			bool nodeSizeChanged = EditorGUI.EndChangeCheck();

			newNodeSize = newNodeSize <= 0.01F ? 0.01F : newNodeSize;

			if (graph.inspectorGridMode == InspectorGridMode.IsometricGrid || graph.inspectorGridMode == InspectorGridMode.Hexagonal || graph.inspectorGridMode == InspectorGridMode.Advanced) {
				graph.aspectRatio = EditorGUILayout.FloatField(new GUIContent("Aspect Ratio", "Ratio between a node's width and depth."), graph.aspectRatio);
			}

			if (graph.inspectorGridMode == InspectorGridMode.IsometricGrid || graph.inspectorGridMode == InspectorGridMode.Advanced) {
				DrawIsometricField(graph);
			}

			if ((nodeSizeChanged && locked) || (newWidth != graph.width || newDepth != graph.depth) || prevRatio != graph.aspectRatio) {
				graph.SetDimensions(newWidth, newDepth, newNodeSize);

				normalizedPivotPoint = NormalizedPivotPoint(graph, pivot);
				var newWorldPoint = graph.CalculateTransform().Transform(normalizedPivotPoint);
				// Move the center so that the pivot point stays at the same point in the world
				graph.center += worldPoint - newWorldPoint;
				graph.center = RoundVector3(graph.center);
				graph.UpdateTransform();
			}

			if ((nodeSizeChanged && !locked)) {
				graph.nodeSize = newNodeSize;
				graph.UpdateTransform();
			}

			DrawPositionField(graph);

			DrawRotationField(graph);
		}

		void DrawRotationField (GridGraph graph) {
			if (graph.is2D) {
				var right = Quaternion.Euler(graph.rotation) * Vector3.right;
				var angle = Mathf.Atan2(right.y, right.x) * Mathf.Rad2Deg;
				if (angle < 0) angle += 360;
				if (Mathf.Abs(angle - Mathf.Round(angle)) < 0.001f) angle = Mathf.Round(angle);
				EditorGUI.BeginChangeCheck();
				angle = EditorGUILayout.FloatField("Rotation", angle);
				if (EditorGUI.EndChangeCheck()) {
					graph.rotation = RoundVector3(new Vector3(-90 + angle, 270, 90));
				}
			} else {
				graph.rotation = RoundVector3(EditorGUILayout.Vector3Field("Rotation", graph.rotation));
			}
		}

		void DrawWidthDepthFields (GridGraph graph, out int newWidth, out int newDepth) {
			lockStyle = lockStyle ?? AstarPathEditor.astarSkin.FindStyle("GridSizeLock") ?? new GUIStyle();

			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			newWidth = EditorGUILayout.IntField(new GUIContent("Width (nodes)", "Width of the graph in nodes"), graph.width);
			newDepth = EditorGUILayout.IntField(new GUIContent("Depth (nodes)", "Depth (or height you might also call it) of the graph in nodes"), graph.depth);

			// Clamping will be done elsewhere as well
			// but this prevents negative widths from being converted to positive ones (since an absolute value will be taken)
			newWidth = Mathf.Max(newWidth, 1);
			newDepth = Mathf.Max(newDepth, 1);

			GUILayout.EndVertical();

			Rect lockRect = GUILayoutUtility.GetRect(lockStyle.fixedWidth, lockStyle.fixedHeight);

			GUILayout.EndHorizontal();

			// All the layouts mess up the margin to the next control, so add it manually
			GUILayout.Space(2);

			// Add a small offset to make it better centred around the controls
			lockRect.y += 3;
			lockRect.width = lockStyle.fixedWidth;
			lockRect.height = lockStyle.fixedHeight;
			lockRect.x += lockStyle.margin.left;
			lockRect.y += lockStyle.margin.top;

			locked = GUI.Toggle(lockRect, locked,
				new GUIContent("", "If the width and depth values are locked, " +
					"changing the node size will scale the grid while keeping the number of nodes consistent " +
					"instead of keeping the size the same and changing the number of nodes in the graph"), lockStyle);
		}

		void DrawIsometricField (GridGraph graph) {
			var isometricGUIContent = new GUIContent("Isometric Angle", "For an isometric 2D game, you can use this parameter to scale the graph correctly.\nIt can also be used to create a hexagonal grid.\nYou may want to rotate the graph 45 degrees around the Y axis to make it line up better.");
			var isometricOptions = new [] { new GUIContent("None (0°)"), new GUIContent("Isometric (≈54.74°)"), new GUIContent("Dimetric (60°)"), new GUIContent("Custom") };
			var isometricValues = new [] { 0f, GridGraph.StandardIsometricAngle, GridGraph.StandardDimetricAngle };
			var isometricOption = isometricValues.Length;

			for (int i = 0; i < isometricValues.Length; i++) {
				if (Mathf.Approximately(graph.isometricAngle, isometricValues[i])) {
					isometricOption = i;
				}
			}

			var prevIsometricOption = isometricOption;
			isometricOption = EditorGUILayout.IntPopup(isometricGUIContent, isometricOption, isometricOptions, new [] { 0, 1, 2, 3 });
			if (prevIsometricOption != isometricOption) {
				// Change to something that will not match the predefined values above
				graph.isometricAngle = 45;
			}

			if (isometricOption < isometricValues.Length) {
				graph.isometricAngle = isometricValues[isometricOption];
			} else {
				EditorGUI.indentLevel++;
				// Custom
				graph.isometricAngle = EditorGUILayout.FloatField(isometricGUIContent, graph.isometricAngle);
				EditorGUI.indentLevel--;
			}
		}

		static Vector3 NormalizedPivotPoint (GridGraph graph, GridPivot pivot) {
			switch (pivot) {
			case GridPivot.Center:
			default:
				return new Vector3(graph.width/2f, 0, graph.depth/2f);
			case GridPivot.TopLeft:
				return new Vector3(0, 0, graph.depth);
			case GridPivot.TopRight:
				return new Vector3(graph.width, 0, graph.depth);
			case GridPivot.BottomLeft:
				return new Vector3(0, 0, 0);
			case GridPivot.BottomRight:
				return new Vector3(graph.width, 0, 0);
			}
		}

		void DrawPositionField (GridGraph graph) {
			GUILayout.BeginHorizontal();
			var normalizedPivotPoint = NormalizedPivotPoint(graph, pivot);
			var worldPoint = RoundVector3(graph.CalculateTransform().Transform(normalizedPivotPoint));
			var newWorldPoint = EditorGUILayout.Vector3Field(ObjectNames.NicifyVariableName(pivot.ToString()), worldPoint);
			var delta = newWorldPoint - worldPoint;
			if (delta.magnitude > 0.001f) {
				graph.center += delta;
			}

			pivot = PivotPointSelector(pivot);
			GUILayout.EndHorizontal();
		}

		protected virtual void DrawMiddleSection (GridGraph graph) {
			DrawNeighbours(graph);
			DrawMaxClimb(graph);
			DrawMaxSlope(graph);
			DrawErosion(graph);
		}

		protected virtual void DrawCutCorners (GridGraph graph) {
			if (graph.inspectorGridMode == InspectorGridMode.Hexagonal) return;

			graph.cutCorners = EditorGUILayout.Toggle(new GUIContent("Cut Corners", "Enables or disables cutting corners. See docs for image example"), graph.cutCorners);
		}

		protected virtual void DrawNeighbours (GridGraph graph) {
			if (graph.inspectorGridMode == InspectorGridMode.Hexagonal) return;

			var neighboursGUIContent = new GUIContent("Connections", "Sets how many connections a node should have to it's neighbour nodes.");
			GUIContent[] neighbourOptions;
			if (graph.inspectorGridMode == InspectorGridMode.Advanced) {
				neighbourOptions = new [] { new GUIContent("Four"), new GUIContent("Eight"), new GUIContent("Six") };
			} else {
				neighbourOptions = new [] { new GUIContent("Four"), new GUIContent("Eight") };
			}
			graph.neighbours = (NumNeighbours)EditorGUILayout.Popup(neighboursGUIContent, (int)graph.neighbours, neighbourOptions);

			EditorGUI.indentLevel++;

			if (graph.neighbours == NumNeighbours.Eight) {
				DrawCutCorners(graph);
			}

			if (graph.neighbours == NumNeighbours.Six) {
				graph.uniformEdgeCosts = EditorGUILayout.Toggle(new GUIContent("Hexagon connection costs", "Tweak the edge costs in the graph to be more suitable for hexagon graphs"), graph.uniformEdgeCosts);
				EditorGUILayout.HelpBox("You can set all settings to make this a hexagonal graph by changing the 'Shape' field above", MessageType.None);
			} else {
				graph.uniformEdgeCosts = false;
			}

			EditorGUI.indentLevel--;
		}

		protected virtual void DrawMaxClimb (GridGraph graph) {
			if (!graph.collision.use2D) {
				graph.maxStepHeight = EditorGUILayout.FloatField(new GUIContent("Max Step Height", "How high a step can be while still allowing the AI to go up/down it. A zero (0) indicates infinity. This affects for example how the graph is generated around ledges and stairs."), graph.maxStepHeight);
				if (graph.maxStepHeight < 0) graph.maxStepHeight = 0;
				if (graph.maxStepHeight > 0) {
					EditorGUI.indentLevel++;
					graph.maxStepUsesSlope = EditorGUILayout.Toggle(new GUIContent("Account for slopes", "Account for slopes when calculating the step sizes. See documentation for more info."), graph.maxStepUsesSlope);
					EditorGUI.indentLevel--;
				}
			}
		}

		protected void DrawMaxSlope (GridGraph graph) {
			if (!graph.collision.use2D) {
				graph.maxSlope = EditorGUILayout.Slider(new GUIContent("Max Slope", "Sets the max slope in degrees for a point to be walkable. Only enabled if Height Testing is enabled."), graph.maxSlope, 0, 90F);
			}
		}

		protected void DrawErosion (GridGraph graph) {
			graph.erodeIterations = EditorGUILayout.IntField(new GUIContent("Erosion iterations", "Sets how many times the graph should be eroded. This adds extra margin to objects."), graph.erodeIterations);
			graph.erodeIterations = graph.erodeIterations < 0 ? 0 : (graph.erodeIterations > 16 ? 16 : graph.erodeIterations); //Clamp iterations to [0,16]

			if (graph.erodeIterations > 0) {
				EditorGUI.indentLevel++;
				graph.erosionUseTags = EditorGUILayout.Toggle(new GUIContent("Erosion Uses Tags", "Instead of making nodes unwalkable, " +
					"nodes will have their tag set to a value corresponding to their erosion level, " +
					"which is a quite good measurement of their distance to the closest wall.\nSee online documentation for more info."),
					graph.erosionUseTags);
				if (graph.erosionUseTags) {
					EditorGUI.indentLevel++;
					graph.erosionFirstTag = EditorGUILayoutHelper.TagField(new GUIContent("First Tag"), graph.erosionFirstTag, AstarPathEditor.EditTags);
					var tagNames = AstarPath.FindTagNames().Clone() as string[];
					var tagMsg = "";
					for (int i = graph.erosionFirstTag; i < graph.erosionFirstTag + graph.erodeIterations; i++) {
						tagMsg += (i > graph.erosionFirstTag ? (i == graph.erosionFirstTag + graph.erodeIterations - 1 ? " or " : ", ") : "") + tagNames[i];
					}
					EditorGUILayout.HelpBox("Tag " + tagMsg + " will be applied to nodes" + (graph.erodeIterations > 1 ? ", based on their distance to obstacles" : " when adjacent to obstacles"), MessageType.None);
					for (int i = graph.erosionFirstTag; i < graph.erosionFirstTag + graph.erodeIterations; i++) tagNames[i] += " (used for erosion)";
					graph.erosionTagsPrecedenceMask = EditorGUILayout.MaskField(
						new GUIContent("Overwritable tags", "Nodes near unwalkable nodes will be marked with tags. " +
							"If these nodes already have tags, you may want the custom tag to take precedence. This mask controls which tags are allowed to be replaced by the new erosion tags."),
						graph.erosionTagsPrecedenceMask,
						tagNames
						);
					if ((graph.erosionTagsPrecedenceMask & 0x1) == 0) {
						EditorGUILayout.HelpBox("The " + tagNames[0] + " tag has been excluded. Since this is the default tag, erosion tags will likely be applied to very few, if any, nodes. This is likely not what you want", MessageType.Warning);
					}
					EditorGUI.indentLevel--;
				}
				EditorGUI.indentLevel--;
			}
		}

		void DrawLastSection (GridGraph graph) {
			GUILayout.BeginHorizontal();
			GUILayout.Space(18);
			graph.showMeshSurface = GUILayout.Toggle(graph.showMeshSurface, new GUIContent("Show surface", "Toggles gizmos for drawing the surface of the mesh"), EditorStyles.miniButtonLeft);
			graph.showMeshOutline = GUILayout.Toggle(graph.showMeshOutline, new GUIContent("Show outline", "Toggles gizmos for drawing an outline of the nodes"), EditorStyles.miniButtonMid);
			graph.showNodeConnections = GUILayout.Toggle(graph.showNodeConnections, new GUIContent("Show connections", "Toggles gizmos for drawing node connections"), EditorStyles.miniButtonRight);
			GUILayout.EndHorizontal();
		}

		/// <summary>Draws the inspector for a <see cref="GraphCollision"/> class</summary>
		protected virtual void DrawCollisionEditor (GridGraph graph, GraphCollision collision) {
			collision = collision ?? new GraphCollision();

			DrawUse2DPhysics(collision);

			collision.collisionCheck = ToggleGroup("Collision testing", collision.collisionCheck);
			if (collision.collisionCheck) {
				EditorGUI.indentLevel++;
				string[] colliderOptions = collision.use2D ? new [] { "Circle", "Point" } : new [] { "Sphere", "Capsule", "Ray" };
				int[] colliderValues = collision.use2D ? new [] { 0, 2 } : new [] { 0, 1, 2 };
				// In 2D the Circle (Sphere) mode will replace both the Sphere and the Capsule modes
				// However make sure that the original value is still stored in the grid graph in case the user changes back to the 3D mode in the inspector.
				var tp = collision.type;
				if (tp == ColliderType.Capsule && collision.use2D) tp = ColliderType.Sphere;
				EditorGUI.BeginChangeCheck();
				tp = (ColliderType)EditorGUILayout.IntPopup("Collider type", (int)tp, colliderOptions, colliderValues);
				if (EditorGUI.EndChangeCheck()) collision.type = tp;

				// Only spheres and capsules have a diameter
				if (collision.type == ColliderType.Capsule || collision.type == ColliderType.Sphere) {
					collision.diameter = EditorGUILayout.FloatField(new GUIContent("Diameter", "Diameter of the capsule or sphere. 1 equals one node width"), collision.diameter);
					collision.diameter = Mathf.Max(collision.diameter, 0.01f);
				}

				if (!collision.use2D) {
					if (collision.type == ColliderType.Capsule || collision.type == ColliderType.Ray) {
						collision.height = EditorGUILayout.FloatField(new GUIContent("Height/Length", "Height of cylinder or length of ray in world units"), collision.height);
						collision.height = Mathf.Max(collision.height, 0.01f);
					}

					collision.collisionOffset = EditorGUILayout.FloatField(new GUIContent("Offset", "Offset upwards from the node. Can be used so that obstacles can be used as ground and at the same time as obstacles for lower positioned nodes"), collision.collisionOffset);
				}

				collision.mask = EditorGUILayoutx.LayerMaskField("Obstacle Layer Mask", collision.mask);

				DrawCollisionPreview(collision);
				EditorGUI.indentLevel--;
			}

			GUILayout.Space(2);

			if (collision.use2D) {
				EditorGUI.BeginDisabledGroup(collision.use2D);
				ToggleGroup("Height testing", false);
				EditorGUI.EndDisabledGroup();
			} else {
				collision.heightCheck = ToggleGroup("Height testing", collision.heightCheck);
				if (collision.heightCheck) {
					EditorGUI.indentLevel++;
					collision.fromHeight = EditorGUILayout.FloatField(new GUIContent("Ray length", "The height from which to check for ground"), collision.fromHeight);

					collision.heightMask = EditorGUILayoutx.LayerMaskField("Mask", collision.heightMask);

					// Layered grid graphs don't support thick raycasts
					if (graph.MaxLayers == 1) {
						collision.thickRaycast = EditorGUILayout.Toggle(new GUIContent("Thick Raycast", "Use a thick line instead of a thin line"), collision.thickRaycast);

						if (collision.thickRaycast) {
							EditorGUI.indentLevel++;
							collision.thickRaycastDiameter = EditorGUILayout.FloatField(new GUIContent("Diameter", "Diameter of the thick raycast"), collision.thickRaycastDiameter);
							EditorGUI.indentLevel--;
						}
					} else {
						collision.thickRaycast = false;
					}

					collision.unwalkableWhenNoGround = EditorGUILayout.Toggle(new GUIContent("Unwalkable when no ground", "Make nodes unwalkable when no ground was found with the height raycast. If height raycast is turned off, this doesn't affect anything"), collision.unwalkableWhenNoGround);
					EditorGUI.indentLevel--;
				}
			}
		}

		Vector3[] arcBuffer = new Vector3[21];
		Vector3[] lineBuffer = new Vector3[2];
		void DrawArc (Vector2 center, float radius, float startAngle, float endAngle) {
			// The AA line doesn't always properly close the gap even for full circles
			endAngle += 1*Mathf.Deg2Rad;
			var width = 4;
			// The DrawAAPolyLine method does not draw a centered line unfortunately
			//radius -= width/2;
			for (int i = 0; i < arcBuffer.Length; i++) {
				float t = i * 1.0f / (arcBuffer.Length-1);
				float angle = Mathf.Lerp(startAngle, endAngle, t);
				arcBuffer[i] = new Vector3(center.x + radius * Mathf.Cos(angle), center.y + radius * Mathf.Sin(angle), 0);
			}
			Handles.DrawAAPolyLine(EditorResourceHelper.HandlesAALineTexture, width, arcBuffer);
		}

		void DrawLine (Vector2 a, Vector2 b) {
			lineBuffer[0] = a;
			lineBuffer[1] = b;
			Handles.DrawAAPolyLine(EditorResourceHelper.HandlesAALineTexture, 4, lineBuffer);
		}

		void DrawDashedLine (Vector2 a, Vector2 b, float dashLength) {
			if (dashLength == 0) {
				DrawLine(a, b);
			} else {
				var dist = (b - a).magnitude;
				int steps = Mathf.RoundToInt(dist / dashLength);
				for (int i = 0; i < steps; i++) {
					var t1 = i * 1.0f / (steps-1);
					var t2 = (i + 0.5f) * 1.0f / (steps-1);
					DrawLine(Vector2.Lerp(a, b, t1), Vector2.Lerp(a, b, t2));
				}
			}
		}

		static int RoundUpToNextOddNumber (float x) {
			return Mathf.CeilToInt((x - 1)/2.0f)*2 + 1;
		}

		float interpolatedGridWidthInNodes = -1;
		float lastTime = 0;

		void DrawCollisionPreview (GraphCollision collision) {
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(2);
			collisionPreviewOpen = EditorGUILayout.Foldout(collisionPreviewOpen, "Preview");
			EditorGUILayout.EndHorizontal();
			if (!collisionPreviewOpen) return;

			EditorGUILayout.Separator();
			var rect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(10, 100));
			var m = Handles.matrix;
			Handles.matrix = Handles.matrix * Matrix4x4.Translate(new Vector3(rect.xMin, rect.yMin));

			// Draw NxN grid with circle in the middle
			// Draw Flat plane with capsule/sphere/line above

			Handles.color = Color.white;
			int gridWidthInNodes = collision.type == ColliderType.Ray ? 3 : Mathf.Max(3, RoundUpToNextOddNumber(collision.diameter + 0.5f));
			if (interpolatedGridWidthInNodes == -1) interpolatedGridWidthInNodes = gridWidthInNodes;
			if (Mathf.Abs(interpolatedGridWidthInNodes - gridWidthInNodes) < 0.01f) interpolatedGridWidthInNodes = gridWidthInNodes;
			else editor.Repaint();

			var dt = Time.realtimeSinceStartup - lastTime;
			lastTime = Time.realtimeSinceStartup;
			interpolatedGridWidthInNodes = Mathf.Lerp(interpolatedGridWidthInNodes, gridWidthInNodes, 5 * dt);

			var gridCenter = collision.use2D ? new Vector2(rect.width / 2.0f, rect.height * 0.5f) : new Vector2(rect.width / 3.0f, rect.height * 0.5f);
			var gridWidth = Mathf.Min(rect.width / 3, rect.height);
			var nodeSize = (this.target as GridGraph).nodeSize;
			var scale = gridWidth / (nodeSize * interpolatedGridWidthInNodes);
			var diameter = collision.type == ColliderType.Ray ? 0.05f : collision.diameter * nodeSize;
			var interpolatedGridScale = gridWidthInNodes * nodeSize * scale;
			for (int i = 0; i <= gridWidthInNodes; i++) {
				var c = i*1.0f/gridWidthInNodes;
				DrawLine(gridCenter + new Vector2(c - 0.5f, -0.5f) * interpolatedGridScale, gridCenter + new Vector2(c - 0.5f, 0.5f) * interpolatedGridScale);
				DrawLine(gridCenter + new Vector2(-0.5f, c - 0.5f) * interpolatedGridScale, gridCenter + new Vector2(0.5f, c - 0.5f) * interpolatedGridScale);
			}

			var sideBase = new Vector2(2*rect.width / 3f, rect.height);
			float sideScale;
			if (collision.type == ColliderType.Sphere) {
				sideScale = scale;
				// A high collision offset should not cause it to break
				sideScale = Mathf.Min(sideScale, sideBase.y / (Mathf.Max(0, collision.collisionOffset) + diameter));
			} else {
				sideScale = Mathf.Max(scale * 0.5f, Mathf.Min(scale, sideBase.y / (collision.height + collision.collisionOffset + diameter * 0.5f)));
				// A high collision offset should not cause it to break
				sideScale = Mathf.Min(sideScale, sideBase.y / (Mathf.Max(0, collision.collisionOffset) + diameter));
			}

			var darkGreen = new Color(9/255f, 150/255f, 23/255f);
			var lightGreen = new Color(12/255f, 194/255f, 30/255f);
			var green = EditorGUIUtility.isProSkin ? lightGreen : darkGreen;

			Handles.color = green;
			DrawArc(gridCenter, diameter * 0.5f * scale, 0, Mathf.PI*2);

			if (!collision.use2D) {
				Handles.color = Color.white;
				var interpolatedGridSideScale = gridWidthInNodes * nodeSize * sideScale;

				DrawLine(sideBase + new Vector2(-interpolatedGridSideScale * 0.5f, 0), sideBase + new Vector2(interpolatedGridSideScale * 0.5f, 0));
				for (int i = 0; i <= gridWidthInNodes; i++) {
					var c = i*1.0f/gridWidthInNodes;
					DrawArc(sideBase + new Vector2(c - 0.5f, 0) * interpolatedGridSideScale, 2, 0, Mathf.PI*2);
				}

				Handles.color = green;

				if (collision.type == ColliderType.Ray) {
					var height = collision.height;
					var maxHeight = sideBase.y / sideScale - (collision.collisionOffset + diameter*0.5f);
					float dashLength = 0;
					if (collision.height > maxHeight + 0.01f) {
						height = maxHeight;
						dashLength = 6;
					}

					var offset = sideBase + new Vector2(0, -collision.collisionOffset) * sideScale;
					DrawLine(offset + new Vector2(0, -height*0.75f) * sideScale, offset);
					DrawDashedLine(offset + new Vector2(0, -height) * sideScale, offset + new Vector2(0, -height * 0.75f) * sideScale, dashLength);
					DrawLine(offset, offset + new Vector2(6, -6));
					DrawLine(offset, offset + new Vector2(-6, -6));
				} else {
					var height = collision.type == ColliderType.Capsule ? collision.height : 0;
					// sideBase.y - (collision.collisionOffset + height + diameter * 0.5f) * scale < 0
					var maxHeight = sideBase.y / sideScale - (collision.collisionOffset + diameter*0.5f);
					float dashLength = 0;
					if (height > maxHeight + 0.01f) {
						height = maxHeight;
						dashLength = 6;
					}
					DrawArc(sideBase + new Vector2(0, -collision.collisionOffset * sideScale), diameter * 0.5f * sideScale, 0, Mathf.PI);
					DrawArc(sideBase + new Vector2(0, -(height + collision.collisionOffset) * sideScale), diameter * 0.5f * sideScale, Mathf.PI, 2*Mathf.PI);
					DrawDashedLine(sideBase + new Vector2(-diameter * 0.5f, -collision.collisionOffset) * sideScale, sideBase + new Vector2(-diameter * 0.5f, -(height + collision.collisionOffset)) * sideScale, dashLength);
					DrawDashedLine(sideBase + new Vector2(diameter * 0.5f, -collision.collisionOffset) * sideScale, sideBase + new Vector2(diameter * 0.5f, -(height + collision.collisionOffset)) * sideScale, dashLength);
				}
			}
			Handles.matrix = m;
			EditorGUILayout.Separator();
		}

		protected virtual void DrawUse2DPhysics (GraphCollision collision) {
			collision.use2D = EditorGUILayout.Toggle(new GUIContent("Use 2D Physics", "Use the Physics2D API for collision checking"), collision.use2D);

			if (collision.use2D) {
				var graph = target as GridGraph;
				if (Mathf.Abs(Vector3.Dot(Vector3.forward, Quaternion.Euler(graph.rotation) * Vector3.up)) < 0.9f) {
					EditorGUILayout.HelpBox("When using 2D physics it is recommended to rotate the graph so that it aligns with the 2D plane.", MessageType.Warning);
				}
			}
		}

		static Dictionary<System.Type, System.Type> ruleEditors;
		static Dictionary<System.Type, string> ruleHeaders;
		static List<System.Type> ruleTypes;
		Dictionary<GridGraphRule, IGridGraphRuleEditor> ruleEditorInstances = new Dictionary<GridGraphRule, IGridGraphRuleEditor>();

		static void FindRuleEditors () {
			ruleEditors = new Dictionary<System.Type, System.Type>();
			ruleHeaders = new Dictionary<System.Type, string>();
			ruleTypes = new List<System.Type>();
			foreach (var type in TypeCache.GetTypesWithAttribute<CustomGridGraphRuleEditorAttribute>()) {
				var attrs = type.GetCustomAttributes(typeof(CustomGridGraphRuleEditorAttribute), false);
				foreach (CustomGridGraphRuleEditorAttribute attr in attrs) {
					ruleEditors[attr.type] = type;
					ruleHeaders[attr.type] = attr.name;
				}
			}

			foreach (var type in TypeCache.GetTypesDerivedFrom<GridGraphRule>()) {
				if (!type.IsAbstract) ruleTypes.Add(type);
			}
		}

		IGridGraphRuleEditor GetEditor (GridGraphRule rule) {
			if (ruleEditors == null) FindRuleEditors();
			IGridGraphRuleEditor ruleEditor;
			if (!ruleEditorInstances.TryGetValue(rule, out ruleEditor)) {
				if (ruleEditors.ContainsKey(rule.GetType())) {
					ruleEditor = ruleEditorInstances[rule] = (IGridGraphRuleEditor)System.Activator.CreateInstance(ruleEditors[rule.GetType()]);
				}
			}
			return ruleEditor;
		}

		protected virtual void DrawRules (GridGraph graph) {
			var rules = graph.rules.GetRules();

			for (int i = 0; i < rules.Count; i++) {
				var rule = rules[i];
				if (rule != null) {
					var ruleEditor = GetEditor(rule);
					var ruleType = rule.GetType();
					GUILayout.BeginHorizontal();
					rule.enabled = ToggleGroup(ruleHeaders.TryGetValue(ruleType, out var header) ? header : ruleType.Name, rule.enabled);
					if (GUILayout.Button("", AstarPathEditor.astarSkin.FindStyle("SimpleDeleteButton"))) {
						graph.rules.RemoveRule(rule);
						ruleEditorInstances.Remove(rule);
						rule.enabled = false;
						rule.DisposeUnmanagedData();
					}
					GUILayout.EndHorizontal();

					if (rule.enabled) {
						if (ruleEditor != null) {
							EditorGUI.indentLevel++;
							EditorGUI.BeginChangeCheck();
							ruleEditor.OnInspectorGUI(graph, rule);
							if (EditorGUI.EndChangeCheck()) rule.SetDirty();
							EditorGUI.indentLevel--;
						} else {
							EditorGUILayout.HelpBox("No editor found for " + rule.GetType().Name, MessageType.Error);
						}
					}
				}
			}

			EditorGUILayout.Separator();

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			if (GUILayout.Button("Add Rule", GUILayout.Height(30))) {
				if (ruleEditors == null) FindRuleEditors();
				GenericMenu menu = new GenericMenu();
				foreach (var type in ruleTypes) {
					menu.AddItem(new GUIContent(ruleHeaders.TryGetValue(type, out var header) ? header : type.Name), false, ruleType => graph.rules.AddRule(System.Activator.CreateInstance((System.Type)ruleType) as GridGraphRule), type);
				}
				menu.ShowAsContext();
			}
			GUILayout.Space(10);
			GUILayout.EndHorizontal();
		}

		public static GridPivot PivotPointSelector (GridPivot pivot) {
			// Find required styles
			gridPivotSelectBackground = gridPivotSelectBackground ?? AstarPathEditor.astarSkin.FindStyle("GridPivotSelectBackground");
			gridPivotSelectButton = gridPivotSelectButton ?? AstarPathEditor.astarSkin.FindStyle("GridPivotSelectButton");

			Rect r = GUILayoutUtility.GetRect(19, 19, gridPivotSelectBackground);

			// I have no idea why... but this is required for it to work well
			r.y -= 14;

			r.width = 19;
			r.height = 19;

			if (gridPivotSelectBackground == null) {
				return pivot;
			}

			if (Event.current.type == EventType.Repaint) {
				gridPivotSelectBackground.Draw(r, false, false, false, false);
			}

			if (GUI.Toggle(new Rect(r.x, r.y, 7, 7), pivot == GridPivot.TopLeft, "", gridPivotSelectButton))
				pivot = GridPivot.TopLeft;

			if (GUI.Toggle(new Rect(r.x+12, r.y, 7, 7), pivot == GridPivot.TopRight, "", gridPivotSelectButton))
				pivot = GridPivot.TopRight;

			if (GUI.Toggle(new Rect(r.x+12, r.y+12, 7, 7), pivot == GridPivot.BottomRight, "", gridPivotSelectButton))
				pivot = GridPivot.BottomRight;

			if (GUI.Toggle(new Rect(r.x, r.y+12, 7, 7), pivot == GridPivot.BottomLeft, "", gridPivotSelectButton))
				pivot = GridPivot.BottomLeft;

			if (GUI.Toggle(new Rect(r.x+6, r.y+6, 7, 7), pivot == GridPivot.Center, "", gridPivotSelectButton))
				pivot = GridPivot.Center;

			return pivot;
		}

		static readonly Vector3[] handlePoints = new [] { new Vector3(0.0f, 0, 0.5f), new Vector3(1.0f, 0, 0.5f), new Vector3(0.5f, 0, 0.0f), new Vector3(0.5f, 0, 1.0f) };

		public override void OnSceneGUI (NavGraph target) {
			Event e = Event.current;

			var graph = target as GridGraph;

			graph.UpdateTransform();
			var currentTransform = graph.transform * Matrix4x4.Scale(new Vector3(graph.width, 1, graph.depth));

			if (e.type == EventType.MouseDown) {
				isMouseDown = true;
			} else if (e.type == EventType.MouseUp) {
				isMouseDown = false;
			}

			if (!isMouseDown) {
				savedTransform = currentTransform;
				savedDimensions = new Vector2(graph.width, graph.depth);
				savedNodeSize = graph.nodeSize;
			}

			Handles.matrix = Matrix4x4.identity;
			Handles.color = AstarColor.BoundsHandles;
			Handles.CapFunction cap = Handles.CylinderHandleCap;

			var center = currentTransform.Transform(new Vector3(0.5f, 0, 0.5f));
			if (Tools.current == Tool.Scale) {
				const float HandleScale = 0.1f;

				Vector3 mn = Vector3.zero;
				Vector3 mx = Vector3.zero;
				EditorGUI.BeginChangeCheck();
				for (int i = 0; i < handlePoints.Length; i++) {
					var ps = currentTransform.Transform(handlePoints[i]);
					Vector3 p = savedTransform.InverseTransform(Handles.Slider(ps, ps - center, HandleScale*HandleUtility.GetHandleSize(ps), cap, 0));

					// Snap to increments of whole nodes
					p.x = Mathf.Round(p.x * savedDimensions.x) / savedDimensions.x;
					p.z = Mathf.Round(p.z * savedDimensions.y) / savedDimensions.y;

					if (i == 0) {
						mn = mx = p;
					} else {
						mn = Vector3.Min(mn, p);
						mx = Vector3.Max(mx, p);
					}
				}

				if (EditorGUI.EndChangeCheck()) {
					graph.center = savedTransform.Transform((mn + mx) * 0.5f);
					graph.unclampedSize = Vector2.Scale(new Vector2(mx.x - mn.x, mx.z - mn.z), savedDimensions) * savedNodeSize;
				}
			} else if (Tools.current == Tool.Move) {
				EditorGUI.BeginChangeCheck();
				center = Handles.PositionHandle(graph.center, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : Quaternion.Euler(graph.rotation));

				if (EditorGUI.EndChangeCheck() && Tools.viewTool != ViewTool.Orbit) {
					graph.center = center;
				}
			} else if (Tools.current == Tool.Rotate) {
				EditorGUI.BeginChangeCheck();
				var rot = Handles.RotationHandle(Quaternion.Euler(graph.rotation), graph.center);

				if (EditorGUI.EndChangeCheck() && Tools.viewTool != ViewTool.Orbit) {
					graph.rotation = rot.eulerAngles;
				}
			}

			var rules = graph.rules.GetRules();
			for (int i = 0; i < rules.Count; i++) {
				var rule = rules[i];
				if (rule != null && rule.enabled) {
					var ruleEditor = GetEditor(rule);
					if (ruleEditor != null) {
						ruleEditor.OnSceneGUI(graph, rule);
					}
				}
			}
		}

		public enum GridPivot {
			Center,
			TopLeft,
			TopRight,
			BottomLeft,
			BottomRight
		}
	}
}
