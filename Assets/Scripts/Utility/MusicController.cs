﻿using UnityEngine;
using System;
using System.Collections.Generic;
using static Utility;

public class MusicController : MonoBehaviour
{
    public static MusicController Instance;

    public List<MusicTrack> tracks;

    private AudioSource player;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    void Start ()
    {
        player = GetComponent<AudioSource>();
	}

    public void PlayTrack (Utility.MusicTrack track)
    {
        AudioClip clip = null;

        foreach (MusicTrack musicTrack in tracks)
        {
            if (musicTrack.track == track)
            {
                clip = musicTrack.clip;
                break;
            }
        }

        player.clip = clip;
        player.Play();
    }
}


[Serializable]
public class MusicTrack
{
    public Utility.MusicTrack track;
    public AudioClip clip;

    public MusicTrack(Utility.MusicTrack track, AudioClip clip)
    {
        this.track = track;
        this.clip = clip;
    }
}