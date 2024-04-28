
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlankGoalClass
{
    public int numberGoalsNeeded;
    public int numberCollectedGoals;
    public Sprite goalSprite;
    public string matchValue;
}

public class goal_manager : MonoBehaviour
{

    public BlankGoalClass[] levelGoals;
    public List<goal_panel> currentGoals = new List<goal_panel>();
    public GameObject goalPrefab;
    public GameObject goalIntroParent;
    public GameObject goalGameParent;
    
    //class
    private end_game_manager egManagerClass;
    private game_board gameBoardClass;


    // Start is called before the first frame update
    void Start()
    {
        gameBoardClass = GameObject.FindWithTag("GameBoard").GetComponent<game_board>();
        egManagerClass = FindObjectOfType<end_game_manager>();

        GetGoals();

        SetupIntroGoals();
    }

    void GetGoals()
    {
        if (gameBoardClass != null)
        {
            if (gameBoardClass.level < gameBoardClass.worldClass.levels.Length)
            {
                if (gameBoardClass.worldClass.levels[gameBoardClass.level] != null)
                {
                    levelGoals = gameBoardClass.worldClass.levels[gameBoardClass.level].levelGoals;

                    //reset goals
                    for (int i = 0; i < levelGoals.Length; i++)
                    {
                        levelGoals[i].numberCollectedGoals = 0;
                    }
                }
            }
        }
    }

    void SetupIntroGoals()
    {
        for (int i = 0; i<levelGoals.Length; i++)
        {
            //intro prefabs
            GameObject introGoal = Instantiate(goalPrefab, goalIntroParent.transform.position, Quaternion.identity);
            introGoal.transform.SetParent(goalIntroParent.transform);
            introGoal.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            goal_panel introPanel = introGoal.GetComponent<goal_panel>();
            introPanel.thisSprite = levelGoals[i].goalSprite;
            introPanel.thisString = "0/" + levelGoals[i].numberGoalsNeeded;

            //ingame
            GameObject ingameGoal = Instantiate(goalPrefab, goalGameParent.transform.position, Quaternion.identity);
            ingameGoal.transform.SetParent(goalGameParent.transform);
            ingameGoal.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            goal_panel gamePanel = ingameGoal.GetComponent<goal_panel>();

            currentGoals.Add(gamePanel);

            gamePanel.thisSprite = levelGoals[i].goalSprite;
            gamePanel.thisString = "0/" + levelGoals[i].numberGoalsNeeded;

        }
    }



    public void UpdatesGoals()
    {
        int goalsCompleted = 0;

        for (int i = 0; i < levelGoals.Length; i++)
        {
            currentGoals[i].thisText.text = "" + levelGoals[i].numberCollectedGoals + "/" + levelGoals[i].numberGoalsNeeded;
            
            if (levelGoals[i].numberCollectedGoals >= levelGoals[i].numberGoalsNeeded)
            {
                goalsCompleted++;
                currentGoals[i].thisText.text = "" + levelGoals[i].numberGoalsNeeded + "/" + levelGoals[i].numberGoalsNeeded;
            }

            if (goalsCompleted >= levelGoals.Length)
            {
                if(egManagerClass != null)
                {
                    egManagerClass.WinGame();
                }
                //Debug.Log("WIN!");
            }
        }
    }

    public void CompareGoal(string goalToCompare)
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            if (goalToCompare == levelGoals[i].matchValue)
            {
                levelGoals[i].numberCollectedGoals++;
            }
        }
    }


}
