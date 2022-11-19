using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private BlockColor color;

    private Renderer renderer;
    private bool disabled;
    private bool falling = false;
    private bool swapping = false;
    private bool levitating  = false;
    private int length;
    private int height;
    [SerializeField] private int posX;
    [SerializeField] private int posY;
    private float fallDownTimer = 0;

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
    public bool isFalling()
    {
        return falling;
    }

    public bool isSwapping()
    {
        return swapping;
    }

    public bool isLevitating()
    {
        return levitating;
    }

    public void disable()
    {
        this.disabled = true;
    }

    public void setFalling(bool newVal)
    {
        this.falling = newVal;
    }

    public void setSwapping(bool newVal)
    {
        this.swapping = newVal;
    }

    public void setLevitating(bool newVal)
    {
        this.levitating = newVal;
    }

    public void enable()
    {
        this.disabled = false;
    }

    public int getX()
    {
        return this.posX;
    }

    public int getY()
    {
        return this.posY;
    }
    public float getFallDownTimer()
    {
        return this.fallDownTimer;
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

    public void setFallDownTimer(float newVal)
    {
        this.fallDownTimer = newVal;
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

    public void greyOut()   //TODO: Spaeter wieder auf private
    {
        this.renderer.material.color = new Color(1, 1, 1, 0.3f);
    }

    public void removeGreyOut()
    {
        this.renderer.material.color = new Color(1, 1, 1, 1);
    }

    // Start is called before the first frame update
    void Start()
    {
        this.renderer = GetComponent<Renderer>();
        this.greyOut();
        this.disable();
    }
}
