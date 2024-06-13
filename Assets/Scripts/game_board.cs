using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

//state of the game
public enum GameState
{
    wait,
    move,
    win,
    lose,
    pause
}

//list of tiles
public enum TileKind
{
    Breakable01,
    Blank,
    Lock,
    Blocker01,
    Slime,
    Normal,
    ColorBomb,
    WrapBomb,
    ColumnBomb,
    RowBomb,
    element_01,
    element_02,
    element_03,
    element_04,
    element_05,
    Breakable02,
    Blocker02
}

//type of matches
[System.Serializable]
public class MatchType
{
    public int type;
    public string color;
}

//type of tiles
[System.Serializable]   
public class TileType
{    
    public int x;
    public int y;
    public TileKind tileKind;
}

public class game_board : MonoBehaviour
{
    [Header("Scriptable Objects")]
    public world worldClass;
    public int level;
    
    public GameState currentState = GameState.move;

    [Header("Size WxH")]
    public int width;
    public int height;
    public int offSet;

    public float refillDelay = .9f;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject tileCornerPrefab;
    public GameObject break01Prefab;
    public GameObject break02Prefab;
    public GameObject lockTilePrefab; 
    public GameObject blocker01Prefab; 
    public GameObject blocker02Prefab; 
    public GameObject slimeTilePrefab; 

    //bonus
    public GameObject colorBombPrefab; //lock object

    //arrays
    public GameObject[] dots;
    public GameObject[,] allDots;
    public Vector2[,] allTypeDotsCoord;
    private bool[,] allNonBlankDots;

    //for corner background
    List<Vector2> cornerCoords = new List<Vector2>();
    List<Vector3> cornerRotation = new List<Vector3>();

    //VFX
    public GameObject destroyEffect;

    [Header("Layout")]
    public TileType[] boardLayout;
    public TileType[] preloadBoardLayout;

    //for blank
    private bool[,] blankCells;

    //for break
    private tile_back[,] breakableCells;

    //for extendable
    private tile_back[,] slimeCells;
    private bool makeSlime = true;

    //for lock
    public tile_back[,] lockCells;
    
    //for blocker_01
    public tile_back[,] blocker01Cells;

    //for preload
    public tile_back[,] preloadCells;

    //dict
    private Dictionary<TileKind, int> preloadDict;
    private Dictionary<TileKind, GameObject> breacableDict;
    private Dictionary<TileKind, GameObject> blockersDict;

    //bonus
    private tile_back[,] bonusCells;

    [Header("Match Suff")]
    public MatchType matchTypeClass;
    public dot currentDot;
    public string colRowBombsName = "line_bomb"; //agregation for count

    //classes
    private find_matches findMatchesClass;
    private score_manager scoreManagerClass;
    private sound_manager soundManagerClass;
    private goal_manager goalManagerClass;
    

    //for score
    public int baseValue = 1;
    public int streakValue = 1;
    public int[] scoreGoals;

    //bombs values    
    private int minMatchCount = 3;
    private int minMatchForBomb = 4;

    private int matchForRowColBomb = 4;
    private int matchForWrapBomb = 2;
    private int matchForColorBomb = 3;


    private void Awake()
    {

        if(PlayerPrefs.HasKey("Current_Level"))
        {
            level = PlayerPrefs.GetInt("Current_Level");
        }

        if(worldClass != null)
        {
            if (level < worldClass.levels.Length)
            {
                if (worldClass.levels[level] != null)
                {
                    width = worldClass.levels[level].width;
                    height = worldClass.levels[level].height;

                    dots = worldClass.levels[level].dots;

                    scoreGoals = worldClass.levels[level].scoreGoals;

                    boardLayout = worldClass.levels[level].boardLayout;
                    preloadBoardLayout = worldClass.levels[level].preloadBoardLayout;
                }
            }
        }


        // Initialize the dictionary for preload
        preloadDict = new Dictionary<TileKind, int>
        {
            { TileKind.element_01, 0 },
            { TileKind.element_02, 1 },
            { TileKind.element_03, 2 },
            { TileKind.element_04, 3 },
            { TileKind.element_05, 4 }
        };

        //for break
        breacableDict = new Dictionary<TileKind, GameObject>
        {
            { TileKind.Breakable01, break01Prefab },
            { TileKind.Breakable02, break02Prefab }
        };

        //for blockers
        blockersDict = new Dictionary<TileKind, GameObject>
        {
            { TileKind.Blocker01, blocker01Prefab },
            { TileKind.Blocker02, blocker02Prefab }
        };

    }


