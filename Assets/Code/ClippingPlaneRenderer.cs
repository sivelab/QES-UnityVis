using UnityEngine;
using System.Collections;

public class ClippingPlaneRenderer : MonoBehaviour, IVolumeTextureUser, IQESSettingsUser {

	Texture3D volumeTexture;
	Vector3 relativeBounds;	
	QESSettings settings;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void LateUpdate() {
		Vector3 patchDims = settings.Reader.PatchDims;
		Vector3 worldDims = settings.Reader.WorldDims;
		Vector3 scale = new Vector3(1/(patchDims.x * worldDims.x),
		                            1/(patchDims.y * worldDims.y),
		                            1/(patchDims.z * worldDims.z));

		//Matrix4x4 mat = Matrix4x4.Scale(scale) * transform.parent.worldToLocalMatrix;
		Matrix4x4 mat = Matrix4x4.Scale (scale) * transform.parent.GetComponent<MeshRenderer>().worldToLocalMatrix;
		GetComponent<MeshRenderer>().material.SetMatrix("WorldToVolume", mat);
	}

	public void UpdateTexture(Texture3D tex, Vector3 rb) {
		Material mat = GetComponent<MeshRenderer>().material;
		if (mat == null) {
			throw new UnityException("Material not set");
		}
		volumeTexture = tex;
		mat.SetTexture("_MainTex", volumeTexture);

		relativeBounds = rb;
		mat.SetVector("_RelativeBounds", new Vector4(relativeBounds.x, relativeBounds.y, relativeBounds.z, 0.0f));

	}

	public void SetSettings (QESSettings set)
	{
		settings = set;
	}
}
