using Godot;
using System;

public class PlayerCharacter : KinematicBody
{
    [Signal]
    delegate void activate_pie();

    [Export]
    int speed = 20;

    static readonly PackedScene THROWN_POTION = ResourceLoader.Load("res://scenes/ThrownPotion.tscn") as PackedScene;
    CollisionShape _collisionShape;

    Vector3 GetInput() {
        if (Input.IsActionJustPressed("activate_pie")) {
            EmitSignal("activate_pie");
        }

        // Detect up/down/left/right keystate and only move when pressed
        var velocity = new Vector3();
        if (Input.IsActionPressed("ui_right")) {
            velocity.z -= 0.5f;
            velocity.x += 1f;
        }
        if (Input.IsActionPressed("ui_left")) {
            velocity.z += 0.5f;
            velocity.x -= 1f;
        }

        if (Input.IsActionPressed("ui_down")) {
            velocity.z += 1f;
            velocity.x += 0.5f;
        }
        if (Input.IsActionPressed("ui_up")) {
            velocity.z -= 1f;
            velocity.x -= 0.5f;
        }

        return velocity.Normalized() * speed;
    }

    public override void _PhysicsProcess(float delta) {
        var velocity = GetInput();
        MoveAndSlide(velocity);
        if (Translation.y < 0.1f) {
            Translation.Set(Translation.x, 0.1f, Translation.z);
        }
    }

    public void LaunchPotion(Vector3 target, String slice, int tier) {
        var potion = (ThrownPotion)THROWN_POTION.Instance();
        GetParent().AddChild(potion);
        potion.GlobalTranslate(_collisionShape.GlobalTransform.origin);
        potion.Translate(new Vector3(0, 2, 0));
        potion.Launch(target);
    }

    public override void _Ready() {
        _collisionShape = (CollisionShape)GetNode("CollisionShape");
    }
}
