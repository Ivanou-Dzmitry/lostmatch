using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tile_back : MonoBehaviour
{
    public int hitPoints;
    private SpriteRenderer sprite;
    private goal_manager goalManagerClass;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        goalManagerClass = FindObjectOfType<goal_manager>();
    }

    public void Update()
    {
        if (hitPoints <= 0)
        {
            //for goals
            if(goalManagerClass != null)
            {
                goalManagerClass.CompareGoal(this.gameObject.tag);
                goalManagerClass.UpdatesGoals();
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
        Color curColor = sprite.color;

        float newAlpha = curColor.a * .5f;

        sprite.color = new Color(curColor.r, curColor.g, curColor.b, newAlpha);
    }

}
