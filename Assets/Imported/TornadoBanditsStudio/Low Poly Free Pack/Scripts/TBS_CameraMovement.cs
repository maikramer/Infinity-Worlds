using System.Collections;
using UnityEngine;

namespace TornadoBanditsStudio
{
	/// <summary>
	///     Camera movement
	/// </summary>
	public class TBS_CameraMovement : MonoBehaviour
	{
		[SerializeField] private float duration = 25f; //one way duration
		private Vector3 startPostion; //start position (transform.position)

		[SerializeField] private Vector3 targePosition; //position to go

		private IEnumerator Start()
		{
			//Set the start position
			startPostion = transform.position;

			//While playing the scene go to target position and back
			while (true)
			{
				yield return StartCoroutine(MoveCamera(startPostion, targePosition));
				yield return StartCoroutine(MoveCamera(targePosition, startPostion));
			}
		}

		/// <summary>
		///     Move camera from start point to target point based on duration, using lerp
		/// </summary>
		/// <param name="startPoint"></param>
		/// <param name="targetPoint"></param>
		/// <returns></returns>
		private IEnumerator MoveCamera(Vector3 startPoint, Vector3 targetPoint)
		{
			//Initialize the function point and the rate based on duration
			var i = 0f;
			var rate = 1 / duration;

			while (i < 1f)
			{
				//Lerp the position
				i += Time.deltaTime * rate;
				transform.position = Vector3.Lerp(startPoint, targetPoint, i);
				yield return null;
			}
		}

		/// <summary>
		///     Draw the camera path
		/// </summary>
		private void OnDrawGizmosSelected()
		{
			//Draw the camera path
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, 0.3f);
			Gizmos.DrawLine(transform.position, targePosition);
			Gizmos.DrawWireSphere(targePosition, 0.3f);
		}
	}
}