using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public delegate void SettingsUpdate();

/// <summary>
/// Class representing the global state of the visualization.
/// 
/// A scene can contain multiple QESSettings and objects using separate QESSettings.
/// </summary>
public class QESSettings {
	public IQESDataSource DataSource { get; private set; }
	public QESReader Reader { get; private set; }
	public int CurrentTimestep { get; private set; }
	public bool IsInteractive { get; private set; }

	/// <summary>
	/// Occurs when a new dataset is loaded
	/// </summary>
	public event SettingsUpdate DatasetChanged;

	/// <summary>
	/// Occurs when the current timestep changes.
	/// </summary>
	public event SettingsUpdate TimestepChanged;

	/// <summary>
	/// Occurs when the interactive state of the visualization is changed.
	/// </summary>
	public event SettingsUpdate InteractiveChanged;

	/// <summary>
	/// Loads data from a directory.  On success, calls the DatasetChanged event.  On
	/// failure, raises an exception, and sets itself inactive (calling the InteractiveChanged
	/// event)
	/// </summary>
	/// <param name="path">Directory path from which to load data</param>
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

	/// <summary>
	/// Seeks to a timestep.  If the new timestep is different than the old timestep, calls the
	/// TimestepChanged event
	/// </summary>
	/// <param name="timestep">Timestep to change to</param>
	public void SeekTo(int timestep) {
		if (CurrentTimestep == timestep) {
			return;
		}
		CurrentTimestep = timestep;

		if (TimestepChanged != null) {
			TimestepChanged ();
		}
	}

	/// <summary>
	/// Sets whether the visualization is interactive or not.  If this changes the interactive
	/// state, calls InteractiveChanged.
	/// </summary>
	/// <param name="i">Whether visualization should be interactive</param>
	public void SetInteractive(bool i) {
		if (i != IsInteractive) {
			IsInteractive = i;
			if (InteractiveChanged != null) {
				InteractiveChanged();
			}
		}
	}
}
