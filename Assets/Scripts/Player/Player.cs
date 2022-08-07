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
    private bool inputDisable;
    
    
    //工具动画所需参数
    private float mouseX;
    private float mouseY;
    private bool useTool;

    private void Awake()
    {
        rb=GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
    }

    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition += OnMoveToPosition;
        EventHandler.MouseClickedEvent += OnMouseClickedEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition -= OnMoveToPosition;
        EventHandler.MouseClickedEvent -= OnMouseClickedEvent;
    }

    private void OnMouseClickedEvent(Vector3 mousepos, ItemDetails itemDetails)
    {
        if (itemDetails.itemType!=ItemType.Commodity&&itemDetails.itemType!=ItemType.Furniture&&itemDetails.itemType!=ItemType.Seed)
        {
            mouseX = mousepos.x - transform.position.x;
            mouseY = mousepos.y - (transform.position.y+0.85f);

            if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
                mouseY = 0;
            else
                mouseX = 0;
            StartCoroutine(UseToolRoutine(mousepos,itemDetails));
        }
        else
        {
            EventHandler.CallExecuteActionAfterAnimation(mousepos,itemDetails);
        }
        
    }

    private IEnumerator UseToolRoutine(Vector3 mousepos, ItemDetails itemDetails)
    {
        useTool = true;
        inputDisable = true;
        yield return null;
        foreach (var anim in animators)
        {
            anim.SetTrigger("useTool");
            anim.SetFloat("Inputx",mouseX);
            anim.SetFloat("Inputy",mouseY);
        }

        yield return new WaitForSeconds(0.45f);
        EventHandler.CallExecuteActionAfterAnimation(mousepos,itemDetails);
        yield return new WaitForSeconds(0.25f);
        useTool = false;
        inputDisable = false;
    }
    private void OnMoveToPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    private void OnAfterSceneLoadedEvent()
    {
        inputDisable = false;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        inputDisable = true;
    }
    
    private void Update()
    {
        if (inputDisable==false)
        {
            PlayerInput();
        }
        else
        {
            IsWalk = false;
        }
        SwitchAnimation();
    }

    private void FixedUpdate()
    {
        if (inputDisable == false)
        {
            Movement();
        }
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
            anim.SetFloat("mouseX",mouseX);
            anim.SetFloat("mouseY",mouseY);
            if (IsWalk)
            {
                anim.SetFloat("Inputx",inputX);
                anim.SetFloat("Inputy",inputY);
            }
        }
    }
}
