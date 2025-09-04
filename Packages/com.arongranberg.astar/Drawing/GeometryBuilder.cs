using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Unity.Burst;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Jobs;

namespace Pathfinding.Drawing {
	using static DrawingData;
	using static CommandBuilder;
	using Pathfinding.Drawing.Text;
	using Unity.Profiling;
	using System.Collections.Generic;
	using UnityEngine.Rendering;

	static class GeometryBuilder {
		public struct CameraInfo {
			public float3 cameraPosition;
			public quaternion cameraRotation;
			public float2 cameraDepthToPixelSize;
			public bool cameraIsOrthographic;

			public CameraInfo(Camera camera) {
				var tr = camera?.transform;
				cameraPosition = tr != null ? (float3)tr.position : float3.zero;
				cameraRotation = tr != null ? (quaternion)tr.rotation : quaternion.identity;
				cameraDepthToPixelSize = (camera != null ? CameraDepthToPixelSize(camera) : 0);
				cameraIsOrthographic = camera != null ? camera.orthographic : false;
			}
		}

		internal static unsafe JobHandle Build (DrawingData gizmos, ProcessedBuilderData.MeshBuffers* buffers, ref CameraInfo cameraInfo, JobHandle dependency) {
			// Create a new builder and schedule it.
			// Why is characterInfo passed as a pointer and a length instead of just a NativeArray?
			// 	This is because passing it as a NativeArray invokes the safety system which adds some tracking to the NativeArray.
			//  This is normally not a problem, but we may be scheduling hundreds of jobs that use that particular NativeArray and this causes a bit of a slowdown
			//  in the safety checking system. Passing it as a pointer + length makes the whole scheduling code about twice as fast compared to passing it as a NativeArray.
			return new GeometryBuilderJob {
					   buffers = buffers,
					   currentMatrix = Matrix4x4.identity,
					   currentLineWidthData = new LineWidthData {
						   pixels = 1,
						   automaticJoins = false,
					   },
					   lineWidthMultiplier = DrawingManager.lineWidthMultiplier,
					   currentColor = (Color32)Color.white,
					   cameraPosition = cameraInfo.cameraPosition,
					   cameraRotation = cameraInfo.cameraRotation,
					   cameraDepthToPixelSize = cameraInfo.cameraDepthToPixelSize,
					   cameraIsOrthographic = cameraInfo.cameraIsOrthographic,
					   characterInfo = (SDFCharacter*)gizmos.fontData.characters.GetUnsafeReadOnlyPtr(),
					   characterInfoLength = gizmos.fontData.characters.Length,
					   maxPixelError = GeometryBuilderJob.MaxCirclePixelError / math.max(0.1f, gizmos.settingsRef.curveResolution),
			}.Schedule(dependency);
		}

		/// <summary>
		/// Helper for determining how large a pixel is at a given depth.
		/// A a distance D from the camera a pixel corresponds to roughly value.x * D + value.y world units.
		/// Where value is the return value from this function.
		/// </summary>
		private static float2 CameraDepthToPixelSize (Camera camera) {
			if (camera.orthographic) {
				return new float2(0.0f, 2.0f * camera.orthographicSize / camera.pixelHeight);
			} else {
				return new float2(Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f) / (0.5f * camera.pixelHeight), 0.0f);
			}
		}

		private static NativeArray<T> ConvertExistingDataToNativeArray<T>(UnsafeAppendBuffer data) where T : struct {
			unsafe {
				var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(data.Ptr, data.Length / UnsafeUtility.SizeOf<T>(), Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
				return arr;
			}
		}

		internal static unsafe void BuildMesh (DrawingData gizmos, List<MeshWithType> meshes, ProcessedBuilderData.MeshBuffers* inputBuffers) {
			if (inputBuffers->triangles.Length > 0) {
				CommandBuilderSamplers.MarkerUpdateBuffer.Begin();
				var mesh = AssignMeshData<GeometryBuilderJob.Vertex>(gizmos, inputBuffers->bounds, inputBuffers->vertices, inputBuffers->triangles, MeshLayouts.MeshLayout);
				meshes.Add(new MeshWithType { mesh = mesh, type = MeshType.Lines });
				CommandBuilderSamplers.MarkerUpdateBuffer.End();
			}

			if (inputBuffers->solidTriangles.Length > 0) {
				var mesh = AssignMeshData<GeometryBuilderJob.Vertex>(gizmos, inputBuffers->bounds, inputBuffers->solidVertices, inputBuffers->solidTriangles, MeshLayouts.MeshLayout);
				meshes.Add(new MeshWithType { mesh = mesh, type = MeshType.Solid });
			}

			if (inputBuffers->textTriangles.Length > 0) {
				var mesh = AssignMeshData<GeometryBuilderJob.TextVertex>(gizmos, inputBuffers->bounds, inputBuffers->textVertices, inputBuffers->textTriangles, MeshLayouts.MeshLayoutText);
				meshes.Add(new MeshWithType { mesh = mesh, type = MeshType.Text });
			}
		}

		private static Mesh AssignMeshData<VertexType>(DrawingData gizmos, Bounds bounds, UnsafeAppendBuffer vertices, UnsafeAppendBuffer triangles, VertexAttributeDescriptor[] layout) where VertexType : struct {
			CommandBuilderSamplers.MarkerConvert.Begin();
			var verticesView = ConvertExistingDataToNativeArray<VertexType>(vertices);
			var trianglesView = ConvertExistingDataToNativeArray<int>(triangles);
			CommandBuilderSamplers.MarkerConvert.End();
			var mesh = gizmos.GetMesh(verticesView.Length);

			CommandBuilderSamplers.MarkerSetLayout.Begin();
			// Resize the vertex buffer if necessary
			// Note: also resized if the vertex buffer is significantly larger than necessary.
			//       This is because apparently when executing the command buffer Unity does something with the whole buffer for some reason (shows up as Mesh.CreateMesh in the profiler)
			// TODO: This could potentially cause bad behaviour if multiple meshes are used each frame and they have differing sizes.
			// We should query for meshes that already have an appropriately sized buffer.
			// if (mesh.vertexCount < verticesView.Length || mesh.vertexCount > verticesView.Length * 2) {

			// }
			// TODO: Use Mesh.GetVertexBuffer/Mesh.GetIndexBuffer once they stop being buggy.
			// Currently they don't seem to get refreshed properly after resizing them (2022.2.0b1)
			mesh.SetVertexBufferParams(math.ceilpow2(verticesView.Length), layout);
			mesh.SetIndexBufferParams(math.ceilpow2(trianglesView.Length), IndexFormat.UInt32);
			CommandBuilderSamplers.MarkerSetLayout.End();

			CommandBuilderSamplers.MarkerUpdateVertices.Begin();
			// Update the mesh data
			mesh.SetVertexBufferData(verticesView, 0, 0, verticesView.Length);
			CommandBuilderSamplers.MarkerUpdateVertices.End();
			CommandBuilderSamplers.MarkerUpdateIndices.Begin();
			// Update the index buffer and assume all our indices are correct
			mesh.SetIndexBufferData(trianglesView, 0, 0, trianglesView.Length, MeshUpdateFlags.DontValidateIndices);
			CommandBuilderSamplers.MarkerUpdateIndices.End();


			CommandBuilderSamplers.MarkerSubmesh.Begin();
			mesh.subMeshCount = 1;
			var submesh = new SubMeshDescriptor(0, trianglesView.Length, MeshTopology.Triangles) {
				vertexCount = verticesView.Length,
				bounds = bounds
			};
			mesh.SetSubMesh(0, submesh, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);
			mesh.bounds = bounds;
			CommandBuilderSamplers.MarkerSubmesh.End();
			return mesh;
		}
	}

