using UnityEngine;
using UnityEngine.UI;

public class ui_manager : MonoBehaviour
{

    [SerializeField] private GameObject characterControls;
    [SerializeField] private GameObject introPanel;
    [SerializeField] public GameObject winPanel;
    [SerializeField] public GameObject tryPanel;

    // Start is called before the first frame update
    void Start()
    {
        SafeArea();

        //panels visibility
        introPanel.SetActive(true);
        winPanel.SetActive(false);
        tryPanel.SetActive(false);
    }


    void SafeArea()
    {
        //get safe area values
        float SaveX = Screen.safeArea.x;
        float SaveY = Screen.safeArea.y;

        //Debug.Log(SaveX + "/" + SaveY);
        //set controls
        RectTransform NewPlayerControls = characterControls.GetComponent<RectTransform>();

        NewPlayerControls.anchoredPosition = new Vector3(0, 0 - SaveY, 0);

        //NewPlayerControls.offsetMin = new Vector2(0, SaveY);
        //NewPlayerControls.offsetMax = new Vector2(0, 0);
    }



}
