using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private float posX = 0;
    private float posY = 1f;

    public void setX(float newX)
    {
        if (newX < 0)
        {
            return;
        }
        this.posX = newX;
        updateGFX();
    }

    public void setY(float newY)
    {
        if(newY < 0)
        {
            return; 
        }
        this.posY = newY;
        updateGFX();
    }

    public float getX()
    {
        return posX;
    }

    public float getY()
    {
        return posY;
    }

    private void updateGFX()
    {
        //TODO die 0.5 muessen variablen sein. gefaellt mir nicht genau wie die 2 im Field
        this.transform.position = new Vector3(posX + 0.5f, posY, transform.position.z); 
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
}
