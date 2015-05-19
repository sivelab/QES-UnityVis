using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]

public class DrawQESVolume2 : MonoBehaviour, IQESSettingsUser, IQESVisualization
{
	public Material transparentMaterial;
	public Camera mainCamera;
	public int numSlices = 20;
	public string volumeName = "ac_temperature";

	public bool doNoise = true;

	// Use this for initialization
	void Start ()
	{
		childs = new List<GameObject> ();

		CreateNoiseTexture ();
	}

	public void ReloadData() {
		material = null;
		CreateTexture ();
		SetMesh ();
	}

	public void SetSettings(QESSettings set) {
		if (settings != null) {
			settings.DatasetChanged -= ReloadData;
			settings.TimestepChanged -= ReloadData;
		}
		settings = set;

		settings.DatasetChanged += ReloadData;
		settings.TimestepChanged += ReloadData;

		ReloadData ();
	}

	int nearestPowerOfTwo (int val)
	{
		if (val > 2048) {
			return 4096;
		} else if (val > 1024) {
			return 2048;
		} else if (val > 512) {
			return 1024;
		} else if (val > 256) {
			return 512;
		} else if (val > 128) {
			return 256;
		} else if (val > 64) {
			return 128;
		} else if (val > 32) {
			return 64;
		} else if (val > 16) {
			return 32;
		} else if (val > 8) {
			return 16;
		} else if (val > 4) {
			return 8;
		} else {
			return 4;
		}
	}

	void CreateNoiseTexture() {
		int noiseDim = 256;
		if (noiseTex == null) {
			noiseTex = new Texture2D (noiseDim, noiseDim, TextureFormat.RGBA32, false);
		}
		Color[] colors = new Color[noiseDim * noiseDim];
		for (int i=0; i<colors.Length; i++) {
			float v = (Random.value * 2 - 1) * (Random.value * 2 - 1) * (Random.value * 2 - 1);
			v = v * 0.5f + 0.5f;
			colors [i] = new Color (v, v, v, v);
		}
		noiseTex.SetPixels (colors);
		noiseTex.filterMode = FilterMode.Bilinear;
		noiseTex.Apply ();
	}

	void CreateTexture ()
	{
		float[] volData = settings.Reader.GetPatchData (volumeName, settings.CurrentTimestep);
		Vector3 patchDims = settings.Reader.PatchDims;
		
		QESVariable var = null;
		
		for (int i=0; i<settings.Reader.getVariables().Length; i++) {
			if (settings.Reader.getVariables () [i].Name == volumeName) {
				var = settings.Reader.getVariables () [i];
			}
		}

		float maxVal = var.Max;
		float minVal = var.Min;

		Vector3 worldDims = settings.Reader.WorldDims;
		int width = (int)worldDims.x;
		int height = (int)worldDims.y;
		int depth = (int)worldDims.z;

		int texWidth = nearestPowerOfTwo (width);
		int texHeight = nearestPowerOfTwo (height);
		int texDepth = nearestPowerOfTwo (depth);

		cubeTex = new Texture3D (texWidth, texHeight, texDepth, TextureFormat.RGBA32, false);
		cubeTex.wrapMode = TextureWrapMode.Clamp;
		

		Color[] colors = new Color[texWidth * texHeight * texDepth];

		for (int z=0; z<texDepth; z++) {
			for (int y=0; y<texHeight; y++) {
				for (int x=0; x<texWidth; x++) {
					int texSample = z * texHeight * texWidth + y * texWidth + x;
					float mappedVal = 0.0f;
					if (z < depth && y < height && x < width) {
						int dataSample = z * height * width + y * width + x;
						mappedVal = (volData [dataSample] - minVal) / (maxVal - minVal);
						if (mappedVal < 0)
							mappedVal = 0;
						if (mappedVal > 1)
							mappedVal = 1;
					}
					colors [texSample] = new Color (mappedVal, mappedVal, mappedVal, mappedVal);
				}
			}
		}
		cubeTex.SetPixels (colors);
		cubeTex.Apply ();
		relativeAmounts = new Vector4 (width * 1.0f / texWidth, height * 1.0f / texHeight, depth * 1.0f / texDepth, 0);
	}
	
