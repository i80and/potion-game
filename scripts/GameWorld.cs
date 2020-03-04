using Godot;
using System;

public class GameWorld : Spatial
{
    PlayerCharacter? _character;
    Control? _piehud;

    void OnPlayerCharacterActivatePie()
    {
        _piehud!.Visible = true;
    }

    void OnPieReleased(Vector2 position, String slice, int tier)
    {
        var camera = GetViewport().GetCamera();
        var from = camera.ProjectRayOrigin(position);
        var to = from + camera.ProjectRayNormal(position) * 1000;
        var intersection = Plane.PlaneXZ.IntersectRay(from, to);
        if (intersection is Vector3 vec)
        {
            _character!.LaunchPotion(vec, slice, tier);
        }
    }

    public override void _Ready()
    {
        _character = (PlayerCharacter)GetNode("PlayerCharacter");
        _piehud = (Control)GetNode("CanvasLayer/PieHUD");

        // character.translate(Vector3(2100 * UNITS_PER_PIXEL, 0, 546 * UNITS_PER_PIXEL))
    }
}
