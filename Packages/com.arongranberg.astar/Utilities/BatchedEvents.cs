using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace Pathfinding.Util {
	/// <summary>Helper for batching updates to many objects efficiently</summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/batchedevents.html")]
	public class BatchedEvents : VersionedMonoBehaviour {
		const int ArchetypeOffset = 22;
		const int ArchetypeMask = 0xFF << ArchetypeOffset;

		static Archetype[] data = new Archetype[0];
		static BatchedEvents instance;
		static int isIteratingOverTypeIndex = -1;
		static bool isIterating = false;

		[System.Flags]
		public enum Event {
			Update = 1 << 0,
			LateUpdate = 1 << 1,
			FixedUpdate = 1 << 2,
			Custom = 1 << 3,
			None = 0,
		};


		struct Archetype {
			public object[] objects;
			public int objectCount;
			public System.Type type;
			public TransformAccessArray transforms;
			public int variant;
			public int archetypeIndex;
			public Event events;
			public System.Action<object[], int, TransformAccessArray, Event> action;
			public CustomSampler sampler;

			public void Add (Component obj) {
				objectCount++;
				UnityEngine.Assertions.Assert.IsTrue(objectCount < (1 << ArchetypeOffset));
				if (objects == null) objects = (object[])System.Array.CreateInstance(type, math.ceilpow2(objectCount));
				if (objectCount > objects.Length) {
					var newObjects = System.Array.CreateInstance(type, math.ceilpow2(objectCount));
					objects.CopyTo(newObjects, 0);
					objects = (object[])newObjects;
				}
				objects[objectCount-1] = obj;
				if (!transforms.isCreated) transforms = new TransformAccessArray(16, -1);
				transforms.Add(obj.transform);
				((IEntityIndex)obj).EntityIndex = (archetypeIndex << ArchetypeOffset) | (objectCount-1);
			}

			public void Remove (int index) {
				objectCount--;
				((IEntityIndex)objects[objectCount]).EntityIndex = (archetypeIndex << ArchetypeOffset) | index;
				((IEntityIndex)objects[index]).EntityIndex = 0;
				objects[index] = objects[objectCount];
				objects[objectCount] = null;
				transforms.RemoveAtSwapBack(index);

				if (objectCount == 0) transforms.Dispose();
			}
		}

#if UNITY_EDITOR
		void DelayedDestroy () {
			UnityEditor.EditorApplication.update -= DelayedDestroy;
			GameObject.DestroyImmediate(gameObject);
		}
#endif

		void OnEnable () {
			if (instance == null) instance = this;
			if (instance != this) {
				// We cannot destroy the object while it is being enabled, so we need to delay it a bit
#if UNITY_EDITOR
				// This is only important in the editor to avoid a build-up of old managers.
				// In an actual game at most 1 (though in practice zero) old managers will be laying around.
				// It would be nice to use a coroutine for this instead, but unfortunately they do not work for objects marked with HideAndDontSave.
				UnityEditor.EditorApplication.update += DelayedDestroy;
#endif
			}
		}

		void OnDisable () {
			if (instance == this) instance = null;
		}

		static void CreateInstance () {
			// If scripts are recompiled the the static variable will be lost.
			// Some users recompile scripts in play mode and then reload the scene (https://forum.arongranberg.com/t/rts-game-pathfinding/6623/48?u=aron_granberg)
			// which makes handling this a requirement.

			// Here one might try to look for existing instances of the class that haven't yet been enabled.
			// However, this turns out to be tricky.
			// Resources.FindObjectsOfTypeAll<T>() is the only call that includes HideInInspector GameObjects.
			// But it is hard to distinguish between objects that are internal ones which will never be enabled and objects that will be enabled.
			// Checking .gameObject.scene.isLoaded doesn't work reliably (object may be enabled and working even if isLoaded is false)
			// Checking .gameObject.scene.isValid doesn't work reliably (object may be enabled and working even if isValid is false)

			// So instead we just always create a new instance. This is not a particularly heavy operation and it only happens once per game, so why not.
			// The OnEnable call will clean up duplicate managers if there are any.

			var go = new GameObject("Batch Helper") {
				hideFlags = HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInInspector | HideFlags.HideInHierarchy
			};

			instance = go.AddComponent<BatchedEvents>();
			DontDestroyOnLoad(go);
		}

		public static T Find<T, K>(K key, System.Func<T, K, bool> predicate) where T : class, IEntityIndex {
			var t = typeof(T);
			for (int i = 0; i < data.Length; i++) {
				if (data[i].type == t) {
					var objs = data[i].objects as T[];
					for (int j = 0; j < data[i].objectCount; j++) {
						if (predicate(objs[j], key)) return objs[j];
					}
				}
			}
			return null;
		}

		public static void Remove<T>(T obj) where T : IEntityIndex {
			int index = obj.EntityIndex;

			if (index == 0) return;

			var archetypeIndex = ((index & ArchetypeMask) >> ArchetypeOffset) - 1;
			index &= ~ArchetypeMask;
			UnityEngine.Assertions.Assert.IsTrue(data[archetypeIndex].type == obj.GetType());

			if (isIterating && isIteratingOverTypeIndex == archetypeIndex) throw new System.Exception("Cannot add or remove entities during an event (Update/LateUpdate/...) that this helper initiated");
			data[archetypeIndex].Remove(index);
		}

		public static int GetComponents<T>(Event eventTypes, out TransformAccessArray transforms, out T[] components) where T : Component, IEntityIndex {
			if (instance == null) CreateInstance();

			// Add in a hash of the event types
			var archetypeVariant = (int)eventTypes * 12582917;
			if (isIterating && isIteratingOverTypeIndex == archetypeVariant) throw new System.Exception("Cannot add or remove entities during an event (Update/LateUpdate/...) that this helper initiated");

			var type = typeof(T);
			for (int i = 0; i < data.Length; i++) {
				if (data[i].type == type && data[i].variant == archetypeVariant) {
					transforms = data[i].transforms;
					components = data[i].objects as T[];
					return data[i].objectCount;
				}
			}

			transforms = default;
			components = null;
			return 0;
		}

		public static bool Has<T>(T obj) where T : IEntityIndex => obj.EntityIndex != 0;

		public static void Add<T>(T obj, Event eventTypes, System.Action<T[], int> action, int archetypeVariant = 0) where T : Component, IEntityIndex {
			Add(obj, eventTypes, null, action, archetypeVariant);
		}

		public static void Add<T>(T obj, Event eventTypes, System.Action<T[], int, TransformAccessArray, Event> action, int archetypeVariant = 0) where T : Component, IEntityIndex {
			Add(obj, eventTypes, action, null, archetypeVariant);
		}

		static void Add<T>(T obj, Event eventTypes, System.Action<T[], int, TransformAccessArray, Event> action1, System.Action<T[], int> action2, int archetypeVariant = 0) where T : Component, IEntityIndex {
			if (obj.EntityIndex != 0) {
				throw new System.ArgumentException("This object is already registered. Call Remove before adding the object again.");
			}

			if (instance == null) CreateInstance();

			// Add in a hash of the event types
			archetypeVariant = (int)eventTypes * 12582917;
			if (isIterating && isIteratingOverTypeIndex == archetypeVariant) throw new System.Exception("Cannot add or remove entities during an event (Update/LateUpdate/...) that this helper initiated");


			var type = obj.GetType();
			for (int i = 0; i < data.Length; i++) {
				if (data[i].type == type && data[i].variant == archetypeVariant) {
					data[i].Add(obj);
					return;
				}
			}

			{
				Memory.Realloc(ref data, data.Length + 1);
				// A copy is made here so that these variables are captured by the lambdas below instead of the original action1/action2 parameters.
				// If this is not done then the C# JIT will allocate a lambda capture object every time this function is executed
				// instead of only when we need to create a new archetype. Doing that would create a lot more unnecessary garbage.
				var ac1 = action1;
				var ac2 = action2;
				System.Action<object[], int, TransformAccessArray, Event> a1 = (objs, count, tr, ev) => ac1((T[])objs, count, tr, ev);
				System.Action<object[], int, TransformAccessArray, Event> a2 = (objs, count, tr, ev) => ac2((T[])objs, count);
				data[data.Length - 1] = new Archetype {
					type = type,
					events = eventTypes,
					variant = archetypeVariant,
					archetypeIndex = (data.Length - 1) + 1, // Note: offset by +1 to ensure that entity index = 0 is an invalid index
					action = ac1 != null ? a1 : a2,
					sampler = CustomSampler.Create(type.Name),
				};
				data[data.Length - 1].Add(obj);
			}
		}

		void Process (Event eventType, System.Type typeFilter) {
			try {
				isIterating = true;
				for (int i = 0; i < data.Length; i++) {
					ref var archetype = ref data[i];
					if (archetype.objectCount > 0 && (archetype.events & eventType) != 0 && (typeFilter == null || typeFilter == archetype.type)) {
						isIteratingOverTypeIndex = archetype.variant;
						try {
							archetype.sampler.Begin();
							archetype.action(archetype.objects, archetype.objectCount, archetype.transforms, eventType);
						} finally {
							archetype.sampler.End();
						}
					}
				}
			} finally {
				isIterating = false;
			}
		}

		public static void ProcessEvent<T>(Event eventType) {
			instance?.Process(eventType, typeof(T));
		}

		void Update () {
			Process(Event.Update, null);
		}

		void LateUpdate () {
			Process(Event.LateUpdate, null);
		}

		void FixedUpdate () {
			Process(Event.FixedUpdate, null);
		}
	}
}
