using UnityEngine;

namespace TrackpadTouch {

public class LockCursor : MonoBehaviour {

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
	public CursorLockMode wantedMode;

	void SetCursorState() {
		Cursor.lockState = wantedMode;
		Cursor.visible = CursorLockMode.Locked != wantedMode;
	}

	void OnGUI() {
		GUILayout.BeginVertical();

		if (Input.GetKeyDown(KeyCode.Escape))
			Cursor.lockState = wantedMode = CursorLockMode.None;

		switch (Cursor.lockState) {
			case CursorLockMode.None:
			if (GUILayout.Button("Lock cursor"))
				wantedMode = CursorLockMode.Locked;
				break;
			case CursorLockMode.Confined:
			if (GUILayout.Button("Lock cursor"))
				wantedMode = CursorLockMode.Locked;
			GUILayout.Label("Cursor is confined.");
			if (GUILayout.Button("Release cursor"))
				wantedMode = CursorLockMode.None;
				break;
			case CursorLockMode.Locked:
				GUILayout.Label("Cursor is locked. Press Escape to unlock.");
				break;
		}
		GUILayout.EndVertical();
		SetCursorState();
	}
#endif
}

} // namespace TrackpadTouch