	/// <summary>Some static fields that need to be in a separate class because Burst doesn't support them</summary>
	static class MeshLayouts {
		internal static readonly VertexAttributeDescriptor[] MeshLayout = {
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
		};

		internal static readonly VertexAttributeDescriptor[] MeshLayoutText = {
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
		};
	}

	/// <summary>
	/// Job to build the geometry from a stream of rendering commands.
	///
	/// See: <see cref="CommandBuilder"/>
	/// </summary>
	// Note: Setting FloatMode to Fast causes visual artificats when drawing circles.
	// I think it is because math.sin(float4) produces slightly different results
	// for each component in the input.
	[BurstCompile(FloatMode = FloatMode.Default)]
	internal struct GeometryBuilderJob : IJob {
		[NativeDisableUnsafePtrRestriction]
		public unsafe ProcessedBuilderData.MeshBuffers* buffers;

		[NativeDisableUnsafePtrRestriction]
		public unsafe SDFCharacter* characterInfo;
		public int characterInfoLength;

		public Color32 currentColor;
		public float4x4 currentMatrix;
		public LineWidthData currentLineWidthData;
		public float lineWidthMultiplier;
		float3 minBounds;
		float3 maxBounds;
		public float3 cameraPosition;
		public quaternion cameraRotation;
		public float2 cameraDepthToPixelSize;
		public float maxPixelError;
		public bool cameraIsOrthographic;

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct Vertex {
			public float3 position;
			public float3 uv2;
			public Color32 color;
			public float2 uv;
		}

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct TextVertex {
			public float3 position;
			public Color32 color;
			public float2 uv;
		}

		static unsafe void Add<T>(UnsafeAppendBuffer* buffer, T value) where T : unmanaged {
			int size = UnsafeUtility.SizeOf<T>();
			// We know that the buffer has enough capacity, so we can just write to the buffer without
			// having to add branches for the overflow case (like buffer->Add will do).
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			UnityEngine.Assertions.Assert.IsTrue(buffer->Length + size <= buffer->Capacity);
#endif
			*(T*)(buffer->Ptr + buffer->Length) = value;
			buffer->Length = buffer->Length + size;
		}

		static unsafe void Reserve (UnsafeAppendBuffer* buffer, int size) {
			var newSize = buffer->Length + size;

			if (newSize > buffer->Capacity) {
				buffer->SetCapacity(math.max(newSize, buffer->Capacity * 2));
			}
		}

		internal static float3 PerspectiveDivide (float4 p) {
			return p.xyz * math.rcp(p.w);
		}

		unsafe void AddText (System.UInt16* text, TextData textData, Color32 color) {
			var pivot = PerspectiveDivide(math.mul(currentMatrix, new float4(textData.center, 1.0f)));

			AddTextInternal(
				text,
				pivot,
				math.mul(cameraRotation, new float3(1, 0, 0)),
				math.mul(cameraRotation, new float3(0, 1, 0)),
				textData.alignment,
				textData.sizeInPixels,
				true,
				textData.numCharacters,
				color
				);
		}

		unsafe void AddText3D (System.UInt16* text, TextData3D textData, Color32 color) {
			var pivot = PerspectiveDivide(math.mul(currentMatrix, new float4(textData.center, 1.0f)));
			var m = math.mul(currentMatrix, new float4x4(textData.rotation, float3.zero));

			AddTextInternal(
				text,
				pivot,
				m.c0.xyz,
				m.c1.xyz,
				textData.alignment,
				textData.size,
				false,
				textData.numCharacters,
				color
				);
		}


