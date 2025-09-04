#pragma warning disable 0282 // Allows the 'partial' keyword without warnings
using UnityEngine;
using System.Collections.Generic;
#if MODULE_ENTITIES
using Pathfinding.RVO;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Rendering;
using Unity.Entities;
using Unity.Transforms;

namespace Pathfinding.Examples {
	using Pathfinding.ECS;
	using Pathfinding.ECS.RVO;
	using Pathfinding.Util;

	/// <summary>
	/// Lightweight example script for simulating rvo agents.
	///
	/// This script, compared to using lots of RVOController components shows the real power of the RVO simulator when
	/// little other overhead (e.g GameObjects and pathfinding) is present.
	///
	/// With this script, I can simulate 30 000 agents at over 60 fps on my, admittedly quite beefy, machine (in a standalone build, with the local avoidance simulation running at a fixed 60 fps and using up to 14 cores of my machine).
	/// This is significantly more than one can simulate when using GameObjects for each agent.
	///
	/// This script will render the agents by generating a square for each agent combined into a single mesh with appropriate UV.
	///
	/// A few GUI buttons will be drawn by this script with which the user can change the number of agents.
	///
	/// [Open online documentation to see images]
	///
	/// Video: https://www.youtube.com/watch?v=wxzrHRIiVyk
	///
	/// See: local-avoidance (view in online documentation for working links)
	/// </summary>
	public partial class LightweightRVO : MonoBehaviour {
		/// <summary>Number of agents created at start</summary>
		public int agentCount = 100;

		/// <summary>How large is the area in which the agents are distributed when starting the simulation</summary>
		public float exampleScale = 100;


		public enum RVOExampleType {
			Circle,
			Line,
			Point,
			RandomStreams,
			Crossing
		}

		/// <summary>How the agents are distributed when starting the simulation</summary>
		public RVOExampleType type = RVOExampleType.Circle;

		/// <summary>Agent radius</summary>
		public float radius = 3;

		/// <summary>Max speed for an agent</summary>
		public float maxSpeed = 2;

		/// <summary>How far in the future too look for agents</summary>
		public float agentTimeHorizon = 10;

		[HideInInspector]
		/// <summary>How far in the future too look for obstacles</summary>
		public float obstacleTimeHorizon = 10;

		/// <summary>Max number of neighbour agents to take into account</summary>
		public int maxNeighbours = 10;

		/// <summary>
		/// Offset from the agent position the actual drawn postition.
		/// Used to get rid of z-buffer issues
		/// </summary>
		public Vector3 renderingOffset = Vector3.up*0.1f;

		/// <summary>Bitmas of debugging options to enable for the agents</summary>
		public AgentDebugFlags debug;
		public Material material;

		public void Start () {
			CreateAgents(agentCount);

			// Create the systems and add them to their respective simulation groups
			// Normally this is handled automatically by Unity, but we use the [DisableAutoCreation] attribute
			// since these systems are only used in an example scene.
			var world = World.DefaultGameObjectInjectionWorld;
			var simulationGroup = world.GetOrCreateSystemManaged<AIMovementSystemGroup>();
			simulationGroup.AddSystemToUpdateList(world.CreateSystem<LightweightRVOControlSystem>());
			simulationGroup.AddSystemToUpdateList(world.CreateSystem<LightweightRVOMoveSystem>());
			var renderSystem = world.AddSystemManaged(new LightweightRVORenderSystem {
				material = material,
				renderingOffset = renderingOffset,
			});
			world.GetOrCreateSystemManaged<PresentationSystemGroup>().AddSystemToUpdateList(renderSystem);

			// Annoyingly, the PresentationSystemGroup is not called when the game is paused.
			// So we need to render the mesh from a different callback, otherwise the mesh would
			// disappear when pausing the game.
			// To add additional complexity, we need different callbacks depending on if
			// we are using the built-in render pipeline or a scriptable render pipeline.

			// Callback when rendering with the built-in render pipeline
			Camera.onPreCull += PreCull;
			// Callback when rendering with a scriptable render pipeline
#if UNITY_2023_3_OR_NEWER
			RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
#else
			RenderPipelineManager.beginFrameRendering += OnBeginFrameRendering;
#endif
		}

#if UNITY_2023_3_OR_NEWER
		void OnBeginContextRendering (ScriptableRenderContext ctx, List<Camera> cameras) {
			for (int i = 0; i < cameras.Count; i++) PreCull(cameras[i]);
		}
#else
		void OnBeginFrameRendering (ScriptableRenderContext ctx, Camera[] cameras) {
			for (int i = 0; i < cameras.Length; i++) PreCull(cameras[i]);
		}
#endif

