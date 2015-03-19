using UnityEngine;

public class QESSensor {
	public QESSensor(QESFace[] faces) {
		Faces = faces;
	}

	public QESFace[] Faces { get; private set; }
}

