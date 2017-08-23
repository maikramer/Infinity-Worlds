using UnityEngine;

namespace MkGames
{
	public class MeshGenerator : MonoBehaviour
	{
		public static MeshData GenerateMeshData(float[,] heightMap, MapParameters mapParameters)
		{
			var size = heightMap.GetLength(0);

			var meshSimplificationIncrement = mapParameters.levelOfDetail == 0 ? 1 : mapParameters.levelOfDetail * 2;
			var verticesPerLine = (size - 1) / meshSimplificationIncrement + 1;

			var meshData = new MeshData(verticesPerLine);

			var vertexIndex = 0;

			for (var x = 0; x < size; x += meshSimplificationIncrement)
			for (var y = 0; y < size; y += meshSimplificationIncrement)
			{
				var vertHeight = mapParameters.heightCurve.Evaluate(heightMap[x, y]) * mapParameters.baseHeight *
				                 mapParameters.mapScale;
				var vert = new Vector3(x * mapParameters.mapScale, vertHeight, y * mapParameters.mapScale);
				meshData.vertices[vertexIndex] = vert;

				meshData.uvs[vertexIndex] =
					new Vector2(x / (float) size, y / (float) size);

				if (x < size - 1 && y < size - 1)
				{
					meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
					meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}

			meshData.CalculateNormals();

			return meshData;
		}

		public static Mesh CreatePlaneMesh(float width, float height)
		{
			var planeMesh = new Mesh
			{
				name = "Scripted_Plane_New_Mesh",
				vertices = new[]
				{
					new Vector3(0, 0, 0),
					new Vector3(0, 0, height),
					new Vector3(width, 0, height),
					new Vector3(width, 0, 0)
				},
				uv = new[]
				{
					new Vector2(0, 0),
					new Vector2(0, 1),
					new Vector2(1, 1),
					new Vector2(1, 0)
				},
				triangles = new[] {0, 1, 2, 0, 2, 3}
			};

			planeMesh.RecalculateNormals();

			return planeMesh;
		}

		public static GameObject CreateMeshGameObject(Mesh mesh, Texture2D texture2D)
		{
			if (!mesh)
				new UnityException("Tentando criar um GameObject de uma mesh nula");

			var meshGameObject = new GameObject();
			meshGameObject.AddComponent<MeshFilter>();
			meshGameObject.AddComponent<MeshRenderer>();
			meshGameObject.name = "Map Chunk " + meshGameObject.GetInstanceID();
			meshGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
			Material material;

			if (!texture2D)
				material = new Material(Shader.Find("Diffuse Vertex Color"));
			else
			{
				material = new Material(Shader.Find("Standard")) {mainTexture = texture2D};
				material.SetFloat("_Glossiness", 0);
			}


			meshGameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
			return meshGameObject;
		}
	} // Fim de MeshGenerator

	public class MeshData
	{
		private int triangleIndex;
		public int[] triangles;
		public Vector2[] uvs;
		public Vector3[] vertices;
		public Vector3[] normals;


		public MeshData(int size)
		{
			vertices = new Vector3[size * size];
			normals = new Vector3[vertices.Length];
			triangles = new int[(size - 1) * (size - 1) * 6];
			uvs = new Vector2[size * size];
		}

		public void AddTriangle(int a, int b, int c)
		{
			triangles[triangleIndex] = a;
			triangles[triangleIndex + 1] = b;
			triangles[triangleIndex + 2] = c;

			triangleIndex += 3;
		}

		public void CalculateTriangleNormals(int a, int b, int c)
		{
			var vertexA = vertices[a];
			var vertexB = vertices[b];
			var vertexC = vertices[c];
			var vectorAB = vertexB - vertexA;
			var vectorAC = vertexC - vertexA;
			var triangleNormal = Vector3.Cross(vectorAB, vectorAC).normalized;
			normals[a] = triangleNormal;
			normals[b] = triangleNormal;
			normals[c] = triangleNormal;
		}

		public void CalculateNormals()
		{
			var triangleCount = triangles.Length / 3;
			for (int i = 0; i < triangleCount; i++)
			{
				var vertexIndex = i * 3;
				var vertexA = vertices[triangles[vertexIndex]];
				var vertexB = vertices[triangles[vertexIndex + 1]];
				var vertexC = vertices[triangles[vertexIndex + 2]];
				var vectorAB = vertexB - vertexA;
				var vectorAC = vertexC - vertexA;
				var triangleNormal = Vector3.Cross(vectorAB, vectorAC).normalized;
				normals[triangles[vertexIndex]] = triangleNormal;
				normals[triangles[vertexIndex + 1]] = triangleNormal;
				normals[triangles[vertexIndex + 2]] = triangleNormal;
			}
		}

		public Mesh CreateMesh()
		{
			var mesh = new Mesh
			{
				vertices = vertices,
				triangles = triangles,
				uv = uvs,
				normals = normals
			};

			return mesh;
		}
	}
} // Fim do namespace