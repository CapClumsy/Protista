﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public List<GameObject> stackedPieces;
    // Variables for moving animation
    public float speed;
    public bool moving;
    private Vector3 target;
    public Vector3 stackingHeight;

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            // Move our position a step closer to the target
            // calculate distance to move
            float step =  speed * Time.deltaTime; 
            transform.position = Vector3.MoveTowards(transform.position, target, step);

            // Check if the position is about where it should be
            if (Vector3.Distance(transform.position, target) < 0.001f)
            {
                moving = false;
            }
        }
    }

    public void Move(Vector3 newHexPos, int newX, int newZ, bool stacking = false, GameObject stackingOnto = null, bool stackingAStack = false, bool bottomPiece = false, int origStackCount = 0)
    {
        // newHexPos:         Position of the hex the piece is moving onto
        // newX:              The board X position of the hex the piece is moving onto
        // newY:              The board Y position of the hex the piece is moving onto
        // stacking:          Whether the piece is stacking onto another piece during this movement
        // stackingOnto:      The piece this piece is stacking onto
        // stackingAStack:    Whether this piece is in a stack that is being added onto another piece or stack
        // currentStackCount: The amount of pieces in the stack that this piece is in

        // Reassign the piece's x and z values
        GetComponent<BoardPos>().z = newZ;
        GetComponent<BoardPos>().x = newX;
        // Set target
        target = newHexPos + new Vector3 (0f, transform.position.y, 0f);

        // Whether this is the bottom piece of a stack that is being moved onto another piece
        bool startingStackingAStack;
        int stackCount;

        if (stacking)
        {
            // How high offset the bottom stacking piece needs to be in the piece its moving onto is stacked
            if (stackingOnto.GetComponent<Piece>().stackedPieces.Count != 0)
            {
                if (stackingAStack)
                {
                    stackCount = origStackCount;
                }
                else
                {
                    stackCount = stackingOnto.GetComponent<Piece>().stackedPieces.Count;
                }
            }
            else
            {
                stackCount = 0;
            }
            Vector3 stackOffset = stackingHeight * stackCount;
            target += stackOffset;

            // Checks if this piece has stacked pieces and is not a piece in the middle of a stack
            // Begins a stack if so, doesn't if not
            if (stackedPieces.Count != 0 && !stackingAStack)
            {
                startingStackingAStack = true;
            }
            else
            {
                startingStackingAStack = false;
            }

            // Add piece to the list of stacked pieces on the piece it's being stacked on
            stackingOnto.GetComponent<Piece>().stackedPieces.Add(gameObject);

            // Assign target position based on the height of the stack the piece is moving onto
            // Since this method is recursive, every time it goes through each stacked piece, target gets added to each time 
            // It gets offset by stackingHeight each time.
            target += stackingHeight;
        }
        else
        {
            // Make sure startingStackingAStack doesn't go unassigned
            startingStackingAStack = false;
            stackCount = 0;
        }

        // Piece is now moving to its next position
        moving = true;

        // Repeats this for all stacked pieces
        foreach (GameObject piece in stackedPieces)
        {
            piece.GetComponent<Piece>().Move(newHexPos, newX, newZ, stacking: startingStackingAStack, stackingOnto: stackingOnto, stackingAStack: startingStackingAStack, origStackCount: stackCount);
        }

        // Things to do if this is the first piece in a stack that's moving
        if (startingStackingAStack)
        {
            // Reset list to empty
            stackedPieces = new List<GameObject>();
        }
    }    

    void OnCollisionEnter(Collision otherObj)
    {
        // If a piece collides with another piece of the opposite color 
        // and that piece is not moving (to prevent both pieces calling this function at the same and destroying each other at the same time)
        // the piece will destroy the other piece
        if ((otherObj.gameObject.tag == "black" || otherObj.gameObject.tag == "white") && otherObj.gameObject.tag != tag && !otherObj.gameObject.GetComponent<Piece>().moving)
        {
            Destroy(otherObj.gameObject);
        }
    }
}
