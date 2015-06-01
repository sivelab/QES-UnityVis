using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]

/// <summary>
/// Class that renders patch QES data as geometry with a color ramp applied
/// </summary>
public class DrawQES : MonoBehaviour, IQESSettingsUser, IQESVisualization
{
	/// <summary>
	/// Material used for solid objects.  Note that this material MUST successfully
	/// render to the depth buffer in order for these objects to correctly interact
	/// with DrawQESVolume3.  One shader that does that is the OpaqueUnlitTextured
	/// shader included in this project.
	/// </summary>
	public Material baseMaterial;

	// TODO: unused?
	public Material transparentMaterial;

	/// <summary>
	/// Should the visualization show the instantaneous value, or the change from timestep to timestep?
	/// </summary>
	public bool showChange = false;

	/// <summary>
	/// Range of values to be shown when showing changes
	/// </summary>
	public float changeRange = 20;

	/// <summary>
	/// GUIText used to display the current time and variable being shown
	/// </summary>
	public GUIText debugText;

	/// <summary>
	/// Name of the variable to display
	/// </summary>
	public string variableName = "patch_nir";

	/// <summary>
	/// Should random noise be added to the values being visualized?  This can help in seeing the
	/// structure of datasets where large regions are at a single value
	/// </summary>
	public bool addRandomNoise = false;

	/// <summary>
	/// Text box used to display the numeric value of the maximum of the current variable
	/// </summary>
	public Text maxTextBox;

	/// <summary>
	/// Text box used to display the numeric value of the minimum of the current variable
	/// </summary>
	public Text minTextBox;

	/// <summary>
	/// Text box used to display the units for the current variable
	/// </summary>
	public Text unitTextBox;

	/// <summary>
	/// Image that shows the current color ramp
	/// </summary>
	public Image legend;
	
	void ReloadMesh() {
		SetMesh ();
	}

	void ReloadData() {
		SetMaterials ();
	}

	// Use this for initialization
	void Start ()
	{
		// Do nothing since we might not have QESSettings yet
	}

