using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Events;

namespace MkGames
{
	public class FullMapGenerator : MonoBehaviour
	{
		public UnityAction<int> ProgressBarAddAction;
		private readonly string MapParentName = MapGenerator.MapParentName;
		private readonly string MapTag = MapGenerator.MapTag;

		private MapParameters _mapParameters;
		private FullMapParameters _fullMapParameters;
		private bool GeneratorIsSet;
		private int _fullMapSize;
		private int _textureMapSize;
		private float[,] _fullHeightMap;
		private float[,] _textureHeightMap;
		private GameObject _mapParent;
		private MapGenerator _mapGenerator;
		private Texture2D _texture2D;

		public void SetGenerator(MapParameters mapParameters, FullMapParameters fullMapParameters)
		{
			_mapParameters = mapParameters;
			_fullMapParameters = fullMapParameters;
			
			_mapParent = GameObject.Find(MapParentName);
			_mapGenerator = FindObjectOfType<MapGenerator>();
			_fullMapSize = _fullMapParameters.fullMapSize * (_mapParameters.size - 1) + 1;
			
			GeneratorIsSet = true;
		}

		public void StartGeneration()
		{	
			ValidateLOD();
			
			if (_mapParameters.size / (_mapParameters.levelOfDetail + 1) > 255)
			{
				Debug.LogError("O numero de vertices final supera o maximo de 65000 vertices");
				_mapGenerator.EndOfFullMapGeneration();
				return;
			}
			
			
			if (!GeneratorIsSet)
			{
				Debug.LogError("Chame SetGenerator antes de comecar a geracao");
				return;
			}

			var noiseGen = new Noise();
			noiseGen.OnNoiseIsReady.AddListener(OnNoiseMapGenerated);
			noiseGen.ProgressBarAddAction = ProgressBarAddAction;

			if (Application.isEditor)
				EditorCoroutine.Start(noiseGen.IEGenerateFastNoiseMap(_mapParameters, _fullMapSize,
					_mapParameters.noisePosition));
			else
				StartCoroutine(noiseGen.IEGenerateFastNoiseMap(_mapParameters, _fullMapSize, _mapParameters.noisePosition));
		}

		public void OnNoiseMapGenerated(float[,] fullHeightMap)
		{
			_textureHeightMap = fullHeightMap;
			_textureMapSize = _textureHeightMap.GetLength(0);

			for (int y = 0; y < _textureMapSize; y++)
			for (int x = 0; x < _textureMapSize; x++)
			{
				var curveX = _fullMapParameters.heightCurveX.Evaluate((float) x / _textureMapSize);
				var curveY = _fullMapParameters.heightCurveY.Evaluate((float) y / _textureMapSize);

				_textureHeightMap[x, y] = Mathf.Clamp01(_textureHeightMap[x, y] * curveX * curveY);
				_textureHeightMap[x, y] = Mathf.Clamp01(_mapParameters.heightCurve.Evaluate(_textureHeightMap[x, y]));
			}

			_fullHeightMap = Noise.SimplifyNoiseMap(_textureHeightMap, _mapParameters.textureResolutionFactor);

			if (Application.isEditor)
				EditorCoroutine.Start(GenerateFullMesh());
			else
				StartCoroutine(GenerateFullMesh());
			
			ProgressBarAddAction(10);//50
		}


		private IEnumerator GenerateFullMesh()
		{
			var meshSize = (_fullMapSize + _mapParameters.levelOfDetail) / (_mapParameters.levelOfDetail + 1);
			var meshDraft = new MeshDraft(meshSize);
			for (var x = 0; x < _fullMapSize; x += (_mapParameters.levelOfDetail + 1))
			{
				for (var y = 0; y < _fullMapSize; y += (_mapParameters.levelOfDetail + 1))
				{
					var vertHeight = _fullHeightMap[x, y] * _mapParameters.baseHeight *
					                 _mapParameters.mapScale;
					var vert = new Vector3(x * _mapParameters.mapScale, vertHeight, y * _mapParameters.mapScale);
					meshDraft.AddVertice(vert);

				}
				
				if (x % 100 == 0)
					yield return null;

				if (x % (meshSize / 3) == 0)
				{
					ProgressBarAddAction(10);//80
				}
			}

			meshDraft.AddTriangles();
			meshDraft.CalculateNormals();
			meshDraft.AddUvs();

			if (Application.isEditor)
				EditorCoroutine.Start(CreateMapSlices(meshDraft));
			else
				StartCoroutine(CreateMapSlices(meshDraft));
		}

		private IEnumerator CreateMapSlices(MeshDraft meshDraft)
		{
			var mapSlices = meshDraft.SliceMeshDraft(_fullMapParameters.fullMapSize);

			if (!_mapParameters.useMeshColor)
			{
				var textureSize = _textureHeightMap.GetLength(0);
				var colorMap = _mapGenerator.GenerateColorMap(_textureHeightMap, textureSize);
				_texture2D = TextureGenerator.TextureFromColorMap(colorMap, textureSize, textureSize);
				_texture2D.filterMode = FilterMode.Point;
			}

			foreach (var slice in mapSlices)
			{
				var mesh = slice.ToMesh();

				mesh.name = "Map Mesh";

				GameObject meshGameObject;
				if (_mapParameters.useMeshColor)
				{
					mesh.colors = _mapGenerator.GenerateMeshColorMap(_fullHeightMap);
					meshGameObject = MeshGenerator.CreateMeshGameObject(mesh, null, null);
				}
				else
				{
					if (_mapParameters.useNormalMap)
					{
						Texture2D normalMap = TextureGenerator.NormalMap(_texture2D, _mapParameters.normalMapStrength);
						meshGameObject = MeshGenerator.CreateMeshGameObject(mesh, _texture2D, normalMap);
					}
					else
						meshGameObject = MeshGenerator.CreateMeshGameObject(mesh, _texture2D, null);
				}
				meshGameObject.transform.parent = _mapParent.transform;
				meshGameObject.tag = MapTag;

				if (!_mapGenerator.AddCollider) continue;
				var collider = meshGameObject.AddComponent<MeshCollider>();
				collider.material = _mapGenerator.defaultPhysicMaterial;
				ProgressBarAddAction(20 / mapSlices.Count);
				yield return null;
			}

			_mapGenerator.EndOfFullMapGeneration();
		}

		
		private void ValidateLOD()
		{
			var fullMapSize = _fullMapParameters.fullMapSize * (_mapParameters.size - 1) + 1;
			if (_mapParameters.levelOfDetail > 0 && fullMapSize % (_mapParameters.levelOfDetail + 1) != 1)
			{
				Debug.LogWarning("Level of Detail invalido, baixando para o maior nivel possivel");
				var lod = _mapParameters.levelOfDetail;
				while (fullMapSize % (lod + 1) != 1 && lod > 0)
				{
					lod--;
				}

				Debug.Log("LOD : " + lod);
				_mapParameters.levelOfDetail = lod;
			}
		}
	} //FullMapGenerator
} //MkGames