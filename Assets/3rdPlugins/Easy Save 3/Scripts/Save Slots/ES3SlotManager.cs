using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if ES3_TMPRO && ES3_UGUI

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ES3SlotManager : MonoBehaviour
{
    [Tooltip("Shows a confirmation if this slot already exists when we select it.")]
    public bool showConfirmationIfExists = true;
    [Tooltip("Whether the Create new slot button should be visible.")]
    public bool showCreateSlotButton = true;
    [Tooltip("Whether we should automatically create an empty save file when the user creates a new save slot. This will be created using the default settings, so you should set this to false if you are using ES3Settings objects.")]
    public bool autoCreateSaveFile = false;
    [Tooltip("Whether a save slot should be selected after a user creates it.")]
    public bool selectSlotAfterCreation = false;

    [Space(16)]

    [Tooltip("The name of a scene to load after the user chooses a slot.")]
    public string loadSceneAfterSelectSlot;

    [Space(16)]

    [Tooltip("An event called after a slot is selected, but before the scene specified by loadSceneAfterSelectSlot is loaded.")]
    public UnityEvent onAfterSelectSlot;

    [Tooltip("An event called after a slot is created by a user, but hasn't been selected.")]
    public UnityEvent onAfterCreateSlot;

    [Space(16)]

    [Tooltip("The subfolder we want to store our save files in. If this is a relative path, it will be relative to Application.persistentDataPath.")]
    public string slotDirectory = "slots/";
    [Tooltip("The extension we want to use for our save files.")]
    public string slotExtension = ".es3";

    [Space(16)]

    [Tooltip("The template we'll instantiate to create our slots.")]
    public GameObject slotTemplate;
    [Tooltip("The dialog box for creating a new slot.")]
    public GameObject createDialog;
    [Tooltip("The dialog box for displaying an error to the user.")]
    public GameObject errorDialog;

    // The relative path of the slot which has been selected, or null if none have been selected.
    public static string selectedSlotPath = null;

    // A list of slots which have been created.
    public List<GameObject> slots = new List<GameObject>();

    // If a file doesn't have a timestamp, it will return have this DateTime.
    static DateTime falseDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    // See Unity's docs for more info: https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html
    protected virtual void OnEnable()
    {
        // Deactivate the slot template so it's not visible.
        slotTemplate.SetActive(false);
        // Destroy any existing slots and start from fresh if necessary.
        DestroySlots();
        // Create our save slots if any exist.
        InstantiateSlots();
    }

    // Finds the save slot files and instantiates a save slot for each of them.
    protected virtual void InstantiateSlots()
    {
        // A list used to store our save slots so we can order them.
        List<(string Name, DateTime Timestamp)> slots = new List<(string Name, DateTime Timestamp)>();

        // If there are no slots to load, do nothing.
        if (!ES3.DirectoryExists(slotDirectory))
            return;

        // Put each of our slots into a List so we can order them.
        foreach (var file in ES3.GetFiles(slotDirectory))
        {
            // Get the slot name, which is the filename without the extension.
            var slotName = Path.GetFileNameWithoutExtension(file);
            // Get the timestamp so that we can display this to the user and use it to order the slots.
            var timestamp = ES3.GetTimestamp(GetSlotPath(slotName)).ToLocalTime();
            // Add the data to the slot list.
            slots.Add((Name: slotName, Timestamp: timestamp));
        }

        // Now order the slots by the timestamp.
        slots = slots.OrderByDescending(x => x.Timestamp).ToList();

        // Now create the slots.
        foreach (var slot in slots)
            InstantiateSlot(slot.Name, slot.Timestamp);
    }

    // Instantiates a single save slot with a given slot name and timestamp.
    public virtual ES3Slot InstantiateSlot(string slotName, DateTime timestamp)
    {
        // Create an instance of our slot.
        var slot = Instantiate(slotTemplate, slotTemplate.transform.parent);

        // Add it to our slot list.
        slots.Add(slot);

        // Ensure that we make it active as the template will be inactive.
        slot.SetActive(true);

        var es3SelectSlot = slot.GetComponent<ES3Slot>();
        es3SelectSlot.nameLabel.text = slotName.Replace('_', ' ');

        // If the file doesn't have a timestamp, don't display the timestamp.
        if (timestamp == falseDateTime)
            es3SelectSlot.timestampLabel.text = "";
        // Otherwise, set the label for the timestamp.
        else
            es3SelectSlot.timestampLabel.text = $"{timestamp.ToString("yyyy-MM-dd")}\n{timestamp.ToString("HH:mm:ss")}";

        return es3SelectSlot;
    }

    // Creates a new slot by instantiating it in the UI and creating a save file for it if necessary.
    public virtual ES3Slot CreateNewSlot(string slotName)
    {
        // Get the current timestamp.
        var creationTimestamp = DateTime.Now;
        // Create the slot in the UI.
        var slot = InstantiateSlot(slotName, creationTimestamp);
        // Move the slot to the top of the list.
        slot.MoveToTop();

        // Automatically create a file for the save slot if the option is enabled.
        if (autoCreateSaveFile)
            ES3.SaveRaw("{}", GetSlotPath(slotName));

        // Select the slot if necessary.
        if (selectSlotAfterCreation)
            slot.SelectSlot();

        // Scroll the scroll view to the top of the list.
        ScrollToTop();

        return slot;
    }

    // Shows the dialog displaying an error to the user.
    public virtual void ShowErrorDialog(string errorMessage)
    {
        errorDialog.transform.Find("Dialog Box/Message").GetComponent<TMP_Text>().text = errorMessage;
        errorDialog.SetActive(true);
    }

    #region Utility Methods

    // Destroys all slots which have been created, but doesn't delete their underlying save files.
    protected virtual void DestroySlots()
    {
        foreach (var slot in slots)
            Destroy(slot);
        slots.Clear();
    }

    // Gets the relative file path of the slot with the given slot name.
    public virtual string GetSlotPath(string slotName)
    {
        // We convert any whitespace characters to underscores at this point to make the file more portable.
        return slotDirectory + Regex.Replace(slotName, @"\s+", "_") + slotExtension;
    }

    // Scrolls to the top of the list of slots.
    public void ScrollToTop()
    {
        transform.Find("Scroll View").GetComponent<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition = 1f;
    }
    #endregion
}
#endif


