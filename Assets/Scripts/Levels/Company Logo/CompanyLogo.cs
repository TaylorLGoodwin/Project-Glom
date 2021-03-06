﻿using UnityEngine;
using static Utility;

public class CompanyLogo : MonoBehaviour
{
    private void Start()
    {
        GetComponentInParent<Canvas>().worldCamera = GameControl.Instance.mainCamera;
    }

    public void PlaySoundEffect()
    {
        SoundEffectsController.Instance.PlaySoundEffect(Utility.SoundEffect.Company_Logo);
    }

    public void TransitionOut()
    {
        LevelManager.Instance.LoadLevel(Level.Level_Zero);
    }
}