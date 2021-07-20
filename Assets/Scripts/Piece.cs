﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Piece : MonoBehaviour
{
    // List of stacked pieces
    public List<GameObject> stackedPieces;
    // Variables for moving animation
    public float speed;
    // Whether the piece is moving
    public bool moving;
    // Whether the piece can damage other pieces
    private bool canHit;
    // The height of a piece, how high each piece should stack
    public Vector3 stackingHeight;
    // Whether the piece is going to update its stack count once it stops moving
    public bool goingToUpdateStack;
    // The last position of the piece
    public Vector3 lastPosition;
    // The position that the piece needs to move to
    private List<Vector3> targets = new List<Vector3>();
    // Game manager
    private Board gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindObjectOfType<Board>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            // Move our position a step closer to the target
            // calculate distance to move
            float step = speed * Time.deltaTime; 
            transform.position = Vector3.MoveTowards(transform.position, targets[0], step);
            
            // Check if the position is about where it should be
            if (Vector3.Distance(transform.position, targets[0]) < 0.001f)
            {
                targets.RemoveAt(0);
                if (targets.Count == 0)
                {
                    // Stop piece
                    moving = false;
                }
            }
        }
        
        if (goingToUpdateStack)
        {
            UpdateStackCount();
        }
    }

    public void Move(
        // List of targets to move to
        List<BoardPos> targets,
        // Whether the piece is stacking onto another piece during this movement
        bool stacking = false, 
        // The piece this piece is stacking onto
        GameObject stackingOnto = null, 
        // Whether this piece is in a stack that is being added onto another piece or stack
        bool stackingAStack = false, 
        // Whether this piece is the only, or bottom piece in a stack
        bool bottomPiece = false, 
        // The original (before moving pieces on this stack into it) amount of pieces in the stack that this piece is moving onto
        int origStackCount = 0,
        // Whether this move is moving multiple spaces like cannon or v movements
        bool multipleHexMove = false,
        // The direction that the multiple hex move is going in
        int multipleHexDirection = 0
    )
    {
        BoardPos newPos = targets[targets.Count - 1];

        // Reassign board position if this piece is not attacking a stack or doing a multiple hex movement and bouncing off
        if (gameManager.hexDex[newPos.z, newPos.x].GetComponent<Hex>().piece.GetComponent<Piece>().stackedPieces.Count == 0 
            || stacking 
            || gameManager.hexDex[newPos.z, newPos.x].GetComponent<Hex>().piece.GetComponent<Piece>().stackedPieces.Contains(gameObject)
            || gameManager.hexDex[newPos.z, newPos.x].GetComponent<Hex>().piece == gameObject)
        {
            // Reassign the piece's x and z values
            GetComponent<BoardPos>().z = newPos.z;
            GetComponent<BoardPos>().x = newPos.x;
        }
        // If the piece is attacking a stack, is it doing a multiple hex move?
        else if (multipleHexMove)
        {
            // Reassign position to one hex short of the target
            BoardPos bouncingOnto;
            bouncingOnto = gameManager.hexDex[newPos.z, newPos.x].GetComponent<Hex>().neighbors[gameManager.GetOppositeDirection(multipleHexDirection)].GetComponent<BoardPos>();
            GetComponent<BoardPos>().x = bouncingOnto.x;
            GetComponent<BoardPos>().z = bouncingOnto.z;
        }

        foreach (BoardPos target in targets)
        {
            this.targets.Add(gameManager.hexDex[target.z, target.x].transform.position + new Vector3(0f, transform.position.y, 0f));
        }

        // Set last position
        if (!multipleHexMove)
        {
            lastPosition = transform.position;
        }
        else
        {
            lastPosition = gameManager.hexDex[newPos.z, newPos.x].GetComponent<Hex>().neighbors[gameManager.GetOppositeDirection(multipleHexDirection)].transform.position + new Vector3(0f, transform.position.y, 0f);
        }

        // Whether this is the bottom piece of a stack that is being moved onto another piece
        bool startingStackingAStack;
        // How high offset the bottom stacking piece needs to be in the piece its moving onto is stacked
        int stackCount;

        // Stacking 
        if (stacking)
        {
            // Checks if the piece this piece is stacking onto has pieces stacked onto it
            if (stackingOnto.GetComponent<Piece>().stackedPieces.Count != 0)
            {
                // Sets the original stack count if this piece is in a stack or the first piece in a stack
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

            // Make stack offset for stacking on a stack
            Vector3 stackOffset = stackingHeight * stackCount;
            for (int i = 0; i < this.targets.Count; i++)
            {
                this.targets[i] += stackOffset;
            }

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
            // It gets offset by stackingHeight each time
            for (int i = 0; i < this.targets.Count; i++)
            {
                this.targets[i] += stackingHeight;
            }
        }
        else
        {
            // Make sure startingStackingAStack doesn't go unassigned
            startingStackingAStack = false;
            stackCount = 0;
        }

        // Piece is now moving to its next position
        moving = true;
        // Piece can damage other pieces
        canHit = true;

        // Repeats this for all stacked pieces
        foreach (GameObject piece in stackedPieces)
        {
            piece.GetComponent<Piece>().Move
            (
                targets,
                stacking: startingStackingAStack, 
                stackingOnto: stackingOnto, 
                stackingAStack: 
                startingStackingAStack, 
                origStackCount: stackCount
            );
        }

        // Things to do if this is the first piece in a stack that's moving
        if (startingStackingAStack)
        {
            // Reset list to empty
            stackedPieces = new List<GameObject>();
        }

        if (bottomPiece && stacking)
        {
            stackingOnto.GetComponent<Piece>().goingToUpdateStack = true;
        }
    }

    void OnCollisionEnter(Collision otherObj)
    {
        Piece otherPiece = otherObj.gameObject.GetComponent<Piece>();
        if (
            // If a piece collides with another piece of the opposite color 
            (otherObj.gameObject.tag == "black" || otherObj.gameObject.tag == "white") && otherObj.gameObject.tag != tag 
            // and that piece is not moving (to prevent both pieces calling this function at the same and destroying each other at the same time)
            && !otherPiece.moving
            // and this piece the bottom of a stack or has no pieces on top of it
            && (stackedPieces.Count != 0 || transform.position.y == gameManager.pieceVertical.y)
            // and this is the final position the piece is going to go in
            && targets.Count == 1
            // and the piece can damage other pieces
            && canHit
        )
        {
            GameObject pieceToDestroy;
            // If attacking a stack
            if (otherPiece.stackedPieces.Count != 0)
            {
                // Set pieceToDestroy
                pieceToDestroy = otherPiece.stackedPieces[otherPiece.stackedPieces.Count - 1];
                // Remove pieceToDestroy from list of stacked pieces to prevent missing GameObjects in the list
                otherPiece.stackedPieces.Remove(pieceToDestroy);
                // Updates stack count for one less piece
                otherPiece.UpdateStackCount();
                // Update target to last position
                targets[0] = lastPosition; 
                // Piece cannot damage other pieces while moving back to last position
                canHit = false;
                foreach (GameObject piece in stackedPieces)
                {
                    piece.GetComponent<Piece>().targets[0] = piece.GetComponent<Piece>().lastPosition;
                }
            }
            // If attacking a single piece
            else
            {
                // Set pieceToDestroy
                pieceToDestroy = otherObj.gameObject;
            }
            // Destroy piece
            Destroy(pieceToDestroy);
        }
    }

    public void UpdateStackCount()
    {
        List<bool> stackMoving = new List<bool>();
        foreach (GameObject piece in stackedPieces)
        {
            bool otherPieceMoving = piece.GetComponent<Piece>().moving;
            if (otherPieceMoving)
            {
                stackMoving.Add(otherPieceMoving);
            }
        }
        if (!stackMoving.Contains(true))
        {
            // Get canvas
            GameObject canvas;
            if (stackedPieces.Count != 0) 
            {
                canvas = stackedPieces[stackedPieces.Count - 1].transform.GetChild(0).gameObject;
            }
            else
            {
                canvas = transform.GetChild(0).gameObject;
            }

            // Get text
            TextMeshProUGUI text = canvas.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();

            // Hide all the number for pieces that are stacked 
            foreach (GameObject piece in stackedPieces)
            {
                // Hide canvas
                piece.transform.GetChild(0).gameObject.SetActive(false);
            }

            if (stackedPieces.Count != 0)
            {
                canvas.SetActive(true);
                text.text = (stackedPieces.Count + 1).ToString();
            }
            else
            {
                canvas.SetActive(false);
            }

            goingToUpdateStack = false;
        }
    }
}
