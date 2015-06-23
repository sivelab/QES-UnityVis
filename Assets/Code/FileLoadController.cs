using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Class to control the "Load Data" canvas
/// </summary>
/// This canvas is only shown if the visualization is inactive.
public class FileLoadController : MonoBehaviour, IQESSettingsUser {
	/// <summary>
	/// Textfield where the user enters the path to be loaded
	/// </summary>
	public InputField directoryPathField;

	/// <summary>
	/// Text area used to display any errors in trying to load data
	/// </summary>
	public Text errorText;

	/// <summary>
	/// Canvas containing the whole "Load Data" interface
	/// </summary>
	public Canvas fileLoadCanvas;

	/// <summary>
	/// Button used to close the "Load Data" interface
	/// </summary>
	public Button closeButton;
	
	void Start () {
		directoryPathField.onEndEdit.AddListener (InputFieldUpdated);
		closeButton.onClick.AddListener (CloseCanvas);
	}

	/// <summary>
	/// When the input field is updated, try loading data from that directory.
	/// On error, set the error text to whatever exception was raised.
	/// </summary>
	/// <param name="str">directory to load</param>
	public void InputFieldUpdated(string str) {
		try {
			qesSettings.LoadDirectory (str);
		} catch (System.Exception e) {
			errorText.text = e.Message;
		}
	}

	/// <summary>
	/// Called when the interactive state changes.  If the visualization is
	/// interactive, disable our canvas.  If the visualization is not interactive,
	/// set our canvas to active.
	/// </summary>
	public void InteractiveChanged() {
		errorText.text = "";

		if (qesSettings.IsInteractive) {
			fileLoadCanvas.enabled = false;
		} else {
			fileLoadCanvas.enabled = true;
		}
	}

	/// <summary>
	/// Closes the canvas.
	/// </summary>
	public void CloseCanvas() {
		qesSettings.SetInteractive (true);
	}

	public void SetSettings(QESSettings settings) {
		if (qesSettings != null) {
			qesSettings.InteractiveChanged -= InteractiveChanged;
		}
		qesSettings = settings;
		qesSettings.InteractiveChanged += InteractiveChanged;

		InteractiveChanged ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private QESSettings qesSettings;
}
