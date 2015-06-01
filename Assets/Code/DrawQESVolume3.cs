using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]

/// <summary>
/// Renders volumetric QES variables.
/// </summary>
/// This class is an evolution from DrawQESVolume and DrawQESVolume2, and 
/// performs integration through space to compute the values for each fragment.
/// Each fragment is only traced if it is visible, and the tracing is only
/// performed on the section of the volume that is visible.
/// 
/// In order to perform this rendering, several steps are performed.  All of
/// the rendering falls under Unity's 'Forward' rendering path, and can be done
/// to an LDR render target without a loss of quality.
/// 
/// 1. Compute a depth texture for each camera.  this is done by setting a property
///    on each camera specifying that we are interested in getting access to the
///    computed depth value.
/// 2. In this depth texture, render the far faces of the volume with
///    onlyWriteDepthMaterial.  Since the shader we wrote doesn't know how to do
///    this, the fallback clause causes the built-in Diffuse shader's depth write
///    fallback to be used.
/// 3. Once we have a texture that contains the depth values, clear the rendered-to
///    depth buffer to a far value.  This is necessary since on DirectX, Unity will
///    use the rendered depth buffer as an early-Z buffer to ensure that only visible
///    fragments have shading computed for them.  Since we have some geometry that
///    shows up in the depth buffer but not the color buffer (the volume far faces),
///    this can cause geometry to be improperly invisible.  In order to clear the
///    depth buffer, we need to render a plane far away from the camera, force
///    the z test to always pass, and force it to write the z value into the buffer.
///    This is done with the farPlaneMaterial.
/// 4. Begin rendering opaque geometry in the entire scene.  Technically, step 3 is
///    a very early step in this phase since it is impossible to force it to appear
///    at a different stage.  After the clear, standard opaque geometry is rendered
///    as normal in a Unity scene.
/// 5. After opaque geometry, but before transparent geometry, the far faces of the
///    volume are rendered (again with onlyWriteDepthMaterial).  Since we are not in
///    a depth rendering pass, we use the shader we wrote.  This sets the stencil
///    to '1' on every fragment that the far faces are rasterized to, but due to the
///    value set for the color write mask, the color buffer is unchanged.
/// 6. We move on to the transparent pass, where the front faces of the volume are
///    rendered with volumeRenderMaterial.  The object this material is placed on
///    has the volume-space coordinates specified as attributes, and so the
///    interpolater will compute for each fragment the position of that fragment in
///    volume space.  We will use this to compute a ray from the camera origin,
///    through this volume entry point, and to wherever this ray exits the volume.
///    We use the depth buffer to find the depth of either the solid geometry in the
///    scene in the direction of this ray, or of the far faces of the volume.  We
///    convert this to volume space coordinates, and can perform our trace.  This
///    shader also sets the stencil values of every fragment it is run on to zero.
/// 7. We now have a stencil buffer set to zero everywhere except for where volume
///    backfaces were drawn, but front faces were not.  This corresponds to fragments
///    where the near clipping plane removed the front faces of the volume because we
///    are inside of the volume.  To solve this, we draw a quad covering the screen,
///    set to render only where the stencil value is 1.
/// 
/// At the end of this process, we have traced the volume.  Note that this will
/// NOT work for overlapping volumes, or on systems such as iOS, Android, and
/// WebGL (I think) where 3D textures don't exist.
public class DrawQESVolume3 : MonoBehaviour, IQESSettingsUser, IQESVisualization
{
	/// <summary>
	/// Material to only output depth values and to set the stencil value to 1
	/// </summary>
	public Material onlyWriteDepthMaterial;

	/// <summary>
	/// Material to draw volume rendering and output to color buffer (step 6)
	/// </summary>
	public Material volumeRenderMaterial;

	/// <summary>
	/// Material to draw volume rendering on the clip plane (step 7)
	/// </summary>
	public Material clipPlaneMaterial;

	/// <summary>
	/// Material to reset the depth value to a far away value (step 3)
	/// </summary>
	public Material farPlaneMaterial;

