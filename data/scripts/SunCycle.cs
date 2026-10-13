using System;
using Unravel.Core;

/// <summary>
/// Sweeps the sun (the directional light on this entity) around the sky to show the global illumination
/// reacting in real time. Press T to start or stop the sweep, R to return to the authored sun.
/// </summary>
[ScriptSourceFile]
public class SunCycle : ScriptComponent
{
    /// <summary>Authored sun elevation above the horizon, in degrees.</summary>
    public float elevation = 38.0f;

    /// <summary>Authored sun yaw (the rotation's Y angle), in degrees.</summary>
    public float yaw = 150.0f;

    /// <summary>Lowest elevation reached during the sweep, in degrees.</summary>
    public float minElevation = 12.0f;

    /// <summary>Highest elevation reached during the sweep, in degrees.</summary>
    public float maxElevation = 58.0f;

    /// <summary>Sweep speed in degrees of yaw per second.</summary>
    public float degreesPerSecond = 6.0f;

    /// <summary>Starts sweeping as soon as play begins.</summary>
    public bool sweepOnStart = false;

    private const float ROLL = 180.0f;

    private bool isSweeping;
    private float sweepYaw;

    public override void OnStart()
    {
        isSweeping = sweepOnStart;
        sweepYaw = yaw;
        ApplySun(elevation, yaw);
    }

    public override void OnUpdate()
    {
        if(Input.IsPressed(KeyCode.T))
        {
            isSweeping = !isSweeping;
        }
        if(Input.IsPressed(KeyCode.R))
        {
            isSweeping = false;
            sweepYaw = yaw;
            ApplySun(elevation, yaw);
        }
        if(!isSweeping)
        {
            return;
        }
        sweepYaw += degreesPerSecond * Time.deltaTime;
        float noon = 0.5f + 0.5f * Mathf.Cos((sweepYaw - yaw) * Mathf.Deg2Rad);
        ApplySun(Mathf.Lerp(minElevation, maxElevation, noon), sweepYaw);
    }

    /// <summary>
    /// The [-(180 - elevation), yaw, 180] Euler family keeps the light pointing down at the given elevation while
    /// the yaw alone steers the azimuth.
    /// </summary>
    private void ApplySun(float elevationDegrees, float yawDegrees)
    {
        transform.eulerAngles = new Vector3(-(180.0f - elevationDegrees), yawDegrees, ROLL);
    }
}
