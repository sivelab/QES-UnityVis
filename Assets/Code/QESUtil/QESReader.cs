using System.Xml;
using UnityEngine;

public class QESReader
{
	/// <summary>
	/// QESReader reads in data files exported from quic_envsim in the xml+folder format.
	/// </summary>
	/// <param name="ds">Datasource to get the required data from.  For directory access, use QESDirectorySource.</param>
	public QESReader (IQESDataSource ds)
	{
		dataSource = ds;
		LoadBuildings ();
	}

	public QESBuilding[] Buildings {
		get {
			return buildings;
		}
	}

	private void LoadBuildings ()
	{
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (dataSource.TextFileContents ("structure.xml"));

		XmlElement topElement = doc.DocumentElement;

		if (topElement.Name != "Settings") {
			throw new XmlException ("Unexpected root node: " + topElement.Name);
		}

		XmlElement outerElement = topElement ["Scene"];

		buildings = new QESBuilding[outerElement.ChildNodes.Count];
		int buildingIndex = 0;
		foreach (XmlNode buildingNode in outerElement) {
			if (buildingNode.Name != "Building") {
				throw new XmlException ("Expected Building node, found " + buildingNode.Name + " instead");
			}
			QESFace[] faces;

			XmlNodeList faceNodes = buildingNode.ChildNodes;
			faces = new QESFace[faceNodes.Count];

			for (int i=0; i<faceNodes.Count; i++) {
				XmlNode faceNode = faceNodes.Item (i);
				if (faceNode.Name != "Face") {
					throw new XmlException ("Expected Face node, found " + faceNode.Name + " instead");
				}
				Vector3 anchor = ReadVector3 (faceNode, "anchor");
				Vector3 v1 = ReadVector3 (faceNode, "v1");
				Vector3 v2 = ReadVector3 (faceNode, "v2");
				int patchIndex = int.Parse (faceNode.Attributes ["patchIndex"].Value);
				int width = int.Parse (faceNode.Attributes ["width"].Value);
				int height = int.Parse (faceNode.Attributes ["height"].Value);
				faces [i] = new QESFace (anchor, v1, v2, width, height, patchIndex);
			}
			buildings [buildingIndex] = new QESBuilding (faces);
			buildingIndex++;
		}

		XmlElement timestampsNode = topElement ["Timestamps"];
		XmlNodeList timestampNodes = timestampsNode.ChildNodes;
		timestamps = new QESTimestamp[timestampNodes.Count];
		for (int i=0; i<timestampNodes.Count; i++) {
			XmlNode tsNode = timestampNodes.Item (i);
			timestamps [i] = new QESTimestamp (int.Parse (tsNode.Attributes ["year"].Value),
			                                 int.Parse (tsNode.Attributes ["month"].Value),
			                                 int.Parse (tsNode.Attributes ["day"].Value),
			                                 int.Parse (tsNode.Attributes ["hour"].Value),
			                                 int.Parse (tsNode.Attributes ["minute"].Value), 
			                                 int.Parse (tsNode.Attributes ["second"].Value));
		}

		XmlElement variablesNode = topElement ["Variables"];
		XmlNodeList variableNodes = variablesNode.ChildNodes;
		variables = new QESVariable[variableNodes.Count];
		for (int i=0; i<variableNodes.Count; i++) {
			XmlNode varNode = variableNodes.Item (i);
			variables [i] = new QESVariable (varNode.Attributes ["name"].Value,
			                               varNode.Attributes ["longname"].Value,
			                               varNode.Attributes ["unit"].Value,
			                               float.Parse (varNode.Attributes ["min"].Value),
			                               float.Parse (varNode.Attributes ["max"].Value));
		}

		XmlElement dimsElement = topElement ["Dimensions"];
		WorldDims = ReadVector3 (dimsElement, "worldDims");
		PatchDims = ReadVector3 (dimsElement, "patchDims");
	}

	public QESTimestamp[] getTimestamps ()
	{
		return timestamps;
	}

	public QESVariable[] getVariables ()
	{
		return variables;
	}

	public float[] GetPatchData (string var, int timestamp)
	{
		byte[] rawBytes = dataSource.BinaryFileContents (var + timestamp.ToString ());
		float[] vals = new float[rawBytes.Length / 4];
		for (int i=0; i<vals.Length; i++) {
			vals [i] = System.BitConverter.ToSingle (rawBytes, i * 4);
		}
		return vals;
	}

	private Vector3 ReadVector3 (XmlNode node, string name)
	{
		foreach (XmlNode child in node) {
			if (child.Name == name) {
				Vector3 ans;
				ans.x = float.Parse (child.Attributes ["x"].Value);
				ans.y = float.Parse (child.Attributes ["y"].Value);
				ans.z = float.Parse (child.Attributes ["z"].Value);

				return ans;
			}
		}
		throw new XmlException ("Vector with name " + name + " not found");
	}

	private IQESDataSource dataSource;
	private QESBuilding[] buildings;
	private QESTimestamp[] timestamps;
	private QESVariable[] variables;

	public Vector3 WorldDims { get; private set; }

	public Vector3 PatchDims { get; private set; }
}
