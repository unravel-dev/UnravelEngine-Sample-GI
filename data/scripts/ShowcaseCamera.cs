using System;
using System.Collections.Generic;
using System.Globalization;
using Unravel.Core;

/// <summary>
/// Demo camera for the showcase: a looping tour along authored viewpoints, or free flight.
/// Keys: F toggles free flight, N skips to the next viewpoint, P pauses the tour.
/// Free flight: hold the right mouse button to look, WASD / arrows to move, Q / E to sink / rise,
/// mouse wheel to change the speed.
/// </summary>
[ScriptSourceFile]
public class ShowcaseCamera : ScriptComponent
{
    /// <summary>Viewpoints as "x y z pitch yaw hold" separated by ';'. hold is the pause in seconds (0 = pass through).</summary>
    public string waypoints = "";

    /// <summary>Travel speed along the path, in metres per second.</summary>
    public float travelSpeed = 2.2f;

    /// <summary>Shortest time spent on one segment, so turns in place stay smooth.</summary>
    public float minSegmentSeconds = 2.0f;

    /// <summary>Turn rate that bounds how fast a segment may rotate the view, in degrees per second.</summary>
    public float turnDegreesPerSecond = 35.0f;

    /// <summary>Runs the tour when play starts; otherwise the camera stays where it is until F is pressed.</summary>
    public bool autoTour = true;

    /// <summary>Free-flight mouse look sensitivity.</summary>
    public float lookSensitivity = 10.0f;

    /// <summary>Free-flight speed in metres per second.</summary>
    public float flySpeed = 4.0f;

    private struct Waypoint
    {
        public Vector3 position;
        public Quaternion rotation;
        public float hold;
    }

    private const float MIN_FLY_SPEED = 0.5f;
    private const float MAX_FLY_SPEED = 40.0f;
    private const float MAX_PITCH = 89.0f;

    private readonly List<Waypoint> points = new List<Waypoint>();
    private int segment;
    private float segmentTime;
    private float segmentDuration;
    private bool isFlying;
    private bool isPaused;
    private float flyPitch;
    private float flyYaw;

    public override void OnStart()
    {
        ParseWaypoints();
        if(!autoTour || points.Count < 2)
        {
            return;
        }
        BeginSegment(0);
    }

    public override void OnUpdate()
    {
        if(Input.IsPressed(KeyCode.F))
        {
            ToggleFlight();
        }
        if(isFlying)
        {
            UpdateFlight();
            return;
        }
        if(!autoTour || points.Count < 2)
        {
            return;
        }
        if(Input.IsPressed(KeyCode.P))
        {
            isPaused = !isPaused;
        }
        if(Input.IsPressed(KeyCode.N))
        {
            BeginSegment(NextViewpoint(segment));
            return;
        }
        if(isPaused)
        {
            return;
        }
        UpdateTour();
    }

    private void ParseWaypoints()
    {
        points.Clear();
        string[] entries = waypoints.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach(string entry in entries)
        {
            string[] values = entry.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if(values.Length < 6)
            {
                continue;
            }
            float[] numbers = new float[6];
            for(int i = 0; i < 6; ++i)
            {
                numbers[i] = float.Parse(values[i], CultureInfo.InvariantCulture);
            }
            points.Add(new Waypoint
            {
                position = new Vector3(numbers[0], numbers[1], numbers[2]),
                rotation = Quaternion.Euler(numbers[3], numbers[4], 0.0f),
                hold = numbers[5],
            });
        }
    }

    private int Wrap(int index)
    {
        int count = points.Count;
        return ((index % count) + count) % count;
    }

    private int NextViewpoint(int from)
    {
        for(int step = 1; step <= points.Count; ++step)
        {
            int index = Wrap(from + step);
            if(points[index].hold > 0.0f)
            {
                return index;
            }
        }
        return Wrap(from + 1);
    }

    private void BeginSegment(int index)
    {
        segment = Wrap(index);
        Waypoint start = points[segment];
        Waypoint end = points[Wrap(segment + 1)];
        float travelSeconds = Vector3.Distance(start.position, end.position) / Mathf.Max(travelSpeed, 0.01f);
        float turnSeconds = Quaternion.Angle(start.rotation, end.rotation) / Mathf.Max(turnDegreesPerSecond, 1.0f);
        segmentDuration = Mathf.Max(minSegmentSeconds, Mathf.Max(travelSeconds, turnSeconds));
        segmentTime = -start.hold;
        transform.position = start.position;
        transform.rotation = start.rotation;
    }

    private void UpdateTour()
    {
        segmentTime += Time.deltaTime;
        if(segmentTime < 0.0f)
        {
            return;
        }
        if(segmentTime >= segmentDuration)
        {
            BeginSegment(segment + 1);
            return;
        }
        float t = segmentTime / segmentDuration;
        Waypoint start = points[segment];
        Waypoint end = points[Wrap(segment + 1)];
        float startTangent = start.hold > 0.0f ? 0.0f : 1.0f;
        float endTangent = end.hold > 0.0f ? 0.0f : 1.0f;
        float eased = Hermite(t, startTangent, endTangent);
        transform.position = CatmullRom(points[Wrap(segment - 1)].position, start.position, end.position,
                                        points[Wrap(segment + 2)].position, eased);
        transform.rotation = Quaternion.Slerp(start.rotation, end.rotation, Mathf.SmoothStep(0.0f, 1.0f, t));
    }

    /// <summary>Cubic Hermite from 0 to 1 with the given end slopes: 0 eases in or out, 1 keeps linear speed.</summary>
    private static float Hermite(float t, float startTangent, float endTangent)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return startTangent * (t3 - 2.0f * t2 + t) + (-2.0f * t3 + 3.0f * t2) + endTangent * (t3 - t2);
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * ((2.0f * p1) + (-p0 + p2) * t + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2
                       + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t3);
    }

    private void ToggleFlight()
    {
        isFlying = !isFlying;
        if(!isFlying)
        {
            return;
        }
        Vector3 forward = transform.forward;
        flyYaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        flyPitch = -Mathf.Asin(Mathf.Clamp(forward.y, -1.0f, 1.0f)) * Mathf.Rad2Deg;
    }

    private void UpdateFlight()
    {
        float speedAdjust = Input.GetAxis("Mouse ScrollWheel");
        flySpeed = Mathf.Clamp(flySpeed * (1.0f + speedAdjust * 0.1f), MIN_FLY_SPEED, MAX_FLY_SPEED);
        if(Input.IsDown("Mouse Right"))
        {
            flyYaw += Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
            flyPitch = Mathf.Clamp(flyPitch - Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime, -MAX_PITCH, MAX_PITCH);
        }
        Quaternion rotation = Quaternion.AngleAxis(flyYaw, Vector3.up) * Quaternion.AngleAxis(flyPitch, Vector3.right);
        float rise = (Input.IsDown(KeyCode.E) ? 1.0f : 0.0f) - (Input.IsDown(KeyCode.Q) ? 1.0f : 0.0f);
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        transform.rotation = rotation;
        transform.position = transform.position + (rotation * move + Vector3.up * rise) * (flySpeed * Time.deltaTime);
    }
}
