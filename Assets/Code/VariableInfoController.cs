using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VariableInfoController : MonoBehaviour, IQESSettingsUser {

	public CheckListController VisualizationCheckList;
	public CheckListController VariableCheckList;
	public Button CloseButton;
	public GameObject Visualization;
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

	public void UpdateVisualizations() {
		visualizations = Visualization.GetComponentsInChildren<IQESVisualization> ();
		string[] names = new string[visualizations.Length];

		for (int i=0; i<visualizations.Length; i++) {
			names[i] = visualizations[i].VisualizationName();
		}

		VisualizationCheckList.SetEntries (names, 0);
	}

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
