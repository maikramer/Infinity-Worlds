using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MkGames
{
	public class PlayerController : MonoBehaviour {

		private const string MapTag = MapGenerator.MapTag;
		public Transform CameraTransform;
		public int ForceStrengh;
		[SerializeField] private float _startDelay;
		[SerializeField] private float _startDistanceFromGround;

		private Rigidbody _rigidbody;
		private bool _onGround;
		private bool _canStart;

		private void Start()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_rigidbody.useGravity = false;

		}

		public void StartGame()
		{
			transform.position = GetStartPosition();
			StartCoroutine(RunAfterDelay(_startDelay));
		}

		private void OnCollisionStay(Collision coll)
		{
			if (!_canStart) return;

			if (coll.collider.tag != MapTag) return;

			if (Input.GetKey(KeyCode.UpArrow))
				_rigidbody.AddForce(CameraTransform.forward * ForceStrengh * Time.deltaTime);
			if (Input.GetKey(KeyCode.DownArrow))
				_rigidbody.AddForce(-CameraTransform.forward * ForceStrengh * Time.deltaTime);
			if (Input.GetKey(KeyCode.LeftArrow))
				_rigidbody.AddForce(-CameraTransform.right * ForceStrengh * Time.deltaTime);
			if (Input.GetKey(KeyCode.RightArrow))
				_rigidbody.AddForce(CameraTransform.right * ForceStrengh * Time.deltaTime);
		}

		private IEnumerator RunAfterDelay(float delay)
		{
			yield return new WaitForSeconds(delay);
			_rigidbody.useGravity = true;
			_canStart = true;
		}

		private Vector3 GetStartPosition()
		{
			var actualPosition = transform.position;
			RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down);

			foreach (var hit in hits)
			{
				if (hit.transform.tag == MapTag)
				{
					var startHeight = actualPosition.y - hit.distance + _startDistanceFromGround;
					return new Vector3(actualPosition.x, startHeight, actualPosition.z);
				}
			}

			return actualPosition;
		}
	}//PlayerController
}//MkGames