		void PreCull (Camera camera) {
			var world = World.DefaultGameObjectInjectionWorld;
			var mesh = world.GetOrCreateSystemManaged<LightweightRVORenderSystem>().mesh;

			// Render the mesh in the game
			Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0, camera);
		}

		void OnDestroy () {
#if UNITY_2023_3_OR_NEWER
			RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
#else
			RenderPipelineManager.beginFrameRendering -= OnBeginFrameRendering;
#endif
			Camera.onPreCull -= PreCull;
		}

		public void OnGUI () {
			if (GUILayout.Button("2")) CreateAgents(2);
			if (GUILayout.Button("10")) CreateAgents(10);
			if (GUILayout.Button("100")) CreateAgents(100);
			if (GUILayout.Button("500")) CreateAgents(500);
			if (GUILayout.Button("1000")) CreateAgents(1000);
			if (GUILayout.Button("5000")) CreateAgents(5000);
			if (GUILayout.Button("10000")) CreateAgents(10000);
			if (GUILayout.Button("20000")) CreateAgents(20000);
			if (GUILayout.Button("30000")) CreateAgents(30000);

			GUILayout.Space(5);

			if (GUILayout.Button("Random Streams")) {
				type = RVOExampleType.RandomStreams;
				CreateAgents(agentCount);
			}

			if (GUILayout.Button("Line")) {
				type = RVOExampleType.Line;
				CreateAgents(Mathf.Min(agentCount, 100));
			}

			if (GUILayout.Button("Circle")) {
				type = RVOExampleType.Circle;
				CreateAgents(agentCount);
			}

			if (GUILayout.Button("Point")) {
				type = RVOExampleType.Point;
				CreateAgents(agentCount);
			}

			if (GUILayout.Button("Crossing")) {
				type = RVOExampleType.Crossing;
				CreateAgents(agentCount);
			}
		}

		public void Update () {
			var world = World.DefaultGameObjectInjectionWorld;
			var system = world.GetOrCreateSystem<LightweightRVOControlSystem>();
			world.Unmanaged.GetUnsafeSystemRef<LightweightRVOControlSystem>(system).debug = debug;
		}

		private float uniformDistance (float radius) {
			float v = UnityEngine.Random.value + UnityEngine.Random.value;

			if (v > 1) return radius * (2-v);
			else return radius * v;
		}

		/// <summary>Some agent data used in the lightweight rvo example scene</summary>
		public struct LightweightAgentData : IComponentData {
			public Color32 color;
			public float maxSpeed;
		}

		/// <summary>Create a single agent entity</summary>
		Entity CreateAgent (EntityArchetype archetype, EntityCommandBuffer buffer, Vector3 position, Vector3 destination, Color color, float priority = 0.5f) {
			var entity = buffer.CreateEntity(archetype);
			buffer.AddComponent<LocalTransform>(entity, LocalTransform.FromPosition(position));
			buffer.AddComponent<DestinationPoint>(entity, new DestinationPoint { destination = destination });
			buffer.AddComponent<RVOAgent>(entity, new RVOAgent {
				agentTimeHorizon = agentTimeHorizon,
				obstacleTimeHorizon = obstacleTimeHorizon,
				maxNeighbours = maxNeighbours,
				layer = RVOLayer.DefaultAgent,
				collidesWith = (RVOLayer)(-1),
				priority = priority,
				priorityMultiplier = 1,
				flowFollowingStrength = 0,
				debug = AgentDebugFlags.Nothing,
				locked = false
			});
			buffer.AddComponent<AgentMovementPlane>(entity, new AgentMovementPlane { value = new NativeMovementPlane(quaternion.identity) });
			buffer.AddComponent<AgentCylinderShape>(entity, new AgentCylinderShape { radius = radius, height = 1.0f });
			buffer.AddComponent<LightweightAgentData>(entity, new LightweightAgentData {
				color = (Color32)color,
				maxSpeed = maxSpeed
			});
			return entity;
		}

