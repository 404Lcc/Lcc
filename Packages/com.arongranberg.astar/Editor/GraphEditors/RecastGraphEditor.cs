using UnityEngine;
using UnityEditor;
using Pathfinding.Graphs.Navmesh;
using UnityEditorInternal;

namespace Pathfinding {
	/// <summary>Editor for the RecastGraph.</summary>
	[CustomGraphEditor(typeof(RecastGraph), "Recast Graph")]
	public class RecastGraphEditor : GraphEditor {
		public static bool tagMaskFoldout;
		public static bool meshesUnreadableAtRuntimeFoldout;
		ReorderableList tagMaskList;
		ReorderableList perLayerModificationsList;

		public enum UseTiles {
			UseTiles = 0,
			DontUseTiles = 1
		}

		static readonly GUIContent[] DimensionModeLabels = new [] {
			new GUIContent("2D"),
			new GUIContent("3D"),
		};

		static Rect SliceColumn (ref Rect rect, float width, float spacing = 0) {
			return GUIUtilityx.SliceColumn(ref rect, width, spacing);
		}

		static void DrawIndentedList (ReorderableList list) {
			GUILayout.BeginHorizontal();
			GUILayout.Space(EditorGUI.IndentedRect(default).xMin);
			list.DoLayoutList();
			GUILayout.Space(3);
			GUILayout.EndHorizontal();
		}

		static void DrawColliderDetail (RecastGraph.CollectionSettings settings) {
			const float LowestApproximationError = 0.5f;
			settings.colliderRasterizeDetail = EditorGUILayout.Slider(new GUIContent("Round Collider Detail", "Controls the detail of the generated sphere and capsule meshes. "+
				"Higher values may increase navmesh quality slightly, and lower values improve graph scanning performance."), Mathf.Round(10*settings.colliderRasterizeDetail)*0.1f, 0, 1.0f / LowestApproximationError);
		}

		void DrawCollectionSettings (RecastGraph.CollectionSettings settings, RecastGraph.DimensionMode dimensionMode) {
			settings.collectionMode = (RecastGraph.CollectionSettings.FilterMode)EditorGUILayout.EnumPopup("Filter Objects By", settings.collectionMode);

			if (settings.collectionMode == RecastGraph.CollectionSettings.FilterMode.Layers) {
				settings.layerMask = EditorGUILayoutx.LayerMaskField("Layer Mask", settings.layerMask);
			} else {
				DrawIndentedList(tagMaskList);
			}

			if (dimensionMode == RecastGraph.DimensionMode.Dimension3D) {
				settings.rasterizeTerrain = EditorGUILayout.Toggle(new GUIContent("Rasterize Terrains", "Should a rasterized terrain be included"), settings.rasterizeTerrain);
				if (settings.rasterizeTerrain) {
					EditorGUI.indentLevel++;
					settings.rasterizeTrees = EditorGUILayout.Toggle(new GUIContent("Rasterize Trees", "Rasterize tree colliders on terrains. " +
						"If the tree prefab has a collider, that collider will be rasterized. " +
						"Otherwise a simple box collider will be used and the script will " +
						"try to adjust it to the tree's scale, it might not do a very good job though so " +
						"an attached collider is preferable."), settings.rasterizeTrees);
					settings.terrainHeightmapDownsamplingFactor = EditorGUILayout.IntField(new GUIContent("Heightmap Downsampling", "How much to downsample the terrain's heightmap. A lower value is better, but slower to scan"), settings.terrainHeightmapDownsamplingFactor);
					settings.terrainHeightmapDownsamplingFactor = Mathf.Max(1, settings.terrainHeightmapDownsamplingFactor);
					EditorGUI.indentLevel--;
				}

				settings.rasterizeMeshes = EditorGUILayout.Toggle(new GUIContent("Rasterize Meshes", "Should meshes be rasterized and used for building the navmesh"), settings.rasterizeMeshes);
				settings.rasterizeColliders = EditorGUILayout.Toggle(new GUIContent("Rasterize Colliders", "Should colliders be rasterized and used for building the navmesh"), settings.rasterizeColliders);
			} else {
				// Colliders are always rasterized in 2D mode
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.Toggle(new GUIContent("Rasterize Colliders", "Should colliders be rasterized and used for building the navmesh. In 2D mode, this is always enabled."), true);
				EditorGUI.EndDisabledGroup();
			}

			if (settings.rasterizeMeshes && settings.rasterizeColliders && dimensionMode == RecastGraph.DimensionMode.Dimension3D) {
				EditorGUILayout.HelpBox("You are rasterizing both meshes and colliders. This is likely just duplicate work if the colliders and meshes are similar in shape. You can use the RecastNavmeshModifier component" +
					" to always include some specific objects regardless of what the above settings are set to.", MessageType.Info);
			}
		}

