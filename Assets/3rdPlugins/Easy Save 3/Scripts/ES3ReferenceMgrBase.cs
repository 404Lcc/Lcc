using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

namespace ES3Internal
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public abstract class ES3ReferenceMgrBase : MonoBehaviour
    {
        internal object _lock = new object();

        public const string referencePropertyName = "_ES3Ref";
        private static ES3ReferenceMgrBase _current = null;
        private static HashSet<ES3ReferenceMgrBase> mgrs = new HashSet<ES3ReferenceMgrBase>();
#if UNITY_EDITOR
        protected static bool isEnteringPlayMode = false;
        static readonly HideFlags[] invalidHideFlags = new HideFlags[] { HideFlags.HideInHierarchy, HideFlags.DontSave, HideFlags.DontSaveInBuild, HideFlags.DontSaveInEditor, HideFlags.HideAndDontSave };
#endif

#if !UNITY_EDITOR
        [NonSerialized]
#endif
        public List<UnityEngine.Object> excludeObjects = new List<UnityEngine.Object>();

        private static System.Random rng;

        [HideInInspector]
        public bool openPrefabs = false; // Whether the prefab list should be open in the Editor.

        public List<ES3Prefab> prefabs = new List<ES3Prefab>();

        public static ES3ReferenceMgrBase Current
        {
            get
            {
                // If the reference manager hasn't been assigned, or we've got a reference to a manager in a different scene which isn't marked as DontDestroyOnLoad, look for this scene's manager.
                if (_current == null /*|| (_current.gameObject.scene.buildIndex != -1 && _current.gameObject.scene != SceneManager.GetActiveScene())*/)
                {
                    ES3ReferenceMgrBase mgr = GetManagerFromScene(SceneManager.GetActiveScene());
                    if (mgr != null)
                        mgrs.Add(_current = mgr);
                }
                return _current;
            }
        }

        public static ES3ReferenceMgrBase GetManagerFromScene(Scene scene, bool getAnyManagerIfNotInScene = true)
        {
            // This has been removed as isLoaded is false during the initial Awake().
            /*if (!scene.isLoaded)
                return null;*/

            // If this is a valid scene, search it for the manager.
            if (scene.IsValid())
            {
                // Check whether the mgr is already in the mgr list.
                foreach (var addedMgr in mgrs)
                    if (addedMgr != null && addedMgr.gameObject.scene == scene)
                        return addedMgr;

                GameObject[] roots;
                try
                {
                    roots = scene.GetRootGameObjects();
                }
                catch
                {
                    return null;
                }

                // First, look for Easy Save 3 Manager in the top-level.
                foreach (var root in roots)
                {
                    if (root.name == "Easy Save 3 Manager")
                    {
                        var mgr = root.GetComponent<ES3ReferenceMgr>();
                        if(mgr != null)
                            return mgr;
                    }
                }

                // If the user has moved or renamed the Easy Save 3 Manager, we need to perform a deep search.
                foreach (var root in roots)
                {
                    var mgr = root.GetComponentInChildren<ES3ReferenceMgr>();
                    if(mgr != null)
                        return mgr;
                }
            }

            // If we can't find a manager in this scene (for example we're in DontDestroyOnLoad), find a manager in any scene.
            if (getAnyManagerIfNotInScene)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var loadedScene = SceneManager.GetSceneAt(i);

                    if (loadedScene != null && loadedScene != scene && loadedScene.IsValid())
                    {
                        var mgr = GetManagerFromScene(loadedScene, false);
                        if (mgr != null)
                        {
                            ES3Debug.LogWarning($"The reference you're trying to save does not exist in any scene, or the scene it belongs to does not contain an Easy Save 3 Manager. Using the reference manager from scene {loadedScene.name} instead. This may cause unexpected behaviour or leak memory in some situations. See <a href=\"https://docs.moodkie.com/easy-save-3/es3-guides/saving-and-loading-references/\">the Saving and Loading References guide</a> for more information.");
                            return mgr;
                        }
                    }
                }
            }
            return null;
        }

        public bool IsInitialised { get { return idRef.Count > 0; } }

        [SerializeField]
        public ES3IdRefDictionary idRef = new ES3IdRefDictionary();
        private ES3RefIdDictionary _refId = null;

        public ES3RefIdDictionary refId
        {
            get
            {
                if (_refId == null)
                {
                    _refId = new ES3RefIdDictionary();
                    // Populate the reverse dictionary with the items from the normal dictionary.
                    foreach (var kvp in idRef)
                        if (kvp.Value != null)
                            _refId[kvp.Value] = kvp.Key;
                }
                return _refId;
            }
            set
            {
                _refId = value;
            }
        }

        public ES3GlobalReferences GlobalReferences
        {
            get
            {
                return ES3GlobalReferences.Instance;
            }
        }

        // Reset static variables to handle disabled domain reloading.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            _current = null;
            mgrs = new HashSet<ES3ReferenceMgrBase>();
