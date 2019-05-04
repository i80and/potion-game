using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

class Chunk : Spatial {
    static readonly PackedScene TREE1_SCENE = ResourceLoader.Load("res://scenes/Tree1.tscn") as PackedScene;
    static readonly PackedScene TREE2_SCENE = ResourceLoader.Load("res://scenes/Tree2.tscn") as PackedScene;
    static readonly PackedScene MAPLE_SCENE = ResourceLoader.Load("res://scenes/Maple.tscn") as PackedScene;
    static readonly PackedScene ROCK1_SCENE = ResourceLoader.Load("res://scenes/LowPolyRock1.tscn") as PackedScene;
    static readonly PackedScene ROCK2_SCENE = ResourceLoader.Load("res://scenes/LowPolyRock2.tscn") as PackedScene;
    static readonly PackedScene CHARACTER_SCENE = ResourceLoader.Load("res://scenes/PlayerCharacter.tscn") as PackedScene;

    static readonly PackedScene[] TREES = {
        TREE1_SCENE,
        TREE2_SCENE,
        MAPLE_SCENE
    };

    public Vector2 ChunkPos { get; }

    public Chunk(Vector2 chunkPos, Image image)
    {
        ChunkPos = chunkPos;

        var rng = new Random(GD.Hash(chunkPos));

        var material = new SpatialMaterial();
        material.VertexColorUseAsAlbedo = true;
        var mesh = new QuadMesh();
        mesh.Material = material;
        mesh.Size = new Vector2(WorldMap.UNITS_PER_PIXEL, WorldMap.UNITS_PER_PIXEL);
        var groundMultimesh = new MultiMesh();
        int instanceCount = WorldMap.CHUNK_PIXELS * WorldMap.CHUNK_PIXELS;
        groundMultimesh.ColorFormat = MultiMesh.ColorFormatEnum.Color8bit;
        groundMultimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
        groundMultimesh.InstanceCount = instanceCount;
        groundMultimesh.Mesh = mesh;

        var data = image.GetData();
        int i = 0;
        int offset = 0;
        for (int y = 0; y < WorldMap.CHUNK_PIXELS; y += 1) {
            for (int x = 0; x < WorldMap.CHUNK_PIXELS; x += 1) {
                byte r = data[offset];
                byte g = data[offset + 1];
                byte b = data[offset + 2];
                if (g == 200 || g == 209) {
                    var scene = TREES[rng.Next(0, TREES.Length)];
                    AddChild(AddObject(scene, new Vector2(x, y), rng));
                }

                var color = Color.Color8(r, g, b, 255);
                groundMultimesh.SetInstanceColor(i, color);
                var transform = new Transform().Rotated(new Vector3(1, 0, 0), Mathf.Pi / -2.0f).Translated(
                    new Vector3(x * WorldMap.UNITS_PER_PIXEL, -y * WorldMap.UNITS_PER_PIXEL, 0)
                );
                groundMultimesh.SetInstanceTransform(i, transform);
                offset += 3;
                i += 1;
            }
        }

        var groundMeshInstance = new MultiMeshInstance();
        groundMeshInstance.Multimesh = groundMultimesh;
        AddChild(groundMeshInstance);
        var collisionBody = new StaticBody();
        var collisionShape = new CollisionShape();
        var shape = new BoxShape();
        shape.Extents = new Vector3(WorldMap.UNITS_PER_CHUNK, 1, WorldMap.UNITS_PER_CHUNK);
        collisionShape.Shape = shape;
        collisionBody.Translate(new Vector3(0, -1, 0));
        collisionBody.AddChild(collisionShape);
        AddChild(collisionBody);

        PlaceCosmetics(rng);
    }

    Node AddObject(PackedScene scene, Vector2 pos, Random rng)
    {
        var node = (Spatial)scene.Instance();
        node.Translation = new Vector3(pos.x, 0, pos.y) * WorldMap.UNITS_PER_PIXEL;
        node.Rotation = new Vector3(0, RandomFloat(rng, 0, 2.0f * Mathf.Pi), 0);
        float scale_y = RandomFloat(rng, 0.8f, 1.2f);
        float scale_radius = RandomFloat(rng, 0.8f, 1.2f);
        node.Scale = new Vector3(scale_radius, scale_y, scale_radius);
        return node;
    }