	/// <summary>
	/// How many steps to trace between the entry and exit point
	/// </summary>
	[Range(10,500)]
	public int numSteps = 20;

	/// <summary>
	/// Which variable to perform volume rendering on
	/// </summary>
	public string volumeName = "ac_temperature";

	/// <summary>
	/// 3D texture representing the data.  Since Unity (at least 4.x) requires
	/// power-of-two 3D textures, this is a large texture.
	/// </summary>
	private Texture3D cubeTex;

	/// <summary>
	/// Noise texture (currently unused) to hide aliasing artifacts
	/// </summary>
	private Texture2D noiseTex;

	/// <summary>
	/// Transfer function (1D) to convert scalar values to color+alpha
	/// </summary>
	private Texture2D colorRamp;

	// How much of each dimension is used in the 3D texture 
	private Vector4 relativeAmounts;

	// Relative size of the three axes (in world-space).  The longest
	// axis has length '1', and shorter axes have a correspondingly
	// smaller value
	private Vector4 relativeSize;
	private Material material;
	private QESSettings settings;

	/// <summary>
	/// GameObject representing the far faces of the volume
	/// </summary>
	private GameObject child;

	/// <summary>
	/// GameObject representing the far clipping plane, used for step 3.
	/// </summary>
	private GameObject farPlane;

	public Camera mainCamaera;
	
	/// <summary>
	/// Create two GameObjects: the volume far faces and the far clipping plane object.
	/// </summary>
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

	/// <summary>
	/// Reload data from QESSettings' QESReader
	/// </summary>
	public void ReloadData() {
		material = null;
		CreateTexture ();
		SetMesh ();
	}

	/// <summary>
	/// Update the QESSettings being used
	/// </summary>
	/// <param name="set">Set.</param>
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

	/// <summary>
	/// Compute the next larger power of two over the passed-in value.
	/// </summary>
	/// Unity (at least 4.x) requires that 3D textures are power-of-two. In
	/// general, our volume data is not necessarily of this dimension. 
	/// Instead of scaling our data, we add blank samples and only use a subset
	/// of the 3D texture space
	/// 
	/// <returns>The next-largest power of two</returns>
	/// <param name="val">Value</param>
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

	/// <summary>
	/// Creates a noise texture
	/// </summary>
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

	/// <summary>
	/// Creates the 3D texture used for volume rendering
	/// </summary>
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

	/// <summary>
	/// When about to be rendered, set the (currently unused) "CameraToWorld"
	/// matrix on each object based on the camera.
	/// </summary>
	public void OnWillRenderObject()
	{
		// TODO: Aside from requesting the depth texture, I don't think anything
		// in this function is needed.
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

	/// <summary>
	/// Calculate the current color ramp texture
	/// </summary>
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

	/// <summary>
	/// Update the 3D mesh representing the front faces of the volume, as
	/// well as the child object representing the back faces of the volume.
	/// </summary>
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

		// For these, note that Unity uses a left-handed coordinate
		// system with clockwise winding for front faces.
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

		// Instead of using a shader that switches the culling mode,
		// we need to use the same winding, since the actual depth rendering
		// of the back faces is performed by a fallback shader that we
		// import indirectly from the "Diffuse" shader, meaning we can't
		// control the culling mode used for that shader.
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

	/// <summary>
	/// Called immediately after a camera has rendered this object.  We use this
	/// time to force the rendering of the near-plane for this specific camera.
	/// </summary>
	void OnRenderObject() {
		Camera camera = Camera.current;
		Vector3[] clipPoints = new Vector3[4];
		Vector2[] uvList = new Vector2[4];
		Vector2[] wList = new Vector2[4];
		int[] indexList = {0, 2, 1, 0, 3, 2};
		// Exactly speaking, we want 0 and 1, but due to floating point error,
		// that would sometimes cause the rightmost column of pixels to not get
		// properly rendered.
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

	/// <summary>
	/// Update the far plane GameObject
	/// </summary>
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
	
	/// <summary>
	/// Updates various shader properties.  If have this in 'Update' instead
	/// of 'LateUpdate', user input can cause these quantities to be one frame
	/// out of date.
	/// </summary>
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
