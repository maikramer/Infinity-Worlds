using System;
using UnityEngine;

namespace MkGames
{
	public class ThreatedMeshGenerator : ThreadedJob
	{
		public MapMeshData mapMeshData;
		public MeshData meshData;


		protected override void ThreadFunction()
		{
			var size = mapMeshData.heightMap.GetLength(0);
			var simplificationIncrement = mapMeshData.levelOfDetail == 0 ? 1 : mapMeshData.levelOfDetail * 2;
			var verticesPerLine = (size - 1) / simplificationIncrement + 1;

			meshData = new MeshData(verticesPerLine);
			var vertexIndex = 0;

			for (var x = 0; x < size; x += simplificationIncrement)
			for (var y = 0; y < size; y += simplificationIncrement)
			{
				var vert = new Vector3(x, mapMeshData.heightCurve.Evaluate(mapMeshData.heightMap[x, y]) * mapMeshData.baseHeight,
					y);
				meshData.vertices[vertexIndex] = vert;
				meshData.uvs[vertexIndex] = new Vector2(x / (float) size, y / (float) size);

				if (x < size - 1 && y < size - 1)
				{
					meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
					meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}
	} //ThreatedMeshGenerator

	public struct MapMeshData
	{
		public float[,] heightMap;

		public float baseHeight;

		//public float[] heightCurve; TODO: Processar curva antes de entrar no thread
		public AnimationCurve heightCurve;

		public int levelOfDetail;

		public MapMeshData(float[,] heightMap, float baseHeight, AnimationCurve heightCurve, int levelOfDetail)
		{
			if (heightMap == null) throw new ArgumentNullException(nameof(heightMap));
			if (heightCurve == null) throw new ArgumentNullException(nameof(heightCurve));
			this.heightMap = heightMap;
			this.baseHeight = baseHeight;
			this.heightCurve = heightCurve;
			this.levelOfDetail = levelOfDetail;
		}
	}
} //MkGames