		public override void OnEnable () {
			base.OnEnable();
			var graph = target as RecastGraph;
			tagMaskList = new ReorderableList(graph.collectionSettings.tagMask, typeof(string), true, true, true, true) {
				drawElementCallback = (Rect rect, int index, bool active, bool isFocused) => {
					graph.collectionSettings.tagMask[index] = EditorGUI.TagField(rect, graph.collectionSettings.tagMask[index]);
				},
				drawHeaderCallback = (Rect rect) => {
					GUI.Label(rect, "Tag mask");
				},
				elementHeight = EditorGUIUtility.singleLineHeight,
				onAddCallback = (ReorderableList list) => {
					graph.collectionSettings.tagMask.Add("Untagged");
				}
			};

			perLayerModificationsList = new ReorderableList(graph.perLayerModifications, typeof(RecastGraph.PerLayerModification), true, true, true, true) {
				drawElementCallback = (Rect rect, int index, bool active, bool isFocused) => {
					var element = graph.perLayerModifications[index];
					var w = rect.width;
					var spacing = EditorGUIUtility.standardVerticalSpacing;
					element.layer = EditorGUI.LayerField(SliceColumn(ref rect, w * 0.3f, spacing), element.layer);

					if (element.mode == RecastNavmeshModifier.Mode.WalkableSurfaceWithTag) {
						element.mode = (RecastNavmeshModifier.Mode)EditorGUI.EnumPopup(SliceColumn(ref rect, w * 0.4f, spacing), element.mode);
						element.surfaceID = Util.EditorGUILayoutHelper.TagField(rect, GUIContent.none, element.surfaceID, AstarPathEditor.EditTags);
						element.surfaceID = Mathf.Clamp(element.surfaceID, 0, GraphNode.MaxTagIndex);
					} else if (element.mode == RecastNavmeshModifier.Mode.WalkableSurfaceWithSeam) {
						element.mode = (RecastNavmeshModifier.Mode)EditorGUI.EnumPopup(SliceColumn(ref rect, w * 0.4f, spacing), element.mode);
						string helpTooltip = "All surfaces on this mesh will be walkable and a " +
											 "seam will be created between the surfaces on this mesh and the surfaces on other meshes (with a different surface id)";
						GUI.Label(SliceColumn(ref rect, 70, spacing), new GUIContent("Surface ID", helpTooltip));
						element.surfaceID = Mathf.Max(0, EditorGUI.IntField(rect, new GUIContent("", helpTooltip), element.surfaceID));
					} else {
						element.mode = (RecastNavmeshModifier.Mode)EditorGUI.EnumPopup(rect, element.mode);
					}

					graph.perLayerModifications[index] = element;
				},
				drawHeaderCallback = (Rect rect) => {
					GUI.Label(rect, "Per Layer Modifications");
				},
				elementHeight = EditorGUIUtility.singleLineHeight,
				onAddCallback = (ReorderableList list) => {
					// Find the first layer that is not already modified
					var availableLayers = graph.collectionSettings.layerMask;
					foreach (var mod in graph.perLayerModifications) {
						availableLayers &= ~(1 << mod.layer);
					}
					var newMod = RecastGraph.PerLayerModification.Default;
					for (int i = 0; i < 32; i++) {
						if ((availableLayers & (1 << i)) != 0) {
							newMod.layer = i;
							break;
						}
					}
					graph.perLayerModifications.Add(newMod);
				}
			};
		}

