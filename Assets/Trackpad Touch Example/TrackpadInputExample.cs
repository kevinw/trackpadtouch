using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TrackpadTouch {

public class TrackpadInputExample : MonoBehaviour {
	public GameObject prefab;

	Dictionary<int, GameObject> touchObjects = new Dictionary<int, GameObject>();

	void OnDisable() {
		foreach (var gameObject in touchObjects.Values)
			Object.Destroy(gameObject);
		touchObjects.Clear();
	}
	
	void Update () {
		foreach (var touch in TrackpadInput.touches) {
			var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));
			worldPos.z = 0;

			GameObject debugSphere;

			switch (touch.phase) {
			case TouchPhase.Began:
				if (touchObjects.TryGetValue(touch.fingerId, out debugSphere))
					Object.Destroy(debugSphere);
				debugSphere = touchObjects[touch.fingerId] = (GameObject)Object.Instantiate(prefab, worldPos, Quaternion.identity);	
				debugSphere.name = "Touch " + touch.fingerId;
				//Debug.Log("Finger " + touch.fingerId + " began at " + touch.position);
				break;
			case TouchPhase.Moved:
				if (touchObjects.TryGetValue(touch.fingerId, out debugSphere))
					debugSphere.transform.position = worldPos;
				//Debug.Log("Finger " + touch.fingerId + " moved to " + touch.position);
				break;
			case TouchPhase.Canceled:
			case TouchPhase.Ended:
				if (touchObjects.TryGetValue(touch.fingerId, out debugSphere))
					Object.Destroy(debugSphere);
				touchObjects.Remove(touch.fingerId);
				//Debug.Log("Finger " + touch.fingerId + " ended at " + touch.position);
				break;
			// case TouchPhase.Stationary:
			// break;
			default:
				break;
			}
		}
	}
}

}

