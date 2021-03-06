﻿using UnityEngine;

public class PixelPerfectScaler : MonoBehaviour
{
    private int screenVerticalPixels;
    public bool preferUncropped = true;

    private float screenPixelsY = 0;
    private bool currentCropped = false;

    void Start ()
    {
        screenVerticalPixels = (int)GetComponent<Camera>().orthographicSize * 2;
    }

    void Update()
    {
        if (screenPixelsY != (float)Screen.height || currentCropped != preferUncropped)
        {
            screenPixelsY = (float)Screen.height;
            currentCropped = preferUncropped;

            float screenRatio = screenPixelsY / screenVerticalPixels;
            float ratio = 0;

            if (preferUncropped)
            {
                ratio = Mathf.Floor(screenRatio) / screenRatio;
            }
            else
            {
                ratio = Mathf.Ceil(screenRatio) / screenRatio;
            }

            transform.localScale = Vector3.one * ratio;
        }
    }
}