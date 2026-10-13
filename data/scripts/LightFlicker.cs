using System;
using Unravel.Core;

/// <summary>
/// Modulates the intensity of the light on its own entity with layered sine noise, like a wood fire.
/// </summary>
[ScriptSourceFile]
public class LightFlicker : ScriptComponent
{
    /// <summary>Mean intensity the flicker oscillates around.</summary>
    public float baseIntensity = 18.0f;

    /// <summary>Relative depth of the flicker (0.3 = +-30 percent at the peaks).</summary>
    public float flickerAmount = 0.3f;

    /// <summary>Base flicker rate in radians per second.</summary>
    public float flickerSpeed = 7.0f;

    private LightComponent lightComponent;
    private float phase;

    public override void OnStart()
    {
        lightComponent = owner.GetComponent<LightComponent>();
        phase = Random.Float() * 100.0f;
    }

    public override void OnUpdate()
    {
        if(lightComponent == null)
        {
            return;
        }
        float t = Time.time * flickerSpeed + phase;
        float noise = 0.5f * Mathf.Sin(t) + 0.3f * Mathf.Sin(t * 2.31f + 1.3f) + 0.2f * Mathf.Sin(t * 5.73f + 0.7f);
        lightComponent.intensity = baseIntensity * (1.0f + flickerAmount * noise);
    }
}
