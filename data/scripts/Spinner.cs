using System;
using Unravel.Core;

/// <summary>
/// Spins its entity about the world up axis at a constant rate. Drives the kinetic sculpture's rotor, whose
/// coloured vanes sweep sunlight and coloured bounce light around the hall.
/// </summary>
[ScriptSourceFile]
public class Spinner : ScriptComponent
{
    /// <summary>Rotation speed in degrees per second; negative spins clockwise seen from above.</summary>
    public float degreesPerSecond = 10.0f;

    public override void OnUpdate()
    {
        transform.RotateAxis(degreesPerSecond * Time.deltaTime, Vector3.up);
    }
}
