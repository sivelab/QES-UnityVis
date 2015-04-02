using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlaybackQES : MonoBehaviour, IQESSettingsUser {

	public Slider Seekbar;
	public Button PlayPause;

	// Use this for initialization
	void Start () {
		Seekbar.onValueChanged.AddListener (SetTimestep);
		PlayPause.onClick.AddListener (TogglePlayPause);
	}
	
	// Update is called once per frame
	void Update () {
		if (playing) {
			timeAccum += Time.deltaTime;
			int currentFrame = qesSettings.CurrentTimestep;
			if (timeAccum >= timePerFrame) {
				int steps = (int)(timeAccum / timePerFrame);
				timeAccum -= steps * timePerFrame;
				currentFrame += steps;
				int maxTimestep = qesSettings.Reader.getTimestamps().Length - 1;
				if (currentFrame >= maxTimestep) {
					playing = false;
					timeAccum = 0;
					currentFrame = maxTimestep;
				}
				qesSettings.SeekTo(currentFrame);
			}
		}
	}

	public void SetTimestep (float val) {
		int seekbarTime = (int)Seekbar.value;
		if (seekbarTime != qesSettings.CurrentTimestep) {
			playing = false;
			qesSettings.SeekTo(seekbarTime);
		}
	}

	public void TogglePlayPause() {
		playing = !playing;
		if (!playing) {
			timeAccum = 0;
		}
	}

	public void StopPlaying() {
		playing = false;
	}

	public void UpdateUI() {
		if (qesSettings == null || qesSettings.Reader == null) {
			return;
		}
		Seekbar.minValue = 0;
		Seekbar.maxValue = qesSettings.Reader.getTimestamps ().Length;
		int seekbarTime = (int)Seekbar.value;
		if (seekbarTime != qesSettings.CurrentTimestep) {
			Seekbar.value = qesSettings.CurrentTimestep;
		}
	}

	public void SetSettings(QESSettings settings) {
		if (qesSettings != null) {
			qesSettings.DatasetChanged -= UpdateUI;
			qesSettings.TimestepChanged -= UpdateUI;
			qesSettings.InteractiveChanged -= StopPlaying;
		}
		qesSettings = settings;
		qesSettings.DatasetChanged += UpdateUI;
		qesSettings.TimestepChanged += UpdateUI;
		qesSettings.InteractiveChanged += StopPlaying;
		UpdateUI ();
	}

	private QESSettings qesSettings;
	private bool playing = false;
	private float timeAccum = 0;
	private const float timePerFrame = 0.5f;
}
