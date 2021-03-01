using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] GameObject[] teamRed;
    [SerializeField] GameObject[] teamBlue;
    [SerializeField] GameObject redFlag;
    [SerializeField] GameObject blueFlag;

    GameObject redPlayerAttack;
    GameObject redPlayerDefend;
    GameObject bluePlayerAttack;
    GameObject bluePlayerDefend;
    
    List<int> validPlayerPool;

    int closestTeammateRedIndex;
    int closestTeammateBlueIndex;   
    int taggedPlayerRedIndex;
    int taggedPlayerBlueIndex;

    int redIndex = -1;
    int blueIndex = -1;
    bool isAttackingRed = false;
    bool isAttackingBlue = false;
    bool isDefendingRed = false;
    bool isDefendingBlue = false;
    bool isFreeingTeammateRed = false;
    bool isFreeingTeammateBlue = false;

    private void Start()
    {
        validPlayerPool = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        //Send random "wandering" or untasked unit on a flag run
        AttackFlag();
        //Check for enemies in you territory and assign untasked unit to "pursue"
        DefendBase();
        //Leftover units are tasked with freeing their "tagged" teammates
        FreeCommrades();
    }

    void AttackFlag()
    {
        int redIndex;
        int blueIndex;

        if(!isAttackingRed)
        {
            if(validPlayerPool.Count > 0)
                validPlayerPool.Clear();

            //Select a player at random and attack enemy flag
            for (int i = 0; i < teamRed.Length; i++)
            {
                if(teamRed[i].GetComponent<Movement>().GetWandering())
                {
                    validPlayerPool.Add(i);
                }
            }
            if (validPlayerPool.Count > 0)
            {
                redIndex = validPlayerPool[Random.Range(0, validPlayerPool.Count - 1)];
                redPlayerAttack = teamRed[redIndex];
                redPlayerAttack.GetComponent<Movement>().target = blueFlag;
                isAttackingRed = true;
                redPlayerAttack.GetComponent<Movement>().SetWandering(false);
            }
            else
            {
                redPlayerAttack = null;
            }
        }
        //Return with enemy flag
        if ( redPlayerAttack != null && redPlayerAttack.GetComponent<Movement>().hasFlag)
        {
            redPlayerAttack.GetComponent<Movement>().target = redFlag;
        }

        if(!isAttackingBlue)
        {
            if (validPlayerPool.Count > 0)
                validPlayerPool.Clear();

            //Select a player at random and attack enemy flag
            for (int i = 0; i < teamBlue.Length; i++)
            {
                if (teamBlue[i].GetComponent<Movement>().GetWandering())
                {
                    validPlayerPool.Add(i);
                }
            }
            if(validPlayerPool.Count > 0)
            {
                //Attack enemy flag
                blueIndex = validPlayerPool[Random.Range(0, validPlayerPool.Count - 1)];
                bluePlayerAttack = teamBlue[blueIndex];
                bluePlayerAttack.GetComponent<Movement>().target = redFlag;
                isAttackingBlue = true;
                bluePlayerAttack.GetComponent<Movement>().SetWandering(false);
            }
            else
            {
                bluePlayerAttack = null;
            }
        }
        //Return with enemy flag
        if (bluePlayerAttack != null && bluePlayerAttack.GetComponent<Movement>().hasFlag)
        {
            bluePlayerAttack.GetComponent<Movement>().target = blueFlag;
        }
    }

    void DefendBase()
    {
        float distanceToTarget = 100.0f;
        float tempDistanceToTarget;

        //If a Red attacker is on the Blue side and untagged.
        if(redPlayerAttack != null && redPlayerAttack.transform.position.z > 10 && !redPlayerAttack.GetComponent<Movement>().GetIsTagged())
        {
            if(!isDefendingBlue)
            {
                //Select nearest defender to attacker that is available or "wandering"
                for (int i = 0; i < teamBlue.Length; i++)
                {
                    if (teamBlue[i].GetComponent<Movement>().GetWandering())
                    {     
                        tempDistanceToTarget = (redPlayerAttack.transform.position - teamBlue[i].transform.position).magnitude;

                        if(tempDistanceToTarget < distanceToTarget)
                        {
                            blueIndex = i;
                            distanceToTarget = tempDistanceToTarget;
                        }
                    }
                }
                isDefendingBlue = true;
            }
            //Chase red player if possible
            if(blueIndex >= 0)
            {
                bluePlayerDefend = teamBlue[blueIndex];
                bluePlayerDefend.GetComponent<Movement>().target = redPlayerAttack;
                bluePlayerDefend.GetComponent<Movement>().SetWandering(false);
            }
            else
            {
                bluePlayerDefend = null;
            }
        }
        //return to start position and wander after tagging player or player escapes with flag
        else if((bluePlayerDefend != null && redPlayerAttack != null) && ((redPlayerAttack.GetComponent<Movement>().GetIsTagged()) || 
            (redPlayerAttack.transform.position.z < 10 && 
            redPlayerAttack.GetComponent<Movement>().hasFlag)))
        {
            blueIndex = -1;
            isDefendingBlue = false;
            isAttackingRed = false;
            bluePlayerDefend.GetComponent<Movement>().SetWandering(true);          
        }

        //If a Blue attacker is on the Red side and untagged.
        if (bluePlayerAttack != null && bluePlayerAttack.transform.position.z < 10 && !bluePlayerAttack.GetComponent<Movement>().GetIsTagged())
        {
            if (!isDefendingRed)
            {
                //Select nearest defender to attacker that is available or "wandering"
                for (int i = 0; i < teamRed.Length; i++)
                {
                    if (teamRed[i].GetComponent<Movement>().GetWandering())
                    {
                        tempDistanceToTarget = (bluePlayerAttack.transform.position - teamRed[i].transform.position).magnitude;

                        if (tempDistanceToTarget < distanceToTarget)
                        {
                            redIndex = i;
                            distanceToTarget = tempDistanceToTarget;
                        }
                    }
                }
                isDefendingRed = true;
            }
            //Chase red player if possible
            if (redIndex >= 0)
            {
                redPlayerDefend = teamRed[redIndex];
                redPlayerDefend.GetComponent<Movement>().target = bluePlayerAttack;
                redPlayerDefend.GetComponent<Movement>().SetWandering(false);
            }
            else
            {
                redPlayerDefend = null;
            }
        }
        //return to start position and wander after tagging player or player escapes with flag
        else if ((redPlayerDefend != null && bluePlayerAttack != null) &&  ((bluePlayerAttack.GetComponent<Movement>().GetIsTagged()) || 
            ((bluePlayerAttack.transform.position.z > 10) && 
            bluePlayerAttack.GetComponent<Movement>().hasFlag)))
        {
            redIndex = -1;
            isDefendingRed = false;
            isAttackingBlue = false;
            redPlayerDefend.GetComponent<Movement>().SetWandering(true);
        }
    }

    void FreeCommrades()
    {
        //Red Team
        if (!isFreeingTeammateRed)
        {
            //Find a tagged player
            taggedPlayerRedIndex = LocateTaggedPlayerIndex(teamRed);
            //Find closest teammate
            if (taggedPlayerRedIndex >= 0)
            {
                closestTeammateRedIndex = FindClosestTeammateIndex(teamRed);

                //Send rescue
                if (closestTeammateRedIndex >= 0)
                {
                    teamRed[closestTeammateRedIndex].GetComponent<Movement>().SetWandering(false);
                    teamRed[closestTeammateRedIndex].GetComponent<Movement>().isRescuing = true;
                    teamRed[closestTeammateRedIndex].GetComponent<Movement>().target = teamRed[taggedPlayerRedIndex];
                }
                isFreeingTeammateRed = true;
            }
        }
        
        else 
        {
            //Return home
            if (!teamRed[taggedPlayerRedIndex].GetComponent<Movement>().GetIsTagged() && (teamRed[taggedPlayerRedIndex].transform.position.z > 7))
            {
                teamRed[taggedPlayerRedIndex].GetComponent<Movement>().target = redFlag;
            }
            //Return home
            if (!teamRed[taggedPlayerRedIndex].GetComponent<Movement>().GetIsTagged() && (teamRed[closestTeammateRedIndex].transform.position.z > 7))
            {
                teamRed[closestTeammateRedIndex].GetComponent<Movement>().target = redFlag;
            }
            //Set to wander
            else if (!teamRed[taggedPlayerRedIndex].GetComponent<Movement>().GetIsTagged())
            {
                teamRed[closestTeammateRedIndex].GetComponent<Movement>().SetWandering(true);
                teamRed[closestTeammateRedIndex].GetComponent<Movement>().isRescuing = false;
                teamRed[taggedPlayerRedIndex].GetComponent<Movement>().SetWandering(true);              
                isFreeingTeammateRed = false;
            }
        }

        //Blue Team
        if (!isFreeingTeammateBlue)
        {
            //Find a tagged player
            taggedPlayerBlueIndex = LocateTaggedPlayerIndex(teamBlue);
            //Find closest teammate
            if (taggedPlayerBlueIndex >= 0)
                {
                    closestTeammateBlueIndex = FindClosestTeammateIndex(teamBlue);
           
                //Send rescue
                if (closestTeammateBlueIndex >= 0)
                {
                    teamBlue[closestTeammateBlueIndex].GetComponent<Movement>().SetWandering(false);
                    teamBlue[closestTeammateBlueIndex].GetComponent<Movement>().isRescuing = true;
                    teamBlue[closestTeammateBlueIndex].GetComponent<Movement>().target = teamBlue[taggedPlayerBlueIndex];
                }
                isFreeingTeammateBlue = true; 
            }
        }

        else
        {
            //Return home
            if (!teamBlue[taggedPlayerBlueIndex].GetComponent<Movement>().GetIsTagged() && (teamBlue[taggedPlayerBlueIndex].transform.position.z < 13))
            {
                teamBlue[taggedPlayerBlueIndex].GetComponent<Movement>().target = blueFlag;
            }
            //Return home
            if (!teamBlue[taggedPlayerBlueIndex].GetComponent<Movement>().GetIsTagged() && (teamBlue[closestTeammateBlueIndex].transform.position.z < 13))
            {
                teamBlue[closestTeammateBlueIndex].GetComponent<Movement>().target = blueFlag;
            }
            //Set to wander
            else if(!teamBlue[taggedPlayerBlueIndex].GetComponent<Movement>().GetIsTagged())
            {
                teamBlue[closestTeammateBlueIndex].GetComponent<Movement>().SetWandering(true);
                teamBlue[closestTeammateBlueIndex].GetComponent<Movement>().isRescuing = false;
                teamBlue[taggedPlayerBlueIndex].GetComponent<Movement>().SetWandering(true);
                isFreeingTeammateBlue = false;
            }
        }

    }

    int LocateTaggedPlayerIndex(GameObject[] team)
    {
        for(int i = 0; i < team.Length; i++)
        {
            if(team[i].GetComponent<Movement>().GetIsTagged())
            {
                return i;
            }
        }

        return -1; 
    }

    int FindClosestTeammateIndex(GameObject[] team)
    {
        float tempDistanceToTarget;
        float distanceToTarget = 100.0f;
        int index = -1;

        for (int i = 0; i < team.Length; i++)
        {
            if (team[i].GetComponent<Movement>().GetWandering())
            {
                tempDistanceToTarget = (redPlayerAttack.transform.position - team[i].transform.position).magnitude;

                if (tempDistanceToTarget < distanceToTarget)
                {
                    index = i;
                    distanceToTarget = tempDistanceToTarget;
                }
            }
        }

        if(index >= 0)
        {
            return index;
        }

        else
        {
            return -1;
        }        
    }
}
