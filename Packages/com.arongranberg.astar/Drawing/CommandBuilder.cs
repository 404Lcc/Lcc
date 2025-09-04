using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

namespace Pathfinding.Drawing {
	using static DrawingData;
	using BitPackedMeta = DrawingData.BuilderData.BitPackedMeta;
	using Pathfinding.Drawing.Text;
	using Unity.Profiling;

	/// <summary>
	/// Specifies text alignment relative to an anchor point.
	///
	/// <code>
	/// Draw.Label2D(transform.position, "Hello World", 14, LabelAlignment.TopCenter);
	/// </code>
	/// <code>
	/// // Draw the label 20 pixels below the object
	/// Draw.Label2D(transform.position, "Hello World", 14, LabelAlignment.TopCenter.withPixelOffset(0, -20));
	/// </code>
	///
	/// See: <see cref="Draw.Label2D"/>
	/// See: <see cref="Draw.Label3D"/>
	/// </summary>
	public struct LabelAlignment {
		/// <summary>
		/// Where on the text's bounding box to anchor the text.
		///
		/// The pivot is specified in relative coordinates, where (0,0) is the bottom left corner and (1,1) is the top right corner.
		/// </summary>
		public float2 relativePivot;
		/// <summary>How much to move the text in screen-space</summary>
		public float2 pixelOffset;