		unsafe void AddTextInternal (System.UInt16* text, float3 pivot, float3 right, float3 up, LabelAlignment alignment, float size, bool sizeIsInPixels, int numCharacters, Color32 color) {
			var distance = math.abs(math.dot(pivot - cameraPosition, math.mul(cameraRotation, new float3(0, 0, 1))));
			var pixelSize = cameraDepthToPixelSize.x * distance + cameraDepthToPixelSize.y;
			float fontWorldSize = size;

			if (sizeIsInPixels) fontWorldSize *= pixelSize;

			right *= fontWorldSize;
			up *= fontWorldSize;

			// Calculate the total width (in pixels divided by fontSize) of the text
			float maxWidth = 0;
			float currentWidth = 0;
			float numLines = 1;

			for (int i = 0; i < numCharacters; i++) {
				var characterInfoIndex = text[i];
				if (characterInfoIndex == SDFLookupData.Newline) {
					maxWidth = math.max(maxWidth, currentWidth);
					currentWidth = 0;
					numLines++;
				} else {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
					if (characterInfoIndex >= characterInfoLength) throw new System.Exception("Invalid character. No info exists. This is a bug.");
#endif
					currentWidth += characterInfo[characterInfoIndex].advance;
				}
			}
			maxWidth = math.max(maxWidth, currentWidth);

			// Calculate the world space position of the text given the camera and text alignment
			var pos = pivot;
			pos -= right * maxWidth * alignment.relativePivot.x;
			// Size of a character as a fraction of a whole line using the current font
			const float FontCharacterFractionOfLine = 0.75f;
			// Where the upper and lower parts of the text will be assuming we start to write at y=0
			var lower = 1 - numLines;
			var upper = FontCharacterFractionOfLine;
			var yAdjustment = math.lerp(lower, upper, alignment.relativePivot.y);
			pos -= up * yAdjustment;
			pos += math.mul(cameraRotation, new float3(1, 0, 0)) * (pixelSize * alignment.pixelOffset.x);
			pos += math.mul(cameraRotation, new float3(0, 1, 0)) * (pixelSize * alignment.pixelOffset.y);

			var textVertices = &buffers->textVertices;
			var textTriangles = &buffers->textTriangles;

			// Reserve all buffer space beforehand
			Reserve(textVertices, numCharacters * VerticesPerCharacter * UnsafeUtility.SizeOf<TextVertex>());
			Reserve(textTriangles, numCharacters * TrianglesPerCharacter * UnsafeUtility.SizeOf<int>());

			var lineStart = pos;

			for (int i = 0; i < numCharacters; i++) {
				var characterInfoIndex = text[i];

				if (characterInfoIndex == SDFLookupData.Newline) {
					lineStart -= up;
					pos = lineStart;
					continue;
				}

				// Get character rendering information from the font
				SDFCharacter ch = characterInfo[characterInfoIndex];

				int vertexIndexStart = textVertices->Length / UnsafeUtility.SizeOf<TextVertex>();

				float3 v;

				v = pos + ch.vertexTopLeft.x * right + ch.vertexTopLeft.y * up;
				minBounds = math.min(minBounds, v);
				maxBounds = math.max(maxBounds, v);
				Add(textVertices, new TextVertex {
					position = v,
					uv = ch.uvTopLeft,
					color = color,
				});

				v = pos + ch.vertexTopRight.x * right + ch.vertexTopRight.y * up;
				minBounds = math.min(minBounds, v);
				maxBounds = math.max(maxBounds, v);
				Add(textVertices, new TextVertex {
					position = v,
					uv = ch.uvTopRight,
					color = color,
				});

				v = pos + ch.vertexBottomRight.x * right + ch.vertexBottomRight.y * up;
				minBounds = math.min(minBounds, v);
				maxBounds = math.max(maxBounds, v);
				Add(textVertices, new TextVertex {
					position = v,
					uv = ch.uvBottomRight,
					color = color,
				});

				v = pos + ch.vertexBottomLeft.x * right + ch.vertexBottomLeft.y * up;
				minBounds = math.min(minBounds, v);
				maxBounds = math.max(maxBounds, v);
				Add(textVertices, new TextVertex {
					position = v,
					uv = ch.uvBottomLeft,
					color = color,
				});

				Add(textTriangles, vertexIndexStart + 0);
				Add(textTriangles, vertexIndexStart + 1);
				Add(textTriangles, vertexIndexStart + 2);

				Add(textTriangles, vertexIndexStart + 0);
				Add(textTriangles, vertexIndexStart + 2);
				Add(textTriangles, vertexIndexStart + 3);

				// Advance character position
				pos += right * ch.advance;
			}
		}

		float3 lastNormalizedLineDir;
		float lastLineWidth;

		public const float MaxCirclePixelError = 0.5f;

		public const int VerticesPerCharacter = 4;
		public const int TrianglesPerCharacter = 6;

