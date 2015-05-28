using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]

// 1. Compute exit coordinates based on depth texture
//    Geometry: Blit
//    Input: Depth texture
//    Output: Exit coordinate texture
//
// 3. Render volume box into exit texture, testing against depth texture
//    Geometry: Volume Box
//    Input: Depth texture
//    Output: Exit coordinate texture
//
// 4. Render volume box into output buffer, using stencil to detect inside of box
//    Geometry: Volume Box
//    Input: Exit coordinate texture, 3D volume
//    Output: Screen
//
// 5. Fill in screen-space hole in enter coordinates, using computed stencil
//    Geometry: Full-screen quad
//    Input: None
//    Output: Enter coordinate texture
//

public class DrawQESVolume3 : MonoBehaviour, IQESSettingsUser, IQESVisualization
{
	public Material onlyWriteDepthMaterial;

	// Material to draw volume rendering and output to color buffer (stage 6)
	public Material volumeRenderMaterial;

	public Material clipPlaneMaterial;

	public Material farPlaneMaterial;
	
	[Range(10,500)]
	public int numSteps = 20;
	public string volumeName = "ac_temperature";
	
	private Texture3D cubeTex;
	private Texture2D noiseTex;
	private Texture2D colorRamp;

	private Vector4 relativeAmounts;
	private Vector4 relativeSize;
	private Material material;
	private QESSettings settings;
	private GameObject child;
	private GameObject farPlane;

	public Camera mainCamaera;
	
	// Use this for initialization
	void Start ()
	{	
		child = new GameObject ("Back face");
		child.AddComponent<MeshFilter> ();
		child.AddComponent<MeshRenderer> ();
		child.GetComponent<MeshFilter> ().mesh = new Mesh ();
		child.transform.SetParent (gameObject.transform, false);
		child.GetComponent<MeshRenderer> ().material = onlyWriteDepthMaterial;

		farPlane = new GameObject ("Far face");
		farPlane.AddComponent<MeshFilter> ();
		farPlane.AddComponent<MeshRenderer> ();
		farPlane.GetComponent<MeshFilter> ().mesh = new Mesh ();
		farPlane.transform.SetParent (gameObject.transform, false);
		farPlane.GetComponent<MeshRenderer> ().material = farPlaneMaterial;

		CreateNoiseTexture ();
	}

	void Update() {

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
		if (width > height && width > depth) {
			relativeSize = new Vector4(1, height * 1.0f / width, depth * 1.0f / width, 0);
		} else if (height > width && height > depth) {
			relativeSize = new Vector4(width * 1.0f / height, 1, depth * 1.0f / height, 0);
		} else {
			relativeSize = new Vector4(width * 1.0f / depth, height * 1.0f / depth, 1, 0);
		}
	}

	public void OnWillRenderObject()
	{
		var act = gameObject.activeInHierarchy && enabled;
		if (!act) {
			return;
		}
		
		var cam = Camera.current;
		if (!cam)
			return;

		cam.depthTextureMode = DepthTextureMode.Depth;

		Matrix4x4 mat = cam.cameraToWorldMatrix;

		volumeRenderMaterial.SetMatrix ("CameraToWorld", mat);
		clipPlaneMaterial.SetMatrix ("CameraToWorld", mat);

		//SetClipPlane ();
	}

	void SetColorRamp() {
		if (colorRamp != null) {
			return;
		}
		int colorRampWidth = 16;
		int colorRampHeight = 256;
		colorRamp = new Texture2D (colorRampWidth, colorRampHeight, TextureFormat.RGBA32, false);
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
	}
	
	void SetMesh ()
	{	
		CreateNoiseTexture ();
		SetColorRamp();
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		if (mesh == null) {
			mesh = new Mesh ();
			GetComponent<MeshFilter> ().mesh = mesh;
			
		}

		Vector3 worldDims = settings.Reader.WorldDims;
		Vector3 patchDims = settings.Reader.PatchDims;
		worldDims.x *= patchDims.x;
		worldDims.y *= patchDims.y;
		worldDims.z *= patchDims.z;

		Vector3[] vertexList = {
			new Vector3 (0, 0, 0),
			new Vector3 (0, 0, worldDims.z),
			new Vector3 (0, worldDims.y, 0),
			new Vector3 (0, worldDims.y, worldDims.z),
			new Vector3 (worldDims.x, 0, 0),
			new Vector3 (worldDims.x, 0, worldDims.z),
			new Vector3 (worldDims.x, worldDims.y, 0),
			new Vector3 (worldDims.x, worldDims.y, worldDims.z)};
		Vector2[] uvList = {
			new Vector2 (0, 0),
			new Vector2 (0, 0),
			new Vector2 (0, 1),
			new Vector2 (0, 1),
			new Vector2 (1, 0),
			new Vector2 (1, 0),
			new Vector2 (1, 1),
			new Vector2 (1, 1)};
		Vector2[] wList = {
			new Vector2 (0, 0),
			new Vector2 (1, 0),
			new Vector2 (0, 0),
			new Vector2 (1, 0),
			new Vector2 (0, 0),
			new Vector2 (1, 0),
			new Vector2 (0, 0),
			new Vector2 (1, 0)};
		int[] indexList = {
			0, 2, 6,
			0, 6, 4,
			4, 6, 7,
			4, 7, 5,
			5, 7, 3,
			5, 3, 1,
			1, 3, 2,
			1, 2, 0,
			6, 3, 7,
			6, 2, 3,
			4, 5, 1,
			4, 1, 0};

		int[] flippedIndexList = { 
			2,0,6,
			6,0,4,
			6,4,7,
			7,4,5,
			7,5,3,
			3,5,1,
			3,1,2,
			2,1,0,
			3,6,7,
			2,6,3,
			5,4,1,
			1,4,0};
		
		// Splitting texture coordinates into separate UV and W arrays
		// because Unity doesn't allow us to set all four (or even three)
		// components of the texture coordinate.  In the fragment shader,
		// we need to convert this back into a vec3.

		mesh.vertices = vertexList;
		mesh.uv = uvList;
		mesh.uv2 = wList;
		mesh.triangles = indexList;

		if (child != null) {
			Mesh childMesh = child.GetComponent<MeshFilter> ().mesh;
			childMesh.vertices = vertexList;
			childMesh.triangles = flippedIndexList;
		}
	}

