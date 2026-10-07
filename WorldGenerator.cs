using System.Collections.Generic;
using UnityEngine;

public enum Biome { Desert, Forest, Snow, Wasteland }

public class WorldGenerator : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public Material terrainMaterial;

    [Header("Chunks")]
    public int chunkSize = 32;       // tamanho do chunk em metros, obs ver isso no menu 2k.
    public int viewDistance = 4;     // raio de chunks ao redor do jogador(podendo variar no final)

    [Header("Ruído de altura")]
    public int seed = 12345;
    public float noiseScale = 120f;
    public int octaves = 4;
    [Range(0f, 1f)] public float persistence = 0.5f;
    public float lacunarity = 2f;
    public float heightMultiplier = 40f;
    public float heightExponent = 1.4f;  // >1 deixa vales mais planos e montanhas mais altas(se gera sozinho pq tem isso ak ?)

    [Header("Biomas")]
    public float biomeScale = 400f;

    Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    Vector2Int lastPlayerChunk = new Vector2Int(int.MaxValue, int.MaxValue);
    float offsetX, offsetZ;

    void Awake()
    {
        var rng = new System.Random(seed);
        offsetX = rng.Next(-10000, 10000);
        offsetZ = rng.Next(-10000, 10000);
    }

    void Start()
    {
        UpdateChunks(); // gera o terreno antes do jogador cair( se cair fudeu 3 cod pro lixo )
    }

    void Update()
    {
        Vector2Int current = WorldToChunk(player.position);
        if (current != lastPlayerChunk)
        {
            lastPlayerChunk = current;
            UpdateChunks();
        }
    }

    Vector2Int WorldToChunk(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkSize),
                              Mathf.FloorToInt(pos.z / chunkSize));
    }

    void UpdateChunks()
    {
        Vector2Int center = WorldToChunk(player.position);

        // Cria chunks que faltam(ou não)
        for (int x = -viewDistance; x <= viewDistance; x++)
        for (int z = -viewDistance; z <= viewDistance; z++)
        {
            var coord = new Vector2Int(center.x + x, center.y + z);
            if (!chunks.ContainsKey(coord)) CreateChunk(coord);
        }

        // Remove chunks distantes(igual do mine ?)
        var toRemove = new List<Vector2Int>();
        foreach (var kv in chunks)
        {
            if (Mathf.Abs(kv.Key.x - center.x) > viewDistance + 1 ||
                Mathf.Abs(kv.Key.y - center.y) > viewDistance + 1)
                toRemove.Add(kv.Key);
        }
        foreach (var coord in toRemove)
        {
            Destroy(chunks[coord].gameObject);
            chunks.Remove(coord);
        }
    }

    void CreateChunk(Vector2Int coord)
    {
        var go = new GameObject($"Chunk {coord.x},{coord.y}");
        go.transform.parent = transform;
        go.transform.position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
        var chunk = go.AddComponent<Chunk>();
        chunk.Build(coord, this);
        chunks.Add(coord, chunk);
    }

    // ---------- ALTURA (fractal Perlin noise) ----------(q nunca soube oque faz ak)
    public float GetHeight(float wx, float wz)
    {
        float amplitude = 1f, frequency = 1f, value = 0f, maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sx = (wx + offsetX) / noiseScale * frequency;
            float sz = (wz + offsetZ) / noiseScale * frequency;
            value += Mathf.PerlinNoise(sx, sz) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        float n = value / maxValue;                       // 0..1...?
        return Mathf.Pow(n, heightExponent) * heightMultiplier;
    }

    // ---------- BIOMA ---------- 2k vc faz 
    public Biome GetBiome(float wx, float wz)
    {
        float b = Mathf.PerlinNoise((wx + offsetX + 5000f) / biomeScale,
                                    (wz + offsetZ + 5000f) / biomeScale);
        if (b < 0.30f) return Biome.Desert;
        if (b < 0.60f) return Biome.Forest;
        if (b < 0.80f) return Biome.Snow;
        return Biome.Wasteland;
    }

    public Color GetBiomeColor(Biome biome)
    {
        switch (biome)
        {
            case Biome.Desert:    return new Color(0.85f, 0.75f, 0.45f);
            case Biome.Forest:    return new Color(0.25f, 0.55f, 0.22f);
            case Biome.Snow:      return new Color(0.95f, 0.97f, 1.00f);
            default:              return new Color(0.45f, 0.38f, 0.33f);
        }
    }
}