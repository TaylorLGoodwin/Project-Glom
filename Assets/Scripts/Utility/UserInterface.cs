﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    public static UserInterface Instance;

    public GameObject spellSlot;
    public GameObject userInterface;
    public List<Sprite> spells;
    public List<Sprite> userInterfaces;
    public List<GameObject> healthBars;

    private GameObject healthBar;
    private Transform parent;

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
        parent = transform;
	}

    //Updates all pieces of the UI, used at the begining of a level.
    public void UpdateUserInterface ()
    {
        spellSlot.GetComponent<SpriteRenderer>().sprite = spells[GameControl.Instance.selectedSpellId];
        userInterface.GetComponent<SpriteRenderer>().sprite = userInterfaces[SaveDataController.Instance.healthLevelUsedForSelectingUIHealthBar - 1];
        Destroy(healthBar);
        healthBar = Instantiate(healthBars[SaveDataController.Instance.healthLevelUsedForSelectingUIHealthBar - 1], parent);
    }

    public void UpdateHealth ()
    {
        healthBar.GetComponent<Slider>().value = healthBar.GetComponent<Slider>().maxValue - GameControl.Instance.player_CurrentHP;
    }

    public void UpdateSelectedSpell ()
    {
        spellSlot.GetComponent<SpriteRenderer>().sprite = spells[GameControl.Instance.selectedSpellId ];
    }
}