		void AddLine (LineData line) {
			// Store the line direction in the vertex.
			// A line consists of 4 vertices. The line direction will be used to
			// offset the vertices to create a line with a fixed pixel thickness
			var a = PerspectiveDivide(math.mul(currentMatrix, new float4(line.a, 1.0f)));
			var b = PerspectiveDivide(math.mul(currentMatrix, new float4(line.b, 1.0f)));

			float lineWidth = currentLineWidthData.pixels;
			var normalizedLineDir = math.normalizesafe(b - a);

			if (math.any(math.isnan(normalizedLineDir))) throw new Exception("Nan line coordinates");
			if (lineWidth <= 0) {
				return;
			}

			// Update the bounding box
			minBounds = math.min(minBounds, math.min(a, b));
			maxBounds = math.max(maxBounds, math.max(a, b));

			unsafe {
				var outlineVertices = &buffers->vertices;

				// Make sure there is enough allocated capacity for 4 more vertices
				Reserve(outlineVertices, 4 * UnsafeUtility.SizeOf<Vertex>());

				// Insert 4 vertices
				// Doing it with pointers is faster, and this is the hottest
				// code of the whole gizmo drawing process.
				var ptr = (Vertex*)((byte*)outlineVertices->Ptr + outlineVertices->Length);

				var startLineDir = normalizedLineDir * lineWidth;
				var endLineDir = normalizedLineDir * lineWidth;

				// If dot(last dir, this dir) >= 0 => use join
				if (lineWidth > 1 && currentLineWidthData.automaticJoins && outlineVertices->Length > 2*UnsafeUtility.SizeOf<Vertex>()) {
					// has previous vertex
					Vertex* lastVertex1 = (Vertex*)(ptr - 1);
					Vertex* lastVertex2 = (Vertex*)(ptr - 2);

					var cosAngle = math.dot(normalizedLineDir, lastNormalizedLineDir);
					if (math.all(lastVertex2->position == a) && lastLineWidth == lineWidth && cosAngle >= -0.6f) {
						// Safety: tangent cannot be 0 because cosAngle > -1
						var tangent = normalizedLineDir + lastNormalizedLineDir;
						// From the law of cosines we get that
						// tangent.magnitude = sqrt(2)*sqrt(1+cosAngle)

						// Create join!
						// Trigonometry gives us
						// joinRadius = lineWidth / (2*cos(alpha / 2))
						// Using half angle identity for cos we get
						// joinRadius = lineWidth / (sqrt(2)*sqrt(1 + cos(alpha))
						// Since the tangent already has mostly the same factors we can simplify the calculation
						// normalize(tangent) * joinRadius * 2
						// = tangent / (sqrt(2)*sqrt(1+cosAngle)) * joinRadius * 2
						// = tangent * lineWidth / (1 + cos(alpha)
						var joinLineDir = tangent * lineWidth / (1 + cosAngle);

						startLineDir = joinLineDir;
						lastVertex1->uv2 = startLineDir;
						lastVertex2->uv2 = startLineDir;
					}
				}

				outlineVertices->Length = outlineVertices->Length + 4 * UnsafeUtility.SizeOf<Vertex>();
				*ptr++ = new Vertex {
					position = a,
					color = currentColor,
					uv = new float2(0, 0),
					uv2 = startLineDir,
				};
				*ptr++ = new Vertex {
					position = a,
					color = currentColor,
					uv = new float2(1, 0),
					uv2 = startLineDir,
				};

				*ptr++ = new Vertex {
					position = b,
					color = currentColor,
					uv = new float2(0, 1),
					uv2 = endLineDir,
				};
				*ptr++ = new Vertex {
					position = b,
					color = currentColor,
					uv = new float2(1, 1),
					uv2 = endLineDir,
				};

				lastNormalizedLineDir = normalizedLineDir;
				lastLineWidth = lineWidth;
			}
		}

		/// <summary>Calculate number of steps to use for drawing a circle at the specified point and radius to get less than the specified pixel error.</summary>
		internal static int CircleSteps (float3 center, float radius, float maxPixelError, ref float4x4 currentMatrix, float2 cameraDepthToPixelSize, float3 cameraPosition) {
			var centerv4 = math.mul(currentMatrix, new float4(center, 1.0f));

			if (math.abs(centerv4.w) < 0.0000001f) return 3;
			var cc = PerspectiveDivide(centerv4);
			// Take the maximum scale factor among the 3 axes.
			// If the current matrix has a uniform scale then they are all the same.
			var maxScaleFactor = math.sqrt(math.max(math.max(math.lengthsq(currentMatrix.c0.xyz), math.lengthsq(currentMatrix.c1.xyz)), math.lengthsq(currentMatrix.c2.xyz))) / centerv4.w;
			var realWorldRadius = radius * maxScaleFactor;
			var distance = math.length(cc - cameraPosition);

			var pixelSize = cameraDepthToPixelSize.x * distance + cameraDepthToPixelSize.y;
			// realWorldRadius += pixelSize * this.currentLineWidthData.pixels * 0.5f;
			var cosAngle = 1 - (maxPixelError * pixelSize) / realWorldRadius;
			int steps = cosAngle < 0 ? 3 : (int)math.ceil(math.PI / (math.acos(cosAngle)));
			return steps;
		}

		void AddCircle (CircleData circle) {
			// If the circle has a zero normal then just ignore it
			if (math.all(circle.normal == 0)) return;

			circle.normal = math.normalize(circle.normal);
			// Canonicalize
			if (circle.normal.y < 0) circle.normal = -circle.normal;

			float3 tangent1;
			if (math.all(math.abs(circle.normal - new float3(0, 1, 0)) < 0.001f)) {
				// The normal was (almost) identical to (0, 1, 0)
				tangent1 = new float3(0, 0, 1);
			} else {
				// Common case
				tangent1 = math.normalizesafe(math.cross(circle.normal, new float3(0, 1, 0)));
			}

			var ex = tangent1;
			var ey = circle.normal;
			var ez = math.cross(ey, ex);
			var oldMatrix = currentMatrix;

			currentMatrix = math.mul(currentMatrix, new float4x4(
				new float4(ex, 0) * circle.radius,
				new float4(ey, 0) * circle.radius,
				new float4(ez, 0) * circle.radius,
				new float4(circle.center, 1)
				));

			AddCircle(new CircleXZData {
				center = new float3(0, 0, 0),
				radius = 1,
				startAngle = 0,
				endAngle = 2 * math.PI,
			});

			currentMatrix = oldMatrix;
		}

		void AddDisc (CircleData circle) {
			// If the circle has a zero normal then just ignore it
			if (math.all(circle.normal == 0)) return;

			var steps = CircleSteps(circle.center, circle.radius, maxPixelError, ref currentMatrix, cameraDepthToPixelSize, cameraPosition);

			circle.normal = math.normalize(circle.normal);
			float3 tangent1;
			if (math.all(math.abs(circle.normal - new float3(0, 1, 0)) < 0.001f)) {
				// The normal was (almost) identical to (0, 1, 0)
				tangent1 = new float3(0, 0, 1);
			} else {
				// Common case
				tangent1 = math.cross(circle.normal, new float3(0, 1, 0));
			}

			float invSteps = 1.0f / steps;

			unsafe {
				var solidVertices = &buffers->solidVertices;
				var solidTriangles = &buffers->solidTriangles;
				Reserve(solidVertices, steps * UnsafeUtility.SizeOf<Vertex>());
				Reserve(solidTriangles, 3*(steps-2) * UnsafeUtility.SizeOf<int>());

				var matrix = math.mul(currentMatrix, Matrix4x4.TRS(circle.center, Quaternion.LookRotation(circle.normal, tangent1), new Vector3(circle.radius, circle.radius, circle.radius)));

				var mn = minBounds;
				var mx = maxBounds;
				int vertexCount = solidVertices->Length / UnsafeUtility.SizeOf<Vertex>();

				for (int i = 0; i < steps; i++) {
					var t = math.lerp(0, 2*Mathf.PI, i * invSteps);
					math.sincos(t, out float sin, out float cos);

					var p = PerspectiveDivide(math.mul(matrix, new float4(cos, sin, 0, 1)));
					// Update the bounding box
					mn = math.min(mn, p);
					mx = math.max(mx, p);

					Add(solidVertices, new Vertex {
						position = p,
						color = currentColor,
						uv = new float2(0, 0),
						uv2 = new float3(0, 0, 0),
					});
				}

				minBounds = mn;
				maxBounds = mx;

				for (int i = 0; i < steps - 2; i++) {
					Add(solidTriangles, vertexCount);
					Add(solidTriangles, vertexCount + i + 1);
					Add(solidTriangles, vertexCount + i + 2);
				}
			}
		}

