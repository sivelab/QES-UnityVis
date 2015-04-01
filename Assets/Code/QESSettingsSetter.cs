using UnityEngine;
using System.Collections;

public class QESSettingsSetter : MonoBehaviour {

	// Use this for initialization
	void Start () {
		QESSettings set = new QESSettings ();
		set.LoadDirectory ("/tmp/export-gothenburg/");
		IQESSettingsUser[] users = GetComponentsInChildren<IQESSettingsUser> ();
		foreach (IQESSettingsUser user in users) {
			user.SetSettings (set);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
