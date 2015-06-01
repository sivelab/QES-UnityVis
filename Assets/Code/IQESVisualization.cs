using UnityEngine;
using System.Collections;
/// <summary>
/// Interface to be implemented by all visualization MonoBehaviours that wish
/// to have their variables changed at runtime.  This is used by
/// VariableInfoController to get a list of these visualizations.
/// </summary>
public interface IQESVisualization {

	/// <summary>
	/// Returns the variable type supported by this visualization
	/// </summary>
	/// <returns>Variable type supported</returns>
	QESVariable.Type VariableType();

	/// <summary>
	/// Returns the variable currently being used
	/// </summary>
	/// <returns>Current variable</returns>
	string CurrentVariable();

	/// <summary>
	/// Sets the current variable.
	/// </summary>
	/// <param name="var">Variable to be visualized</param>
	void SetCurrentVariable(string var);

	/// <summary>
	/// Returns a short, user-understandable name for the visualization
	/// </summary>
	/// <returns>The name.</returns>
	string VisualizationName();
}