		/// <summary>Create a number of agents in circle and restart simulation</summary>
		public void CreateAgents (int num) {
			this.agentCount = num;
			var world = World.DefaultGameObjectInjectionWorld;
			var entityManager = world.EntityManager;
			var archetype = entityManager.CreateArchetype(
				typeof(LocalTransform),
				typeof(LocalToWorld),
				typeof(AgentCylinderShape),
				typeof(ResolvedMovement),
				typeof(DestinationPoint),
				typeof(MovementControl),
				typeof(RVOAgent),
				typeof(AgentMovementPlane),
				typeof(LightweightAgentData),
				typeof(SimulateMovement),
				typeof(SimulateMovementRepair),
				typeof(SimulateMovementControl),
				typeof(SimulateMovementFinalize)
				);

			var buffer = new EntityCommandBuffer(Allocator.Temp);
			var existingEntities = entityManager.CreateEntityQuery(typeof(LightweightAgentData));

#if MODULE_ENTITIES_1_0_8_OR_NEWER
			buffer.DestroyEntity(existingEntities, EntityQueryCaptureMode.AtPlayback);
#else
			buffer.DestroyEntity(existingEntities);
#endif

			if (type == RVOExampleType.Circle) {
				float agentArea = agentCount * radius * radius * Mathf.PI;
				const float EmptyFraction = 0.7f;
				const float PackingDensity = 0.9f;
				float innerCircleRadius = Mathf.Sqrt(agentArea/(Mathf.PI*(1-EmptyFraction*EmptyFraction)));
				float outerCircleRadius = Mathf.Sqrt(innerCircleRadius*innerCircleRadius + agentCount*radius*radius/PackingDensity);

				for (int i = 0; i < agentCount; i++) {
					Vector3 pos = new Vector3(Mathf.Cos(i * Mathf.PI * 2.0f / agentCount), 0, Mathf.Sin(i * Mathf.PI * 2.0f / agentCount)) * math.lerp(innerCircleRadius, outerCircleRadius, UnityEngine.Random.value);
					var destination = new float3(-pos.x, 0, -pos.z);
					var color = AstarMath.HSVToRGB(i * 360.0f / agentCount, 0.8f, 0.6f);
					CreateAgent(archetype, buffer, pos, destination, color);
				}
			} else if (type == RVOExampleType.Line) {
				for (int i = 0; i < agentCount; i++) {
					Vector3 pos = new Vector3((i % 2 == 0 ? 1 : -1) * exampleScale, 0, (i / 2) * radius * 2.5f);
					CreateAgent(archetype, buffer, pos, new float3(-pos.x, 0, pos.z), i % 2 == 0 ? Color.red : Color.blue);
				}
			} else if (type == RVOExampleType.Point) {
				for (int i = 0; i < agentCount; i++) {
					Vector3 pos = new Vector3(Mathf.Cos(i * Mathf.PI * 2.0f / agentCount), 0, Mathf.Sin(i * Mathf.PI * 2.0f / agentCount)) * exampleScale;
					CreateAgent(archetype, buffer, pos, new float3(0, 0, 0), AstarMath.HSVToRGB(i * 360.0f / agentCount, 0.8f, 0.6f));
				}
			} else if (type == RVOExampleType.RandomStreams) {
				float circleRad = Mathf.Sqrt(agentCount * radius * radius * 4 / Mathf.PI) * exampleScale * 0.05f;

				for (int i = 0; i < agentCount; i++) {
					float angle = UnityEngine.Random.value * Mathf.PI * 2.0f;
					float targetAngle = UnityEngine.Random.value * Mathf.PI * 2.0f;
					Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * uniformDistance(circleRad);
					var destination = new float3(Mathf.Cos(targetAngle), 0, Mathf.Sin(targetAngle)) * uniformDistance(circleRad);
					var color = AstarMath.HSVToRGB(targetAngle * Mathf.Rad2Deg, 0.8f, 0.6f);
					CreateAgent(archetype, buffer, pos, destination, color);
				}
			} else if (type == RVOExampleType.Crossing) {
				float distanceBetweenGroups = exampleScale * radius * 0.5f;
				int directions = (int)Mathf.Sqrt(agentCount / 25f);
				directions = Mathf.Max(directions, 2);

				const int AgentsPerDistance = 10;
				for (int i = 0; i < agentCount; i++) {
					float angle = ((i % directions)/(float)directions) * Mathf.PI * 2.0f;
					var dist = distanceBetweenGroups * ((i/(directions*AgentsPerDistance) + 1) + 0.3f*UnityEngine.Random.value);
					Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * dist;
					var destination = math.normalizesafe(new float3(-pos.x, 0, -pos.z)) * distanceBetweenGroups * 3;
					var color = AstarMath.HSVToRGB(angle * Mathf.Rad2Deg, 0.8f, 0.6f);
					CreateAgent(archetype, buffer, pos, destination, color, priority: (i % directions) == 0 ? 1 : 0.01f);
				}
			}

			buffer.Playback(entityManager);
		}

