using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region Serialized fields for setting in the editor
    [SerializeField]
    public Board board;
    [SerializeField]
    private Camera cam;

    #region Game pieces
    [Header("Game Pieces")]
    [SerializeField]
    private GameObject whiteHexPrefab;
    [SerializeField]
    private GameObject blackHexPrefab;
    [SerializeField]
    private GameObject whiteObjectiveHexPrefab;
    [SerializeField]
    private GameObject blackObjectiveHexPrefab;
    [SerializeField]
    private GameObject neutralHexPrefab;
    [SerializeField]
    private GameObject whitePiecePrefab;
    [SerializeField]
    private GameObject blackPiecePrefab;
    #endregion

    #region Icons
    [Header("Icons")]
    [SerializeField]
    private GameObject movementArrow;
    [SerializeField]
    private GameObject attackIcon;
    [SerializeField]
    private GameObject stackIcon;
    [SerializeField]
    private GameObject hoveringPrism;
    #endregion

    #region UI
    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI invalidMovementOptionText;
    [SerializeField]
    private TextMeshProUGUI turnCounterText;
    [SerializeField]
    private TextMeshProUGUI moveCounterText;
    [SerializeField]
    private TextMeshProUGUI maxMoveCounterText;
    [SerializeField]
    private Animator cameraAnimator;
    [SerializeField]
    private Animator invalidMovementOptionAnimator;
    [SerializeField]
    private Animator whiteTurnAnimator;
    [SerializeField]
    private Animator blackTurnAnimator;
    [SerializeField]
    private Animator turnCounterAnimator;
    [SerializeField]
    private Animator moveCounterAnimator;
    [SerializeField]
    private Animator maxMoveCounterAnimator;
    [SerializeField]
    private GameObject[] buttons;
    #endregion

    #region Game behavior variables for tweaking
    [Header("Game behavior variables for tweaking")]
    [SerializeField]
    // The number of max moves that a turn starts with
    private int baseMaxMoves;
    [SerializeField]
    // The maximum number of extra moves that can be gained from completing loops in one movemovemovemove
    private int maxExtraMovesPerMove;
    [SerializeField]
    // The number of pieces to generate during the random generation move
    private int randomPieceNum;
    [SerializeField]
    // The number of seconds between generating new pieces and switching moves
    private float randomGenerationWaitTime;
    // Number of objective hexes for each player
    private int objHexNum;
    // Objective hexes in the board
    private Dictionary<string, List<GameObject>> objectiveHexes = new Dictionary<string, List<GameObject>> {
        {"white", new List<GameObject>()},
        {"black", new List<GameObject>()}
    };
    [SerializeField]
    // The ratio of objective hexes that need to be occupied to win
    private float objHexRatio;
    // Hexes between each player's side
    private int rows = (int) Layout.standard.Rows;
    [SerializeField]
    // The way/distance hexes are tiled from left to right
    private Vector3 rowSpace;
    // Hexes to the right and left of player
    private int columns = (int) Layout.standard.Columns;
    [SerializeField]
    // The horizontal/z offset when hexes are tiled from top to bottom
    private Vector3 columnSpace;
    [SerializeField]
    // The offset of every other row
    private Vector3 rowOffset;
    // Number of pieces for each player
    private int pieceNum;
    [SerializeField]
    // Vertical offset of each piece
    public Vector3 pieceVertical;
    [SerializeField]
    // The height of a piece, how high each piece should stack
    public Vector3 stackingHeight;
    [SerializeField]
    // Vertical offset of the movement icons
    private Vector3 movementIconVertical;
    [SerializeField]
    // Vertical offset of the movement arrows
    private Vector3 movementArrowVertical;
    [SerializeField]
    // The amount that the amount of rows or columns are multiplied to get the vertical position of the camera
    private float cameraVerticalMultiplier;
    #endregion
    #endregion

    #region Variables for use during generation and gameplay
    // The current turn
    private int turnCount = 0;
    [SerializeField]
    // Can be either "black" or "white" to correpond to the piece tags
    // Whatever this is set to in the editor, the game will start with the opposite one
    private string turnColor;
    // The number of moves that have been taken so far this turn
    private int movesTaken = 0;
    // The maximum number of moves that can be taken on this turn
    private int maxMoves;
    // Selected hexes
    private List<GameObject> selected = new List<GameObject>();
    [NonSerialized]
    // The pieces that are moving
    public List<Piece> movingPieces = new List<Piece>();
    // The elapsed time of the match
    private Stopwatch stopwatch = new Stopwatch();

    #region Variables for finding loops at the end of each turn
    // The pieces that have been examined on the whole board over this floodfill iteration
    private List<BoardPosition> hexesFilled = new List<BoardPosition>();
    // The number of the loops in each color
    private Dictionary<string, int> previousMoveLoops = new Dictionary<string, int> {
        {"white", 0},
        {"black", 0}
    };
    #endregion

    // Movement arrow object currently in use
    private Dictionary<string, List<GameObject>> movementIcons;
    // Template for empty movement icons variable
    private Dictionary<string, List<GameObject>> emptyMovementIcons = new Dictionary<string, List<GameObject>> {
        {"arrows", new List<GameObject>()},
        {"attack", new List<GameObject>()},
        {"stack",  new List<GameObject>()}
    };
    // The hexes that a move would take a piece along if it were to move to a certain hex
    private Dictionary<GameObject, List<BoardPosition>> stepsTo = new Dictionary<GameObject, List<BoardPosition>>();

    #region Variables specific to movement types
    #region Variables for contiguous movement
    // The valid hexes and the directions taken to them
    // For each hex there is a list of the lists of directions that it took to get there
    private Dictionary<GameObject, List<List<int>>> contiguousHexes = new Dictionary<GameObject, List<List<int>>>();
    // The hexes with pieces visited
    private List<GameObject> contiguousVisits;
    // The directions it has taken to get to a hex
    private List<int> directionsList = new List<int>();
    #endregion

    #region Variables for wave movement
    // The wave that is selected (this should have the same elements as selected, but should be in order)
    private List<GameObject> wave = new List<GameObject>();
    // The two directions perpendicular to the wave
    private int[] perpendicularDirections = new int[2];
    // The worst status on each side
    private Dictionary<int, int> worstStatuses = new Dictionary<int, int>();

    // Dictionary containing whether or not each piece in a wave movement that was going to damage another piece has completed its movement 
    public Dictionary<GameObject, bool> waveDamageCompleted = new Dictionary<GameObject, bool>();
    [NonSerialized]
    // Whether or not the wave should continue through or bounce off
    public bool waveBouncingOff;
    #endregion

    #region Movement option chosen
    // Whether a movement option is chosen at all
    private bool selectedMoving = false;
    private MovementType movementType;
    #endregion
    #endregion
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Start stopwatch
        stopwatch.Start();

        // Decide variables based on whether a custom layout was provided or not
        if (MainMenu.layout != null)
        {
            board.layout = MainMenu.layout;
        }
        else
        {
            // Set rows and columns to be the ones set in the editor
            board.layout = new Layout{
                Rows = rows,
                Columns = columns
            };
        }
        
        // If the seed was not set, set it
        if (board.layout.Seed == null)
        {
            board.layout.Seed = Environment.TickCount;
        }

        // Set board size based on layout
        // Row and columns were set to defaults if not specified during the display in the main menu
        rows = (int) board.layout.Rows;
        columns = (int) board.layout.Columns;
        // Initialize hexDex as 2D array with size of rows and columns specified
        board.hexDex = new GameObject[rows, columns];

        // halfBoard = the number of rows that make up half the board, minus the middle row 
        int halfBoard = rows / 2;
        // Initialize random
        System.Random random = new System.Random((int) board.layout.Seed);

        #region Generate objective hex arrangement
        // Makes an array that has whether or not an objective hex needs to be generated in a coordinate, then makes all values false
        bool[,] objHexes = new bool[rows, columns];
        // If no objective hex layout was specified by the custom layout or no layout was selected
        if (board.layout.ObjectiveHexes == null)
        {
            // Set the new objective hex num if it is specified
            objHexNum = board.layout.ObjectiveHexNum == null ? (int) Layout.standard.ObjectiveHexNum : (int) board.layout.ObjectiveHexNum;
            board.layout.ObjectiveHexNum = objHexNum;
            // Set size of objective hex list to be twice the size of the objective hex num set in the editor to account for both sides
            board.layout.ObjectiveHexes = new int[2 * objHexNum][];
            // Generates the needed number of objective hexes on each side
            for (int i = 0; i < objHexNum; i++)
            {
                // Chooses the rows that belong to each player
                // If the position is already occupied by a previously generated objective hex, it will go through the do while loop again
                (int Z, int X) position;
                do
                {
                    // Choose position on one half of the board
                    position.X = random.Next(0, columns);
                    position.Z = random.Next(0, halfBoard);
                    // Add objective hex at chosen position
                    objHexes[position.Z, position.X] = true;
                    board.layout.ObjectiveHexes[i] = new int[] {position.Z, position.X};
                    // Mirror objective hexes across board
                    position.Z = (rows - 1) - position.Z;
                    // Add objective hex at mirrored position
                    objHexes[position.Z, position.X] = true;
                    board.layout.ObjectiveHexes[i + objHexNum] = new int[] {position.Z, position.X};
                }
                while (!objHexes[position.Z, position.X]);
            }
        }
        else
        {
            // Translate objective hexes specified in layout to bool[] objHexes
            foreach (int[] objHex in board.layout.ObjectiveHexes)
            {
                objHexes[objHex[0], objHex[1]] = true;
            }
        }
        #endregion

        #region Generate piece arrangement
        // Makes an array that has whether or not a piece needs to be generated in a coordinate, then makes all values false
        List<PieceInfo> pieces;
        // If no objective hex layout was specified by the custom layout or no layout was selected
        if (board.layout.Pieces == null)
        {
            // Initalize pieces
            pieces = new List<PieceInfo>();
            // Set the new piece num if it is specified
            pieceNum = board.layout.PieceNum == null ? (int) Layout.standard.PieceNum : (int)board.layout.PieceNum;
            // Generates the needed number of pieces on each side
            for (int i = 0; i < pieceNum; i++)
            {
                // Chooses the rows that belong to each player
                // If the position is already occupied by a previously generated piece, it will go through the do while loop again
                int xPos, zPos, samePositions;
                do
                {
                    // Choose position on one half of the board
                    xPos = random.Next(0, columns);
                    zPos = random.Next(0, halfBoard);
                    // Get list of pieces that have the same position as the selected one to make sure that two pieces aren't generated on top of each other
                    samePositions =
                        (from piece in pieces 
                        where piece.Position[0] == zPos && piece.Position[1] == xPos
                        select piece).Count();
                }
                // There should only be one piece with the same position as the one we just chose
                while (samePositions > 0);

                // Add piece at chosen position
                pieces.Add(new PieceInfo {
                    Position = new int[] {zPos, xPos},
                    Stacked = 0,
                    White = true
                });
                // Mirror pieces across board
                zPos = (rows - 1) - zPos;
                // Add piece at mirrored position
                pieces.Add(new PieceInfo {
                    Position = new int[] {zPos, xPos},
                    Stacked = 0,
                    White = false
                });
            }
        }
        else
        {
            pieces = board.layout.Pieces;
        }
        #endregion

        #region Generate gameboard
        // Set up hex prefab array
        // First dimension is color, second is hex type (normal/objective)
        // 0 = white, 1 = black, 2 = neutral
        // 0 = normal, 1 = objective
        GameObject[,] hexPrefabs = new GameObject[3, 2];
        hexPrefabs[0, 0] = whiteHexPrefab;
        hexPrefabs[0, 1] = blackObjectiveHexPrefab;
        hexPrefabs[1, 0] = blackHexPrefab;
        hexPrefabs[1, 1] = whiteObjectiveHexPrefab;
        hexPrefabs[2, 0] = neutralHexPrefab;
        
        // lastPosition = the last place we spawned in a hex, we'll then add some vectors to it to get our new position and spawn a new hex there
        Vector3 lastPosition = new Vector3(0f, 0f, 0f);
        // Whether or not the first hex in the last row generated was offsetted to the right
        bool lastWentRight = true;
        // Loop through each row and hex in each row
        // Dimension 1
        for (int z = 0; z < rows; z++)
        {
            // Dimension 2
            for (int x = 0; x < columns; x++)
            {
                // Hex and piece that are going to be placed
                GameObject hexToPlace;

                #region Choose colors
                // Color for top of hex
                int color;
                // Color on base of hex
                int type;
                
                // If on the first half
                // Color is white
                if (z < halfBoard)
                {
                    color = 0;
                }
                // If z is one more than half the board and the number of rows is odd
                // Color is neutral
                else if (z == halfBoard && rows % 2 == 1)
                {
                    color = 2;
                }
                // If on the second half
                // Color is black
                else
                {
                    color = 1;
                }
                
                // Choose to place normal or objective hex based on earlier generation
                if (objHexes[z, x])
                {
                    type = 1;
                }
                else
                {
                    type = 0;
                }
                // Choose hex to place from color and type
                hexToPlace = hexPrefabs[color, type];
                #endregion

                #region Spawn hexes
                // Spawn hex, add correct board position, and add to Hex object
                GameObject hexSpawned = Instantiate(hexToPlace, lastPosition, Quaternion.Euler(0f, 30f, 0f));
                // Add hex to list of objective hexes if it is one
                if (type == 1)
                {
                    objectiveHexes[color == 0 ? "black" : "white"].Add(hexSpawned);
                }
                // Cache board position
                BoardPosition hexPosition = hexSpawned.GetComponent<BoardPosition>();
                hexPosition.x = x;
                hexPosition.z = z;
                board.hexDex[z, x] = hexSpawned;
                #endregion

                // Offets next hex position (for next time through loop)
                lastPosition += rowSpace;
            }
            // Resets the x position of the first hex in the row to 0 and then adds the column space
            lastPosition.Set(0f, lastPosition.y, lastPosition.z);
            lastPosition += columnSpace;
            // Offsets first hex (for next time through loop)
            if (lastWentRight) 
            {
                lastPosition += rowOffset;
                lastWentRight = false;
            } 
            else 
            {
                lastWentRight = true; 
            }
        }
        // Place pieces on hexes
        // Loop through each piece information that was created or provided
        foreach (PieceInfo piece in pieces)
        {
            // The hex this piece is going on
            GameObject hex = board.hexDex[piece.Position[0], piece.Position[1]];
            // Choose which color piece to place
            GameObject pieceToPlace = piece.White ? whitePiecePrefab : blackPiecePrefab;

            // Spawn piece above hex, add correct board position, and place in hexDex
            GameObject pieceSpawned = Instantiate(pieceToPlace, hex.transform.position + pieceVertical, Quaternion.identity);
            // Cache board position
            BoardPosition piecePosition = pieceSpawned.GetComponent<BoardPosition>();
            // Set board position
            piecePosition.z = piece.Position[0];
            piecePosition.x = piece.Position[1];
            hex.GetComponent<Hex>().piece = pieceSpawned;

            // Stack pieces on if necessary
            for (int i = 0; i < piece.Stacked; i++)
            {
                // Spawn piece at correct stacked height 
                GameObject stackedPiece = Instantiate(pieceToPlace, hex.transform.position + pieceVertical + (stackingHeight * (i + 1)), Quaternion.identity);
                // Cache board position
                BoardPosition stackedPosition = stackedPiece.GetComponent<BoardPosition>();
                // Set board position
                stackedPosition.z = piece.Position[0];
                stackedPosition.x = piece.Position[1];
                // Parent to bottom piece
                stackedPiece.GetComponent<Piece>().ParentTo(pieceSpawned.transform);
            }
            // Update stack count
            pieceSpawned.GetComponent<Piece>().UpdateStackCount();
        }
        #endregion

        #region Let hexes know their neighboring hexes
        int vertLeft;
        int vertRight;
        // These two variables determine what the top left and top right are, since they change depending on which row and the offset is
        int transVert = 0;
        int transHoriz = 0;
        // Loop through each row
        for (int z = 0; z < board.hexDex.GetLength(0); z++)
        {
            // Determine what top left and top right are based on whether it's an odd or even row
            vertLeft = -1 + z % 2;
            vertRight = 0 + z % 2;

            // Loop through each hex in each row
            for (int x = 0; x < board.hexDex.GetLength(1); x++)
            {
                GameObject[] neighbors = new GameObject[6];
                board.hexDex[z, x].GetComponent<Hex>().neighbors = neighbors;
                // Assign for each of the six surrounding hexes
                for (int direction = 0; direction < 6; direction++)
                {
                    // Assign a vertical translation based on top or bottom position
                    if (Board.DirectionIsBottom(direction))
                    {
                        transVert = -1;
                    }
                    else if (Board.DirectionIsMiddle(direction))
                    {
                        transVert = 0;
                    }
                    else if (Board.DirectionIsTop(direction))
                    {
                        transVert = 1;
                    }

                    // Assign a horizontal translation based on if we're moving up or not and whether we're moving left or right
                    if (Board.DirectionIsBottom(direction) || Board.DirectionIsTop(direction))
                    {
                        if (Board.DirectionIsRight(direction))
                        {
                            transHoriz = vertRight;
                        }
                        else
                        {
                            transHoriz = vertLeft;
                        }
                    }
                    else
                    {
                        if (Board.DirectionIsRight(direction))
                        {
                            transHoriz = 1;
                        }
                        else
                        {
                            transHoriz = -1;
                        }
                    }
                    GameObject neighborHex;
                    // Makes sure that hexes on the edge get defined as null
                    if (!((x + transHoriz < 0 || x + transHoriz >= columns) || (z + transVert < 0 || z + transVert >= rows)))
                    {
                        neighborHex = board.hexDex[z + transVert, x + transHoriz];
                    }
                    else
                    {
                        neighborHex = null;
                    }
                    // Assigns the neighbor hex if there is one
                    board.hexDex[z, x].GetComponent<Hex>().neighbors[direction] = neighborHex;
                }
            }
            
        }
        #endregion

        // Centers camera with generated board
        cam.transform.position += 
            // The middle point of the rows
            (rowOffset / 2) + rowSpace * ((float)(columns - 1) / 2) + 
            // The middle point of the colums 
            columnSpace * ((float)(rows - 1) / 2) + 
            // The vertical position of the camera based on board size
            new Vector3(0, (float) Math.Max(rows, columns * 1.1) * cameraVerticalMultiplier, 0);
            // new Vector3(0, Math.Max(rows, columns) * cameraVerticalMultiplier + (float) Math.Pow(1.15, -2.5 * (Math.Max(rows, columns) - 15)), 0);

        // Start the first turn
        StartNewTurn();
    }

    #region Input listeners
    public void OnSelect()
    {
        GameObject hexSelected = hoveringPrism.GetComponent<HoveringPrism>().hoveringOver;
        if (hexSelected != null && hoveringPrism.activeSelf == true && (Gamepad.current == null || (!Gamepad.current.leftShoulder.isPressed && !Gamepad.current.rightShoulder.isPressed)))
        {
            int color = hexSelected.GetComponent<cakeslice.Outline>().color;
            // If the player has not selected a movement option and there is a piece on the hex and no pieces are moving
            if (!selectedMoving && hexSelected.GetComponent<Hex>().piece != null && hexSelected.GetComponent<Hex>().piece.tag == turnColor && movingPieces.Count == 0)
            {
                // Makes sure outline color is selection color
                hexSelected.GetComponent<cakeslice.Outline>().color = 0;
                // Adds to list of selected if it's not selected, remove if it is
                if (!selected.Contains(hexSelected))
                {
                    selected.Add(hexSelected);
                }
                else
                {
                    selected.Remove(hexSelected);
                }
                // Toggles outline
                hexSelected.GetComponent<cakeslice.Outline>().enabled = selected.Contains(hexSelected);
            }
            else
            {
                // When you click the movement option button, the correct options are highlighted green
                // Checks if hex clicked is highlighted green which would mean that you can move there
                if (hexSelected.GetComponent<cakeslice.Outline>().enabled && (color == 1 || color == 2))
                {
                    // Checks movement option and executes proper move when clicked
                    if 
                    (
                        movementType == MovementType.Single
                        || movementType == MovementType.Cannon
                        || movementType == MovementType.V
                        || movementType == MovementType.Unstack
                    ) 
                    {
                        // Move piece
                        selected[0].GetComponent<Hex>().piece.GetComponent<Piece>().Move(stepsTo[hexSelected], movementType);
                    }
                    else if (movementType == MovementType.Wave) 
                    {
                        // Get direction
                        int direction = FindWaveDirection(hexSelected);
                        // Decide whether the wave is bouncing off or not
                        waveBouncingOff = worstStatuses[direction] == 1;
                        // Reset damage statuses
                        waveDamageCompleted = new Dictionary<GameObject, bool>();
                        // Go through every hex in the wave and move it
                        foreach (GameObject hex in wave)
                        {
                            // Cache piece GameObject
                            GameObject piece = hex.GetComponent<Hex>().piece;
                            // Move hex to board position one step in the direction
                            piece.GetComponent<Piece>().Move(
                                new List<BoardPosition> {hex.GetComponent<Hex>().neighbors[direction].GetComponent<BoardPosition>()},
                                movementType
                            );
                        }
                    }
                    else if (movementType == MovementType.Contiguous) 
                    {
                        // Get smallest directions list
                        List<int> directionList = GetDirectionsTo(hexSelected);
                        // Start with source hex
                        GameObject hex = selected[0];
                        // Initalize targets
                        List<BoardPosition> targets = new List<BoardPosition>();
                        foreach (int direction in directionList)
                        {
                            hex = hex.GetComponent<Hex>().neighbors[direction];
                            targets.Add(hex.GetComponent<BoardPosition>());
                        }
                        // Move piece
                        selected[0].GetComponent<Hex>().piece.GetComponent<Piece>().Move(targets, MovementType.Contiguous);
                        // Reset variables
                        contiguousHexes  = new Dictionary<GameObject, List<List<int>>>();
                        contiguousVisits = new List<GameObject>();
                        directionsList   = new List<int>();
                    }
                    EndSelection();
                    KillAllMovementIcons();
                }
            }
        }
    }

    public void OnDeselectAll()
    {
        EndSelection();
    }

    public void OnMoveHover(InputValue inputValue)
    {
        // Get value and invert if board is turned
        Vector2 change = (Vector2) inputValue.Get() * (turnColor == "white" ? 1 : -1);
        // Make sure we don't do anything if this is when the button is released
        if (change.magnitude != 0)
        {
            // Get prism
            HoveringPrism prism = hoveringPrism.GetComponent<HoveringPrism>();
            // If this is the first time and nothing has been hovered over yet, start at the middle of the board
            GameObject hoveringOver;
            if (prism.hoveringOver == null)
            {
                hoveringOver = board.hexDex[rows / 2, columns / 2];
            }
            else
            {
                hoveringOver = prism.hoveringOver;
            }

            // Get angle
            float angle = Vector2.Angle(new Vector2(1, 0), change);
            // Get angle within the correct 0-360 range if it is below 0 degrees
            if (change.y > 0)
            {
                angle = -angle + 360;
            }
            // Shift by 30 degrees because the right direction is split with 30 degrees above and 30 degrees below
            angle += 30;
            // Mod by 360 to ensure nothing is above 360
            angle %= 360;
            // Divide angle into 6 sectors and reverse sector because directions go clockwise
            int sector = (int) (angle / 60);

            // See if the hex in this direction exists
            if (hoveringOver.GetComponent<Hex>().neighbors[sector] != null)
            {
                // Get new hex in the direction determined
                GameObject newHex = hoveringOver.GetComponent<Hex>().neighbors[sector];
                // Tell prism to hover over new hex
                prism.HoverOver(newHex.GetComponent<BoardPosition>());
                // Update movement icons on this hex
                KillAllMovementIcons();
                PlaceMovementIcons(newHex.GetComponent<BoardPosition>());
            }
        }
    }
    #endregion

    #region Functions for utility
    public void PlaceMovementIcons(BoardPosition position)
    {
        // Get hex
        GameObject hex = board.hexDex[position.z, position.x];

        // Cache color
        int color = hex.GetComponent<cakeslice.Outline>().color;

        // Movement icons
        // If we're selecting a move and the hex hit is highlighted a valid color or there is already a movement icon
        if (selectedMoving && (hex.GetComponent<cakeslice.Outline>().enabled && (color == 1 || color == 2)))
        {
            // Initialize movementIcons and keys
            movementIcons = emptyMovementIcons;

            if (movementType == MovementType.Single)
            {
                // If there's no piece on moused over hex 
                if (hex.GetComponent<Hex>().piece == null
                    // Or if the piece on the moused over hex is the opposite color and not stacked
                    || (hex.GetComponent<Hex>().piece.tag != turnColor
                        && hex.GetComponent<Hex>().piece.transform.childCount <= 1)
                    // Or if the piece on the moused over hex is the same color as the selected piece
                    || hex.GetComponent<Hex>().piece.tag == turnColor)
                // Otherwise display only movement icon
                {
                    // Place arrow
                    PlaceArrow(selected[0], hex);

                    // Spawn aditional icons
                    // If there is a piece on the hex
                    if (hex.GetComponent<Hex>().piece != null)
                    {
                        // Cache piece
                        GameObject piece = hex.GetComponent<Hex>().piece;
                        // Type of icon to choose
                        string key;
                        // If the piece moused over is the opposite color
                        // (We shouldn't have to check for it being stacked since we did that already)
                        if (piece.tag != turnColor)
                        {
                            key = "attack";
                        }
                        // If the piece moused over is the same color
                        else
                        {
                            key = "stack";
                        }
                        PlaceIcon(hex, key);
                    }
                }
                else
                {
                    PlaceIcon(hex);
                }
            }
            else if (movementType == MovementType.Cannon || movementType == MovementType.V || movementType == MovementType.Unstack)
            {
                List<BoardPosition> steps = stepsTo[hex];
                // Place arrows up to hit hex
                for (int i = 0; i < steps.Count - 1; i++)
                {
                    // Get hex
                    GameObject nextHex = board.hexDex[steps[i + 1].z, steps[i + 1].x];
                    // Cache hex component
                    Hex hexComponent = nextHex.GetComponent<Hex>();
                    // If there are pieces on this hex and it's not the selected hex
                    if (hexComponent.piece != null)
                    {
                        // Add attack icon
                        PlaceIcon(nextHex);
                        // If the pieces on this hex are stacked then don't place arrow
                        if (hexComponent.piece.transform.childCount > 1)
                        {
                            break;
                        }
                    }
                    // Place arrow between current and next hex
                    PlaceArrow(board.hexDex[steps[i].z, steps[i].x], nextHex);
                }
            }
            else if (movementType == MovementType.Wave)
            {
                // Find which side of the wave this hex is on
                // The side of the wave this is on
                int direction = FindWaveDirection(hex);
                // The status on this side of the wave
                int status = worstStatuses[direction];
                // Place movement icons for each hex in the wave
                foreach (GameObject waveHex in wave)
                {
                    // Get hex on the perpendicularDirection side of the wave
                    GameObject perpendicularHex = waveHex.GetComponent<Hex>().neighbors[direction];
                    // Make sure the hex on this side exists
                    if (perpendicularHex != null)
                    {
                        // If there is a piece on the hex
                        if (perpendicularHex.GetComponent<Hex>().piece != null)
                        {
                            // Place attack icon
                            PlaceIcon(perpendicularHex);
                        }
                        // If the status is bouncing off, do not place movement arrows
                        if (status != 1)
                        {
                            // Place movement arrow between hex in the wave and perpendicular hex
                            PlaceArrow(waveHex, perpendicularHex);
                        }
                    }
                }
            }
            else if (movementType == MovementType.Contiguous)
            {
                // Get shortest list of directions
                List<int> directionList = GetDirectionsTo(hex);
                // Go through list of directions and place arrows
                // Start with selected hex
                GameObject nextHex = selected[0];
                // Do not place arrows if hitHex has a stack on it
                // Should only trigger for last hex in the list
                if (!(hex.GetComponent<Hex>().piece != null
                    && hex.GetComponent<Hex>().piece.tag != turnColor
                    && hex.GetComponent<Hex>().piece.transform.childCount > 1))
                {
                    foreach (int direction in directionList)
                    {
                        // Place arrow
                        PlaceArrow(nextHex, nextHex.GetComponent<Hex>().neighbors[direction]);
                        // Update hex for next time through the loop
                        nextHex = nextHex.GetComponent<Hex>().neighbors[direction];
                    }
                }
                // Place attack icons
                // Because of our highlighting algorithm, hexes with pieces of the same color should not be highlighted so we don't need to worry
                if (hex.GetComponent<Hex>().piece != null)
                {
                    PlaceIcon(hex);
                }
            }

        }
    }

    /// <summary>Destroys all movement icon <c>GameObject</c>s currently in scene</summary>
    public void KillAllMovementIcons()
    {
        if (movementIcons != null)
        {
            foreach (string type in movementIcons.Keys)
            {
                foreach (GameObject icon in movementIcons[type])
                {
                    GameObject.Destroy(icon);
                }
            }
            movementIcons = null;
        }
    }  

    /// <summary>Deselects all hexes currently selected</summary>
    private void DeselectAllHexes()
    {
        // Iterate through each selected hex
        foreach (GameObject hex in selected)
        {
            // Turn off outline
            hex.GetComponent<cakeslice.Outline>().enabled = false;
        }
        // Resets selected back to empty list
        selected = new List<GameObject>();
    }

    /// <summary>Place movement arrow from <paramref>hex1</paramref> to <paramref>hex2</paramref></summary>
    /// <param name = "hex1">the hex the arrow will point from</param>
    /// <param name = "hex2">the hex the arrow will point to</param>
    private void PlaceArrow(GameObject hex1, GameObject hex2)
    {
        // Get the position the arrow will be in
        // Average position of the two hexes plus the vertical offset
        Vector3 iconPosition = ((hex1.transform.position + hex2.transform.position) / 2) + movementArrowVertical;
        // Calculate relative position
        Vector3 relativePosition = hex2.transform.position - hex1.transform.position;
        // Calculate relative angle
        float angle = - (float) (Math.Atan2(relativePosition.z, relativePosition.x) * (180/Math.PI));
        // Spawn movement arrow
        movementIcons["arrows"].Add(Instantiate(movementArrow, iconPosition, Quaternion.Euler(-90f, 0f, angle)));
    }

    /// <summary>Places a movement icon above a hex</summary>
    /// <param name = "hex">the hex to place the movement icon above</param>
    /// <param name = "type">the type of movement icon to place; can be either <c>"attack"</c> (default value) for the attacking icon or 
    /// <c>"stack"</c> for the stacking icon</param>
    private void PlaceIcon(GameObject hex, string type = "attack")
    {
        GameObject icon = null;
        if (type == "attack")
        {
            icon = attackIcon;
        }
        else if (type == "stack")
        {
            icon = stackIcon;
        }
        movementIcons[type].Add(
            Instantiate(icon, movementIconVertical + hex.GetComponent<Hex>().piece.transform.position, Quaternion.identity)
        );
    }

    /// <summary>Peforms operations to start the next turn, like resetting counters and shifting colors</summary>
    private void StartNewTurn()
    {
        // Reset number of moves
        movesTaken = 0;
        maxMoves = baseMaxMoves;
        // Switch color and choose animation
        string slideAnimation, changeToCurrentColorAnimation, changeToOppositeColorAnimation, spinAnimation;
        Animator turnTextAnimator;
        if (turnColor == "white")
        {
            turnColor = "black";
            slideAnimation = "SlideRight";
            changeToCurrentColorAnimation = "WhiteToBlack";
            changeToOppositeColorAnimation = "BlackToWhite";
            spinAnimation = "SpinToBlack";
            turnTextAnimator = blackTurnAnimator;
        }
        else 
        {
            turnColor = "white";
            slideAnimation = "SlideLeft";
            changeToCurrentColorAnimation = "BlackToWhite";
            changeToOppositeColorAnimation = "WhiteToBlack";
            spinAnimation = "SpinToWhite";
            turnTextAnimator = whiteTurnAnimator;
        }
        // Display turn text
        turnTextAnimator.Play(slideAnimation);
        // Update turn count
        turnCounterText.SetText("Turn " + ++turnCount);
        turnCounterAnimator.Play(changeToCurrentColorAnimation);
        // Change move counter color
        moveCounterAnimator.Play(changeToCurrentColorAnimation);
        maxMoveCounterAnimator.Play(changeToOppositeColorAnimation);
        // Spin camera
        cameraAnimator.Play(spinAnimation);
        // Update move count
        UpdateMoveCounter();
    }

    /// <summary>Ends the current move, switches turn if no extra moves have been garnered, and ends the game if enough objective hexes are occupied</summary>
    public void EndMove()
    {
        // Increment move counter
        movesTaken++;
        if (turnColor == "white")
        {
            Results.whiteStats.MovesTaken++;
        }
        else
        {
            Results.blackStats.MovesTaken++;
        }

        #region End the game if someone has won
        // The integer number of hexes needed for this color to win
        int hexesNeeded = (int) (objHexRatio * objectiveHexes[turnColor].Count);
        // Make sure you need to occupy at least one objective hex so the game won't end immediately
        if (hexesNeeded == 0)
        {
            hexesNeeded = 1;
        }
        // Count the number of occupied hexes this color has
        int occupied = 0;
        foreach (GameObject hex in objectiveHexes[turnColor])
        {
            if (hex.GetComponent<Hex>().piece != null && hex.GetComponent<Hex>().piece.tag == turnColor)
            {
                occupied++;
            }
        }
        // If this color has won
        if (occupied >= hexesNeeded)
        {
            // Set results
            Results.whiteWinner = turnColor == "white";
            Results.turns = turnCount;
            // Get elapsed time
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            Results.matchDuration = String.Format("{0:00}:{1:00}:{2:00}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds);

            #region Objective hex statistics
            int whiteOccupied = 0;
            foreach (GameObject hex in objectiveHexes["white"])
            {
                if (hex.GetComponent<Hex>().piece != null && hex.GetComponent<Hex>().piece.tag == "white")
                {
                    whiteOccupied++;
                }
            }
            Results.whiteStats.ObjectiveHexesOccupied = whiteOccupied;

            int blackOccupied = 0;
            foreach (GameObject hex in objectiveHexes["black"])
            {
                if (hex.GetComponent<Hex>().piece != null && hex.GetComponent<Hex>().piece.tag == "black")
                {
                    blackOccupied++;
                }
            }
            Results.blackStats.ObjectiveHexesOccupied = blackOccupied;
            #endregion
            
            // Load results screen
            SceneManager.LoadScene("Results");
        }
        #endregion

        // The loops in this color on the board after this move
        Dictionary<string, int> loops = new Dictionary<string, int> {
            {"white", 0},
            {"black", 0}
        };

        string[] colors = {"white", "black"};
        
        // Search for loops
        // Repeat for each color
        foreach (string color in colors)
        {
            // Reset hexes filled for this color
            hexesFilled = new List<BoardPosition>();
            // Search through every hex on the board
            for (int i = 0; i < rows * columns; i++)
            {
                // Cache stuff at this position
                GameObject hex = board.hexDex[i % rows, i / rows];
                BoardPosition position = hex.GetComponent<BoardPosition>();
                // If the hex has not already been filled and it either has no pieces on it or pieces of the opposite color
                if (!hexesFilled.Contains(position) && (hex.GetComponent<Hex>().piece == null || hex.GetComponent<Hex>().piece.tag != color))
                {
                    // Determine if there is a region at this loop
                    bool isLoop = RegionIsLoop(position, color);
                    // If the region is a loop
                    if (isLoop)
                    {
                        // Add region found to list of loops in this color
                        loops[color]++;
                    }
                }
            }
        }

        // The number of new loops
        int newLoops = loops[turnColor] - previousMoveLoops[turnColor];
        // Cap the number of extra moves 
        if (newLoops > maxExtraMovesPerMove)
        {
            newLoops = maxExtraMovesPerMove;
        }
        // Make sure you don't lose moves if you break a loop
        else if (newLoops < 0)
        {
            newLoops = 0;
        }
        // Increase number of moves this turn
        maxMoves += newLoops;

        // If we we've taken too many moves
        if (movesTaken >= maxMoves)
        {
            StartNewTurn();
        }

        // Set this move's loops as the previous loops for next move
        previousMoveLoops = loops;
        UpdateMoveCounter();
    }

    /// <summary>Finds a region bordered by pieces of a certain color using a floodfill algorithm.
    /// Sets <c>region</c> to all of the hexes within the region and <c>hitWall</c> to true if wall was hit.</summary>
    /// <param name = "source">A hex within the region that the status of would like to be determined</param>
    /// <param name = "color">The string of the color to find loops of</param>
    private bool RegionIsLoop(BoardPosition source, String color)
    {
        // Cache source hex
        GameObject sourceHex = board.hexDex[source.z, source.x];
        // If this hex has no piece on it or it has a piece of the opposite color
        if (!hexesFilled.Contains(source) && (sourceHex.GetComponent<Hex>().piece == null || sourceHex.GetComponent<Hex>().piece.tag != color))
        {
            // This position has now been examined
            hexesFilled.Add(source);
            // Whether any of the neighbors is the edge of the board
            bool[] isLoop = new bool[6];
            // Loop through each neighbor
            for (int i = 0; i < 6; i++)
            {
                // Get this neighbor hex
                GameObject hex = board.hexDex[source.z, source.x].GetComponent<Hex>().neighbors[i];
                // If this hex is null (meaning that there is no neigbor in this direction, then we hit a wall)
                if (hex != null)
                {
                    // Floodfill with the next neighbor
                    isLoop[i] = RegionIsLoop(hex.GetComponent<BoardPosition>(), color);
                }
            }
            return isLoop.All(b => b);
        }
        else
        {
            return true;
        }
    }

    /// <summary>Starts a move of the specified type by setting buttons and variables</summary>
    private void StartSelection(MovementType movementT)
    {
        // Toggles moving and sets movement type
        selectedMoving = true;
        movementType = movementT;
        ChangeButtons(false, movementT);
        // Resets lists that need to be reset after a move is over
        board.damageable = new List<GameObject>();
        movingPieces = new List<Piece>();
    }

    /// <summary>Ends a move and resets selections, highlights, and variables.</summary>
    private void EndSelection()
    {
        // Turns off moving
        selectedMoving = false;
        // Copies highlighted list to damageable list
        board.damageable = board.highlighted;
        // Unoutlines and deselects all hexes
        board.DehighlightAllHexes();
        DeselectAllHexes();
        stepsTo = new Dictionary<GameObject, List<BoardPosition>>();
        // Resets all buttons to interactable
        ChangeButtons(true);
    }
    #endregion

    #region Functions for moving
    /// <summary>Displays a short message.</summary>
    /// <param name = "text">The text to display; default value is <c>"Invalid Movement Option"</c>.</param>
    public void DisplayMessage(string text = "Invalid Movement Option")
    {
        invalidMovementOptionText.SetText(text);
        invalidMovementOptionAnimator.Play("InvalidMovementOption");
    }

    /// <summary>Updates the move counter to the current values</summary>
    private void UpdateMoveCounter()
    {
        moveCounterText.SetText((movesTaken + 1).ToString());
        maxMoveCounterText.SetText(maxMoves.ToString());
    }

    /// <summary>Changes clickable status of every movement button except for the one specified</summary>
    /// <param name = "button">movement button to not change the status of</param>
    /// <param name = "on">whether to turn the buttons on or off; <c>true</c> corresponds to on and <c>false</c> corresponds to off</param>
    private void ChangeButtons(bool on, MovementType button = (MovementType)(-1))
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i != (int)button)
            {
                buttons[i].GetComponent<Button>().interactable = on;
            }
        }
    }

    #region Functions to check at the beginning of movement buttons
    #region Functions to check number of pieces selected and display errors
    /// <summary>Checks if no pieces are selected and displays <c>"No pieces selected"</c> if true</summary>
    /// <returns><c>true</c> if no pieces are selected, <c>false</c> if any pieces are selected</returns>
    private bool NothingSelected()
    {
        if (selected.Count == 0)
        {
            DisplayMessage("No pieces selected");
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>Checks if only one piece is selected and displays <c>"Select only one piece"</c> if false</summary>
    /// <returns><c>true</c> if only one piece is selected, <c>false</c> if any other number is</returns>
    private bool OnlyOneSelected()
    {
        if (selected.Count == 1)
        {
            return true;
        }
        else
        {
            DisplayMessage("Select only one piece");
            return false;
        }
    }
    #endregion

    /// <summary>Determines if a move is being selected and stops the piece being selected if true; 
    /// should be used at the beginning of a movement button method to stop movement selection if button is pressed again</summary>
    /// <returns><c>true</c> if a move is being selected, <c>false</c> if not</returns>
    private bool NotMoving()
    {
        if (selectedMoving)
        {
            EndSelection();
            return false;
        }
        else
        {
            return true;
        }
    }
    #endregion

    #region Functions attached to movement buttons
    // These functions check and highlight possible moves
    public void OnSingle()
    {
        // Makes sure only one piece is selected and we aren't already trying to move
        if (!NothingSelected() && OnlyOneSelected() && NotMoving())
        {
            StartSelection(MovementType.Single);
            // Loops through all neighbors and outlines them as valid moves
            foreach (GameObject hex in selected[0].GetComponent<Hex>().neighbors)
            {
                // Makes sure there is a hex in the neighbor position
                if (hex != null)
                {
                    int color;
                    if (hex.GetComponent<Hex>().piece == null)
                    {
                        color = 1;
                    }
                    else
                    {
                        color = 2;
                    }
                    board.OutlineHex(hex, color);
                    stepsTo[hex] = new List<BoardPosition> {hex.GetComponent<BoardPosition>()};
                }
            }
        }
    }

    public void OnWave()
    {
        // Makes sure pieces are selected and we aren't already trying to move
        if (!NothingSelected() && NotMoving())
        {
            if (selected.Count >= 3)
            {
                // List of hexes that we have seen and validated so far
                // Will be compared to the list of selected to see if they match
                // If the lists match, then all pieces were seen and validated and we can move on
                wave = new List<GameObject>();

                // Find piece at the end of the wave
                GameObject end = null;
                // The direction that the wave starts in from the end
                int direction = -1;
                // The piece at the end fo the wave should only have 1 selected neighbor while all others should have 2
                // Loop through each selected hex
                foreach (GameObject hex in selected)
                {
                    // Start count
                    int count = 0;
                    // Cache neighbors
                    GameObject[] neighbors = hex.GetComponent<Hex>().neighbors;
                    // Count how many neighbors are selected
                    for (int i = 0; i < 6; i++)
                    {
                        // If hex exists and is selected
                        if (neighbors[i] != null && selected.Contains(neighbors[i]))
                        {
                            // Increment count
                            count++;
                            // Set direction to current direction (if this is the hex we're looking for, then this if statement should only trigger once
                            // since there's only one neighbor that is selected, so the direction should get saved and not overwritten)
                            direction = i;
                        }
                    }
                    // If count is 1, then it is on the end
                    if (count == 1)
                    {
                        // Set this hex as end
                        end = hex;
                        // Say that we've seen this hex
                        wave.Add(end);
                        break;
                    }
                }

                // If an end hex was found
                if (end != null && direction != -1)
                {
                    // Find direction(s) to find the wave in
                    // Cache end hex component
                    Hex endHexComponent = end.GetComponent<Hex>();
                    // Set neighbors to be equal to the neighbors of the hex adjacent to the end hex in the found direction
                    GameObject[] neighbors = endHexComponent.neighbors[direction].GetComponent<Hex>().neighbors;
                    // Get directions cycled clockwise and counterclockwise
                    int directionClockwise = Board.CycleDirection(direction, 1);
                    int directionCounterclockwise = Board.CycleDirection(direction, -1);
                    // Offset to start cycling in
                    int cycle = 0;
                    // If the hex in the cycled direction exists and is selected
                    if (neighbors[directionClockwise] != null && selected.Contains(neighbors[directionClockwise]))
                    {
                        cycle = 1;
                    }
                    else if (neighbors[directionCounterclockwise] != null && selected.Contains(neighbors[directionCounterclockwise]))
                    {
                        cycle = -1;
                    }
                    else
                    {
                        DisplayMessage("Select a wave");
                        return;
                    }

                    // Store directions perpendicular to wave
                    int otherDirection = Board.CycleDirection(direction, cycle);
                    perpendicularDirections = new int[2];
                    perpendicularDirections[0] = Board.CycleDirection(direction, -cycle);
                    perpendicularDirections[1] = Board.CycleDirection(otherDirection, cycle);

                    // Set initial hex to hex one away from the end of the wave
                    GameObject hex = endHexComponent.neighbors[direction];
                    // Go up the wave, starting in direction and cycling by cycle and -cycle each time
                    // Neighbors was already set as the neighbors of the second hex in the wave so we can just continue from there
                    while (selected.Contains(hex))
                    {
                        // Say that we have seen this hex
                        wave.Add(hex);

                        // Point to next hex
                        // Cache hex component
                        Hex hexComponent = hex.GetComponent<Hex>();
                        // Get next direction
                        int cycledDirection = Board.CycleDirection(direction, cycle);
                        // Make sure hex exists
                        if (hexComponent.neighbors[cycledDirection] != null)
                        {
                            hex = hexComponent.neighbors[cycledDirection];
                        }
                        else
                        {
                            break;
                        }
                        // Invert cycle for next time through the loop
                        cycle = -cycle;
                        direction = cycledDirection;
                    }

                    // Whether or not every hex in the selected list is in the wave
                    bool allSelectedInWave = true;
                    foreach (GameObject selectedHex in selected)
                    {
                        if (!wave.Contains(selectedHex))
                        {
                            allSelectedInWave = false;
                        }
                    }

                    // Check if every selected hex was seen and validated
                    if (allSelectedInWave)
                    {
                        StartSelection(MovementType.Wave);

                        // Loop through both perpendicular direction
                        foreach (int perpendicularDirection in perpendicularDirections)
                        {
                            // The worst status (not being able to move is worse than being able to bounce off is worse than being able to move)
                            //  that all pieces in this direction will obey
                            int worstStatus = 2;
                            // Find worst status
                            foreach (GameObject waveHex in wave)
                            {
                                // Get adjacent hex
                                GameObject nextHex = waveHex.GetComponent<Hex>().neighbors[perpendicularDirection];
                                // Make sure hex exists
                                if (nextHex != null)
                                {
                                    int positionStatus = board.PositionStatus(nextHex.GetComponent<BoardPosition>(), waveHex.GetComponent<Hex>().piece);
                                    if (positionStatus < worstStatus)
                                    {
                                        worstStatus = positionStatus;
                                    }
                                }
                                else
                                {
                                    // If the hex on this side does not exist, then the wave cannot move there
                                    worstStatus = 0;
                                }
                            }
                            // Save status on this side for later
                            worstStatuses[perpendicularDirection] = worstStatus;

                            if (worstStatus != 0)
                            {
                                // Loop through each hex in the wave
                                foreach (GameObject waveHex in wave)
                                {
                                    // Get hex adjacent to waveHex perpedicular to the wave
                                    GameObject perpendicularHex = waveHex.GetComponent<Hex>().neighbors[perpendicularDirection];
                                    if (perpendicularHex != null)
                                    {
                                        // If there is a piece on the hex
                                        if (perpendicularHex.GetComponent<Hex>().piece != null)
                                        {
                                            board.OutlineHex(perpendicularHex, 2);
                                        }
                                        else if (worstStatus != 1)
                                        {
                                            board.OutlineHex(perpendicularHex, 1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        DisplayMessage("Select only a wave");
                    }
                }
                else
                {
                    DisplayMessage("Select a wave");
                }
            }
            else
            {
                DisplayMessage("Select at least three pieces");
            }
        }
    }

    /// <summary>Finds the direction that a wave is in given a hex next to a wave.</summary>
    /// <param name = "hex">The hex that is on one side of the wave.</param>
    /// <returns>Direction that the given hex in is relative to the wave if the direction is found, -1 otherwise.</returns>
    public int FindWaveDirection(GameObject hex)
    {
        // Loop through each hex in the wave
        foreach (GameObject waveHex in wave)
        {
            // Check both sides of this hex
            foreach (int perpendicularDirection in perpendicularDirections)
            {
                // Get hex on the perpendicularDirection side of the wave
                GameObject perpendicularHex = waveHex.GetComponent<Hex>().neighbors[perpendicularDirection];
                // Make sure the hex on this side exists
                if (perpendicularHex != null)
                {
                    // If this is the hex that was hit, this is the correct direction
                    if (perpendicularHex == hex)
                    {
                        return perpendicularDirection;
                    }
                }
            }
        }
        // Return -1 if not found
        return -1;
    }

    public void OnCannon()
    {
        // Make sure that only one hex is selected and we aren't already trying to move
        if (!NothingSelected() && OnlyOneSelected() && NotMoving())
        {
            #region Initialize/Cache variables
            // Hex to perform operations on
            GameObject hex = selected[0];
            // Gets lines from selected
            List<GameObject>[] lines = board.FindLines(hex.GetComponent<BoardPosition>());
            // Get directions line go in
            List<int> directions = Board.GetLineDirections(lines);
            // Cache neighbors
            GameObject[] neighbors = hex.GetComponent<Hex>().neighbors;
            #endregion

            // Make sure that the piece has at least one line and is not only in the middle of line(s)
            if (directions.Count != 0)
            {
                StartSelection(MovementType.Cannon);
                // Loop through all directions piece can move
                // This if for if piece is the end of multiple lines
                foreach (int direction in directions)
                {
                    // Set source hex as first hex
                    hex = selected[0];
                    // Initialize step list for this direction
                    List<BoardPosition> steps = new List<BoardPosition>();
                    // Set source hex as first step
                    steps.Add(hex.GetComponent<BoardPosition>());

                    // Highlight possible moves
                    for (int i = 0; i < lines[Board.GetOppositeDirection(direction)].Count; i++)
                    {
                        // Makes sure the hexes down the board exist
                        try
                        {
                            // Cache hex component
                            Hex hexComponent = hex.GetComponent<Hex>();
                            // Checks and highlights valid moves
                            // Make sure the hex down the board exists
                            if (hexComponent.neighbors[direction] != null)
                            {
                                int positionStatus = board.PositionStatus(
                                    hexComponent.neighbors[direction].GetComponent<BoardPosition>(),
                                    selected[0].GetComponent<Hex>().piece
                                );
                                // Checks if the hex can move through the position
                                if (positionStatus != 0)
                                {
                                    // Sets current hex to hex to the direction of the past hex
                                    hex = hexComponent.neighbors[direction];
                                    hexComponent = hex.GetComponent<Hex>();
                                    // Adds current hex to steps
                                    steps.Add(hex.GetComponent<BoardPosition>());
                                    // Set steps to current hex
                                    stepsTo[hex] = new List<BoardPosition>(steps);
                                    // Outline hex
                                    board.OutlineHex(hex, 1);
                                    // If there's a piece on the current hex and it is stacked
                                    if (positionStatus == 1)
                                    {
                                        hex.GetComponent<cakeslice.Outline>().color = 2;
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            // Stop the highlighting, make all moves down the line invalid
                            else
                            {
                                break;
                            }
                        }
                        catch (KeyNotFoundException)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                DisplayMessage("Piece must be at the end of a line");
            }
        }
    }

    public void OnV()
    {
        // Make sure that only one hex is selected and we aren't already trying to move
        if (!NothingSelected() && OnlyOneSelected() && NotMoving())
        {
            // Lines and directions
            List<GameObject>[] lines = board.FindLines(selected[0].GetComponent<BoardPosition>());
            // Find all directions where there is a hex/line
            List<int> directions = new List<int>();
            foreach (int direction in Enum.GetValues(typeof(Direction)))
            {
                if (lines[direction].Count > 1)
                {
                    directions.Add(direction);
                }
            }

            if (directions.Count >= 2)
            {
                // Find valid directions
                // List of pairs of valid directions
                List<int[]> Vs = new List<int[]>();
                // Loop through all directions of lines found
                foreach (int direction in directions)
                {
                    // The two directions off the selected hex that the V goes in
                    int[] VDirections = new int[2];
                    // Next consecutive direction
                    int direction2 = Board.CycleDirection(direction, 1);
                    // If direction and the next direction clockwise have lines going in those directions, add the V going in those two directions to the list of Vs
                    if (directions.Contains(direction2))
                    {
                        VDirections[0] = direction;
                        VDirections[1] = direction2;
                        Vs.Add(VDirections);
                    }
                }
                // If any valid Vs are found
                if (Vs.Count != 0)
                {
                    StartSelection(MovementType.V);
                    // Loops through each V and reverses its direction (since Vs point away from direction they should be firing)
                    for (int i = 0; i < Vs.Count; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            Vs[i][j] = Board.GetOppositeDirection(Vs[i][j]);
                        }
                    }
                    
                    // Loops through each V
                    foreach (int[] V in Vs)
                    {
                        // Get starting BoardPosition
                        BoardPosition position = selected[0].GetComponent<BoardPosition>();
                        int z = position.z;
                        int x = position.x;

                        // List of steps taken
                        List<BoardPosition> steps = new List<BoardPosition>();
                        // Set source hex as first step
                        steps.Add(position);

                        // Loops as many times as the smallest amount of pieces in either part of the V
                        // Since we revered the direction earlier we need to re-reverse it
                        for (int i = 0; i < Math.Min(lines[Board.GetOppositeDirection(V[0])].Count, lines[Board.GetOppositeDirection(V[1])].Count); i++)
                        {
                            // If V is pointing straight up
                            if (Board.DirectionIsTop(V[0]) && Board.DirectionIsTop(V[1]))
                            {
                                z += 2;
                            }
                            // If V is pointing straight down
                            else if (Board.DirectionIsBottom(V[0]) && Board.DirectionIsBottom(V[1]))
                            {
                                z -= 2;
                            }
                            else
                            {
                                // If either direction of the V is left, then it will be pointing left
                                if (Board.DirectionIsLeft(V[0]))
                                {
                                    x -= 2 - z % 2;
                                }
                                else
                                {
                                    x += 1 + z % 2;
                                }
                                // If either direction of the V is up, then it will be pointing up
                                if (Board.DirectionIsTop(V[0]) || Board.DirectionIsTop(V[1]))
                                {
                                    z++;
                                }
                                else
                                {
                                    z--;
                                }
                            }
                            // Make sure we don't go over the edge of the board
                            try
                            {
                                // Cache hex component
                                Hex hex = board.hexDex[z, x].GetComponent<Hex>();
                                // Find position status
                                int positionStatus = board.PositionStatus(z, x, selected[0].GetComponent<Hex>().piece);
                                // If piece can move through position
                                if (positionStatus != 0)
                                {
                                    // Set steps and outline hex
                                    steps.Add(board.hexDex[z, x].GetComponent<BoardPosition>());
                                    stepsTo[board.hexDex[z, x]] = new List<BoardPosition>(steps);
                                    board.OutlineHex(board.hexDex[z, x], 1);
                                    // If there's a piece on the current hex and it is stacked
                                    if (positionStatus == 1)
                                    {
                                        hex.GetComponent<cakeslice.Outline>().color = 2;
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    DisplayMessage("Directions of a V must be consecutive");
                }
            }
            else
            {
                DisplayMessage("Select a piece at the end of a V");
            }
        }
    }

    public void OnContiguous()
    {
        // Make sure that only one hex is selected and we aren't already trying to move
        if (!NothingSelected() && OnlyOneSelected() && NotMoving())
        {
            // Reset variables
            contiguousHexes  = new Dictionary<GameObject, List<List<int>>>();
            contiguousVisits = new List<GameObject>();
            directionsList   = new List<int>();
            // Finds contiguous pieces
            FindContiguous(selected[0]);
            // Makes sure there are contiguous pieces
            if (contiguousHexes.Count != 0)
            {
                StartSelection(MovementType.Contiguous);
                // Go through every found piece
                foreach (GameObject hex in contiguousHexes.Keys)
                {
                    // Choose color
                    int color = 0;
                    // If there's no piece, green
                    if (hex.GetComponent<Hex>().piece == null)
                    {
                        color = 1;
                    }
                    // If there is a piece of opposite color, hit color
                    else
                    {
                        color = 2;
                    }
                    board.OutlineHex(hex, color);
                }
            }
            else
            {
                DisplayMessage("Piece has no contiguous pieces");
            }
        }
    }

    #region Contiguous movement utility functions 
    /// <summary>Finds shortest path from the selected hex to the given hex</summary>
    /// <returns>smallest list of directions to get from the selected hex to the given hex</returns>
    private List<int> GetDirectionsTo(GameObject hex)
    {
        // Initialize list to store shortest list of directions
        List<int> directionList = null;
        // Go through each directions list and find the shortest one
        foreach (List<int> directions in contiguousHexes[hex])
        {
            if (directionList == null)
            {
                directionList = directions;
            }
            else if (directions.Count < directionList.Count)
            {
                directionList = directions;
            }
        }
        return directionList;
    }

    /// <summary>Find hexes contiguous to the given hex and creates a list of directions it took to get there from the given hex</summary>
    /// <param name = "sourceHex">the hex to find hexes contiguous to</param>
    private void FindContiguous(GameObject sourceHex)
    {
        // Get pieces adjacent to current hex
        List<GameObject> adjacent = board.GetAdjacentPieces(sourceHex);
        // Loops through all found adjacent hexes with pieces
        foreach (GameObject hex in adjacent)
        {
            // If hex is not the source hex or has been visited in this string of visits
            if (hex != selected[0] && !contiguousVisits.Contains(hex))
            {
                // Add hex to list of visits
                contiguousVisits.Add(hex);
                // Add direction to directions list
                directionsList.Add(board.GetDirection(sourceHex, hex));
                
                // Go through every neighbor of each found contiguous piece
                foreach (GameObject hexNeighbor in hex.GetComponent<Hex>().neighbors)
                {
                    // Don't deal with it if it has a piece of the same color on it
                    if (hexNeighbor != null
                        && !(hexNeighbor.GetComponent<Hex>().piece != null && hexNeighbor.GetComponent<Hex>().piece.tag == turnColor))
                    {
                        // Add direction to directions list
                        directionsList.Add(board.GetDirection(hex, hexNeighbor));
                        // Initialize the list of lists at the key of the hex if not already done
                        if (!contiguousHexes.ContainsKey(hexNeighbor))
                        {
                            contiguousHexes[hexNeighbor] = new List<List<int>>();
                        }
                        // Add list of directions to big dictionary of directions for each hex
                        contiguousHexes[hexNeighbor].Add(new List<int>(directionsList));

                        // Clear this direction from the end of the list
                        if (directionsList.Count != 0)
                        {
                            directionsList.RemoveAt(directionsList.Count - 1);
                        }
                    }
                }
                // Find contiguous from current hex
                FindContiguous(hex);
                // We're done with this hex, so remove it from the string of visits
                contiguousVisits.RemoveAt(contiguousVisits.Count - 1);
            }
        }
        if (directionsList.Count != 0)
        {
            directionsList.RemoveAt(directionsList.Count - 1);
        }
    }
    #endregion
    
    public void OnUnstack()
    {
        if (!NothingSelected() && OnlyOneSelected() && NotMoving())
        {
            // Cache hex component
            Hex hex = selected[0].GetComponent<Hex>();
            if (hex.piece != null && hex.piece.transform.childCount > 1)
            {
                StartSelection(MovementType.Unstack);
                int stackCount = selected[0].GetComponent<Hex>().piece.transform.childCount;
                // Go out in all directions
                for (int direction = 0; direction < 6; direction++)
                {
                    // Whether or not the stack can unstack completely in this direction
                    bool canUnstack = true;
                    hex = selected[0].GetComponent<Hex>();
                    // Highlight possible moves
                    for (int i = 0; i < stackCount - 1; i++)
                    {
                        // Makes sure the hexes down the board exist
                        try
                        {
                            // Checks and highlights valid moves
                            // Make sure the hex down the board exists
                            if (hex.neighbors[direction] != null)
                            {
                                // Sets current hex to hex to the direction of the past hex
                                hex = hex.neighbors[direction].GetComponent<Hex>();
                                // Get position status
                                int positionStatus = board.PositionStatus(
                                    hex.GetComponent<BoardPosition>(),
                                    selected[0].GetComponent<Hex>().piece
                                );
                                // Checks if the hex can move through the position
                                if (positionStatus != 2)
                                {
                                    // Mark this direction as invalid
                                    canUnstack = false;
                                    break;
                                }
                            }
                            else
                            {
                                // Mark this direction as invalid
                                canUnstack = false;
                                break;
                            }
                        }
                        catch (KeyNotFoundException)
                        {
                            // Mark this direction as invalid
                            canUnstack = false;
                            break;
                        }
                    }

                    // If direction was determined to be valid, highlight in that direction
                    if (canUnstack)
                    {
                        // Highlight moves in this direction
                        // Get first hex again
                        hex = selected[0].GetComponent<Hex>();
                        // List of spaces in this direction
                        List<BoardPosition> steps = new List<BoardPosition>();
                        // Add source hex to first position in line
                        steps.Add(selected[0].GetComponent<BoardPosition>());

                        for (int i = 0; i < stackCount - 1; i++)
                        {
                            // Sets current hex to hex to the direction of the past hex
                            hex = hex.neighbors[direction].GetComponent<Hex>();
                            // Choose color
                            // Since stacks in the way have been weeded out already, 
                            // if is any piece on the hex it should be unstacked and able to be moved through
                            int color;
                            // If there is no piece on the hex
                            if (hex.piece == null)
                            {
                                color = 1;
                            }
                            // If there is a piece on the hex
                            else
                            {
                                color = 2;
                            }
                            // Outline hex
                            board.OutlineHex(hex.gameObject, color);
                            // Add to line
                            steps.Add(hex.GetComponent<BoardPosition>());
                        }

                        // Go through each hex in the line and set their steps to equal to the steps for the whole line
                        foreach (BoardPosition step in steps)
                        {
                            stepsTo[step.gameObject] = steps;
                        }
                    }
                }
            }
            else
            {
                DisplayMessage("Select a stack");
            }
        }
    }
    
    public void OnRandom()
    {
        // If the process of generating pieces has not already started
        if (!selectedMoving)
        {    
            // The number of empty hexes found
            int empty = 0;
            // Loop through each hex but break if there are enough empty hexes to spawn
            for (int i = 0; i < rows * columns && empty < randomPieceNum; i++)
            {
                if (board.hexDex[i % rows, i / rows].GetComponent<Hex>().piece == null)
                {
                    empty++;
                }
            }
            
            // If there aren't enough empty spaces
            if (empty < randomPieceNum)
            {
                DisplayMessage("There are not enough empty hexes");
            }
            else
            {
                // Init random
                System.Random random = new System.Random();
                // Generate the specified number of pieces
                for (int i = 0; i < randomPieceNum; i++)
                {
                    int z, x;
                    do
                    {
                        // Generate random positions
                        z = random.Next(0, rows);
                        x = random.Next(0, columns);
                    } 
                    // If there is a piece on the chosen position, choose again
                    // Breaks when there is no piece on the chosen position
                    while (board.hexDex[z, x].GetComponent<Hex>().piece != null);

                    // Choose which color piece to place based on turn color
                    GameObject pieceToPlace;
                    if (turnColor == "white")
                    {
                        pieceToPlace = whitePiecePrefab;
                    }
                    else
                    {
                        pieceToPlace = blackPiecePrefab;
                    }

                    // Cache hex and component
                    GameObject hex = board.hexDex[z, x];
                    Hex hexComponent = hex.GetComponent<Hex>();
                    // Spawn piece
                    GameObject piece = Instantiate(pieceToPlace, hex.transform.position + pieceVertical, Quaternion.identity);
                    // Instantiate new piece and add to hex
                    hexComponent.piece = piece;
                    // Cache BoardPosition
                    BoardPosition boardPosition = piece.GetComponent<BoardPosition>();
                    boardPosition.z = z;
                    boardPosition.x = x;
                }
                StartSelection(MovementType.RandomGeneration);
                // End this move in a specified amount of time
                Invoke("EndRandomGenerationWait", randomGenerationWaitTime);
            }
        }
    }

    /// <summary>Ends the wait time between pieces being randomly generated and beginning the next turn.
    /// This is to prevent being able to hit the button to rapidly spawn pieces.</summary>
    private void EndRandomGenerationWait()
    {
        EndSelection();
        EndMove();
    }
    #endregion
    #endregion
}
