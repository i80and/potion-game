using Godot;
using System;
using System.Diagnostics;

public class ThrownPotion : RigidBody
{
    const float GRAVITY = 9.80665f;

    static Vector3 SolveBallisticArcLateral(Vector3 proj_pos, float proj_speed, Vector3 target, bool force)
    {
        Debug.Assert(proj_pos != target && proj_speed > 0);
        var diff = target - proj_pos;
        var diffXZ = new Vector3(diff.x, 0, diff.z);
        var groundDist = diffXZ.Length();
        var groundDir = diffXZ.Normalized();

        var speed2 = proj_speed * proj_speed;
        var speed4 = proj_speed * proj_speed * proj_speed * proj_speed;
        var y = diff.y;
        var x = groundDist;
        var gx = GRAVITY * x;

        var root = speed4 - GRAVITY * ((GRAVITY * x * x) + (2 * y * speed2));

        // No solution
        if (root < 0)
        {
            if (!force)
            {
                return new Vector3();
            }

            return groundDir * proj_speed + new Vector3(0, 1, 0) * Mathf.Sin(Mathf.Pi / 4.0f) * proj_speed;
        }

        root = Mathf.Sqrt(root);

        var lowAng = Mathf.Atan2(speed2 - root, gx);
        var highAng = Mathf.Atan2(speed2 + root, gx);
        var numSolutions = (lowAng != highAng) ? 2 : 1;

        return groundDir * Mathf.Cos(lowAng) * proj_speed + new Vector3(0, 1, 0) * Mathf.Sin(lowAng) * proj_speed;
    }

    public void Launch(Vector3 target)
    {
        ApplyTorqueImpulse(new Vector3((float)GD.Randf(), (float)GD.Randf(), (float)GD.Randf()));
        var vec = SolveBallisticArcLateral(GlobalTransform.origin, 10, target, true);
        ApplyCentralImpulse(vec);
    }

    void OnHit(Node node)
    {
        GetParent().RemoveChild(this);
    }
}
