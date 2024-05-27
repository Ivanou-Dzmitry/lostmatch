using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum GameState
{
    wait,
    move,
    win,
    lose,
    pause
}

public enum TileKind
{
    Breakable,
    Blank,
    Lock,
    Blocker01,
    Slime,
    Normal
}

[System.Serializable]
public class MatchType
{
    public int type;
    public string color;
}

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

    public float refillDelay = .5f;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject tileCornerPrefab;

    public GameObject breakableTilePrefab; 
    public GameObject lockTilePrefab; //lock object
    public GameObject blocker01Prefab; //lock object
    public GameObject slimeTilePrefab; //lock object

    public GameObject[] dots;
    public GameObject[,] allDots;

    //for corner background
    List<Vector2> cornerCoords = new List<Vector2>();
    List<Vector3> cornerRotation = new List<Vector3>();

    public GameObject destroyEffect;
    
    [Header("Layout")]
    public TileType[] boardLayout;
    
    //for blank
    private bool[,] blankCells;
    
    //for break
    private tile_back[,] breakableCells;
    private tile_back[,] slimeCells;
    
    //for lock
    public tile_back[,] lockCells;
    
    //for blocker_01
    public tile_back[,] blocker01Cells;



    [Header("Match Suff")]
    public MatchType matchTypeClass;
    public dot currentDot;

    //classes
    private find_matches findMatchesClass;
    private score_manager scoreManagerClass;
    private sound_manager soundManagerClass;
    private goal_manager goalManagerClass;
    private bool makeSlime = true;

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
                }

            }
        }
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

        allDots = new GameObject[width, height];

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
            if (boardLayout[i].tileKind == TileKind.Breakable)
            {
                Vector2 tempPos = new Vector2(boardLayout[i].x, boardLayout[i].y);

                GameObject tile = Instantiate(breakableTilePrefab, tempPos, Quaternion.identity);

                breakableCells[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<tile_back>();
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
            }
        }
    }

    //bubble gum
    private void GenBlock01Tiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Blocker01)
            {                
                Vector2 tempPos = new Vector2(boardLayout[i].x, boardLayout[i].y);

                GameObject tile = Instantiate(blocker01Prefab, tempPos, Quaternion.identity);

                blocker01Cells[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<tile_back>();

                //add back tile
                Vector2 tilePosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject boardTile1 = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                boardTile1.transform.parent = this.transform;
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

                GameObject tile = Instantiate(slimeTilePrefab, tempPos, Quaternion.identity);

                slimeCells[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<tile_back>();

                //add back tile
                Vector2 tilePosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject boardTile1 = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                boardTile1.transform.parent = this.transform;
            }
        }
    }

    // setub board
    private void SetUp()
    {
        GenBlankCells();

        GenBreakTiles();

        GenLockTiles();

        GenBlock01Tiles();

        GenSlimeTiles();

        int counter = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankCells[i, j] && !blocker01Cells[i, j] && !slimeCells[i, j])
                {
                //temp position and offset
                Vector2 tempPos = new Vector2(i, j + offSet);
                Vector2 tilePos = new Vector2(i, j);                   
                                   
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

                //instance
                GameObject dot = Instantiate(dots[dotToUse], tempPos, Quaternion.identity);

                //for offset
                dot.GetComponent<dot>().row = j;
                dot.GetComponent<dot>().column = i;

                //set properties
                dot.transform.parent = this.transform;

                counter++;

                dot.name = dot.tag + "_" + counter;
              
                //add elements to array
                allDots[i,j] = dot;                
                }
            }
        }

        int columnsAllDots = allDots.GetLength(0);
        int rowsAllDots = allDots.GetLength(1);

        //left bottom
        for (int i = 0; i < columnsAllDots; i++)
        {
            for (int j = 0; j < rowsAllDots; j++)
            {
                if (allDots[i, j] != null)
                {
                    //add corner position
                    Vector2 tempCorner = new Vector2(allDots[i, j].transform.position.x, (allDots[i, j].transform.position.y - offSet));
                    cornerCoords.Add(tempCorner);

                    //add corenr rotation
                    Vector3 tempAngle = new Vector3(0, 0, 0);
                    cornerRotation.Add(tempAngle);
                    break;
                }
            }

            if (allDots[i, 0] != null)
                break;
        }

        //left top
        for (int i = 0; i < columnsAllDots; i++)
        {
            for (int j = rowsAllDots - 1; j >= 0; j--)
            {
                if (allDots[i, j] != null)
                {
                    Vector2 tempCorner = new Vector2(allDots[i, j].transform.position.x, (allDots[i, j].transform.position.y - offSet));
                    cornerCoords.Add(tempCorner);

                    Vector3 tempAngle = new Vector3(0, 0, -90);
                    cornerRotation.Add(tempAngle);
                    break;
                }
            }

            if (allDots[i, rowsAllDots-1] != null)
                break;
        }


        //bottom right
        for (int i = columnsAllDots - 1; i >= 0; i--)
        {
            for (int j = 0; j < rowsAllDots; j++)
            {
                if (allDots[i, j] != null)
                {
                    Vector2 tempCorner = new Vector2(allDots[i, j].transform.position.x, (allDots[i, j].transform.position.y - offSet));
                    cornerCoords.Add(tempCorner);

                    Vector3 tempAngle = new Vector3(0, 0, 90);
                    cornerRotation.Add(tempAngle);
                    break;
                }
            }

            if (allDots[i, 0] != null)
                break;
        }


        //top right
        for (int i = columnsAllDots - 1; i >= 0; i--)
        {
            for (int j = rowsAllDots - 1; j >= 0; j--)
            {
                if (allDots[i, j] != null)
                {
                    Vector2 tempCorner = new Vector2(allDots[i, j].transform.position.x, (allDots[i, j].transform.position.y - offSet));
                    cornerCoords.Add(tempCorner);

                    Vector3 tempAngle = new Vector3(0, 0, 180);
                    cornerRotation.Add(tempAngle);
                    break;
                }
            }

            if (allDots[i, rowsAllDots - 1] != null)
                break;
        }

        counter = 0;

        //fill back corners
        for (int i = 0; i < cornerCoords.Count; i++)
        {
            GameObject backTileCorner = Instantiate(tileCornerPrefab, cornerCoords[i], Quaternion.Euler(cornerRotation[i])) as GameObject;
            backTileCorner.transform.parent = this.transform;
            counter++;
            backTileCorner.name = "" + counter + "_BackCorner" + cornerCoords[i].x + "-" + cornerCoords[i].y;
        }

        counter = 0;

        for (int i = 0; i < columnsAllDots; i++)
        {
            for (int j = 0; j < rowsAllDots; j++)
            {
                if (!blankCells[i, j] && !blocker01Cells[i, j] && !slimeCells[i, j])
                {

                    Vector2 tilePos = new Vector2(i, j);

                    //skip corners
                    if (!cornerCoords.Contains(tilePos))
                    {
                        //add background                        
                        GameObject backgroudTile = Instantiate(tilePrefab, tilePos, Quaternion.identity) as GameObject;
                        backgroudTile.transform.parent = this.transform;
                        counter++;
                        backgroudTile.name = "" + counter + "_Back" + i + "-" + j;
                    }

                }
            }
        }
        

    }

    private bool MatchesAt(int column, int row, GameObject element)
    {
        if (column > 1 && row > 1)
        {
            //for blank
            if (allDots[column-1, row] != null && allDots[column - 2, row] != null)
            {            
                if (allDots[column - 1, row].tag == element.tag && allDots[column - 2, row].tag == element.tag)
                {
                    return true;
                }
            }

            //for blank
            if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
            {
                if (allDots[column, row - 1].tag == element.tag && allDots[column, row - 2].tag == element.tag)
                {
                    return true;
                }
            }

        } else if(column <= 1 || row <= 1)
        {
            if(row > 1)
            {
                if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
                {
                    if (allDots[column, row - 1].tag == element.tag && allDots[column, row - 2].tag == element.tag)
                        return true;
                }

            }

            if (column > 1)
            {
                if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
                {
                    if (allDots[column - 1, row].tag == element.tag && allDots[column - 2, row].tag == element.tag)
                        return true;
                }
            }

        }
        return false;
    }

    private MatchType ColumnOrRow()
    {
        //copy of cur match
        List<GameObject> matchCopy = findMatchesClass.currentMatch as List<GameObject>;

        //Debug.Log("ColumnOrRow - matchCopy: " + matchCopy.Count);

        matchTypeClass.type = 0;
        matchTypeClass.color = "";

        //cycle
        for (int i = 0; i < matchCopy.Count; i++)
        {
            //sotore this dot
            dot thisDot = matchCopy[i].GetComponent<dot>();

            //get color
            string color = matchCopy[i].tag; //for class

            //get column
            int column = thisDot.column;
            int row = thisDot.row;

            int columnMatch = 0;
            int rowMatch = 0;

            //cycle 2
            for (int j = 0; j < matchCopy.Count; j++)
            {
                //store next
                dot nextDot = matchCopy[j].GetComponent<dot>();

                if (nextDot == thisDot)
                {
                    continue;
                }

                if(nextDot.column == thisDot.column && nextDot.tag == color)
                {
                    columnMatch++;  
                }

                if (nextDot.row == thisDot.row && nextDot.tag == color)
                {
                    rowMatch++;
                }
            }

            //Debug.Log("columnMatch:" + columnMatch + "/ rowMatch: " + rowMatch);

            if (columnMatch == matchForRowColBomb || rowMatch == matchForRowColBomb) //column bomb
            {
                matchTypeClass.type = 1;
                matchTypeClass.color = color;
                return matchTypeClass; 
            } else if (columnMatch == matchForWrapBomb && rowMatch == matchForWrapBomb) // wrap
            {
                matchTypeClass.type = 2;
                matchTypeClass.color = color;
                return matchTypeClass;   
            } else if (columnMatch == matchForColorBomb || rowMatch == matchForColorBomb) //color bomb
            {
                matchTypeClass.type = 3;
                matchTypeClass.color = color;
                return matchTypeClass; 
            }
        }

        matchTypeClass.type = 0;
        matchTypeClass.color = "";

        return matchTypeClass;        
    }


    private void CheckToCookBombs()
    {
        if(findMatchesClass.currentMatch.Count > minMatchCount)
        {
            //match type
            MatchType typeOfMatch = ColumnOrRow();

            if (typeOfMatch.type == 1)
            {
                //color bomb
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOfMatch.color)
                {
                        currentDot.isMatched = false;
                        currentDot.CookColorBomb();
                }
                else
                {
                    if (currentDot.otherDot != null)
                    {
                        dot otherDot = currentDot.otherDot.GetComponent<dot>();

                        if (otherDot.isMatched && otherDot.tag == typeOfMatch.color)
                        {
                            otherDot.isMatched = false;
                            otherDot.CookColorBomb();
                        }
                    }
                }
            } else if (typeOfMatch.type == 2)
            {
                //wrap bomb
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOfMatch.color)
                {
                    currentDot.isMatched = false;
                    currentDot.CookWrapBomb();
                }
                else if (currentDot.otherDot != null)
                {                    
                    dot otherDot = currentDot.otherDot.GetComponent<dot>();

                    if (otherDot.isMatched && otherDot.tag == typeOfMatch.color)
                    {
                        otherDot.isMatched = false;
                        otherDot.CookWrapBomb();
                    }
                }
            } else if (typeOfMatch.type == 3)
            {
                findMatchesClass.ColumnRowBombsCheck(typeOfMatch);
            }
        }        
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<dot>().isMatched)
        {
            //breakable tiles
            if (breakableCells[column, row] != null)
            {
                breakableCells[column, row].TakeDamage(1);

                if (breakableCells[column, row].hitPoints <= 0)
                {
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

            //goal
            if (goalManagerClass != null)
            {
                goalManagerClass.CompareGoal(allDots[column, row].tag.ToString());
                goalManagerClass.UpdateGoals();    
            }


            //sound
            if (soundManagerClass != null)
            {
                soundManagerClass.PlayDestroySound();
            }

            //particles
            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, .6f);

            Destroy(allDots[column, row]);

            scoreManagerClass.IncreaseScore(baseValue * streakValue);

            allDots[column,row] = null;
        }
    }

    //for bomb for concrete
    public void BombRow(int row)
    {
        for (int i = 0; i < width; i++)
        {
           Debug.Log("row: " + row);

            if (blocker01Cells[i, row])
            {
                blocker01Cells[i, row].TakeDamage(1);

                if (blocker01Cells[i, row].hitPoints <= 0)
                {
                    blocker01Cells[i, row] = null;
                }
            }
        }

        //Debug.Log("Bomb Row Created!");
    }

    public void BombColumn(int column)
    {
        Debug.Log("column: " + column);

        for (int i = 0; i < width; i++)
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

        //Debug.Log("Bomb Column Created!");
    }

    private void DamageBlocker01(int column, int row)
    {
        if(column > 0)
        {
            if (blocker01Cells[column -1, row])
            {
                blocker01Cells[column-1, row].TakeDamage(1);

                if (blocker01Cells[column-1, row].hitPoints <= 0)
                {
                    blocker01Cells[column-1, row] = null;
                }
            }
        }

        if (column <  width -1 )
        {            
            if (blocker01Cells[column + 1, row])
            {
                blocker01Cells[column + 1, row].TakeDamage(1);

                if (blocker01Cells[column + 1, row].hitPoints <= 0)
                {
                    blocker01Cells[column + 1, row] = null;
                }
            }
        }

        if (row > 0)
        {
            if (blocker01Cells[column, row - 1])
            {
                blocker01Cells[column, row-1].TakeDamage(1);

                if (blocker01Cells[column, row - 1].hitPoints <= 0)
                {
                    blocker01Cells[column, row - 1] = null;
                }
            }
        }

        if (row < height -1)
        {
            if (blocker01Cells[column, row + 1])
            {

                blocker01Cells[column, row+1].TakeDamage(1);

                if (blocker01Cells[column, row + 1].hitPoints <= 0)
                {
                    blocker01Cells[column, row + 1] = null;
                }
            }
        }
    }

    private void DamageSlime(int column, int row)
    {
        if (column > 0)
        {
            if (slimeCells[column - 1, row])
            {
                slimeCells[column - 1, row].TakeDamage(1);

                if (slimeCells[column - 1, row].hitPoints <= 0)
                {
                    slimeCells[column - 1, row] = null;
                }
                makeSlime = false;
            }
        }

        if (column < width - 1)
        {
            if (slimeCells[column + 1, row])
            {
                slimeCells[column + 1, row].TakeDamage(1);

                if (slimeCells[column + 1, row].hitPoints <= 0)
                {
                    slimeCells[column + 1, row] = null;
                }

                makeSlime = false;
            }
        }

        if (row > 0)
        {
            if (slimeCells[column, row - 1])
            {
                slimeCells[column, row - 1].TakeDamage(1);

                if (slimeCells[column, row - 1].hitPoints <= 0)
                {
                    slimeCells[column, row - 1] = null;
                }

                makeSlime = false;
            }
        }

        if (row < height - 1)
        {
            if (slimeCells[column, row + 1])
            {

                slimeCells[column, row + 1].TakeDamage(1);

                if (slimeCells[column, row + 1].hitPoints <= 0)
                {
                    slimeCells[column, row + 1] = null;
                }

                makeSlime = false;
            }
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
        bool slime = false;

        int loops = 0;

        while (!slime && loops < 200)
        {
            int newX = UnityEngine.Random.Range(0, width);  
            int newY = UnityEngine.Random.Range(0, height);

            if (slimeCells[newX, newY])
            {
                Vector2 adj = CheckForAdj(newX, newY);  

                if (adj != Vector2.zero)
                {
                    Destroy(allDots[newX + (int)adj.x, newY + (int)adj.y]);
                    Vector2 tempPos = new Vector2(newX + (int)adj.x, newY + (int)adj.y);

                    //add slime
                    GameObject tile = Instantiate(slimeTilePrefab, tempPos, Quaternion.identity);
                    slimeCells[newX + (int)adj.x, newY + (int)adj.y] = tile.GetComponent<tile_back>();

                    slime = true;
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

        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !blankCells[i, j] && !blocker01Cells[i, j] && !slimeCells[i, j]) //add skip to fill cells
                {

                    for(int k = j+1; k < height; k++)
                    {
                        if (allDots[i, k] != null)
                        {
                            allDots[i, k].GetComponent<dot>().row = j;
                            allDots[i, k] = null;
                            break;
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        int counter = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !blankCells[i,j] && !blocker01Cells[i, j] && !slimeCells[i, j]) //not refill here
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);

                    int dotToUse = UnityEngine.Random.Range(0, dots.Length);

                    int maxIter = 0;
                    
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIter<100)
                    {
                        maxIter++;
                        dotToUse = UnityEngine.Random.Range(0, dots.Length);
                    }
                    
                    maxIter = 0;

                    GameObject element = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);                    
                    allDots[i, j] = element;

                    //for offset
                    element.GetComponent<dot>().row = j;
                    element.GetComponent<dot>().column = i;

                    counter++;

                    //for naming
                    DateTime now = DateTime.Now;
                    string currentTime = now.ToString("mmss");

                    element.name = element.tag + "_" + currentTime + "_" + counter;
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

        //Debug.Log("Refill");
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
