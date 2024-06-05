using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "World", menuName = "Level")]
public class level : ScriptableObject
{
    [Header("Size")]
    public int width;
    public int height;

    [Header("Layout")]
    public TileType[] boardLayout;

    [Header("Preload Layout")]
    public TileType[] preloadBoardLayout;

    [Header("Object Types")]
    public GameObject[] dots;

    [Header("Level Goals")]
    public int[] scoreGoals;


    [Header("End Game Rules")]
    public EndGameRequriments egRequrimentsLVL; //end game manager
    public BlankGoalClass[] levelGoals; // goal manager
}