		public static readonly LabelAlignment TopLeft = new LabelAlignment { relativePivot = new float2(0.0f, 1.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment MiddleLeft = new LabelAlignment { relativePivot = new float2(0.0f, 0.5f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment BottomLeft = new LabelAlignment { relativePivot = new float2(0.0f, 0.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment BottomCenter = new LabelAlignment { relativePivot = new float2(0.5f, 0.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment BottomRight = new LabelAlignment { relativePivot = new float2(1.0f, 0.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment MiddleRight = new LabelAlignment { relativePivot = new float2(1.0f, 0.5f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment TopRight = new LabelAlignment { relativePivot = new float2(1.0f, 1.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment TopCenter = new LabelAlignment { relativePivot = new float2(0.5f, 1.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment Center = new LabelAlignment { relativePivot = new float2(0.5f, 0.5f), pixelOffset = new float2(0, 0) };

		/// <summary>
		/// Moves the text by the specified amount of pixels in screen-space.
		///
		/// <code>
		/// // Draw the label 20 pixels below the object
		/// Draw.Label2D(transform.position, "Hello World", 14, LabelAlignment.TopCenter.withPixelOffset(0, -20));
		/// </code>
		/// </summary>
		public LabelAlignment withPixelOffset (float x, float y) {
			return new LabelAlignment {
					   relativePivot = this.relativePivot,
					   pixelOffset = new float2(x, y),
			};
		}
	}

	/// <summary>Maximum allowed delay for a job that is drawing to a command buffer</summary>
	public enum AllowedDelay {
		/// <summary>
		/// If the job is not complete at the end of the frame, drawing will block until it is completed.
		/// This is recommended for most jobs that are expected to complete within a single frame.
		/// </summary>
		EndOfFrame,
		/// <summary>
		/// Wait indefinitely for the job to complete, and only submit the results for rendering once it is done.
		/// This is recommended for long running jobs that may take many frames to complete.
		/// </summary>
		Infinite,
	}

	/// <summary>Some static fields that need to be in a separate class because Burst doesn't support them</summary>
	static class CommandBuilderSamplers {
		internal static readonly ProfilerMarker MarkerConvert = new ProfilerMarker("Convert");
		internal static readonly ProfilerMarker MarkerSetLayout = new ProfilerMarker("SetLayout");
		internal static readonly ProfilerMarker MarkerUpdateVertices = new ProfilerMarker("UpdateVertices");
		internal static readonly ProfilerMarker MarkerUpdateIndices = new ProfilerMarker("UpdateIndices");
		internal static readonly ProfilerMarker MarkerSubmesh = new ProfilerMarker("Submesh");
		internal static readonly ProfilerMarker MarkerUpdateBuffer = new ProfilerMarker("UpdateComputeBuffer");

		internal static readonly ProfilerMarker MarkerProcessCommands = new ProfilerMarker("Commands");
		internal static readonly ProfilerMarker MarkerCreateTriangles = new ProfilerMarker("CreateTriangles");
	}

	/// <summary>
	/// Builder for drawing commands.
	/// You can use this to queue many drawing commands. The commands will be queued for rendering when you call the Dispose method.
	/// It is recommended that you use the <a href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-statement">using statement</a> which automatically calls the Dispose method.
	///
	/// <code>
	/// // Create a new CommandBuilder
	/// using (var draw = DrawingManager.GetBuilder()) {
	///     // Use the exact same API as the global Draw class
	///     draw.WireBox(Vector3.zero, Vector3.one);
	/// }
	/// </code>
	///
	/// Warning: You must call either <see cref="Dispose"/> or <see cref="DiscardAndDispose"/> when you are done with this object to avoid memory leaks.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	[BurstCompile]
	public partial struct CommandBuilder : IDisposable {
		// Note: Many fields/methods are explicitly marked as private. This is because doxygen otherwise thinks they are public by default (like struct members are in c++)

		[NativeDisableUnsafePtrRestriction]
		internal unsafe UnsafeAppendBuffer* buffer;

		private GCHandle gizmos;

		[NativeSetThreadIndex]
		private int threadIndex;

		private DrawingData.BuilderData.BitPackedMeta uniqueID;

		internal unsafe CommandBuilder(UnsafeAppendBuffer* buffer, GCHandle gizmos, int threadIndex, DrawingData.BuilderData.BitPackedMeta uniqueID) {
			this.buffer = buffer;
			this.gizmos = gizmos;
			this.threadIndex = threadIndex;
			this.uniqueID = uniqueID;
		}


		internal CommandBuilder(DrawingData gizmos, Hasher hasher, RedrawScope frameRedrawScope, RedrawScope customRedrawScope, bool isGizmos, bool isBuiltInCommandBuilder, int sceneModeVersion) {
			// We need to use a GCHandle instead of a normal reference to be able to pass this object to burst compiled function pointers.
			// The NativeSetClassTypeToNullOnSchedule unfortunately only works together with the job system, not with raw functions.
			this.gizmos = GCHandle.Alloc(gizmos, GCHandleType.Normal);

			threadIndex = 0;
			uniqueID = gizmos.data.Reserve(isBuiltInCommandBuilder);
			gizmos.data.Get(uniqueID).Init(hasher, frameRedrawScope, customRedrawScope, isGizmos, gizmos.GetNextDrawOrderIndex(), sceneModeVersion);
			unsafe {
				buffer = gizmos.data.Get(uniqueID).bufferPtr;
			}
		}

		internal unsafe int BufferSize {
			get {
				return buffer->Length;
			}
			set {
				buffer->Length = value;
			}
		}

		/// <summary>
		/// Wrapper for drawing in the XY plane.
		///
		/// <code>
		/// var p1 = new Vector2(0, 1);
		/// var p2 = new Vector2(5, 7);
		///
		/// // Draw it in the XY plane
		/// Draw.xy.Line(p1, p2);
		///
		/// // Draw it in the XZ plane
		/// Draw.xz.Line(p1, p2);
		/// </code>
		///
		/// See: 2d-drawing (view in online documentation for working links)
		/// See: <see cref="Draw.xz"/>
		/// </summary>
		public CommandBuilder2D xy => new CommandBuilder2D(this, true);

		/// <summary>
		/// Wrapper for drawing in the XZ plane.
		///
		/// <code>
		/// var p1 = new Vector2(0, 1);
		/// var p2 = new Vector2(5, 7);
		///
		/// // Draw it in the XY plane
		/// Draw.xy.Line(p1, p2);
		///
		/// // Draw it in the XZ plane
		/// Draw.xz.Line(p1, p2);
		/// </code>
		///
		/// See: 2d-drawing (view in online documentation for working links)
		/// See: <see cref="Draw.xy"/>
		/// </summary>
		public CommandBuilder2D xz => new CommandBuilder2D(this, false);

		static readonly float3 DEFAULT_UP = new float3(0, 1, 0);

		/// <summary>
		/// Can be set to render specifically to these cameras.
		/// If you set this property to an array of cameras then this command builder will only be rendered
		/// to the specified cameras. Setting this property bypasses <see cref="Drawing.DrawingManager.allowRenderToRenderTextures"/>.
		/// The camera will be rendered to even if it renders to a render texture.
		///
		/// A null value indicates that all valid cameras should be rendered to. This is the default value.
		///
		/// <code>
		/// var draw = DrawingManager.GetBuilder(true);
		///
		/// draw.cameraTargets = new Camera[] { myCamera };
		/// // This sphere will only be rendered to myCamera
		/// draw.WireSphere(Vector3.zero, 0.5f, Color.black);
		/// draw.Dispose();
		/// </code>
		///
		/// See: advanced (view in online documentation for working links)
		/// </summary>
		public Camera[] cameraTargets {
			get {
				if (gizmos.IsAllocated && gizmos.Target != null) {
					var target = gizmos.Target as DrawingData;
					if (target.data.StillExists(uniqueID)) {
						return target.data.Get(uniqueID).meta.cameraTargets;
					}
				}
				throw new System.Exception("Cannot get cameraTargets because the command builder has already been disposed or does not exist.");
			}
			set {
				if (uniqueID.isBuiltInCommandBuilder) throw new System.Exception("You cannot set the camera targets for a built-in command builder. Create a custom command builder instead.");
				if (gizmos.IsAllocated && gizmos.Target != null) {
					var target = gizmos.Target as DrawingData;
					if (!target.data.StillExists(uniqueID)) {
						throw new System.Exception("Cannot set cameraTargets because the command builder has already been disposed or does not exist.");
					}
					target.data.Get(uniqueID).meta.cameraTargets = value;
				}
			}
		}

		/// <summary>Submits this command builder for rendering</summary>
		public void Dispose () {
			if (uniqueID.isBuiltInCommandBuilder) throw new System.Exception("You cannot dispose a built-in command builder");
			DisposeInternal();
		}

		/// <summary>
		/// Disposes this command builder after the given job has completed.
		///
		/// This is convenient if you are using the entity-component-system/burst in Unity and don't know exactly when the job will complete.
		///
		/// You will not be able to use this command builder on the main thread anymore.
		///
		/// See: job-system (view in online documentation for working links)
		/// </summary>
		/// <param name="dependency">The job that must complete before this command builder is disposed.</param>
		/// <param name="allowedDelay">Whether to block on this dependency before rendering the current frame or not.
		///    If the job is expected to complete during a single frame, leave at the default of \reflink{AllowedDelay.EndOfFrame}.
		///    But if the job is expected to take multiple frames to complete, you can set this to \reflink{AllowedDelay.Infinite}.</param>
		public void DisposeAfter (JobHandle dependency, AllowedDelay allowedDelay = AllowedDelay.EndOfFrame) {
			if (!gizmos.IsAllocated) throw new System.Exception("You cannot dispose an invalid command builder. Are you trying to dispose it twice?");
			try {
				if (gizmos.IsAllocated && gizmos.Target != null) {
					var target = gizmos.Target as DrawingData;
					if (!target.data.StillExists(uniqueID)) {
						throw new System.Exception("Cannot dispose the command builder because the drawing manager has been destroyed");
					}
					target.data.Get(uniqueID).SubmitWithDependency(gizmos, dependency, allowedDelay);
				}
			} finally {
				this = default;
			}
		}

		internal void DisposeInternal () {
			if (!gizmos.IsAllocated) throw new System.Exception("You cannot dispose an invalid command builder. Are you trying to dispose it twice?");
			try {
				if (gizmos.IsAllocated && gizmos.Target != null) {
					var target = gizmos.Target as DrawingData;
					if (!target.data.StillExists(uniqueID)) {
						throw new System.Exception("Cannot dispose the command builder because the drawing manager has been destroyed");
					}
					target.data.Get(uniqueID).Submit(gizmos.Target as DrawingData);
				}
			} finally {
				gizmos.Free();
				this = default;
			}
		}

		/// <summary>
		/// Discards the contents of this command builder without rendering anything.
		/// If you are not going to draw anything (i.e. you do not call the <see cref="Dispose"/> method) then you must call this method to avoid
		/// memory leaks.
		/// </summary>
		public void DiscardAndDispose () {
			if (uniqueID.isBuiltInCommandBuilder) throw new System.Exception("You cannot dispose a built-in command builder");
			DiscardAndDisposeInternal();
		}

		internal void DiscardAndDisposeInternal () {
			try {
				if (gizmos.IsAllocated && gizmos.Target != null) {
					var target = gizmos.Target as DrawingData;
					if (!target.data.StillExists(uniqueID)) {
						throw new System.Exception("Cannot dispose the command builder because the drawing manager has been destroyed");
					}
					target.data.Release(uniqueID);
				}
			} finally {
				if (gizmos.IsAllocated) gizmos.Free();
				this = default;
			}
		}

		/// <summary>
		/// Pre-allocates the internal buffer to an additional size bytes.
		/// This can give you a minor performance boost if you are drawing a lot of things.
		///
		/// Note: Only resizes the buffer for the current thread.
		/// </summary>
		public void Preallocate (int size) {
			Reserve(size);
		}

		/// <summary>Internal rendering command</summary>
		[System.Flags]
		internal enum Command {
			PushColorInline = 1 << 8,
			PushColor = 0,
			PopColor,
			PushMatrix,
			PushSetMatrix,
			PopMatrix,
			Line,
			Circle,
			CircleXZ,
			Disc,
			DiscXZ,
			SphereOutline,
			Box,
			WirePlane,
			WireBox,
			SolidTriangle,
			PushPersist,
			PopPersist,
			Text,
			Text3D,
			PushLineWidth,
			PopLineWidth,
			CaptureState,
		}

		internal struct TriangleData {
			public float3 a, b, c;
		}

		/// <summary>Holds rendering data for a line</summary>
		internal struct LineData {
			public float3 a, b;
		}

		internal struct LineDataV3 {
			public Vector3 a, b;
		}

		/// <summary>Holds rendering data for a circle</summary>
		internal struct CircleXZData {
			public float3 center;
			public float radius, startAngle, endAngle;
		}

		/// <summary>Holds rendering data for a circle</summary>
		internal struct CircleData {
			public float3 center;
			public float3 normal;
			public float radius;
		}

		/// <summary>Holds rendering data for a sphere</summary>
		internal struct SphereData {
			public float3 center;
			public float radius;
		}

		/// <summary>Holds rendering data for a box</summary>
		internal struct BoxData {
			public float3 center;
			public float3 size;
		}

		internal struct PlaneData {
			public float3 center;
			public quaternion rotation;
			public float2 size;
		}

		internal struct PersistData {
			public float endTime;
		}

		internal struct LineWidthData {
			public float pixels;
			public bool automaticJoins;
		}



		internal struct TextData {
			public float3 center;
			public LabelAlignment alignment;
			public float sizeInPixels;
			public int numCharacters;
		}

		internal struct TextData3D {
			public float3 center;
			public quaternion rotation;
			public LabelAlignment alignment;
			public float size;
			public int numCharacters;
		}

		/// <summary>Ensures the buffer has room for at least N more bytes</summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private void Reserve (int additionalSpace) {
			unsafe {
				if (Unity.Burst.CompilerServices.Hint.Unlikely(threadIndex >= 0)) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
					if (threadIndex < 0 || threadIndex >= JobsUtility.MaxJobThreadCount) throw new System.Exception("Thread index outside the expected range");
					if (threadIndex > 0 && uniqueID.isBuiltInCommandBuilder) throw new System.Exception("You should use a custom command builder when using the Unity Job System. Take a look at the documentation for more info.");
					if (buffer == null) throw new System.Exception("CommandBuilder does not have a valid buffer. Is it properly initialized?");

					// Exploit the fact that right after this package has drawn gizmos the buffers will be empty
					// and the next task is that Unity will render its own internal gizmos.
					// We can therefore easily (and without a high performance cost)
					// trap accidental Draw.* calls from OnDrawGizmos functions
					// by doing this check when the first Reserve call is made.
					AssertNotRendering();
#endif

					buffer += threadIndex;
					threadIndex = -1;
				}

				var newLength = buffer->Length + additionalSpace;
				if (newLength > buffer->Capacity) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
					// This really should run every time we access the buffer... but that would be a bit slow
					// This code will catch the error eventually.
					AssertBufferExists();
					const int MAX_BUFFER_SIZE = 1024 * 1024 * 256; // 256 MB
					if (buffer->Length * 2 > MAX_BUFFER_SIZE) {
						throw new System.Exception("CommandBuilder buffer is very large. Are you trying to draw things in an infinite loop?");
					}
#endif
					buffer->SetCapacity(math.max(newLength, buffer->Length * 2));
				}
			}
		}

		[BurstDiscard]
		private void AssertBufferExists () {
			if (!gizmos.IsAllocated || gizmos.Target == null || !(gizmos.Target as DrawingData).data.StillExists(uniqueID)) {
				// This command builder is invalid, clear all data on it to prevent it being used again
				this = default;
				throw new System.Exception("This command builder no longer exists. Are you trying to draw to a command builder which has already been disposed?");
			}
		}

		[BurstDiscard]
		static void AssertNotRendering () {
			// Some checking to see if drawing is being done from inside OnDrawGizmos
			// This check is relatively fast (about 0.05 ms), but we still do it only every 128th frame for performance reasons
			if (!GizmoContext.drawingGizmos && !JobsUtility.IsExecutingJob && (Time.renderedFrameCount & 127) == 0) {
				// Inspect the stack-trace to be able to provide more helpful error messages
				var st = StackTraceUtility.ExtractStackTrace();
				if (st.Contains("OnDrawGizmos")) {
					throw new System.Exception("You are trying to use Draw.* functions from within Unity's OnDrawGizmos function. Use this package's gizmo callbacks instead (see the documentation).");
				}
			}
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		internal void Reserve<A>() where A : struct {
			Reserve(UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<A>());
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		internal void Reserve<A, B>() where A : struct where B : struct {
			Reserve(UnsafeUtility.SizeOf<Command>() * 2 + UnsafeUtility.SizeOf<A>() + UnsafeUtility.SizeOf<B>());
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		internal void Reserve<A, B, C>() where A : struct where B : struct where C : struct {
			Reserve(UnsafeUtility.SizeOf<Command>() * 3 + UnsafeUtility.SizeOf<A>() + UnsafeUtility.SizeOf<B>() + UnsafeUtility.SizeOf<C>());
		}

		/// <summary>
		/// Converts a Color to a Color32.
		/// This method is faster than Unity's native color conversion, especially when using Burst.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		internal static unsafe uint ConvertColor (Color color) {
			// If SSE2 is supported (which it is on essentially all X86 CPUs)
			// then we can use a much faster conversion from Color to Color32.
			// This will only be possible inside Burst.
			if (Unity.Burst.Intrinsics.X86.Sse2.IsSse2Supported) {
				// Convert from 0-1 float range to 0-255 integer range
				var ci = (int4)(255 * new float4(color.r, color.g, color.b, color.a) + 0.5f);
				var v32 = new Unity.Burst.Intrinsics.v128(ci.x, ci.y, ci.z, ci.w);
				// Convert four 32-bit numbers to four 16-bit numbers
				var v16 = Unity.Burst.Intrinsics.X86.Sse2.packs_epi32(v32, v32);
				// Convert four 16-bit numbers to four 8-bit numbers
				var v8 = Unity.Burst.Intrinsics.X86.Sse2.packus_epi16(v16, v16);
				return v8.UInt0;
			} else {
				// If we don't have SSE2 (most likely we are not running inside Burst),
				// then we will do a manual conversion from Color to Color32.
				// This is significantly faster than just casting to a Color32.
				var r = (uint)Mathf.Clamp((int)(color.r*255f + 0.5f), 0, 255);
				var g = (uint)Mathf.Clamp((int)(color.g*255f + 0.5f), 0, 255);
				var b = (uint)Mathf.Clamp((int)(color.b*255f + 0.5f), 0, 255);
				var a = (uint)Mathf.Clamp((int)(color.a*255f + 0.5f), 0, 255);
				return (a << 24) | (b << 16) | (g << 8) | r;
			}
		}

		internal unsafe void Add<T>(T value) where T : struct {
			int num = UnsafeUtility.SizeOf<T>();
			var buffer = this.buffer;
			var bufferSize = buffer->Length;
			// We assume this because the Reserve function has already taken care of that.
			// This removes a few branches from the assembly when running in burst.
			Unity.Burst.CompilerServices.Hint.Assume(buffer->Ptr != null);
			Unity.Burst.CompilerServices.Hint.Assume(buffer->Ptr + bufferSize != null);

			unsafe {
				UnsafeUtility.CopyStructureToPtr(ref value, (void*)((byte*)buffer->Ptr + bufferSize));
				buffer->Length = bufferSize + num;
			}
		}

		public struct ScopeMatrix : IDisposable {
			internal CommandBuilder builder;
			public void Dispose () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (!builder.gizmos.IsAllocated || !(builder.gizmos.Target is DrawingData data) || !data.data.StillExists(builder.uniqueID)) throw new System.InvalidOperationException("The drawing instance this matrix scope belongs to no longer exists. Matrix scopes cannot survive for longer than a frame unless you have a custom drawing instance. Are you using a matrix scope inside a coroutine?");
#endif
				unsafe {
					builder.PopMatrix();
					builder.buffer = null;
				}
			}
		}

		public struct ScopeColor : IDisposable {
			internal CommandBuilder builder;
			public void Dispose () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (!builder.gizmos.IsAllocated || !(builder.gizmos.Target is DrawingData data) || !data.data.StillExists(builder.uniqueID)) throw new System.InvalidOperationException("The drawing instance this color scope belongs to no longer exists. Color scopes cannot survive for longer than a frame unless you have a custom drawing instance. Are you using a color scope inside a coroutine?");
#endif
				unsafe {
					builder.PopColor();
					builder.buffer = null;
				}
			}
		}

		public struct ScopePersist : IDisposable {
			internal CommandBuilder builder;
			public void Dispose () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (!builder.gizmos.IsAllocated || !(builder.gizmos.Target is DrawingData data) || !data.data.StillExists(builder.uniqueID)) throw new System.InvalidOperationException("The drawing instance this persist scope belongs to no longer exists. Persist scopes cannot survive for longer than a frame unless you have a custom drawing instance. Are you using a persist scope inside a coroutine?");
#endif
				unsafe {
					builder.PopDuration();
					builder.buffer = null;
				}
			}
		}

		/// <summary>
		/// Scope that does nothing.
		/// Used for optimization in standalone builds.
		/// </summary>
		public struct ScopeEmpty : IDisposable {
			public void Dispose () {
			}
		}

		public struct ScopeLineWidth : IDisposable {
			internal CommandBuilder builder;
			public void Dispose () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (!builder.gizmos.IsAllocated || !(builder.gizmos.Target is DrawingData data) || !data.data.StillExists(builder.uniqueID)) throw new System.InvalidOperationException("The drawing instance this line width scope belongs to no longer exists. Line width scopes cannot survive for longer than a frame unless you have a custom drawing instance. Are you using a line width scope inside a coroutine?");
#endif
				unsafe {
					builder.PopLineWidth();
					builder.buffer = null;
				}
			}
		}

		/// <summary>
		/// Scope to draw multiple things with an implicit matrix transformation.
		/// All coordinates for items drawn inside the scope will be multiplied by the matrix.
		/// If WithMatrix scopes are nested then coordinates are multiplied by all nested matrices in order.
		///
		/// <code>
		/// using (Draw.InLocalSpace(transform)) {
		///     // Draw a box at (0,0,0) relative to the current object
		///     // This means it will show up at the object's position
		///     Draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		///
		/// // Equivalent code using the lower level WithMatrix scope
		/// using (Draw.WithMatrix(transform.localToWorldMatrix)) {
		///     Draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		/// </code>
		///
		/// See: <see cref="InLocalSpace"/>
		/// </summary>
		[BurstDiscard]
		public ScopeMatrix WithMatrix (Matrix4x4 matrix) {
			PushMatrix(matrix);
			// TODO: Keep track of alive scopes and prevent dispose unless all scopes have been disposed
			unsafe {
				return new ScopeMatrix { builder = this };
			}
		}

		/// <summary>
		/// Scope to draw multiple things with an implicit matrix transformation.
		/// All coordinates for items drawn inside the scope will be multiplied by the matrix.
		/// If WithMatrix scopes are nested then coordinates are multiplied by all nested matrices in order.
		///
		/// <code>
		/// using (Draw.InLocalSpace(transform)) {
		///     // Draw a box at (0,0,0) relative to the current object
		///     // This means it will show up at the object's position
		///     Draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		///
		/// // Equivalent code using the lower level WithMatrix scope
		/// using (Draw.WithMatrix(transform.localToWorldMatrix)) {
		///     Draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		/// </code>
		///
		/// See: <see cref="InLocalSpace"/>
		/// </summary>
		[BurstDiscard]
		public ScopeMatrix WithMatrix (float3x3 matrix) {
			PushMatrix(new float4x4(matrix, float3.zero));
			// TODO: Keep track of alive scopes and prevent dispose unless all scopes have been disposed
			unsafe {
				return new ScopeMatrix { builder = this };
			}
		}

		/// <summary>
		/// Scope to draw multiple things with the same color.
		///
		/// <code>
		/// void Update () {
		///     using (Draw.WithColor(Color.red)) {
		///         Draw.Line(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
		///         Draw.Line(new Vector3(0, 0, 0), new Vector3(0, 1, 2));
		///     }
		/// }
		/// </code>
		///
		/// Any command that is passed an explicit color parameter will override this color.
		/// If another color scope is nested inside this one then that scope will override this color.
		/// </summary>
		[BurstDiscard]
		public ScopeColor WithColor (Color color) {
			PushColor(color);
			unsafe {
				return new ScopeColor { builder = this };
			}
		}

		/// <summary>
		/// Scope to draw multiple things for a longer period of time.
		///
		/// Normally drawn items will only be rendered for a single frame.
		/// Using a persist scope you can make the items be drawn for any amount of time.
		///
		/// <code>
		/// void Update () {
		///     using (Draw.WithDuration(1.0f)) {
		///         var offset = Time.time;
		///         Draw.Line(new Vector3(offset, 0, 0), new Vector3(offset, 0, 1));
		///     }
		/// }
		/// </code>
		///
		/// Note: Outside of play mode the duration is measured against Unity's Time.realtimeSinceStartup.
		///
		/// Warning: It is recommended not to use this inside a DrawGizmos callback since DrawGizmos is called every frame anyway.
		/// </summary>
		/// <param name="duration">How long the drawn items should persist in seconds.</param>

		[BurstDiscard]
		public ScopePersist WithDuration (float duration) {
			PushDuration(duration);
			unsafe {
				return new ScopePersist { builder = this };
			}
		}

		/// <summary>
		/// Scope to draw multiple things with a given line width.
		///
		/// Note that the line join algorithm is a quite simple one optimized for speed. It normally looks good on a 2D plane, but if the polylines curve a lot in 3D space then
		/// it can look odd from some angles.
		///
		/// [Open online documentation to see images]
		///
		/// In the picture the top row has automaticJoins enabled and in the bottom row it is disabled.
		/// </summary>
		/// <param name="pixels">Line width in pixels</param>
		/// <param name="automaticJoins">If true then sequences of lines that are adjacent will be automatically joined at their vertices. This typically produces nicer polylines without weird gaps.</param>
		[BurstDiscard]
		public ScopeLineWidth WithLineWidth (float pixels, bool automaticJoins = true) {
			PushLineWidth(pixels, automaticJoins);
			unsafe {
				return new ScopeLineWidth { builder = this };
			}
		}

		/// <summary>
		/// Scope to draw multiple things relative to a transform object.
		/// All coordinates for items drawn inside the scope will be multiplied by the transform's localToWorldMatrix.
		///
		/// <code>
		/// void Update () {
		///     using (Draw.InLocalSpace(transform)) {
		///         // Draw a box at (0,0,0) relative to the current object
		///         // This means it will show up at the object's position
		///         // The box is also rotated and scaled with the transform
		///         Draw.WireBox(Vector3.zero, Vector3.one);
		///     }
		/// }
		/// </code>
		///
		/// [Open online documentation to see videos]
		/// </summary>
		[BurstDiscard]
		public ScopeMatrix InLocalSpace (Transform transform) {
			return WithMatrix(transform.localToWorldMatrix);
		}

		/// <summary>
		/// Scope to draw multiple things in screen space of a camera.
		/// If you draw 2D coordinates (i.e. (x,y,0)) they will be projected onto a plane approximately [2*near clip plane of the camera] world units in front of the camera (but guaranteed to be between the near and far planes).
		///
		/// The lower left corner of the camera is (0,0,0) and the upper right is (camera.pixelWidth, camera.pixelHeight, 0)
		///
		/// Note: As a corollary, the centers of pixels are offset by 0.5. So for example the center of the top left pixel is at (0.5, 0.5, 0).
		/// Therefore, if you want to draw 1 pixel wide lines in screen space, you may want to offset the coordinates by 0.5 pixels.
		///
		/// See: <see cref="InLocalSpace"/>
		/// See: <see cref="WithMatrix"/>
		/// </summary>
		[BurstDiscard]
		public ScopeMatrix InScreenSpace (Camera camera) {
			return WithMatrix(camera.cameraToWorldMatrix * camera.nonJitteredProjectionMatrix.inverse * Matrix4x4.TRS(new Vector3(-1.0f, -1.0f, 0), Quaternion.identity, new Vector3(2.0f/camera.pixelWidth, 2.0f/camera.pixelHeight, 1)));
		}

		/// <summary>
		/// Multiply all coordinates until the next <see cref="PopMatrix"/> with the given matrix.
		///
		/// PushMatrix and PushSetMatrix are slightly different:
		///
		/// - PushMatrix stacks with all previously pushed matrices. The active matrix becomes the product of the given matrix and the previously active one.
		/// - PushSetMatrix sets the current matrix directly. The active matrix becomes the last pushed matrix.
		/// </summary>
		public void PushMatrix (Matrix4x4 matrix) {
			Reserve<float4x4>();
			Add(Command.PushMatrix);
			Add(matrix);
		}

		/// <summary>
		/// Multiply all coordinates until the next <see cref="PopMatrix"/> with the given matrix.
		///
		/// PushMatrix and PushSetMatrix are slightly different:
		///
		/// - PushMatrix stacks with all previously pushed matrices. The active matrix becomes the product of the given matrix and the previously active one.
		/// - PushSetMatrix sets the current matrix directly. The active matrix becomes the last pushed matrix.
		/// </summary>
		public void PushMatrix (float4x4 matrix) {
			Reserve<float4x4>();
			Add(Command.PushMatrix);
			Add(matrix);
		}

		/// <summary>
		/// Multiply all coordinates until the next <see cref="PopMatrix"/> with the given matrix.
		///
		/// PushMatrix and PushSetMatrix are slightly different:
		///
		/// - PushMatrix stacks with all previously pushed matrices. The active matrix becomes the product of the given matrix and the previously active one.
		/// - PushSetMatrix sets the current matrix directly. The active matrix becomes the last pushed matrix.
		/// </summary>
		public void PushSetMatrix (Matrix4x4 matrix) {
			Reserve<float4x4>();
			Add(Command.PushSetMatrix);
			Add((float4x4)matrix);
		}

		/// <summary>
		/// Multiply all coordinates until the next PopMatrix with the given matrix.
		///
		/// PushMatrix and PushSetMatrix are slightly different:
		///
		/// - PushMatrix stacks with all previously pushed matrices. The active matrix becomes the product of the given matrix and the previously active one.
		/// - PushSetMatrix sets the current matrix directly. The active matrix becomes the last pushed matrix.
		/// </summary>
		public void PushSetMatrix (float4x4 matrix) {
			Reserve<float4x4>();
			Add(Command.PushSetMatrix);
			Add(matrix);
		}

		/// <summary>
		/// Pops a matrix from the stack.
		///
		/// See: <see cref="PushMatrix"/>
		/// See: <see cref="PushSetMatrix"/>
		/// </summary>
		public void PopMatrix () {
			Reserve(4);
			Add(Command.PopMatrix);
		}

		/// <summary>
		/// Draws everything until the next PopColor with the given color.
		/// Any command that is passed an explicit color parameter will override this color.
		/// If another color scope is nested inside this one then that scope will override this color.
		/// </summary>
		public void PushColor (Color color) {
			Reserve<Color32>();
			Add(Command.PushColor);
			Add(ConvertColor(color));
		}

		/// <summary>Pops a color from the stack</summary>
		public void PopColor () {
			Reserve(4);
			Add(Command.PopColor);
		}

		/// <summary>
		/// Draws everything until the next PopDuration for a number of seconds.
		/// Warning: This is not recommended inside a DrawGizmos callback since DrawGizmos is called every frame anyway.
		/// </summary>
		public void PushDuration (float duration) {
			Reserve<PersistData>();
			Add(Command.PushPersist);
			// We must use the BurstTime variable which is updated more rarely than Time.time.
			// This is necessary because this code may be called from a burst job or from a different thread.
			// Time.time can only be accessed in the main thread.
			Add(new PersistData { endTime = SharedDrawingData.BurstTime.Data + duration });
		}

		/// <summary>Pops a duration scope from the stack</summary>
		public void PopDuration () {
			Reserve(4);
			Add(Command.PopPersist);
		}

		/// <summary>
		/// Draws everything until the next PopPersist for a number of seconds.
		/// Warning: This is not recommended inside a DrawGizmos callback since DrawGizmos is called every frame anyway.
		///
		/// Deprecated: Renamed to <see cref="PushDuration"/>
		/// </summary>
		[System.Obsolete("Renamed to PushDuration for consistency")]
		public void PushPersist (float duration) {
			PushDuration(duration);
		}

		/// <summary>
		/// Pops a persist scope from the stack.
		/// Deprecated: Renamed to <see cref="PopDuration"/>
		/// </summary>
		[System.Obsolete("Renamed to PopDuration for consistency")]
		public void PopPersist () {
			PopDuration();
		}

		/// <summary>
		/// Draws all lines until the next PopLineWidth with a given line width in pixels.
		///
		/// Note that the line join algorithm is a quite simple one optimized for speed. It normally looks good on a 2D plane, but if the polylines curve a lot in 3D space then
		/// it can look odd from some angles.
		///
		/// [Open online documentation to see images]
		///
		/// In the picture the top row has automaticJoins enabled and in the bottom row it is disabled.
		/// </summary>
		/// <param name="pixels">Line width in pixels</param>
		/// <param name="automaticJoins">If true then sequences of lines that are adjacent will be automatically joined at their vertices. This typically produces nicer polylines without weird gaps.</param>
		public void PushLineWidth (float pixels, bool automaticJoins = true) {
			if (pixels < 0) throw new System.ArgumentOutOfRangeException("pixels", "Line width must be positive");

			Reserve<LineWidthData>();
			Add(Command.PushLineWidth);
			Add(new LineWidthData { pixels = pixels, automaticJoins = automaticJoins });
		}

		/// <summary>Pops a line width scope from the stack</summary>
		public void PopLineWidth () {
			Reserve(4);
			Add(Command.PopLineWidth);
		}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (float3 a, float3 b) {
			Reserve<LineData>();
			Add(Command.Line);
			Add(new LineData { a = a, b = b });
		}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (Vector3 a, Vector3 b) {
			Reserve<LineData>();
			// Add(Command.Line);
			// Add(new LineDataV3 { a = a, b = b });

			// The code below is equivalent to the commented out code above.
			// But drawing lines is the most common operation so it needs to be really fast.
			// Having this hardcoded improves line rendering performance by about 8%.
			var bufferSize = BufferSize;

			unsafe {
				var newLen = bufferSize + 4 + 24;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				UnityEngine.Assertions.Assert.IsTrue(newLen <= buffer->Capacity);
#endif
				var ptr = (byte*)buffer->Ptr + bufferSize;
				*(Command*)ptr = Command.Line;
				var lineData = (LineDataV3*)(ptr + 4);
				lineData->a = a;
				lineData->b = b;
				buffer->Length = newLen;
			}
		}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (Vector3 a, Vector3 b, Color color) {
			Reserve<Color32, LineData>();
			// Add(Command.Line | Command.PushColorInline);
			// Add(ConvertColor(color));
			// Add(new LineDataV3 { a = a, b = b });

			// The code below is equivalent to the code which is commented out above.
			// But drawing lines is the most common operation so it needs to be really fast
			// Having this hardcoded improves line rendering performance by about 8%.
			var bufferSize = BufferSize;

			unsafe {
				var newLen = bufferSize + 4 + 24 + 4;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				UnityEngine.Assertions.Assert.IsTrue(newLen <= buffer->Capacity);
#endif
				var ptr = (byte*)buffer->Ptr + bufferSize;
				*(Command*)ptr = Command.Line | Command.PushColorInline;
				*(uint*)(ptr + 4) = ConvertColor(color);
				var lineData = (LineDataV3*)(ptr + 8);
				lineData->a = a;
				lineData->b = b;
				buffer->Length = newLen;
			}
		}

		/// <summary>
		/// Draws a ray starting at a point and going in the given direction.
		/// The ray will end at origin + direction.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// Draw.Ray(Vector3.zero, Vector3.up);
		/// </code>
		/// </summary>
		public void Ray (float3 origin, float3 direction) {
			Line(origin, origin + direction);
		}

		/// <summary>
		/// Draws a ray with a given length.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// Draw.Ray(Camera.main.ScreenPointToRay(Vector3.zero), 10);
		/// </code>
		/// </summary>
		public void Ray (Ray ray, float length) {
			Line(ray.origin, ray.origin + ray.direction * length);
		}

		/// <summary>
		/// Draws an arc between two points.
		///
		/// The rendered arc is the shortest arc between the two points.
		/// The radius of the arc will be equal to the distance between center and start.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// float a1 = Mathf.PI*0.9f;
		/// float a2 = Mathf.PI*0.1f;
		/// var arcStart = new float3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
		/// var arcEnd = new float3(Mathf.Cos(a2), 0, Mathf.Sin(a2));
		/// Draw.Arc(new float3(0, 0, 0), arcStart, arcEnd, color);
		/// </code>
		///
		/// See: <see cref="CommandBuilder2D.Circle(float3,float,float,float)"/>
		/// </summary>
		/// <param name="center">Center of the imaginary circle that the arc is part of.</param>
		/// <param name="start">Starting point of the arc.</param>
		/// <param name="end">End point of the arc.</param>
		public void Arc (float3 center, float3 start, float3 end) {
			var d1 = start - center;
			var d2 = end - center;
			var normal = math.cross(d2, d1);

			if (math.any(normal != 0) && math.all(math.isfinite(normal))) {
				var m = Matrix4x4.TRS(center, Quaternion.LookRotation(d1, normal), Vector3.one);
				var angle = Vector3.SignedAngle(d1, d2, normal) * Mathf.Deg2Rad;
				PushMatrix(m);
				CircleXZInternal(float3.zero, math.length(d1), 90 * Mathf.Deg2Rad, 90 * Mathf.Deg2Rad - angle);
				PopMatrix();
			}
		}

		/// <summary>
		/// Draws a circle in the XZ plane.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="CommandBuilder.Circle(float3,float3,float)"/>
		/// See: <see cref="CircleXY(float3,float,float,float)"/>
		/// See: <see cref="Arc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the circle or arc.</param>
		/// <param name="radius">Radius of the circle or arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		[System.Obsolete("Use Draw.xz.Circle instead")]
		public void CircleXZ (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			CircleXZInternal(center, radius, startAngle, endAngle);
		}

		internal void CircleXZInternal (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			Reserve<CircleXZData>();
			Add(Command.CircleXZ);
			Add(new CircleXZData { center = center, radius = radius, startAngle = startAngle, endAngle = endAngle });
		}

		internal void CircleXZInternal (float3 center, float radius, float startAngle, float endAngle, Color color) {
			Reserve<Color32, CircleXZData>();
			Add(Command.CircleXZ | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new CircleXZData { center = center, radius = radius, startAngle = startAngle, endAngle = endAngle });
		}

		internal static readonly float4x4 XZtoXYPlaneMatrix = float4x4.RotateX(-math.PI*0.5f);
		internal static readonly float4x4 XZtoYZPlaneMatrix = float4x4.RotateZ(math.PI*0.5f);

		/// <summary>
		/// Draws a circle in the XY plane.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="CommandBuilder.Circle(float3,float3,float)"/>
		/// See: <see cref="Arc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the circle or arc.</param>
		/// <param name="radius">Radius of the circle or arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public void CircleXY (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			PushMatrix(XZtoXYPlaneMatrix);
			CircleXZ(new float3(center.x, -center.z, center.y), radius, startAngle, endAngle);
			PopMatrix();
		}

		/// <summary>
		/// Draws a circle.
		///
		/// [Open online documentation to see images]
		///
		/// Note: This overload does not allow you to draw an arc. For that purpose use <see cref="Arc"/>, <see cref="CircleXY"/> or <see cref="CircleXZ"/> instead.
		/// </summary>
		public void Circle (float3 center, float3 normal, float radius) {
			Reserve<CircleData>();
			Add(Command.Circle);
			Add(new CircleData { center = center, normal = normal, radius = radius });
		}

		/// <summary>
		/// Draws a solid arc between two points.
		///
		/// The rendered arc is the shortest arc between the two points.
		/// The radius of the arc will be equal to the distance between center and start.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// float a1 = Mathf.PI*0.9f;
		/// float a2 = Mathf.PI*0.1f;
		/// var arcStart = new float3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
		/// var arcEnd = new float3(Mathf.Cos(a2), 0, Mathf.Sin(a2));
		/// Draw.SolidArc(new float3(0, 0, 0), arcStart, arcEnd, color);
		/// </code>
		///
		/// See: <see cref="CommandBuilder2D.SolidCircle(float3,float,float,float)"/>
		/// </summary>
		/// <param name="center">Center of the imaginary circle that the arc is part of.</param>
		/// <param name="start">Starting point of the arc.</param>
		/// <param name="end">End point of the arc.</param>
		public void SolidArc (float3 center, float3 start, float3 end) {
			var d1 = start - center;
			var d2 = end - center;
			var normal = math.cross(d2, d1);

			if (math.any(normal)) {
				var m = Matrix4x4.TRS(center, Quaternion.LookRotation(d1, normal), Vector3.one);
				var angle = Vector3.SignedAngle(d1, d2, normal) * Mathf.Deg2Rad;
				PushMatrix(m);
				SolidCircleXZInternal(float3.zero, math.length(d1), 90 * Mathf.Deg2Rad, 90 * Mathf.Deg2Rad - angle);
				PopMatrix();
			}
		}

		/// <summary>
		/// Draws a disc in the XZ plane.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="SolidCircle(float3,float3,float)"/>
		/// See: <see cref="CommandBuilder2D.SolidCircle(float3,float,float,float)"/>
		/// See: <see cref="SolidArc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the disc or solid arc.</param>
		/// <param name="radius">Radius of the disc or solid arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		[System.Obsolete("Use Draw.xz.SolidCircle instead")]
		public void SolidCircleXZ (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			SolidCircleXZInternal(center, radius, startAngle, endAngle);
		}

		internal void SolidCircleXZInternal (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			Reserve<CircleXZData>();
			Add(Command.DiscXZ);
			Add(new CircleXZData { center = center, radius = radius, startAngle = startAngle, endAngle = endAngle });
		}

		internal void SolidCircleXZInternal (float3 center, float radius, float startAngle, float endAngle, Color color) {
			Reserve<Color32, CircleXZData>();
			Add(Command.DiscXZ | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new CircleXZData { center = center, radius = radius, startAngle = startAngle, endAngle = endAngle });
		}

		/// <summary>
		/// Draws a disc in the XY plane.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="SolidCircle(float3,float3,float)"/>
		/// See: <see cref="CommandBuilder2D.SolidCircle(float3,float,float,float)"/>
		/// See: <see cref="SolidArc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the disc or solid arc.</param>
		/// <param name="radius">Radius of the disc or solid arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		[System.Obsolete("Use Draw.xy.SolidCircle instead")]
		public void SolidCircleXY (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			PushMatrix(XZtoXYPlaneMatrix);
			SolidCircleXZInternal(new float3(center.x, -center.z, center.y), radius, startAngle, endAngle);
			PopMatrix();
		}

		/// <summary>
		/// Draws a disc.
		///
		/// [Open online documentation to see images]
		///
		/// Note: This overload does not allow you to draw an arc. For that purpose use <see cref="SolidArc"/> or <see cref="CommandBuilder2D.SolidCircle(float3,float,float,float)"/> instead.
		/// </summary>
		public void SolidCircle (float3 center, float3 normal, float radius) {
			Reserve<CircleData>();
			Add(Command.Disc);
			Add(new CircleData { center = center, normal = normal, radius = radius });
		}

		/// <summary>
		/// Draws a circle outline around a sphere.
		///
		/// Visually, this is a circle that always faces the camera, and is resized automatically to fit the sphere.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void SphereOutline (float3 center, float radius) {
			Reserve<SphereData>();
			Add(Command.SphereOutline);
			Add(new SphereData { center = center, radius = radius });
		}

		/// <summary>
		/// Draws a cylinder.
		/// The cylinder's bottom circle will be centered at the bottom parameter and similarly for the top circle.
		///
		/// <code>
		/// // Draw a tilted cylinder between the points (0,0,0) and (1,1,1) with a radius of 0.5
		/// Draw.WireCylinder(Vector3.zero, Vector3.one, 0.5f, Color.black);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void WireCylinder (float3 bottom, float3 top, float radius) {
			WireCylinder(bottom, top - bottom, math.length(top - bottom), radius);
		}

		/// <summary>
		/// Draws a cylinder.
		///
		/// <code>
		/// // Draw a two meter tall cylinder at the world origin with a radius of 0.5
		/// Draw.WireCylinder(Vector3.zero, Vector3.up, 2, 0.5f, Color.black);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="position">The center of the cylinder's "bottom" circle.</param>
		/// <param name="up">The cylinder's main axis. Does not have to be normalized. If zero, nothing will be drawn.</param>
		/// <param name="height">The length of the cylinder, as measured along it's main axis.</param>
		/// <param name="radius">The radius of the cylinder.</param>
		public void WireCylinder (float3 position, float3 up, float height, float radius) {
			up = math.normalizesafe(up);
			if (math.all(up == 0) || math.any(math.isnan(up)) || math.isnan(height) || math.isnan(radius)) return;

			OrthonormalBasis(up, out var basis1, out var basis2);

			PushMatrix(new float4x4(
				new float4(basis1 * radius, 0),
				new float4(up * height, 0),
				new float4(basis2 * radius, 0),
				new float4(position, 1)
				));

			CircleXZInternal(float3.zero, 1);
			if (height > 0) {
				CircleXZInternal(new float3(0, 1, 0), 1);
				Line(new float3(1, 0, 0), new float3(1, 1, 0));
				Line(new float3(-1, 0, 0), new float3(-1, 1, 0));
				Line(new float3(0, 0, 1), new float3(0, 1, 1));
				Line(new float3(0, 0, -1), new float3(0, 1, -1));
			}
			PopMatrix();
		}

		/// <summary>
		/// Constructs an orthonormal basis from a single normal vector.
		///
		/// This is similar to math.orthonormal_basis, but it tries harder to be continuous in its input.
		/// In contrast, math.orthonormal_basis has a tendency to jump around even with small changes to the normal.
		///
		/// It's not as fast as math.orthonormal_basis, though.
		/// </summary>
		static void OrthonormalBasis (float3 normal, out float3 basis1, out float3 basis2) {
			basis1 = math.cross(normal, new float3(1, 1, 1));
			if (math.all(basis1 == 0)) basis1 = math.cross(normal, new float3(-1, 1, 1));
			basis1 = math.normalizesafe(basis1);
			basis2 = math.cross(normal, basis1);
		}

		/// <summary>
		/// Draws a capsule with a (start,end) parameterization.
		///
		/// The behavior of this method matches common Unity APIs such as Physics.CheckCapsule.
		///
		/// <code>
		/// // Draw a tilted capsule between the points (0,0,0) and (1,1,1) with a radius of 0.5
		/// Draw.WireCapsule(Vector3.zero, Vector3.one, 0.5f, Color.black);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="start">Center of the start hemisphere of the capsule.</param>
		/// <param name="end">Center of the end hemisphere of the capsule.</param>
		/// <param name="radius">Radius of the capsule.</param>
		public void WireCapsule (float3 start, float3 end, float radius) {
			var dir = end - start;
			var length = math.length(dir);

			if (length < 0.0001) {
				// The endpoints are the same, we can't draw a capsule from this because we don't know its orientation.
				// Draw a sphere as a fallback
				WireSphere(start, radius);
			} else {
				var normalized_dir = dir / length;

				WireCapsule(start - normalized_dir*radius, normalized_dir, length + 2*radius, radius);
			}
		}

		// TODO: Change to center, up, height parameterization
		/// <summary>
		/// Draws a capsule with a (position,direction/length) parameterization.
		///
		/// <code>
		/// // Draw a capsule that touches the y=0 plane, is 2 meters tall and has a radius of 0.5
		/// Draw.WireCapsule(Vector3.zero, Vector3.up, 2.0f, 0.5f, Color.black);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="position">One endpoint of the capsule. This is at the edge of the capsule, not at the center of one of the hemispheres.</param>
		/// <param name="direction">The main axis of the capsule. Does not have to be normalized. If zero, nothing will be drawn.</param>
		/// <param name="length">Distance between the two endpoints of the capsule. The length will be clamped to be at least 2*radius.</param>
		/// <param name="radius">The radius of the capsule.</param>
		public void WireCapsule (float3 position, float3 direction, float length, float radius) {
			direction = math.normalizesafe(direction);
			if (math.all(direction == 0) || math.any(math.isnan(direction)) || math.isnan(length) || math.isnan(radius)) return;

			if (radius <= 0) {
				Line(position, position + direction * length);
			} else {
				length = math.max(length, radius*2);
				OrthonormalBasis(direction, out var basis1, out var basis2);

				PushMatrix(new float4x4(
					new float4(basis1, 0),
					new float4(direction, 0),
					new float4(basis2, 0),
					new float4(position, 1)
					));
				CircleXZInternal(new float3(0, radius, 0), radius);
				PushMatrix(XZtoXYPlaneMatrix);
				CircleXZInternal(new float3(0, 0, radius), radius, Mathf.PI, 2 * Mathf.PI);
				PopMatrix();
				PushMatrix(XZtoYZPlaneMatrix);
				CircleXZInternal(new float3(radius, 0, 0), radius, Mathf.PI*0.5f, Mathf.PI*1.5f);
				PopMatrix();
				if (length > 0) {
					var upperY = length - radius;
					var lowerY = radius;
					CircleXZInternal(new float3(0, upperY, 0), radius);
					PushMatrix(XZtoXYPlaneMatrix);
					CircleXZInternal(new float3(0, 0, upperY), radius, 0, Mathf.PI);
					PopMatrix();
					PushMatrix(XZtoYZPlaneMatrix);
					CircleXZInternal(new float3(upperY, 0, 0), radius, -Mathf.PI*0.5f, Mathf.PI*0.5f);
					PopMatrix();
					Line(new float3(radius, lowerY, 0), new float3(radius, upperY, 0));
					Line(new float3(-radius, lowerY, 0), new float3(-radius, upperY, 0));
					Line(new float3(0, lowerY, radius), new float3(0, upperY, radius));
					Line(new float3(0, lowerY, -radius), new float3(0, upperY, -radius));
				}
				PopMatrix();
			}
		}

		/// <summary>
		/// Draws a wire sphere.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// // Draw a wire sphere at the origin with a radius of 0.5
		/// Draw.WireSphere(Vector3.zero, 0.5f, Color.black);
		/// </code>
		///
		/// See: <see cref="Circle"/>
		/// </summary>
		public void WireSphere (float3 position, float radius) {
			SphereOutline(position, radius);
			Circle(position, new float3(1, 0, 0), radius);
			Circle(position, new float3(0, 1, 0), radius);
			Circle(position, new float3(0, 0, 1), radius);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		[BurstDiscard]
		public void Polyline (List<Vector3> points, bool cycle = false) {
			for (int i = 0; i < points.Count - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Count > 1) Line(points[points.Count - 1], points[0]);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		public void Polyline<T>(T points, bool cycle = false) where T : IReadOnlyList<float3> {
			for (int i = 0; i < points.Count - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Count > 1) Line(points[points.Count - 1], points[0]);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		[BurstDiscard]
		public void Polyline (Vector3[] points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		[BurstDiscard]
		public void Polyline (float3[] points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		public void Polyline (NativeArray<float3> points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>Determines the symbol to use for <see cref="PolylineWithSymbol"/></summary>
		public enum SymbolDecoration : byte {
			/// <summary>
			/// No symbol.
			///
			/// Space will still be reserved, but no symbol will be drawn.
			/// Can be used to draw dashed lines.
			///
			/// [Open online documentation to see images]
			/// </summary>
			None,
			/// <summary>
			/// An arrowhead symbol.
			///
			/// [Open online documentation to see images]
			/// </summary>
			ArrowHead,
			/// <summary>
			/// A circle symbol.
			///
			/// [Open online documentation to see images]
			/// </summary>
			Circle,
		}

		/// <summary>
		/// Draws a dashed line between two points.
		///
		/// <code>
		/// Draw.DashedPolyline(points, 0.1f, 0.1f, color);
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// Warning: An individual line segment is drawn for each dash. This means that performance may suffer if you make the dash + gap distance too small.
		/// But for most use cases the performance is nothing to worry about.
		///
		/// See: <see cref="DashedPolyline"/>
		/// See: <see cref="PolylineWithSymbol"/>
		/// </summary>
		public void DashedLine (float3 a, float3 b, float dash, float gap) {
			var p = new PolylineWithSymbol(SymbolDecoration.None, gap, 0, dash + gap);
			p.MoveTo(ref this, a);
			p.MoveTo(ref this, b);
		}

		/// <summary>
		/// Draws a dashed line through a sequence of points.
		///
		/// <code>
		/// Draw.DashedPolyline(points, 0.1f, 0.1f, color);
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// Warning: An individual line segment is drawn for each dash. This means that performance may suffer if you make the dash + gap distance too small.
		/// But for most use cases the performance is nothing to worry about.
		///
		/// If you have a different collection type, or you do not have the points in a collection at all, then you can use the <see cref="PolylineWithSymbol"/> struct directly.
		///
		/// <code>
		/// using (Draw.WithColor(color)) {
		///     var dash = 0.1f;
		///     var gap = 0.1f;
		///     var p = new CommandBuilder.PolylineWithSymbol(CommandBuilder.SymbolDecoration.None, gap, 0, dash + gap);
		///     for (int i = 0; i < points.Count; i++) {
		///         p.MoveTo(ref Draw.editor, points[i]);
		///     }
		/// }
		/// </code>
		///
		/// See: <see cref="DashedLine"/>
		/// See: <see cref="PolylineWithSymbol"/>
		/// </summary>
		public void DashedPolyline (List<Vector3> points, float dash, float gap) {
			var p = new PolylineWithSymbol(SymbolDecoration.None, gap, 0, dash + gap);
			for (int i = 0; i < points.Count; i++) {
				p.MoveTo(ref this, points[i]);
			}
		}

		/// <summary>
		/// Helper for drawing a polyline with symbols at regular intervals.
		///
		/// <code>
		/// var generator = new CommandBuilder.PolylineWithSymbol(CommandBuilder.SymbolDecoration.Circle, 0.2f, 0.0f, 0.47f);
		/// generator.MoveTo(ref Draw.editor, new float3(-0.5f, 0, -0.5f));
		/// generator.MoveTo(ref Draw.editor, new float3(0.5f, 0, 0.5f));
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// [Open online documentation to see images]
		///
		/// You can also draw a dashed line using this struct, but for common cases you can use the <see cref="DashedPolyline"/> helper function instead.
		///
		/// <code>
		/// using (Draw.WithColor(color)) {
		///     var dash = 0.1f;
		///     var gap = 0.1f;
		///     var p = new CommandBuilder.PolylineWithSymbol(CommandBuilder.SymbolDecoration.None, gap, 0, dash + gap);
		///     for (int i = 0; i < points.Count; i++) {
		///         p.MoveTo(ref Draw.editor, points[i]);
		///     }
		/// }
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		public struct PolylineWithSymbol {
			float3 prev;
			float offset;
			readonly float symbolSize;
			readonly float connectingSegmentLength;
			readonly float symbolPadding;
			readonly float symbolOffset;

			/// <summary>
			/// The up direction of the symbols.
			///
			/// This is used to determine the orientation of the symbols.
			/// By default this is set to (0,1,0).
			/// </summary>
			public float3 up;

			readonly SymbolDecoration symbol;
			State state;
			readonly bool reverseSymbols;

			enum State : byte {
				NotStarted,
				ConnectingSegment,
				PreSymbolPadding,
				Symbol,
				PostSymbolPadding,
			}

			/// <summary>
			/// Create a new polyline with symbol generator.
			///
			/// Note: If symbolSize + 2*symbolPadding > symbolSpacing, the symbolSpacing parameter will be increased to accommodate the symbol and its padding.
			/// There will be no connecting lines between the symbols in this case, as there's no space for them.
			/// </summary>
			/// <param name="symbol">The symbol to use</param>
			/// <param name="symbolSize">The size of the symbol. In case of a circle, this is the diameter.</param>
			/// <param name="symbolPadding">The padding on both sides of the symbol between the symbol and the line.</param>
			/// <param name="symbolSpacing">The spacing between symbols. This is the distance between the centers of the symbols.</param>
			/// <param name="reverseSymbols">If true, the symbols will be reversed. For cicles this has no effect, but arrowhead symbols will be reversed.</param>
			/// <param name="offset">Distance to shift all symbols forward along the line. Useful for animations. If offset=0, the first symbol's center is at symbolSpacing/2.</param>
			public PolylineWithSymbol(SymbolDecoration symbol, float symbolSize, float symbolPadding, float symbolSpacing, bool reverseSymbols = false, float offset = 0) {
				if (symbolSpacing <= math.FLT_MIN_NORMAL) throw new System.ArgumentOutOfRangeException(nameof(symbolSpacing), "Symbol spacing must be greater than zero");
				if (symbolSize <= math.FLT_MIN_NORMAL) throw new System.ArgumentOutOfRangeException(nameof(symbolSize), "Symbol size must be greater than zero");
				if (symbolPadding < 0) throw new System.ArgumentOutOfRangeException(nameof(symbolPadding), "Symbol padding must non-negative");

				this.prev = float3.zero;
				this.symbol = symbol;
				this.symbolSize = symbolSize;
				this.symbolPadding = symbolPadding;
				this.connectingSegmentLength = math.max(0, symbolSpacing - symbolPadding * 2f - symbolSize);
				// Calculate actual value, after clamping to a valid range
				symbolSpacing = symbolPadding * 2 + symbolSize + connectingSegmentLength;
				this.reverseSymbols = reverseSymbols;
				this.up = new float3(0, 1, 0);
				symbolOffset = symbol == SymbolDecoration.ArrowHead ? -0.25f * symbolSize : 0;
				if (reverseSymbols) {
					symbolOffset = -symbolOffset;
				}
				symbolOffset += 0.5f * symbolSize;
				this.offset = (this.connectingSegmentLength * 0.5f + offset) % symbolSpacing;
				// Ensure the initial offset is always negative. This makes the state machine start in the correct state when the offset turns positive.
				if (this.offset > 0) this.offset -= symbolSpacing;
				this.state = State.NotStarted;
			}

			/// <summary>
			/// Move to a new point.
			///
			/// This will draw the symbols and line segments between the previous point and the new point.
			/// </summary>
			/// <param name="draw">The command builder to draw to. You can use a built-in builder like \reflink{Draw.editor} or \reflink{Draw.ingame}, or use a custom one.</param>
			/// <param name="next">The next point in the polyline to move to.</param>
			public void MoveTo (ref CommandBuilder draw, float3 next) {
				if (state == State.NotStarted) {
					prev = next;
					state = State.ConnectingSegment;
					return;
				}

				var len = math.length(next - prev);
				var invLen = math.rcp(len);
				var dir = next - prev;
				float3 up = default;
				if (symbol != SymbolDecoration.None) {
					up = math.normalizesafe(math.cross(dir, math.cross(dir, this.up)));
					if (math.all(up == 0f)) {
						up = new float3(0, 0, 1);
					}
					if (reverseSymbols) dir = -dir;
				}

				var currentPositionOnSegment = 0f;
				while (true) {
					if (state == State.ConnectingSegment) {
						if (offset >= 0 && offset != currentPositionOnSegment) {
							currentPositionOnSegment = math.max(0, currentPositionOnSegment);
							var pLast = math.lerp(prev, next, currentPositionOnSegment * invLen);
							var p = math.lerp(prev, next, math.min(offset * invLen, 1));
							draw.Line(pLast, p);
						}

						if (offset < len) {
							state = State.PreSymbolPadding;
							currentPositionOnSegment = offset;
							offset += symbolPadding;
						} else {
							break;
						}
					} else if (state == State.PreSymbolPadding) {
						if (offset >= len) break;

						state = State.Symbol;
						currentPositionOnSegment = offset;
						offset += symbolOffset;
					} else if (state == State.Symbol) {
						if (offset >= len) break;

						if (offset >= 0) {
							var p = math.lerp(prev, next, offset * invLen);
							switch (symbol) {
							case SymbolDecoration.None:
								break;
							case SymbolDecoration.ArrowHead:
								draw.Arrowhead(p, dir, up, symbolSize);
								break;
							case SymbolDecoration.Circle:
							default:
								draw.Circle(p, up, symbolSize * 0.5f);
								break;
							}
						}

						state = State.PostSymbolPadding;
						currentPositionOnSegment = offset;
						offset += -symbolOffset + symbolSize + symbolPadding;
					} else if (state == State.PostSymbolPadding) {
						if (offset >= len) break;

						state = State.ConnectingSegment;
						currentPositionOnSegment = offset;
						offset += connectingSegmentLength;
					} else {
						throw new System.Exception("Invalid state");
					}
				}
				offset -= len;
				prev = next;
			}
		}

		/// <summary>
		/// Draws the outline of a box which is axis-aligned.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void WireBox (float3 center, float3 size) {
			Reserve<BoxData>();
			Add(Command.WireBox);
			Add(new BoxData { center = center, size = size });
		}

		/// <summary>
		/// Draws the outline of a box.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="rotation">Rotation of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void WireBox (float3 center, quaternion rotation, float3 size) {
			PushMatrix(float4x4.TRS(center, rotation, size));
			WireBox(float3.zero, new float3(1, 1, 1));
			PopMatrix();
		}

		/// <summary>
		/// Draws the outline of a box.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void WireBox (Bounds bounds) {
			WireBox(bounds.center, bounds.size);
		}

		/// <summary>
		/// Draws a wire mesh.
		/// Every single edge of the mesh will be drawn using a <see cref="Line"/> command.
		///
		/// <code>
		/// var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		/// go.transform.position = new Vector3(0, 0, 0);
		/// using (Draw.InLocalSpace(go.transform)) {
		///     Draw.WireMesh(go.GetComponent<MeshFilter>().sharedMesh, color);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="SolidMesh(Mesh)"/>
		///
		/// Version: Supported in Unity 2020.1 or later.
		/// </summary>
		public void WireMesh (Mesh mesh) {
#if UNITY_2020_1_OR_NEWER
			if (mesh == null) throw new System.ArgumentNullException();

			// Use a burst compiled function to draw the lines
			// This is significantly faster than pure C# (about 5x).
			var meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh);
			var meshData = meshDataArray[0];

			JobWireMesh.JobWireMeshFunctionPointer(ref meshData, ref this);
			meshDataArray.Dispose();
#else
			Debug.LogError("The WireMesh method is only suppored in Unity 2020.1 or later");
#endif
		}

		/// <summary>
		/// Draws a wire mesh.
		/// Every single edge of the mesh will be drawn using a <see cref="Line"/> command.
		///
		/// <code>
		/// var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		/// go.transform.position = new Vector3(0, 0, 0);
		/// using (Draw.InLocalSpace(go.transform)) {
		///     Draw.WireMesh(go.GetComponent<MeshFilter>().sharedMesh, color);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="SolidMesh(Mesh)"/>
		///
		/// Version: Supported in Unity 2020.1 or later.
		/// </summary>
		public void WireMesh (NativeArray<float3> vertices, NativeArray<int> triangles) {
#if UNITY_2020_1_OR_NEWER
			unsafe {
				JobWireMesh.WireMesh((float3*)vertices.GetUnsafeReadOnlyPtr(), (int*)triangles.GetUnsafeReadOnlyPtr(), vertices.Length, triangles.Length, ref this);
			}
#else
			Debug.LogError("The WireMesh method is only suppored in Unity 2020.1 or later");
#endif
		}

#if UNITY_2020_1_OR_NEWER
		/// <summary>Helper job for <see cref="WireMesh"/></summary>
		[BurstCompile]
		class JobWireMesh {
			public delegate void JobWireMeshDelegate(ref Mesh.MeshData rawMeshData, ref CommandBuilder draw);

			public static readonly JobWireMeshDelegate JobWireMeshFunctionPointer = BurstCompiler.CompileFunctionPointer<JobWireMeshDelegate>(Execute).Invoke;

			[BurstCompile]
			public static unsafe void WireMesh (float3* verts, int* indices, int vertexCount, int indexCount, ref CommandBuilder draw) {
				if (indexCount % 3 != 0) {
					throw new System.ArgumentException("Invalid index count. Must be a multiple of 3");
				}
				// Ignore warning about NativeHashMap being obsolete in early versions of the collections package.
				// It works just fine, and in later versions the NativeHashMap is not obsolete.
				#pragma warning disable 618
				var seenEdges = new NativeHashMap<int2, bool>(indexCount, Allocator.Temp);
				#pragma warning restore 618
				for (int i = 0; i < indexCount; i += 3) {
					var a = indices[i];
					var b = indices[i+1];
					var c = indices[i+2];
					if (a < 0 || b < 0 || c < 0 || a >= vertexCount || b >= vertexCount || c >= vertexCount) {
						throw new Exception("Invalid vertex index. Index out of bounds");
					}
					int v1, v2;

					// Draw each edge of the triangle.
					// Check so that we do not draw an edge twice.
					v1 = math.min(a, b);
					v2 = math.max(a, b);
					if (!seenEdges.ContainsKey(new int2(v1, v2))) {
						seenEdges.Add(new int2(v1, v2), true);
						draw.Line(verts[v1], verts[v2]);
					}

					v1 = math.min(b, c);
					v2 = math.max(b, c);
					if (!seenEdges.ContainsKey(new int2(v1, v2))) {
						seenEdges.Add(new int2(v1, v2), true);
						draw.Line(verts[v1], verts[v2]);
					}

					v1 = math.min(c, a);
					v2 = math.max(c, a);
					if (!seenEdges.ContainsKey(new int2(v1, v2))) {
						seenEdges.Add(new int2(v1, v2), true);
						draw.Line(verts[v1], verts[v2]);
					}
				}
			}

			[BurstCompile]
			[AOT.MonoPInvokeCallback(typeof(JobWireMeshDelegate))]
			static void Execute (ref Mesh.MeshData rawMeshData, ref CommandBuilder draw) {
				int maxIndices = 0;
				for (int subMeshIndex = 0; subMeshIndex < rawMeshData.subMeshCount; subMeshIndex++) {
					maxIndices = math.max(maxIndices, rawMeshData.GetSubMesh(subMeshIndex).indexCount);
				}
				var tris = new NativeArray<int>(maxIndices, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
				var verts = new NativeArray<Vector3>(rawMeshData.vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
				rawMeshData.GetVertices(verts);

				for (int subMeshIndex = 0; subMeshIndex < rawMeshData.subMeshCount; subMeshIndex++) {
					var submesh = rawMeshData.GetSubMesh(subMeshIndex);
					rawMeshData.GetIndices(tris, subMeshIndex);
					unsafe {
						WireMesh((float3*)verts.GetUnsafeReadOnlyPtr(), (int*)tris.GetUnsafeReadOnlyPtr(), verts.Length, submesh.indexCount, ref draw);
					}
				}
			}
		}
#endif

		/// <summary>
		/// Draws a solid mesh.
		/// The mesh will be drawn with a solid color.
		///
		/// <code>
		/// var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		/// go.transform.position = new Vector3(0, 0, 0);
		/// using (Draw.InLocalSpace(go.transform)) {
		///     Draw.SolidMesh(go.GetComponent<MeshFilter>().sharedMesh, color);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: This method is not thread safe and must not be used from the Unity Job System.
		/// TODO: Are matrices handled?
		///
		/// See: <see cref="WireMesh(Mesh)"/>
		/// </summary>
		public void SolidMesh (Mesh mesh) {
			SolidMeshInternal(mesh, false);
		}

		void SolidMeshInternal (Mesh mesh, bool temporary, Color color) {
			PushColor(color);
			SolidMeshInternal(mesh, temporary);
			PopColor();
		}


		void SolidMeshInternal (Mesh mesh, bool temporary) {
			var g = gizmos.Target as DrawingData;

			g.data.Get(uniqueID).meshes.Add(new SubmittedMesh {
				mesh = mesh,
				temporary = temporary,
			});
			// Internally we need to make sure to capture the current state
			// (which includes the current matrix and color) so that it
			// can be applied to the mesh.
			Reserve(4);
			Add(Command.CaptureState);
		}

		/// <summary>
		/// Draws a solid mesh with the given vertices.
		///
		/// [Open online documentation to see images]
		///
		/// Note: This method is not thread safe and must not be used from the Unity Job System.
		/// TODO: Are matrices handled?
		/// </summary>
		[BurstDiscard]
		public void SolidMesh (List<Vector3> vertices, List<int> triangles, List<Color> colors) {
			if (vertices.Count != colors.Count) throw new System.ArgumentException("Number of colors must be the same as the number of vertices");

			// TODO: Is this mesh getting recycled at all?
			var g = gizmos.Target as DrawingData;
			var mesh = g.GetMesh(vertices.Count);

			// Set all data on the mesh
			mesh.Clear();
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.SetColors(colors);
			// Upload all data
			mesh.UploadMeshData(false);
			SolidMeshInternal(mesh, true);
		}

		/// <summary>
		/// Draws a solid mesh with the given vertices.
		///
		/// [Open online documentation to see images]
		///
		/// Note: This method is not thread safe and must not be used from the Unity Job System.
		/// TODO: Are matrices handled?
		/// </summary>
		[BurstDiscard]
		public void SolidMesh (Vector3[] vertices, int[] triangles, Color[] colors, int vertexCount, int indexCount) {
			if (vertices.Length != colors.Length) throw new System.ArgumentException("Number of colors must be the same as the number of vertices");

			// TODO: Is this mesh getting recycled at all?
			var g = gizmos.Target as DrawingData;
			var mesh = g.GetMesh(vertices.Length);

			// Set all data on the mesh
			mesh.Clear();
			mesh.SetVertices(vertices, 0, vertexCount);
			mesh.SetTriangles(triangles, 0, indexCount, 0);
			mesh.SetColors(colors, 0, vertexCount);
			// Upload all data
			mesh.UploadMeshData(false);
			SolidMeshInternal(mesh, true);
		}

		/// <summary>
		/// Draws a 3D cross.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void Cross (float3 position, float size = 1) {
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, size, 0), position + new float3(0, size, 0));
			Line(position - new float3(0, 0, size), position + new float3(0, 0, size));
		}

		/// <summary>
		/// Draws a cross in the XZ plane.
		///
		/// [Open online documentation to see images]
		/// </summary>
		[System.Obsolete("Use Draw.xz.Cross instead")]
		public void CrossXZ (float3 position, float size = 1) {
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, 0, size), position + new float3(0, 0, size));
		}

		/// <summary>
		/// Draws a cross in the XY plane.
		///
		/// [Open online documentation to see images]
		/// </summary>
		[System.Obsolete("Use Draw.xy.Cross instead")]
		public void CrossXY (float3 position, float size = 1) {
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, size, 0), position + new float3(0, size, 0));
		}

		/// <summary>Returns a point on a cubic bezier curve. t is clamped between 0 and 1</summary>
		public static float3 EvaluateCubicBezier (float3 p0, float3 p1, float3 p2, float3 p3, float t) {
			t = math.clamp(t, 0, 1);
			float tr = 1-t;
			return tr*tr*tr * p0 + 3 * tr*tr * t * p1 + 3 * tr * t*t * p2 + t*t*t * p3;
		}

		/// <summary>
		/// Draws a cubic bezier curve.
		///
		/// [Open online documentation to see images]
		///
		/// [Open online documentation to see images]
		///
		/// TODO: Currently uses a fixed resolution of 20 segments. Resolution should depend on the distance to the camera.
		///
		/// See: https://en.wikipedia.org/wiki/Bezier_curve
		/// </summary>
		/// <param name="p0">Start point</param>
		/// <param name="p1">First control point</param>
		/// <param name="p2">Second control point</param>
		/// <param name="p3">End point</param>
		public void Bezier (float3 p0, float3 p1, float3 p2, float3 p3) {
			float3 prev = p0;

			for (int i = 1; i <= 20; i++) {
				float t = i/20.0f;
				float3 p = EvaluateCubicBezier(p0, p1, p2, p3, t);
				Line(prev, p);
				prev = p;
			}
		}

		/// <summary>
		/// Draws a smooth curve through a list of points.
		///
		/// A catmull-rom spline is equivalent to a bezier curve with control points determined by an algorithm.
		/// In fact, this package displays catmull-rom splines by first converting them to bezier curves.
		///
		/// [Open online documentation to see images]
		///
		/// See: https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
		/// See: <see cref="CatmullRom(float3,float3,float3,float3)"/>
		/// </summary>
		/// <param name="points">The curve will smoothly pass through each point in the list in order.</param>
		public void CatmullRom (List<Vector3> points) {
			if (points.Count < 2) return;

			if (points.Count == 2) {
				Line(points[0], points[1]);
			} else {
				// count >= 3
				var count = points.Count;
				// Draw first curve, this is special because the first two control points are the same
				CatmullRom(points[0], points[0], points[1], points[2]);
				for (int i = 0; i + 3 < count; i++) {
					CatmullRom(points[i], points[i+1], points[i+2], points[i+3]);
				}
				// Draw last curve
				CatmullRom(points[count-3], points[count-2], points[count-1], points[count-1]);
			}
		}

		/// <summary>
		/// Draws a centripetal catmull rom spline.
		///
		/// The curve starts at p1 and ends at p2.
		///
		/// [Open online documentation to see images]
		/// [Open online documentation to see images]
		///
		/// See: <see cref="CatmullRom(List<Vector3>)"/>
		/// </summary>
		/// <param name="p0">First control point</param>
		/// <param name="p1">Second control point. Start of the curve.</param>
		/// <param name="p2">Third control point. End of the curve.</param>
		/// <param name="p3">Fourth control point.</param>
		public void CatmullRom (float3 p0, float3 p1, float3 p2, float3 p3) {
			// References used:
			// p.266 GemsV1
			//
			// tension is often set to 0.5 but you can use any reasonable value:
			// http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
			//
			// bias and tension controls:
			// http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

			// We will convert the catmull rom spline to a bezier curve for simplicity.
			// The end result of this will be a conversion matrix where we transform catmull rom control points
			// into the equivalent bezier curve control points.

			// Conversion matrix
			// =================

			// A centripetal catmull rom spline can be separated into the following terms:
			// 1 * p1 +
			// t * (-0.5 * p0 + 0.5*p2) +
			// t*t * (p0 - 2.5*p1  + 2.0*p2 + 0.5*t2) +
			// t*t*t * (-0.5*p0 + 1.5*p1 - 1.5*p2 + 0.5*p3)
			//
			// Matrix form:
			// 1     t   t^2 t^3
			// {0, -1/2, 1, -1/2}
			// {1, 0, -5/2, 3/2}
			// {0, 1/2, 2, -3/2}
			// {0, 0, -1/2, 1/2}

			// Transposed matrix:
			// M_1 = {{0, 1, 0, 0}, {-1/2, 0, 1/2, 0}, {1, -5/2, 2, -1/2}, {-1/2, 3/2, -3/2, 1/2}}

			// A bezier spline can be separated into the following terms:
			// (-t^3 + 3 t^2 - 3 t + 1) * c0 +
			// (3t^3 - 6*t^2 + 3t) * c1 +
			// (3t^2 - 3t^3) * c2 +
			// t^3 * c3
			//
			// Matrix form:
			// 1  t  t^2  t^3
			// {1, -3, 3, -1}
			// {0, 3, -6, 3}
			// {0, 0, 3, -3}
			// {0, 0, 0, 1}

			// Transposed matrix:
			// M_2 = {{1, 0, 0, 0}, {-3, 3, 0, 0}, {3, -6, 3, 0}, {-1, 3, -3, 1}}

			// Thus a bezier curve can be evaluated using the expression
			// output1 = T * M_1 * c
			// where T = [1, t, t^2, t^3] and c being the control points c = [c0, c1, c2, c3]^T
			//
			// and a catmull rom spline can be evaluated using
			//
			// output2 = T * M_2 * p
			// where T = same as before and p = [p0, p1, p2, p3]^T
			//
			// We can solve for c in output1 = output2
			// T * M_1 * c = T * M_2 * p
			// M_1 * c = M_2 * p
			// c = M_1^(-1) * M_2 * p
			// Thus a conversion matrix from p to c is M_1^(-1) * M_2
			// This can be calculated and the result is the following matrix:
			//
			// {0, 1, 0, 0}
			// {-1/6, 1, 1/6, 0}
			// {0, 1/6, 1, -1/6}
			// {0, 0, 1, 0}
			// ------------------------------------------------------------------
			//
			// Using this we can calculate c = M_1^(-1) * M_2 * p
			var c0 = p1;
			var c1 = (-p0 + 6*p1 + 1*p2)*(1/6.0f);
			var c2 = (p1 + 6*p2 - p3)*(1/6.0f);
			var c3 = p2;

			// And finally draw the bezier curve which is equivalent to the desired catmull-rom spline
			Bezier(c0, c1, c2, c3);
		}

		/// <summary>
		/// Draws an arrow between two points.
		///
		/// The size of the head defaults to 20% of the length of the arrow.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="ArrowheadArc"/>
		/// See: <see cref="Arrow(float3,float3,float3,float)"/>
		/// See: <see cref="ArrowRelativeSizeHead"/>
		/// </summary>
		/// <param name="from">Base of the arrow.</param>
		/// <param name="to">Head of the arrow.</param>
		public void Arrow (float3 from, float3 to) {
			ArrowRelativeSizeHead(from, to, DEFAULT_UP, 0.2f);
		}

		/// <summary>
		/// Draws an arrow between two points.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="ArrowRelativeSizeHead"/>
		/// See: <see cref="ArrowheadArc"/>
		/// </summary>
		/// <param name="from">Base of the arrow.</param>
		/// <param name="to">Head of the arrow.</param>
		/// <param name="up">Up direction of the world, the arrowhead plane will be as perpendicular as possible to this direction. Defaults to Vector3.up.</param>
		/// <param name="headSize">The size of the arrowhead in world units.</param>
		public void Arrow (float3 from, float3 to, float3 up, float headSize) {
			var length_sq = math.lengthsq(to - from);

			if (length_sq > 0.000001f) {
				ArrowRelativeSizeHead(from, to, up, headSize * math.rsqrt(length_sq));
			}
		}

		/// <summary>
		/// Draws an arrow between two points with a head that varies with the length of the arrow.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="ArrowheadArc"/>
		/// See: <see cref="Arrow"/>
		/// </summary>
		/// <param name="from">Base of the arrow.</param>
		/// <param name="to">Head of the arrow.</param>
		/// <param name="up">Up direction of the world, the arrowhead plane will be as perpendicular as possible to this direction.</param>
		/// <param name="headFraction">The length of the arrowhead is the distance between from and to multiplied by this fraction. Should be between 0 and 1.</param>
		public void ArrowRelativeSizeHead (float3 from, float3 to, float3 up, float headFraction) {
			Line(from, to);
			var dir = to - from;

			var normal = math.cross(dir, up);
			// Pick a different up direction if the direction happened to be colinear with that one.
			if (math.all(normal == 0)) normal = math.cross(new float3(1, 0, 0), dir);
			// Pick a different up direction if up=(1,0,0) and thus the above check would have generated a zero vector again
			if (math.all(normal == 0)) normal = math.cross(new float3(0, 1, 0), dir);
			normal = math.normalizesafe(normal) * math.length(dir);

			Line(to, to - (dir + normal) * headFraction);
			Line(to, to - (dir - normal) * headFraction);
		}

		/// <summary>
		/// Draws an arrowhead at a point.
		///
		/// <code>
		/// Draw.Arrowhead(Vector3.zero, Vector3.forward, 0.75f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Arrow"/>
		/// See: <see cref="ArrowRelativeSizeHead"/>
		/// </summary>
		/// <param name="center">Center of the arrowhead.</param>
		/// <param name="direction">Direction the arrow is pointing.</param>
		/// <param name="radius">Distance from the center to each corner of the arrowhead.</param>
		public void Arrowhead (float3 center, float3 direction, float radius) {
			Arrowhead(center, direction, DEFAULT_UP, radius);
		}

		/// <summary>
		/// Draws an arrowhead at a point.
		///
		/// <code>
		/// Draw.Arrowhead(Vector3.zero, Vector3.forward, 0.75f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Arrow"/>
		/// See: <see cref="ArrowRelativeSizeHead"/>
		/// </summary>
		/// <param name="center">Center of the arrowhead.</param>
		/// <param name="direction">Direction the arrow is pointing.</param>
		/// <param name="up">Up direction of the world, the arrowhead plane will be as perpendicular as possible to this direction. Defaults to Vector3.up. Must be normalized.</param>
		/// <param name="radius">Distance from the center to each corner of the arrowhead.</param>
		public void Arrowhead (float3 center, float3 direction, float3 up, float radius) {
			if (math.all(direction == 0)) return;
			direction = math.normalizesafe(direction);
			var normal = math.cross(direction, up);
			const float SinPiOver3 = 0.866025f;
			const float CosPiOver3 = 0.5f;
			var circleCenter = center - radius * (1 - CosPiOver3)*0.5f * direction;
			var p1 = circleCenter + radius * direction;
			var p2 = circleCenter - radius * CosPiOver3 * direction + radius * SinPiOver3 * normal;
			var p3 = circleCenter - radius * CosPiOver3 * direction - radius * SinPiOver3 * normal;
			Line(p1, p2);
			Line(p2, circleCenter);
			Line(circleCenter, p3);
			Line(p3, p1);
		}

		/// <summary>
		/// Draws an arrowhead centered around a circle.
		///
		/// This can be used to for example show the direction a character is moving in.
		///
		/// [Open online documentation to see images]
		///
		/// Note: In the image above the arrowhead is the only part that is drawn by this method. The cylinder is only included for context.
		///
		/// See: <see cref="Arrow"/>
		/// </summary>
		/// <param name="origin">Point around which the arc is centered</param>
		/// <param name="direction">Direction the arrow is pointing</param>
		/// <param name="offset">Distance from origin that the arrow starts.</param>
		/// <param name="width">Width of the arrowhead in degrees (defaults to 60). Should be between 0 and 90.</param>
		public void ArrowheadArc (float3 origin, float3 direction, float offset, float width = 60) {
			if (!math.any(direction)) return;
			if (offset < 0) throw new System.ArgumentOutOfRangeException(nameof(offset));
			if (offset == 0) return;

			var rot = Quaternion.LookRotation(direction, DEFAULT_UP);
			PushMatrix(Matrix4x4.TRS(origin, rot, Vector3.one));
			var a1 = math.PI * 0.5f - width * (0.5f * Mathf.Deg2Rad);
			var a2 = math.PI * 0.5f + width * (0.5f * Mathf.Deg2Rad);
			CircleXZInternal(float3.zero, offset, a1, a2);
			var p1 = new float3(math.cos(a1), 0, math.sin(a1)) * offset;
			var p2 = new float3(math.cos(a2), 0, math.sin(a2)) * offset;
			const float sqrt2 = 1.4142f;
			var p3 = new float3(0, 0, sqrt2 * offset);
			Line(p1, p3);
			Line(p3, p2);
			PopMatrix();
		}

		/// <summary>
		/// Draws a grid of lines.
		///
		/// <code>
		/// Draw.xz.WireGrid(Vector3.zero, new int2(3, 3), new float2(1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the grid</param>
		/// <param name="rotation">Rotation of the grid. The grid will be aligned to the X and Z axes of the rotation.</param>
		/// <param name="cells">Number of cells of the grid. Should be greater than 0.</param>
		/// <param name="totalSize">Total size of the grid along the X and Z axes.</param>
		public void WireGrid (float3 center, quaternion rotation, int2 cells, float2 totalSize) {
			cells = math.max(cells, new int2(1, 1));
			PushMatrix(float4x4.TRS(center, rotation, new Vector3(totalSize.x, 0, totalSize.y)));
			int w = cells.x;
			int h = cells.y;
			for (int i = 0; i <= w; i++) Line(new float3(i/(float)w - 0.5f, 0, -0.5f), new float3(i/(float)w - 0.5f, 0, 0.5f));
			for (int i = 0; i <= h; i++) Line(new float3(-0.5f, 0, i/(float)h - 0.5f), new float3(0.5f, 0, i/(float)h - 0.5f));
			PopMatrix();
		}

		/// <summary>
		/// Draws a triangle outline.
		///
		/// <code>
		/// Draw.WireTriangle(new Vector3(-0.5f, 0, 0), new Vector3(0, 1, 0), new Vector3(0.5f, 0, 0), Color.black);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.WirePlane(float3,quaternion,float2)"/>
		/// See: <see cref="WirePolygon"/>
		/// See: <see cref="SolidTriangle"/>
		/// </summary>
		/// <param name="a">First corner of the triangle</param>
		/// <param name="b">Second corner of the triangle</param>
		/// <param name="c">Third corner of the triangle</param>
		public void WireTriangle (float3 a, float3 b, float3 c) {
			Line(a, b);
			Line(b, c);
			Line(c, a);
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// The rectangle will be aligned to the X and Z axes.
		///
		/// <code>
		/// Draw.xz.WireRectangle(new Vector3(0f, 0, 0), new Vector2(1, 1), Color.black);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WirePolygon"/>
		/// </summary>
		[System.Obsolete("Use Draw.xz.WireRectangle instead")]
		public void WireRectangleXZ (float3 center, float2 size) {
			WireRectangle(center, quaternion.identity, size);
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// The rectangle will be oriented along the rotation's X and Z axes.
		///
		/// <code>
		/// Draw.WireRectangle(new Vector3(0f, 0, 0), Quaternion.identity, new Vector2(1, 1), Color.black);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// This is identical to <see cref="Draw.WirePlane(float3,quaternion,float2)"/>, but this name is added for consistency.
		///
		/// See: <see cref="WirePolygon"/>
		/// </summary>
		public void WireRectangle (float3 center, quaternion rotation, float2 size) {
			WirePlane(center, rotation, size);
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// The rectangle corners are assumed to be in XY space.
		/// This is particularly useful when combined with <see cref="InScreenSpace"/>.
		///
		/// <code>
		/// using (Draw.InScreenSpace(Camera.main)) {
		///     Draw.xy.WireRectangle(new Rect(10, 10, 100, 100), Color.black);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WireRectangleXZ"/>
		/// See: <see cref="WireRectangle(float3,quaternion,float2)"/>
		/// See: <see cref="WirePolygon"/>
		/// </summary>
		[System.Obsolete("Use Draw.xy.WireRectangle instead")]
		public void WireRectangle (Rect rect) {
			xy.WireRectangle(rect);
		}


		/// <summary>
		/// Draws a triangle outline.
		///
		/// <code>
		/// Draw.WireTriangle(Vector3.zero, Quaternion.identity, 0.5f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: This is a convenience wrapper for <see cref="WirePolygon(float3,int,quaternion,float)"/>
		///
		/// See: <see cref="WireTriangle(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the triangle.</param>
		/// <param name="rotation">Rotation of the triangle. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WireTriangle (float3 center, quaternion rotation, float radius) {
			WirePolygon(center, 3, rotation, radius);
		}

		/// <summary>
		/// Draws a pentagon outline.
		///
		/// <code>
		/// Draw.WirePentagon(Vector3.zero, Quaternion.identity, 0.5f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: This is a convenience wrapper for <see cref="WirePolygon(float3,int,quaternion,float)"/>
		/// </summary>
		/// <param name="center">Center of the polygon.</param>
		/// <param name="rotation">Rotation of the polygon. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WirePentagon (float3 center, quaternion rotation, float radius) {
			WirePolygon(center, 5, rotation, radius);
		}

		/// <summary>
		/// Draws a hexagon outline.
		///
		/// <code>
		/// Draw.WireHexagon(Vector3.zero, Quaternion.identity, 0.5f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: This is a convenience wrapper for <see cref="WirePolygon(float3,int,quaternion,float)"/>
		/// </summary>
		/// <param name="center">Center of the polygon.</param>
		/// <param name="rotation">Rotation of the polygon. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WireHexagon (float3 center, quaternion rotation, float radius) {
			WirePolygon(center, 6, rotation, radius);
		}

		/// <summary>
		/// Draws a regular polygon outline.
		///
		/// <code>
		/// Draw.WirePolygon(new Vector3(-0.5f, 0, +0.5f), 3, Quaternion.identity, 0.4f, color);
		/// Draw.WirePolygon(new Vector3(+0.5f, 0, +0.5f), 4, Quaternion.identity, 0.4f, color);
		/// Draw.WirePolygon(new Vector3(-0.5f, 0, -0.5f), 5, Quaternion.identity, 0.4f, color);
		/// Draw.WirePolygon(new Vector3(+0.5f, 0, -0.5f), 6, Quaternion.identity, 0.4f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WireTriangle"/>
		/// See: <see cref="WirePentagon"/>
		/// See: <see cref="WireHexagon"/>
		/// </summary>
		/// <param name="center">Center of the polygon.</param>
		/// <param name="vertices">Number of corners (and sides) of the polygon.</param>
		/// <param name="rotation">Rotation of the polygon. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WirePolygon (float3 center, int vertices, quaternion rotation, float radius) {
			PushMatrix(float4x4.TRS(center, rotation, new float3(radius, radius, radius)));
			float3 prev = new float3(0, 0, 1);
			for (int i = 1; i <= vertices; i++) {
				float a = 2 * math.PI * (i / (float)vertices);
				var p = new float3(math.sin(a), 0, math.cos(a));
				Line(prev, p);
				prev = p;
			}
			PopMatrix();
		}

		/// <summary>
		/// Draws a solid rectangle.
		/// The rectangle corners are assumed to be in XY space.
		/// This is particularly useful when combined with <see cref="InScreenSpace"/>.
		///
		/// Behind the scenes this is implemented using <see cref="SolidPlane"/>.
		///
		/// <code>
		/// using (Draw.InScreenSpace(Camera.main)) {
		///     Draw.xy.SolidRectangle(new Rect(10, 10, 100, 100), Color.black);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WireRectangleXZ"/>
		/// See: <see cref="WireRectangle(float3,quaternion,float2)"/>
		/// See: <see cref="SolidBox"/>
		/// </summary>
		[System.Obsolete("Use Draw.xy.SolidRectangle instead")]
		public void SolidRectangle (Rect rect) {
			xy.SolidRectangle(rect);
		}

		/// <summary>
		/// Draws a solid plane.
		///
		/// <code>
		/// Draw.SolidPlane(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="normal">Direction perpendicular to the plane. If this is (0,0,0) then nothing will be rendered.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void SolidPlane (float3 center, float3 normal, float2 size) {
			if (math.any(normal)) {
				SolidPlane(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
			}
		}

		/// <summary>
		/// Draws a solid plane.
		///
		/// The plane will lie in the XZ plane with respect to the rotation.
		///
		/// <code>
		/// Draw.SolidPlane(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void SolidPlane (float3 center, quaternion rotation, float2 size) {
			PushMatrix(float4x4.TRS(center, rotation, new float3(size.x, 0, size.y)));
			Reserve<BoxData>();
			Add(Command.Box);
			Add(new BoxData { center = 0, size = 1 });
			PopMatrix();
		}

		/// <summary>Returns an arbitrary vector which is orthogonal to the given one</summary>
		private static float3 calculateTangent (float3 normal) {
			var tangent = math.cross(new float3(0, 1, 0), normal);

			if (math.all(tangent == 0)) tangent = math.cross(new float3(1, 0, 0), normal);
			return tangent;
		}

		/// <summary>
		/// Draws a wire plane.
		///
		/// <code>
		/// Draw.WirePlane(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="normal">Direction perpendicular to the plane. If this is (0,0,0) then nothing will be rendered.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void WirePlane (float3 center, float3 normal, float2 size) {
			if (math.any(normal)) {
				WirePlane(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
			}
		}

		/// <summary>
		/// Draws a wire plane.
		///
		/// This is identical to <see cref="WireRectangle(float3,quaternion,float2)"/>, but it is included for consistency.
		///
		/// <code>
		/// Draw.WirePlane(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="rotation">Rotation of the plane. The plane will lie in the XZ plane with respect to the rotation.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void WirePlane (float3 center, quaternion rotation, float2 size) {
			Reserve<PlaneData>();
			Add(Command.WirePlane);
			Add(new PlaneData { center = center, rotation = rotation, size = size });
		}

		/// <summary>
		/// Draws a plane and a visualization of its normal.
		///
		/// <code>
		/// Draw.PlaneWithNormal(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="normal">Direction perpendicular to the plane. If this is (0,0,0) then nothing will be rendered.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void PlaneWithNormal (float3 center, float3 normal, float2 size) {
			if (math.any(normal)) {
				PlaneWithNormal(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
			}
		}

		/// <summary>
		/// Draws a plane and a visualization of its normal.
		///
		/// <code>
		/// Draw.PlaneWithNormal(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="rotation">Rotation of the plane. The plane will lie in the XZ plane with respect to the rotation.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void PlaneWithNormal (float3 center, quaternion rotation, float2 size) {
			SolidPlane(center, rotation, size);
			WirePlane(center, rotation, size);
			ArrowRelativeSizeHead(center, center + math.mul(rotation, new float3(0, 1, 0)) * 0.5f, math.mul(rotation, new float3(0, 0, 1)), 0.2f);
		}

		/// <summary>
		/// Draws a solid triangle.
		///
		/// <code>
		/// Draw.xy.SolidTriangle(new float2(-0.43f, -0.25f), new float2(0, 0.5f), new float2(0.43f, -0.25f), color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: If you are going to be drawing lots of triangles it's better to use <see cref="Draw.SolidMesh"/> instead as it will be more efficient.
		///
		/// See: <see cref="Draw.SolidMesh"/>
		/// See: <see cref="Draw.WireTriangle"/>
		/// </summary>
		/// <param name="a">First corner of the triangle.</param>
		/// <param name="b">Second corner of the triangle.</param>
		/// <param name="c">Third corner of the triangle.</param>
		public void SolidTriangle (float3 a, float3 b, float3 c) {
			Reserve<TriangleData>();
			Add(Command.SolidTriangle);
			Add(new TriangleData { a = a, b = b, c = c });
		}

		/// <summary>
		/// Draws a solid box.
		///
		/// <code>
		/// Draw.SolidBox(new float3(0, 0, 0), new float3(1, 1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void SolidBox (float3 center, float3 size) {
			Reserve<BoxData>();
			Add(Command.Box);
			Add(new BoxData { center = center, size = size });
		}

