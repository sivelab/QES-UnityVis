using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public delegate void CheckListEvent();

public class CheckListController : MonoBehaviour {

	public GameObject Check;
	public GameObject CheckListContent;

	public event CheckListEvent SelectionChanged;

	public int SelectedIndex { get; private set; }

	// Use this for initialization
	void OnEnable () {
		checkTemplate = Instantiate (Check);
		Check.SetActive (false);
		SelectedIndex = -1;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

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
