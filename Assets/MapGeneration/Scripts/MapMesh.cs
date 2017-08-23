using System.Collections;
using UnityEngine;

namespace MkGames
{
	public class MapMesh : MonoBehaviour
	{
		private MapGenerator _mapGenerator;
		private MeshCollider _meshCollider;
		private MeshFilter _meshFilter;
		private MeshRenderer _meshRenderer;
		public Bounds bounds;
		[HideInInspector] public bool isColliderReady;
		[HideInInspector] public bool isMeshReady;
		[HideInInspector] public bool isTextureReady;

		private void Start()
		{
			gameObject.name = "Map Chunk " + gameObject.GetInstanceID();
			transform.parent = FindObjectOfType<MapGenerator>().gameObject.transform;
			_mapGenerator = FindObjectOfType<MapGenerator>();
			_meshFilter = gameObject.AddComponent<MeshFilter>();
			_meshCollider = gameObject.AddComponent<MeshCollider>();
			_meshRenderer = gameObject.AddComponent<MeshRenderer>();
			StartCoroutine(SetMesh());
			StartCoroutine(SetTexture());
			StartCoroutine(SetCollider());
		}

		private IEnumerator SetMesh()
		{
			var mapParameters = _mapGenerator.parameters;
			mapParameters.noisePosition = new Vector2(transform.position.x, transform.position.z);
			var noiseGen = new ThreadedNoise {parameters = mapParameters};
			noiseGen.Start();
			yield return StartCoroutine(noiseGen.WaitFor());
			var noiseMap = noiseGen.retNoiseMap;

			var mapMeshData = new MapMeshData(noiseMap, _mapGenerator.parameters.baseHeight,
				_mapGenerator.parameters.heightCurve, _mapGenerator.parameters.levelOfDetail);
			var generator = new ThreatedMeshGenerator {mapMeshData = mapMeshData};
			generator.Start();
			yield return StartCoroutine(generator.WaitFor());
			var meshData = generator.meshData;

			var mesh = meshData.CreateMesh();
			mesh.name = "Map Mesh";
			_meshFilter.mesh = mesh;
			var meshBounds = mesh.bounds;
			var center = transform.position +
			             new Vector3(_mapGenerator.parameters.size / 2, 0, _mapGenerator.parameters.size / 2);
			bounds = new Bounds(center, meshBounds.size);

			isMeshReady = true;
		}

		//Todo: Gerar o noiseMap somente para os valores utilizados
		private IEnumerator SetCollider()
		{
			var mapParameters = _mapGenerator.parameters;
			mapParameters.noisePosition = new Vector2(transform.position.x, transform.position.z);
			mapParameters.levelOfDetail = 6;
			var noiseGen = new ThreadedNoise {parameters = mapParameters};
			noiseGen.Start();
			yield return StartCoroutine(noiseGen.WaitFor());
			var noiseMap = noiseGen.retNoiseMap;

			var mapMeshData = new MapMeshData(noiseMap, _mapGenerator.parameters.baseHeight,
				_mapGenerator.parameters.heightCurve, _mapGenerator.parameters.levelOfDetail);
			var generator = new ThreatedMeshGenerator {mapMeshData = mapMeshData};
			generator.Start();
			yield return StartCoroutine(generator.WaitFor());
			var meshData = generator.meshData;

			var mesh = meshData.CreateMesh();
			mesh.name = "Collider Mesh";
			_meshCollider.sharedMesh = mesh;
			_meshCollider.material = _mapGenerator.defaultPhysicMaterial;

			isColliderReady = true;
		}

		private IEnumerator SetTexture()
		{
			var mapParameters = _mapGenerator.parameters;
			mapParameters.noisePosition = new Vector2(transform.position.x, transform.position.z);
			var noiseGen = new ThreadedNoise
			{
				parameters = mapParameters,
				isTexture = true
			};
			noiseGen.Start();
			yield return StartCoroutine(noiseGen.WaitFor());
			var noiseMap = noiseGen.retNoiseMap;

			var textureParameters = new TextureParameters(noiseMap, _mapGenerator.parameters);
			var generator = new ThreadedTextureGenerator {parameters = textureParameters};
			generator.Start();
			yield return StartCoroutine(generator.WaitFor());
			var colorMap = generator.colorMap;

			var textureSize = noiseMap.GetLength(0);
			var texture2D = new Texture2D(textureSize, textureSize);
			texture2D.SetPixels(colorMap);
			texture2D.filterMode = FilterMode.Point;
			texture2D.Apply();
			var material = new Material(Shader.Find("Standard"))
			{
				mainTexture = texture2D,
				name = "Map Material"
			};
			
//			Texture normalMap = TextureGenerator.NormalMap(texture2D, 0.5f);
//			material.SetTexture("_BumpMap", normalMap);
			
			_meshRenderer.material = material;

			isTextureReady = true;
		}
	} //MapMesh
} //MkGames