using UnityEngine;
using System.Collections;

/// <summary>
/// Interface for MonoBehaviours that wish to gain access to the visualization-wide QESSettings object.
/// 
/// QESSettingsSetter will call SetSettings on all components that implement this interface.
/// </summary>
public interface IQESSettingsUser {
	void SetSettings(QESSettings settings);
}