#if UNITY_EDITOR
// Manages the context menu items for creating the slots.
public class ES3SlotMenuItems : MonoBehaviour
{
    [MenuItem("GameObject/Easy Save 3/Add Save Slots to Scene", false, 33)]
    [MenuItem("Assets/Easy Save 3/Add Save Slots to Scene", false, 33)]
    [MenuItem("Tools/Easy Save 3/Add Save Slots to Scene", false, 150)]
    public static void AddSaveSlotsToScene()
    {
#if !ES3_TMPRO || !ES3_UGUI
        EditorUtility.DisplayDialog("Cannot create save slots", "The 'TextMeshPro' and 'Unity UI' packages must be installed in Window > Package Manager to use Easy Save's slot functionality.", "Ok");
#else
        var mgr = AddSlotsToScene();
        mgr.gameObject.name = "Save Slots Canvas";
        mgr.transform.parent.gameObject.name = "Save Slots";
        mgr.showConfirmationIfExists = true;
        mgr.showCreateSlotButton = true;
        AddEventSystemToSceneIfNotExists();
#endif
    }

    [MenuItem("GameObject/Easy Save 3/Add Load Slots to Scene", false, 33)]
    [MenuItem("Assets/Easy Save 3/Add Load Slots to Scene", false, 33)]
    [MenuItem("Tools/Easy Save 3/Add Load Slots to Scene", false, 150)]
    public static void AddLoadSlotsToScene()
    {
#if !ES3_TMPRO || !ES3_UGUI
        EditorUtility.DisplayDialog("Cannot create save slots", "The 'TextMeshPro' and 'Unity UI' packages must be installed in Window > Package Manager to use Easy Save's slot functionality.", "Ok");
#else
        var mgr = AddSlotsToScene();
        mgr.gameObject.name = "Load Slots Canvas";
        mgr.transform.parent.gameObject.name = "Load Slots";
        mgr.showConfirmationIfExists = false;
        mgr.showCreateSlotButton = false;
        mgr.GetComponentInChildren<ES3CreateSlot>().gameObject.SetActive(false);
        AddEventSystemToSceneIfNotExists();
#endif
    }

#if ES3_TMPRO && ES3_UGUI

    static void AddEventSystemToSceneIfNotExists()
    {
#if UNITY_2022_3_OR_NEWER
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
#else
        if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
#endif
        {
            GameObject eventSystemGameObject = new GameObject("EventSystem");
            eventSystemGameObject.AddComponent<EventSystem>();
            eventSystemGameObject.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystemGameObject, "Created EventSystem");
        }
    }

    static ES3SlotManager AddSlotsToScene()
    {
        if (!SceneManager.GetActiveScene().isLoaded)
            EditorUtility.DisplayDialog("Could not add manager to scene", "Could not add Save Slots to scene because there is not currently a scene open.", "Ok");

        var pathToEasySaveFolder = ES3Settings.PathToEasySaveFolder();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathToEasySaveFolder + "Scripts/Save Slots/Easy Save Slots Canvas.prefab");
        var instance = (GameObject)Instantiate(prefab);
        Undo.RegisterCreatedObjectUndo(instance, "Added Save Slots to Scene");

        return instance.GetComponentInChildren<ES3SlotManager>();
    }
#endif
}
#endif
