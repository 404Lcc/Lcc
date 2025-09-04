using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Unity.Burst;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Jobs;

namespace Pathfinding.Drawing {
	using static CommandBuilder;

	[BurstCompile]
	internal struct StreamSplitter : IJob {
		public NativeArray<UnsafeAppendBuffer> inputBuffers;
		[NativeDisableUnsafePtrRestriction]
		public unsafe UnsafeAppendBuffer* staticBuffer, dynamicBuffer, persistentBuffer;

		internal static readonly int PushCommands = (1 << (int)Command.PushColor) | (1 << (int)Command.PushMatrix) | (1 << (int)Command.PushSetMatrix) | (1 << (int)Command.PushPersist) | (1 << (int)Command.PushLineWidth);
		internal static readonly int PopCommands = (1 << (int)Command.PopColor) | (1 << (int)Command.PopMatrix) | (1 << (int)Command.PopPersist) | (1 << (int)Command.PopLineWidth);
		internal static readonly int MetaCommands = PushCommands | PopCommands;
		internal static readonly int DynamicCommands = (1 << (int)Command.SphereOutline) | (1 << (int)Command.CircleXZ) | (1 << (int)Command.Circle) | (1 << (int)Command.DiscXZ) | (1 << (int)Command.Disc) | (1 << (int)Command.Text) | (1 << (int)Command.Text3D) | (1 << (int)Command.CaptureState) | MetaCommands;
		internal static readonly int StaticCommands = (1 << (int)Command.Line) | (1 << (int)Command.Box) | (1 << (int)Command.WirePlane) | (1 << (int)Command.WireBox) | (1 << (int)Command.SolidTriangle) | MetaCommands;

		internal static readonly int[] CommandSizes;
		static StreamSplitter() {
			// Size of all commands in bytes
			CommandSizes = new int[22];
			CommandSizes[(int)Command.PushColor] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<Color32>();
			CommandSizes[(int)Command.PopColor] = UnsafeUtility.SizeOf<Command>() + 0;
			CommandSizes[(int)Command.PushMatrix] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<float4x4>();
			CommandSizes[(int)Command.PushSetMatrix] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<float4x4>();
			CommandSizes[(int)Command.PopMatrix] = UnsafeUtility.SizeOf<Command>() + 0;
			CommandSizes[(int)Command.Line] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<LineData>();
			CommandSizes[(int)Command.CircleXZ] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleXZData>();
			CommandSizes[(int)Command.SphereOutline] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<SphereData>();
			CommandSizes[(int)Command.Circle] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleData>();
			CommandSizes[(int)Command.Disc] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleData>();
			CommandSizes[(int)Command.DiscXZ] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleXZData>();
			CommandSizes[(int)Command.Box] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<BoxData>();
			CommandSizes[(int)Command.WirePlane] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<PlaneData>();
			CommandSizes[(int)Command.WireBox] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<BoxData>();
			CommandSizes[(int)Command.SolidTriangle] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<TriangleData>();
			CommandSizes[(int)Command.PushPersist] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<PersistData>();
			CommandSizes[(int)Command.PopPersist] = UnsafeUtility.SizeOf<Command>();
			CommandSizes[(int)Command.Text] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<TextData>(); // Dynamically sized
			CommandSizes[(int)Command.Text3D] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<TextData3D>(); // Dynamically sized
			CommandSizes[(int)Command.PushLineWidth] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<LineWidthData>();
			CommandSizes[(int)Command.PopLineWidth] = UnsafeUtility.SizeOf<Command>();
			CommandSizes[(int)Command.CaptureState] = UnsafeUtility.SizeOf<Command>();
		}

