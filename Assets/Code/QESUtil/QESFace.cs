using UnityEngine;

/// <summary>
/// Struct to represent a face of a QESBuilding.
/// </summary>
public class QESFace
{
	public QESFace (Vector3 anchor, Vector3 v1, Vector3 v2, int w, int h, int patchIndex)
	{
		Anchor = anchor;
		V1 = v1;
		V2 = v2;
		SampleWidth = w;
		SampleHeight = h;
		PatchIndex = patchIndex;
	}

	public Vector3 Anchor { get; private set; }

	public Vector3 V1 { get; private set; }

	public Vector3 V2 { get; private set; }

	public Vector3 Normal {
		get {
			return Vector3.Cross(V1, V2).normalized;
		}
	}

	public int SampleWidth { get; private set; }

	public int SampleHeight { get; private set; }

	public int PatchIndex { get; private set;}
}
