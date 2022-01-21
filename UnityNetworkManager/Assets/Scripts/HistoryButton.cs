using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryButton : MonoBehaviour
{
    public ExerciseData curData;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DisplayHistory()
    {
        GameObject.FindObjectOfType<ExampleGameManager>().DisplayHistory(curData);
    }

    public void DisplayDeletePopUp()
    {
        GameObject.FindObjectOfType<ExampleGameManager>().DisplayDeletePopUp(curData, this.gameObject);
    }

}