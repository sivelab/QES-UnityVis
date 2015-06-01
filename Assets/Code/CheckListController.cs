using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public delegate void CheckListEvent();

/// <summary>
/// Class to aid implementation of checklists where exactly one item can be chosen
/// 
/// </summary>
public class CheckListController : MonoBehaviour {
	//TODO: Make it so that the CheckListContent intelligently grows and shrinks
	// as the number of check objects changes.

	/// <summary>
	/// GameObject that represents a single entry in the checklist.  This should be inside of the clip view.
	/// </summary>
	public GameObject Check;

	/// <summary>
	/// The view inside of the clip view that will contain all of the Checks.  'Check' should be a child
	/// of this object
	/// </summary>
	public GameObject CheckListContent;

	/// <summary>
	/// Event called when the selected item is changed
	/// </summary>
	public event CheckListEvent SelectionChanged;

	// Index of the currently selected item
	public int SelectedIndex { get; private set; }
	
	void OnEnable () {
		// Take the Check object and create a template object, outside of the scene graph, that will
		// be instantiated as needed.  Set the Check object to be hidden.
		checkTemplate = Instantiate (Check);
		Check.SetActive (false);
		SelectedIndex = -1;
	}

	/// <summary>
	/// Called whenever a checkbox has its value changed
	/// </summary>
	/// <param name="val">Required parameter of the checkbox event</param>
	void CheckAction(bool val) {
		SelectedIndex = -1;
		for (int i=0; i<children.Length; i++) {
			Toggle toggle = children[i].GetComponentInChildren<Toggle>();
			if (toggle.isOn) {
				SelectedIndex = i;
			}
		}
		if (SelectionChanged != null) {
			SelectionChanged();
		}
	}

	/// <summary>
	/// Sets the checkbox list to have exactly the specified entries, with the entry at the specified index selected
	/// </summary>
	/// <param name="entries">Entries to be displayed.</param>
	/// <param name="selectedIndex">index to be selected.</param>
	public void SetEntries(string[] entries, int selectedIndex) {
		if (children != null) {
			foreach (GameObject child in children) {
				Destroy(child);
			}
		}
		children = new GameObject[entries.Length];
		RectTransform parentTransform = CheckListContent.GetComponent<RectTransform> ();
		for (int i=0; i<entries.Length; i++) {
			children[i] = Instantiate (checkTemplate);
			RectTransform childTransform = children[i].GetComponent<RectTransform>();
			childTransform.SetParent(parentTransform, false);
			Vector2 offset = new Vector2(0, -i * 30);
			childTransform.offsetMin += offset;
			childTransform.offsetMax += offset;

			Toggle toggle = children[i].GetComponentInChildren<Toggle>();

			if (i == selectedIndex) {
				toggle.isOn = true;
			} else {
				toggle.isOn = false;
			}

			toggle.onValueChanged.AddListener(CheckAction);

			Text text = children[i].GetComponentInChildren<Text>();
			text.text = entries[i];
		}
		Toggle selectedToggle = children[selectedIndex].GetComponentInChildren<Toggle>();

		SelectedIndex = selectedIndex;
	}
	

	GameObject checkTemplate;
	GameObject[] children;
}