		void AddSphereOutline (SphereData circle) {
			var centerv4 = math.mul(currentMatrix, new float4(circle.center, 1.0f));

			if (math.abs(centerv4.w) < 0.0000001f) return;
			var center = PerspectiveDivide(centerv4);
			// Figure out the actual radius of the sphere after all the matrix multiplications.
			// In case of a non-uniform scale, pick the largest radius
			var maxScaleFactor = math.sqrt(math.max(math.max(math.lengthsq(currentMatrix.c0.xyz), math.lengthsq(currentMatrix.c1.xyz)), math.lengthsq(currentMatrix.c2.xyz))) / centerv4.w;
			var realWorldRadius = circle.radius * maxScaleFactor;

			if (cameraIsOrthographic) {
				var prevMatrix = this.currentMatrix;
				this.currentMatrix = float4x4.identity;
				AddCircle(new CircleData {
					center = center,
					normal = math.mul(this.cameraRotation, new float3(0, 0, 1)),
					radius = realWorldRadius,
				});
				this.currentMatrix = prevMatrix;
			} else {
				var dist = math.length(this.cameraPosition - center);
				// Camera is inside the sphere, cannot draw
				if (dist <= realWorldRadius) return;

				var offsetTowardsCamera = realWorldRadius * realWorldRadius / dist;
				var outlineRadius = math.sqrt(realWorldRadius * realWorldRadius - offsetTowardsCamera * offsetTowardsCamera);
				var normal = math.normalize(this.cameraPosition - center);
				var prevMatrix = this.currentMatrix;
				this.currentMatrix = float4x4.identity;
				AddCircle(new CircleData {
					center = center + normal * offsetTowardsCamera,
					normal = normal,
					radius = outlineRadius,
				});
				this.currentMatrix = prevMatrix;
			}
		}

		void AddCircle (CircleXZData circle) {
			circle.endAngle = math.clamp(circle.endAngle, circle.startAngle - Mathf.PI * 2, circle.startAngle + Mathf.PI * 2);

			unsafe {
				var m = math.mul(currentMatrix, new float4x4(
					new float4(circle.radius, 0, 0, 0),
					new float4(0, circle.radius, 0, 0),
					new float4(0, 0, circle.radius, 0),
					new float4(circle.center, 1)
					));
				var steps = CircleSteps(float3.zero, 1.0f, maxPixelError, ref m, cameraDepthToPixelSize, cameraPosition);
				var lineWidth = currentLineWidthData.pixels;
				if (lineWidth < 0) return;

				var byteSize = steps * 4 * UnsafeUtility.SizeOf<Vertex>();
				Reserve(&buffers->vertices, byteSize);
				var ptr = (Vertex*)(buffers->vertices.Ptr + buffers->vertices.Length);
				buffers->vertices.Length += byteSize;
				math.sincos(circle.startAngle, out float sin0, out float cos0);
				var prev = PerspectiveDivide(math.mul(m, new float4(cos0, 0, sin0, 1)));
				var prevTangent = math.normalizesafe(math.mul(m, new float4(-sin0, 0, cos0, 0)).xyz) * lineWidth;
				var invSteps = math.rcp(steps);

				for (int i = 1; i <= steps; i++) {
					var t = math.lerp(circle.startAngle, circle.endAngle, i * invSteps);
					math.sincos(t, out float sin, out float cos);
					var next = PerspectiveDivide(math.mul(m, new float4(cos, 0, sin, 1)));
					var tangent = math.normalizesafe(math.mul(m, new float4(-sin, 0, cos, 0)).xyz) * lineWidth;
					*ptr++ = new Vertex {
						position = prev,
						color = currentColor,
						uv = new float2(0, 0),
						uv2 = prevTangent,
					};
					*ptr++ = new Vertex {
						position = prev,
						color = currentColor,
						uv = new float2(1, 0),
						uv2 = prevTangent,
					};
					*ptr++ = new Vertex {
						position = next,
						color = currentColor,
						uv = new float2(0, 1),
						uv2 = tangent,
					};
					*ptr++ = new Vertex {
						position = next,
						color = currentColor,
						uv = new float2(1, 1),
						uv2 = tangent,
					};

					prev = next;
					prevTangent = tangent;
				}

				// Update the global bounds with the bounding box of the circle
				var b0 = PerspectiveDivide(math.mul(m, new float4(-1, 0, 0, 1)));
				var b1 = PerspectiveDivide(math.mul(m, new float4(0, -1, 0, 1)));
				var b2 = PerspectiveDivide(math.mul(m, new float4(+1, 0, 0, 1)));
				var b3 = PerspectiveDivide(math.mul(m, new float4(0, +1, 0, 1)));
				minBounds = math.min(math.min(math.min(math.min(b0, b1), b2), b3), minBounds);
				maxBounds = math.max(math.max(math.max(math.max(b0, b1), b2), b3), maxBounds);
			}
		}

