using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Utils
{
    // https://en.wikipedia.org/wiki/Projectile_motion : Angle θ required to hit coordinate (x, y)
    public static float[] GetAnglesToHitCoordinate(Vector3 launchCoord, Vector3 targetCoord, float projectileInitialVelocity)
    {
        float[] angles = new float[2] {0f, 0f};

        float horizontalDisplacement = Vector3.Distance(new Vector3(launchCoord.x, 0f, launchCoord.z), new Vector3(targetCoord.x, 0f, targetCoord.z));
        float verticalDisplacement = targetCoord.y - launchCoord.y;

        float g = -Physics.gravity.y;
        float root = Mathf.Pow(projectileInitialVelocity, 4f) - g * (g * Mathf.Pow(horizontalDisplacement, 2f) + 2f * verticalDisplacement * Mathf.Pow(projectileInitialVelocity, 2f));

        if (root >= 0)
        {
            root = Mathf.Sqrt(root);
            angles[0] = Mathf.Atan((Mathf.Pow(projectileInitialVelocity, 2f) - root) / (g * horizontalDisplacement)) * Mathf.Rad2Deg;
            angles[1] = Mathf.Atan((Mathf.Pow(projectileInitialVelocity, 2f) + root) / (g * horizontalDisplacement)) * Mathf.Rad2Deg;
        }
        else
        {
            Debug.Log("Imaginary root!");
        }

        return angles;
    }

    public static float GetMinAngle(float[] angles)
    {
        return Mathf.Min(angles[0], angles[1]);
    }

    // https://en.wikipedia.org/wiki/Projectile_motion
    public static float GetLaunchVelocityToReachCoordinates(Vector3 launchCoord, Vector3 targetCoord, float launchAngle)
    {
        // v = sqrt( (x^2 * g) / (x * sin(2 * a) - 2 * y * cos2(a)) )
        float x = Vector3.Distance(targetCoord, launchCoord); // Horizontal displacement
        float y = launchCoord.y - targetCoord.y; // Vertical displacement
        float a = launchAngle * Mathf.Deg2Rad; // Launch angle
        float g = Physics.gravity.y; // Gravity

        float velocity = Mathf.Abs(
            Mathf.Sqrt(
                    (Mathf.Pow(x, 2) * g) /
                    (x * Mathf.Sin(2f * a) - 2 * y * Mathf.Pow(Mathf.Cos(a), 2))
                )
        );
        return velocity;
    }

    public static Vector3 GetYZeroPositionByRay(Ray ray)
    {
        Vector3 hitPoint = Vector3.zero;

        Plane yZeroPlane = new Plane(Vector3.up, Vector3.zero);
        float enter = 0f;

        // Make a ray
        if (yZeroPlane.Raycast(ray, out enter))
        {
            hitPoint = ray.GetPoint(enter);
        }

        return hitPoint;
    }

    public static string StringColor(string str, string col)
    {
        return "<color=" + col + ">" + str + "</color>";
    }

    public static bool IsPointerOverUIObject()
    {
        // Code from video https://www.youtube.com/watch?v=QL6LOX5or84
        // This is expensive to performance. Call only once per frame.
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        // Make ignored results list because the "results"-list can't be modified while iterating over it
        LayerMask ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        List<RaycastResult> ignoredResults = new List<RaycastResult>();
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.layer == ignoreLayer)
            {
                ignoredResults.Add(result);
            }
        }
        foreach (RaycastResult ignoredResult in ignoredResults)
        {
            results.Remove(ignoredResult);
        }

        return results.Count > 0;
    }

    public static float GetSoundVolume()
    {
        return ClientManager.instance.masterVolume * ClientManager.instance.soundVolume;
    }

    public static float GetMusicVolume()
    {
        // Make it a bit quieter by multiplying with 0.5f
        return ClientManager.instance.masterVolume * ClientManager.instance.musicVolume * 0.5f;
    }

    public static void PlayButtonSound()
    {
        ClientManager.instance.PlayButtonSound();
    }
}
