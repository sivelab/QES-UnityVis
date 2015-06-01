using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Behaviour class to allow a Button to cause the change variable Canvas to become visible.
/// </summary>
public class ChangeVariablesButton : MonoBehaviour {

	public Button button;
	public VariableInfoController Controller;

	// Use this for initialization
	void Start () {
		button.onClick.AddListener (ChangeVariables);
	}

	void ChangeVariables() {
		Controller.Show ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
