using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
