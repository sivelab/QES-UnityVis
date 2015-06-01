using UnityEngine;

/// <summary>
/// Simple struct to represent a QES building, which can contain one or more QESFaces
/// </summary>
public class QESBuilding {
	public QESBuilding(QESFace[] faces) {
		Faces = faces;
	}

	public QESFace[] Faces { get; private set; }
}