	/// <summary>
	/// Reload the mesh geometry from QESSettings' QESReader instance
	/// </summary>
	/// Separate meshes are created for each face (since each face has its own texture)
	/// and each of these meshes is added as a submesh
	void SetMesh ()
	{
		if (qesSettings == null || qesSettings.Reader == null) {
			return;
		}
		List<Vector3> vertices = new List<Vector3> ();
		List<Vector3> normals = new List<Vector3> ();
		List<Vector2> uvs = new List<Vector2> ();
		List<List<int>> indices = new List<List<int>> ();
		faces = new List<QESFace> ();
		
		float scaleFactor = 1.0f;
		
		foreach (QESBuilding building in qesSettings.Reader.Buildings) {
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
		foreach (QESSensor sensor in qesSettings.Reader.Sensors) {
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
		
		SetMaterials ();
	}

	/// <summary>
	/// Sets materials for the faces in the scene
	/// </summary>
	void SetMaterials ()
	{
		QESTimestamp ts = qesSettings.Reader.getTimestamps () [qesSettings.CurrentTimestep];
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
		
		

		float[] data = qesSettings.Reader.GetPatchData (varName, qesSettings.CurrentTimestep);
		if (showChange) {
			float [] nextData = qesSettings.Reader.GetPatchData (varName, qesSettings.CurrentTimestep + 1);
			for (int i=0; i<data.Length; i++) {
				data [i] = nextData [i] - data [i];
			}
		}
		
		QESVariable[] vars = qesSettings.Reader.getVariables ();
		float minVal = -1, maxVal = -1;
		float volMinVal = -1, volMaxVal = -1;
		string unitString = "Unknown";

		if (showChange) {
			minVal = -Mathf.Abs (changeRange);
			maxVal = Mathf.Abs (changeRange);
			unitString = "Difference";
		} else {
			for (int i=0; i<vars.Length; i++) {
				if (vars [i].Name == varName) {
					minVal = vars [i].Min;
					maxVal = vars [i].Max;
					unitString = vars[i].Unit;
				}
			}
		}

		// Set the bounds in the legend - data is used to change text elements in the legend
		maxTextBox.text = maxVal.ToString();
		minTextBox.text = minVal.ToString();
		unitTextBox.text = unitString;

		//
		// UI Panel has a Raw Image that has a texture. Try to get a hold of texture and set a 1D texture to map across the space?
		//
		int numSamples = 50;

		Texture2D legendTex = new Texture2D (1, numSamples);
		Color[] colorRamp = ramp.Ramp (numSamples);
		legendTex.SetPixels (colorRamp);
		legendTex.wrapMode = TextureWrapMode.Clamp;
		legendTex.Apply ();
		Sprite spr = Sprite.Create (legendTex, new Rect (0, 0, legendTex.width, legendTex.height), new Vector2 (0, 0), 1);
		legend.sprite = spr;


		for (int faceIndex=0; faceIndex < faces.Count; faceIndex++) {
			
			materials [faceIndex] = new Material (baseMaterial);
			
			Texture2D faceTex = new Texture2D (faces [faceIndex].SampleWidth, faces [faceIndex].SampleHeight);
			
			faceTex.filterMode = FilterMode.Point;
			faceTex.wrapMode = TextureWrapMode.Clamp;
			
			int sampleCount = faces [faceIndex].SampleWidth * faces [faceIndex].SampleHeight;
			int baseIndex = faces [faceIndex].PatchIndex;
			
			Color[] colors = new Color[sampleCount];

			float faceRandom = 0;
			if (addRandomNoise) {
				faceRandom = Random.value * 0.3f;
			}
			
			for (int patch=baseIndex; patch<baseIndex + sampleCount; patch++) {
				float mappedVal = (data [patch] - minVal) / (maxVal - minVal);
				if (addRandomNoise) {
					mappedVal = mappedVal * 0.4f + Random.value * 0.3f + faceRandom;
				}
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
	
	/// <summary>
	/// Provide simple keyboard controls to change variables
	/// </summary>
	void Update ()
	{
		if (qesSettings == null) {
			return;
		}
		if (!qesSettings.IsInteractive) {
			return;
		}
		string oldVariable = variableName;
		bool oldShowChange = showChange;

		if (Input.GetKeyDown (KeyCode.C)) {
			showChange = !showChange;
		}
		if (Input.GetKeyDown (KeyCode.N)) {
			variableName = "patch_nir";
		}
		if (Input.GetKeyDown (KeyCode.P)) {
			variableName = "patch_par";
		}
		if (Input.GetKeyDown (KeyCode.L)) {
			variableName = "patch_longwave";
		}
		if (Input.GetKeyDown (KeyCode.T)) {
			variableName = "patch_temperature";
		}

		if (
		    variableName != oldVariable
		    || oldShowChange != showChange) {
			System.Diagnostics.Stopwatch allMat = new System.Diagnostics.Stopwatch ();
			SetMaterials ();
		}
	}

	public void SetSettings(QESSettings settings) {
		if (qesSettings != null) {
			qesSettings.DatasetChanged -= ReloadMesh;
			qesSettings.TimestepChanged -= ReloadData;
		}
		qesSettings = settings;
		qesSettings.DatasetChanged += ReloadMesh;
		qesSettings.TimestepChanged += ReloadData;
		ReloadMesh ();
	}

	// IQESVisualization member functions:

	public QESVariable.Type VariableType() {
		return QESVariable.Type.PATCH;
	}
	
	public string CurrentVariable() {
		return variableName;
	}
	
	public void SetCurrentVariable(string var) {
		if (var != variableName) {
			variableName = var;
			SetMaterials ();
		}
	}

	public string VisualizationName() {
		return "Surface Scalar Fields";
	}

	private QESSettings qesSettings;
	private List<QESFace> faces;
}
