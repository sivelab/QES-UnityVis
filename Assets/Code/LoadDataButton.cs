using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Class for the global-level "Load Data" button that toggles the interactive
/// status of the visualization as a whole.
/// </summary>
public class LoadDataButton : MonoBehaviour, IQESSettingsUser {

	public Button button;

	// Use this for initialization
	void Start () {
		button.onClick.AddListener (SetInactive);
	}

	public void SetSettings(QESSettings settings) {
		qesSettings = settings;

	}

	public void SetInactive() {
		qesSettings.SetInteractive (false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private QESSettings qesSettings;
}
