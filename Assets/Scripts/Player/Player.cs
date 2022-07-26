using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed;
    private float inputX;
    private float inputY;
    private Vector2 movementInput;
    private Animator[] animators;
    private bool IsWalk;

    private void Awake()
    {
        rb=GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
    }

    private void Update()
    {
        PlayerInput();
        SwitchAnimation();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void PlayerInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(KeyCode.LeftControl))
        {
            inputX = inputX * 0.5f;
            inputY = inputY * 0.5f;
        }
        if (inputX!=0&&inputY!=0)
        {
            inputX = inputX * 0.5f;
            inputY = inputY * 0.5f;
        }
        movementInput = new Vector2(inputX, inputY);
        IsWalk = movementInput != Vector2.zero;
    }

    private void Movement()
    {
        rb.MovePosition(rb.position+movementInput*speed*Time.deltaTime);
    }

    private void SwitchAnimation()
    {
        foreach (var anim in animators)
        {
            anim.SetBool("IsWalk",IsWalk);
            if (IsWalk)
            {
                anim.SetFloat("Inputx",inputX);
                anim.SetFloat("Inputy",inputY);
            }
        }
    }
}
