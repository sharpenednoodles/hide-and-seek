using UnityEngine;

namespace UnityStandardAssets.Utility
{
	public class SmoothFollow : MonoBehaviour
	{

		// The target we are following
		[SerializeField]
		private Transform target;
		// The distance in the x-z plane to the target
		[SerializeField]
		private float distance = 10.0f;
		// the height we want the camera to be above the target
		[SerializeField]
		private float height = 5.0f;
        //camera transform
        [SerializeField]
        private GameObject cameraTransform;

		[SerializeField]
		private float rotationDamping;
		[SerializeField]
		private float heightDamping;

		// Use this for initialization
		void Start() { }

		// Update is called once per frame
		void LateUpdate()
		{
			// Early out if we don't have a target
			if (!target)
				return;

			// Calculate the current rotation angles
			var wantedRotationAngle = target.eulerAngles.y;
			var wantedHeight = target.position.y + height;

			var currentRotationAngle = transform.eulerAngles.y;
			var currentHeight = transform.position.y;

			// Damp the rotation around the y-axis
			currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

			// Damp the height
			currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

			// Convert the angle into a rotation
			var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            cameraTransform.transform.position = target.position;
            cameraTransform.transform.position -= currentRotation * Vector3.forward * distance;
            cameraTransform.transform.eulerAngles = new Vector3(90, target.transform.eulerAngles.y, 0);

            // Set the height of the camera
            cameraTransform.transform.position = new Vector3(cameraTransform.transform.position.x ,currentHeight , cameraTransform.transform.position.z);

            // Always look at the target
            cameraTransform.transform.LookAt(target);
		}
	}
}