    // Start is called before the first frame update
    void Start()
    {
        //class init
        findMatchesClass = FindObjectOfType<find_matches>();
        scoreManagerClass = FindObjectOfType<score_manager>();
        soundManagerClass = FindObjectOfType<sound_manager>();
        goalManagerClass = FindObjectOfType<goal_manager>();        

        //init type of objects
        blankCells = new bool[width, height];
        breakableCells = new tile_back[width, height];
        lockCells = new tile_back[width, height];
        blocker01Cells = new tile_back[width, height];
        slimeCells = new tile_back[width, height];

        //for preload
        preloadCells = new tile_back[width, height];

        //for bonus
        bonusCells = new tile_back[width, height];

        //all dots on board
        allDots = new GameObject[width, height];

        //all types
        allTypeDotsCoord = new Vector2[width, height];

        //non blanc dots
        allNonBlankDots = new bool [width, height];

        //setup board
        SetUp();

        //Set Framerate
        Application.targetFrameRate = 30;

        //set resoluton
        Screen.SetResolution(1920, 1080, true);
        Screen.SetResolution((int)Screen.width, (int)Screen.height, true);

        //state
        currentState = GameState.pause;        
    }


    public void GenPreloadLayout()
    {
        for (int i = 0; i < preloadBoardLayout.Length; i++)
        {
            TileKind kind = preloadBoardLayout[i].tileKind;

            if (preloadDict.ContainsKey(kind))
            {
                Vector2 tempPos = new Vector2(preloadBoardLayout[i].x, preloadBoardLayout[i].y);
                int valueX = preloadBoardLayout[i].x;
                int valueY = preloadBoardLayout[i].y;

                // Delete old dot
                Destroy(allDots[valueX, valueY].gameObject);

                // Create preload
                GameObject preloadDot = Instantiate(dots[preloadDict[kind]], tempPos, Quaternion.identity);

                // Set position
                dot dotComponent = preloadDot.GetComponent<dot>();
                dotComponent.column = valueX;
                dotComponent.row = valueY;

                // Rename
                preloadDot.name = dots[preloadDict[kind]].name + "_preload_" + i;

                // Add to dots
                allDots[valueX, valueY] = preloadDot;
            }
        }
    }

