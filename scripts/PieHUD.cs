using Godot;
using System;

class Selection
{
    public String slice { get; }
    public int tier { get; }

    public Selection(String slice, int tier)
    {
        this.slice = slice;
        this.tier = tier;
    }
}

public class PieHUD : Control
{
    public const float TAU = Mathf.Pi / 2.0f;

    [Signal]
    delegate void pie_released(Vector2 position, int slice, int tier);

    // Pixel offsets for each tier
    static readonly int[] TIERS = {
        62,
        100,
        142
    };

    // Radius of the dead zone, within which nothing can be selected.
    const int DEAD_ZONE_RADIUS = 10;


    // From θ=0 up, a list of radial slices
    static readonly String[] SLICES = {
        "damage",
        "damage-buff",
        "buff",
        "buff-health",
        "health",
        "health-curse",
        "curse",
        "curse-damage"
    };
    static readonly float SLICE_RADIANS = (2.0f * Mathf.Pi) / (float)SLICES.Length;

    Selection? _selected;

    bool AngleBetween(float n, float a, float b)
    {
        n = (TAU + (n % TAU)) % TAU;
        a = (TAU * 10 + a) % TAU;
        b = (TAU * 10 + b) % TAU;

        if (a < b) {
            return a <= n && n <= b;
        }

        return a <= n || n <= b;
    }

    public override void _Draw()
    {
        if (_selected == null)
        {
            return;
        }

        int r = TIERS[_selected.tier - 1];
        float theta = Array.FindIndex(SLICES, x => x == _selected.slice) * SLICE_RADIANS;
        Vector2 cartesian = Mathf.Polar2Cartesian(r, -theta);
        DrawCircle(cartesian, 15.0f, new Color(1.0f, 1.0f, 1.0f, 0.8f));
    }

    void  OnPieVisibilityChanged()
    {
        if (Visible == true)
        {
            Engine.TimeScale = 0.1f;
            RectPosition = GetGlobalMousePosition();
        }
        else
        {
            Engine.TimeScale = 1.0f;
            this.WarpMouse(new Vector2(0, 0));
            if (_selected != null) {
                EmitSignal("pie_released", RectPosition, _selected.slice, _selected.tier);
                _selected = null;
            }
        }
    }

    public override void _Input(InputEvent ev)
    {
        if (Input.IsActionJustReleased("activate_pie"))
        {
            Visible = false;
        }

        var cursor = GetLocalMousePosition();
        float r = Mathf.Sqrt(Mathf.Pow(cursor.x, 2) + Mathf.Pow(cursor.y, 2));
        float theta = Mathf.Wrap(Mathf.Atan2(-cursor.y, cursor.x), 0, TAU);

        if (r <= DEAD_ZONE_RADIUS)
        {
            _selected = null;
            Update();
            return;
        }

        int tier = 0;
        float delta = float.PositiveInfinity;
        for(int i = 0; i < TIERS.Length; i += 1)
        {
            float candidate_delta = Math.Abs(TIERS[i] - r);
            if (candidate_delta < delta)
            {
                tier = i + 1;
                delta = candidate_delta;
            }
        }

        float slice_theta = TAU - (SLICE_RADIANS / 2);
        String slice = "";
        foreach (var candidateSlice in SLICES) {
            if (AngleBetween(theta, slice_theta, Mathf.Wrap((slice_theta + SLICE_RADIANS), 0, TAU))) {
                slice = candidateSlice;
                break;
            }

            slice_theta = Mathf.Wrap(slice_theta + SLICE_RADIANS, 0, TAU);
        }

        if (slice.Empty()) {
            _selected = null;
        } else {
            _selected = new Selection(slice, tier);
            Update();
        }
    }
}
