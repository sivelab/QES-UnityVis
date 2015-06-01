using System.Collections.Generic;
using UnityEngine;
using System.Xml;


/// <summary>
/// Class to access color ramps defined in the asset "ColorMaps".  This file
/// is in paraview XML format.
/// 
/// You should never need to instantiate this class directly, instead getting access
/// to instances from GetColorRamp()
/// </summary>
public class ColorRamp
{
	ColorRamp() {
		colors = new List<Color> ();
		positions = new List<float> ();
	}

	/// <summary>
	/// Gets color ramp names.
	/// </summary>
	/// <returns>Names of available color ramps</returns>
	public static string[] GetColorRampNames ()
	{
		LoadIfNeeded ();
		string[] ans = new string[colorRamps.Keys.Count];
		colorRamps.Keys.CopyTo (ans, 0);
		return ans;
	}

	/// <summary>
	/// Gets the color ramp with the specified name
	/// </summary>
	/// <returns>A (shared) instance of ColorRamp</returns>
	/// <param name="name">Name of color ramp to return</param>
	public static ColorRamp GetColorRamp (string name)
	{
		LoadIfNeeded ();
		return colorRamps [name];
	}

	/// <summary>
	/// Called to load data from the ColorMaps Asset.
	/// </summary>
	private static void LoadIfNeeded ()
	{
		if (loaded) {
			return;
		}
		colorRamps = new Dictionary<string, ColorRamp> ();
		TextAsset colormapText = Resources.Load ("ColorMaps") as TextAsset;
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (colormapText.text);

		XmlNode root = doc.DocumentElement;
		foreach (XmlNode rampNode in root) {
			if (rampNode.Name != "ColorMap") {
				continue;
			}
			ColorRamp ramp = new ColorRamp ();

			foreach (XmlNode pointNode in rampNode) {
				if (pointNode.Name != "Point") {
					continue;
				}
				Color c = new Color ();
				c.r = float.Parse (pointNode.Attributes ["r"].Value);
				c.g = float.Parse (pointNode.Attributes ["g"].Value);
				c.b = float.Parse (pointNode.Attributes ["b"].Value);
				c.a = 1.0f;
				ramp.colors.Add (c);
				ramp.positions.Add (float.Parse (pointNode.Attributes ["x"].Value));
			}
			float min = ramp.positions [0];
			float max = min;
			for (int i=1; i<ramp.positions.Count; i++) {
				if (ramp.positions [i] < min) {
					min = ramp.positions [i];
				}
				if (ramp.positions [i] > max) {
					max = ramp.positions [i];
				}
			}
			for (int i=0; i<ramp.positions.Count; i++) {
				ramp.positions [i] = (ramp.positions [i] - min) / (max - min);
			}
			ramp.name = rampNode.Attributes["name"].Value;
			colorRamps.Add(ramp.name, ramp);
		}
		loaded = true;
	}

	/// <summary>
	/// Returns the Color at the specified position in the color ramp.
	/// Ramps are scaled to lie in the range 0..1, and values outside this range are 
	/// either the first or last control point's color, respectively.
	/// </summary>
	/// <param name="pos">Position.</param>
	public Color Value (float pos)
	{
		if (pos < 0) {
			return colors [0];
		}
		if (pos >= 1) {
			return colors [colors.Count - 1];
		}

		for (int i=0; i<positions.Count; i++) {
			if (pos < positions [i]) {
				float curPos = positions [i];
				float prevPos = positions [i - 1];
				float blend = (pos - prevPos) / (curPos - prevPos);
				return colors [i] * blend + colors [i - 1] * (1 - blend);
			}
		}
		return colors [colors.Count - 1];
	}

	/// <summary>
	/// Returns a resampled color ramp with the specified number of samples
	/// </summary>
	/// <param name="numSamples">Number of samples.</param>
	public Color[] Ramp(int numSamples) 
	{
		Color[] result = new Color[numSamples];
		for (int i=0; i<numSamples; i++) {
			result[i] = Value(i / (numSamples - 1.0f));
		}
		return result;
	}

	private string name;
	private static Dictionary<string, ColorRamp> colorRamps;
	private static bool loaded = false;
	private List<Color> colors;
	private List<float> positions;
}
