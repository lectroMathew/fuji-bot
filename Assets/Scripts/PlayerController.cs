﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    internal Rigidbody2D rigidBody;

    public GameController gameController;

    public enum STATE {
        FLYING,
        STICKED
    }

    public STATE currentState;

    public Vector3 moveDirection;

    public float speed;

    void Start()
    {
        moveDirection = Vector3.zero;
        currentState = STATE.STICKED;
        speed = gameController.levelNumber;
    }

    void Update()
    {
        GetInput();
        Move(moveDirection);
        LandingCheck(moveDirection);
    }

    void ChangeState(STATE state) {
        currentState = state;
        Debug.Log("Changed State: " + state);
    }

    void Move(Vector3 moveDir) {
        if (currentState == STATE.FLYING) {
            rigidBody.velocity = moveDir.normalized * speed * 4f;
        } else if (currentState == STATE.STICKED) {
            rigidBody.velocity = Vector3.up * speed;
        }
    }

    public LayerMask layerMask;
    void LandingCheck(Vector3 direction) {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Vector2 rayDirection = new Vector2(direction.x, direction.y);
        if (Physics2D.Raycast(origin, rayDirection, 3f, layerMask)) {
            RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection * 3f, 2f, layerMask);
            LandRotate(hit);

            #if UNITY_EDITOR
                Debug.DrawRay(transform.position, direction * 2f, Color.red); 
             #endif 

        }
    }

    void LandRotate(RaycastHit2D raycast) {
        Vector2 normal = raycast.normal;
        Vector3 targetPosition = new Vector3(normal.x, normal.y, 0f);
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, targetPosition);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3.5f);
    }

    void HandleSwipe(Vector2 rawSwipe) {
        
        Vector2 swipe = new Vector2();

        // We ignore y component of the swipe and add constant 200f
        // lenght, thus making one-hand play more comfortable.
        if (rawSwipe.y > 0) {
            swipe = new Vector2(rawSwipe.x, 200f);
        } else if (rawSwipe.y <= 0) {
            swipe = new Vector2(rawSwipe.x, -200f);
        }

        if (currentState == STATE.STICKED) {
            moveDirection = new Vector3(swipe.x, swipe.y, 0f).normalized;
            ChangeState(STATE.FLYING);
        }

        #if UNITY_EDITOR
            Debug.DrawLine(transform.position, new Vector3(swipe.x, swipe.y, 0f)); 
        #endif
    }

    private Vector2 startPosition;
    private Vector2 endPosition;

    void GetInput() {
        if (Input.GetMouseButtonDown(0)) {
            startPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0)) {
            endPosition = Input.mousePosition;
            Vector2 rawSwipe = endPosition - startPosition;

            HandleSwipe(rawSwipe);
        }
    }

    void Stick() {
        ChangeState(STATE.STICKED);
        moveDirection = Vector3.zero;
        collisionFramesCount = 0;
    }

    private int collisionFramesCount;
    private int lastCollisionFramesCount;

    void OnCollisionEnter2D(Collision2D collision) {
        Vector2 contactNormal = collision.GetContact(0).normal;
        Vector3 targetRotation = new Vector3(contactNormal.x, contactNormal.y, 0f);
        transform.rotation = Quaternion.LookRotation(Vector3.forward, targetRotation);
        
        Stick();
    }

    void OnCollisionStay2D(Collision2D collision) {
        collisionFramesCount++;
        if (collisionFramesCount > 20) {
            Stick();
        }
    }
}
