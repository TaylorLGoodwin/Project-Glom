﻿using UnityEngine;

public class StartController : MonoBehaviour
{
    void Start ()
    {
        SceneTransitions.instance.TransitionIn();
    }
}