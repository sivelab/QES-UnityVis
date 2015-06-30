using UnityEngine;
using System.Collections;

public interface IVolumeTextureUser {

	void UpdateTexture(Texture3D tex, Vector3 relativeBounds);
}
