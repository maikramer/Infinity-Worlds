using System.Collections;
using UnityEngine;

namespace MkGames
{
	public class FlyingPlayer : MonoBehaviour
	{
		public Transform CameraTransform;
		public int ForceStrengh;
		[SerializeField] private float _startDelay;

		private Rigidbody _rigidbody;
		private bool _onGround;
		private bool _canStart;

		private void Start()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_rigidbody.useGravity = false;

			if (GameObject.Find(MapGenerator.MapParentName) == null)
				FindObjectOfType<MapGenerator>().GenerateFullMap();
			else
				StartGame();
		}

		public void StartGame()
		{
			StartCoroutine(RunAfterDelay(_startDelay));
		}

		private void OnCollisionStay(Collision coll)
		{
			if (!_canStart) return;

			if (coll.collider.tag != "Map") return;

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
			if (delay > 0)
				yield return new WaitForSeconds(delay);
			
			_rigidbody.useGravity = true;
			_canStart = true;
		}
	} //FlyingPlayer
} //MkGames