		/// <summary>
		/// Draws a solid box.
		///
		/// <code>
		/// Draw.SolidBox(new float3(0, 0, 0), new float3(1, 1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="bounds">Bounding box of the box</param>
		public void SolidBox (Bounds bounds) {
			SolidBox(bounds.center, bounds.size);
		}

		/// <summary>
		/// Draws a solid box.
		///
		/// <code>
		/// Draw.SolidBox(new float3(0, 0, 0), new float3(1, 1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="rotation">Rotation of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void SolidBox (float3 center, quaternion rotation, float3 size) {
			PushMatrix(float4x4.TRS(center, rotation, size));
			SolidBox(float3.zero, Vector3.one);
			PopMatrix();
		}

		/// <summary>
		/// Draws a label in 3D space.
		///
		/// The default alignment is <see cref="Drawing.LabelAlignment.MiddleLeft"/>.
		///
		/// <code>
		/// Draw.Label3D(new float3(0.2f, -1f, 0.2f), Quaternion.Euler(45, -110, -90), "Label", 1, LabelAlignment.Center, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label3D(float3,quaternion,string,float,LabelAlignment)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="rotation">Rotation in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="size">World size of the text. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		public void Label3D (float3 position, quaternion rotation, string text, float size) {
			Label3D(position, rotation, text, size, LabelAlignment.MiddleLeft);
		}

		/// <summary>
		/// Draws a label in 3D space.
		///
		/// <code>
		/// Draw.Label3D(new float3(0.2f, -1f, 0.2f), Quaternion.Euler(45, -110, -90), "Label", 1, LabelAlignment.Center, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label3D(float3,quaternion,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method cannot be used in burst since managed strings are not suppported in burst. However, you can use the separate Label3D overload which takes a FixedString.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="rotation">Rotation in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="size">World size of the text. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		/// <param name="alignment">How to align the text relative to the given position.</param>
		public void Label3D (float3 position, quaternion rotation, string text, float size, LabelAlignment alignment) {
			AssertBufferExists();
			Reserve<TextData3D>();
			Add(Command.Text3D);
			Add(new TextData3D { center = position, rotation = rotation, numCharacters = text.Length, size = size, alignment = alignment });
			AddText(text);
		}

		/// <summary>
		/// Draws a label in 3D space aligned with the camera.
		///
		/// The default alignment is <see cref="Drawing.LabelAlignment.MiddleLeft"/>.
		///
		/// <code>
		/// Draw.Label2D(Vector3.zero, "Label", 48, LabelAlignment.Center, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label2D(float3,string,float,LabelAlignment)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="sizeInPixels">Size of the text in screen pixels. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		public void Label2D (float3 position, string text, float sizeInPixels = 14) {
			Label2D(position, text, sizeInPixels, LabelAlignment.MiddleLeft);
		}

		/// <summary>
		/// Draws a label in 3D space aligned with the camera.
		///
		/// <code>
		/// Draw.Label2D(Vector3.zero, "Label", 48, LabelAlignment.Center, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label2D(float3,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method cannot be used in burst since managed strings are not suppported in burst. However, you can use the separate Label2D overload which takes a FixedString.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="sizeInPixels">Size of the text in screen pixels. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		/// <param name="alignment">How to align the text relative to the given position.</param>
		public void Label2D (float3 position, string text, float sizeInPixels, LabelAlignment alignment) {
			AssertBufferExists();
			Reserve<TextData>();
			Add(Command.Text);
			Add(new TextData { center = position, numCharacters = text.Length, sizeInPixels = sizeInPixels, alignment = alignment });
			AddText(text);
		}

		void AddText (string text) {
			var g = gizmos.Target as DrawingData;
			Reserve(UnsafeUtility.SizeOf<System.UInt16>() * text.Length);
			for (int i = 0; i < text.Length; i++) {
				char c = text[i];
				System.UInt16 index = (System.UInt16)g.fontData.GetIndex(c);
				Add(index);
			}
		}

		#region Label2DFixedString
		/// <summary>
		/// Draws a label in 3D space aligned with the camera.
		///
		/// <code>
		/// // This part can be inside a burst job
		/// for (int i = 0; i < 10; i++) {
		///     Unity.Collections.FixedString32Bytes text = $"X = {i}";
		///     builder.Label2D(new float3(i, 0, 0), ref text, 12, LabelAlignment.Center);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label2D(float3,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method requires the Unity.Collections package version 0.8 or later.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="sizeInPixels">Size of the text in screen pixels. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		public void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels = 14) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels = 14) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels = 14) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels = 14) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
		}

		/// <summary>
		/// Draws a label in 3D space aligned with the camera.
		///
		/// <code>
		/// // This part can be inside a burst job
		/// for (int i = 0; i < 10; i++) {
		///     Unity.Collections.FixedString32Bytes text = $"X = {i}";
		///     builder.Label2D(new float3(i, 0, 0), ref text, 12, LabelAlignment.Center);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label2D(float3,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method requires the Unity.Collections package version 0.8 or later.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="sizeInPixels">Size of the text in screen pixels. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		/// <param name="alignment">How to align the text relative to the given position.</param>
		public void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
			unsafe {
				Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
			}
