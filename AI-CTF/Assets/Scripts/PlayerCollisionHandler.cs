using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollisionHandler : MonoBehaviour
{
    public GameObject redFlag;
    public GameObject blueFlag;

    private void OnCollisionEnter(Collision other)
    {
        //Blue player takes Red flag
        if(other.gameObject.tag == "RedFlag" 
            && gameObject.tag == "Blue" 
            && !gameObject.GetComponent<Movement>().GetIsTagged() 
            && !gameObject.GetComponent<Movement>().isRescuing)
        {
            TakeFlag(other);
        }
        //Red Player takes Blue flag
        else if(other.gameObject.tag == "BlueFlag" 
            && gameObject.tag == "Red" 
            && !gameObject.GetComponent<Movement>().GetIsTagged() 
            && !gameObject.GetComponent<Movement>().isRescuing)
        {
            TakeFlag(other);
        }
        //Blue player Tags Red player if it is in the blue zone
        else if(other.gameObject.tag == "Red" 
            && gameObject.tag == "Blue" 
            && other.gameObject.transform.position.z > 10 
            && !other.gameObject.GetComponent<Movement>().GetIsTagged() 
            && !gameObject.GetComponent<Movement>().GetIsTagged())
        {
            other.gameObject.GetComponent<Movement>().SetIsTagged(true);

            if(other.gameObject.GetComponent<Movement>().hasFlag)
            {
                ReturnFlag(other);
            }
        }
        //Red player Tags Blue Player if it is in the red zone and untagged
        else if (other.gameObject.tag == "Blue" 
            && gameObject.tag == "Red" 
            && other.gameObject.transform.position.z < 10 
            && !other.gameObject.GetComponent<Movement>().GetIsTagged() 
            && !gameObject.GetComponent<Movement>().GetIsTagged())
        {
            other.gameObject.GetComponent<Movement>().SetIsTagged(true);

            if (other.gameObject.GetComponent<Movement>().hasFlag)
            {
                ReturnFlag(other);
            }
        }
        //Teammate releases tagged player
        else if (other.gameObject.tag == gameObject.tag)
        {
            gameObject.GetComponent<Movement>().SetIsTagged(false);
        }
        //Red Scores! Reload scene
        else if(other.gameObject.tag == "RedFlag" && gameObject.transform.GetChild(3).gameObject.tag == "BlueFlagHeld")
        {
            if(gameObject.transform.GetChild(3).gameObject.activeSelf)
            {
                SceneManager.LoadScene(1);
            }
        }
        //Blue Scores! Reload scene
        else if (other.gameObject.tag == "BlueFlag" && gameObject.transform.GetChild(3).gameObject.tag == "RedFlagHeld")
        {
            if (gameObject.transform.GetChild(3).gameObject.activeSelf)
            {
                SceneManager.LoadScene(1);
            }
        }
    }

    private void TakeFlag(Collision other)
    {   
        other.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        other.gameObject.transform.GetChild(1).gameObject.SetActive(false);
        gameObject.GetComponent<Movement>().hasFlag = true;
        gameObject.transform.GetChild(3).gameObject.SetActive(true);
    }

    private void ReturnFlag(Collision other)
    {
        other.gameObject.transform.GetChild(3).gameObject.SetActive(false);

        if(gameObject.tag == "Blue")
        {
            blueFlag.transform.GetChild(0).gameObject.SetActive(true);
            blueFlag.transform.GetChild(1).gameObject.SetActive(true);
        }                                                      
        else if (gameObject.tag == "Red")                          
        {                                                      
            redFlag.transform.GetChild(0).gameObject.SetActive(true);
            redFlag.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}
