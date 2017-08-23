using UnityEngine;

namespace MkGames
{
	public class MapDisplay : MonoBehaviour
	{
		[SerializeField] [HideInInspector] private Material material;
		[SerializeField] [HideInInspector] private Mesh mesh;
		[SerializeField] [HideInInspector] private GameObject meshGameObject;
		[SerializeField] [HideInInspector] private Texture2D texture2D;

		private MapGenerator _mapGenerator;

		private void Start()
		{
			_mapGenerator = FindObjectOfType<MapGenerator>();
		}

		private void OnValidate()
		{
			_mapGenerator = FindObjectOfType<MapGenerator>();
		}
		
		public void DrawMap(Texture2D texture, int size)
		{
			mesh = MeshGenerator.CreatePlaneMesh(size, size);
			mesh.name = "Base Plane";
			SetTexture(texture);
			CreateMeshGameObject("Height Map (Generated)");
		}

		private void SetTexture(Texture2D texture)
		{
			if (!texture2D)
				texture2D = new Texture2D(texture.width, texture.height);
			if (texture2D.width != texture.width || texture2D.height != texture.height)
				texture2D.Resize(texture.width, texture.height);
			texture2D.SetPixels(texture.GetPixels());
			texture2D.filterMode = FilterMode.Point;
			texture2D.Apply();
		}
		

		private void CreateMeshGameObject(string meshName)
		{
			if (!mesh)
			{
				Debug.LogError("Mesh inexistente");
				return;
			}

			if (!meshGameObject)
			{
				meshGameObject = new GameObject();
				meshGameObject.AddComponent<MeshFilter>();
				meshGameObject.AddComponent<MeshRenderer>();
			}
			meshGameObject.name = meshName;
			meshGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
			if (!material)
				material = new Material(Shader.Find("Standard")) {mainTexture = texture2D};
			if (_mapGenerator.parameters.useNormalMap)
			{
				Texture2D normalMap = TextureGenerator.NormalMap(texture2D, _mapGenerator.parameters.normalMapStrength);
				material.SetTexture("_BumpMap", normalMap);
			}
			else
			{
				material.SetTexture("_BumpMap", null);
			}
			meshGameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
		}

		
	} // Fim de MapDisplay
} // Fim do namespace