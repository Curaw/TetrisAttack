using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private BlockColor color;
    private Renderer renderer;
    private bool disabled;
    private int length;
    private int height;
    private int posX;
    private int posY;

    public Block(BlockColor color)
    {
        this.color = color;
        length = 1;
        height = 1;
    }

    public bool isDisabled()
    {
        return disabled;
    }

    public void disable()
    {
        this.disabled = true;
        greyOut();
    }

    public void enable()
    {
        this.disabled = false;
        removeGreyOut();
    }

    public int getX()
    {
        return this.posX;
    }

    public int getY()
    {
        return this.posY;
    }

    public void setPosition(int x, int y)
    {
        this.posX = x;
        this.posY = y;
    }

    public void setX(int newX)
    {
        this.posX = newX;
    }

    public void setY(int newY)
    {
        this.posY = newY;
    }

    public BlockColor getBlockColor()
    {
        return this.color;
    }

    public void setBlockColor(BlockColor newColor)
    {
        this.color = newColor;
        //TODO hier die Grafik updaten
        this.renderer.material.color = new Color(1, 1, 1, 0f);
    }

    private void greyOut()
    {
        this.renderer.material.color = new Color(1, 1, 1, 0.3f);
    }

    private void removeGreyOut()
    {
        this.renderer.material.color = new Color(1, 1, 1, 1);
    }

    // Start is called before the first frame update
    void Start()
    {
        this.renderer = GetComponent<Renderer>();
        this.disable();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
