using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class that controls the interface for choosing which visualizations show which variables
/// </summary>
public class VariableInfoController : MonoBehaviour, IQESSettingsUser {
	/// <summary>
	/// Checklist for selecting the visualization to choose the variable for
	/// </summary>
	public CheckListController VisualizationCheckList;

	/// <summary>
	/// Checklist for selecting the variable to be set for the selected visualization
	/// </summary>
	public CheckListController VariableCheckList;

	/// <summary>
	/// Button to close this interface
	/// </summary>
	public Button CloseButton;

	/// <summary>
	/// GameObject that contains all of the GameObjects that we want to adjust the visualizations of
	/// </summary>
	public GameObject Visualization;

	/// <summary>
	/// Canvas containing this interface
	/// </summary>
	public Canvas VariableInfoCanvas;

	// Use this for initialization
	void Start () {
		VisualizationCheckList.SelectionChanged += VisSelectionChanged;
		VariableCheckList.SelectionChanged += VariableSelectionChanged;
		CloseButton.onClick.AddListener (Close);
	}

	public void Show() {
		VariableInfoCanvas.enabled = true;
	}

	void Close() {
		VariableInfoCanvas.enabled = false;
	}

	/// <summary>
	/// Update our list of visualizations based on the GameObjects with MonoBehaviours
	/// implementing IQESVisualization
	/// </summary>
	public void UpdateVisualizations() {
		visualizations = Visualization.GetComponentsInChildren<IQESVisualization> ();
		string[] names = new string[visualizations.Length];

		for (int i=0; i<visualizations.Length; i++) {
			names[i] = visualizations[i].VisualizationName();
		}

		VisualizationCheckList.SetEntries (names, 0);
	}

	/// <summary>
	/// Update the list of avaialbe variables for the type supported by the current visualization
	/// </summary>
	public void UpdateVariables() {
		if (qesSettings == null) {
			return;
		}
		IQESVisualization vis = visualizations [VisualizationCheckList.SelectedIndex];
		QESVariable.Type type = vis.VariableType ();
		string currentVar = vis.CurrentVariable ();

		List<string> varList = new List<string> ();
		QESVariable[] vars = qesSettings.Reader.getVariables ();
		int selectedIndex = -1;
		for (int i=0; i<vars.Length; i++) {
			if (vars[i].type == type) {
				if (vars[i].Name == currentVar) {
					selectedIndex = varList.Count;
				}
				varList.Add(vars[i].Name);
			}
		}
		variables = varList.ToArray ();
		VariableCheckList.SetEntries (variables, selectedIndex);
	}

	public void VariableSelectionChanged() {
		IQESVisualization vis = visualizations [VisualizationCheckList.SelectedIndex];
		string var = variables [VariableCheckList.SelectedIndex];
		vis.SetCurrentVariable (var);
	}
	
	// Update is called once per frame
	void Update () {
		if (visualizations == null) {
			UpdateVisualizations ();
		}
	}

	public void VisSelectionChanged() {
		UpdateVariables ();
	}

	public void SetSettings(QESSettings settings) {
		if (qesSettings != null) {
			qesSettings.DatasetChanged -= VisSelectionChanged;
		}
		qesSettings = settings;
		qesSettings.DatasetChanged += VisSelectionChanged;
		UpdateVisualizations ();
		VisSelectionChanged ();
	}

	IQESVisualization[] visualizations;
	string[] variables;
	QESSettings qesSettings;
}
