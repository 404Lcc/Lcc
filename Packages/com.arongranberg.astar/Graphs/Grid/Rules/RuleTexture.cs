using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pathfinding.Graphs.Grid.Rules {
	using Pathfinding.Jobs;

	/// <summary>
	/// Modifies nodes based on the contents of a texture.
	///
	/// This can be used to "paint" penalties or walkability using an external program such as Photoshop.
	///
	/// [Open online documentation to see images]
	///
	/// This rule will pick up changes made to the texture during runtime, assuming the <code> Texture.imageContentsHash </code> property is changed.
	/// This is not always done automatically, so you may have to e.g. increment that property manually if you are doing changes to the texture via code.
	/// Any changes will be applied when the graph is scanned, or a graph update is performed.
	///
	/// See: grid-rules (view in online documentation for working links)
	/// </summary>
	[Pathfinding.Util.Preserve]
	public class RuleTexture : GridGraphRule {
		public Texture2D texture;

		public ChannelUse[] channels = new ChannelUse[4];
		public float[] channelScales = { 1000, 1000, 1000, 1000 };

		public ScalingMode scalingMode = ScalingMode.StretchToFitGraph;
		public float nodesPerPixel = 1;

		NativeArray<int> colors;

		public enum ScalingMode {
			FixedScale,
			StretchToFitGraph,
		}

		public override int Hash {
			get {
				var h = base.Hash ^ (texture != null ? (31 * texture.GetInstanceID()) ^ (int)texture.updateCount : 0);
#if UNITY_EDITOR
				if (texture != null) h ^= (int)texture.imageContentsHash.GetHashCode();
#endif
				return h;
			}
		}

		public enum ChannelUse {
			None,
			/// <summary>Penalty goes from 0 to channelScale depending on the channel value</summary>
			Penalty,
			/// <summary>Node Y coordinate goes from 0 to channelScale depending on the channel value</summary>
			Position,
			/// <summary>If channel value is zero the node is made unwalkable, penalty goes from 0 to channelScale depending on the channel value</summary>
			WalkablePenalty,
			/// <summary>If channel value is zero the node is made unwalkable</summary>
			Walkable,
		}

		public override void Register (GridGraphRules rules) {
			if (texture == null) return;

			if (!texture.isReadable) {
				Debug.LogError("Texture for the texture rule on a grid graph is not marked as readable.", texture);
				return;
			}

			if (colors.IsCreated) colors.Dispose();
			colors = new NativeArray<Color32>(texture.GetPixels32(), Allocator.Persistent).Reinterpret<int>();

			// Make sure this is done outside the delegate, just in case the texture is later resized
			var textureSize = new int2(texture.width, texture.height);

			float4 channelPenaltiesCombined = float4.zero;
			bool4 channelDeterminesWalkability = false;
			float4 channelPositionScalesCombined = float4.zero;
			for (int i = 0; i < 4; i++) {
				channelPenaltiesCombined[i] = channels[i] == ChannelUse.Penalty || channels[i] == ChannelUse.WalkablePenalty ? channelScales[i] : 0;
				channelDeterminesWalkability[i] = channels[i] == ChannelUse.Walkable || channels[i] == ChannelUse.WalkablePenalty;
				channelPositionScalesCombined[i] = channels[i] == ChannelUse.Position ? channelScales[i] : 0;
			}

			channelPositionScalesCombined /= 255.0f;
			channelPenaltiesCombined /= 255.0f;

			if (math.any(channelPositionScalesCombined)) {
				rules.AddJobSystemPass(Pass.BeforeCollision, context => {
					new JobTexturePosition {
						colorData = colors,
						nodePositions = context.data.nodes.positions,
						nodeNormals = context.data.nodes.normals,
						bounds = context.data.nodes.bounds,
						colorDataSize = textureSize,
						scale = scalingMode == ScalingMode.FixedScale ? 1.0f/math.max(0.01f, nodesPerPixel) : textureSize / new float2(context.graph.width, context.graph.depth),
						channelPositionScale = channelPositionScalesCombined,
						graphToWorld = context.data.transform.matrix,
					}.Schedule(context.tracker);
				});
			}

			rules.AddJobSystemPass(Pass.BeforeConnections, context => {
				new JobTexturePenalty {
					colorData = colors,
					penalty = context.data.nodes.penalties,
					walkable = context.data.nodes.walkable,
					nodeNormals = context.data.nodes.normals,
					bounds = context.data.nodes.bounds,
					colorDataSize = textureSize,
					scale = scalingMode == ScalingMode.FixedScale ? 1.0f/math.max(0.01f, nodesPerPixel) : textureSize / new float2(context.graph.width, context.graph.depth),
					channelPenalties = channelPenaltiesCombined,
					channelDeterminesWalkability = channelDeterminesWalkability,
				}.Schedule(context.tracker);
			});
		}

		public override void DisposeUnmanagedData () {
			if (colors.IsCreated) colors.Dispose();
		}

		[BurstCompile]
		public struct JobTexturePosition : IJob, GridIterationUtilities.INodeModifier {
			[ReadOnly]
			public NativeArray<int> colorData;
			[WriteOnly]
			public NativeArray<Vector3> nodePositions;
			[ReadOnly]
			public NativeArray<float4> nodeNormals;

			public Matrix4x4 graphToWorld;
			public IntBounds bounds;
			public int2 colorDataSize;
			public float2 scale;
			public float4 channelPositionScale;

			public void ModifyNode (int dataIndex, int dataX, int dataLayer, int dataZ) {
				var offset = bounds.min.xz;
				int2 colorPos = math.clamp((int2)math.round((new float2(dataX, dataZ) + offset) * scale), int2.zero, colorDataSize - new int2(1, 1));
				int colorIndex = colorPos.y*colorDataSize.x + colorPos.x;

				int4 color = new int4((colorData[colorIndex] >> 0) & 0xFF, (colorData[colorIndex] >> 8) & 0xFF, (colorData[colorIndex] >> 16) & 0xFF, (colorData[colorIndex] >> 24) & 0xFF);

				float y = math.dot(channelPositionScale, color);

				nodePositions[dataIndex] = graphToWorld.MultiplyPoint3x4(new Vector3((bounds.min.x + dataX) + 0.5f, y, (bounds.min.z + dataZ) + 0.5f));
			}

			public void Execute () {
				GridIterationUtilities.ForEachNode(bounds.size, nodeNormals, ref this);
			}
		}

		[BurstCompile]
		public struct JobTexturePenalty : IJob, GridIterationUtilities.INodeModifier {
			[ReadOnly]
			public NativeArray<int> colorData;
			public NativeArray<uint> penalty;
			public NativeArray<bool> walkable;
			[ReadOnly]
			public NativeArray<float4> nodeNormals;

			public IntBounds bounds;
			public int2 colorDataSize;
			public float2 scale;
			public float4 channelPenalties;
			public bool4 channelDeterminesWalkability;

			public void ModifyNode (int dataIndex, int dataX, int dataLayer, int dataZ) {
				var offset = bounds.min.xz;
				int2 colorPos = math.clamp((int2)math.round((new float2(dataX, dataZ) + offset) * scale), int2.zero, colorDataSize - new int2(1, 1));
				int colorIndex = colorPos.y*colorDataSize.x + colorPos.x;

				int4 color = new int4((colorData[colorIndex] >> 0) & 0xFF, (colorData[colorIndex] >> 8) & 0xFF, (colorData[colorIndex] >> 16) & 0xFF, (colorData[colorIndex] >> 24) & 0xFF);

				penalty[dataIndex] += (uint)math.dot(channelPenalties, color);
				walkable[dataIndex] = walkable[dataIndex] & !math.any(channelDeterminesWalkability & (color == 0));
			}

			public void Execute () {
				GridIterationUtilities.ForEachNode(bounds.size, nodeNormals, ref this);
			}
		}
	}
}
