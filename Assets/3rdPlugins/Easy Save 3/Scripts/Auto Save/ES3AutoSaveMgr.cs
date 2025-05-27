using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

#if UNITY_VISUAL_SCRIPTING
[Unity.VisualScripting.IncludeInSettings(true)]
#elif BOLT_VISUAL_SCRIPTING
[Ludiq.IncludeInSettings(true)]
#endif
public class ES3AutoSaveMgr : MonoBehaviour
{
	public static ES3AutoSaveMgr _current = null;
    public static ES3AutoSaveMgr Current
    {
        get
        {
            if (_current == null /*|| _current.gameObject.scene != SceneManager.GetActiveScene()*/)
            {
                var scene = SceneManager.GetActiveScene();
                var roots = scene.GetRootGameObjects();

                // First, look for Easy Save 3 Manager in the top-level.
                foreach (var root in roots)
                    if (root.name == "Easy Save 3 Manager")
                        return _current = root.GetComponent<ES3AutoSaveMgr>();

                // If the user has moved or renamed the Easy Save 3 Manager, we need to perform a deep search.
                foreach (var root in roots)
                    if ((_current = root.GetComponentInChildren<ES3AutoSaveMgr>()) != null)
                        return _current;
            }
            return _current;
        }
    }

    public static Dictionary<Scene, ES3AutoSaveMgr> managers = new Dictionary<Scene, ES3AutoSaveMgr>();

    // Included for backwards compatibility.
    public static ES3AutoSaveMgr Instance
    {
        get { return Current; }
    }

    public enum LoadEvent { None, Awake, Start }
	public enum SaveEvent { None, OnApplicationQuit, OnApplicationPause }

	public string key = System.Guid.NewGuid().ToString();
    public bool immediatelyCommitToFile = true;
	public SaveEvent saveEvent = SaveEvent.OnApplicationQuit;
	public LoadEvent loadEvent = LoadEvent.Start;
	public ES3SerializableSettings settings = new ES3SerializableSettings("SaveFile.es3", ES3.Location.Cache);

	public HashSet<ES3AutoSave> autoSaves = new HashSet<ES3AutoSave>();

    List<long> destroyedIds = new List<long>();

    public void Save()
	{
        if (autoSaves == null || autoSaves.Count == 0)
            return;

        ManageSlots();

        // If we're using caching and we've not already cached this file, cache it.
        if (settings.location == ES3.Location.Cache && !ES3.FileExists(settings))
            ES3.CacheFile(settings);

        if (autoSaves == null || autoSaves.Count == 0)
        {
            ES3.DeleteKey(key, settings);
        }
        else
        {
            var gameObjects = new List<GameObject>();
            foreach (var autoSave in autoSaves)
            {
                // If the ES3AutoSave component is disabled, don't save it.
                if (autoSave != null && autoSave.enabled)
                    gameObjects.Add(autoSave.gameObject);
            }

            // Save in the same order as their depth in the hierarchy.
            ES3.Save(key, gameObjects.OrderBy(x => GetDepth(x.transform)).ToArray(), settings);

            if(destroyedIds != null && destroyedIds.Count > 0)
                ES3.Save($"{key}_destroyed", destroyedIds, settings);
        }

        if(immediatelyCommitToFile && settings.location == ES3.Location.Cache && ES3.FileExists(settings))
            ES3.StoreCachedFile(settings);
	}

	public void Load()
	{
        ManageSlots();

        try
        {
            // If we're using caching and we've not already cached this file, cache it.
            if (settings.location == ES3.Location.Cache && !ES3.FileExists(settings))
                ES3.CacheFile(settings);
        }
        catch { }


        // Ensure that the reference manager for this scene has been initialised.
        var mgr = ES3ReferenceMgr.GetManagerFromScene(this.gameObject.scene, false);
        mgr.Awake();

        ES3.Load<GameObject[]>(key, new GameObject[0], settings);

        // Destroy any objects for which the destroyed state was saved.
        foreach(var id in ES3.Load($"{key}_destroyed", new List<long>(), settings))
        {
            var go = mgr.Get(id, true);
            if(go != null)
            {
                var autoSave = ((GameObject)go).GetComponent<ES3AutoSave>();
                if(autoSave != null)
                    DestroyAutoSave(autoSave);
                Destroy(go);
            }
        }
    }

    void Start()
	{
		if(loadEvent == LoadEvent.Start)
			Load();
	}

    public void Awake()
    {
        managers[this.gameObject.scene] = this;
        GetAutoSaves();

        if (loadEvent == LoadEvent.Awake)
            Load();
    }

    void OnApplicationQuit()
	{
		if(saveEvent == SaveEvent.OnApplicationQuit)
			Save();
	}

	void OnApplicationPause(bool paused)
	{
		if(	(saveEvent == SaveEvent.OnApplicationPause || 
			(Application.isMobilePlatform && saveEvent == SaveEvent.OnApplicationQuit)) && paused)
			Save();
	}

	/* Register an ES3AutoSave with the ES3AutoSaveMgr, if there is one */
	public static void AddAutoSave(ES3AutoSave autoSave)
	{
        if (autoSave == null)
            return;

        ES3AutoSaveMgr mgr;
        if (managers.TryGetValue(autoSave.gameObject.scene, out mgr))
            mgr.autoSaves.Add(autoSave);
	}

	/* Remove an ES3AutoSave from the ES3AutoSaveMgr, for example if it's GameObject has been destroyed */
	public static void DestroyAutoSave(ES3AutoSave autoSave)
	{
        if (autoSave == null)
            return;

        ES3AutoSaveMgr mgr;
        if (managers.TryGetValue(autoSave.gameObject.scene, out mgr))
        {
            mgr.autoSaves.Remove(autoSave);

            // Get the reference ID of the GameObject and add it to the destroyed list if it's not a prefab instance.
            if (autoSave.saveDestroyed)
            {
                var refMgr = ES3ReferenceMgr.GetManagerFromScene(autoSave.gameObject.scene, true);
                if (refMgr != null)
                {
                    var id = refMgr.Add(autoSave.gameObject);
                    if (id != -1)
                        mgr.destroyedIds.Add(id);
                }
            }
        }
	}

    /* Gathers all of the ES3AutoSave Components in the scene and registers them with the manager */
    public void GetAutoSaves()
    {
        autoSaves = new HashSet<ES3AutoSave>();

        foreach (var go in this.gameObject.scene.GetRootGameObjects())
            autoSaves.UnionWith(go.GetComponentsInChildren<ES3AutoSave>(true));
    }

    // Gets the depth of a Transform in the hierarchy.
    static int GetDepth(Transform t)
    {
        int depth = 0;

        while (t.parent != null)
        {
            t = t.parent;
            depth++;
        }

        return depth;
    }

    // Changes the path for this ES3AutoSave if we're using save slots.
    void ManageSlots()
    {
#if ES3_TMPRO && ES3_UGUI
        if (ES3SlotManager.selectedSlotPath != null)
            settings.path = ES3SlotManager.selectedSlotPath;
#endif
    }
}
