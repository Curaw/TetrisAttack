using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField] private int height;
    [SerializeField] private int width;
    [SerializeField] private GameObject cursor;
    [SerializeField] private GameObject blockRowPrefab;
    [SerializeField] private GameObject emptyBlockPrefab;
    [SerializeField] private GameObject redBlockPrefab;
    [SerializeField] private GameObject blueBlockPrefab;
    [SerializeField] private GameObject greenBlockPrefab;
    [SerializeField] private GameObject yellowBlockPrefab;
    [SerializeField] private GameObject purpleBlockPrefab;
    private Headarray<GameObject> blockRows;
    private Cursor cursorScript;
    private BlockRow lastCreatedRow;
    private List<GameObject> solveCandidates;
    private List<GameObject> solvedBlocks;
    private int fieldComboCounter = 1;

    private const float ONE_PIXEL_UNIT = 0.0625f;

    // Start is called before the first frame update
    void Start()
    {
        this.cursorScript = cursor.GetComponent<Cursor>();
        this.blockRows = new Headarray<GameObject>(height);
        this.solveCandidates = new List<GameObject>();
        this.solvedBlocks = new List<GameObject>();
    }

    public void activateLastRow()
    {
        if(blockRows.get(0) == null)
        {
            return;
        }
        blockRows.get(0).GetComponent<BlockRow>().activate();
    }

    public void shiftEverythingUp()
    {
        shiftControllerUp();
        shiftBlocksUp();
    }

    private void shiftControllerUp()
    {
        if (isCursorAtTop())
        {
            return;
        }
        cursorScript.setY(cursorScript.getY() + ONE_PIXEL_UNIT);
        //Vector3 cursorPos = cursor.transform.position;
        //cursor.transform.position = new Vector3(cursorPos.x, cursorPos.y + ONE_PIXEL_UNIT, cursorPos.z);
    }

    private void shiftBlocksUp()
    {
        GameObject blockRowGO;
        Transform blockTransform;
        for (int i = 0; i < blockRows.getSize(); i++)
        {
            if (blockRows.get(i) != null)
            {
                blockRowGO = blockRows.get(i);
                blockTransform = blockRowGO.transform;
                blockTransform.position = new Vector3(blockTransform.position.x, blockTransform.position.y + ONE_PIXEL_UNIT, blockTransform.position.z);
            }
        }
    }

    public void addRandomBlockRow()
    {
        removeTopBlockRow();
        GameObject newBlockRow = createNewBlockRowGO();
        fillBlockRowWithRandomBlocks(newBlockRow);
        blockRows.addToBot(newBlockRow);
        updateBlockPositions();
        shiftControllerUp();
    }

    private GameObject createNewBlockRowGO()
    {
        GameObject newRowGO = GameObject.Instantiate(blockRowPrefab, Vector3.zero, transform.rotation, transform);
        newRowGO.AddComponent<BlockRow>();
        newRowGO.GetComponent<BlockRow>().init(this, 0, width);
        return newRowGO;
    }

    //Spaeter nur vom Host benutzt
    private void fillBlockRowWithRandomBlocks(GameObject row)
    {
        BlockRow br = row.GetComponent<BlockRow>();
        for (int i = 0; i < this.width; i++)
        {
            GameObject newBlock;
            if (i == 0)
            {
                //Der erste Block darf nur nicht die gleiche farbe haben, wie der ueber sich
                BlockColor rowColorAbove = getColorFromLastRowAtIndex(i);
                newBlock = createRandomBlockWithException(i, rowColorAbove);
            } else
            {
                //Alle folgenden Bloecke duerfen zusaetzlich nicht die Farbe von ihrem linken Nachbarn haben
                BlockColor lastBlockColor = br.get(i - 1).GetComponent<Block>().getBlockColor();
                BlockColor rowColorAbove = getColorFromLastRowAtIndex(i);
                newBlock = createRandomBlockWithTwoExceptions(i, lastBlockColor, rowColorAbove);
                Block newBlockScript = newBlock.GetComponent<Block>();
                newBlockScript.setPosition(i, 0);
            }
            newBlock.transform.SetParent(row.transform, false);
            br.set(i,newBlock);
        }
        this.lastCreatedRow = br;
    }

    private BlockColor getColorFromLastRowAtIndex(int index)
    {
        if(lastCreatedRow != null)
        {
            //Debug.Log("Ueber mir: " + lastCreatedRow.get(index).GetComponent<Block>().GetBlockColor());
            return lastCreatedRow.get(index).GetComponent<Block>().getBlockColor();
        }
        return BlockColor.Empty;
    }

    //Spaeter nur vom Host benutzt, unused
    private GameObject createRandomBlock(int posX)
    {
        BlockColor randomType = (BlockColor)UnityEngine.Random.Range(0, 5);
        return createBlock(randomType, posX);
    }

    //Spaeter nur vom Host benutzt
    private GameObject createRandomBlockWithException(int posX, BlockColor exceptionColor)
    {
        //Die Funktion stellt in O(1) sicher, dass nicht die Farbe exceptionColor generiert wird
        BlockColor randomType = (BlockColor)UnityEngine.Random.Range(0, 4);
        if(randomType >= exceptionColor)
        {
            randomType = randomType + 1;
        }
        return createBlock(randomType, posX);
    }

    //Spaeter nur vom Host benutzt
    //Die Funktion stellt in O(1) sicher, dass nicht die Farbe exceptionColor1 und exceptionColor2 generiert wird
    private GameObject createRandomBlockWithTwoExceptions(int posX, BlockColor exceptionColor1, BlockColor exceptionColor2)
    {
        //Sind die Farben gleich, dann gehen wir den einfachen Weg
        if(exceptionColor1 == exceptionColor2)
        {
            return createRandomBlockWithException(posX, exceptionColor1);
        }

        //Erst Farben sortieren
        if (exceptionColor1 > exceptionColor2)
        {
            BlockColor temp = exceptionColor1;
            exceptionColor1 = exceptionColor2;
            exceptionColor2 = temp;
        }

        //Zufällige Farbe ziehen
        BlockColor randomType = (BlockColor)UnityEngine.Random.Range(0, 3);
        //Debug.Log("random: " + randomType + ", exception1: " + exceptionColor1 + ", exception2: " + exceptionColor2);

        //Farbe anpassen wenn eine Exception getroffen wurde
        if (randomType >= exceptionColor1)
        {
            randomType = randomType + 1;
        }
        if (randomType >= exceptionColor2)
        {
            randomType = randomType + 1;
        }
        return createBlock(randomType, posX);
    }

    //Der hier dann vom Client?
    private GameObject createBlock(BlockColor color, int posX)
    {
        switch (color)
        {
            case BlockColor.Red:
                return GameObject.Instantiate(redBlockPrefab, new Vector3(posX, transform.position.y, 0), transform.rotation);
            case BlockColor.Blue:
                return GameObject.Instantiate(blueBlockPrefab, new Vector3(posX, transform.position.y, 0), transform.rotation);
            case BlockColor.Green:
                return GameObject.Instantiate(greenBlockPrefab, new Vector3(posX, transform.position.y, 0), transform.rotation);
            case BlockColor.Yellow:
                return GameObject.Instantiate(yellowBlockPrefab, new Vector3(posX, transform.position.y, 0), transform.rotation);
            case BlockColor.Purple:
                return GameObject.Instantiate(purpleBlockPrefab, new Vector3(posX, transform.position.y, 0), transform.rotation);
            default:
                return GameObject.Instantiate(emptyBlockPrefab, new Vector3(posX, transform.position.y, 0), transform.rotation);
        }
    }

    private void removeTopBlockRow()
    {
        GameObject.Destroy(blockRows.get(blockRows.getSize() - 1));
    }

    private void updateBlockPositions()
    {
        GameObject blockRowGo;
        BlockRow blockRow;
        for(int i = 0; i < blockRows.getSize(); i++)
        {
            if(blockRows.get(i) != null)
            {
                blockRowGo = blockRows.get(i);
                blockRowGo.transform.position = new Vector3(0, i, 0);

                blockRow = blockRowGo.GetComponent<BlockRow>();
                blockRow.setPosY(blockRow.getPosY() + 1);
            }
        }
    }

    private bool isCursorAtTop()
    {
        //TODO Die 2 gefaellt mir hier gar nicht. vielleicht irgendwie einheitlicher machen mit den Blockgrafiken
        if (cursorScript.getY() >= height - 2)
        {
            return true;
        }
        return false;
    }

    public int getWidth()
    {
        return this.width;
    }

    private void moveCursorUp()
    {
        int roundedY = (int)Math.Floor(cursorScript.getY() + 1);
        if(roundedY >= this.height)
        {
            return;
        }
        cursorScript.setY(cursorScript.getY() + 1);
    }
    private void moveCursorDown()
    {
        int roundedY = (int)Math.Floor(cursorScript.getY() - 1);
        if (roundedY <= 0)
        {
            return;
        }
        cursorScript.setY(cursorScript.getY() - 1);
    }

    private void moveCursorLeft()
    {
        int roundedX = (int)Math.Floor(cursorScript.getX() - 1);
        if(roundedX < 0)
        {
            return;
        }
        cursorScript.setX(roundedX);
    }
    private void moveCursorRight()
    {
        int roundedX = (int)Math.Floor(cursorScript.getX() + 1);
        if (roundedX >= this.width - 1)
        {
            return;
        }
        cursorScript.setX(roundedX);
    }

    private void swapBlocksAtCursorPosition()
    {
        int roundedY = (int)Math.Floor(cursorScript.getY());
        int roundedX = (int)Math.Floor(cursorScript.getX());

        if(blockRows.get(roundedY) == null)
        {
            return;
        }

        blockRows.get(roundedY).GetComponent<BlockRow>().initSwap(roundedX);
        //blockRows.get(roundedY).GetComponent<BlockRow>().swap(roundedX);
    }

    public void handleBlockSolvingAfterSwap(int posX, int posY)
    {
        solvedBlocks.Clear();
        checkForSolvedBlocks(posX, posY);
        checkForSolvedBlocks(posX + 1, posY);
        solvedBlocks.Sort(compareBlockPositions); //TODO: Hier die kacksortierung
        foreach (GameObject item in solvedBlocks)
        {
            Block block = item.GetComponent<Block>();
            Debug.Log(block.getX() + ", " + block.getY());
        }
        disableSolvedBlocks();
    }
    private void handleBlockSolvingforRow(int posX, int posY)
    {
        solvedBlocks.Clear();
        //TODO hier schleife ueber die reihe
        checkForSolvedBlocks(posX, posY);
        disableSolvedBlocks();
    }

    private int compareBlockPositions(GameObject go1, GameObject go2)
    {
        Block b1 = go1.GetComponent<Block>();
        Block b2 = go2.GetComponent<Block>();

        if(b1.getY() > b2.getY())
        {
            return -1;
        }

        if (b1.getY() < b2.getY())
        {
            return 1;
        }

        if(b1.getX() < b2.getX())
        {
            return -1;
        }

        return 1;
    }

    private void disableSolvedBlocks()
    {
        foreach (GameObject block in solvedBlocks)
        {
            block.GetComponent<Block>().setBlockColor(BlockColor.Empty);
            //Debug.Log("Block gone: " + block.gameObject.transform.position.x + ", " + block.gameObject.transform.position.y);
            //block.GetComponent<Block>().disable();
        }
    }

    private void checkForSolvedBlocks(int blockX, int blockY)
    {
        int currentComboCounter = 1;
        solveCandidates.Clear();
        Debug.Log("Y!!! " + blockY);
        BlockRow blockRow = blockRows.get(blockY).GetComponent<BlockRow>();
        Block block = blockRow.get(blockX).GetComponent<Block>();
        solveCandidates.Add(block.gameObject);

        //Block horizontal checken
        currentComboCounter += countLeftNeighbors(blockX, blockY, block.getBlockColor());
        currentComboCounter += countRightNeighbors(blockX, blockY, block.getBlockColor());

        if (currentComboCounter >= 3)
        {
            foreach (GameObject candidate in solveCandidates)
            {
                solvedBlocks.Add(candidate);
            }
        }
        solveCandidates.Clear();
        solveCandidates.Add(block.gameObject);
        currentComboCounter = 1;

        //block vertikal checken
        currentComboCounter += countTopNeighbors(blockX, blockY, block.getBlockColor());
        currentComboCounter += countBotNeighbors(blockX, blockY, block.getBlockColor());

        if (currentComboCounter >= 3)
        {
            foreach (GameObject candidate in solveCandidates)
            {
                solvedBlocks.Add(candidate);
            }
        }
    }
    private int countTopNeighbors(int posX, int posY, BlockColor colorToLookFor)
    {
        if (posY == this.height - 1)
        {
            return 0;
        }
        GameObject topRow = blockRows.get(posY + 1);  //TODO Auch checken, ob der Block disabled ist
        if (topRow == null)
        {
            return 0;
        }

        Block topNeighbor = topRow.GetComponent<BlockRow>().get(posX).GetComponent<Block>();

        if (topNeighbor.getBlockColor() == colorToLookFor)
        {
            solveCandidates.Add(topNeighbor.gameObject);
            return 1 + countTopNeighbors(posX, posY + 1, colorToLookFor);
        }
        else
        {
            return 0;
        }
    }

    private int countBotNeighbors(int posX, int posY, BlockColor colorToLookFor)
    {
        if (posY == 0)
        {
            return 0;
        }
        GameObject botRow = blockRows.get(posY - 1);
        if(botRow == null) //TODO Auch checken, ob der Block disabled ist
        {
            return 0;
        }
        Block botNeighbor = botRow.GetComponent<BlockRow>().get(posX).GetComponent<Block>();

        if (botNeighbor.getBlockColor() == colorToLookFor)
        {
            solveCandidates.Add(botNeighbor.gameObject);
            return 1 + countBotNeighbors(posX, posY - 1, colorToLookFor);
        }
        else
        {
            return 0;
        }
    }

    private int countLeftNeighbors(int posX, int posY, BlockColor colorToLookFor)
    {
        if(posX == 0)
        {
            return 0;
        }
        Block leftNeighbor = blockRows.get(posY).GetComponent<BlockRow>().get(posX - 1).GetComponent<Block>(); //TODO Auch checken, ob der Block disabled ist

        if (leftNeighbor.getBlockColor() == colorToLookFor)
        {
            solveCandidates.Add(leftNeighbor.gameObject);
            return 1 + countLeftNeighbors(posX - 1, posY, colorToLookFor);
        } else
        {
            return 0;
        }
    }

    private int countRightNeighbors(int posX, int posY, BlockColor colorToLookFor)
    {
        if (posX == this.width - 1)
        {
            return 0;
        }
        Block rightNeighbor = blockRows.get(posY).GetComponent<BlockRow>().get(posX + 1).GetComponent<Block>(); //TODO Auch checken, ob der Block disabled ist

        if (rightNeighbor.getBlockColor() == colorToLookFor)
        {
            solveCandidates.Add(rightNeighbor.gameObject);
            return 1 + countRightNeighbors(posX + 1, posY, colorToLookFor);
        }
        else
        {
            return 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //TODO ranges festlegen. koennte das field hier selbst entscheiden und die pos nur aendern, wenn es noch im feld ist
        if (Input.GetKeyDown("w"))
        {
            moveCursorUp();
        }
        if (Input.GetKeyDown("a"))
        {
            moveCursorLeft();
        }
        if (Input.GetKeyDown("d"))
        {
            moveCursorRight();
        }
        if (Input.GetKeyDown("s"))
        {
            moveCursorDown();
        }
        if (Input.GetKeyDown("k"))
        {
            swapBlocksAtCursorPosition();
        }
    }
}
