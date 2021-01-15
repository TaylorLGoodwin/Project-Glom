﻿using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class ProjectileMovement : MonoBehaviour
{
    [HideInInspector]
    public int spellId;
    private Controller2D controller;
    private int direction;
    private Vector3 velocity;
    private int damage;
    private bool initialized = false;
    private int castKey = 0;

    #region Deterioration
    private bool deteriorating = false;
    private float lastPos = 0;
    #endregion

    void Start()
    {
        controller = GetComponent<Controller2D>();
        GetComponent<Animator>().Play(PlayerSpellControl.Instance.spells[spellId].name);

        if (GetComponent<SpriteRenderer>().flipX == true)
        {
            direction = -1;
        }
        else
        {
            direction = 1;
        }

        velocity.y = 0;
        velocity.x = PlayerSpellControl.Instance.spells[spellId].projectileSpeed * direction;

        initialized = true;
    }

    void Update()
    {
        if (initialized)
        {
            if (IsNotMoving() && !deteriorating)
            {
                StartCoroutine(Deterierate());
            }

            controller.Move(velocity, Vector2.zero);

            if (transform.position.x < -10 || transform.position.x > 500)
            {
                Destroy(gameObject);
            }
            
        }
    }

    private bool IsNotMoving()
    {
        if (transform.position.x == lastPos)
        {
            return true;
        }
        else
        {
            lastPos = transform.position.x;
            return false;
        }
    }

    public void Initialize(float speed, int damage, int castKey)
    {
        velocity = new Vector3(speed * direction, 0, 0);
        this.damage = damage;
        this.castKey = castKey;
        initialized = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 9 || collision.gameObject.layer == 10 || collision.gameObject.layer == 11)
        {
            if (collision is BoxCollider2D && collision.gameObject.layer == 11)
            {
                velocity.x = 0;

                //old way
                //collision.GetComponent<EnemyStats>().currentHp -= (int)PlayerSpellControl.Instance.spells[spellId].baseDamage;

                //new way
                collision.GetComponent<EnemyStats>().currentHp -= damage;
                PlayerSpellControl.Instance.DisseminateHitInformation(castKey, collision.gameObject.GetInstanceID());
            }
        }
    }

    IEnumerator Deterierate()
    {
        deteriorating = true;
        yield return new WaitForSeconds(.1f);
        Destroy(gameObject);
    }
}