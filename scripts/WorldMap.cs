using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

struct BlockType {
    public static readonly int[] SIZE = new int[] {
        2048 / WorldMap.CHUNK_PIXELS,
        2048 / WorldMap.CHUNK_PIXELS};

    public string Description { get; }
    public Image Albedo { get; }
    public bool HaveTree { get; }

    public BlockType(string description, Color color, bool haveTree=false) {
        this.Description = description;
        this.Albedo = new Image();
        this.Albedo.Create(SIZE[0], SIZE[1], false, Image.Format.Rgb8);
        this.Albedo.Lock();
        this.Albedo.Fill(color);
        this.Albedo.Unlock();
        this.HaveTree = haveTree;
    }
}

class Chunk : Spatial {
    static readonly Spatial TREE1_SCENE = (Spatial)((PackedScene)ResourceLoader.Load("res://scenes/Tree1.tscn")).Instance();
    static readonly Spatial TREE2_SCENE = (Spatial)((PackedScene)ResourceLoader.Load("res://scenes/Tree2.tscn")).Instance();
    static readonly Spatial MAPLE_SCENE = (Spatial)((PackedScene)ResourceLoader.Load("res://scenes/Maple.tscn")).Instance();
    static readonly Spatial ROCK1_SCENE = (Spatial)((PackedScene)ResourceLoader.Load("res://scenes/LowPolyRock1.tscn")).Instance();
    static readonly Spatial ROCK2_SCENE = (Spatial)((PackedScene)ResourceLoader.Load("res://scenes/LowPolyRock2.tscn")).Instance();
    static readonly Spatial CHARACTER_SCENE = (Spatial)((PackedScene)ResourceLoader.Load("res://scenes/PlayerCharacter.tscn")).Instance();

    static readonly Spatial[] TREES = {
        TREE1_SCENE,
        TREE2_SCENE,
        MAPLE_SCENE
    };

    static readonly BlockType FALLBACK_BLOCK_TYPE = new BlockType("Space", new Color(0, 0, 0, 1));
    static readonly Dictionary<(byte, byte, byte), BlockType> BLOCK_TYPES = new Dictionary<(byte, byte, byte), BlockType> {
        {(0, 255, 0), new BlockType("Grass", Color.Color8(0, 255, 0, 255))},
        {(0, 200, 0), new BlockType("Tree", Color.Color8(0, 200, 0, 255), true)},
        {(157, 209, 137), new BlockType("Tree", Color.Color8(157, 209, 137, 255), true)},
        {(232, 180, 107), new BlockType("", Color.Color8(232, 180, 107, 255))},
        {(255, 255, 0), new BlockType("", Color.Color8(255, 255, 0, 255))},
        {(50, 50, 255), new BlockType("", Color.Color8(50, 50, 255, 255))},
        {(152, 255, 160), new BlockType("", Color.Color8(152, 255, 160, 255))},
        {(52, 255, 68), new BlockType("", Color.Color8(52, 255, 68, 255))},
        {(0, 0, 255), new BlockType("Ocean", Color.Color8(0, 0, 255, 255))},
        {(255, 255, 255), new BlockType("Snow", Color.Color8(255, 255, 255, 255))}
    };

    public Vector2 ChunkPos { get; }

