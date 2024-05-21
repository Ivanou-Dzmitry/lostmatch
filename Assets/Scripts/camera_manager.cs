using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_manager : MonoBehaviour
{
    //classes
    private game_board gameBoardClass;

    [Header("Camera Tuning Stuff")]
    public float cameraOffset;
    public float aspectRatio = 1.78f;
    public float padding = 1;
    public float yOffset = 1;

    public GameObject backImage;

    // Start is called before the first frame update
    void Start()
    {
        gameBoardClass = GameObject.FindWithTag("GameBoard").GetComponent<game_board>();

        if (gameBoardClass != null)
        {
            CameraPos(gameBoardClass.width - 1, gameBoardClass.height - 1);
        }
    }

    void CameraPos(float x, float y)
    {
        Vector3 temPos = new Vector3(x/2, y/2 + yOffset, cameraOffset);        
        
        transform.position = temPos;

        Camera.main.orthographicSize = 8.2f;

        //background
        backImage.transform.position = new Vector3(temPos.x, temPos.y, 0);

        //Debug.Log(temPos);
    }

}
