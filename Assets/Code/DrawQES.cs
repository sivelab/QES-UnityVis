using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]

public class DrawQES : MonoBehaviour
{
	public Material baseMaterial;
	public Material transparentMaterial;
	public bool showChange = false;
	public float changeRange = 20;
	public GUIText debugText;
	public string variableName = "patch_nir";

	public Text maxTextBox;
	public Text minTextBox;

	public RawImage legend;

	// If we want an automated way to step from the start time to the end time
	public bool automateTimestepping = false;
	public float timePerTimestep = 3.0f; // 3 seconds per timestepp

	// Use this for initialization
	void Start ()
	{
		QESDirectorySource directorySource = new QESDirectorySource ("/tmp/export-gothenburg/");
		
		qesReader = new QESReader (directorySource);

		if (automateTimestepping) {
			// if we're automating the time stepping, start at the beginning
			timestep = 0;
		}
		else {
			timestep = qesReader.getTimestamps ().Length / 2;
		}
		timestepUp = true;

		timeWait = timePerTimestep;
		
		SetMesh ();
	}
	
	void SetMesh ()
	{
		List<Vector3> vertices = new List<Vector3> ();
		List<Vector3> normals = new List<Vector3> ();
		List<Vector2> uvs = new List<Vector2> ();
		List<List<int>> indices = new List<List<int>> ();
		faces = new List<QESFace> ();
		
		float scaleFactor = 1.0f;
		
		foreach (QESBuilding building in qesReader.Buildings) {
			foreach (QESFace face in building.Faces) {
				List<int> myIndices = new List<int> ();
				vertices.Add (face.Anchor * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2 (0, 0));
				
				vertices.Add ((face.Anchor + face.V1) * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2 (1, 0));
				
				vertices.Add ((face.Anchor + face.V1 + face.V2) * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2 (1, 1));
				
				vertices.Add ((face.Anchor + face.V2) * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2 (0, 1));
				
				myIndices.Add (vertices.Count - 4);
				myIndices.Add (vertices.Count - 3);
				myIndices.Add (vertices.Count - 2);
				
				myIndices.Add (vertices.Count - 4);
				myIndices.Add (vertices.Count - 2);
				myIndices.Add (vertices.Count - 3);
				
				
				myIndices.Add (vertices.Count - 4);
				myIndices.Add (vertices.Count - 2);
				myIndices.Add (vertices.Count - 1);
				
				myIndices.Add (vertices.Count - 4);
				myIndices.Add (vertices.Count - 1);
				myIndices.Add (vertices.Count - 2);
				
				indices.Add (myIndices);
				faces.Add (face);
			}
		}
		
		// Render the sensors
		foreach (QESSensor sensor in qesReader.Sensors) {
			foreach (QESFace face in sensor.Faces) {
				List<int> myIndices = new List<int> ();
				vertices.Add (face.Anchor * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2 (0, 0));
				
				vertices.Add ((face.Anchor + face.V1) * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2 (1, 0));
				
				vertices.Add ((face.Anchor + face.V1 + face.V2) * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2 (1, 1));
				
				vertices.Add ((face.Anchor + face.V2) * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2 (0, 1));
				
				myIndices.Add (vertices.Count - 4);
				myIndices.Add (vertices.Count - 3);
				myIndices.Add (vertices.Count - 2);
				
				myIndices.Add (vertices.Count - 4);
				myIndices.Add (vertices.Count - 2);
				myIndices.Add (vertices.Count - 3);
				
				
				myIndices.Add (vertices.Count - 4);
				myIndices.Add (vertices.Count - 2);
				myIndices.Add (vertices.Count - 1);
				
				myIndices.Add (vertices.Count - 4);
				myIndices.Add (vertices.Count - 1);
				myIndices.Add (vertices.Count - 2);
				
				indices.Add (myIndices);
				faces.Add (face);
			}
		}
		
		Vector3[] verticesArray = new Vector3[vertices.Count];
		Vector3[] normalsArray = new Vector3[normals.Count];
		Vector2[] uvArray = new Vector2[uvs.Count];
		
		vertices.CopyTo (verticesArray);
		normals.CopyTo (normalsArray);
		uvs.CopyTo (uvArray);
		
		Mesh mesh = new Mesh ();
		
		mesh.subMeshCount = indices.Count;
		
		mesh.vertices = verticesArray;
		mesh.normals = normalsArray;
		mesh.uv = uvArray;
		
		for (int faceIndex=0; faceIndex < indices.Count; faceIndex++) {
			int[] indicesArray = new int[indices [faceIndex].Count];
			indices [faceIndex].CopyTo (indicesArray);
			mesh.SetTriangles (indicesArray, faceIndex);
		}
		
		mesh.name = "DrawQES";
		
		mesh.Optimize ();
		
		GetComponent<MeshFilter> ().mesh = mesh;
		
		setMaterials ();
	}
	
