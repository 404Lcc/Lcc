using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Pathfinding.Drawing {
	using static CommandBuilder;

	[BurstCompile]
	internal struct PersistentFilterJob : IJob {
		[NativeDisableUnsafePtrRestriction]
		public unsafe UnsafeAppendBuffer* buffer;
		public float time;

		public void Execute () {
			var stackPersist = new NativeArray<bool>(GeometryBuilderJob.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);
			var stackScope = new NativeArray<int>(GeometryBuilderJob.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);

			unsafe {
				// Store in local variables for performance (makes it possible to use registers for a lot of fields)
				var bufferPersist = *buffer;

				long writeOffset = 0;
				long readOffset = 0;
				bool shouldWrite = false;
				int stackSize = 0;
				long lastNonMetaWrite = -1;

				while (readOffset < bufferPersist.Length) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
					UnityEngine.Assertions.Assert.IsTrue(readOffset + UnsafeUtility.SizeOf<Command>() <= bufferPersist.Length);
#endif
					var cmd = *(Command*)((byte*)bufferPersist.Ptr + readOffset);
					var cmdBit = 1 << ((int)cmd & 0xFF);
					bool isMeta = (cmdBit & StreamSplitter.MetaCommands) != 0;
					int size = StreamSplitter.CommandSizes[(int)cmd & 0xFF] + ((cmd & Command.PushColorInline) != 0 ? UnsafeUtility.SizeOf<Color32>() : 0);

					if ((cmd & (Command)0xFF) == Command.Text) {
						// Very pretty way of reading the TextData struct right after the command label and optional Color32
						var data = *((TextData*)((byte*)bufferPersist.Ptr + readOffset + size) - 1);
						// Add the size of the embedded string in the buffer
						size += data.numCharacters * UnsafeUtility.SizeOf<System.UInt16>();
					} else if ((cmd & (Command)0xFF) == Command.Text3D) {
						// Very pretty way of reading the TextData struct right after the command label and optional Color32
						var data = *((TextData3D*)((byte*)bufferPersist.Ptr + readOffset + size) - 1);
						// Add the size of the embedded string in the buffer
						size += data.numCharacters * UnsafeUtility.SizeOf<System.UInt16>();
					}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
					UnityEngine.Assertions.Assert.IsTrue(readOffset + size <= bufferPersist.Length);
					UnityEngine.Assertions.Assert.IsTrue(writeOffset + size <= bufferPersist.Length);
#endif

					if (shouldWrite || isMeta) {
						if (!isMeta) lastNonMetaWrite = writeOffset;
						if (writeOffset != readOffset) {
							// We need to use memmove instead of memcpy because the source and destination regions may overlap
							UnsafeUtility.MemMove((byte*)bufferPersist.Ptr + writeOffset, (byte*)bufferPersist.Ptr + readOffset, size);
						}
						writeOffset += size;
					}

					if ((cmdBit & StreamSplitter.PushCommands) != 0) {
						if ((cmd & (Command)0xFF) == Command.PushPersist) {
							// Very pretty way of reading the PersistData struct right after the command label and optional Color32
							// (even though a PushColorInline command is not usually combined with PushPersist)
							var data = *((PersistData*)((byte*)bufferPersist.Ptr + readOffset + size) - 1);
							// Scopes only survive if this condition is true
							shouldWrite = time <= data.endTime;
						}

						stackScope[stackSize] = (int)(writeOffset - size);
						stackPersist[stackSize] = shouldWrite;
						stackSize++;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
						if (stackSize >= GeometryBuilderJob.MaxStackSize) throw new System.Exception("Push commands are too deeply nested. This can happen if you have deeply nested WithMatrix or WithColor scopes.");
#else
						if (stackSize >= GeometryBuilderJob.MaxStackSize) {
							buffer->Length = 0;
							return;
						}
#endif
					} else if ((cmdBit & StreamSplitter.PopCommands) != 0) {
						stackSize--;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
						if (stackSize < 0) throw new System.Exception("Trying to issue a pop command but there is no corresponding push command");
#else
						if (stackSize < 0) {
							buffer->Length = 0;
							return;
						}
#endif
						// If a scope was pushed and later popped, but no actual draw commands were written to the buffers
						// inside that scope then we erase the whole scope.
						if ((int)lastNonMetaWrite < stackScope[stackSize]) {
							writeOffset = (long)stackScope[stackSize];
						}

						shouldWrite = stackPersist[stackSize];
					}

					readOffset += size;
				}

				bufferPersist.Length = (int)writeOffset;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (stackSize != 0) throw new System.Exception("Inconsistent push/pop commands. Are your push and pop commands properly matched?");
#else
				if (stackSize != 0) {
					buffer->Length = 0;
					return;
				}
#endif

				*buffer = bufferPersist;
			}
		}
	}
}
