using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRow : MonoBehaviour
{
    //Das hier als Array von Gameobjects machen? Ist das cleverer?
    private GameObject[] data;
    private int posY = 0;

    public void init(int posY, int width)
    {
        this.posY = posY;
        this.data = new GameObject[width];
    }

    public GameObject get(int index)
    {
        return data[index];
    }

    public void set(int index, GameObject newBlock)
    {
        data[index] = newBlock;
    }
    
    public int getSize()
    {
        return data.Length;
    }

    public void setPosY(int newY)
    {
        this.posY = newY;
        updateBlockPositionValues();
    }

    public int getPosY()
    {
        return this.posY;
    }

    public void swap(int index)
    {
        GameObject temp = data[index];
        Vector3 tempPos = temp.transform.position;

        //Grafikposition swappen
        data[index].transform.position = data[index + 1].transform.position;
        //Objekt swappen
        data[index] = data[index + 1];

        //Grafikposition swappen
        data[index + 1].transform.position = tempPos;
        //Objekt swappen
        data[index + 1] = temp;

        //interne Werte des Blocks swappen
        data[index].GetComponent<Block>().setX(index);
        data[index + 1].GetComponent<Block>().setX(index + 1);

        Debug.Log("neuer linker block: " + data[index].GetComponent<Block>().getX() + ", " + data[index].GetComponent<Block>().getY());
        Debug.Log("neuer rechter block: " + data[index + 1].GetComponent<Block>().getX() + ", " + data[index + 1].GetComponent<Block>().getY());
    }

    public void updateBlockPositionValues()
    {
        for (int i = 0; i < data.Length; i++)
        {
            Block block = data[i].GetComponent<Block>();
            block.setPosition(i, this.posY);
        }
    }

    public void activate()
    {
        for (int i = 0; i < data.Length; i++)
        {
            Block block = data[i].GetComponent<Block>();
            block.enable();
        }
    }
}
