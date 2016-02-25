using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace TrackpadTouch {

public class TrackpadInputExample : MonoBehaviour {
	public GameObject prefab;
	Dictionary<int, GameObject> touchObjects = new Dictionary<int, GameObject>();

	void Update () {

		foreach (var touch in TrackpadInput.touches) {
			
			var screenPoint = new Vector3(touch.position.x, touch.position.y, 0);
			var worldPos = Camera.main.ScreenToWorldPoint(screenPoint);
			worldPos.z = 0;

			GameObject debugSphere;

			switch (touch.phase) {

				case TouchPhase.Began:
					if (touchObjects.TryGetValue (touch.fingerId, out debugSphere))
						Object.Destroy (debugSphere);
					debugSphere = touchObjects [touch.fingerId] = (GameObject)Object.Instantiate (prefab, worldPos, Quaternion.identity);	
					debugSphere.GetComponentInChildren<Text> ().text = touch.fingerId.ToString();
					debugSphere.name = "Touch " + touch.fingerId;
					break;

			case TouchPhase.Moved:
				if (touchObjects.TryGetValue(touch.fingerId, out debugSphere))
					debugSphere.transform.position = worldPos;
				break;

			case TouchPhase.Canceled:
			case TouchPhase.Ended:
				if (touchObjects.TryGetValue(touch.fingerId, out debugSphere))
					Object.Destroy(debugSphere);
				touchObjects.Remove(touch.fingerId);
				break;

			// case TouchPhase.Stationary:
				// break;

			default:
				break;
			}
		}
	}

	void OnDisable() {
		foreach (var gameObject in touchObjects.Values)
			Object.Destroy(gameObject);
		touchObjects.Clear();
	}
	
}

}

