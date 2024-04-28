using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hint_manager : MonoBehaviour
{

    //classes
    private game_board gameBoardClass;

    public float hintDelay;
    private float hintDelaySec;
    public GameObject hintParticle;
    public GameObject currentHint;

    // Start is called before the first frame update
    void Start()
    {
        gameBoardClass = GameObject.FindWithTag("GameBoard").GetComponent<game_board>();

        hintDelaySec = hintDelay;
    }

    // Update is called once per frame
    void Update()
    {
        hintDelaySec -= Time.deltaTime;
        if (hintDelaySec <= 0 && currentHint == null)
        {
            MarkHint();
            hintDelaySec = hintDelay;
        }
    }

    //all posible matches
    List <GameObject> FindAllMatches()
    {

        List<GameObject> possibleMatches = new List<GameObject>();  

        for (int i = 0; i < gameBoardClass.width; i++)
        {
            for (int j = 0; j < gameBoardClass.height; j++)
            {
                if (gameBoardClass.allDots[i, j] != null)
                {
                    if (i < gameBoardClass.width - 1)
                    {
                        if (gameBoardClass.SwithAndCheck(i, j, Vector2.right))
                        {
                            possibleMatches.Add(gameBoardClass.allDots[i,j]);
                        }
                            
                    }

                    if (j < gameBoardClass.height - 1)
                    {
                        if (gameBoardClass.SwithAndCheck(i, j, Vector2.up))
                        {
                            possibleMatches.Add(gameBoardClass.allDots[i, j]);
                        }
                    }
                }
            }
        }

        return possibleMatches;

    }


    //pick match
    GameObject PickRandomMatch()
    {
        List <GameObject> possibleMoves = new List<GameObject> ();

        possibleMoves = FindAllMatches();

        if(possibleMoves.Count > 0)
        {
            int pieceToUse = Random.Range(0, possibleMoves.Count);
            return possibleMoves[pieceToUse];
        }

        return null;
    }

    //create hint
    private void MarkHint()
    {
        GameObject move = PickRandomMatch();

        if (move != null)
        {
            currentHint = Instantiate(hintParticle, move.transform.position, Quaternion.identity);
        }
    }

    public void DestroyHint()
    {
        if(currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
            hintDelaySec = hintDelay;
        }
    }

}
