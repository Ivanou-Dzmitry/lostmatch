using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;


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
    Concrete,
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
    public GameObject breakableTilePrefab; 
    public GameObject lockTilePrefab; //lock object
    public GameObject concreteTilePrefab; //lock object

    public GameObject[] dots;
    public GameObject[,] allDots;

    public GameObject destroyEffect;
    
    [Header("Layout")]
    public TileType[] boardLayout;
    //for blank
    private bool[,] blankCells;
    //for break
    private tile_back[,] breakableCells;
    //for lock
    public tile_back[,] lockCells;
    //for concrete
    public tile_back[,] concreteCells;



    [Header("Match Suff")]
    public MatchType matchTypeClass;
    public dot currentDot;

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
        concreteCells = new tile_back[width, height];

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

    private void GenConcreteTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Concrete)
            {
                Vector2 tempPos = new Vector2(boardLayout[i].x, boardLayout[i].y);

                GameObject tile = Instantiate(concreteTilePrefab, tempPos, Quaternion.identity);

                concreteCells[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<tile_back>();

                Vector2 tilePosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject boardTile1 = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                boardTile1.transform.parent = this.transform;

            }
        }
    }


    private void SetUp()
    {
        GenBlankCells();

        GenBreakTiles();

        GenLockTiles();

        GenConcreteTiles();

        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if (!blankCells[i, j] && !concreteCells[i, j])
                {              
                //temp position and offset
                Vector2 tempPos = new Vector2(i, j + offSet);
                Vector2 tilePos = new Vector2(i, j);

                //add background
                GameObject backgroudTile = Instantiate(tilePrefab, tilePos, Quaternion.identity) as GameObject;
                backgroudTile.transform.parent = this.transform;
                backgroudTile.name = "(" + i + "-" + j + ")";
               
                //add elements
                int dotToUse = Random.Range(0, dots.Length);

                int maxItertion = 0;

                //board without match
                while(MatchesAt(i, j, dots[dotToUse]) && maxItertion < 100)
                {
                    dotToUse = Random.Range(0, dots.Length);
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
                dot.name = "(" + i + "-" + j + ")";

                

                //add elements to array
                allDots[i,j] = dot;

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

        matchTypeClass.type = 0;
        matchTypeClass.color = "";

        //cycle
        for (int i = 0; i < matchCopy.Count; i++)
        {
            //sotore this dot
            dot thisDot = matchCopy[i].GetComponent<dot>();

            string color = matchCopy[i].tag; //for class

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

            if (columnMatch == matchForRowColBomb || rowMatch == matchForRowColBomb) //column bomb
            {
                matchTypeClass.type = 1;
                matchTypeClass.color = color;
                return matchTypeClass; 
            } else if (columnMatch == matchForWrapBomb || rowMatch == matchForWrapBomb) // wrap
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
        /*        int horNum = 0;
        int vertNum = 0;

        dot firstPiese = findMatchesClass.currentMatch[0].GetComponent<dot>();
        
        if (firstPiese != null)
        {

            foreach (GameObject currenPiece in findMatchesClass.currentMatch)
            {
                dot dot = currenPiece.GetComponent<dot>();

                if (dot.row == firstPiese.row)
                {
                    horNum++;
                }

                if (dot.column == firstPiese.column)
                {
                    vertNum++;
                }
            }
        }

        return (vertNum == 5 || horNum == 5);*/
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
                findMatchesClass.CheckBombs(typeOfMatch);
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

            DamageConcrete(column, row);

            //goal
            if (goalManagerClass != null)
            {
                goalManagerClass.CompareGoal(allDots[column, row].tag.ToString());
                goalManagerClass.UpdatesGoals();    
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

            //score
            scoreManagerClass.IncreaseScore(baseValue * streakValue);

            allDots[column,row] = null;
        }
    }

    //for bomb for concrete
    public void BombRow(int row)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (concreteCells[i, j])
                {
                    concreteCells[i, row].TakeDamage(1);

                    if (concreteCells[i, row].hitPoints <= 0)
                    {
                        concreteCells[i, row] = null;
                    }
                }
            }
        }
    }

    public void BombColumn(int column)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (concreteCells[i, j])
                {
                    concreteCells[column, i].TakeDamage(1);

                    if (concreteCells[column, i].hitPoints <= 0)
                    {
                        concreteCells[column, i] = null;
                    }
                }
            }
        }
    }

    private void DamageConcrete(int column, int row)
    {
        if(column > 0)
        {
            if (concreteCells[column -1, row])
            {
                concreteCells[column-1, row].TakeDamage(1);

                if (concreteCells[column-1, row].hitPoints <= 0)
                {
                    concreteCells[column-1, row] = null;
                }
            }
        }

        if (column <  width -1 )
        {            
            if (concreteCells[column + 1, row])
            {
                concreteCells[column + 1, row].TakeDamage(1);

                if (concreteCells[column + 1, row].hitPoints <= 0)
                {
                    concreteCells[column + 1, row] = null;
                }
            }
        }

        if (row > 0)
        {
            if (concreteCells[column, row - 1])
            {
                concreteCells[column, row-1].TakeDamage(1);

                if (concreteCells[column, row - 1].hitPoints <= 0)
                {
                    concreteCells[column, row - 1] = null;
                }
            }
        }

        if (row < height -1)
        {
            if (concreteCells[column, row + 1])
            {

                concreteCells[column, row+1].TakeDamage(1);

                if (concreteCells[column, row + 1].hitPoints <= 0)
                {
                    concreteCells[column, row + 1] = null;
                }
            }
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
                if (allDots[i, j] == null && !blankCells[i, j] && !concreteCells[i, j]) //add skip to fill cells
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
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !blankCells[i,j] && !concreteCells[i, j]) //not refill here
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);

                    int dotToUse = Random.Range(0, dots.Length);

                    int maxIter = 0;
                    
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIter<100)
                    {
                        maxIter++;
                        dotToUse = Random.Range(0, dots.Length);
                    }
                    
                    maxIter = 0;

                    GameObject element = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);                    
                    allDots[i, j] = element;

                    //for offset
                    element.GetComponent<dot>().row = j;
                    element.GetComponent<dot>().column = i;
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
            streakValue ++; //for score
            DestroyMatches();
            yield break;          
        }
        
        //for bomb
        currentDot = null;
        
        if (IsDeadLock())
        {
            ShuffleBoard();
        }

        yield return new WaitForSeconds(refillDelay);

        currentState = GameState.move;
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
                if (!blankCells[i, j] && !concreteCells[i, j])
                {
                    int cellToUse = Random.Range(0, newBoard.Count);

                    int maxItertion = 0;

                    //board without match
                    while (MatchesAt(i, j, newBoard[cellToUse]) && maxItertion < 100)
                    {
                        cellToUse = Random.Range(0, newBoard.Count);
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
