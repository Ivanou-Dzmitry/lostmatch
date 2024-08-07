using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEditor;

public class find_matches : MonoBehaviour
{

    private game_board gameBoardClass;
    public TMP_Text debugText;

    [HideInInspector]
    public List<GameObject> currentMatch = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        gameBoardClass = GameObject.FindWithTag("GameBoard").GetComponent<game_board>();
    }


    //for match
    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }


    //list for wrap
    private List<GameObject> IsWrapBomb(dot dot01, dot dot02, dot dot03)
    {
        List<GameObject> currentDots = new List<GameObject>();


        if (dot01.isWrapBomb)
        {            
            currentMatch.AddRange(GetWrapPieces(dot01.column, dot01.row));
        }

        if (dot02.isWrapBomb)
        {
            currentMatch.AddRange(GetWrapPieces(dot02.column, dot02.row));
        }

        if (dot03.isWrapBomb)
        {
            currentMatch.AddRange(GetWrapPieces(dot03.column, dot03.row));
        }

        return currentDots;
    }

    //row bomb list
    private List<GameObject> IsRowBomb(dot dot01, dot dot02, dot dot03)
    {
        List<GameObject> currentDots = new List<GameObject>();

        if (dot01.isRowBomb)
        {
            currentMatch.AddRange(GetRowPieces(dot01.row));
            gameBoardClass.BombRow(dot01.row);
        }

        if (dot02.isRowBomb)
        {
            currentMatch.AddRange(GetRowPieces(dot02.row));
            gameBoardClass.BombRow(dot02.row);
        }

        if (dot03.isRowBomb)
        {
            currentMatch.AddRange(GetRowPieces(dot03.row));
            gameBoardClass.BombRow(dot03.row);
        }
          
        return currentDots;
    }

    //column bomb list
    private List<GameObject> IsColumnBomb(dot dot01, dot dot02, dot dot03)
    {
        List<GameObject> currentDots = new List<GameObject>();


        if (dot01.isColumnBomb)
        {
            currentMatch.AddRange(GetColumnPieces(dot01.column));
            gameBoardClass.BombColumn(dot01.column);
        }

        if (dot02.isColumnBomb)
        {
            currentMatch.AddRange(GetColumnPieces(dot02.column));
            gameBoardClass.BombColumn(dot02.column);
        }

        if (dot03.isColumnBomb)
        {
            currentMatch.AddRange(GetColumnPieces(dot03.column));
            gameBoardClass.BombColumn(dot03.column);
        }

        return currentDots;
    }

    private void AddToListMatch(GameObject dot)
    {
        if (!currentMatch.Contains(dot))
        {
            currentMatch.Add(dot);            
        }

        dot.GetComponent<dot>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot01, GameObject dot02, GameObject dot03)
    {
        if (dot01 != null)        
            AddToListMatch(dot01);
        
        if (dot02 != null)
            AddToListMatch(dot02);
        
        if (dot03 != null)
            AddToListMatch(dot03);
    }

    private IEnumerator FindAllMatchesCo()
    {        
        yield return null;

        for (int i = 0; i < gameBoardClass.width; i++)
        {
            for (int j = 0; j < gameBoardClass.height; j++)
            {
                GameObject currentDot = gameBoardClass.allDots[i, j]; //store board
                
                if (currentDot != null)                    
                {
                    dot curDotGet = currentDot.GetComponent<dot>(); //get first dot

                    //horizontal
                    if (i > 0 && i < gameBoardClass.width - 1)
                    {
                        //get neiborhood
                        GameObject leftDot = gameBoardClass.allDots[i - 1, j];                        
                        GameObject rightDot = gameBoardClass.allDots[i + 1, j];

                        if (leftDot != null && rightDot != null)
                        {
                            //get neiborhood dots
                            dot leftDotGet = leftDot.GetComponent<dot>(); //get 2nd dot
                            dot rightDotGet = rightDot.GetComponent<dot>(); //get 3rd dot
                        
                            if (leftDot != null && rightDot != null)
                            {
                                if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag) //compare tags form lr dots
                                {
                                    //row bomb
                                    currentMatch.AddRange(IsRowBomb(leftDotGet, curDotGet, rightDotGet));

                                    //column bomb
                                    currentMatch.AddRange(IsColumnBomb(leftDotGet, curDotGet, rightDotGet));

                                    //wrap bomb
                                    currentMatch.AddRange(IsWrapBomb(leftDotGet, curDotGet, rightDotGet));

                                    GetNearbyPieces(leftDot, currentDot, rightDot);
                                }
                            }
                        }
                    }

                    //vertical
                    if (j > 0 && j < gameBoardClass.height - 1)
                    {
                        GameObject upDot = gameBoardClass.allDots[i, j+1];                        
                        GameObject downDot = gameBoardClass.allDots[i, j-1];

                        if (upDot != null && downDot != null)
                        {
                            dot upDotGet = upDot.GetComponent<dot>(); //optimize
                            dot downDotGet = downDot.GetComponent<dot>(); //optimize
                        
                            if (upDot != null && downDot != null)
                            {
                                if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                                {

                                    //column bobm
                                    currentMatch.AddRange(IsColumnBomb(upDotGet, curDotGet, downDotGet));

                                    //row bomb
                                    currentMatch.AddRange(IsRowBomb(upDotGet, curDotGet, downDotGet));

                                    //wrap bomb
                                    currentMatch.AddRange(IsWrapBomb(upDotGet, curDotGet, downDotGet));

                                    GetNearbyPieces(upDot, currentDot, downDot);
                                }
                            }
                        }
                    }
                }
            }
        }

    }


    //color bobmb
    public void MatchColorPieces(string color)
    {
        for (int i = 0; i < gameBoardClass.width; i++)
        {
            for (int j = 0; j < gameBoardClass.height; j++)
            {
                if (gameBoardClass.allDots[i, j] != null)
                {
                    if (gameBoardClass.allDots[i, j].tag == color)
                    {
                        gameBoardClass.allDots[i,j].GetComponent<dot>().isMatched = true;
                    }
                }
            }
        }

    }

    //wpap bomb
    List<GameObject> GetWrapPieces(int column, int row)
    {
        List<GameObject> dots = new List<GameObject>();

        //only around
        for (int i = column - 1; i <= column+1; i++)
        {
            for (int j = row - 1; j <= row+1 ; j++)
            {
                //for border
                if(i >= 0 && i < gameBoardClass.width && j>= 0 && j < gameBoardClass.height)
                {
                    //fix bug
                    if (gameBoardClass.allDots[i,j] != null)
                    {
                        dots.Add(gameBoardClass.allDots[i, j]);
                        gameBoardClass.allDots[i, j].GetComponent<dot>().isMatched = true;
                    }
                }
            }
        }

        return dots;
    }

    //for column bomb
    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();

        for(int i = 0; i < gameBoardClass.height; i++)
        {
            if (gameBoardClass.allDots[column, i] != null)
            {

                dot localDot = gameBoardClass.allDots[column, i].GetComponent<dot>();

                if (localDot.isRowBomb)
                {
                    dots.Union(GetRowPieces(i)).ToList();
                }

                dots.Add(gameBoardClass.allDots[column,i]);

                localDot.isMatched = true;
            }
        }

        
        return dots;
    }

    //for row bobm
    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();

        for (int i = 0; i < gameBoardClass.width; i++)
        {
            if (gameBoardClass.allDots[i, row] != null)
            {

                dot localDot = gameBoardClass.allDots[i, row].GetComponent<dot>();

                if (localDot.isColumnBomb)
                {
                    dots.Union(GetColumnPieces(i)).ToList();
                }

                dots.Add(gameBoardClass.allDots[i, row]);
                localDot.isMatched = true;
            }
        }

        return dots;
    }


    public void ColumnRowBombsCheck(MatchType matchType)
    {
        //move or not move?
        if (gameBoardClass.currentDot != null)
        {
            if (gameBoardClass.currentDot.isMatched && gameBoardClass.currentDot.tag == matchType.color)
            {
                //unmatch
                gameBoardClass.currentDot.isMatched = false;

                float angle1 = 0;

                angle1 = gameBoardClass.currentDot.swipeAngle;
      
                //for swipe
                if ((angle1 > -45 && angle1 <= 45) || (angle1 < -135 || angle1 >= 135))
                {
                    gameBoardClass.currentDot.CookRowBomb();
                }
                else
                {
                    gameBoardClass.currentDot.CookColumnBomb();
                }      


            } else if (gameBoardClass.currentDot.otherDot != null)
            {
                dot otherDot = gameBoardClass.currentDot.otherDot.GetComponent<dot>();

                //if other dots matched
                if (otherDot.isMatched && otherDot.tag == matchType.color)
                {
                    otherDot.isMatched = false;

                    float angle2 = 0;

                    angle2 = gameBoardClass.currentDot.swipeAngle;

                    //for swipe
                    if ((angle2 > -45 && angle2 <= 45) || (angle2 < -135 || angle2 >= 135))
                    {
                        otherDot.CookRowBomb();
                        otherDot.dotSound = otherDot.rowBombSound; // sound
                    }
                    else
                    {
                        otherDot.CookColumnBomb();
                        otherDot.dotSound = otherDot.colBombSound; //sound
                    }
                }
            }
        }
    }


    private void DebugText(string text)
    {
        debugText.text = debugText.text + ", " + text;
    }

}