		/// <summary>Lightweight example system for moving agents</summary>
		[UpdateAfter(typeof(RVOSystem))]
		[UpdateInGroup(typeof(AIMovementSystemGroup))]
		[DisableAutoCreation]
		public partial struct LightweightRVOMoveSystem : ISystem {
			EntityQuery entityQuery;

			public void OnCreate (ref SystemState state) {
				entityQuery = state.GetEntityQuery(
					ComponentType.ReadWrite<LocalTransform>(),
					ComponentType.ReadOnly<AgentMovementPlane>(),
					ComponentType.ReadOnly<ResolvedMovement>()
					);
			}

			public void OnUpdate (ref SystemState state) {
				state.Dependency = new JobMoveAgents { deltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel(entityQuery, state.Dependency);
			}

			[BurstCompile]
			partial struct JobMoveAgents : IJobEntity {
				public float deltaTime;

				public void Execute (ref LocalTransform transform, in AgentMovementPlane movementPlane, in ResolvedMovement resolvedMovement) {
					transform.Position += Pathfinding.ECS.JobMoveAgent.MoveWithoutGravity(ref transform, in resolvedMovement, in movementPlane, deltaTime);
				}
			}
		}

		/// <summary>
		/// Lightweight example system for controlling and rendering RVO agents.
		///
		/// This system is not intended to be used for anything other than the RVO example scene, and perhaps for reference for a curious reader.
		///
		/// It also relies on the <see cref="LightweightRVOMoveSystem"/> and <see cref="RVOSystem"/>.
		/// </summary>
		[UpdateBefore(typeof(RVOSystem))]
		[UpdateInGroup(typeof(AIMovementSystemGroup))]
		[DisableAutoCreation]
		public partial struct LightweightRVOControlSystem : ISystem {
			/// <summary>Determines what kind of debug info the RVO system should render as gizmos</summary>
			public AgentDebugFlags debug;
			EntityQuery entityQueryDirection;
			EntityQuery entityQueryControl;

			public void OnCreate (ref SystemState state) {
				entityQueryDirection = state.GetEntityQuery(
					ComponentType.ReadWrite<LocalTransform>(),
					ComponentType.ReadOnly<AgentCylinderShape>(),
					ComponentType.ReadOnly<AgentMovementPlane>(),
					ComponentType.ReadOnly<MovementControl>(),
					ComponentType.ReadOnly<ResolvedMovement>()
					);
				entityQueryControl = state.GetEntityQuery(
					ComponentType.ReadOnly<LightweightAgentData>(),
					ComponentType.ReadOnly<DestinationPoint>(),
					ComponentType.ReadWrite<RVOAgent>(),
					ComponentType.ReadWrite<MovementControl>()
					);
			}

