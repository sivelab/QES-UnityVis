using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public delegate void SettingsUpdate();

public class QESSettings {
	public IQESDataSource DataSource { get; private set; }
	public QESReader Reader { get; private set; }
	public int CurrentTimestep { get; private set; }

	public event SettingsUpdate DatasetChanged;
	public event SettingsUpdate TimestepChanged;
	
	public void LoadDirectory(string path) {
		DataSource = new QESDirectorySource(path);
		Reader = new QESReader(DataSource);

		CurrentTimestep = 0;

		if (DatasetChanged != null) {
			DatasetChanged ();
		}

	}

	public void SeekTo(int timestep) {
		if (CurrentTimestep == timestep) {
			return;
		}
		CurrentTimestep = timestep;

		if (TimestepChanged != null) {
			TimestepChanged ();
		}
	}
}
