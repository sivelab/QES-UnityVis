using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]

public class DrawQES : MonoBehaviour
{
	public Material baseMaterial;
	// Use this for initialization
	void Start ()
	{
		QESDirectorySource directorySource = new QESDirectorySource ("/scratch/schr0640/tmp/testexport/");

		QESReader qesReader = new QESReader (directorySource);

		List<Vector3> vertices = new List<Vector3> ();
		List<Vector3> normals = new List<Vector3> ();
		List<Vector2> uvs = new List<Vector2> ();
		List<List<int>> indices = new List<List<int>> ();
		List<QESFace> faces = new List<QESFace> ();

		float scaleFactor = 0.1f;

		foreach (QESBuilding building in qesReader.Buildings) {
			foreach (QESFace face in building.Faces) {
				List<int> myIndices = new List<int>();
				vertices.Add (face.Anchor * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2(0,0));

				vertices.Add ((face.Anchor + face.V1) * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2(1,0));

				vertices.Add ((face.Anchor + face.V1 + face.V2) * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2(1,1));

				vertices.Add ((face.Anchor + face.V2) * scaleFactor);
				normals.Add (face.Normal);
				uvs.Add (new Vector2(0,1));

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

				indices.Add(myIndices);
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

		Material[] materials = new Material[indices.Count];

		float[] data = qesReader.GetPatchData ("patch_temperature");
		float minVal = data [0];
		float maxVal = data [0];
		for (int i=0; i<data.Length; i++) {
			if (data[i] > maxVal) {
				maxVal = data[i];
			}
			if (data[i] < minVal) {
				minVal = data[i];
			}
		}

		for (int faceIndex=0; faceIndex < indices.Count; faceIndex++) {
			int[] indicesArray = new int[indices[faceIndex].Count];
			indices[faceIndex].CopyTo(indicesArray);
			mesh.SetTriangles(indicesArray, faceIndex);

			materials[faceIndex] = new Material(baseMaterial);

			Texture2D faceTex = new Texture2D(faces[faceIndex].SampleWidth, faces[faceIndex].SampleHeight);

			faceTex.filterMode = FilterMode.Point;


			int sampleCount = faces[faceIndex].SampleWidth * faces[faceIndex].SampleHeight;
			int baseIndex = faces[faceIndex].PatchIndex;

			Color[] colors = new Color[sampleCount];

			for (int patch=baseIndex; patch<baseIndex + sampleCount; patch++) {
				float mappedVal = (data[patch] - minVal)/(maxVal - minVal);
				colors[patch - baseIndex] = new Color(mappedVal, mappedVal, mappedVal);
			}

			faceTex.SetPixels(colors);
			faceTex.Apply();
			materials[faceIndex].mainTexture = faceTex;
		}

		mesh.name = "DrawQES";

		mesh.Optimize ();

		GetComponent<MeshFilter> ().mesh = mesh;
		MeshRenderer mr = GetComponent<MeshRenderer> ();
		mr.materials = materials;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
