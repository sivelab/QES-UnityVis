using UnityEngine;
using System.Collections;

/// <summary>
/// Class that iterates through the scenegraph of all child nodes and finds
/// MonoBehaviour instances that implement IQESSettingsUser and gives them
/// access to the same instance of QESSettings.
/// </summary>
public class QESSettingsSetter : MonoBehaviour {

	// Use this for initialization
	void Start () {
		QESSettings set = new QESSettings ();
		try {
			switch (Application.platform) {
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
				set.LoadDirectory ("C:/scratch/export-qes");
				break;
			default:
				set.LoadDirectory ("/tmp/export-qes/");
				break;
			}
		} catch {
		}
		IQESSettingsUser[] users = GetComponentsInChildren<IQESSettingsUser> ();
		foreach (IQESSettingsUser user in users) {
			user.SetSettings (set);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
