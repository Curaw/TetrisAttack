using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    [SerializeField] private GameObject playingFieldGO;
    [SerializeField] private float shiftSpeed = 0.5f;
    private Field playingField;
    private int pixelsShifted = 0;
   
    private float shiftTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        this.playingField = playingFieldGO.GetComponent<Field>();
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        shiftTime += dt;

        if (shiftTime > shiftSpeed)
        {
            shiftTime = 0;
            pixelsShifted += 1;
            if(pixelsShifted == 16)
            {
                pixelsShifted = 0;
                activateLastRow();
                addRandomBlockRow();
            } else
            {
                playingField.shiftEverythingUp();
            }
        }
    }

    private void activateLastRow()
    {
        playingField.activateLastRow();
    }

    private void addRandomBlockRow()
    {
        playingField.addRandomBlockRow();
    }
}