		void AddDisc (CircleXZData circle) {
			var steps = CircleSteps(circle.center, circle.radius, maxPixelError, ref currentMatrix, cameraDepthToPixelSize, cameraPosition);

			circle.endAngle = math.clamp(circle.endAngle, circle.startAngle - Mathf.PI * 2, circle.startAngle + Mathf.PI * 2);

			float invSteps = 1.0f / steps;

			unsafe {
				var solidVertices = &buffers->solidVertices;
				var solidTriangles = &buffers->solidTriangles;
				Reserve(solidVertices, (2+steps) * UnsafeUtility.SizeOf<Vertex>());
				Reserve(solidTriangles, 3*steps * UnsafeUtility.SizeOf<int>());

				var matrix = math.mul(currentMatrix, Matrix4x4.Translate(circle.center) * Matrix4x4.Scale(new Vector3(circle.radius, circle.radius, circle.radius)));

				var worldCenter = PerspectiveDivide(math.mul(matrix, new float4(0, 0, 0, 1)));
				Add(solidVertices, new Vertex {
					position = worldCenter,
					color = currentColor,
					uv = new float2(0, 0),
					uv2 = new float3(0, 0, 0),
				});

				var mn = math.min(minBounds, worldCenter);
				var mx = math.max(maxBounds, worldCenter);
				int vertexCount = solidVertices->Length / UnsafeUtility.SizeOf<Vertex>();

				for (int i = 0; i <= steps; i++) {
					var t = math.lerp(circle.startAngle, circle.endAngle, i * invSteps);
					math.sincos(t, out float sin, out float cos);

					var p = PerspectiveDivide(math.mul(matrix, new float4(cos, 0, sin, 1)));
					// Update the bounding box
					mn = math.min(mn, p);
					mx = math.max(mx, p);

					Add(solidVertices, new Vertex {
						position = p,
						color = currentColor,
						uv = new float2(0, 0),
						uv2 = new float3(0, 0, 0),
					});
				}

				minBounds = mn;
				maxBounds = mx;

				for (int i = 0; i < steps; i++) {
					// Center vertex
					Add(solidTriangles, vertexCount - 1);
					Add(solidTriangles, vertexCount + i + 0);
					Add(solidTriangles, vertexCount + i + 1);
				}
			}
		}

		void AddSolidTriangle (TriangleData triangle) {
			unsafe {
				var solidVertices = &buffers->solidVertices;
				var solidTriangles = &buffers->solidTriangles;
				Reserve(solidVertices, 3 * UnsafeUtility.SizeOf<Vertex>());
				Reserve(solidTriangles, 3 * UnsafeUtility.SizeOf<int>());
				var matrix = currentMatrix;
				var a = PerspectiveDivide(math.mul(matrix, new float4(triangle.a, 1)));
				var b = PerspectiveDivide(math.mul(matrix, new float4(triangle.b, 1)));
				var c = PerspectiveDivide(math.mul(matrix, new float4(triangle.c, 1)));
				int startVertex = solidVertices->Length / UnsafeUtility.SizeOf<Vertex>();

				minBounds = math.min(math.min(math.min(minBounds, a), b), c);
				maxBounds = math.max(math.max(math.max(maxBounds, a), b), c);

				Add(solidVertices, new Vertex {
					position = a,
					color = currentColor,
					uv = new float2(0, 0),
					uv2 = new float3(0, 0, 0),
				});
				Add(solidVertices, new Vertex {
					position = b,
					color = currentColor,
					uv = new float2(0, 0),
					uv2 = new float3(0, 0, 0),
				});
				Add(solidVertices, new Vertex {
					position = c,
					color = currentColor,
					uv = new float2(0, 0),
					uv2 = new float3(0, 0, 0),
				});

				Add(solidTriangles, startVertex + 0);
				Add(solidTriangles, startVertex + 1);
				Add(solidTriangles, startVertex + 2);
			}
		}

		void AddWireBox (BoxData box) {
			var min = box.center - box.size * 0.5f;
			var max = box.center + box.size * 0.5f;
			AddLine(new LineData { a = new float3(min.x, min.y, min.z), b = new float3(max.x, min.y, min.z) });
			AddLine(new LineData { a = new float3(max.x, min.y, min.z), b = new float3(max.x, min.y, max.z) });
			AddLine(new LineData { a = new float3(max.x, min.y, max.z), b = new float3(min.x, min.y, max.z) });
			AddLine(new LineData { a = new float3(min.x, min.y, max.z), b = new float3(min.x, min.y, min.z) });

			AddLine(new LineData { a = new float3(min.x, max.y, min.z), b = new float3(max.x, max.y, min.z) });
			AddLine(new LineData { a = new float3(max.x, max.y, min.z), b = new float3(max.x, max.y, max.z) });
			AddLine(new LineData { a = new float3(max.x, max.y, max.z), b = new float3(min.x, max.y, max.z) });
			AddLine(new LineData { a = new float3(min.x, max.y, max.z), b = new float3(min.x, max.y, min.z) });

			AddLine(new LineData { a = new float3(min.x, min.y, min.z), b = new float3(min.x, max.y, min.z) });
			AddLine(new LineData { a = new float3(max.x, min.y, min.z), b = new float3(max.x, max.y, min.z) });
			AddLine(new LineData { a = new float3(max.x, min.y, max.z), b = new float3(max.x, max.y, max.z) });
			AddLine(new LineData { a = new float3(min.x, min.y, max.z), b = new float3(min.x, max.y, max.z) });
		}

		void AddPlane (PlaneData plane) {
			var oldMatrix = currentMatrix;

			currentMatrix = math.mul(currentMatrix, float4x4.TRS(plane.center, plane.rotation, new float3(plane.size.x * 0.5f, 1, plane.size.y * 0.5f)));

			AddLine(new LineData { a = new float3(-1, 0, -1), b = new float3(1, 0, -1) });
			AddLine(new LineData { a = new float3(1, 0, -1), b = new float3(1, 0, 1) });
			AddLine(new LineData { a = new float3(1, 0, 1), b = new float3(-1, 0, 1) });
			AddLine(new LineData { a = new float3(-1, 0, 1), b = new float3(-1, 0, -1) });

			currentMatrix = oldMatrix;
		}

