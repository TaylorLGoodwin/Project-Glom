﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    #region Variables
    [Tooltip("This field is used to specify which layers block the attacking and abilities raycasts.")]
    public LayerMask attackingLayer;

    public float initialMaxJumpHeight;
    private float maxJumpHeight;
    public float initialMinJumpHeight;
    private float minJumpHeight;
    public float timeToJumpApex;
    public float InitialMoveSpeed;
    public float moveSpeed;
    public float InitialClimbSpeed;
    public float climbSpeed;
    
    private float accelerationTimeAirborne = .1f;
    private float accelerationTimeGrounded = .1f;
    private Vector2 input;
    private bool isClimbable;
    private bool summiting;
    public bool casting;
    public int selectedSpell;

    //private bool uninteruptable;

    [HideInInspector] public int knockBackForce;
    [HideInInspector] public float climbingUpPosition;

    private PlayerAnimationController animator;
    private Controller2D controller;

    private float gravity;
    private float maxJumpVelocity;
    private float minJumpVelocity;
    private Vector2 velocity;
    private float velocityXSmoothing;

    public Dictionary<int, float> cooldownList;
    #endregion

    void Start()
    {
        GameControl.playerCurrentHP = 10;
        controller = GetComponent<Controller2D>();
        animator = GetComponent<PlayerAnimationController>();
        cooldownList = new Dictionary<int, float>();
        casting = false;

        ResetCharacterPhysics();
        controller.characterState = Controller2D.CharacterStates.Standing;
    }

    void Update()
    {
        if (GameControl.playerHasControl)
        {
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            DetermineState(input);
            velocity = StateResult(input);
            controller.Move(velocity, input);
        }

        UpdateCoolDownList();

        #region Old

        #region Flinching Section
        //else if (controller.characterState == Controller2D.CharacterStates.Flinching)
        //{
        //    UnPauseAnimators();
        //    attackLaunched = false;
        //    //weaponCollider.DisableActiveCollider();
        //    //SkillsController.skillsController.activatingAbility = false;
        //    //CombatEngine.combatEngine.comboCount = 1;
        //    //animator.PlayAnimation(PlayerAnimationController.Animations.Flinching);
        //    //PlayerSoundEffects.playerSoundEffects.PlaySoundEffect(PlayerSoundEffects.playerSoundEffects.SoundEffectToArrayInt(PlayerSoundEffects.SoundEffect.MenuUnable));

        //    gravity = -1000;
        //    velocity.y = 0;
        //    velocity.y += gravity * Time.deltaTime;
        //    velocity.x = 0;
        //    //controller.Move(velocity * Time.deltaTime, input);
        //}
        #endregion

        #region Wall Jumping
        //int wallDirX = (controller.collisions.left) ? -1 : 1;
        //bool wallSliding = false;
        //if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0 && controller.wallJumpReady == true)
        //{
        //    wallSliding = true;

        //    if (velocity.y < -wallSlideSpeedMax)
        //    {
        //        velocity.y = -wallSlideSpeedMax;
        //    }

        //    if (timeToWallUnstick > 0)
        //    {
        //        velocityXSmoothing = 0;
        //        velocity.x = 0;

        //        if (input.x != wallDirX && input.x != 0)
        //        {
        //            timeToWallUnstick -= Time.deltaTime;
        //        }
        //        else
        //        {
        //            timeToWallUnstick = wallStickTime;
        //        }
        //    }
        //    else
        //    {
        //        timeToWallUnstick = wallStickTime;
        //    }
        //}
        #endregion
        
        #endregion
    }

    private void DetermineState (Vector2 input)
    {
        //Dying
        if (GameControl.playerCurrentHP <= 0)
        {
            controller.characterState = Controller2D.CharacterStates.Dying;
        }
        //Climbing
        else if (Input.GetButtonDown("Interact") && isClimbable && !casting)
        {
            controller.characterState = Controller2D.CharacterStates.Climbing;
        }
        //Summiting
        else if (summiting && controller.characterState == Controller2D.CharacterStates.Climbing)
        {
            controller.characterState = Controller2D.CharacterStates.Summiting;
        }
        //Jumping
        else if ((Input.GetButtonDown("Jump")) && (controller.collisions.below || controller.characterState == Controller2D.CharacterStates.Climbing))
        {
            controller.characterState = Controller2D.CharacterStates.Jumping;
        }
        //Falling
        else if (!controller.collisions.below && (controller.characterState != Controller2D.CharacterStates.Climbing || !isClimbable))
        {
            if ((Input.GetButtonDown("Cast") && !cooldownList.ContainsKey(selectedSpell)) || casting)
            {
                controller.characterState = Controller2D.CharacterStates.AerialCasting;
            }
            else
            {
                controller.characterState = Controller2D.CharacterStates.Falling;
            }
        }
        //Running
        else if (controller.collisions.below && input.x != 0)
        {
            if ((Input.GetButtonDown("Cast") && !cooldownList.ContainsKey(selectedSpell)) || casting)
            {
                controller.characterState = Controller2D.CharacterStates.RunCasting;
            }
            else
            {
                controller.characterState = Controller2D.CharacterStates.Running;
            }
        }
        //Standing
        else if (input.x == 0 && controller.collisions.below)
        {
            if ((Input.GetButtonDown("Cast") && !cooldownList.ContainsKey(selectedSpell)) || casting)
            {
                controller.characterState = Controller2D.CharacterStates.StandCasting;
            }
            else
            {
                controller.characterState = Controller2D.CharacterStates.Standing;
            }
        }
    }

    private Vector2 StateResult (Vector2 input)
    {
        #region Dying
        if (controller.characterState == Controller2D.CharacterStates.Dying)
        {
            UnPauseAnimators();
            GameControl.playerHasControl = false;

            if (controller.collisions.below)
            {
                velocity = Vector2.zero;
            }
            else
            {
                velocity.x = 0;
                velocity.y += gravity * Time.deltaTime;
            }
        }
        #endregion
        #region Standing
        else if (controller.characterState == Controller2D.CharacterStates.Standing)
        {
            UnPauseAnimators();

            if (velocity.x > 0)
            {
                velocity.x -= 0.1f;
            }
            else if (velocity.x < 0)
            {
                velocity.x += 0.1f;
            }

            if (velocity.x > -0.1 && velocity.x < 0.1)
            {
                velocity.x = 0;
            }

            velocity.y = 0;
            velocity.y += gravity * Time.deltaTime;
        }
        #endregion
        #region Running
        else if (controller.characterState == Controller2D.CharacterStates.Running)
        {
            UnPauseAnimators();

            velocity.y = 0;
            velocity.y += gravity * Time.deltaTime;
            velocity.x = Mathf.SmoothDamp(velocity.x, input.x * moveSpeed, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        #endregion
        #region Jumping
        else if (controller.characterState == Controller2D.CharacterStates.Jumping)
        {
            UnPauseAnimators();
            velocity.y = maxJumpVelocity;
            velocity.y += gravity * Time.deltaTime;
        }
        #endregion
        #region Falling
        else if (controller.characterState == Controller2D.CharacterStates.Falling)
        {
            UnPauseAnimators();

            if (Input.GetButtonUp("Jump"))
            {
                if (velocity.y > minJumpVelocity)
                {
                    velocity.y = minJumpVelocity;
                }
            }

            if (controller.collisions.above)
            {
                velocity.y = 0;
            }

            velocity.y += gravity * Time.deltaTime;
            velocity.x = Mathf.SmoothDamp(velocity.x, input.x * moveSpeed, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        #endregion
        #region Climbing
        else if (controller.characterState == Controller2D.CharacterStates.Climbing)
        {
            velocity.y = input.y * climbSpeed;
            velocity.x = input.x * climbSpeed;
            PlayAnimation(input.x);

            if (input == Vector2.zero)
            {
                Invoke("PauseAnimators", 0.1f);
            }
            else
            {
                UnPauseAnimators();
            }

            return velocity;
        }
        #endregion
        #region Summiting
        else if (controller.characterState == Controller2D.CharacterStates.Summiting)
        {
            climbingUpPosition = transform.position.y + 10;
            CancelInvoke("PauseAnimators");
            UnPauseAnimators();
            velocity = Vector2.zero;
            GameControl.playerHasControl = false;
        }
        #endregion
        #region Stand Casting
        else if (controller.characterState == Controller2D.CharacterStates.StandCasting)
        {
            UnPauseAnimators();

            if (!casting)
            {
                casting = true;
                cooldownList.Add(selectedSpell, SpellDatabase.spells[selectedSpell].cooldown);
                animator.PlaySpellAnimation(selectedSpell);
            }

            if (velocity.x > 0)
            {
                velocity.x -= 0.1f;
            }
            else if (velocity.x < 0)
            {
                velocity.x += 0.1f;
            }

            if (velocity.x > -0.1 && velocity.x < 0.1)
            {
                velocity.x = 0;
            }

            velocity.y = 0;
            velocity.y += gravity * Time.deltaTime;
        }
        #endregion
        #region Run Casting
        else if (controller.characterState == Controller2D.CharacterStates.RunCasting)
        {
            UnPauseAnimators();

            if (!casting)
            {
                casting = true;
                cooldownList.Add(selectedSpell, SpellDatabase.spells[selectedSpell].cooldown);
                animator.PlaySpellAnimation(selectedSpell);
            }

            velocity.y = 0;
            velocity.y += gravity * Time.deltaTime;
            velocity.x = Mathf.SmoothDamp(velocity.x, input.x * moveSpeed, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        #endregion
        #region Aerial Casting
        else if (controller.characterState == Controller2D.CharacterStates.AerialCasting)
        {
            UnPauseAnimators();

            if (!casting)
            {
                casting = true;
                cooldownList.Add(selectedSpell, SpellDatabase.spells[selectedSpell].cooldown);
                animator.PlaySpellAnimation(selectedSpell);
            }

            velocity.y += gravity * Time.deltaTime;
            velocity.x = Mathf.SmoothDamp(velocity.x, input.x * moveSpeed, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        #endregion

        PlayAnimation(input.x);
        return velocity;
    }

    public void AdjustCharacterPhysics(float maxJumpModifier, float minJumpModifier, float moveSpeedModifier, float climbSpeedModifier)
    {
        maxJumpHeight *= maxJumpModifier;
        minJumpHeight *= minJumpModifier;
        moveSpeed *= moveSpeedModifier;
        climbSpeed *= climbSpeedModifier;
        maxJumpVelocity = (Mathf.Abs(gravity) * (timeToJumpApex)) * ((Mathf.Pow(maxJumpHeight, -0.5221f)) * 0.1694f) * maxJumpHeight;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    private void ResetCharacterPhysics ()
    {
        maxJumpHeight = initialMaxJumpHeight / 56;
        minJumpHeight = initialMinJumpHeight / 56;
        moveSpeed = InitialMoveSpeed;
        climbSpeed = InitialClimbSpeed;

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    private void PlayAnimation (float input)
    {
        string state = controller.characterState.ToString();

        //Falling and Jumping are the same animation.
        if (state == "Falling")
        {
            state = "Jumping";
        }

        animator.PlayAnimation(state, input);
    }

    public void Death()
    {
        GameControl.playerCurrentHP = 0;
    }

    //Triggers dictate climbing, interactables, level triggers, and other things.
    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == 13)
        {
            isClimbable = true;
        }

        if (collider.gameObject.layer == 14)
        {
            summiting = true;
        }

        //Reaching the Goal
        //if (collider.gameObject.layer == 18)
        //{
        //    GameControl.gameControl.endOfLevel = true;
        //    velocity.x = 1 * moveSpeed;
        //    UserInterface uI = GameObject.FindGameObjectWithTag("UserInterface").GetComponent<UserInterface>();
        //    uI.EndOfLevel();
        //}

        //Falling off the world
        if (collider.gameObject.layer == 19)
        {
            GameControl.playerCurrentHP = 0;
        }

        //Interactable Objects
        if (collider.gameObject.layer == 21)
        {
            //GameObject.FindGameObjectWithTag("UserInterface").GetComponent<UserInterface>().showInteractableDisplay = true;
        }

        if (collider.tag == "EnemyWeaponCollider")
        {
            //GameObject enemy = collider.transform.parent.gameObject;
            //CombatEngine.combatEngine.AttackingPlayer(enemy.GetComponent<Collider2D>(), enemy.GetComponent<EnemyStats>().maximumDamage);
        }
    }

    //this will be used to gauge interactions...I might need to do these things in the climbable script.
    public void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.layer == 13)
        {
            isClimbable = false;
        }

        //Interactable Objects
        if (collider.gameObject.layer == 14)
        {
            summiting = false;
        }
    }

    public void PauseAnimators()
    {
        animator.enabled = false;
        animator.spellAnimator.gameObject.GetComponent<Animator>().enabled = false;
        animator.bodyAnimator.gameObject.GetComponent<Animator>().enabled = false;
        animator.scarAnimator.gameObject.GetComponent<Animator>().enabled = false;
        animator.castingAnimator.gameObject.GetComponent<Animator>().enabled = false;
    }

    public void UnPauseAnimators()
    {
        animator.enabled = true;
        animator.spellAnimator.gameObject.GetComponent<Animator>().enabled = true;
        animator.bodyAnimator.gameObject.GetComponent<Animator>().enabled = true;
        animator.scarAnimator.gameObject.GetComponent<Animator>().enabled = true;
        animator.castingAnimator.gameObject.GetComponent<Animator>().enabled = true;
    }

    //called by the animator.
    public void FullyRevived()
    {
        //deathStanding = false;
    }

    public void UpdateCoolDownList()
    {
        for (int i = 0; i < cooldownList.Count; i++)
        {
            var key = cooldownList.ElementAt(i);
            int itemKey = key.Key;
            if (cooldownList[itemKey] <= 0)
            {
                cooldownList.Remove(itemKey);
            }
            else
            {
                cooldownList[itemKey] -= Time.deltaTime;
            }
        }
    }
}