#else
			Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
			unsafe {
				Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
			}
#else
			Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
			unsafe {
				Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
			}
#else
			Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
			unsafe {
				Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
			}
#else
			Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		internal unsafe void Label2D (float3 position, byte* text, int byteCount, float sizeInPixels, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
			AssertBufferExists();
			Reserve<TextData>();
			Add(Command.Text);
			Add(new TextData { center = position, numCharacters = byteCount, sizeInPixels = sizeInPixels, alignment = alignment });

			Reserve(UnsafeUtility.SizeOf<System.UInt16>() * byteCount);
			for (int i = 0; i < byteCount; i++) {
				// The first 128 elements in the font data are guaranteed to be laid out as ascii.
				// We use this since we cannot use the dynamic font lookup.
				System.UInt16 c = *(text + i);
				if (c >= 128) c = (System.UInt16) '?';
				if (c == (byte)'\n') c = SDFLookupData.Newline;
				// Ignore carriage return instead of printing them as '?'. Windows encodes newlines as \r\n.
				if (c == (byte)'\r') continue;
				Add(c);
			}
#endif
		}
		#endregion

		#region Label3DFixedString
		/// <summary>
		/// Draws a label in 3D space.
		///
		/// <code>
		/// // This part can be inside a burst job
		/// for (int i = 0; i < 10; i++) {
		///     Unity.Collections.FixedString32Bytes text = $"X = {i}";
		///     builder.Label3D(new float3(i, 0, 0), quaternion.identity, ref text, 1, LabelAlignment.Center);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label3D(float3,quaternion,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method requires the Unity.Collections package version 0.8 or later.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="rotation">Rotation in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="size">World size of the text. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		public void Label3D (float3 position, quaternion rotation, ref FixedString32Bytes text, float size) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString64Bytes text, float size) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString128Bytes text, float size) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString512Bytes text, float size) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
		}

		/// <summary>
		/// Draws a label in 3D space.
		///
		/// <code>
		/// // This part can be inside a burst job
		/// for (int i = 0; i < 10; i++) {
		///     Unity.Collections.FixedString32Bytes text = $"X = {i}";
		///     builder.Label3D(new float3(i, 0, 0), quaternion.identity, ref text, 1, LabelAlignment.Center);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label3D(float3,quaternion,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method requires the Unity.Collections package version 0.8 or later.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="rotation">Rotation in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="size">World size of the text. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		/// <param name="alignment">How to align the text relative to the given position.</param>
		public void Label3D (float3 position, quaternion rotation, ref FixedString32Bytes text, float size, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
			unsafe {
				Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
			}
#else
			Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString64Bytes text, float size, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
			unsafe {
				Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
			}
#else
			Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString128Bytes text, float size, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
			unsafe {
				Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
			}
#else
			Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString512Bytes text, float size, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
			unsafe {
				Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
			}
#else
			Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float,LabelAlignment)}</summary>
		internal unsafe void Label3D (float3 position, quaternion rotation, byte* text, int byteCount, float size, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
			AssertBufferExists();
			Reserve<TextData3D>();
			Add(Command.Text3D);
			Add(new TextData3D { center = position, rotation = rotation, numCharacters = byteCount, size = size, alignment = alignment });

			Reserve(UnsafeUtility.SizeOf<System.UInt16>() * byteCount);
			for (int i = 0; i < byteCount; i++) {
				// The first 128 elements in the font data are guaranteed to be laid out as ascii.
				// We use this since we cannot use the dynamic font lookup.
				System.UInt16 c = *(text + i);
				if (c >= 128) c = (System.UInt16) '?';
				if (c == (byte)'\n') c = SDFLookupData.Newline;
				Add(c);
			}
#endif
		}
		#endregion
	}
}
