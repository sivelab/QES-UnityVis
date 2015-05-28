using UnityEngine;
using System.Collections;

public class QESSettingsSetter : MonoBehaviour {

	// Use this for initialization
	void Start () {
		QESSettings set = new QESSettings ();
		try {
			//set.LoadDirectory ("/scratch/schr0640/tmp/export-uehara/");
			set.LoadDirectory ("C:/scratch/tmp/export-uehara/");
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
