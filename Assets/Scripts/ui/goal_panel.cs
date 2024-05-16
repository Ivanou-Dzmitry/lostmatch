
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class goal_panel : MonoBehaviour
{

    public Image thisImage;
    public Sprite thisSprite;
    public TMP_Text thisText;
    public string thisString;


    void SetupGoals()
    {
        thisImage.sprite = thisSprite;
        thisText.text = thisString;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupGoals();
    }

}