		public override void OnInspectorGUI (NavGraph target) {
			var graph = target as RecastGraph;

			Header("Shape");

			graph.dimensionMode = (RecastGraph.DimensionMode)EditorGUILayout.Popup(new GUIContent("Dimensions", "Should the graph be for a 2D or 3D world?"), (int)graph.dimensionMode, DimensionModeLabels);
			if (graph.dimensionMode == RecastGraph.DimensionMode.Dimension2D && Mathf.Abs(Vector3.Dot(Quaternion.Euler(graph.rotation) * Vector3.up, Vector3.forward)) < 0.99999f) {
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
				GUILayout.Label(EditorGUIUtility.IconContent("console.warnicon"), GUILayout.ExpandWidth(false));
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Your graph is not in the XY plane");
				if (GUILayout.Button("Align")) {
					graph.rotation = new Vector3(-90, 0, 0);
					graph.forcedBoundsCenter = new Vector3(graph.forcedBoundsCenter.x, graph.forcedBoundsCenter.y, -graph.forcedBoundsSize.y * 0.5f);
				}
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
			}

			// In 3D mode, we use the graph's center as the pivot point, but in 2D mode, we use the center of the base plane of the graph as the pivot point.
			// This makes sense because in 2D mode, you typically want to set the base plane's center to Z=0, and you don't care much about the height of the graph.
			var pivot = graph.dimensionMode == RecastGraph.DimensionMode.Dimension2D ? new Vector3(0.0f, -0.5f, 0.0f) : Vector3.zero;
			var centerOffset = Quaternion.Euler(graph.rotation) * Vector3.Scale(graph.forcedBoundsSize, pivot);
			var newCenter = EditorGUILayout.Vector3Field("Center", graph.forcedBoundsCenter + centerOffset);
			var newSize = EditorGUILayout.Vector3Field("Size", graph.forcedBoundsSize);

			// Make sure the bounding box is not infinitely thin along any axis
			newSize = Vector3.Max(newSize, Vector3.one * 0.001f);

			// Recalculate the center offset with the new size, and then adjust the center so that the pivot point stays the same if the size changes
			centerOffset = Quaternion.Euler(graph.rotation) * Vector3.Scale(newSize, pivot);
			graph.forcedBoundsCenter = RoundVector3(newCenter) - centerOffset;
			graph.forcedBoundsSize = RoundVector3(newSize);

			graph.rotation = RoundVector3(EditorGUILayout.Vector3Field("Rotation", graph.rotation));

			long estWidth = Mathf.RoundToInt(Mathf.Ceil(graph.forcedBoundsSize.x / graph.cellSize));
			long estDepth = Mathf.RoundToInt(Mathf.Ceil(graph.forcedBoundsSize.z / graph.cellSize));

			EditorGUI.BeginDisabledGroup(true);
			var estTilesX = (estWidth + graph.editorTileSize - 1) / graph.editorTileSize;
			var estTilesZ = (estDepth + graph.editorTileSize - 1) / graph.editorTileSize;
			var label = estWidth.ToString() + " x " + estDepth.ToString() + " voxels";
			if (graph.useTiles) {
				label += ", divided into " + (estTilesX*estTilesZ) + " tiles";
			}
			EditorGUILayout.LabelField(new GUIContent("Size", "Based on the voxel size and the bounding box"), new GUIContent(label));
			EditorGUI.EndDisabledGroup();

			// Show a warning if the number of voxels is too large
			if (estWidth*estDepth >= 3000*3000) {
				GUIStyle helpBox = GUI.skin.FindStyle("HelpBox") ?? GUI.skin.FindStyle("Box");

				Color preColor = GUI.color;
				if (estWidth*estDepth >= 8192*8192) {
					GUI.color = Color.red;
				} else {
					GUI.color = Color.yellow;
				}

				GUILayout.Label("Warning: Might take some time to calculate", helpBox);
				GUI.color = preColor;
			}

			if (!editor.isPrefab) {
				if (GUILayout.Button(new GUIContent("Snap bounds to scene", "Will snap the bounds of the graph to exactly contain all meshes in the scene that matches the masks."))) {
					graph.SnapBoundsToScene();
					GUI.changed = true;
				}
			}

			Separator();
			Header("Input Filtering");

			DrawCollectionSettings(graph.collectionSettings, graph.dimensionMode);

			Separator();
			Header("Agent Characteristics");

			graph.characterRadius = EditorGUILayout.FloatField(new GUIContent("Character Radius", "Radius of the character. It's good to add some margin.\nIn world units."), graph.characterRadius);
			graph.characterRadius = Mathf.Max(graph.characterRadius, 0);

			if (graph.characterRadius < graph.cellSize * 2) {
				EditorGUILayout.HelpBox("For best navmesh quality, it is recommended to keep the character radius at least 2 times as large as the voxel size. Smaller voxels will give you higher quality navmeshes, but it will take more time to scan the graph.", MessageType.Warning);
			}

			if (graph.dimensionMode == RecastGraph.DimensionMode.Dimension3D) {
				graph.walkableHeight = EditorGUILayout.DelayedFloatField(new GUIContent("Character Height", "Minimum distance to the roof for an area to be walkable"), graph.walkableHeight);
				graph.walkableHeight = Mathf.Max(graph.walkableHeight, 0);

				graph.walkableClimb = EditorGUILayout.FloatField(new GUIContent("Max Step Height", "How high can the character step up vertically"), graph.walkableClimb);

				// A walkableClimb higher than this can cause issues when generating the navmesh since then it can in some cases
				// Both be valid for a character to walk under an obstacle and climb up on top of it (and that cannot be handled with a navmesh without links)
				if (graph.walkableClimb >= graph.walkableHeight) {
					graph.walkableClimb = graph.walkableHeight;
					EditorGUILayout.HelpBox("Max Step Height should be less than Character Height. Clamping to " + graph.walkableHeight+".", MessageType.Warning);
				} else if (graph.walkableClimb < 0) {
					graph.walkableClimb = 0;
				}

				graph.maxSlope = EditorGUILayout.Slider(new GUIContent("Max Slope", "Approximate maximum slope"), graph.maxSlope, 0F, 90F);
			}

			if (graph.dimensionMode == RecastGraph.DimensionMode.Dimension2D) {
				graph.backgroundTraversability = (RecastGraph.BackgroundTraversability)EditorGUILayout.EnumPopup("Background traversability", graph.backgroundTraversability);
			}

			DrawIndentedList(perLayerModificationsList);

			int seenLayers = 0;
			for (int i = 0; i < graph.perLayerModifications.Count; i++) {
				if ((seenLayers & 1 << graph.perLayerModifications[i].layer) != 0) {
					EditorGUILayout.HelpBox("Duplicate layers. Each layer can only be modified by a single rule.", MessageType.Error);
					break;
				}
				seenLayers |= 1 << graph.perLayerModifications[i].layer;
			}

			Separator();
			Header("Rasterization");

			graph.cellSize = EditorGUILayout.FloatField(new GUIContent("Voxel Size", "Size of one voxel in world units"), graph.cellSize);
			if (graph.cellSize < 0.001F) graph.cellSize = 0.001F;

			graph.useTiles = (UseTiles)EditorGUILayout.EnumPopup("Use Tiles", graph.useTiles ? UseTiles.UseTiles : UseTiles.DontUseTiles) == UseTiles.UseTiles;

			if (graph.useTiles) {
				EditorGUI.indentLevel++;
				graph.editorTileSize = EditorGUILayout.IntField(new GUIContent("Tile Size (voxels)", "Size in voxels of a single tile.\n" +
					"This is the width of the tile.\n" +
					"\n" +
					"A large tile size can be faster to initially scan (but beware of out of memory issues if you try with a too large tile size in a large world)\n" +
					"smaller tile sizes are (much) faster to update.\n" +
					"\n" +
					"Different tile sizes can affect the quality of paths. It is often good to split up huge open areas into several tiles for\n" +
					"better quality paths, but too small tiles can lead to effects looking like invisible obstacles.\n\n" +
					"Typical values are between 64 and 256"), graph.editorTileSize);
				graph.editorTileSize = Mathf.Max(10, graph.editorTileSize);
				EditorGUI.indentLevel--;
			}

			graph.maxEdgeLength = EditorGUILayout.FloatField(new GUIContent("Max Border Edge Length", "Maximum length of one border edge in the completed navmesh before it is split. A lower value can often yield better quality graphs, but don't use so low values so that you get a lot of thin triangles."), graph.maxEdgeLength);
			graph.maxEdgeLength = graph.maxEdgeLength < graph.cellSize ? graph.cellSize : graph.maxEdgeLength;

			// This is actually a float, but to make things easier for the user, we only allow picking integers. Small changes don't matter that much anyway.
			graph.contourMaxError = EditorGUILayout.IntSlider(new GUIContent("Edge Simplification", "Simplifies the edges of the navmesh such that it is no more than this number of voxels away from the true value.\nIn voxels."), Mathf.RoundToInt(graph.contourMaxError), 0, 5);
			graph.minRegionSize = EditorGUILayout.FloatField(new GUIContent("Min Region Size", "Small regions will be removed. In voxels. Only regions within single tiles can be removed. Regions that span multiple tiles will always be kept. If you don't use tiling, then all small regions will be removed."), graph.minRegionSize);

			var effectivelyRasterizingColliders = graph.collectionSettings.rasterizeColliders || (graph.dimensionMode == RecastGraph.DimensionMode.Dimension3D && graph.collectionSettings.rasterizeTerrain && graph.collectionSettings.rasterizeTrees) || graph.dimensionMode == RecastGraph.DimensionMode.Dimension2D;
			if (effectivelyRasterizingColliders) {
				DrawColliderDetail(graph.collectionSettings);
			}

			var countStillUnreadable = 0;
			for (int i = 0; graph.meshesUnreadableAtRuntime != null && i < graph.meshesUnreadableAtRuntime.Count; i++) {
				countStillUnreadable += graph.meshesUnreadableAtRuntime[i].Item2.isReadable ? 0 : 1;
			}
			if (countStillUnreadable > 0) {
				GUILayout.BeginHorizontal();
				GUILayout.Space(EditorGUI.IndentedRect(new Rect(0, 0, 0, 0)).xMin);
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				meshesUnreadableAtRuntimeFoldout = GUILayout.Toggle(meshesUnreadableAtRuntimeFoldout, "", EditorStyles.foldout, GUILayout.Width(10));
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();

				GUILayout.Label(EditorGUIUtility.IconContent("console.warnicon"), GUILayout.ExpandWidth(false));
				GUILayout.Label(graph.meshesUnreadableAtRuntime.Count + " " + (graph.meshesUnreadableAtRuntime.Count > 1 ? "meshes" : "mesh") + " will be ignored if scanned in a standalone build, because they are marked as not readable." +
					"If you plan to scan the graph in a standalone build, all included meshes must be marked as read/write in their import settings.", EditorStyles.wordWrappedMiniLabel);
				// EditorGUI.DrawTextureTransparent() EditorGUIUtility.IconContent("console.warnicon")
				GUILayout.EndHorizontal();

				if (meshesUnreadableAtRuntimeFoldout) {
					EditorGUILayout.Separator();
					for (int i = 0; i < graph.meshesUnreadableAtRuntime.Count; i++) {
						var(source, mesh) = graph.meshesUnreadableAtRuntime[i];
						if (!mesh.isReadable) {
							GUILayout.BeginHorizontal();
							EditorGUI.BeginDisabledGroup(true);
							EditorGUILayout.ObjectField(source, typeof(Mesh), true);
							EditorGUILayout.ObjectField(mesh, typeof(Mesh), false);
							EditorGUI.EndDisabledGroup();
							if (GUILayout.Button("Make readable")) {
								var importer = ModelImporter.GetAtPath(AssetDatabase.GetAssetPath(mesh)) as ModelImporter;
								if (importer != null) {
									importer.isReadable = true;
									importer.SaveAndReimport();
								}
							}
							GUILayout.EndHorizontal();
						}
					}
				}
				EditorGUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}

			Separator();
			Header("Runtime Settings");

			graph.enableNavmeshCutting = EditorGUILayout.Toggle(new GUIContent("Affected by Navmesh Cuts", "Makes this graph affected by NavmeshCut and NavmeshAdd components. See the documentation for more info."), graph.enableNavmeshCutting);

			Separator();
			Header("Debug");
			GUILayout.BeginHorizontal();
			GUILayout.Space(18);
			graph.showMeshSurface = GUILayout.Toggle(graph.showMeshSurface, new GUIContent("Show surface", "Toggles gizmos for drawing the surface of the mesh"), EditorStyles.miniButtonLeft);
			graph.showMeshOutline = GUILayout.Toggle(graph.showMeshOutline, new GUIContent("Show outline", "Toggles gizmos for drawing an outline of the nodes"), EditorStyles.miniButtonMid);
			graph.showNodeConnections = GUILayout.Toggle(graph.showNodeConnections, new GUIContent("Show connections", "Toggles gizmos for drawing node connections"), EditorStyles.miniButtonRight);
			GUILayout.EndHorizontal();


			Separator();
			Header("Advanced");

			graph.relevantGraphSurfaceMode = (RecastGraph.RelevantGraphSurfaceMode)EditorGUILayout.EnumPopup(new GUIContent("Relevant Graph Surface Mode",
				"Require every region to have a RelevantGraphSurface component inside it.\n" +
				"A RelevantGraphSurface component placed in the scene specifies that\n" +
				"the navmesh region it is inside should be included in the navmesh.\n\n" +
				"If this is set to OnlyForCompletelyInsideTile\n" +
				"a navmesh region is included in the navmesh if it\n" +
				"has a RelevantGraphSurface inside it, or if it\n" +
				"is adjacent to a tile border. This can leave some small regions\n" +
				"which you didn't want to have included because they are adjacent\n" +
				"to tile borders, but it removes the need to place a component\n" +
				"in every single tile, which can be tedious (see below).\n\n" +
				"If this is set to RequireForAll\n" +
				"a navmesh region is included only if it has a RelevantGraphSurface\n" +
				"inside it. Note that even though the navmesh\n" +
				"looks continous between tiles, the tiles are computed individually\n" +
				"and therefore you need a RelevantGraphSurface component for each\n" +
				"region and for each tile."),
				graph.relevantGraphSurfaceMode);

			#pragma warning disable 618
			if (graph.nearestSearchOnlyXZ) {
				graph.nearestSearchOnlyXZ = EditorGUILayout.Toggle(new GUIContent("Nearest node queries in XZ space",
					"Recomended for single-layered environments.\nFaster but can be inacurate esp. in multilayered contexts."), graph.nearestSearchOnlyXZ);

				EditorGUILayout.HelpBox("The global toggle for node queries in XZ space has been deprecated. Use the NNConstraint settings instead.", MessageType.Warning);
			}
			#pragma warning restore 618

			if (GUILayout.Button("Export to .obj file")) {
				editor.RunTask(() => ExportToFile(graph));
			}
		}

