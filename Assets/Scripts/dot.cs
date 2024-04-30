using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class dot : MonoBehaviour
{
    private Vector2 firstTouchPos = Vector2.zero; //default
    private Vector2 finalTouchPos = Vector2.zero;

    //classes
    private game_board gameBoardClass;
    private find_matches findMatchesClass;
    private hint_manager hintManagerClass;
    private end_game_manager endGameManagerClass;

    private Vector2 tempPos;
    public GameObject otherDot;

    
    [Header("Board Variables")]
    public bool isMatched = false;
    public int previousColumn, previousRow;
    public int column, row;
    public int targetX, targetY;

    [Header("Swipe Stuff")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    //bomb
    [Header("PowerUps")]
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isColorBomb;
    public bool isWrapBomb;

    public GameObject columnBomb;
    public GameObject rowBomb;
    public GameObject colorBomb;
    public GameObject wrapBomb;

    // Start is called before the first frame update
    void Start()
    {
        //classes
        gameBoardClass = GameObject.FindWithTag("GameBoard").GetComponent<game_board>();
        findMatchesClass = FindObjectOfType<find_matches>();
        hintManagerClass = FindObjectOfType<hint_manager>();
        endGameManagerClass = FindObjectOfType<end_game_manager>();

        //false bomb
        isColumnBomb  = false;
        isRowBomb = false;
        isColorBomb = false;
        isWrapBomb = false;


/*        if (usedSprite != null)
        {
            Camera cam = Camera.main;

            Vector3 min = usedSprite.bounds.min;
            Vector3 max = usedSprite.bounds.max;

            Vector3 screenMin = cam.WorldToScreenPoint(min);
            Vector3 screenMax = cam.WorldToScreenPoint(max);

            Debug.Log(screenMin + "/" + screenMax);

            float screenWidth = screenMax.x - screenMin.x;
            float screenHeight = screenMax.y - screenMin.y;

            Debug.Log(screenWidth + "/" + screenHeight);
        }*/
    }


    //debug only!
    private void OnMouseOver()
    {
        if (Input.GetMouseButton(1))
        {
            isWrapBomb = true;
            GameObject marker = Instantiate(wrapBomb, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }


    // Update is called once per frame
    void Update()
    {
        targetX = column;
        targetY = row;

        //Pos X
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            //Move Toward the target
            tempPos = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPos, .5f);
                
            if (gameBoardClass.allDots[column, row] != this.gameObject)
            {
                gameBoardClass.allDots[column, row] = this.gameObject;
                findMatchesClass.FindAllMatches();
            }            
        } else {
            //directly set pos
            tempPos = new Vector2(targetX, transform.position.y);
            transform.position = tempPos;
        }

        //Pos Y
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            //Move Toward the target
            tempPos = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPos, .5f);

            if (gameBoardClass.allDots[column, row] != this.gameObject)
            {
                gameBoardClass.allDots[column, row] = this.gameObject;
                findMatchesClass.FindAllMatches();
            }            
        } else {
            //directly set pos
            tempPos = new Vector2(transform.position.x, targetY);
            transform.position = tempPos;
        }
    }

    public IEnumerator CheckMoveCo()
    {
        //for color bobmb
        if(isColorBomb)
        {
            findMatchesClass.MatchColorPieces(otherDot.tag);
            isMatched = true;
        } else if(otherDot.GetComponent<dot>().isColorBomb)
        {
            findMatchesClass.MatchColorPieces(this.gameObject.tag);
            otherDot.GetComponent<dot>().isMatched = true;
        }
        
        yield return new WaitForSeconds(.3f);

        if (otherDot != null)
        {
            if(!isMatched && !otherDot.GetComponent<dot>().isMatched)
            {
                otherDot.GetComponent<dot>().row = row;
                otherDot.GetComponent<dot>().column = column;

                row = previousRow;
                column = previousColumn;

                yield return new WaitForSeconds(.1f);
                gameBoardClass.currentDot = null;
                gameBoardClass.currentState = GameState.move;
            }
            else
            {
                //score
                if(endGameManagerClass != null)
                {
                    if(endGameManagerClass.egRequrimentsClass.gameType == GameType.Moves)
                    {
                        endGameManagerClass.DecreaseCounterVal();   
                    }
                }

                gameBoardClass.DestroyMatches();
            }

            //otherDot = null;
        }
    }

    private void OnMouseDown()
    {
        //destroy hint
        if(hintManagerClass != null)
        {
            hintManagerClass.DestroyHint();
        }

        //set position
        if (gameBoardClass.currentState == GameState.move)
        {
            firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        
    }

    private void OnMouseUp()
    {
        if (gameBoardClass.currentState == GameState.move)
        {
            finalTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalcAngle();
        }
    }

    private void CalcAngle()
    {
        //work with swipe only
        if (Mathf.Abs(finalTouchPos.y - firstTouchPos.y) > swipeResist || Mathf.Abs(finalTouchPos.x - firstTouchPos.x) > swipeResist)
        {
            //state
            gameBoardClass.currentState = GameState.wait;

            swipeAngle = Mathf.Atan2(finalTouchPos.y - firstTouchPos.y, finalTouchPos.x - firstTouchPos.x) * 180 / Mathf.PI;

            MoveElement();

            gameBoardClass.currentDot = this;
        }
        else
        {
            //state
            gameBoardClass.currentState = GameState.move;
        }
    }


    void MovePieceMechanics(Vector2 direction)
    {
        otherDot = gameBoardClass.allDots[column + (int)direction.x, row + (int)direction.y];

        previousRow = row;
        previousColumn = column;

        //lock tiles 
        if (gameBoardClass.lockCells[column, row] == null && gameBoardClass.lockCells[column + (int)direction.x, row + (int)direction.y] == null)
        {
            if (otherDot != null)
            {
                otherDot.GetComponent<dot>().column += -1 * (int)direction.x;
                otherDot.GetComponent<dot>().row += -1 * (int)direction.y;

                column += (int)direction.x;
                row += (int)direction.y;

                StartCoroutine(CheckMoveCo());
            }
            else
            {
                gameBoardClass.currentState = GameState.move;
            }
        }
        else
        {
            gameBoardClass.currentState = GameState.move;
        }
    }

    void MoveElement()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < gameBoardClass.width-1)
        {
            //right swipe
            MovePieceMechanics(Vector2.right);

        } else if (swipeAngle > 45 && swipeAngle <= 135 && row < gameBoardClass.height-1)
        {
            //up swipe
            MovePieceMechanics(Vector2.up);

        } else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            //left swipe
            MovePieceMechanics(Vector2.left);
        } else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            //down swipe
            MovePieceMechanics(Vector2.down);
        }
        else
        {
            gameBoardClass.currentState = GameState.move;
        }
    }

    public void CookColumnBomb()
    {
        if (!isRowBomb && !isColorBomb && !isWrapBomb)
        {
            isColumnBomb = true;
            GameObject bombC = Instantiate(columnBomb, transform.position, Quaternion.identity);
            bombC.transform.parent = this.transform;
        }
    }

    public void CookRowBomb()
    {
        if(!isColumnBomb && !isColorBomb && !isWrapBomb)
        {
            isRowBomb = true;
            GameObject bombR = Instantiate(rowBomb, transform.position, Quaternion.identity);
            bombR.transform.parent = this.transform;
        }
    }

    public void CookColorBomb()
    {
        if (!isColumnBomb && !isRowBomb && !isWrapBomb)
        {
            isColorBomb = true;
            GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
            color.transform.parent = this.transform;
            this.gameObject.tag = "ColorBomb";
        }

    }

    public void CookWrapBomb()
    {
        if (!isColumnBomb && !isRowBomb && !isColorBomb)
        {
            isWrapBomb = true;
            GameObject wrap = Instantiate(wrapBomb, transform.position, Quaternion.identity);
            wrap.transform.parent = this.transform;
        }
    }

}