	void SetMesh ()
	{	
		CreateNoiseTexture ();
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		if (mesh == null) {
			mesh = new Mesh ();
			GetComponent<MeshFilter> ().mesh = mesh;

		}
		if (material == null) {
			material = new Material (transparentMaterial);
			material.mainTexture = cubeTex;
			material.SetVector ("_RelativeBounds", relativeAmounts);

			int colorRampWidth = 16;
			int colorRampHeight = 256;
			Texture2D colorRamp = new Texture2D (colorRampWidth, colorRampHeight, TextureFormat.RGBA32, false);
			colorRamp.wrapMode = TextureWrapMode.Clamp;
			ColorRamp ramp = ColorRamp.GetColorRamp("erdc_cyan2orange");
			Color[] colors = new Color[colorRampWidth * colorRampHeight];
			for (int y=0; y<colorRampHeight; y++) {
				float yVal = y * 1.0f / colorRampHeight;

				if (yVal < 0) yVal = 0;
				if (yVal > 1) yVal = 1;

				Color col = ramp.Value (yVal);
				col.a = Mathf.Pow (yVal, 3);
				for (int x=0; x<colorRampWidth; x++) {
					colors[y * colorRampWidth + x] = col;
				}
			}
			colorRamp.SetPixels(colors);
			colorRamp.Apply();
			material.SetTexture("_RampTex", colorRamp);
			material.SetTexture ("_NoiseTex", noiseTex);
		}
		GetComponent<MeshRenderer> ().material = material;

		List<Vector3> vertices = new List<Vector3> ();
		List<Vector2> uvs = new List<Vector2> ();
		List<Vector2> ws = new List<Vector2> ();
		List<int> indices = new List<int> ();
		// Splitting texture coordinates into separate UV and W arrays
		// because Unity doesn't allow us to set all four (or even three)
		// components of the texture coordinate.  In the fragment shader,
		// we need to convert this back into a vec3.

		Vector3 worldDims = settings.Reader.WorldDims;
		Vector3 patchDims = settings.Reader.PatchDims;

		Vector3 worldCameraPosition = mainCamera.transform.position;
		Vector3 worldCameraOrientation = mainCamera.transform.forward;
		Vector3 localCameraPosition = transform.InverseTransformPoint (worldCameraPosition);
		Vector3 localCameraOrientation = transform.InverseTransformDirection (worldCameraOrientation);

		Vector3 localCenter = worldDims * 0.5f;
		localCenter.Scale (patchDims);
		float localRadius = localCenter.magnitude;

		Vector3 localN = -localCameraOrientation.normalized;
		Vector3 localU = new Vector3 (1, 0, 0);
		Vector3 localV = Vector3.Cross (localN, localU);
		if (localV.sqrMagnitude < 0.05) {
			localU = new Vector3 (0, 1, 0);
			localV = Vector3.Cross (localN, localU);
		}
		localV.Normalize ();
		localU = Vector3.Cross (localV, localN);
		localU.Normalize ();

		material.SetVector ("_CameraTexPosition", texCoords(localCenter, localCameraPosition));
		material.SetFloat ("_NumSlices", numSlices);

		for (int i=0; i<=numSlices; i++) {
			Vector3 planeOrigin = localCenter + localN * localRadius * ((i * 2.0f) / (numSlices) - 1.0f);

			Vector3 pt, uvw;

			pt = planeOrigin - localRadius * localU - localRadius * localV;
			uvw = texCoords (localCenter, pt);
			vertices.Add (pt);
			uvs.Add (new Vector2 (uvw.x, uvw.y));
			ws.Add (new Vector2 (uvw.z, 0));

			pt = planeOrigin + localRadius * localU - localRadius * localV;
			uvw = texCoords (localCenter, pt);
			vertices.Add (pt);
			uvs.Add (new Vector2 (uvw.x, uvw.y));
			ws.Add (new Vector2 (uvw.z, 0));

			pt = planeOrigin + localRadius * localU + localRadius * localV;
			uvw = texCoords (localCenter, pt);
			vertices.Add (pt);
			uvs.Add (new Vector2 (uvw.x, uvw.y));
			ws.Add (new Vector2 (uvw.z, 0));

			pt = planeOrigin - localRadius * localU + localRadius * localV;
			uvw = texCoords (localCenter, pt);
			vertices.Add (pt);
			uvs.Add (new Vector2 (uvw.x, uvw.y));
			ws.Add (new Vector2 (uvw.z, 0));

			indices.Add (vertices.Count - 4);
			indices.Add (vertices.Count - 3);
			indices.Add (vertices.Count - 2);

			indices.Add (vertices.Count - 4);
			indices.Add (vertices.Count - 2);
			indices.Add (vertices.Count - 3);
			
			indices.Add (vertices.Count - 4);
			indices.Add (vertices.Count - 2);
			indices.Add (vertices.Count - 1);

			indices.Add (vertices.Count - 4);
			indices.Add (vertices.Count - 1);
			indices.Add (vertices.Count - 2);
		}

		Vector3[] vertexList = new Vector3[vertices.Count];
		Vector2[] uvList = new Vector2[uvs.Count];
		Vector2[] wList = new Vector2[ws.Count];
		int[] indexList = new int[indices.Count];

		vertices.CopyTo (vertexList);
		uvs.CopyTo (uvList);
		ws.CopyTo (wList);
		indices.CopyTo (indexList);

		mesh.vertices = vertexList;
		mesh.uv = uvList;
		mesh.uv2 = wList;
		mesh.triangles = indexList;

	}

	private Vector3 texCoords (Vector3 localCenter, Vector3 pos)
	{
		Vector3 bounds = localCenter * 2.0f;
		Vector3 ans = pos;
		ans.Scale (new Vector3 (1 / bounds.x, 1 / bounds.y, 1 / bounds.z));
		return ans;
	}
	
	// Update is called once per frame
	void Update ()
	{
		SetMesh ();
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			volumeName = "ac_temperature";
			material = null;
			CreateTexture ();
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			volumeName = "ac_temps";
			material = null;
			CreateTexture ();
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			volumeName = "ac_diff";
			material = null;
			CreateTexture ();
		}

	}

	// IQESVisualization methods:

	public QESVariable.Type VariableType() {
		return QESVariable.Type.AIRCELL;
	}
	
	public string CurrentVariable() {
		return volumeName;
	}
	
	public void SetCurrentVariable(string var) {
		if (volumeName != var) {
			volumeName = var;
			material = null;
			CreateTexture ();
		}
	}

	public string VisualizationName() {
		return "Volume Scalar Fields";
	}

	private List<QESFace> faces;
	private List<GameObject> childs;
	private Texture3D cubeTex;
	private Texture2D noiseTex;
	private Vector4 relativeAmounts;
	private Material material;
	private QESSettings settings;
}