			public void OnUpdate (ref SystemState state) {
				state.Dependency = new AlignAgentWithMovementDirectionJob {
					deltaTime = SystemAPI.Time.DeltaTime,
					rotationSpeed = 5,
				}.ScheduleParallel(entityQueryDirection, state.Dependency);

				state.Dependency = new JobControlAgents {
					deltaTime = SystemAPI.Time.DeltaTime,
					debug = debug,
				}.Schedule(entityQueryControl, state.Dependency);
			}

			/// <summary>
			/// Job to set the direction each agent wants to move in.
			///
			/// The <see cref="RVOSystem"/> will then try to move the agent in that direction, but taking care to avoid other agents and obstacles.
			/// </summary>
			[BurstCompile]
			public partial struct JobControlAgents : IJobEntity {
				public float deltaTime;
				public AgentDebugFlags debug;

				public void Execute (in LightweightAgentData agentData, in DestinationPoint destination, ref RVOAgent rvoAgent, ref MovementControl movementControl, [EntityIndexInQuery] int index) {
					movementControl = new MovementControl {
						// This is the point the agent will try to move towards
						targetPoint = destination.destination,
						endOfPath = destination.destination,
						speed = agentData.maxSpeed,
						// Allow the agent to move slightly faster than its desired speed if necessary
						maxSpeed = agentData.maxSpeed * 1.1f,
						// We don't have a graph, so this field is not relevant
						hierarchicalNodeIndex = -1,
						targetRotation = 0,
						targetRotationOffset = 0,
						rotationSpeed = 0,
						overrideLocalAvoidance = false,
					};

					if (index == 0) {
						// Show most debug info only for the first agent, to reduce clutter
						rvoAgent.debug = debug;
					} else {
						rvoAgent.debug = debug & AgentDebugFlags.ReachedState;
					}
				}
			}

			/// <summary>Job to update each agent's position and rotation based on its movement direction</summary>
			[BurstCompile(FloatMode = FloatMode.Fast)]
			public partial struct AlignAgentWithMovementDirectionJob : IJobEntity {
				public float deltaTime;
				public float rotationSpeed;

				public void Execute (ref LocalTransform transform, in AgentCylinderShape shape, in AgentMovementPlane movementPlane, in MovementControl movementControl, in ResolvedMovement resolvedMovement) {
					if (resolvedMovement.speed > shape.radius*0.01f) {
						var speedFraction = math.sqrt(math.clamp(resolvedMovement.speed / movementControl.maxSpeed, 0, 1));
						// If the agent is moving, align it with the movement direction
						var actualDirection = movementPlane.value.ToPlane(resolvedMovement.targetPoint - transform.Position);
						var actualAngle = math.atan2(actualDirection.y, actualDirection.x) - math.PI*0.5f;
						var targetRotation = movementPlane.value.ToWorldRotation(actualAngle);
						transform.Rotation = math.slerp(transform.Rotation, targetRotation, deltaTime*speedFraction*rotationSpeed);
					}
				}
			}
		}

		/// <summary>
		/// System to render RVO agents on a mesh.
		///
		/// The system does not do any rendering itself, but only writes to the <see cref="mesh"/> field.
		/// </summary>
		[DisableAutoCreation]
		public partial class LightweightRVORenderSystem : SystemBase {
			/// <summary>Mesh for rendering</summary>
			public Mesh mesh;
			/// <summary>Material for rendering</summary>
			public Material material;
			/// <summary>Offset with which to render the mesh from the agent's original positions</summary>
			public Vector3 renderingOffset;

			EntityQuery entityQuery;

