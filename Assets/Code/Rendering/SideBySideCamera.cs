using UnityEngine;
using System.Collections;

/// <summary>
/// Utility script to set two cameras for side-by-side stereo.
/// 
/// This handles off-axis projection
/// </summary>
[ExecuteInEditMode]
public class SideBySideCamera : MonoBehaviour
{

	public Camera leftEyeCamera;
	public Camera rightEyeCamera;
	public float NearClipPlane = 0.01f;
	public float FarClipPlane = 100f;
	[Range(0.1f,179)]
	public float
		FieldOfView = 60f;
	[Range(0,1)]
	public float
		HorizontalOffset = 0.1f;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		float realRatio = leftEyeCamera.pixelWidth * 1.0f / leftEyeCamera.pixelHeight;
		float fakeRatio = realRatio * 2.0f;
		float fvw = Mathf.Tan (Mathf.Deg2Rad * FieldOfView / 2);
		leftEyeCamera.projectionMatrix = PerspectiveOffCenter ((-fakeRatio * fvw + HorizontalOffset / 2) * NearClipPlane,
		                                                       (fakeRatio * fvw + HorizontalOffset / 2) * NearClipPlane,
		                                                       -1 * fvw * NearClipPlane,
		                                                       1 * fvw * NearClipPlane,
		                                                       NearClipPlane,
		                                                       FarClipPlane);

		rightEyeCamera.projectionMatrix = PerspectiveOffCenter ((-fakeRatio * fvw - HorizontalOffset / 2) * NearClipPlane,
		(fakeRatio * fvw - HorizontalOffset / 2) * NearClipPlane,
		-1 * fvw * NearClipPlane,
		1 * fvw * NearClipPlane,
		NearClipPlane,
		FarClipPlane);
	}

	static Matrix4x4 PerspectiveOffCenter (float left, float right, float bottom, float top, float near, float far)
	{
		float x = 2.0F * near / (right - left);
		float y = 2.0F * near / (top - bottom);
		float a = (right + left) / (right - left);
		float b = (top + bottom) / (top - bottom);
		float c = -(far + near) / (far - near);
		float d = -(2.0F * far * near) / (far - near);
		float e = -1.0F;
		Matrix4x4 m = new Matrix4x4 ();
		m [0, 0] = x;
		m [0, 1] = 0;
		m [0, 2] = a;
		m [0, 3] = 0;
		m [1, 0] = 0;
		m [1, 1] = y;
		m [1, 2] = b;
		m [1, 3] = 0;
		m [2, 0] = 0;
		m [2, 1] = 0;
		m [2, 2] = c;
		m [2, 3] = d;
		m [3, 0] = 0;
		m [3, 1] = 0;
		m [3, 2] = e;
		m [3, 3] = 0;
		return m;
	}
}
