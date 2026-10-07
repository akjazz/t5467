using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public void Build(Vector2Int coord, WorldGenerator world)
    {
        int size = world.chunkSize;
        int verts = size + 1;                       // vértices por lado ( onde ?)

        var vertices  = new Vector3[verts * verts];
        var colors    = new Color[verts * verts];
        var triangles = new int[size * size * 6];
        int t = 0;

        for (int z = 0; z < verts; z++)
        for (int x = 0; x < verts; x++)
        {
            int i = z * verts + x;

            // coordenada no mundo (garante que chunks vizinhos "encaixem") ou não tmb ak 
            float wx = coord.x * size + x;
            float wz = coord.y * size + z;

            vertices[i] = new Vector3(x, world.GetHeight(wx, wz), z);
            colors[i]   = world.GetBiomeColor(world.GetBiome(wx, wz));

            if (x < size && z < size)
            {
                triangles[t++] = i;
                triangles[t++] = i + verts;
                triangles[t++] = i + 1;

                triangles[t++] = i + 1;
                triangles[t++] = i + verts;
                triangles[t++] = i + verts + 1;
            }
        }

        var mesh = new Mesh();
        mesh.name = "ChunkMesh";
        mesh.vertices  = vertices;
        mesh.triangles = triangles;
        mesh.colors    = colors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = world.terrainMaterial;

        SpawnVegetation(coord, world, vertices);
    }

    void SpawnVegetation(Vector2Int coord, WorldGenerator world, Vector3[] vertices)
    {
        // Passo 6 (árvores) entra aqui aqui voce faz 2k to sem saco ja.
    }
}