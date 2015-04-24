using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public delegate void SettingsUpdate();

public class QESSettings {
	public IQESDataSource DataSource { get; private set; }
	public QESReader Reader { get; private set; }
	public int CurrentTimestep { get; private set; }
	public bool IsInteractive { get; private set; }

	public event SettingsUpdate DatasetChanged;
	public event SettingsUpdate TimestepChanged;
	public event SettingsUpdate InteractiveChanged;

	// 
	public event SettingsUpdate RenderCapChanged;
	
	public void LoadDirectory(string path) {
		try {
			DataSource = new QESDirectorySource(path);
			Reader = new QESReader(DataSource);

			CurrentTimestep = 0;

			if (DatasetChanged != null) {
				DatasetChanged ();
			}
			SetInteractive(true);
		} catch (Exception e) {
			DataSource = null;
			Reader = null;

			SetInteractive(false);

			throw;
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

	public void SetInteractive(bool i) {
		if (i != IsInteractive) {
			IsInteractive = i;
			if (InteractiveChanged != null) {
				InteractiveChanged();
			}
		}
	}
}
