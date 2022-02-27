﻿#if ZED_OPENCV_FOR_UNITY

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
public class randmove : MarkerObject
{
    private float movementDuration = 0.5f;
    private float waitBeforeMoving = 0f;
    private bool hasArrived = false;
    float timecounter = 0;
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
        GameObject camera_eyes = GameObject.Find("Camera_eyes");
        Vector3 average_marker_position = GetAveragePosition();
        Vector3 relative_vector = average_marker_position - camera_eyes.transform.position;
        float relative_distance = relative_vector.magnitude;

        if (relative_distance < 0.3f)
        {
            

            hiddenFramesCount = 0;
            gameObject.SetActive(true); //We don't check disableWhenNotSeen when applying this in case the user changes the setting at runtime. 

            positionStack.Push(worldposition);
            if (!hasArrived)
            {
                hasArrived = true;
                float randX = Random.Range( -0.05f, 0.05f);
                float randY = Random.Range( -0.05f, 0.05f);
                StartCoroutine(MoveToPoint(GetAveragePosition() + new Vector3(randX, randY, 0f)));
            }

            rotationStack.Push(worldrotation);
            transform.rotation = GetAverageRotation();
        }
        else if (relative_distance < 0.6f)
        {
            hiddenFramesCount = 0;
            gameObject.SetActive(true); //We don't check disableWhenNotSeen when applying this in case the user changes the setting at runtime. 

            positionStack.Push(worldposition);
            if (!hasArrived)
            {
                hasArrived = true;
                float randX = Random.Range( -0.2f, 0.2f);
                float randY = Random.Range(-0.2f, 0.2f);
                StartCoroutine(MoveToPoint(GetAveragePosition()+ new Vector3(randX, randY, 0f)));
            }

            rotationStack.Push(worldrotation);
            transform.rotation = GetAverageRotation();
        }
        else if (relative_distance < 1f)
        {
            hiddenFramesCount = 0;
            gameObject.SetActive(true); //We don't check disableWhenNotSeen when applying this in case the user changes the setting at runtime. 

            positionStack.Push(worldposition);
            if (!hasArrived)
            {
                hasArrived = true;
                float randX = Random.Range( -0.4f, 0.4f);
                float randY = Random.Range(-0.4f, 0.4f);
                StartCoroutine(MoveToPoint(GetAveragePosition() + new Vector3(randX, randY, 0f)));
            }

            rotationStack.Push(worldrotation);
            transform.rotation = GetAverageRotation();
        }
        else
        {
            hiddenFramesCount = 0;
            gameObject.SetActive(true); //We don't check disableWhenNotSeen when applying this in case the user changes the setting at runtime. 

            positionStack.Push(worldposition);
            if (!hasArrived)
            {
                hasArrived = true;
                float randX = Random.Range( -0.6f, 0.6f);
                float randY = Random.Range(-0.6f, 0.6f);
                StartCoroutine(MoveToPoint(GetAveragePosition() + new Vector3(randX, randY, 0f)));
            }

            rotationStack.Push(worldrotation);
            transform.rotation = GetAverageRotation();
        }



    }


    private IEnumerator MoveToPoint(Vector3 targetPos)
    {
        float timer = 0.0f;
        Vector3 startPos = transform.position;

        while (timer < movementDuration)
        {
            timer += Time.deltaTime;
            float t = timer / movementDuration;
           // t = t * t * t * (t * (6f * t - 15f) + 10f);
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        yield return new WaitForSeconds(waitBeforeMoving);
        hasArrived = false;
    }

    /// <summary>
    /// Calculates average position of all positions in positionStack.
    /// </summary>
    /// <returns>Smoothed position.</returns>
    private Vector3 GetAveragePosition()
    {
        //Slight change in this function for generating a circular motion

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