	Vector3 LocalCoordsForPoint(Vector3 worldPoint) {
		Vector3 worldDims = settings.Reader.WorldDims;
		Vector3 patchDims = settings.Reader.PatchDims;
		Vector3 localPosition = transform.InverseTransformPoint (worldPoint);
		return new Vector3 (localPosition.x / (worldDims.x * patchDims.x),
		                   localPosition.y / (worldDims.y * patchDims.y),
		                   localPosition.z / (worldDims.z * patchDims.z));

	}

	void OnRenderObject() {
		Camera camera = Camera.current;
		Vector3[] clipPoints = new Vector3[4];
		Vector2[] uvList = new Vector2[4];
		Vector2[] wList = new Vector2[4];
		int[] indexList = {0, 2, 1, 0, 3, 2};
		Vector2[] positions = {
			new Vector2 (-0.1f, -0.1f),
			new Vector2 (1.1f, -0.1f),
			new Vector2 (1.1f, 1.1f),
			new Vector2 (-0.1f, 1.1f)};
		for (int i=0; i<4; i++) {
			Vector2 p = positions[i];
			clipPoints[i] = camera.ViewportToWorldPoint(new Vector3(p.x, p.y, 5.0f));
			//clipPoints[i] = clipPlane.transform.InverseTransformPoint(clipPoints[i]);
			Vector3 realPos = LocalCoordsForPoint(camera.ViewportToWorldPoint(new Vector3(p.x, p.y, camera.nearClipPlane)));
			uvList[i] = new Vector2(realPos.x, realPos.y);
			wList[i] = new Vector2(realPos.z, 0);
		}
		Mesh mesh = new Mesh ();
		mesh.Clear ();
		mesh.vertices = clipPoints;
		mesh.uv = uvList;
		mesh.uv2 = wList;
		mesh.triangles = indexList;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.UploadMeshData(true);

		clipPlaneMaterial.SetPass (0);

		Graphics.DrawMeshNow (mesh, Matrix4x4.identity);
	}

	private void SetFarPlane() {
		if (farPlane == null)
			return;

		Camera camera = mainCamaera;
		Vector3[] clipPoints = new Vector3[4];
		int[] indexList = {0, 2, 1, 0, 3, 2};
		Vector2[] positions = {
			new Vector2 (-5, -5),
			new Vector2 (6, -5),
			new Vector2 (6, 6),
			new Vector2 (-5, 6)};
		for (int i=0; i<4; i++) {
			Vector2 p = positions[i];
			clipPoints[i] = camera.ViewportToWorldPoint(new Vector3(p.x, p.y, 0.1f * camera.farClipPlane));
			clipPoints[i] = farPlane.transform.InverseTransformPoint(clipPoints[i]);
		}

		if (farPlane != null) {
			Mesh mesh = farPlane.GetComponent<MeshFilter> ().mesh;
			mesh.Clear ();
			mesh.vertices = clipPoints;
			mesh.triangles = indexList;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.UploadMeshData(false);
			mesh.RecalculateBounds();
		}
	}
	
	private Vector3 texCoords (Vector3 localCenter, Vector3 pos)
	{
		Vector3 bounds = localCenter * 2.0f;
		Vector3 ans = pos;
		ans.Scale (new Vector3 (1 / bounds.x, 1 / bounds.y, 1 / bounds.z));
		return ans;
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		SetMesh ();
		GetComponent<MeshRenderer> ().material = volumeRenderMaterial;
		Vector3 worldDims = settings.Reader.WorldDims;
		Vector3 patchDims = settings.Reader.PatchDims;
		Matrix4x4 m = Matrix4x4.Scale (new Vector3(1.0f / (worldDims.x * patchDims.x),
		                               1.0f / (worldDims.y * patchDims.y),
		                               1.0f / (worldDims.z * patchDims.z))) * transform.worldToLocalMatrix;
		volumeRenderMaterial.SetMatrix ("WorldToLocal", m);
		volumeRenderMaterial.SetVector("_RelativeBounds", relativeAmounts);
		volumeRenderMaterial.SetVector("_RelativeSize", relativeSize);
		volumeRenderMaterial.SetFloat("NumSteps", numSteps);
		volumeRenderMaterial.SetTexture("_MainTex", cubeTex);
		volumeRenderMaterial.SetTexture("_RampTex", colorRamp);
		volumeRenderMaterial.SetTexture ("_NoiseTex", noiseTex);

		clipPlaneMaterial.SetMatrix ("WorldToLocal", m);
		clipPlaneMaterial.SetVector("_RelativeBounds", relativeAmounts);
		clipPlaneMaterial.SetVector("_RelativeSize", relativeSize);
		clipPlaneMaterial.SetFloat("NumSteps", numSteps);
		clipPlaneMaterial.SetTexture("_MainTex", cubeTex);
		clipPlaneMaterial.SetTexture("_RampTex", colorRamp);
		clipPlaneMaterial.SetTexture("_NoiseTex", noiseTex);

		SetFarPlane ();
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
}
