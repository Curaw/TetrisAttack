using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRow : MonoBehaviour
{
    private const float ANIMATION_DELTA = 0.3f;
    private const float ANIMATION_OFFSET_X = 0.125f;
    private const int ANIMATION_STEPS = 8;

    private Field containingField;
    private GameObject[] data;
    private int posY = 0;
    private bool isSwapInProgress = false;
    private Block leftSwappingBlock, rightSwappingBlock;
    private int animationCounter = 0;
    private float lastUpdate = 0;

    public void init(Field field, int posY, int width)
    {
        this.containingField = field;
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
        //data[index].transform.position = data[index + 1].transform.position;
        //Objekt swappen
        data[index] = data[index + 1];

        //Grafikposition swappen
        //data[index + 1].transform.position = tempPos;
        //Objekt swappen
        data[index + 1] = temp;

        //interne Werte des Blocks swappen
        data[index].GetComponent<Block>().setX(index);
        data[index + 1].GetComponent<Block>().setX(index + 1);

        Debug.Log("neuer linker block: " + data[index].GetComponent<Block>().getX() + ", " + data[index].GetComponent<Block>().getY());
        Debug.Log("neuer rechter block: " + data[index + 1].GetComponent<Block>().getX() + ", " + data[index + 1].GetComponent<Block>().getY());
        this.containingField.handleBlockSolvingAfterSwap(index, this.posY);
    }

    public void initSwap(int index)
    {
        this.rightSwappingBlock = data[index].GetComponent<Block>();
        this.leftSwappingBlock = data[index + 1].GetComponent<Block>();
        isSwapInProgress = true;
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

    // Update is called once per frame
    void Update()
    {
        if (isSwapInProgress)
        {
            lastUpdate += Time.fixedDeltaTime;
            if (lastUpdate >= ANIMATION_DELTA)
            {
                lastUpdate = 0;
                Vector3 newBlockDrawPos = new Vector3(rightSwappingBlock.transform.position.x + ANIMATION_OFFSET_X, rightSwappingBlock.transform.position.y, rightSwappingBlock.transform.position.z);
                rightSwappingBlock.transform.position = newBlockDrawPos;
                newBlockDrawPos = new Vector3(leftSwappingBlock.transform.position.x + ANIMATION_OFFSET_X * -1, leftSwappingBlock.transform.position.y, leftSwappingBlock.transform.position.z);
                leftSwappingBlock.transform.position = newBlockDrawPos;
                animationCounter++;
                if (animationCounter >= ANIMATION_STEPS)
                {
                    isSwapInProgress = false;
                    animationCounter = 0;
                    swap(this.rightSwappingBlock.getX());
                }
            }
        }
    }
}