	void setMaterials ()
	{
		QESTimestamp ts = qesReader.getTimestamps () [timestep];
		debugText.text = (variableName + ": " + ts.Month + "/" + ts.Day + " " + ts.Hour + ":" + ts.Minute);
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		ColorRamp ramp;
		if (showChange) {
			ramp = ColorRamp.GetColorRamp ("erdc_divLow_icePeach");
		} else {
			ramp = ColorRamp.GetColorRamp("erdc_pbj_lin");
		}
		Material[] materials = new Material[mesh.subMeshCount];
		
		string varName = variableName;
		
		

		float[] data = qesReader.GetPatchData (varName, timestep);
		if (showChange) {
			float [] nextData = qesReader.GetPatchData (varName, timestep + 1);
			for (int i=0; i<data.Length; i++) {
				data [i] = nextData [i] - data [i];
			}
		}
		
		QESVariable[] vars = qesReader.getVariables ();
		float minVal = -1, maxVal = -1;
		float volMinVal = -1, volMaxVal = -1;

		if (showChange) {
			minVal = -Mathf.Abs (changeRange);
			maxVal = Mathf.Abs (changeRange);
		} else {
			for (int i=0; i<vars.Length; i++) {
				if (vars [i].Name == varName) {
					minVal = vars [i].Min;
					maxVal = vars [i].Max;
				}
			}
		}

		// Set the bounds in the legend - data is used to change text elements in the legend
		maxTextBox.text = maxVal.ToString();
		minTextBox.text = minVal.ToString();

		//
		// UI Panel has a Raw Image that has a texture. Try to get a hold of texture and set a 1D texture to map across the space?
		//
		int numSamples = 50;

		Texture2D legendTex = new Texture2D (1, numSamples);
		Color[] colorRamp = ramp.Ramp (numSamples);
		legendTex.SetPixels (colorRamp);
		legendTex.wrapMode = TextureWrapMode.Clamp;
		legendTex.Apply ();
		legend.texture = legendTex;


		for (int faceIndex=0; faceIndex < faces.Count; faceIndex++) {
			
			materials [faceIndex] = new Material (baseMaterial);
			
			Texture2D faceTex = new Texture2D (faces [faceIndex].SampleWidth, faces [faceIndex].SampleHeight);
			
			faceTex.filterMode = FilterMode.Point;
			faceTex.wrapMode = TextureWrapMode.Clamp;
			
			int sampleCount = faces [faceIndex].SampleWidth * faces [faceIndex].SampleHeight;
			int baseIndex = faces [faceIndex].PatchIndex;
			
			Color[] colors = new Color[sampleCount];
			
			for (int patch=baseIndex; patch<baseIndex + sampleCount; patch++) {
				float mappedVal = (data [patch] - minVal) / (maxVal - minVal);
				colors [patch - baseIndex] = ramp.Value (mappedVal);
			}
			
			faceTex.SetPixels (colors);
			faceTex.Apply ();
			materials [faceIndex].mainTexture = faceTex;
		}
		
		
		
		MeshRenderer mr = GetComponent<MeshRenderer> ();
		mr.materials = materials;
		
		Resources.UnloadUnusedAssets ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		int oldTimestep = timestep;
		string oldVariable = variableName;
		bool oldShowChange = showChange;

		if (automateTimestepping) {

			if (timePerTimestep > 0.0) {
				// Decrease time to wait
				timePerTimestep -= Time.deltaTime;
			}
			else {
				timePerTimestep = timeWait;
				timestep++;
			}

			if (timestep > qesReader.getTimestamps ().Length) {
				// go back to regular control of timestepping
				automateTimestepping = false;
				oldTimestep = timestep;
			}

		} else {

			bool shift = Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift);
			if (Input.GetKey (KeyCode.LeftBracket)) {
				if (timestepUp) {
					if (shift) {
						timestep -= 6;
					} else {
						timestep--;
					}
				}
				timestepUp = false;
			} else if (Input.GetKey (KeyCode.RightBracket)) {
				if (timestepUp) {
					if (shift) {
						timestep += 6;
					} else {
						timestep++;
					}
				}
				timestepUp = false;
			} else {
				timestepUp = true;
			}
		}

		if (Input.GetKey (KeyCode.C)) {
			showChange = !showChange;
		}
		if (Input.GetKey (KeyCode.N)) {
			variableName = "patch_nir";
		}
		if (Input.GetKey (KeyCode.P)) {
			variableName = "patch_par";
		}
		if (Input.GetKey (KeyCode.L)) {
			variableName = "patch_longwave";
		}
		if (Input.GetKey (KeyCode.T)) {
			variableName = "patch_temperature";
		}
		if (timestep < 0) {
			timestep = 0;
		}
		
		if (timestep >= qesReader.getTimestamps ().Length) {
			timestep = qesReader.getTimestamps ().Length - 1;
		}
		if (timestep != oldTimestep
		    || variableName != oldVariable
		    || oldShowChange != showChange) {
			System.Diagnostics.Stopwatch allMat = new System.Diagnostics.Stopwatch ();
			setMaterials ();
		}
	}
	
	private QESReader qesReader;
	private int timestep;
	private List<QESFace> faces;
	private bool timestepUp;
	private float timeWait;
}
