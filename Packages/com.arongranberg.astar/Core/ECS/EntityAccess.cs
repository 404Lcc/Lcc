#if MODULE_ENTITIES
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Pathfinding.ECS {
	/// <summary>Helper for EntityAccess</summary>
	static class EntityAccessHelper {
		public static readonly int GlobalSystemVersionOffset = UnsafeUtility.GetFieldOffset(typeof(ComponentTypeHandle<int>).GetField("m_GlobalSystemVersion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
	}

	/// <summary>
	/// Wrapper for a pointer.
	///
	/// Very similar to the entities package RefRW<T> struct. But unfortunately that one cannot be used because the required constructor is not exposed.
	/// </summary>
	public ref struct ComponentRef<T> where T : unmanaged {
		unsafe byte* ptr;

		public unsafe ComponentRef(byte* ptr) {
			this.ptr = ptr;
		}

		public ref T value {
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			get {
				unsafe {
					return ref *(T*)ptr;
				}
			}
		}
	}

	/// <summary>Utility for efficient random access to entity storage data from the main thread</summary>
	public struct EntityStorageCache {
		EntityStorageInfo storage;
		Entity entity;
		int lastWorldHash;

		/// <summary>
		/// Retrieves the storage for a given entity.
		///
		/// This method is very fast if the entity is the same as the last call to this method.
		/// If the entity is different, it will be slower.
		///
		/// Returns: True if the entity exists, and false if it does not.
		/// </summary>
		// Inlining makes this method about 20% faster. It's hot when accessing properties on the FollowerEntity component.
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public bool Update (World world, Entity entity, out EntityManager entityManager, out EntityStorageInfo storage) {
			entityManager = default;
			storage = this.storage;
			if (world == null) return false;
			entityManager = world.EntityManager;
			// We must use entityManager.EntityOrderVersion here, not GlobalSystemVersion, because
			// the GlobalSystemVersion does not necessarily update when structural changes happen.
			var worldHash = entityManager.EntityOrderVersion ^ ((int)world.SequenceNumber << 8);
			if (worldHash != lastWorldHash || entity != this.entity) {
				if (!entityManager.Exists(entity)) return false;
				this.storage = storage = entityManager.GetStorageInfo(entity);
				this.entity = entity;
				lastWorldHash = worldHash;
			}
			return true;
		}

		/// <summary>
		/// Retrieves a component for a given entity.
		///
		/// This is a convenience method to call <see cref="Update"/> on this object and update on the access object, and then retrieve the component data.
		///
		/// This method is very fast if the entity is the same as the last call to this method.
		/// If the entity is different, it will be slower.
		///
		/// Warning: You must not store the returned reference past a structural change in the ECS world.
		///
		/// Returns: True if the entity exists, and false if it does not.
		/// Throws: An exception if the entity does not have the given component.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public bool GetComponentData<A>(Entity entity, ref EntityAccess<A> access, out ComponentRef<A> value) where A : unmanaged, IComponentData {
			if (Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage)) {
				access.Update(entityManager);
				unsafe {
					value = new ComponentRef<A>((byte*)UnsafeUtility.AddressOf(ref access[storage]));
				}
				return true;
			} else {
				value = default;
				return false;
			}
		}
	}

	/// <summary>
	/// Utility for efficient random access to entity component data from the main thread.
	///
	/// Since this struct returns a reference to the component data, it is faster than using EntityManager.GetComponentData,
	/// in particular for larger component types.
	///
	/// Warning: Some checks are not enforced by this API. It is the user's responsibility to ensure that
	/// this struct does not survive past an ECS system update. If you only use this struct from the main thread
	/// and only store it locally on the stack, this should not be a problem.
	/// This struct also does not enforce that you only read to the component data if the readOnly flag is set.
	/// </summary>
	public struct EntityAccess<T> where T : unmanaged, IComponentData {
		public ComponentTypeHandle<T> handle;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
		SystemHandle systemHandle;
#endif
		uint lastSystemVersion;
		ulong worldSequenceNumber;
		bool readOnly;

		public EntityAccess(bool readOnly) {
			handle = default;
			this.readOnly = readOnly;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			systemHandle = default;
#endif

			// Version 0 is never used by EntityManager.GlobalSystemVersion
			lastSystemVersion = 0;
			worldSequenceNumber = 0;
		}

		/// <summary>
		/// Update the component type handle if necessary.
		///
		/// This must be called if any jobs or system might have been scheduled since the struct was created or since the last call to Update.
		/// </summary>
		public void Update (EntityManager entityManager) {
			// If the global system version has changed, jobs may have been scheduled which writes
			// to the component data. Therefore we need to complete all dependencies before we can
			// safely read or write to the component data.

			var systemVersion = entityManager.GlobalSystemVersion;
			var sequenceNumber = entityManager.WorldUnmanaged.SequenceNumber;
			if (systemVersion != lastSystemVersion || worldSequenceNumber != sequenceNumber) {
				if (lastSystemVersion == 0 || worldSequenceNumber != sequenceNumber) {
					handle = entityManager.GetComponentTypeHandle<T>(readOnly);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
					entityManager.WorldUnmanaged.IsSystemValid(systemHandle);
					systemHandle = entityManager.WorldUnmanaged.GetExistingUnmanagedSystem<AIMoveSystem>();
#endif
				}

				lastSystemVersion = systemVersion;
				worldSequenceNumber = sequenceNumber;
				if (readOnly) entityManager.CompleteDependencyBeforeRO<T>();
				else entityManager.CompleteDependencyBeforeRW<T>();
			}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
			handle.Update(ref entityManager.WorldUnmanaged.ResolveSystemStateRef(systemHandle));
#else
			// handle.Update just does the same thing as this unsafe code, but in a much more roundabout way
			unsafe {
				var ptr = (byte*)UnsafeUtility.AddressOf(ref handle);
				*(uint*)(ptr + EntityAccessHelper.GlobalSystemVersionOffset) = systemVersion;
			}
#endif
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public bool HasComponent (EntityStorageInfo storage) {
			return storage.Chunk.Has<T>(ref handle);
		}

		public ref T this[EntityStorageInfo storage] {
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			get {
				unsafe {
					var ptr = readOnly ? ((T*)storage.Chunk.GetRequiredComponentDataPtrRO(ref handle) + storage.IndexInChunk) : ((T*)storage.Chunk.GetRequiredComponentDataPtrRW(ref handle) + storage.IndexInChunk);
					return ref *ptr;
				}
			}
		}
	}

	/// <summary>
	/// Utility for efficient random access to managed entity component data from the main thread.
	///
	/// Warning: Some checks are not enforced by this API. It is the user's responsibility to ensure that
	/// this struct does not survive past an ECS system update. If you only use this struct from the main thread
	/// and only store it locally on the stack, this should not be a problem.
	/// This struct also does not enforce that you only read to the component data if the readOnly flag is set.
	/// </summary>
	public struct ManagedEntityAccess<T> where T : class, IComponentData {
		EntityManager entityManager;
		ComponentTypeHandle<T> handle;
		bool readOnly;

		public ManagedEntityAccess(bool readOnly) {
			entityManager = default;
			handle = default;
			this.readOnly = readOnly;
		}

		public ManagedEntityAccess(EntityManager entityManager, bool readOnly) : this(readOnly) {
			Update(entityManager);
		}

		public void Update (EntityManager entityManager) {
			if (readOnly) entityManager.CompleteDependencyBeforeRO<T>();
			else entityManager.CompleteDependencyBeforeRW<T>();
			handle = entityManager.GetComponentTypeHandle<T>(readOnly);
			this.entityManager = entityManager;
		}

		public T this[EntityStorageInfo storage] {
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			get {
				return storage.Chunk.GetManagedComponentAccessor<T>(ref handle, entityManager)[storage.IndexInChunk];
			}
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			set {
				var accessor = storage.Chunk.GetManagedComponentAccessor<T>(ref handle, entityManager);
				accessor[storage.IndexInChunk] = value;
			}
		}
	}
}
#endif
