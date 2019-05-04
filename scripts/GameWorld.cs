using Godot;
using System;

public class GameWorld : Spatial
{
    PlayerCharacter _character;
    Control _piehud;
    WorldMap _map;
    Timer _timer;

    void OnPlayerCharacterActivatePie() {
        _piehud.Visible = true;
    }

    void OnPieReleased(Vector2 position, String slice, int tier) {
        var camera = GetViewport().GetCamera();
        var from = camera.ProjectRayOrigin(position);
        var to = from + camera.ProjectRayNormal(position) * 1000;
        var intersection = Plane.PlaneXZ.IntersectRay(from, to);
        _character.LaunchPotion(intersection, slice, tier);
    }

    void CheckIfNeedChunk() {
        var camera = GetViewport().GetCamera();
        var half_size = camera.Size / 2.0f;
        var camera_translation = camera.GlobalTransform.origin;
        var center = new Vector2(camera_translation.x + half_size, camera_translation.z + half_size);
        var chunk_pos = (center / WorldMap.UNITS_PER_CHUNK).Floor();
        _map.LoadChunk(chunk_pos);
    }

    public override void _Ready() {
        _character = (PlayerCharacter)GetNode("PlayerCharacter");
        _piehud = (Control)GetNode("CanvasLayer/PieHUD");
        _map = new WorldMap((Image)ResourceLoader.Load("res://world-1544060774196.png"), this);
        _timer = new Timer();

        // character.translate(Vector3(2100 * UNITS_PER_PIXEL, 0, 546 * UNITS_PER_PIXEL))
        CheckIfNeedChunk();
        AddChild(_timer);
        _timer.WaitTime = 0.5f;
        _timer.ProcessMode = Timer.TimerProcessMode.Physics;
        _timer.Connect("timeout", this, "CheckIfNeedChunk");
        _timer.Start();
    }
}