		static readonly Vector3[] handlePoints = new [] { new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector3(0, -1, 0) };

		public override void OnSceneGUI (NavGraph target) {
			var graph = target as RecastGraph;

			Handles.matrix = Matrix4x4.identity;
			Handles.color = AstarColor.BoundsHandles;
			Handles.CapFunction cap = Handles.CylinderHandleCap;

			var center = graph.forcedBoundsCenter;
			Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.Euler(graph.rotation), graph.forcedBoundsSize * 0.5f);

			if (Tools.current == Tool.Scale) {
				const float HandleScale = 0.1f;

				Vector3 mn = Vector3.zero;
				Vector3 mx = Vector3.zero;
				EditorGUI.BeginChangeCheck();
				for (int i = 0; i < handlePoints.Length; i++) {
					var ps = matrix.MultiplyPoint3x4(handlePoints[i]);
					Vector3 p = matrix.inverse.MultiplyPoint3x4(Handles.Slider(ps, ps - center, HandleScale*HandleUtility.GetHandleSize(ps), cap, 0));

					if (i == 0) {
						mn = mx = p;
					} else {
						mn = Vector3.Min(mn, p);
						mx = Vector3.Max(mx, p);
					}
				}

				if (EditorGUI.EndChangeCheck()) {
					graph.forcedBoundsCenter = matrix.MultiplyPoint3x4((mn + mx) * 0.5f);
					graph.forcedBoundsSize = Vector3.Scale(graph.forcedBoundsSize, (mx - mn) * 0.5f);
				}
			} else if (Tools.current == Tool.Move) {
				EditorGUI.BeginChangeCheck();
				center = Handles.PositionHandle(center, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : Quaternion.Euler(graph.rotation));

				if (EditorGUI.EndChangeCheck() && Tools.viewTool != ViewTool.Orbit) {
					graph.forcedBoundsCenter = center;
				}
			} else if (Tools.current == Tool.Rotate) {
				EditorGUI.BeginChangeCheck();
				var rot = Handles.RotationHandle(Quaternion.Euler(graph.rotation), graph.forcedBoundsCenter);

				if (EditorGUI.EndChangeCheck() && Tools.viewTool != ViewTool.Orbit) {
					graph.rotation = rot.eulerAngles;
				}
			}
		}