    public void GenBlankCells()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Blank)
            {
                blankCells[boardLayout[i].x, boardLayout[i].y] = true;                
            }    
        }
    }

    public void GenBreakTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            TileKind kind = boardLayout[i].tileKind;

            if (breacableDict.ContainsKey(kind))
            {
                Vector2 tempPos = new Vector2(boardLayout[i].x, boardLayout[i].y);
                
                GameObject prefab = breacableDict[kind];
                GameObject tileBreakable = Instantiate(prefab, tempPos, Quaternion.identity);

                breakableCells[boardLayout[i].x, boardLayout[i].y] = tileBreakable.GetComponent<tile_back>();

                string dotName = prefab.name + "_" + i + "_" + boardLayout[i].x + "x" + boardLayout[i].y;

                tileBreakable.name = dotName;

                // Add to all
                allTypeDotsCoord[boardLayout[i].x, boardLayout[i].y] = tempPos;
            }
        }
    }

    private void GenLockTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Lock)
            {
                Vector2 tempPos = new Vector2(boardLayout[i].x, boardLayout[i].y);

                GameObject tile = Instantiate(lockTilePrefab, tempPos, Quaternion.identity);

                lockCells[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<tile_back>();

                string dotName = tile.name + "-" + i;
                tile.name = dotName;

               //add to all
               allTypeDotsCoord[boardLayout[i].x, boardLayout[i].y] = tempPos;
            }
        }
    }

    //bubble gum
    private void GenBlock01Tiles()
    {

        for (int i = 0; i < boardLayout.Length; i++)
        {
            TileKind kind = boardLayout[i].tileKind;
            if (blockersDict.ContainsKey(kind))
            {
                Vector2 tempPos = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject prefab = blockersDict[kind];

                GameObject tileBlocker = Instantiate(prefab, tempPos, Quaternion.identity);

                blocker01Cells[boardLayout[i].x, boardLayout[i].y] = tileBlocker.GetComponent<tile_back>();

                //naming
                string dotName = prefab.name + "-" + i + "-" + boardLayout[i].x + "x" + boardLayout[i].y;
                tileBlocker.name = dotName;

                // Add to all
                allTypeDotsCoord[boardLayout[i].x, boardLayout[i].y] = tempPos;
            }
        }

    }

    private void GenSlimeTiles()
    {

        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Slime)
            {
                Vector2 tempPos = new Vector2(boardLayout[i].x, boardLayout[i].y);

                GameObject expandingTile = Instantiate(slimeTilePrefab, tempPos, Quaternion.identity);

                slimeCells[boardLayout[i].x, boardLayout[i].y] = expandingTile.GetComponent<tile_back>();

                string dotName = expandingTile.name + "-" + i;
                expandingTile.name = dotName;

                //add to all
                allTypeDotsCoord[boardLayout[i].x, boardLayout[i].y] = tempPos;

            }
        }
    }

    private void GenBonusCells()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //colorBomb
            if (boardLayout[i].tileKind == TileKind.ColorBomb)
            {                                     
                int column = boardLayout[i].x;
                int row = boardLayout[i].y;

                //get current dot
                GameObject currentDot = allDots[column, row];

                if (currentDot != null)
                {
                    //get dot component
                    dot curDotGet = currentDot.GetComponent<dot>();

                    if (curDotGet != null)
                    {
                        curDotGet.CookColorBomb();                        
                        curDotGet.isColorBomb = true;                        
                    }
                }              
            }

            //wrap bomb
            if (boardLayout[i].tileKind == TileKind.WrapBomb)
            {
                int column = boardLayout[i].x;
                int row = boardLayout[i].y;

                //get current dot
                GameObject currentDot = allDots[column, row];

                if (currentDot != null)
                {
                    //get dot component
                    dot curDotGet = currentDot.GetComponent<dot>();

                    if (curDotGet != null)
                    {
                        curDotGet.CookWrapBomb();
                        curDotGet.isWrapBomb = true;
                    }
                }
            }

        }
    }


    private void FindCorners()
    {

        int columnsAllDots = allTypeDotsCoord.GetLength(0);
        int rowsAllDots = allTypeDotsCoord.GetLength(1);
       
        //left bottom
        for (int i = 0; i < columnsAllDots; i++)
        {
            for (int j = 0; j < rowsAllDots; j++)
            {                
                if (allNonBlankDots[i, j])
                {
                    //add corner position
                    float tempX = allTypeDotsCoord[i, j].x;
                    float tempY = allTypeDotsCoord[i, j].y;

                    Vector2 tempCorner = new Vector2(tempX, tempY);

                    if (!cornerCoords.Contains(tempCorner))
                    {
                        cornerCoords.Add(tempCorner);

                        //add corenr rotation
                        Vector3 tempAngle = new Vector3(0, 0, 0);
                        cornerRotation.Add(tempAngle);
                    }                        
                    break;
                }
            }

            if (allNonBlankDots[i, 0])
                break;
        }

        //bottom right
        for (int i = columnsAllDots - 1; i >= 0; i--)
        {
            for (int j = 0; j < rowsAllDots; j++)
            {
                if (allNonBlankDots[i, j])
                {
                    //add corner position
                    float tempX = allTypeDotsCoord[i, j].x;
                    float tempY = allTypeDotsCoord[i, j].y;

                    Vector2 tempCorner = new Vector2(tempX, tempY);

                    if (!cornerCoords.Contains(tempCorner))
                    {
                        cornerCoords.Add(tempCorner);
                        //add corenr rotation
                        Vector3 tempAngle = new Vector3(0, 0, 90);
                        cornerRotation.Add(tempAngle);
                    }
                                           
                    break;
                }
            }

            if (allNonBlankDots[i, 0])
                break;
        }

        //left top
        for (int i = 0; i < columnsAllDots; i++)
        {
            for (int j = rowsAllDots - 1; j >= 0; j--)
            {

                //add corner position
                if (allNonBlankDots[i, j])
                {                   
                    //add corner position
                    float tempX = allTypeDotsCoord[i, j].x;
                    float tempY = allTypeDotsCoord[i, j].y;

                    Vector2 tempCorner = new Vector2(tempX, tempY);
                    Vector2 prevCorner = new Vector2(tempX-1, tempY); // for previous non blank coord

                    if (!cornerCoords.Contains(tempCorner) && !cornerCoords.Contains(prevCorner))
                    {
                        cornerCoords.Add(tempCorner);
                        //add corenr rotation
                        Vector3 tempAngle = new Vector3(0, 0, -90);
                        cornerRotation.Add(tempAngle);
                    }
                        

                    break;
                }
            }

            if (allNonBlankDots[i, rowsAllDots - 1])
                break;           
        }

        //top right
        for (int i = columnsAllDots - 1; i >= 0; i--)
        {
            for (int j = rowsAllDots - 1; j >= 0; j--)
            {
                if (allNonBlankDots[i, j])
                {                    
                    //add corner position
                    float tempX = allTypeDotsCoord[i, j].x;
                    float tempY = allTypeDotsCoord[i, j].y;

                    Vector2 tempCorner = new Vector2(tempX, tempY);
                    Vector2 prevCorner = new Vector2(tempX + 1, tempY); // for previous non blank coord

                    if (!cornerCoords.Contains(tempCorner) && !cornerCoords.Contains(prevCorner))
                    {
                        cornerCoords.Add(tempCorner);
                        Vector3 tempAngle = new Vector3(0, 0, 180);
                        cornerRotation.Add(tempAngle);
                    }                       

                    break;
                }
            }

            if (allNonBlankDots[i, rowsAllDots - 1])
                break;
        }
    }



    // setub board
    private void SetUp()
    {
        //gen all types of dots
        GenBlankCells();
        GenBreakTiles();
        GenLockTiles();
        GenBlock01Tiles();
        GenSlimeTiles();

        //for naming
        int counter = 0;

        //fill board with elements
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankCells[i, j] && !blocker01Cells[i, j] && !slimeCells[i, j])
                {
                    //temp position and offset
                    Vector2 tempPos = new Vector2(i, j + offSet);
                    //Vector2 tilePos = new Vector2(i, j);                   
                                   
                    //add elements
                    int dotToUse = UnityEngine.Random.Range(0, dots.Length);

                    int maxItertion = 0;

                    //board without match
                    while(MatchesAt(i, j, dots[dotToUse]) && maxItertion < 100)
                    {
                        dotToUse = UnityEngine.Random.Range(0, dots.Length);
                        maxItertion++;
                    }

                    maxItertion = 0;

                    //instance dot
                    GameObject dot = Instantiate(dots[dotToUse], tempPos, Quaternion.identity);

                    //for offset
                    dot.GetComponent<dot>().row = j;
                    dot.GetComponent<dot>().column = i;

                    //set properties
                    dot.transform.parent = this.transform;

                    counter++;

                    //dots naming
                    dot.name = dot.tag + "_" + counter + "_" + i + "-" + j;
              
                    //add elements to array
                    allDots[i,j] = dot;                
                }
            }
        }

        //bonus cells
        GenBonusCells();

        //if not null gen preload cells
        if (preloadBoardLayout != null )
        {
            GenPreloadLayout();
        }        

        //add all dots to array
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (blocker01Cells[i, j] || slimeCells[i, j] || allDots[i, j])
                {
                    Vector2 tilePos = new Vector2(i, j);
                    allTypeDotsCoord[i, j] = tilePos;
                    allNonBlankDots[i, j] = true;
                }            
            }
        }

        //find corners for background
        FindCorners();        

        //fill back corners
        for (int i = 0; i < cornerCoords.Count; i++)
        {
            GameObject backTileCorner = Instantiate(tileCornerPrefab, cornerCoords[i], Quaternion.Euler(cornerRotation[i])) as GameObject;
            backTileCorner.transform.parent = this.transform;
            counter++;
            backTileCorner.name = "" + counter + "_BackCorner" + cornerCoords[i].x + "-" + cornerCoords[i].y;
        }

        counter = 0;

        int columnsAllDots = allDots.GetLength(0);
        int rowsAllDots = allDots.GetLength(1);

        //Fill back std tiles
        for (int i = 0; i < columnsAllDots; i++)
        {
            for (int j = 0; j < rowsAllDots; j++)
            {
                if (!blankCells[i, j] && !blocker01Cells[i, j] && !slimeCells[i, j])
                {
                    Vector2 tilePos = new Vector2(i, j);

                    if (!cornerCoords.Contains(tilePos))
                    {
                        //add background                        
                        GameObject backgroudTile = Instantiate(tilePrefab, tilePos, Quaternion.identity) as GameObject;
                        backgroudTile.transform.parent = this.transform;

                        counter++;

                        string dotName = "" + counter + "-B1-" + i + "-" + j;
                        backgroudTile.name = dotName;
                    }
                } else
                {
                    Vector2 tilePos = new Vector2(i, j);

                    if (!cornerCoords.Contains(tilePos) && !blankCells[i, j])
                    {
                        GameObject backgroudTile = Instantiate(tilePrefab, tilePos, Quaternion.identity) as GameObject;
                        backgroudTile.transform.parent = this.transform;
                        counter++;

                        string dotName = "" + counter + "-B2-" + i + "-" + j;
                        backgroudTile.name = dotName;
                    }

                }
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject element)
    {
        // Check horizontally for matches
        if (column > 1)
        {
            if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
            {
                if (allDots[column - 1, row].tag == element.tag && allDots[column - 2, row].tag == element.tag)
                {
                    return true;
                }
            }
        }

        // Check vertically for matches
        if (row > 1)
        {
            if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
            {
                if (allDots[column, row - 1].tag == element.tag && allDots[column, row - 2].tag == element.tag)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private MatchType ColumnOrRow()
    {
        // Copy of the current match
        List<GameObject> matchCopy = new List<GameObject>(findMatchesClass.currentMatch);

        matchTypeClass.type = 0;
        matchTypeClass.color = "";

        // Iterate through each dot in the match
        foreach (GameObject matchObject in matchCopy)
        {
            dot thisDot = matchObject.GetComponent<dot>();
            string color = matchObject.tag;  // Get the color from the tag

            int column = thisDot.column;
            int row = thisDot.row;

            int columnMatch = 0;
            int rowMatch = 0;

            // Compare with other dots in the match
            foreach (GameObject otherMatchObject in matchCopy)
            {
                if (otherMatchObject == matchObject)
                {
                    continue;
                }

                dot nextDot = otherMatchObject.GetComponent<dot>();

                if (nextDot.column == column && nextDot.CompareTag(color))
                {
                    columnMatch++;
                }

                if (nextDot.row == row && nextDot.CompareTag(color))
                {
                    rowMatch++;
                }
            }

            // Check for the type of match
            if (columnMatch == matchForRowColBomb || rowMatch == matchForRowColBomb)
            {
                matchTypeClass.type = 1;
                matchTypeClass.color = color;
                return matchTypeClass;
            }
            else if (columnMatch == matchForWrapBomb && rowMatch == matchForWrapBomb)
            {
                matchTypeClass.type = 2;
                matchTypeClass.color = color;
                return matchTypeClass;
            }
            else if (columnMatch == matchForColorBomb || rowMatch == matchForColorBomb)
            {
                matchTypeClass.type = 3;
                matchTypeClass.color = color;
                return matchTypeClass;
            }
        }

        // If no match type found, return default
        matchTypeClass.type = 0;
        matchTypeClass.color = "";
        return matchTypeClass;
    }


    private void CheckToCookBombs()
    {
        if (findMatchesClass.currentMatch.Count > minMatchCount)
        {
            // Determine match type
            MatchType typeOfMatch = ColumnOrRow();

            if (currentDot != null)
            {
                bool currentDotMatched = currentDot.isMatched && currentDot.tag == typeOfMatch.color;
                dot otherDot = currentDot.otherDot != null ? currentDot.otherDot.GetComponent<dot>() : null;
                bool otherDotMatched = otherDot != null && otherDot.isMatched && otherDot.tag == typeOfMatch.color;

                switch (typeOfMatch.type)
                {
                    case 1:
                        // Color bomb
                        if (currentDotMatched)
                        {
                            currentDot.isMatched = false;
                            currentDot.CookColorBomb();
                        }
                        else if (otherDotMatched)
                        {
                            otherDot.isMatched = false;
                            otherDot.CookColorBomb();
                        }
                        break;
                    case 2:
                        // Wrap bomb
                        if (currentDotMatched)
                        {
                            currentDot.isMatched = false;
                            currentDot.CookWrapBomb();
                        }
                        else if (otherDotMatched)
                        {
                            otherDot.isMatched = false;
                            otherDot.CookWrapBomb();
                        }
                        break;
                    case 3:
                        // Column/Row bomb
                        findMatchesClass.ColumnRowBombsCheck(typeOfMatch);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void DestroyMatchesAt(int column, int row)
    {        
        if (allDots[column, row].GetComponent<dot>().isMatched)
        {

            GameObject currentDot = allDots[column, row];            
            dot curDotGet = currentDot.GetComponent<dot>();

           
            //breakable tiles
            if (breakableCells[column, row] != null)
            {
                breakableCells[column, row].TakeDamage(1);

                if (breakableCells[column, row].hitPoints <= 0)
                {
                    tile_back currentBreak01 = breakableCells[column, row];

                    //particles for break
                    GameObject break01Part = Instantiate(currentBreak01.destroyParticle, allDots[column, row].transform.position, Quaternion.identity);
                    Destroy(break01Part, 0.9f);

                    breakableCells[column, row] = null;
                }
            }

            //lock tiles
            if (lockCells[column, row] != null)
            {
                lockCells[column, row].TakeDamage(1);

                if (lockCells[column, row].hitPoints <= 0)
                {
                    lockCells[column, row] = null;
                }
            }

                     
            //for blockers
            DamageBlocker01(column, row);

            //for slime
            DamageSlime(column, row);

            
            //goal for dots
            if (goalManagerClass != null)
            {
                if (curDotGet.isRowBomb || curDotGet.isColumnBomb)
                {
                    goalManagerClass.CompareGoal(colRowBombsName); //for line bombs
                }else if (curDotGet.isWrapBomb)
                {
                    goalManagerClass.CompareGoal("WrapBomb"); //for Wrap bombs                    
                }else
                {
                    goalManagerClass.CompareGoal(allDots[column, row].tag.ToString()); //for usual dots
                }

                goalManagerClass.UpdateGoals();    
            }


            //sound
            if (soundManagerClass != null)
            {
                soundManagerClass.PlayDestroySound();
            }    

            //particles
            GameObject particleDot = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particleDot, .9f);


            if (allDots[column, row].GetComponent<dot>().isMatched)
            {
                // Optional: Add any additional logic before destroying the match
                scoreManagerClass.IncreaseScore(baseValue * streakValue);
                Destroy(allDots[column, row]);             
                allDots[column, row] = null;
            }

        }
    }

    //for bomb for concrete
    public void BombRow(int row)
    {
        for (int i = 0; i < width; i++)
        {
            if (blocker01Cells[i, row])
            {
                blocker01Cells[i, row].TakeDamage(1);

                if (blocker01Cells[i, row].hitPoints <= 0)
                {
                    blocker01Cells[i, row] = null;
                }
            }
        }
      
    }

    public void BombColumn(int column)
    {   
        for (int i = 0; i < height; i++)
        {
            if (blocker01Cells[column, i])
            {
                blocker01Cells[column, i].TakeDamage(1);

                if (blocker01Cells[column, i].hitPoints <= 0)
                {
                    blocker01Cells[column, i] = null;
                }
            }
        }
    }


    private void DamageBlocker01(int column, int row)
    {
        DamageBlockerAt(column - 1, row);
        DamageBlockerAt(column + 1, row);
        DamageBlockerAt(column, row - 1);
        DamageBlockerAt(column, row + 1);
    }


    private void DamageBlockerAt(int column, int row)
    {
        // Check if the position is within bounds
        if (column >= 0 && column < width && row >= 0 && row < height)
        {
            // Check if there is a blocker at the position
            if (blocker01Cells[column, row])
            {
                // Apply damage
                blocker01Cells[column, row].TakeDamage(1);

                // Log the current blocker
                tile_back currentBlocker = blocker01Cells[column, row];
                
                //particles for break
                GameObject blocker01Part = Instantiate(currentBlocker.destroyParticle, blocker01Cells[column, row].transform.position, Quaternion.identity);
                Destroy(blocker01Part, 0.9f);

                // Remove the blocker if its hit points are 0 or less
                if (blocker01Cells[column, row].hitPoints <= 0)
                {
                    blocker01Cells[column, row] = null;
                }
            }
        }
    }


    private void DamageAdjacentSlime(int column, int row)
    {
        if (slimeCells[column, row])
        {
            slimeCells[column, row].TakeDamage(1);

            if (slimeCells[column, row].hitPoints <= 0)
            {
                slimeCells[column, row] = null;
            }

            makeSlime = false;
        }
    }


    private void DamageSlime(int column, int row)
    {
        if (column > 0)
        {
            DamageAdjacentSlime(column - 1, row);
        }

        if (column < width - 1)
        {
            DamageAdjacentSlime(column + 1, row);
        }

        if (row > 0)
        {
            DamageAdjacentSlime(column, row - 1);
        }

        if (row < height - 1)
        {
            DamageAdjacentSlime(column, row + 1);
        }
    }

    private void CheckToMakeSlime()
    {
        for (int i = 0; i<width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (slimeCells[i,j] && makeSlime)
                {
                    BirthSlimes();
                    return;
                }
            }
        }
    }

    private Vector2 CheckForAdj(int column, int row)
    {

        if (column < width - 1 && allDots[column + 1, row])
        {
            return Vector2.right;
        }

        if (column > 0 && allDots[column - 1, row])
        {
            return Vector2.left;
        }


        if (row < height - 1 && allDots[column, row + 1])
        {
            return Vector2.up;
        }


        if (row > 0 && allDots[column, row - 1])
        {
            return Vector2.down;
        }

        return Vector2.zero;
    }


    private void BirthSlimes()
    {
        bool slimeBorn = false;
        int loops = 0;
        const int maxLoops = 200;

        while (!slimeBorn && loops < maxLoops)
        {
            int newX = UnityEngine.Random.Range(0, width);
            int newY = UnityEngine.Random.Range(0, height);

            if (slimeCells[newX, newY])
            {
                Vector2 adj = CheckForAdj(newX, newY);

                if (adj != Vector2.zero)
                {
                    int adjX = newX + (int)adj.x;
                    int adjY = newY + (int)adj.y;

                    Destroy(allDots[adjX, adjY]);
                    Vector2 tempPos = new Vector2(adjX, adjY);

                    // Add slime
                    GameObject tile = Instantiate(slimeTilePrefab, tempPos, Quaternion.identity);
                    slimeCells[adjX, adjY] = tile.GetComponent<tile_back>();

                    slimeBorn = true;
                }
            }

            loops++;
        }
    }

    public void DestroyMatches()
    {       
        //match count
        if (findMatchesClass.currentMatch.Count >= minMatchForBomb)
        {
            CheckToCookBombs();
        }

        //clear match
        findMatchesClass.currentMatch.Clear();

        for (int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);                    
                }
            }
        }

        //findMatchesClass.currentMatch.Clear();

        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !blankCells[i, j] && !blocker01Cells[i, j] && !slimeCells[i, j])
                {
                    for (int k = j + 1; k < height; k++)
                    {
                        if (allDots[i, k] != null)
                        {
                            // Move dot to the new position
                            allDots[i, k].GetComponent<dot>().row = j;
                            allDots[i, j] = allDots[i, k]; // Move reference to the new position
                            allDots[i, k] = null; // Clear the old position
                            break;
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.4f); //refillDelay * 1.0f

        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        int counter = 0;
        string currentTime = DateTime.Now.ToString("mmss");

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !blankCells[i, j] && !blocker01Cells[i, j] && !slimeCells[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = UnityEngine.Random.Range(0, dots.Length);
                    int maxIter = 0;

                    while (MatchesAt(i, j, dots[dotToUse]) && maxIter < 200)
                    {
                        dotToUse = UnityEngine.Random.Range(0, dots.Length);
                        maxIter++;
                    }

                    GameObject element = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = element;

                    // Set dot properties
                    dot dotComponent = element.GetComponent<dot>();
                    dotComponent.row = j;
                    dotComponent.column = i;

                    counter++;
                    element.name = $"{element.tag}_{currentTime}_{counter}";
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        findMatchesClass.FindAllMatches();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if(allDots[i, j].GetComponent<dot>().isMatched)
                    {
                        //Debug.Log("MatchesOnBoard!");
                        return true;
                    }
                }
            }
        }

        return false;
    }


    private IEnumerator FillBoardCo()
    {
        
        yield return new WaitForSeconds(refillDelay);
        
        RefillBoard();

        yield return new WaitForSeconds(refillDelay);

        while (MatchesOnBoard())
        {
            streakValue++; //for score            
            DestroyMatches();
            yield break;          
        }
        
        //for bomb
        currentDot = null;

        CheckToMakeSlime();


        if (IsDeadLock())
        {
            ShuffleBoard();
        }

        yield return new WaitForSeconds(refillDelay);

        if(currentState != GameState.pause)
            currentState = GameState.move;

        makeSlime = true;

        streakValue = 1;
    }

    private void SwitchPieces (int column, int row, Vector2 direction)
    {
        if (allDots[column + (int)direction.x, row + (int)direction.y]!= null)
        {
            GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;

            allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];

            allDots[column, row] = holder;
        }
    }

    private bool CheckForMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                
                
                if (allDots[i, j] != null)
                {
                    if (i < width - 2)
                    {                    
                        if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
                        {
                            if (allDots[i + 1, j].tag == allDots[i, j].tag && allDots[i + 2, j].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                

                    if (j < height - 2)
                    {                    
                        if (allDots[i, j + 1] != null && allDots[i, j  +2] != null)
                        {
                            if (allDots[i, j + 1].tag == allDots[i, j].tag && allDots[i, j+2].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
                
        return false;
    }


    public bool SwithAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);

        if (CheckForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }

        SwitchPieces(column, row, direction);
        return false;
    }


    private bool IsDeadLock()
    {      
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (i < width - 1)
                    {
                        if (SwithAndCheck(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }

                    if (j < height - 1)
                    {
                        if (SwithAndCheck(i, j, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }


    private void ShuffleBoard()
    {
        //for game obj
        List<GameObject> newBoard = new List<GameObject>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }


        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankCells[i, j] && !blocker01Cells[i, j] && !slimeCells[i, j]) //list of not
                {
                    int cellToUse = UnityEngine.Random.Range(0, newBoard.Count);

                    int maxItertion = 0;

                    //board without match
                    while (MatchesAt(i, j, newBoard[cellToUse]) && maxItertion < 100)
                    {
                        cellToUse = UnityEngine.Random.Range(0, newBoard.Count);
                        maxItertion++;
                    }

                    //container
                    dot piece = newBoard[cellToUse].GetComponent<dot>();

                    maxItertion = 0;

                    //assign col
                    piece.column = i;                    

                    //assig row
                    piece.row = j;

                    allDots[i, j] = newBoard[cellToUse];

                    newBoard.Remove(newBoard[cellToUse]);

                }
            }
        }

        if(IsDeadLock())
        {
            ShuffleBoard();
        }

    }

}
