using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Controller for the playback interface, responsible for updating UI and
/// advancing time as necessary/
/// </summary>
public class PlaybackQES : MonoBehaviour, IQESSettingsUser {
	/// <summary>
	/// Slider used to seek through time and show the current playback position
	/// </summary>
	public Slider Seekbar;

	/// <summary>
	/// Button to toggle playing and pausing
	/// </summary>
	public Button PlayPause;
	
	void Start () {
		Seekbar.onValueChanged.AddListener (SetTimestep);
		PlayPause.onClick.AddListener (TogglePlayPause);
	}
	
	/// <summary>
	/// Every time, if we are playing, accumulate time since we previously
	/// advanced frame.  If this is greater than the set threshold for this
	/// to happen, update QESSettings to be that many frames advanced.
	/// </summary>
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

	/// <summary>
	/// Callback from Slider to set the current time.
	/// </summary>
	/// <param name="val">Currently selected time</param>
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

	/// <summary>
	/// Update the UI to reflect the current internal state
	/// </summary>
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