		public void Execute () {
			var lastWriteStatic = -1;
			var lastWriteDynamic = -1;
			var lastWritePersist = -1;
			var stackStatic = new NativeArray<int>(GeometryBuilderJob.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);
			var stackDynamic = new NativeArray<int>(GeometryBuilderJob.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);
			var stackPersist = new NativeArray<int>(GeometryBuilderJob.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);

			unsafe {
				// Store in local variables for performance (makes it possible to use registers for a lot of fields)
				var bufferStatic = *staticBuffer;
				var bufferDynamic = *dynamicBuffer;
				var bufferPersist = *persistentBuffer;

				bufferStatic.Reset();
				bufferDynamic.Reset();
				bufferPersist.Reset();

				for (int i = 0; i < inputBuffers.Length; i++) {
					int stackSize = 0;
					int persist = 0;
					var reader = inputBuffers[i].AsReader();

					// Guarantee we have enough space for copying the whole buffer
					if (bufferStatic.Capacity < bufferStatic.Length + reader.Size) bufferStatic.SetCapacity(math.ceilpow2(bufferStatic.Length + reader.Size));
					if (bufferDynamic.Capacity < bufferDynamic.Length + reader.Size) bufferDynamic.SetCapacity(math.ceilpow2(bufferDynamic.Length + reader.Size));
					if (bufferPersist.Capacity < bufferPersist.Length + reader.Size) bufferPersist.SetCapacity(math.ceilpow2(bufferPersist.Length + reader.Size));

					// To ensure that even if exceptions are thrown the output buffers still point to valid memory regions
					*staticBuffer = bufferStatic;
					*dynamicBuffer = bufferDynamic;
					*persistentBuffer = bufferPersist;

					while (reader.Offset < reader.Size) {
						var cmd = *(Command*)((byte*)reader.Ptr + reader.Offset);
						var cmdBit = 1 << ((int)cmd & 0xFF);
						int size = CommandSizes[(int)cmd & 0xFF] + ((cmd & Command.PushColorInline) != 0 ? UnsafeUtility.SizeOf<Color32>() : 0);
						bool isMeta = (cmdBit & MetaCommands) != 0;

						if ((cmd & (Command)0xFF) == Command.Text) {
							// Very pretty way of reading the TextData struct right after the command label and optional Color32
							var data = *((TextData*)((byte*)reader.Ptr + reader.Offset + size) - 1);
							// Add the size of the embedded string in the buffer
							// TODO: Unaligned memory access performance penalties?? Update: Doesn't seem to be so bad on Intel at least.
							size += data.numCharacters * UnsafeUtility.SizeOf<System.UInt16>();
						} else if ((cmd & (Command)0xFF) == Command.Text3D) {
							// Very pretty way of reading the TextData struct right after the command label and optional Color32
							var data = *((TextData3D*)((byte*)reader.Ptr + reader.Offset + size) - 1);
							// Add the size of the embedded string in the buffer
							// TODO: Unaligned memory access performance penalties?? Update: Doesn't seem to be so bad on Intel at least.
							size += data.numCharacters * UnsafeUtility.SizeOf<System.UInt16>();
						}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
						UnityEngine.Assertions.Assert.IsTrue(reader.Offset + size <= reader.Size);
#endif

						if ((cmdBit & DynamicCommands) != 0 && persist == 0) {
							if (!isMeta) lastWriteDynamic = bufferDynamic.Length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
							UnityEngine.Assertions.Assert.IsTrue(bufferDynamic.Length + size <= bufferDynamic.Capacity);
#endif
							UnsafeUtility.MemCpy((byte*)bufferDynamic.Ptr + bufferDynamic.Length, (byte*)reader.Ptr + reader.Offset, size);
							bufferDynamic.Length = bufferDynamic.Length + size;
						}

						if ((cmdBit & StaticCommands) != 0 && persist == 0) {
							if (!isMeta) lastWriteStatic = bufferStatic.Length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
							UnityEngine.Assertions.Assert.IsTrue(bufferStatic.Length + size <= bufferStatic.Capacity);
#endif
							UnsafeUtility.MemCpy((byte*)bufferStatic.Ptr + bufferStatic.Length, (byte*)reader.Ptr + reader.Offset, size);
							bufferStatic.Length = bufferStatic.Length + size;
						}

						if ((cmdBit & MetaCommands) != 0 || persist > 0) {
							if (persist > 0 && !isMeta) lastWritePersist = bufferPersist.Length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
							UnityEngine.Assertions.Assert.IsTrue(bufferPersist.Length + size <= bufferPersist.Capacity);
#endif
							UnsafeUtility.MemCpy((byte*)bufferPersist.Ptr + bufferPersist.Length, (byte*)reader.Ptr + reader.Offset, size);
							bufferPersist.Length = bufferPersist.Length + size;
						}

						if ((cmdBit & PushCommands) != 0) {
							stackStatic[stackSize] = bufferStatic.Length - size;
							stackDynamic[stackSize] = bufferDynamic.Length - size;
							stackPersist[stackSize] = bufferPersist.Length - size;
							stackSize++;
							if ((cmd & (Command)0xFF) == Command.PushPersist) {
								persist++;
							}
#if ENABLE_UNITY_COLLECTIONS_CHECKS
							if (stackSize >= GeometryBuilderJob.MaxStackSize) throw new System.Exception("Push commands are too deeply nested. This can happen if you have deeply nested WithMatrix or WithColor scopes.");
#else
							if (stackSize >= GeometryBuilderJob.MaxStackSize) {
								return;
							}
#endif
						} else if ((cmdBit & PopCommands) != 0) {
							stackSize--;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
							if (stackSize < 0) throw new System.Exception("Trying to issue a pop command but there is no corresponding push command");
#else
							if (stackSize < 0) return;
#endif
							// If a scope was pushed and later popped, but no actual draw commands were written to the buffers
							// inside that scope then we erase the whole scope.
							if (lastWriteStatic < stackStatic[stackSize]) {
								bufferStatic.Length = stackStatic[stackSize];
							}
							if (lastWriteDynamic < stackDynamic[stackSize]) {
								bufferDynamic.Length = stackDynamic[stackSize];
							}
							if (lastWritePersist < stackPersist[stackSize]) {
								bufferPersist.Length = stackPersist[stackSize];
							}
							if ((cmd & (Command)0xFF) == Command.PopPersist) {
								persist--;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
								if (persist < 0) throw new System.Exception("Too many PopPersist commands. Are your PushPersist/PopPersist calls matched?");
#else
								if (persist < 0) return;
#endif
							}
						}

						reader.Offset += size;
					}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
					if (stackSize != 0) throw new System.Exception("Too few pop commands and too many push commands. Are your push and pop commands properly matched?");
					if (reader.Offset != reader.Size) throw new System.Exception("Did not end up at the end of the buffer. This is a bug.");
#else
					if (stackSize != 0) return;
					if (reader.Offset != reader.Size) return;
#endif
				}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (bufferStatic.Length > bufferStatic.Capacity) throw new System.Exception("Buffer overrun. This is a bug");
				if (bufferDynamic.Length > bufferDynamic.Capacity) throw new System.Exception("Buffer overrun. This is a bug");
				if (bufferPersist.Length > bufferPersist.Capacity) throw new System.Exception("Buffer overrun. This is a bug");
#endif

				*staticBuffer = bufferStatic;
				*dynamicBuffer = bufferDynamic;
				*persistentBuffer = bufferPersist;
			}
		}
	}
}
