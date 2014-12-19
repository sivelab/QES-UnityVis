using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]

public class DrawQES : MonoBehaviour
{
	public Material baseMaterial;
	public Material transparentMaterial;
	// Use this for initialization
	void Start ()
	{
		QESDirectorySource directorySource = new QESDirectorySource ("/scratch/schr0640/tmp/export-uehara/");

		qesReader = new QESReader (directorySource);

		timestep = qesReader.getTimestamps ().Length / 2;

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
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		ColorRamp ramp = ColorRamp.GetColorRamp ("erdc_pbj_lin");
		Material[] materials = new Material[mesh.subMeshCount];

		string varName = "patch_temperature";


		
		float[] data = qesReader.GetPatchData (varName, timestep);

		QESVariable[] vars = qesReader.getVariables ();
		float minVal = -1, maxVal = -1;
		float volMinVal = -1, volMaxVal = -1;
		for (int i=0; i<vars.Length; i++) {
			if (vars [i].Name == varName) {
				minVal = vars [i].Min;
				maxVal = vars [i].Max;
			}
		}

		Debug.Log (volMaxVal);

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
				colors [patch - baseIndex] = ramp.Value(mappedVal);
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
		if (Input.GetKey (KeyCode.LeftBracket)) {
			timestep--;
		}
		if (Input.GetKey (KeyCode.RightBracket)) {
			timestep++;
		}
		if (timestep < 0) {
			timestep = 0;
		}

		if (timestep >= qesReader.getTimestamps ().Length) {
			timestep = qesReader.getTimestamps ().Length - 1;
		}
		if (timestep != oldTimestep) {
			QESTimestamp ts = qesReader.getTimestamps () [timestep];
			Debug.Log ("Current time: " + ts.Hour + ":" + ts.Minute);
			System.Diagnostics.Stopwatch allMat = new System.Diagnostics.Stopwatch ();
			setMaterials ();
		}
	}

	private QESReader qesReader;
	private int timestep;
	private List<QESFace> faces;
}
