using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FileLoadController : MonoBehaviour, IQESSettingsUser {

	public InputField directoryPathField;
	public Text errorText;
	public Canvas fileLoadCanvas;
	public Button closeButton;

	// Use this for initialization
	void Start () {
		directoryPathField.onEndEdit.AddListener (InputFieldUpdated);
		closeButton.onClick.AddListener (CloseCanvas);
	}

	public void InputFieldUpdated(string str) {
		try {
			qesSettings.LoadDirectory (str);
		} catch (System.Exception e) {
			errorText.text = e.Message;
		}
	}

	public void InteractiveChanged() {
		errorText.text = "";
		if (qesSettings.IsInteractive) {
			fileLoadCanvas.enabled = false;
		} else {
			fileLoadCanvas.enabled = true;
		}
	}

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
