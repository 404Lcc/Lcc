using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Util;
using Pathfinding.Drawing;
using Pathfinding.Pooling;

namespace Pathfinding.Examples {
	/// <summary>
	/// Demos different path types.
	/// This script is an example script demoing a number of different path types included in the project.
	/// Since only the Pro version has access to many path types, it is only included in the pro version
	///
	/// See: Pathfinding.ABPath
	/// See: Pathfinding.MultiTargetPath
	/// See: Pathfinding.ConstantPath
	/// See: Pathfinding.FleePath
	/// See: Pathfinding.RandomPath
	/// See: Pathfinding.FloodPath
	/// See: Pathfinding.FloodPathTracer
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/pathtypesdemo.html")]
	public class PathTypesDemo : MonoBehaviour {
		public DemoMode activeDemo = DemoMode.ABPath;

		public enum DemoMode {
			ABPath,
			MultiTargetPath,
			RandomPath,
			FleePath,
			ConstantPath,
			FloodPath,
			FloodPathTracer
		}

		/// <summary>Start of paths</summary>
		public Transform start;

		/// <summary>Target point of paths</summary>
		public Transform end;

		/// <summary>
		/// Offset from the real path to where it is rendered.
		/// Used to avoid z-fighting
		/// </summary>
		public Vector3 pathOffset;

		/// <summary>Material used for rendering paths</summary>
		public Material lineMat;

		/// <summary>Material used for rendering result of the ConstantPath</summary>
		public Material squareMat;
		public float lineWidth;

		public int searchLength = 1000;
		public int spread = 100;
		public float aimStrength = 0;
		public bool onlyShortestPath = false;

		GameObject constantPathMeshGo;

		Path lastPath = null;
		FloodPath lastFloodPath = null;

		List<GameObject> lastRender = new List<GameObject>();

		List<Vector3> multipoints = new List<Vector3>();

		// Update is called once per frame
		void Update () {
			var mousePos = Input.mousePosition;

			// If the game view is not active, the mouse position can be infinite
			if (!float.IsFinite(mousePos.x)) return;

			var cam = Camera.main;
			if (cam == null) return;
			Ray ray = cam.ScreenPointToRay(mousePos);

			// Find the intersection with the y=0 plane
			Vector3 zeroIntersect = ray.origin + ray.direction * (ray.origin.y / -ray.direction.y);

			end.position = zeroIntersect;

			if (Input.GetMouseButtonUp(0)) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					multipoints.Add(zeroIntersect);
				}

				if (Input.GetKey(KeyCode.LeftControl)) {
					multipoints.Clear();
				}
			}

