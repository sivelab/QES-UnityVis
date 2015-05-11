using UnityEngine;
using System.Collections;

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
