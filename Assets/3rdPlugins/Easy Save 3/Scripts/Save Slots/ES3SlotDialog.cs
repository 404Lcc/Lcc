#if ES3_TMPRO && ES3_UGUI

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A script attached to the error dialog and confirm dialog to provide events which other scripts can receive.
/// </summary>
public class ES3SlotDialog : MonoBehaviour
{
    [Tooltip("The UnityEngine.UI.Button Component for the Confirm button.")]
    public Button confirmButton;
    [Tooltip("The UnityEngine.UI.Button Component for the Cancel button.")]
    public Button cancelButton;

    protected virtual void OnEnable()
    {
        // Make it so that the cancel button deactivates the dialog.
        cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    protected virtual void OnDisable()
    {
        // Remove any listeners when disabling this dialog.
        cancelButton.onClick.RemoveAllListeners();
    }
}

#endif