		internal static readonly float4[] BoxVertices = {
			new float4(-1, -1, -1, 1),
			new float4(-1, -1, +1, 1),
			new float4(-1, +1, -1, 1),
			new float4(-1, +1, +1, 1),
			new float4(+1, -1, -1, 1),
			new float4(+1, -1, +1, 1),
			new float4(+1, +1, -1, 1),
			new float4(+1, +1, +1, 1),
		};

		internal static readonly int[] BoxTriangles = {
			// Bottom two triangles
			0, 1, 5,
			0, 5, 4,

			// Top
			7, 3, 2,
			7, 2, 6,

			// -X
			0, 1, 3,
			0, 3, 2,

			// +X
			4, 5, 7,
			4, 7, 6,

			// +Z
			1, 3, 7,
			1, 7, 5,

			// -Z
			0, 2, 6,
			0, 6, 4,
		};

		void AddBox (BoxData box) {
			unsafe {
				var solidVertices = &buffers->solidVertices;
				var solidTriangles = &buffers->solidTriangles;
				Reserve(solidVertices, BoxVertices.Length * UnsafeUtility.SizeOf<Vertex>());
				Reserve(solidTriangles, BoxTriangles.Length * UnsafeUtility.SizeOf<int>());

				var scale = box.size * 0.5f;
				var matrix = math.mul(currentMatrix, new float4x4(
					new float4(scale.x, 0, 0, 0),
					new float4(0, scale.y, 0, 0),
					new float4(0, 0, scale.z, 0),
					new float4(box.center, 1)
					));

				var mn = minBounds;
				var mx = maxBounds;
				int vertexOffset = solidVertices->Length / UnsafeUtility.SizeOf<Vertex>();
				var ptr = (Vertex*)(solidVertices->Ptr + solidVertices->Length);
				for (int i = 0; i < BoxVertices.Length; i++) {
					var p = PerspectiveDivide(math.mul(matrix, BoxVertices[i]));
					// Update the bounding box
					mn = math.min(mn, p);
					mx = math.max(mx, p);

					*ptr++ = new Vertex {
						position = p,
						color = currentColor,
						uv = new float2(0, 0),
						uv2 = new float3(0, 0, 0),
					};
				}
				solidVertices->Length += BoxVertices.Length * UnsafeUtility.SizeOf<Vertex>();

				minBounds = mn;
				maxBounds = mx;

				var triPtr = (int*)(solidTriangles->Ptr + solidTriangles->Length);
				for (int i = 0; i < BoxTriangles.Length; i++) {
					*triPtr++ = vertexOffset + BoxTriangles[i];
				}
				solidTriangles->Length += BoxTriangles.Length * UnsafeUtility.SizeOf<int>();
			}
		}

		// AggressiveInlining because this is only called from a single location, and burst doesn't inline otherwise
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void Next (ref UnsafeAppendBuffer.Reader reader, ref NativeArray<float4x4> matrixStack, ref NativeArray<Color32> colorStack, ref NativeArray<LineWidthData> lineWidthStack, ref int matrixStackSize, ref int colorStackSize, ref int lineWidthStackSize) {
			var fullCmd = reader.ReadNext<Command>();
			var cmd = fullCmd & (Command)0xFF;
			Color32 oldColor = default;

			if ((fullCmd & Command.PushColorInline) != 0) {
				oldColor = currentColor;
				currentColor = reader.ReadNext<Color32>();
			}

			switch (cmd) {
			case Command.PushColor:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (colorStackSize >= colorStack.Length) throw new System.Exception("Too deeply nested PushColor calls");
#else
				if (colorStackSize >= colorStack.Length) colorStackSize--;
#endif
				colorStack[colorStackSize] = currentColor;
				colorStackSize++;
				currentColor = reader.ReadNext<Color32>();
				break;
			case Command.PopColor:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (colorStackSize <= 0) throw new System.Exception("PushColor and PopColor are not matched");
#else
				if (colorStackSize <= 0) break;
#endif
				colorStackSize--;
				currentColor = colorStack[colorStackSize];
				break;
			case Command.PushMatrix:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (matrixStackSize >= matrixStack.Length) throw new System.Exception("Too deeply nested PushMatrix calls");
#else
				if (matrixStackSize >= matrixStack.Length) matrixStackSize--;
#endif
				matrixStack[matrixStackSize] = currentMatrix;
				matrixStackSize++;
				currentMatrix = math.mul(currentMatrix, reader.ReadNext<float4x4>());
				break;
			case Command.PushSetMatrix:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (matrixStackSize >= matrixStack.Length) throw new System.Exception("Too deeply nested PushMatrix calls");
#else
				if (matrixStackSize >= matrixStack.Length) matrixStackSize--;
#endif
				matrixStack[matrixStackSize] = currentMatrix;
				matrixStackSize++;
				currentMatrix = reader.ReadNext<float4x4>();
				break;
			case Command.PopMatrix:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (matrixStackSize <= 0) throw new System.Exception("PushMatrix and PopMatrix are not matched");
#else
				if (matrixStackSize <= 0) break;
#endif
				matrixStackSize--;
				currentMatrix = matrixStack[matrixStackSize];
				break;
			case Command.PushLineWidth:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (lineWidthStackSize >= lineWidthStack.Length) throw new System.Exception("Too deeply nested PushLineWidth calls");
#else
				if (lineWidthStackSize >= lineWidthStack.Length) lineWidthStackSize--;
#endif
				lineWidthStack[lineWidthStackSize] = currentLineWidthData;
				lineWidthStackSize++;
				currentLineWidthData = reader.ReadNext<LineWidthData>();
				currentLineWidthData.pixels *= lineWidthMultiplier;
				break;
			case Command.PopLineWidth:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (lineWidthStackSize <= 0) throw new System.Exception("PushLineWidth and PopLineWidth are not matched");
#else
				if (lineWidthStackSize <= 0) break;
#endif
				lineWidthStackSize--;
				currentLineWidthData = lineWidthStack[lineWidthStackSize];
				break;
			case Command.Line:
				AddLine(reader.ReadNext<LineData>());
				break;
			case Command.SphereOutline:
				AddSphereOutline(reader.ReadNext<SphereData>());
				break;
			case Command.CircleXZ:
				AddCircle(reader.ReadNext<CircleXZData>());
				break;
			case Command.Circle:
				AddCircle(reader.ReadNext<CircleData>());
				break;
			case Command.DiscXZ:
				AddDisc(reader.ReadNext<CircleXZData>());
				break;
			case Command.Disc:
				AddDisc(reader.ReadNext<CircleData>());
				break;
			case Command.Box:
				AddBox(reader.ReadNext<BoxData>());
				break;
			case Command.WirePlane:
				AddPlane(reader.ReadNext<PlaneData>());
				break;
			case Command.WireBox:
				AddWireBox(reader.ReadNext<BoxData>());
				break;
			case Command.SolidTriangle:
				AddSolidTriangle(reader.ReadNext<TriangleData>());
				break;
			case Command.PushPersist:
				// This command does not need to be handled by the builder
				reader.ReadNext<PersistData>();
				break;
			case Command.PopPersist:
				// This command does not need to be handled by the builder
				break;
			case Command.Text:
				var data = reader.ReadNext<TextData>();
				unsafe {
					System.UInt16* ptr = (System.UInt16*)reader.ReadNext(UnsafeUtility.SizeOf<System.UInt16>() * data.numCharacters);
					AddText(ptr, data, currentColor);
				}
				break;
			case Command.Text3D:
				var data2 = reader.ReadNext<TextData3D>();
				unsafe {
					System.UInt16* ptr = (System.UInt16*)reader.ReadNext(UnsafeUtility.SizeOf<System.UInt16>() * data2.numCharacters);
					AddText3D(ptr, data2, currentColor);
				}
				break;
			case Command.CaptureState:
				unsafe {
					buffers->capturedState.Add(new ProcessedBuilderData.CapturedState {
						color = this.currentColor,
						matrix = this.currentMatrix,
					});
				}
				break;
			default:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				throw new System.Exception("Unknown command");
#else
				break;
#endif
			}

			if ((fullCmd & Command.PushColorInline) != 0) {
				currentColor = oldColor;
			}
		}

