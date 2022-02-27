#if ZED_OPENCV_FOR_UNITY

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves the object to the marker's location each grab, and turns itself off when it's not seen if desired. 
/// Unlike the non-advanced version, also waits several frames until disabling itself in case of camera flicker, 
/// and smoothes its position and rotation over a few frames. 
/// </summary><remarks>
/// Note that if you see multiple copies of the same marker in the scene, the object will jump wildly between them.
/// It's recommended to avoid this, but if it's important to support multiple copies of the same marker, 
/// see MarkerObject_CreateObjectsAtMarkers.cs.
/// </remarks>
public class nosechange : MarkerObject
{
    /// <summary>
    /// If true, will disable (set inactive) this object's gameObject when it was visible, but then is no longer visible. 
    /// </summary>
    [Tooltip("If true, will disable (set inactive) this object's gameObject when it was visible, but then is no longer visible. ")]
    public bool disableWhenNotSeen = true;

    /// <summary>
    /// How many consecutive frames of detection does this object not have to be seen before it's disabled.
    /// Used to avoid flickering when noise causes frames not to be detected for a short time
    /// when they're actually in view of the camera.
    /// </summary>
    [Tooltip("How many consecutive frames of detection does this object not have to be seen before it's disabled.\r\n" +
        "Used to avoid flickering when noise causes frames not to be detected for a short time " +
        "when they're actually in view of the camera.")]
    public int missedFramesUntilDisabled = 3;
    /// <summary>
    /// How many frames in a row the corresponding marker was not seen. 
    /// </summary>
    private int hiddenFramesCount = 0;

    /// <summary>
    /// How many frames to use to smooth marker position/rotation updates.
    /// Larger numbers reduce jitter from detection inaccuracy, but add latency to the marker's movements.
    /// </summary>
    [Tooltip("How many frames to use to smooth marker position/rotation updates. " +
        "Larger numbers reduce jitter from detection inaccuracy, but add latency to the marker's movements.")]
    public int smoothedFrames = 4;

    /// <summary>
    /// List of last X positions updated, where X is the max number of smoothed frames (unless that many haven't happened yet).
    /// Used for smoothing positions. 
    /// </summary>
    private CappedStack<Vector3> positionStack;
    /// <summary>
    /// List of last X rotations updated, where X is the max number of smoothed frames (unless that many haven't happened yet).
    /// Used for smoothing rotations. 
    /// </summary>
    private CappedStack<Quaternion> rotationStack;

    private void Awake()
    {
        positionStack = new CappedStack<Vector3>(smoothedFrames);
        rotationStack = new CappedStack<Quaternion>(smoothedFrames);
    }

    public override void MarkerDetectedSingle(Vector3 worldposition, Quaternion worldrotation)
    {
        hiddenFramesCount = 0;
        gameObject.SetActive(true); //We don't check disableWhenNotSeen when applying this in case the user changes the setting at runtime. 

        positionStack.Push(worldposition);

        /// Placing the object a fixed distance in front of the camera
        //GameObject cameraEyes = GameObject.Find("Camera_eyes");
        //Vector3 cameraPosition = cameraEyes.transform.position;
        //Vector3 cameraDirection = cameraEyes.transform.forward;
        //cameraPosition += cameraDirection;
        //transform.position = cameraPosition;
        /// Original code placing the camera at the marker position
        //transform.position = GetAveragePosition();

        /// Place the object inbetween the marker and the camera according to a function
        GameObject camera_eyes = GameObject.Find("Camera_eyes");
        Vector3 average_marker_position = GetAveragePosition();
        Vector3 relative_vector = average_marker_position - camera_eyes.transform.position;
        float relative_distance = relative_vector.magnitude;
        /// Use a magic scaling function to change how close it is -> plug this into desmos to see what it looks like x-e^{\left(-\frac{\left(x-2\right)}{1.4}^{2}\right)}
        //float transformed_distance = relative_distance - (float)(Math.Exp(-Math.Pow((relative_distance-2),2)/1.4));
        float transformed_distance;
        float clip_distance = (float)1.6;
        if (relative_distance < clip_distance)
        {
            transformed_distance = (float)(Math.Pow((clip_distance), 4) / 20);
            float scale = (float)((clip_distance) / relative_distance);
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else if (relative_distance < 2.714)
        {
            float scale_zero = (float)0.0;
            transformed_distance = (float)(Math.Pow((relative_distance), 4) / 20);
            transform.localScale = new Vector3(scale_zero, scale_zero, scale_zero);
        }
        else
        {
            transformed_distance = (float)relative_distance;
            float scale_zero = (float)0.0;
            transform.localScale = new Vector3(scale_zero, scale_zero, scale_zero);
        }

        transform.position = camera_eyes.transform.position + relative_vector * transformed_distance / relative_distance;

        //Vector3 cameraPosition = cameraEyes.transform.position;

        rotationStack.Push(worldrotation);
        transform.rotation = GetAverageRotation();
    }

    public override void MarkerNotDetected()
    {
        if (disableWhenNotSeen)
        {
            hiddenFramesCount++;
            if (hiddenFramesCount >= missedFramesUntilDisabled)
            {
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Calculates average position of all positions in positionStack.
    /// </summary>
    /// <returns>Smoothed position.</returns>
    private Vector3 GetAveragePosition()
    {
        Vector3 sumvector = Vector3.zero;
        foreach (Vector3 vec in positionStack)
        {
            sumvector += vec;
        }
        return sumvector / positionStack.Count;
    }

    /// <summary>
    /// Calculates average rotation of all rotations in rotationStack.
    /// </summary>
    /// <returns>Smoothed rotation.</returns>
    private Quaternion GetAverageRotation()
    {
        Vector4 sumquatvalues = Vector4.zero;
        foreach (Quaternion quat in rotationStack)
        {
            sumquatvalues.x += quat.x;
            sumquatvalues.y += quat.y;
            sumquatvalues.z += quat.z;
            sumquatvalues.w += quat.w;
        }

        Quaternion returnquat = new Quaternion();
        sumquatvalues /= rotationStack.Count;
        returnquat.x = sumquatvalues.x;
        returnquat.y = sumquatvalues.y;
        returnquat.z = sumquatvalues.z;
        returnquat.w = sumquatvalues.w;

        return returnquat;
    }
}

#endif