			if (Input.GetMouseButton(0) && Input.mousePosition.x > 225 && (lastPath == null || lastPath.IsDone())) {
				DemoPath();
			}
		}

		public void AddEndAsMultiTargetPathTarget () {
			multipoints.Add(end.position);
		}

		/// <summary>Draw some helpful gui</summary>
		public void OnGUI () {
			GUILayout.BeginArea(new Rect(5, 5, 220, Screen.height-10), "", "Box");

			switch (activeDemo) {
			case DemoMode.ABPath:
				GUILayout.Label("Basic path. Finds a path from point A to point B."); break;
			case DemoMode.MultiTargetPath:
				GUILayout.Label("Multi Target Path. Finds a path quickly from one point to many others in a single search."); break;
			case DemoMode.RandomPath:
				GUILayout.Label("Randomized Path. Finds a path with a specified length in a random direction or biased towards some point when using a larger aim strenggth."); break;
			case DemoMode.FleePath:
				GUILayout.Label("Flee Path. Tries to flee from a specified point. Remember to set Flee Strength!"); break;
			case DemoMode.ConstantPath:
				GUILayout.Label("Finds all nodes which it costs less than some value to reach."); break;
			case DemoMode.FloodPath:
				GUILayout.Label("Searches the whole graph from a specific point. FloodPathTracer can then be used to quickly find a path to that point"); break;
			case DemoMode.FloodPathTracer:
				GUILayout.Label("Traces a path to where the FloodPath started. Compare the calculation times for this path with ABPath!\nGreat for TD games"); break;
			}

			GUILayout.Space(5);

			GUILayout.Label("Note that the paths are rendered without ANY post-processing applied, so they might look a bit jagged");

			GUILayout.Space(5);

			GUILayout.Label("Click anywhere to recalculate the path. Hold to continuously recalculate the path.");

			if (activeDemo == DemoMode.ConstantPath || activeDemo == DemoMode.RandomPath || activeDemo == DemoMode.FleePath) {
				GUILayout.Label("Search Distance ("+searchLength+")");
				searchLength = Mathf.RoundToInt(GUILayout.HorizontalSlider(searchLength, 0, 100000));
			}

			if (activeDemo == DemoMode.RandomPath || activeDemo == DemoMode.FleePath) {
				GUILayout.Label("Spread ("+spread+")");
				spread = Mathf.RoundToInt(GUILayout.HorizontalSlider(spread, 0, 40000));

				GUILayout.Label((activeDemo == DemoMode.RandomPath ? "Aim strength" : "Flee strength") + " ("+aimStrength+")");
				aimStrength = GUILayout.HorizontalSlider(aimStrength, 0, 1);
			}

			if (activeDemo == DemoMode.MultiTargetPath) {
				GUILayout.Label("Hold shift and click to add new target points. Hold ctr and click to remove all target points");
				onlyShortestPath = GUILayout.Toggle(onlyShortestPath, "Only Shortest Path");
			}

			if (GUILayout.Button("A to B path")) activeDemo = DemoMode.ABPath;
			if (GUILayout.Button("Multi Target Path")) activeDemo = DemoMode.MultiTargetPath;
			if (GUILayout.Button("Random Path")) activeDemo = DemoMode.RandomPath;
			if (GUILayout.Button("Flee path")) activeDemo = DemoMode.FleePath;
			if (GUILayout.Button("Constant Path")) activeDemo = DemoMode.ConstantPath;
			if (GUILayout.Button("Flood Path")) activeDemo = DemoMode.FloodPath;
			if (GUILayout.Button("Flood Path Tracer")) activeDemo = DemoMode.FloodPathTracer;

			GUILayout.EndArea();
		}

		/// <summary>Will be called when the paths have been calculated</summary>
		public void OnPathComplete (Path p) {
			// To prevent it from creating new GameObjects when the application is quitting when using multithreading.
			if (lastRender == null) return;

			ClearPrevious();

			if (p.error) return;

			GameObject ob = new GameObject("LineRenderer", typeof(LineRenderer));
			LineRenderer line = ob.GetComponent<LineRenderer>();
			line.sharedMaterial = lineMat;

			line.startWidth = lineWidth;
			line.endWidth = lineWidth;
			line.positionCount = p.vectorPath.Count;

			for (int i = 0; i < p.vectorPath.Count; i++) {
				line.SetPosition(i, p.vectorPath[i] + pathOffset);
			}

			lastRender.Add(ob);
		}

		/// <summary>Destroys all previous render objects</summary>
		void ClearPrevious () {
			for (int i = 0; i < lastRender.Count; i++) {
				Destroy(lastRender[i]);
			}
			if (constantPathMeshGo != null) constantPathMeshGo.SetActive(false);
			lastRender.Clear();
		}

		/// <summary>Clears renders when the object is destroyed</summary>
		void OnDestroy () {
			ClearPrevious();
			lastRender = null;
		}

		/// <summary>Starts a path specified by PathTypesDemo.activeDemo</summary>
		public void DemoPath () {
			Path p = null;

			switch (activeDemo) {
			case DemoMode.ABPath:
				p = ABPath.Construct(start.position, end.position, OnPathComplete);
				break;
			case DemoMode.MultiTargetPath:
				DemoMultiTargetPath();
				break;
			case DemoMode.ConstantPath:
				DemoConstantPath();
				break;
			case DemoMode.RandomPath:
				RandomPath rp = RandomPath.Construct(start.position, searchLength, OnPathComplete);
				rp.spread = spread;
				rp.aimStrength = aimStrength;
				rp.aim = end.position;

				p = rp;
				break;
			case DemoMode.FleePath:
				FleePath fp = FleePath.Construct(start.position, end.position, searchLength, OnPathComplete);
				fp.aimStrength = aimStrength;
				fp.spread = spread;

				p = fp;
				break;
			case DemoMode.FloodPath:
				p = lastFloodPath = FloodPath.Construct(end.position, null);
				break;
			case DemoMode.FloodPathTracer:
				if (lastFloodPath != null) {
					FloodPathTracer fpt = FloodPathTracer.Construct(end.position, lastFloodPath, OnPathComplete);
					p = fpt;
				}
				break;
			}

			if (p != null) {
				AstarPath.StartPath(p);
				lastPath = p;
			}
		}

		public void DemoMultiTargetPath () {
			MultiTargetPath mp = MultiTargetPath.Construct(multipoints.ToArray(), end.position, null, null);
			mp.pathsForAll = !onlyShortestPath;

			lastPath = mp;
			AstarPath.StartPath(mp);
			mp.BlockUntilCalculated();

			List<GameObject> unused = new List<GameObject>(lastRender);
			lastRender.Clear();

			if (mp.vectorPaths != null) {
				for (int i = 0; i < mp.vectorPaths.Length; i++) {
					if (mp.vectorPaths[i] == null) continue;

					List<Vector3> vpath = mp.vectorPaths[i];

					GameObject ob = null;
					if (unused.Count > i && unused[i].GetComponent<LineRenderer>() != null) {
						ob = unused[i];
						unused.RemoveAt(i);
					} else {
						ob = new GameObject("LineRenderer_"+i, typeof(LineRenderer));
					}

					LineRenderer lr = ob.GetComponent<LineRenderer>();
					lr.sharedMaterial = lineMat;
					lr.startWidth = lineWidth;
					lr.endWidth = lineWidth;
					lr.positionCount = vpath.Count;

					for (int j = 0; j < vpath.Count; j++) {
						lr.SetPosition(j, vpath[j] + pathOffset);
					}

					lastRender.Add(ob);
				}
			}

			for (int i = 0; i < unused.Count; i++) {
				Destroy(unused[i]);
			}
		}


		public void DemoConstantPath () {
			ConstantPath constPath = ConstantPath.Construct(end.position, searchLength, null);
			constPath.Claim(this);

			AstarPath.StartPath(constPath);
			lastPath = constPath;
			// Wait for the path to be calculated
			constPath.BlockUntilCalculated();

			ClearPrevious();

			if (constantPathMeshGo == null) {
				constantPathMeshGo = new GameObject("Mesh", typeof(MeshRenderer), typeof(MeshFilter));
				MeshRenderer re = constantPathMeshGo.GetComponent<MeshRenderer>();
				re.material = squareMat;
			}
			constantPathMeshGo.SetActive(true);
			var meshFilter = constantPathMeshGo.GetComponent<MeshFilter>();
			Mesh mesh;
			if (meshFilter.sharedMesh == null) {
				mesh = meshFilter.sharedMesh = new Mesh();
				// Allow rendering more than 16k nodes
				mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
				mesh.MarkDynamic();
			} else {
				mesh = meshFilter.sharedMesh;
			}
			mesh.Clear();

			// The following code will build a mesh with a square for each node visited
			List<GraphNode> nodes = constPath.allNodes;

			int vertexCount = nodes.Count*4;

			// Get an array with at least vertexCount elements from an object pool
			Vector3[] verts = ArrayPool<Vector3>.Claim(vertexCount);

			// This will loop through the nodes from nearest to furthest
			for (int i = 0; i < nodes.Count; i++) {
				Vector3 pos = (Vector3)nodes[i].position+pathOffset;

				GridGraph gg = AstarData.GetGraph(nodes[i]) as GridGraph;
				float scale = 1F;

				if (gg != null) scale = gg.nodeSize;

				// Add vertices in a square
				verts[i*4+0] = pos+new Vector3(-0.5F, 0, -0.5F)*scale;
				verts[i*4+1] = pos+new Vector3(0.5F, 0, -0.5F)*scale;
				verts[i*4+2] = pos+new Vector3(-0.5F, 0, 0.5F)*scale;
				verts[i*4+3] = pos+new Vector3(0.5F, 0, 0.5F)*scale;
			}

			// Build triangles for the squares
			var indexCount = (3*vertexCount)/2;
			int[] tris = ArrayPool<int>.Claim(indexCount);
			for (int i = 0, j = 0; i < vertexCount; j += 6, i += 4) {
				tris[j+0] = i;
				tris[j+1] = i+1;
				tris[j+2] = i+2;

				tris[j+3] = i+1;
				tris[j+4] = i+3;
				tris[j+5] = i+2;
			}

			Vector2[] uv = ArrayPool<Vector2>.Claim(vertexCount);
			// Set up some basic UV
			for (int i = 0; i < vertexCount; i += 4) {
				uv[i] = new Vector2(0, 0);
				uv[i+1] = new Vector2(1, 0);
				uv[i+2] = new Vector2(0, 1);
				uv[i+3] = new Vector2(1, 1);
			}

			mesh.SetVertices(verts, 0, vertexCount);
			mesh.SetTriangles(tris, 0, indexCount, 0);
			mesh.SetUVs(0, uv, 0, vertexCount);
			mesh.RecalculateNormals();

			constPath.Release(this);
			ArrayPool<int>.Release(ref tris);
			ArrayPool<Vector2>.Release(ref uv);
			ArrayPool<Vector3>.Release(ref verts);
		}
	}
}