#if UNITY_EDITOR
            isEnteringPlayMode = false;
#endif
            rng = null;
        }

        internal void Awake()
        {
            if (_current != null && _current != this)
            {
                var existing = _current;

                /* We intentionally use Current rather than _current here, as _current may contain a reference to a manager in another scene, 
                 * but Current only returns the Manager for the active scene. */
                if (Current != null)
                {
                    RemoveNullValues();

                    //existing.Merge(this);
                    //Destroy(this);
                    _current = existing; // Undo the call to Current, which may have set it to NULL.
                }
            }
            else
                _current = this;
            mgrs.Add(this);
        }

        private void OnDestroy()
        {
            if (_current == this)
                _current = null;
            mgrs.Remove(this);
        }

        // Merges two managers, not allowing any clashes of IDs
        public void Merge(ES3ReferenceMgrBase otherMgr)
        {
            foreach (var kvp in otherMgr.idRef)
                Add(kvp.Value, kvp.Key);
        }

        public long Get(UnityEngine.Object obj)
        {
            if (!mgrs.Contains(this))
                mgrs.Add(this);

            foreach (var mgr in mgrs)
            {
                if (mgr == null)
                    continue;

                if (obj == null)
                    return -1;

                long id;
                if (mgr.refId.TryGetValue(obj, out id))
                    return id;
            }
            return -1;
        }

        internal UnityEngine.Object Get(long id, Type type, bool suppressWarnings = false)
        {
            if (!mgrs.Contains(this))
                mgrs.Add(this);

            foreach (var mgr in mgrs)
            {
                if (mgr == null)
                    continue;

                if (id == -1)
                    return null;

                UnityEngine.Object obj;
                if (mgr.idRef.TryGetValue(id, out obj))
                {
                    if (obj == null) // If obj has been marked as destroyed but not yet destroyed, don't return it.
                        return null;
                    return obj;
                }
            }

            if (GlobalReferences != null)
            {
                var globalRef = GlobalReferences.Get(id);
                if (globalRef != null)
                    return globalRef;
            }

            if (!suppressWarnings)
            {
                if (type != null)
                    ES3Debug.LogWarning("Reference for " + type + " with ID " + id + " could not be found in Easy Save's reference manager. See <a href=\"https://docs.moodkie.com/easy-save-3/es3-guides/saving-and-loading-references/#reference-could-not-be-found-warning\">the Saving and Loading References guide</a> for more information.", this);
                else
                    ES3Debug.LogWarning("Reference with ID " + id + " could not be found in Easy Save's reference manager. See <a href=\"https://docs.moodkie.com/easy-save-3/es3-guides/saving-and-loading-references/#reference-could-not-be-found-warning\">the Saving and Loading References guide</a> for more information.", this);
            }

            return null;
        }

        public UnityEngine.Object Get(long id, bool suppressWarnings = false)
        {
            return Get(id, null, suppressWarnings);
        }

        public ES3Prefab GetPrefab(long id, bool suppressWarnings = false)
        {
            if (!mgrs.Contains(this))
                mgrs.Add(this);

            foreach (var mgr in mgrs)
            {
                if (mgr == null)
                    continue;

                foreach (var prefab in mgr.prefabs)
                    if (prefab != null && prefab.prefabId == id)
                        return prefab;
            }
            if (!suppressWarnings)
                ES3Debug.LogWarning("Prefab with ID " + id + " could not be found in Easy Save's reference manager. Try pressing the Refresh References button on the ES3ReferenceMgr Component of the Easy Save 3 Manager in your scene, or exit play mode and right-click the prefab and select Easy Save 3 > Add Reference(s) to Manager.", this);
            return null;
        }

        public long GetPrefab(ES3Prefab prefabToFind, bool suppressWarnings = false)
        {
            if (!mgrs.Contains(this))
                mgrs.Add(this);

            foreach (var mgr in mgrs)
            {
                if (mgr == null)
                    continue;

                foreach (var prefab in prefabs)
                    if (prefab == prefabToFind)
                        return prefab.prefabId;
            }
            if (!suppressWarnings)
                ES3Debug.LogWarning("Prefab with name " + prefabToFind.name + " could not be found in Easy Save's reference manager. Try pressing the Refresh References button on the ES3ReferenceMgr Component of the Easy Save 3 Manager in your scene, or exit play mode and right-click the prefab and select Easy Save 3 > Add Reference(s) to Manager.", prefabToFind);
            return -1;
        }

        public long Add(UnityEngine.Object obj)
        {
            if (obj == null)
                return -1;

            if (!CanBeSaved(obj))
                return -1;

            long id;
            // If it already exists in the list, do nothing.
            if (refId.TryGetValue(obj, out id))
                return id;

            if (GlobalReferences != null)
            {
                id = GlobalReferences.GetOrAdd(obj);
                if (id != -1)
                {
                    Add(obj, id);
                    return id;
                }
            }

            lock (_lock)
            {
                // Add the reference to the Dictionary.
                id = GetNewRefID();
                return Add(obj, id);
            }
        }

        public long Add(UnityEngine.Object obj, long id)
        {
            if (obj == null)
                return -1;

            if (!CanBeSaved(obj))
                return -1;

            // If the ID is -1, auto-generate an ID.
            if (id == -1)
                id = GetNewRefID();
            // Add the reference to the Dictionary.
            lock (_lock)
            {
                idRef[id] = obj;
                if (obj != null)
                    refId[obj] = id;
            }
            return id;
        }

        public bool AddPrefab(ES3Prefab prefab)
        {
            if (!prefabs.Contains(prefab))
            {
                prefabs.Add(prefab);
                return true;
            }
            return false;
        }

        public void Remove(UnityEngine.Object obj)
        {
            if (!mgrs.Contains(this))
                mgrs.Add(this);

            foreach (var mgr in mgrs)
            {
                if (mgr == null)
                    continue;

                // Only remove from this manager if we're in the Editor.
                if (!Application.isPlaying && mgr != this)
                    continue;

                lock (mgr._lock)
                {
                    mgr.refId.Remove(obj);
                    // There may be multiple references with the same ID, so remove them all.
                    foreach (var item in mgr.idRef.Where(kvp => kvp.Value == obj).ToList())
                        mgr.idRef.Remove(item.Key);
                }
            }
        }

        public void Remove(long referenceID)
        {
            foreach (var mgr in mgrs)
            {
                if (mgr == null)
                    continue;

                lock (mgr._lock)
                {
                    mgr.idRef.Remove(referenceID);
                    // There may be multiple references with the same ID, so remove them all.
                    foreach (var item in mgr.refId.Where(kvp => kvp.Value == referenceID).ToList())
                        mgr.refId.Remove(item.Key);
                }
            }
        }

        public void RemoveNullValues()
        {
            var nullKeys = idRef.Where(pair => pair.Value == null).Select(pair => pair.Key).ToList();
            foreach (var key in nullKeys)
                idRef.Remove(key);
        }

        public void RemoveNullOrInvalidValues()
        {
            var nullKeys = idRef.Where(pair => pair.Value == null || !CanBeSaved(pair.Value) || excludeObjects.Contains(pair.Value)).Select(pair => pair.Key).ToList();
            foreach (var key in nullKeys)
                idRef.Remove(key);

            if (GlobalReferences != null)
                GlobalReferences.RemoveInvalidKeys();
        }

        public void Clear()
        {
            lock (_lock)
            {
                refId.Clear();
                idRef.Clear();
            }
        }

        public bool Contains(UnityEngine.Object obj)
        {
            return refId.ContainsKey(obj);
        }

        public bool Contains(long referenceID)
        {
            return idRef.ContainsKey(referenceID);
        }

        public void ChangeId(long oldId, long newId)
        {
            idRef.ChangeKey(oldId, newId);
            // Empty the refId so it has to be refreshed.
            refId = null;
        }

        internal static long GetNewRefID()
        {
            if (rng == null)
                rng = new System.Random();

            byte[] buf = new byte[8];
            rng.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (System.Math.Abs(longRand % (long.MaxValue - 0)) + 0);
        }

        /*#if UNITY_EDITOR
                public static HashSet<UnityEngine.Object> CollectDependenciesLegacy(UnityEngine.Object obj, HashSet<UnityEngine.Object> dependencies = null, int depth = int.MinValue)
                {
                    return CollectDependenciesLegacy(new UnityEngine.Object[] { obj }, dependencies, depth);
                }


                 //Collects all top-level dependencies of an object.
                 //For GameObjects, it will traverse all children.
                 //For Components or ScriptableObjects, it will get all serialisable UnityEngine.Object fields/properties as dependencies.
                public static HashSet<UnityEngine.Object> CollectDependenciesLegacy(UnityEngine.Object[] objs, HashSet<UnityEngine.Object> dependencies = null, int depth = int.MinValue)
                {
                    if (depth == int.MinValue)
                        depth = ES3Settings.defaultSettingsScriptableObject.collectDependenciesDepth;

                    if (depth < 0)
                        return dependencies;

                    if (dependencies == null)
                        dependencies = new HashSet<UnityEngine.Object>();

                    foreach (var obj in objs)
                    {
                        if (obj == null)
                            continue;

                        var type = obj.GetType();

                        // Skip types which don't need processing
                        if (type == typeof(ES3ReferenceMgr) || type == typeof(ES3AutoSaveMgr) || type == typeof(ES3AutoSave) || type == typeof(ES3InspectorInfo))
                            continue;

                        // Add the prefab to the manager but don't process it. We'll use this to work out what prefabs to add to the prefabs list later.
                        if (type == typeof(ES3Prefab))
                        {
                            dependencies.Add(obj);
                            continue;
                        }

                        // If it's a GameObject, get the GameObject's Components and collect their dependencies.
                        if (type == typeof(GameObject))
                        {
                            var go = (GameObject)obj;
                            // If we've not already processed this GameObject ...
                            if (dependencies.Add(go))
                            {
                                // Get the dependencies of each Component in the GameObject.
                                CollectDependenciesLegacy(go.GetComponents<Component>(), dependencies, depth - 1);
                                // Get the dependencies of each child in the GameObject.
                                foreach (Transform child in go.transform)
                                    CollectDependenciesLegacy(child.gameObject, dependencies, depth); // Don't decrement child, as we consider this a top-level object.
                            }
                        }
                        // Else if it's a Component or ScriptableObject, add the values of any UnityEngine.Object fields as dependencies.
                        else
                            CollectDependenciesFromFieldsLegacy(obj, dependencies, depth - 1);
                    }

                    return dependencies;
                }

                private static void CollectDependenciesFromFieldsLegacy(UnityEngine.Object obj, HashSet<UnityEngine.Object> dependencies, int depth)
                {
                    // If we've already collected dependencies for this, do nothing.
                    if (!dependencies.Add(obj))
                        return;

                    if (depth == int.MinValue)
                        depth = ES3Settings.defaultSettingsScriptableObject.collectDependenciesDepth;

                    if (depth < 0)
                        return;

                    var type = obj.GetType();

                    if (isEnteringPlayMode && type == typeof(UnityEngine.UI.Text))
                        return;

                    try
                    {
                        // SerializedObject is expensive, so for known classes we manually gather references.

                        if (type == typeof(Animator) || obj is Transform || type == typeof(CanvasRenderer) || type == typeof(Mesh) || type == typeof(AudioClip) || type == typeof(Rigidbody) || obj is HorizontalOrVerticalLayoutGroup)
                            return;

                        if(obj is Texture)
                        {
                            // This ensures that Sprites which are children of the Texture are also added. In the Editor you would otherwise need to expand the Texture to add the Sprite.
                            foreach(var dependency in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(obj)))
                                if (dependency != obj)
                                    dependencies.Add(dependency);
                        }

                        if (obj is Graphic)
                        {
                            var m = (Graphic)obj;
                            dependencies.Add(m.material);
                            dependencies.Add(m.defaultMaterial);
                            dependencies.Add(m.mainTexture);

                            if (type == typeof(Text))
                            {
                                var text = (Text)obj;
                                dependencies.Add(text.font);
                            }
                            else if (type == typeof(Image))
                            {
                                var img = (Image)obj;
                                dependencies.Add(img.sprite);
                            }
                            return;
                        }

                        if (type == typeof(Mesh))
                        {
                            if (UnityEditor.AssetDatabase.Contains(obj))
                                dependencies.Add(obj);
                            return;
                        }

                        if (type == typeof(Material))
                        {
                            var material = (Material)obj;
                            var shader = material.shader;
                            if (shader != null)
                            {
                                dependencies.Add(material.shader);

        #if UNITY_2019_3_OR_NEWER
                                for (int i = 0; i < shader.GetPropertyCount(); i++)
                                    if (shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
                                        dependencies.Add(material.GetTexture(shader.GetPropertyName(i)));
                            }
        #endif

                            return;
                        }

                        if (type == typeof(MeshFilter))
                        {
                            dependencies.Add(((MeshFilter)obj).sharedMesh);
                            return;
                        }

                        if (type == typeof(MeshCollider))
                        {
                            var mc = (MeshCollider)obj;
                            dependencies.Add(mc.sharedMesh);
                            dependencies.Add(mc.sharedMaterial);
                            dependencies.Add(mc.attachedRigidbody);
                            return;
                        }

                        if (type == typeof(Camera))
                        {
                            var c = (Camera)obj;
                            dependencies.Add(c.targetTexture);
                            return;
                        }

                        if (type == typeof(SkinnedMeshRenderer))
                            dependencies.Add(((SkinnedMeshRenderer)obj).sharedMesh); // Don't return. Let this fall through to the if(obj is renderer) call.
                        else if (type == typeof(SpriteRenderer))
                            dependencies.Add(((SpriteRenderer)obj).sprite); // Don't return. Let this fall through to the if(obj is renderer) call.
                        else if (type == typeof(ParticleSystemRenderer))
                            dependencies.Add(((ParticleSystemRenderer)obj).mesh); // Don't return. Let this fall through to the if(obj is renderer) call.

                        if (obj is Renderer)
                        {
                            var renderer = (Renderer)obj;
                            foreach (var material in renderer.sharedMaterials)
                                CollectDependenciesFromFieldsLegacy(material, dependencies, depth - 1);
                            return;
                        }
                    }
                    catch { }

                    var so = new UnityEditor.SerializedObject(obj);
                    if (so == null)
                        return;

                    var property = so.GetIterator();
                    if (property == null)
                        return;

                    // Iterate through each of this object's properties.
                    while (property.NextVisible(true))
                    {
                        try
                        {
                            // If it's an array which contains UnityEngine.Objects, add them as dependencies.
                            if (property.isArray && property.propertyType != UnityEditor.SerializedPropertyType.String)
                            {
                                for (int i = 0; i < property.arraySize; i++)
                                {
                                    var element = property.GetArrayElementAtIndex(i);

                                    // If the array contains UnityEngine.Object types, add them to the dependencies.
                                    if (element.propertyType == UnityEditor.SerializedPropertyType.ObjectReference)
                                    {
                                        var elementValue = element.objectReferenceValue;
                                        var elementType = elementValue.GetType();

                                        // If it's a GameObject, use CollectDependencies so that Components are also added.
                                        if (elementType == typeof(GameObject))
                                            CollectDependenciesLegacy(elementValue, dependencies, depth - 1);
                                        else
                                            CollectDependenciesFromFieldsLegacy(elementValue, dependencies, depth - 1);
                                    }
                                    // Otherwise this array does not contain UnityEngine.Object types, so we should stop.
                                    else
                                        break;
                                }
                            }
                            // Else if it's a normal UnityEngine.Object field, add it.
                            else if (property.propertyType == UnityEditor.SerializedPropertyType.ObjectReference)
                            {
                                var propertyValue = property.objectReferenceValue;
                                if (propertyValue == null)
                                    continue;

                                // If it's a GameObject, use CollectDependencies so that Components are also added.
                                if (propertyValue.GetType() == typeof(GameObject))
                                    CollectDependenciesLegacy(propertyValue, dependencies, depth - 1);
                                else
                                    CollectDependenciesFromFieldsLegacy(propertyValue, dependencies, depth - 1);
                            }
                        }
                        catch { }
                    }
                }

                // Called in the Editor when this Component is added.
                private void Reset()
                {
                    // Ensure that Component can only be added by going to Assets > Easy Save 3 > Add Manager to Scene.
                    if (gameObject.name != "Easy Save 3 Manager")
                    {
                        UnityEditor.EditorUtility.DisplayDialog("Cannot add ES3ReferenceMgr directly", "Please go to 'Tools > Easy Save 3 > Add Manager to Scene' to add an Easy Save 3 Manager to your scene.", "Ok");
                        DestroyImmediate(this);
                    }
                }
        #endif*/

        internal static bool CanBeSaved(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (obj == null)
                return true;

            foreach (var flag in invalidHideFlags)
                if ((obj.hideFlags & flag) != 0 && /*obj.hideFlags != HideFlags.HideInHierarchy &&*/ obj.hideFlags != HideFlags.HideInInspector && obj.hideFlags != HideFlags.NotEditable)
                    if (!(obj is Mesh || obj is Material))
                        return false;

            if (obj is UnityEngine.U2D.SpriteAtlas)
                return false;

            // Exclude the Easy Save 3 Manager, and all components attached to it.
            if (obj.name == "Easy Save 3 Manager")
                return false;
#endif
            return true;
        }

#if UNITY_EDITOR
        public void ExcludeObject(UnityEngine.Object obj)
        {
            if (excludeObjects == null)
                excludeObjects = new List<UnityEngine.Object>();

            if (!excludeObjects.Contains(obj))
                excludeObjects.Add(obj);
        }
#endif
    }

    [System.Serializable]
    public class ES3IdRefDictionary : ES3SerializableDictionary<long, UnityEngine.Object>
    {
        protected override bool KeysAreEqual(long a, long b)
        {
            return a == b;
        }

        protected override bool ValuesAreEqual(UnityEngine.Object a, UnityEngine.Object b)
        {
            return a == b;
        }
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [System.Serializable]
    public class ES3RefIdDictionary : ES3SerializableDictionary<UnityEngine.Object, long>
    {
        protected override bool KeysAreEqual(UnityEngine.Object a, UnityEngine.Object b)
        {
            return a == b;
        }

        protected override bool ValuesAreEqual(long a, long b)
        {
            return a == b;
        }
    }
}