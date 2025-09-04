using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Pathfinding.Graphs.Navmesh {
	/// <summary>
	/// Represents a rectangular group of tiles of a recast graph.
	///
	/// This is a portable representation in that it can be serialized to and from a byte array.
	///
	/// <code>
	/// // Scans the first 6x6 chunk of tiles of the recast graph (the IntRect uses inclusive coordinates)
	/// var graph = AstarPath.active.data.recastGraph;
	/// var buildSettings = RecastBuilder.BuildTileMeshes(graph, new TileLayout(graph), new IntRect(0, 0, 5, 5));
	/// var disposeArena = new Pathfinding.Jobs.DisposeArena();
	/// var promise = buildSettings.Schedule(disposeArena);
	///
	/// AstarPath.active.AddWorkItem(() => {
	///     // Block until the asynchronous job completes
	///     var result = promise.Complete();
	///     TileMeshes tiles = result.tileMeshes.ToManaged();
	///     // Take the scanned tiles and place them in the graph,
	///     // but not at their original location, but 2 tiles away, rotated 90 degrees.
	///     tiles.tileRect = tiles.tileRect.Offset(new Vector2Int(2, 0));
	///     tiles.Rotate(1);
	///     graph.ReplaceTiles(tiles);
	///
	///     // Dispose unmanaged data
	///     disposeArena.DisposeAll();
	///     result.Dispose();
	/// });
	/// </code>
	///
	/// See: <see cref="NavmeshPrefab"/> uses this representation internally for storage.
	/// See: <see cref="RecastGraph.ReplaceTiles"/>
	/// See: <see cref="RecastBuilder.BuildTileMeshes"/>
	/// </summary>
	public struct TileMeshes {
		/// <summary>Tiles laid out row by row</summary>
		public TileMesh[] tileMeshes;
		/// <summary>Which tiles in the graph this group of tiles represents</summary>
		public IntRect tileRect;
		/// <summary>World-space size of each tile</summary>
		public Vector2 tileWorldSize;

		/// <summary>Rotate this group of tiles by 90*N degrees clockwise about the group's center</summary>
		public void Rotate (int rotation) {
			rotation = -rotation;
			// Get the positive remainder modulo 4. I.e. a number between 0 and 3.
			rotation = ((rotation % 4) + 4) % 4;
			if (rotation == 0) return;
			var rot90 = new int2x2(0, -1, 1, 0);
			var rotN = int2x2.identity;
			for (int i = 0; i < rotation; i++) rotN = math.mul(rotN, rot90);

			var tileSize = (Int3) new Vector3(tileWorldSize.x, 0, tileWorldSize.y);
			var offset = -math.min(int2.zero, math.mul(rotN, new int2(tileSize.x, tileSize.z)));
			var size = new int2(tileRect.Width, tileRect.Height);
			var offsetTileCoordinate = -math.min(int2.zero, math.mul(rotN, size - 1));
			var newTileMeshes = new TileMesh[tileMeshes.Length];
			var newSize = (rotation % 2) == 0 ? size : new int2(size.y, size.x);

			for (int z = 0; z < size.y; z++) {
				for (int x = 0; x < size.x; x++) {
					var vertices = tileMeshes[x + z*size.x].verticesInTileSpace;
					for (int i = 0; i < vertices.Length; i++) {
						var v = vertices[i];
						var rotated = math.mul(rotN, new int2(v.x, v.z)) + offset;
						vertices[i] = new Int3(rotated.x, v.y, rotated.y);
					}

					var tileCoord = math.mul(rotN, new int2(x, z)) + offsetTileCoordinate;
					newTileMeshes[tileCoord.x + tileCoord.y*newSize.x] = tileMeshes[x + z*size.x];
				}
			}

			tileMeshes = newTileMeshes;
			tileWorldSize = rotation % 2 == 0 ? tileWorldSize : new Vector2(tileWorldSize.y, tileWorldSize.x);
			tileRect = new IntRect(tileRect.xmin, tileRect.ymin, tileRect.xmin + newSize.x - 1, tileRect.ymin + newSize.y - 1);
		}

		/// <summary>
		/// Serialize this struct to a portable byte array.
		/// The data is compressed using the deflate algorithm to reduce size.
		/// See: <see cref="Deserialize"/>
		/// </summary>
		public byte[] Serialize () {
			var buffer = new System.IO.MemoryStream();
			var writer = new System.IO.BinaryWriter(new System.IO.Compression.DeflateStream(buffer, System.IO.Compression.CompressionMode.Compress));
			// Version
			writer.Write(0);
			writer.Write(tileRect.Width);
			writer.Write(tileRect.Height);
			writer.Write(this.tileWorldSize.x);
			writer.Write(this.tileWorldSize.y);
			for (int z = 0; z < tileRect.Height; z++) {
				for (int x = 0; x < tileRect.Width; x++) {
					var tile = tileMeshes[(z*tileRect.Width) + x];
					UnityEngine.Assertions.Assert.IsTrue(tile.tags.Length*3 == tile.triangles.Length);
					writer.Write(tile.triangles.Length);
					writer.Write(tile.verticesInTileSpace.Length);
					for (int i = 0; i < tile.verticesInTileSpace.Length; i++) {
						var v = tile.verticesInTileSpace[i];
						writer.Write(v.x);
						writer.Write(v.y);
						writer.Write(v.z);
					}
					for (int i = 0; i < tile.triangles.Length; i++) {
						UnityEngine.Assertions.Assert.IsTrue(tile.triangles[i] >= 0 && tile.triangles[i] < tile.verticesInTileSpace.Length, "Triangle index is out of bounds");
						writer.Write(tile.triangles[i]);
					}
					for (int i = 0; i < tile.tags.Length; i++) writer.Write(tile.tags[i]);
				}
			}
			writer.Close();
			return buffer.ToArray();
		}

		/// <summary>
		/// Deserialize an instance from a byte array.
		/// See: <see cref="Serialize"/>
		/// </summary>
		public static TileMeshes Deserialize (byte[] bytes) {
			var reader = new System.IO.BinaryReader(new System.IO.Compression.DeflateStream(new System.IO.MemoryStream(bytes), System.IO.Compression.CompressionMode.Decompress));
			var version = reader.ReadInt32();
			if (version != 0) throw new System.Exception("Invalid data. Unexpected version number.");
			var w = reader.ReadInt32();
			var h = reader.ReadInt32();
			var tileSize = new Vector2(reader.ReadSingle(), reader.ReadSingle());
			if (w < 0 || h < 0) throw new System.Exception("Invalid bounds");

			var tileRect = new IntRect(0, 0, w - 1, h - 1);

			var tileMeshes = new TileMesh[w*h];
			for (int z = 0; z < h; z++) {
				for (int x = 0; x < w; x++) {
					int[] tris = new int[reader.ReadInt32()];
					Int3[] vertsInTileSpace = new Int3[reader.ReadInt32()];
					uint[] tags = new uint[tris.Length/3];

					for (int i = 0; i < vertsInTileSpace.Length; i++) vertsInTileSpace[i] = new Int3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
					for (int i = 0; i < tris.Length; i++) {
						tris[i] = reader.ReadInt32();
						UnityEngine.Assertions.Assert.IsTrue(tris[i] >= 0 && tris[i] < vertsInTileSpace.Length, "Triangle index is out of bounds");
					}
					for (int i = 0; i < tags.Length; i++) tags[i] = reader.ReadUInt32();

					tileMeshes[x + z*w] = new TileMesh {
						triangles = tris,
						verticesInTileSpace = vertsInTileSpace,
						tags = tags,
					};
				}
			}
			return new TileMeshes {
					   tileMeshes = tileMeshes,
					   tileRect = tileRect,
					   tileWorldSize = tileSize,
			};
		}
	}

	/// <summary>Unsafe representation of a <see cref="TileMeshes"/> struct</summary>
	public struct TileMeshesUnsafe {
		public NativeArray<TileMesh.TileMeshUnsafe> tileMeshes;
		public IntRect tileRect;
		public Vector2 tileWorldSize;

		public TileMeshesUnsafe(NativeArray<TileMesh.TileMeshUnsafe> tileMeshes, IntRect tileRect, Vector2 tileWorldSize) {
			this.tileMeshes = tileMeshes;
			this.tileRect = tileRect;
			this.tileWorldSize = tileWorldSize;
		}

		/// <summary>Copies the native data to managed data arrays which are easier to work with</summary>
		public TileMeshes ToManaged () {
			var output = new TileMesh[tileMeshes.Length];
			for (int i = 0; i < output.Length; i++) {
				output[i] = tileMeshes[i].ToManaged();
			}
			return new TileMeshes {
					   tileMeshes = output,
					   tileRect = this.tileRect,
					   tileWorldSize = this.tileWorldSize,
			};
		}

		public void Dispose (Allocator allocator) {
			// Allows calling Dispose on zero-initialized instances
			if (!tileMeshes.IsCreated) return;

			for (int i = 0; i < tileMeshes.Length; i++) tileMeshes[i].Dispose(allocator);
			tileMeshes.Dispose();
		}
	}
}
