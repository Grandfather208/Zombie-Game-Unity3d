﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playermovement : MonoBehaviour
{
    private Transform firstPerson_View;
    private Transform firstPerson_Cam;

    private Vector3 firstPerson_view_Rotation = Vector3.zero;

    public float walkSpeed = 6.75f;
    public float runSpeed = 10f;
    public float crouchSpeed = 4f;
    public float jumpSpeed = 8f;
    public float gravity = 20f;

    private float speed;

    private bool is_Moving, is_Grounded, is_Crouching;

    private float inputX, inputY;
    private float inputX_Set, inputY_Set;
    private float inputModifyFactor;

    private bool limitDiagonalSpeed = true;

    private float antoBumpFactor = 0.75f;

    private CharacterController charController;
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
    	firstPerson_View = transform.Find("fps cam").transform;
    	charController = GetComponent<CharacterController>();
    	speed = walkSpeed;
    	is_Moving = false;
    }

    void Update()
    {
    	PlayerMovement();
    }

    void PlayerMovement()
    {
    	if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
    	{
    		if(Input.GetKey(KeyCode.W))
    		{
    			inputY_Set = 1f;
    		}
    		else
    		{
    			inputY_Set = -1f;
    		}
    	}
    	else
    	{
    		inputY_Set = 0f;
    	}

    	if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
    	{
    		if(Input.GetKey(KeyCode.A))
    		{
    			inputX_Set = -1f;
    		}
    		else
    		{
    			inputX_Set = 1f;
    		}
    	}
    	else
    	{
    		inputX_Set = 0f;
    	}

    	inputY = Mathf.Lerp(inputY, inputY_Set, Time.deltaTime * 19f);
    	inputX = Mathf.Lerp(inputX, inputX_Set, Time.deltaTime * 19f);

    	inputModifyFactor = Mathf.Lerp(inputModifyFactor, (inputY_Set != 0 && inputX_Set != 0 && limitDiagonalSpeed) ? 0.75f : 1.0f, Time.deltaTime * 19f);

    	firstPerson_view_Rotation = Vector3.Lerp(firstPerson_view_Rotation, Vector3.zero, Time.deltaTime * 5f);
    	firstPerson_View.localEulerAngles = firstPerson_view_Rotation;

    	if(is_Grounded)
    	{
    		moveDirection = new Vector3(inputX * inputModifyFactor, -antoBumpFactor, inputY * inputModifyFactor);
    		moveDirection = transform.TransformDirection(moveDirection) * speed;
    	}
    	moveDirection.y -= gravity * Time.deltaTime;

    	is_Grounded = (charController.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
    	is_Moving = charController.velocity.magnitude > 0.15f;
    }


}
