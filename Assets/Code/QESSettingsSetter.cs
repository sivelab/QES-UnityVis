using UnityEngine;
using System.Collections;

public class QESSettingsSetter : MonoBehaviour {

	// Use this for initialization
	void Start () {
		QESSettings set = new QESSettings ();
		try {
			set.LoadDirectory ("/tmp/export-gothenburg/");
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
