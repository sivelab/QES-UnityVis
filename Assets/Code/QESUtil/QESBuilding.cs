using UnityEngine;

public class QESBuilding {
	public QESBuilding(QESFace[] faces) {
		Faces = faces;
	}

	public QESFace[] Faces { get; private set; }
}