			protected override void OnCreate () {
				mesh = new Mesh {
					name = "RVO Agents",
				};
				entityQuery = GetEntityQuery(
					ComponentType.ReadOnly<LocalTransform>(),
					ComponentType.ReadOnly<LightweightAgentData>(),
					ComponentType.ReadOnly<DestinationPoint>(),
					ComponentType.ReadOnly<RVOAgent>(),
					ComponentType.ReadOnly<AgentCylinderShape>()
					);
			}

			protected override void OnDestroy () {
				Mesh.Destroy(mesh);
			}

			protected override void OnUpdate () {
				var agentCount = entityQuery.CalculateEntityCount();
				var vertexCount = agentCount*4;
				var indexCount = agentCount*6;
				var vertices = CollectionHelper.CreateNativeArray<Vertex>(vertexCount, WorldUpdateAllocator);
				var tris = CollectionHelper.CreateNativeArray<int>(indexCount, WorldUpdateAllocator);
				Dependency = new JobGenerateMesh {
					verts = vertices,
					tris = tris,
					renderingOffset = renderingOffset
				}.Schedule(entityQuery, Dependency);

				// Specify the layout of each vertex. This should match the Vertex struct
				var layout = new[] {
					new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
					new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
					new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
				};
				mesh.SetVertexBufferParams(vertexCount, layout);
				// To allow for more than ≈16k agents we need to use a 32 bit format for the mesh
				mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

				// Wait for the JobGenerateMesh job to complete before we try to use the mesh data
				Dependency.Complete();

				// Set the vertex and index data
				mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
				mesh.SetIndexBufferData(tris, 0, 0, tris.Length);

				mesh.subMeshCount = 1;
				mesh.SetSubMesh(0, new SubMeshDescriptor(0, tris.Length, MeshTopology.Triangles), MeshUpdateFlags.DontRecalculateBounds);
				// SetSubMesh doesn't seem to update the bounds properly for some reason, so we do it manually instead
				mesh.RecalculateBounds();
			}

			[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
			public struct Vertex {
				public float3 position;
				public Color32 color;
				public float2 uv;
			}

			/// <summary>
			/// Generates a simple mesh for rendering the agents.
			/// Each agent is a quad rotated and positioned to align with the agent.
			/// </summary>
			[BurstCompile(FloatMode = FloatMode.Fast)]
			public partial struct JobGenerateMesh : IJobEntity {
				[WriteOnly] public NativeArray<Vertex> verts;
				[WriteOnly] public NativeArray<int> tris;

				public Vector3 renderingOffset;

				public void Execute (in LocalTransform transform, in LightweightAgentData agentData, in AgentCylinderShape shape, [EntityIndexInQuery] int entityIndex) {
					// Create a square with the "forward" direction along the agent's velocity
					float3 forward = transform.Forward() * shape.radius;
					if (math.all(forward == 0)) forward = new float3(0, 0, shape.radius);
					float3 right = math.cross(new float3(0, 1, 0), forward);
					float3 orig = transform.Position + (float3)renderingOffset;

					int vc = 4*entityIndex;
					int tc = 2*3*entityIndex;

					Color32 color = agentData.color;
					verts[vc+0] = new Vertex {
						position = (orig + forward - right),
						uv = new float2(0, 1),
						color = color,
					};

					verts[vc+1] = new Vertex {
						position = (orig + forward + right),
						uv = new float2(1, 1),
						color = color,
					};

					verts[vc+2] = new Vertex {
						position = (orig - forward + right),
						uv = new float2(1, 0),
						color = color,
					};

					verts[vc+3] = new Vertex {
						position = (orig - forward - right),
						uv = new float2(0, 0),
						color = color,
					};

					tris[tc+0] = (vc + 0);
					tris[tc+1] = (vc + 1);
					tris[tc+2] = (vc + 2);

					tris[tc+3] = (vc + 0);
					tris[tc+4] = (vc + 2);
					tris[tc+5] = (vc + 3);
				}
			}
		}
	}
}
#else
namespace Pathfinding.Examples {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/lightweightrvo.html")]
	public class LightweightRVO : MonoBehaviour {
		public void Start () {
			Debug.LogError("Lightweight RVO example script requires the entities package to be installed.");
		}
	}
}
#endif
