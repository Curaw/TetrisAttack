using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRow : MonoBehaviour
{
    private const float ANIMATION_DELTA = 0.02f;
    private const float ANIMATION_OFFSET_X = 0.25f;
    private const int ANIMATION_STEPS = 4;

    private Field containingField;
    private GameObject[] data;
    [SerializeField] private int posY = 0;
    private int width = 0;
    private bool isSwapInProgress = false;
    private Block leftSwappingBlock, rightSwappingBlock;
    private int animationCounter = 0;
    private float lastUpdate = 0;

    public void init(Field field, int posY, int width)
    {
        this.containingField = field;
        this.posY = posY;
        this.width = width;
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

    public int getWidth()
    {
        return this.width;
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

        //Enable blocks again
        data[index].GetComponent<Block>().enable();
        data[index + 1].GetComponent<Block>().enable();

        //Debug.Log("neuer linker block: " + data[index].GetComponent<Block>().getX() + ", " + data[index].GetComponent<Block>().getY());
        //Debug.Log("neuer rechter block: " + data[index + 1].GetComponent<Block>().getX() + ", " + data[index + 1].GetComponent<Block>().getY());

        //TODO Das hier kann alles das Field machen
        this.containingField.handleLogicAfterSwap(index, this.posY);
    }

    public void initSwap(int index)
    {
        if(isSwapInProgress)
        {
            return;
        }

        this.rightSwappingBlock = data[index].GetComponent<Block>();
        this.leftSwappingBlock = data[index + 1].GetComponent<Block>();

        if(this.rightSwappingBlock.isDisabled() || this.leftSwappingBlock.isDisabled())
        {
            return;
        }
        isSwapInProgress = true;
        this.rightSwappingBlock.setSwapping(true);
        this.rightSwappingBlock.setFalling(false);
        this.leftSwappingBlock.setSwapping(true);
        this.leftSwappingBlock.setFalling(false);
        this.rightSwappingBlock.disable();
        this.leftSwappingBlock.disable();
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
            block.removeGreyOut();
            block.enable();
        }
    }

    private void handleSwapping()
    {
        lastUpdate += Time.deltaTime;
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
                animationCounter = 0;
                leftSwappingBlock.setSwapping(false);
                rightSwappingBlock.setSwapping(false);
                isSwapInProgress = false;
                swap(this.rightSwappingBlock.getX());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isSwapInProgress)
        {
            handleSwapping();
        }
    }
}