    public Chunk(Vector2 chunkPos, Image image, bool compress)
    {
        ChunkPos = chunkPos;

        var rng = new Random(GD.Hash(chunkPos));

        var groundMaterial = new SpatialMaterial();
        var groundMesh = new QuadMesh();
        var ground = new MeshInstance();
        ground.Rotate(new Vector3(1, 0, 0), Mathf.Pi / -2.0f);
        ground.Translate(
            new Vector3(
                WorldMap.UNITS_PER_CHUNK / 2,
                -WorldMap.UNITS_PER_CHUNK / 2,
                0));
        ground.Mesh = groundMesh;
        groundMesh.Material = groundMaterial;
        groundMesh.Size = new Vector2(
            WorldMap.UNITS_PER_CHUNK,
            WorldMap.UNITS_PER_CHUNK);

        var groundImage = new Image();
        groundImage.Create(2048, 2048, false, Image.Format.Rgb8);
        groundImage.Lock();

        var data = image.GetData();
        int i = 0;
        int offset = 0;
        for (int y = 0; y < WorldMap.CHUNK_PIXELS; y += 1) {
            for (int x = 0; x < WorldMap.CHUNK_PIXELS; x += 1) {
                byte r = data[offset];
                byte g = data[offset + 1];
                byte b = data[offset + 2];
                BlockType blockType;
                if (BLOCK_TYPES.TryGetValue((r, g, b), out blockType))
                {
                    if (blockType.HaveTree) {
                        var scene = TREES[rng.Next(0, TREES.Length)];
                        var tree = AddObject(scene, new Vector2(x, y), rng);
                        AddChild(tree);
                    }
                }
                else
                {
                    blockType = FALLBACK_BLOCK_TYPE;
                }

                groundImage.BlitRect(
                    blockType.Albedo,
                    blockType.Albedo.GetUsedRect(),
                    new Vector2(x * BlockType.SIZE[0], y * BlockType.SIZE[1]));
                offset += 3;
                i += 1;
            }
        }

        groundImage.Unlock();
        if (compress)
        {
            groundImage.Compress(Image.CompressMode.S3tc, Image.CompressSource.Srgb, 0.9f);
        }
        var groundTexture = new ImageTexture();
        groundTexture.CreateFromImage(groundImage);
        groundMaterial.AlbedoTexture = groundTexture;

        AddChild(ground);
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

    Spatial AddObject(Spatial scene, Vector2 pos, Random rng)
    {
        var node = (Spatial)scene.Duplicate(0);
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
            var rock = (Spatial)((rng.NextDouble() > 0.5) ? ROCK1_SCENE.Duplicate() : ROCK2_SCENE.Duplicate());
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

class Worker : Node
{
    Image _image;
    int pending = 0;

    public Worker(Image image)
    {
        _image = image;
    }

    /// <summary>
    /// Main function for the chunk load task.
    /// </summary>
    public void WorkerLoadChunk(Vector2[] candidate_positions, bool compress)
    {
        Interlocked.Add(ref pending, candidate_positions.Length);

        Task.Run(() => {
            foreach (var candidate in candidate_positions) {
                var rect = new Rect2(
                    candidate.x * WorldMap.CHUNK_PIXELS,
                    candidate.y * WorldMap.CHUNK_PIXELS,
                    WorldMap.CHUNK_PIXELS,
                    WorldMap.CHUNK_PIXELS);
                var chunk = new Chunk(candidate, _image.GetRect(rect), compress);
                chunk.Translate(new Vector3(
                    candidate.x * WorldMap.UNITS_PER_CHUNK,
                    0,
                    candidate.y * WorldMap.UNITS_PER_CHUNK));
                GetParent().CallDeferred("OnChunkCreated", chunk);
                Interlocked.Decrement(ref pending);
            }
        });
    }

    public bool IsCompleted {
        get {
            return pending == 0;
        }
    }
}

public class WorldMap : Node
{
    //   4 units per map pixel
    public const int UNITS_PER_PIXEL = 4;
    // * 16 map pixels per chunk
    public const int CHUNK_PIXELS = 16;
    // = 160 units per chunk
    public const int UNITS_PER_CHUNK = CHUNK_PIXELS * UNITS_PER_PIXEL;

    static readonly Vector2[] DIRECTIONS = {
        new Vector2(0, 0),
        new Vector2(-1, 1), new Vector2(0, 1), new Vector2(1, 1),
        new Vector2(-1, 0), new Vector2(1, 0),
        new Vector2(-1, -1), new Vector2(0, -1), new Vector2(1, -1),
    };

    readonly Image _image;
    readonly System.Collections.Generic.Dictionary<Vector2, Chunk> _chunks = new Dictionary<Vector2, Chunk>();
    Worker _worker;

    public WorldMap() {}

    public WorldMap(Image image)
    {
        _image = image;

        Debug.Assert(_image.GetFormat() == Image.Format.Rgb8);
        Debug.Assert(_image.GetWidth() % CHUNK_PIXELS == 0);
        Debug.Assert(_image.GetHeight() % CHUNK_PIXELS == 0);

        _worker = new Worker(_image);
        AddChild(_worker);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {}

    void GarbageCollectChunks(Vector2 camera_chunk_pos)
    {
        var removeKeys = new List<Vector2>(_chunks.Count);
        foreach (var chunk in _chunks) {
            var chunk_pos = chunk.Key;
            if (Math.Abs(chunk_pos.x - camera_chunk_pos.x) > 1 ||
                Math.Abs(chunk_pos.y - camera_chunk_pos.y) > 1) {
                Chunk node = (Chunk)chunk.Value;
                node.QueueFree();
                removeKeys.Add(chunk_pos);
            }
        }

        foreach (var chunk_pos in removeKeys)
        {
            _chunks.Remove(chunk_pos);
        }
    }

    /// <summary>
    /// Load the given chunk position into th game world.
    /// </summary>
    public void LoadChunk(Vector2 chunk_pos, bool urgent = false)
    {
        if (!_worker.IsCompleted)
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
        if (needed.Length == 0)
        {
            return;
        }

        // Don't compress if we're in an urgent state
        bool haveCompression = urgent ? false : VisualServer.HasOsFeature("s3tc");

        _worker.WorkerLoadChunk(needed, haveCompression);
    }

    void OnChunkCreated(Chunk chunk)
    {
        var t = OS.GetTicksMsec();
        _chunks[chunk.ChunkPos] = chunk;
        AddChild(chunk);
        GD.Print(OS.GetTicksMsec() - t);
    }
}
