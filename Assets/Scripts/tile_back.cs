using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tile_back : MonoBehaviour
{
    public int hitPoints;

    public GameObject destroyParticle;
    //private SpriteRenderer sprite;
    private goal_manager goalManagerClass;

    //for mylti hits objects
    [Header("Layers")]
    public GameObject objLayer02;
    public GameObject objLayer03;

    [Header("Sound")]
    public AudioClip dotSound;


    private void Start()
    {
        //sprite = GetComponent<SpriteRenderer>();
        goalManagerClass = FindObjectOfType<goal_manager>();
    }

    public void Update()
    {
        if (hitPoints <= 0)
        {
            //for goals for breakable
            if(goalManagerClass != null)
            {
                string tagForCompare = this.gameObject.tag;

                //hack for various breakable
                if (this.gameObject.tag == "breakable_02" || this.gameObject.tag == "breakable_03" && this.gameObject.tag!= null)
                {
                    tagForCompare = "breakable_01";
                }

                //hack for various blockers
                if (this.gameObject.tag == "blocker_02" || this.gameObject.tag == "blocker_03" && this.gameObject.tag != null)
                {
                    tagForCompare = "blocker_01";
                }

                goalManagerClass.CompareGoal(tagForCompare);
                
                Debug.Log("tag " + tagForCompare);

                goalManagerClass.UpdateGoals();
            }

            Destroy(this.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        hitPoints -= damage;

        SpriteOpacity();
    }

    void SpriteOpacity()
    {
        //hide if 1 hitpoint
        if (objLayer02 != null && hitPoints == 1)
        {
            objLayer02.gameObject.SetActive(false);
        }

        //hide if 2 hitpoint
        if (objLayer02 != null && hitPoints == 2)
        {
            objLayer02.gameObject.SetActive(false);
        }

        //Color curColor = sprite.color;

        //float newAlpha = curColor.a * .5f;

        //sprite.color = new Color(curColor.r, curColor.g, curColor.b, newAlpha);
    }

}
