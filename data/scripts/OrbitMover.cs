using System;
using Unravel.Core;

/// <summary>
/// Moves its entity on a horizontal circle with a gentle vertical bob. Carries the Light Lab's glowing orb (an
/// emissive sphere with a point light) so the room's direct and indirect lighting change continuously.
/// </summary>
[ScriptSourceFile]
public class OrbitMover : ScriptComponent
{
    /// <summary>World-space centre of the orbit; its Y is the mean height.</summary>
    public Vector3 center = new Vector3(0.0f, 1.6f, 0.0f);

    /// <summary>Orbit radius in metres.</summary>
    public float radius = 2.4f;

    /// <summary>Angular speed in degrees per second.</summary>
    public float degreesPerSecond = 22.0f;

    /// <summary>Peak vertical offset of the bob, in metres.</summary>
    public float bobHeight = 0.35f;

    /// <summary>Bob cycles per second.</summary>
    public float bobFrequency = 0.12f;

    private const float FULL_TURN_RADIANS = 6.2831853f;

    private float angleDegrees;

    public override void OnStart()
    {
        Vector3 offset = transform.position - center;
        angleDegrees = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
    }

    public override void OnUpdate()
    {
        angleDegrees += degreesPerSecond * Time.deltaTime;
        float radians = angleDegrees * Mathf.Deg2Rad;
        float bob = Mathf.Sin(Time.time * bobFrequency * FULL_TURN_RADIANS) * bobHeight;
        transform.position = new Vector3(center.x + Mathf.Sin(radians) * radius,
                                         center.y + bob,
                                         center.z + Mathf.Cos(radians) * radius);
    }
}