    void PlaceCosmetics(Random rng)
    {
        int nRocks = rng.Next(0, 50);
        for (int i = 0; i < nRocks; i += 1)
        {
            var rock = (Spatial)((rng.NextDouble() > 0.5) ? ROCK1_SCENE.Instance() : ROCK2_SCENE.Instance());
            float x = RandomFloat(rng, 0, WorldMap.UNITS_PER_CHUNK);
            float y = RandomFloat(rng, 0, WorldMap.UNITS_PER_CHUNK);
            rock.Translation = new Vector3(x, 0, y);
            rock.Rotation = new Vector3(0, RandomFloat(rng, 0, 2.0f * Mathf.Pi), 0);
            AddChild(rock);
        }
    }

    static float RandomFloat(Random rng, float min, float max) {
        return (float)rng.NextDouble() * (max - min) + min;
    }
}

public class WorldMap : Spatial
{
    //   10 units per map pixel
    public const int UNITS_PER_PIXEL = 5;
    // * 16 map pixels per chunk
    public const int CHUNK_PIXELS = 16;
    // = 160 units per chunk
    public const int UNITS_PER_CHUNK = CHUNK_PIXELS * UNITS_PER_PIXEL;

    static readonly Vector2[] DIRECTIONS = {
        new Vector2(-1, 1), new Vector2(0, 1), new Vector2(1, 1),
        new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 0),
        new Vector2(-1, -1), new Vector2(0, -1), new Vector2(1, -1),
    };

    readonly Image _image;
    readonly System.Collections.Generic.Dictionary<Vector2, Chunk> _chunks = new Dictionary<Vector2, Chunk>();
    readonly Node _node;
    Task<Chunk[]> _worker;

    public WorldMap() {}

    public WorldMap(Image image, Node node)
    {
        _node = node;
        _image = image;
        Debug.Assert(_image.GetFormat() == Image.Format.Rg8);
        Debug.Assert(_image.GetWidth() % CHUNK_PIXELS == 0);
        Debug.Assert(_image.GetHeight() % CHUNK_PIXELS == 0);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {}

    void GarbageCollectChunks(Vector2 camera_chunk_pos)
    {
        foreach (var chunk_pos in _chunks.Keys) {
            if (Math.Abs(chunk_pos.x - camera_chunk_pos.x) > 1 ||
                Math.Abs(chunk_pos.y - camera_chunk_pos.y) > 1) {
                Node node = _chunks[chunk_pos];
                _chunks.Remove(chunk_pos);
                _node.RemoveChild(node);
            }
        }
    }

    Chunk[] WorkerLoadChunk(Vector2[] candidate_positions)
    {
        var chunks = new Chunk[candidate_positions.Length];
        int i = 0;
        foreach (var candidate in candidate_positions) {
            var rect = new Rect2(candidate.x * CHUNK_PIXELS, candidate.y * CHUNK_PIXELS, CHUNK_PIXELS, CHUNK_PIXELS);
            var chunk = new Chunk(candidate, _image.GetRect(rect));
            chunk.Translate(new Vector3(
                candidate.x * UNITS_PER_CHUNK,
                0,
                candidate.y * UNITS_PER_CHUNK));
            chunks[i++] = chunk;
        }

        CallDeferred("WorkerFinished");
        return chunks;
    }

    void WorkerFinished()
    {
        var chunks = _worker.Result;
        foreach (var chunk in chunks) {
            _chunks[chunk.ChunkPos] = chunk;
            _node.AddChild(chunk);
        }
        _worker = null;
    }

    public void LoadChunk(Vector2 chunk_pos)
    {
        if (_worker != null && !_worker.IsCompleted)
        {
            return;
        }

        if (_chunks.Count > 12)
        {
            GarbageCollectChunks(chunk_pos);
        }

        var needed = new Vector2[DIRECTIONS.Length];
        int i = 0;
        foreach (var dir in DIRECTIONS)
        {
            var candidate = chunk_pos + dir;
            if (candidate.x < 0 || candidate.y < 0) {
                continue;
            }

            if (_chunks.ContainsKey(candidate)) {
                continue;
            }

            needed[i++] = candidate;
        }

        Array.Resize(ref needed, i);
        if (needed.Length == 0) {
            return;
        }

        _worker = Task.Run<Chunk[]>(() => {
            return this.WorkerLoadChunk(needed);
        });
    }
}