		/// <summary>Exports the INavmesh graph to a .obj file</summary>
		public static void ExportToFile (NavmeshBase target) {
			if (target == null) return;

			NavmeshTile[] tiles = target.GetTiles();

			if (tiles == null) {
				if (EditorUtility.DisplayDialog("Scan graph before exporting?", "The graph does not contain any mesh data. Do you want to scan it?", "Ok", "Cancel")) {
					AstarPathEditor.MenuScan();
					tiles = target.GetTiles();
					if (tiles == null) return;
				} else {
					return;
				}
			}

			string path = EditorUtility.SaveFilePanel("Export .obj", "", "navmesh.obj", "obj");
			if (path == "") return;

			//Generate .obj
			var sb = new System.Text.StringBuilder();

			string name = System.IO.Path.GetFileNameWithoutExtension(path);

			sb.Append("g ").Append(name).AppendLine();

			//Vertices start from 1
			int vCount = 1;

			//Define single texture coordinate to zero
			sb.Append("vt 0 0\n");

			for (int t = 0; t < tiles.Length; t++) {
				NavmeshTile tile = tiles[t];

				if (tile == null) continue;

				var vertices = tile.verts;

				//Write vertices
				for (int i = 0; i < vertices.Length; i++) {
					var v = (Vector3)vertices[i];
					sb.Append(string.Format("v {0} {1} {2}\n", -v.x, v.y, v.z));
				}

				//Write triangles
				TriangleMeshNode[] nodes = tile.nodes;
				for (int i = 0; i < nodes.Length; i++) {
					TriangleMeshNode node = nodes[i];
					if (node == null) {
						Debug.LogError("Node was null or no TriangleMeshNode. Critical error. Graph type " + target.GetType().Name);
						return;
					}
					if (node.GetVertexArrayIndex(0) < 0 || node.GetVertexArrayIndex(0) >= vertices.Length) throw new System.Exception("ERR");

					sb.Append(string.Format("f {0}/1 {1}/1 {2}/1\n", (node.GetVertexArrayIndex(0) + vCount), (node.GetVertexArrayIndex(1) + vCount), (node.GetVertexArrayIndex(2) + vCount)));
				}

				vCount += vertices.Length;
			}

			string obj = sb.ToString();

			using (var sw = new System.IO.StreamWriter(path)) {
				sw.Write(obj);
			}
		}
	}
}
