using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Toggle

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Toggle tiltControlsToggle;

    // This is the key we will use to save the setting
    public const string TILT_CONTROLS_KEY = "UseTiltControls";

    private void Start()
    {
        // 1. Ensure the toggle is assigned in the Inspector
        if (tiltControlsToggle == null)
        {
            Debug.LogError("Tilt Controls Toggle is not assigned in the Inspector!");
            return;
        }

        // 2. Load the saved preference and set the toggle's state
        // We default to 0 (false, or "Swipe") if no setting is found
        bool useTilt = PlayerPrefs.GetInt(TILT_CONTROLS_KEY, 0) == 1;
        tiltControlsToggle.isOn = useTilt;

        // 3. Add a listener to save the setting whenever the toggle is changed
        tiltControlsToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    /// <summary>
    /// Called when the user clicks the toggle in the menu.
    /// </summary>
    /// <param name="isTiltOn">The new state of the toggle (true if checked).</param>
    public void OnToggleChanged(bool isTiltOn)
    {
        // 1. Convert the boolean to an integer (1 for true, 0 for false)
        int prefValue = isTiltOn ? 1 : 0;

        // 2. Save the value to PlayerPrefs
        PlayerPrefs.SetInt(TILT_CONTROLS_KEY, prefValue);
        PlayerPrefs.Save(); // Make sure it saves immediately

        Debug.Log("Control scheme saved: " + (isTiltOn ? "Tilt" : "Swipe"));
    }

    private void OnDestroy()
    {
        // Clean up the listener when the object is destroyed
        if (tiltControlsToggle != null)
        {
            tiltControlsToggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }
}