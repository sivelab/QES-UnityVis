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
			//set.LoadDirectory ("/tmp/export-qes/");
			set.LoadDirectory ("C:/scratch/tmp/coupledLSMTTM_SLC/export-qes");
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
