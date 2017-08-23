using UnityEngine;

namespace MkGames
{
	public class MeshMerger : MonoBehaviour
	{
		public void MergeMeshes()
		{
			var meshFilters = GetComponentsInChildren<MeshFilter>();
			var combine = new CombineInstance[meshFilters.Length];
			var i = 0;
			while (i < meshFilters.Length)
			{
				combine[i].mesh = meshFilters[i].sharedMesh;
				combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
				meshFilters[i].gameObject.SetActive(false);
				i++;
			}
			transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
			transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
			meshFilters[i].gameObject.SetActive(true);
		}
	} //MeshMerger
} //MkGames