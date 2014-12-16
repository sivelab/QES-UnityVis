using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrawQESVolume : MonoBehaviour
{
	public Material transparentMaterial;
	// Use this for initialization
	void Start ()
	{
		QESDirectorySource directorySource = new QESDirectorySource ("/scratch/schr0640/tmp/export-rider/");
		
		qesReader = new QESReader (directorySource);
		
		timestep = qesReader.getTimestamps ().Length / 2;

		childs = new List<GameObject> ();
		
		SetMesh ();


	}
	
	void SetMesh ()
	{	
		string volumeName = "ac_temperature";
		float[] volData = qesReader.GetPatchData (volumeName, timestep);
		Vector3 patchDims = qesReader.PatchDims;

		QESVariable var = null;

		for (int i=0; i<qesReader.getVariables().Length; i++) {
			if (qesReader.getVariables () [i].Name == volumeName) {
				var = qesReader.getVariables () [i];
			}
		}
		
		for (int z=0; z<qesReader.WorldDims.z; z++) {
			List<Vector3> vertices = new List<Vector3> ();
			List<Vector2> uvs = new List<Vector2> ();
			List<Vector3> normals = new List<Vector3> ();
			List<int> myIndices = new List<int> ();
			Vector3 normal = new Vector3 (0, 0, 1);
			
			int x = (int)qesReader.WorldDims.x;
			int y = (int)qesReader.WorldDims.y;
			
			vertices.Add (Vector3.Scale (new Vector3 (0, 0, z), patchDims));
			normals.Add (normal);
			uvs.Add (new Vector2 (0, 0));
			
			vertices.Add (Vector3.Scale (new Vector3 (x, 0, z), patchDims));
			normals.Add (normal);
			uvs.Add (new Vector2 (1, 0));
			
			vertices.Add (Vector3.Scale (new Vector3 (x, y, z), patchDims));
			normals.Add (normal);
			uvs.Add (new Vector2 (1, 1));
			
			vertices.Add (Vector3.Scale (new Vector3 (0, y, z), patchDims));
			normals.Add (normal);
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
			
			GameObject go = new GameObject ();
			MeshFilter meshFilter = go.AddComponent<MeshFilter> ();
			MeshRenderer meshRenderer = go.AddComponent<MeshRenderer> ();
			Mesh mesh = new Mesh ();

			Vector3[] vertArray = new Vector3[vertices.Count];
			Vector2[] uvArray = new Vector2[uvs.Count];
			Vector3[] normalArray = new Vector3[normals.Count];
			int[] indexArray = new int[myIndices.Count];

			vertices.CopyTo (vertArray);
			uvs.CopyTo (uvArray);
			normals.CopyTo (normalArray);
			myIndices.CopyTo (indexArray);

			mesh.vertices = vertArray;
			mesh.uv = uvArray;
			mesh.normals = normalArray;
			mesh.triangles = indexArray;
			meshFilter.mesh = mesh;
			meshRenderer.material = new Material (transparentMaterial);
			meshRenderer.material.mainTexture = TextureForSlice (z, volData, var);

			go.transform.SetParent(this.transform, false);

			childs.Add(go);
			
		}

		

	}

	Texture2D TextureForSlice (int slice, float[] data, QESVariable var)
	{
		float maxVal = var.Max;
		float minVal = var.Min;
		minVal = 300;
		ColorRamp ramp = ColorRamp.GetColorRamp ("erdc_pbj_lin");
		Texture2D sliceTex = new Texture2D ((int)qesReader.WorldDims.x, (int)qesReader.WorldDims.y);
		sliceTex.wrapMode = TextureWrapMode.Clamp;
			
		int sampleCount = sliceTex.width * sliceTex.height;
		Color[] colors = new Color[sampleCount];
		int baseIndex = slice * sampleCount;
		for (int sample=baseIndex; sample<baseIndex + sampleCount; sample++) {
			float mappedVal = (data [sample] - minVal) / (maxVal - minVal);
			colors [sample - baseIndex] = ramp.Value (mappedVal);
			colors [sample - baseIndex].a = mappedVal * mappedVal * mappedVal;
		}
		sliceTex.SetPixels (colors);
		sliceTex.Apply ();

		return sliceTex;
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
		}
	}
	
	private QESReader qesReader;
	private int timestep;
	private List<QESFace> faces;
	private List<GameObject> childs;
}