		void CreateTriangles () {
			// Create triangles for all lines
			// A triangle consists of 3 indices
			// A line (4 vertices) consists of 2 triangles, so 6 triangle indices
			unsafe {
				var outlineVertices = &buffers->vertices;
				var outlineTriangles = &buffers->triangles;
				var vertexCount = outlineVertices->Length / UnsafeUtility.SizeOf<Vertex>();
				// Each line is made out of 4 vertices
				var lineCount = vertexCount / 4;
				var trianglesSizeInBytes = lineCount * 6 * UnsafeUtility.SizeOf<int>();
				if (trianglesSizeInBytes >= outlineTriangles->Capacity) {
					outlineTriangles->SetCapacity(math.ceilpow2(trianglesSizeInBytes));
				}

				int* ptr = (int*)outlineTriangles->Ptr;
				for (int i = 0, vi = 0; i < lineCount; i++, vi += 4) {
					// First triangle
					*ptr++ = vi + 0;
					*ptr++ = vi + 1;
					*ptr++ = vi + 2;

					// Second triangle
					*ptr++ = vi + 1;
					*ptr++ = vi + 3;
					*ptr++ = vi + 2;
				}
				outlineTriangles->Length = trianglesSizeInBytes;
			}
		}

		public const int MaxStackSize = 32;

		public void Execute () {
			unsafe {
				buffers->vertices.Reset();
				buffers->triangles.Reset();
				buffers->solidVertices.Reset();
				buffers->solidTriangles.Reset();
				buffers->textVertices.Reset();
				buffers->textTriangles.Reset();
				buffers->capturedState.Reset();
			}

			currentLineWidthData.pixels *= lineWidthMultiplier;

			minBounds = new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			maxBounds = new float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

			var matrixStack = new NativeArray<float4x4>(MaxStackSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			var colorStack = new NativeArray<Color32>(MaxStackSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			var lineWidthStack = new NativeArray<LineWidthData>(MaxStackSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			int matrixStackSize = 0;
			int colorStackSize = 0;
			int lineWidthStackSize = 0;

			CommandBuilderSamplers.MarkerProcessCommands.Begin();
			unsafe {
				var reader = buffers->splitterOutput.AsReader();
				while (reader.Offset < reader.Size) Next(ref reader, ref matrixStack, ref colorStack, ref lineWidthStack, ref matrixStackSize, ref colorStackSize, ref lineWidthStackSize);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (reader.Offset != reader.Size) throw new Exception("Didn't reach the end of the buffer");
#endif
			}
			CommandBuilderSamplers.MarkerProcessCommands.End();

			CommandBuilderSamplers.MarkerCreateTriangles.Begin();
			CreateTriangles();
			CommandBuilderSamplers.MarkerCreateTriangles.End();

			unsafe {
				var outBounds = &buffers->bounds;
				*outBounds = new Bounds((minBounds + maxBounds) * 0.5f, maxBounds - minBounds);

				if (math.any(math.isnan(outBounds->min)) && (buffers->vertices.Length > 0 || buffers->solidTriangles.Length > 0)) {
					// Fall back to a bounding box that covers everything
					*outBounds = new Bounds(Vector3.zero, new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));
#if ENABLE_UNITY_COLLECTIONS_CHECKS
					throw new Exception("NaN bounds. A Draw.* command may have been given NaN coordinates.");
#endif
				}
			}
		}
	}
}
