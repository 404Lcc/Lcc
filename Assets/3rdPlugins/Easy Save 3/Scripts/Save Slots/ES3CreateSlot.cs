#if ES3_TMPRO && ES3_UGUI

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// A script attached to the Create Slot button to manage slot creation.
/// </summary>
public class ES3CreateSlot : MonoBehaviour
{
    [Tooltip("The button used to bring up the 'Create Slot' dialog.")]
    public Button createButton;
    [Tooltip("The ES3SlotDialog Component of the Create Slot dialog")]
    public ES3SlotDialog createDialog;
    [Tooltip("The TMP_Text input text field of the create slot dialog.")]
    public TMP_InputField inputField;
    [Tooltip("The ES3SlotManager this Create Slot Dialog belongs to.")]
    public ES3SlotManager mgr;

    protected virtual void OnEnable()
    {
        // Whether we should show or hide this Create Slot button based on the settings in the slot manager.
        gameObject.SetActive(mgr.showCreateSlotButton);
        // Make it so the Create Slot button brings up the Create Slot dialog.
        createButton.onClick.AddListener(ShowCreateSlotDialog);
        // Add listener to the confirmation button.
        createDialog.confirmButton.onClick.AddListener(TryCreateNewSlot);
    }

    protected virtual void OnDisable()
    {
        // Make sure the text field is empty for next time.
        inputField.text = string.Empty;
        // Remove all listeners.
        createButton.onClick.RemoveAllListeners();
        createDialog.confirmButton.onClick.RemoveAllListeners();
    }

    // Called when the Create new slot button is pressed.
    protected void ShowCreateSlotDialog()
    {
        // Make the dialog visible and active.
        createDialog.gameObject.SetActive(true);
        // Set the input field as active so the player doesn't need to click it to input their name.
        inputField.Select();
        inputField.ActivateInputField();
    }

    // Called when the Create button is pressed in the Create New Slot dialog.
    public virtual void TryCreateNewSlot()
    {
        // If the user hasn't specified a name, throw an error.
        // Note that no other validation of the name is a required as this is handled using a REGEX in the TMP_InputField Component.
        if (string.IsNullOrEmpty(inputField.text))
        {
            mgr.ShowErrorDialog("You must specify a name for your save slot");
            return;
        }

        // Get the file path for the slot we're trying to create.
        var slotPath = mgr.GetSlotPath(inputField.text);

        // If a slot with this name already exists, require the user to enter a different name,
        // or if the slot is marked to be deleted, delete it's file so that this one can be created.
        if (ES3.FileExists(slotPath))
        {
            // Check whether a slot exists with this name which has been marked for deletion.
            var slotMarkedForDeletion = mgr.slots.Select(go => go.GetComponent<ES3Slot>()).FirstOrDefault(slot => mgr.GetSlotPath(slot.nameLabel.text) == slotPath && slot.markedForDeletion);

            // If there's not a slot with this path marked for deletion, force user to choose another name.
            if (slotMarkedForDeletion == null)
            {
                mgr.ShowErrorDialog("A slot already exists with this name. Please choose a different name.");
                return;
            }
            // Otherwise, delete the slot so that it can be created from scratch.
            else
                slotMarkedForDeletion.DeleteSlot();
        }

        // Create the slot.
        var slot = mgr.CreateNewSlot(inputField.text);
        // Clear the input field so the value isn't there when we reopen it.
        inputField.text = "";
        // Hide the dialog.
        createDialog.gameObject.SetActive(false);

        // If we've specified an event to be called after the user creaktes a slot, invoke it.
        mgr.onAfterCreateSlot?.Invoke();
    }
}

#endif