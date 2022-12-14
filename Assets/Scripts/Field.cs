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
    [SerializeField] private Block coyoteBlock;

    private List<GameObject> solveCandidates;
    private List<GameObject> solvedBlocks;
    private List<GameObject> fallingBlocks;
    private int fieldComboCounter = 1;

    private const float ONE_PIXEL_UNIT = 0.0625f;
    private const float FALLDOWN_DELTA = 0.02f;
    private const float COYOTE_TIME = 0.18f;

    // Start is called before the first frame update
    void Start()
    {
        this.cursorScript = cursor.GetComponent<Cursor>();
        this.blockRows = new Headarray<GameObject>(height);
        this.solveCandidates = new List<GameObject>();
        this.solvedBlocks = new List<GameObject>();
        this.fallingBlocks = new List<GameObject>();
    }

    public int getCoyoteBlockColumn()
    {
        if(coyoteBlock == null)
        {
            return -1;
        }
        return this.coyoteBlock.getX();
    }

    public void activateLastRow()
    {
        if(blockRows.get(0) == null)
        {
            return;
        }
        blockRows.get(0).GetComponent<BlockRow>().activate();
        handleBlockSolvingforRow(0);
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

    public void addRandomBlockRowToBottom()
    {
        removeTopBlockRow();
        GameObject newBlockRow = createNewBlockRowGO(-1);   //-1 because increaseBlockPositions sets the new row to 0.
        fillBlockRowWithRandomBlocks(newBlockRow);
        blockRows.addToBot(newBlockRow);
        increaseBlockPositions();
        shiftControllerUp();
    }

    private GameObject createNewBlockRowGO(int yPos)
    {
        GameObject newRowGO = GameObject.Instantiate(blockRowPrefab, Vector3.zero, transform.rotation, transform);
        newRowGO.AddComponent<BlockRow>();
        newRowGO.GetComponent<BlockRow>().init(this, yPos, width);
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

        //Zuf?llige Farbe ziehen
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

    private void increaseBlockPositions()
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

    public void handleBlockSolvingAtPosition(int posX, int posY)
    {
        solvedBlocks.Clear();
        checkForSolvedBlocks(posX, posY);
        solvedBlocks.Sort(compareBlockPositions); //TODO: Hier die kacksortierung
        //foreach (GameObject item in solvedBlocks)
        //{
        //    Block block = item.GetComponent<Block>();
        //    //Debug.Log(block.getX() + ", " + block.getY());
        //}
        emptySolvedBlocks();
    }
    private void handleBlockSolvingforRow(int posY)
    {
        solvedBlocks.Clear();
        BlockRow row = blockRows.get(posY).GetComponent<BlockRow>();
        for(int i = 0; i < row.getWidth(); i++)
        {
            checkForSolvedBlocks(i, posY);
        }
        solvedBlocks.Sort(compareBlockPositions);
        emptySolvedBlocks();
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

    public void handleLogicAfterSwap(int x, int y)
    {
        if (isBlockSupposedToFall(x, y))
        {
            this.coyoteBlock = blockRows.get(y).GetComponent<BlockRow>().get(x).GetComponent<Block>();
            this.coyoteBlock.disable();
            this.coyoteBlock.setFallDownTimer(COYOTE_TIME);
            this.coyoteBlock.setLevitating(true);
        }
        else
        {
            handleBlockSolvingAtPosition(x, y);
            noticeFallDown(x, y);
        }

        if (isBlockSupposedToFall(x + 1, y))
        {
            this.coyoteBlock = blockRows.get(y).GetComponent<BlockRow>().get(x + 1).GetComponent<Block>();
            this.coyoteBlock.disable();
            this.coyoteBlock.setFallDownTimer(COYOTE_TIME);
            this.coyoteBlock.setLevitating(true);
        }
        else
        {
            handleBlockSolvingAtPosition(x + 1, y);
            noticeFallDown(x + 1, y);
        }
    }
    private bool isBlockSupposedToFall(int x, int y)
    {
        GameObject rowGo = blockRows.get(y - 1);
        if(rowGo == null)
        {
            return false;
        }
        BlockRow row = rowGo.GetComponent<BlockRow>();
        if(row == null)
        {
            return false;
        }
        return row.get(x).GetComponent<Block>().getBlockColor() == BlockColor.Empty;
    }

    private void emptySolvedBlocks()
    {
        Block block = null;
        int blockYPos = 0;
        int blockXPos = 0;
        foreach (GameObject blockGO in solvedBlocks)
        {
            block = blockGO.GetComponent<Block>();
            blockYPos = block.getY();
            blockXPos = block.getX();

            block.setBlockColor(BlockColor.Empty);
            //Debug.Log("Block gone: " + block.gameObject.transform.position.x + ", " + block.gameObject.transform.position.y);
            //block.disable();
            if(blockRows.get(blockYPos + 1) != null)
            {
                noticeFallDown(blockRows.get(blockYPos + 1).GetComponent<BlockRow>().get(blockXPos).GetComponent<Block>());
            }
        }
    }

    private void noticeFallDown(int x, int y)
    {
        if(y < this.height && x < this.width && blockRows.get(y) != null)
        {
            Block block = blockRows.get(y).GetComponent<BlockRow>().get(x).GetComponent<Block>();
            if(block.getBlockColor() == BlockColor.Empty)
            {
                noticeFallDown(x, y + 1);
            } else
            {
                noticeFallDown(block);
            }
        }
    }

    private void noticeFallDown(Block blockToCheck)
    {
        if(blockToCheck == null)
        {
            return;
        }
        if (!blockToCheck.isDisabled() && blockToCheck.getBlockColor() != BlockColor.Empty)
        {
            blockToCheck.setFallDownTimer(FALLDOWN_DELTA);
            blockToCheck.setFalling(true);
            this.fallingBlocks.Add(blockToCheck.gameObject);
            int yIndex = blockToCheck.getY() + 1;
            if(yIndex >= this.height)
            {
                return;
            }
            if (blockRows.get(yIndex) != null)
            {
                noticeFallDown(blockRows.get(blockToCheck.getY() + 1).GetComponent<BlockRow>().get(blockToCheck.getX()).GetComponent<Block>());
            }
        }
        return;
    }

    private void checkForSolvedBlocks(int blockX, int blockY)
    {
        int currentComboCounter = 1;
        solveCandidates.Clear();
        BlockRow blockRow = blockRows.get(blockY).GetComponent<BlockRow>();
        Block block = blockRow.get(blockX).GetComponent<Block>();
        if(block.getBlockColor() == BlockColor.Empty)
        {
            return;
        }
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
        GameObject topRow = blockRows.get(posY + 1);
        if (topRow == null)
        {
            return 0;
        }

        Block topNeighbor = topRow.GetComponent<BlockRow>().get(posX).GetComponent<Block>();
        if(topNeighbor.isDisabled())
        {
            return 0;
        }

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
        if(botRow == null)
        {
            return 0;
        }
        Block botNeighbor = botRow.GetComponent<BlockRow>().get(posX).GetComponent<Block>();

        if(botNeighbor.isDisabled())
        {
            return 0;
        }

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

        if(leftNeighbor.isDisabled())
        {
            return 0;
        }

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

        if(rightNeighbor.isDisabled())
        {
            return 0;
        }

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

    private void updateFallingBlocks()
    {
        Block currentBlock = null;
        Block lowerBlock = null;
        List<GameObject> finishedBlocks = new List<GameObject>();
        foreach (GameObject obj in fallingBlocks) {
            if(obj == null)
            {
                continue;
            }
            currentBlock = obj.GetComponent<Block>();
            lowerBlock = blockRows.get(currentBlock.getY() - 1).GetComponent<BlockRow>().get(currentBlock.getX()).GetComponent<Block>();
            if (lowerBlock.isSwapping() || (lowerBlock.getBlockColor() != BlockColor.Empty && !lowerBlock.isFalling()))
            {
                currentBlock.setFalling(false);
                currentBlock.enable();
                finishedBlocks.Add(obj);
                continue;
            }
            currentBlock.setFallDownTimer(currentBlock.getFallDownTimer() - Time.deltaTime);
            if(currentBlock.getFallDownTimer() <= 0)
            {
                currentBlock.setFallDownTimer(FALLDOWN_DELTA);
                swapBlockWithLowerNeighbor(currentBlock);
            }
        }
        removeAllLandedBlocksFromFallingList(finishedBlocks);
    }

    private void removeAllLandedBlocksFromFallingList(List<GameObject> finishedBlocks)
    {
        foreach(GameObject obj in finishedBlocks)
        {
            fallingBlocks.Remove(obj);
        }
    }

    private void swapBlockWithLowerNeighbor(Block blockToDecrease)
    {
        BlockRow upperBlockRow = blockRows.get(blockToDecrease.getY()).GetComponent<BlockRow>();
        BlockRow lowerBlockRow = blockRows.get(blockToDecrease.getY() -1).GetComponent<BlockRow>();

        if(upperBlockRow != null && lowerBlockRow != null)
        {
            //Grafiken und interne Werte vom Block anpassen
            Block upperBlock = blockToDecrease;
            Block lowerBlock = lowerBlockRow.get(blockToDecrease.getX()).GetComponent<Block>();

            upperBlock.transform.SetParent(lowerBlockRow.transform);
            lowerBlock.transform.SetParent(upperBlockRow.transform);

            upperBlock.setY(upperBlock.getY() - 1);
            lowerBlock.setY(lowerBlock.getY() + 1);

            //BlockColor tempColor = upperBlock.getBlockColor();
            //upperBlock.setBlockColor(lowerBlock.getBlockColor());
            //lowerBlock.setBlockColor(tempColor);
            upperBlock.transform.localPosition = new Vector3(upperBlock.getX(), 0, 0);
            lowerBlock.transform.localPosition = new Vector3(lowerBlock.getX(), 0, 0);

            //Interne Werte der Reihe anpassen
            upperBlockRow.set(blockToDecrease.getX(), lowerBlock.gameObject);
            lowerBlockRow.set(blockToDecrease.getX(), upperBlock.gameObject);
        }
    }

    private void updateCoyoteBlock()
    {
        if(this.coyoteBlock == null)
        {
            return;
        }
        this.coyoteBlock.setFallDownTimer(this.coyoteBlock.getFallDownTimer() - Time.deltaTime);
        Debug.Log(this.coyoteBlock.getFallDownTimer());
        if(this.coyoteBlock.getFallDownTimer() <= 0)
        {
            Debug.Log("Coyote time done");
            this.coyoteBlock.setLevitating(false);
            this.coyoteBlock.setFalling(true);
            this.coyoteBlock.setFallDownTimer(FALLDOWN_DELTA);
            this.fallingBlocks.Add(this.coyoteBlock.gameObject);
            this.coyoteBlock = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        updateCoyoteBlock();
        updateFallingBlocks();
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

        //50 FPS
        //1 Frame = 0,02 Sekunden
        //4 Frames zum Swappen
        //13 Frames bis zum Runterfallen nach dem Swap (9 Coyote Frames)
        //Beim Fallen: 1 Frame --> 1 Reihe tiefer
    }
}
