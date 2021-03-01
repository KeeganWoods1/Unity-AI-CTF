using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Movement : MonoBehaviour
{
    public GameObject target;
    public GameObject target2;
    public bool hasFlag = false;
    public bool isRescuing = false;
    
    [SerializeField]
    bool isKinematic = true;
    [SerializeField]
    bool isFleeing = false;
    [SerializeField]
    bool isPlayingCTF = false;
    [SerializeField]
    bool isWandering = false;
    [SerializeField]
    bool isTagged = false;
    [SerializeField]
    float rotationSpeedRads = 1.0f;
    [SerializeField]
    float arrivalRadiusKinematic = 1.0f;
    [SerializeField]
    float arrivalRadiusSteering = 1.5f;

    float speed;
    float dragCoefficient;
    float distanceFromTarget;
    float angleOfView;
    float directRouteDistance;
    float toroidRouteDistance;
    
    Quaternion lookWhereYoureGoing;
 
    Vector3 goalFacing;
    Vector3 heading;
    Vector3 direction;
    Vector3 velocity;
    Vector3 acceleration;
    Vector3 next_pos;
    Vector3 intermediateTarget;
    Vector3 pursuitLeadPos;
    Vector3 nextRandomPoint;
    
    
    float maxSpeed = 2.0f;
    float accelMax = 0.5f;
    float t2t = 0.5f;
    float t2tKinematic = 1.5f;   
    float pursuitLeadDistance = 2.0f;
    float wanderCircleCenterOffset = 20f;
    float wanderCircleRadius = 100f;
    float maxCircleVariance = 0.0f;
    float pointRefreshTimer = 1;
    bool isFacing = false;

    void Update()
    {        
        GetComponent<Rigidbody>().velocity = Vector3.zero;

        //Demo Flee
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFleeing = !isFleeing;
        }
        //Demo Kinematic/Steering
        if(Input.GetKeyDown(KeyCode.K))
        {
            isKinematic = !isKinematic;
        }
        //Set new target on the fly
        if (Input.GetKeyDown(KeyCode.N))
        {
            if(target2)
            {
                target = target2;
            }
        }
        //Load Scene 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene(0);
        }
        //Load Scene 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene(1);
        }
        //Wander
        if (isWandering)
        {
            Wander();
        }
        //Freeze when tagged
        else if (isTagged)
        {
            velocity = Vector3.zero;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        //Kinematic behaviour
        else if (isKinematic) 
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            if (!isFleeing)
            {
                //Use pursue if demonstrating CTF game, Arrive if demonstrating R2
                if (!isPlayingCTF)
                {
                    KinematicArrive(); 
                }
                else 
                {
                    KinematicPursue();
                }                       
            }
            else
            {
                KinematicFlee();
            }         
        }
        //Steering behaviour
        else
        {
            
            if (!isFleeing)
            {
                //Use pursue if demonstrating CTF game, Arrive if demonstrating R2
                if (!isPlayingCTF)
                {
                    SteeringArrive();
                }
                else
                {
                    SteeringPursue();
                }
            }
            else
            {
                SteeringFlee();
            }
        }
    }

    private void KinematicArrive()
    {
        speed = 2.0f;
        heading = ChartCourse(target.transform.position);
        distanceFromTarget = heading.magnitude;
        goalFacing = heading.normalized;
        lookWhereYoureGoing = Quaternion.LookRotation(goalFacing, Vector3.up);

        //Requirement 2: A-i, A-ii
        if (speed <= 1.25)
        {
            if (distanceFromTarget > 3.0f && lookWhereYoureGoing != transform.rotation)
            {
                KinematicAlign(heading);
            }
            else
            {
                if (distanceFromTarget < arrivalRadiusKinematic)
                {
                    speed = 0;
                }
                else
                {
                    speed = Mathf.Min(speed, distanceFromTarget / t2tKinematic);
                }

                KinematicSeek(heading, distanceFromTarget);
            }
        }

        //Requirement 2: B-i, B-ii
        else
        {
            //Calculate target distance from the line in the direction character is currently facing            
            float angleTarget = Vector3.Angle(heading, transform.forward);

            //Restrict angle as a function of speed
            angleOfView = 20;
            angleOfView = angleOfView / speed;

            //rotate while moving
            if (angleTarget < angleOfView)
            {
                KinematicAlign(heading);

                if (distanceFromTarget < arrivalRadiusKinematic)
                {
                    speed = 0;
                }
                else
                {
                    speed = Mathf.Min(speed, distanceFromTarget / t2tKinematic);
                }

                KinematicSeek(heading, distanceFromTarget);
            }
            //rotate in place then move
            else
            {
                if (distanceFromTarget > 3.0f && lookWhereYoureGoing != transform.rotation)
                {
                    KinematicAlign(heading);
                }
                else
                {
                    if (distanceFromTarget < arrivalRadiusKinematic)
                    {
                        speed = 0;
                    }
                    else
                    {
                        speed = Mathf.Min(speed, distanceFromTarget / t2tKinematic);
                    }

                    KinematicSeek(heading, distanceFromTarget);
                }
            }
        }
    }

    //Kinematic Seek but with ofset X in direction of Target velocity (Seek but for moving target)
    private void KinematicPursue()
    {
        speed = 2.0f;
        pursuitLeadPos = target.transform.position;

        if (target.GetComponent<Movement>())
        {
            pursuitLeadPos = target.transform.position + (target.GetComponent<Movement>().velocity.normalized * pursuitLeadDistance);
        }
        
        heading = ChartCourse(pursuitLeadPos);
        distanceFromTarget = heading.magnitude;
        goalFacing = heading.normalized;
        lookWhereYoureGoing = Quaternion.LookRotation(goalFacing, Vector3.up);

        //Calculate target distance from the line in the direction character is currently facing            
        float angleTarget = Vector3.Angle(heading, transform.forward);

        //Restrict angle as a function of speed
        angleOfView = 20;
        angleOfView = angleOfView / speed;

        //rotate while moving
        if (angleTarget < angleOfView)
        {
            KinematicAlign(heading);

            if (distanceFromTarget < arrivalRadiusKinematic)
            {
                speed = 0;
            }

            KinematicSeek(heading, distanceFromTarget);
        }
        //rotate in place then move
        else
        {
            if (distanceFromTarget > 3.0f && lookWhereYoureGoing != transform.rotation)
            {
                KinematicAlign(heading);
            }
            else
            {
                if (distanceFromTarget < arrivalRadiusKinematic)
                {
                    speed = 0;
                }
                else
                {
                    speed = Mathf.Min(speed, distanceFromTarget / t2tKinematic);
                }

                KinematicSeek(heading, distanceFromTarget);
            }
        }
    }
    
    private void KinematicFlee()
    {
        //Requirement 2: C-i, C-ii
        speed = 2.0f;
        heading = transform.position - target.transform.position;
        distanceFromTarget = heading.magnitude;
        goalFacing = heading.normalized;
        lookWhereYoureGoing = Quaternion.LookRotation(goalFacing, Vector3.up);

        if (distanceFromTarget > 3.0f && lookWhereYoureGoing != transform.rotation)
        {
            KinematicAlign(heading);
        }
        else
        {
            KinematicSeek(heading, distanceFromTarget);
        }
    }

    private void KinematicSeek(Vector3 heading, float distanceFromTarget)
    {
        direction = heading / distanceFromTarget;
        velocity = direction * speed;

        next_pos = transform.position + (velocity * Time.deltaTime);
        next_pos.y = 0.25f;
        transform.position = next_pos;
    }

    private void KinematicAlign(Vector3 heading)
    {
        goalFacing = heading.normalized;
        lookWhereYoureGoing = Quaternion.LookRotation(goalFacing, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookWhereYoureGoing, rotationSpeedRads);
    }

    private void SteeringArrive()
    {
        heading = ChartCourse(target.transform.position);
        distanceFromTarget = heading.magnitude;
        direction = heading / distanceFromTarget;
        speed = velocity.magnitude;
        acceleration = accelMax * direction;
        goalFacing = heading.normalized;
        lookWhereYoureGoing = Quaternion.LookRotation(goalFacing, Vector3.up);

        //Requirement 2: A-i, A-ii
        if (speed <= 1.25)
        {
            if (distanceFromTarget > 3.0f && lookWhereYoureGoing != transform.rotation)
            {
                SteeringAlign(heading);
            }
            else
            {
                if (distanceFromTarget < arrivalRadiusSteering)
                {
                    acceleration = (new Vector3(0, 0, 0) - velocity) / t2t;
                }
                else
                {
                    acceleration = ((direction * maxSpeed) - velocity) / t2t;
                }

                SteeringSeek(direction, acceleration);
            }
        }

        //Requirement 2: B-i, B-ii
        else
        {
            //Calculate target distance from the line in the direction character is currently facing            
            float angleTarget = Vector3.Angle(heading, transform.forward);

            //Restrict angle as a function of speed
            angleOfView = 20;
            angleOfView = angleOfView / speed;

            //Target is in FOV: rotate while moving
            if (angleTarget < angleOfView)
            {
                SteeringAlign(heading);

                if (distanceFromTarget < arrivalRadiusSteering)
                {
                    acceleration = (new Vector3(0, 0, 0) - velocity) / t2t;
                }
                else
                {
                    acceleration = ((direction * maxSpeed) - velocity) / t2t;
                }

                SteeringSeek(direction, acceleration);
            }
            //Target is out of FOV: rotate in place then move
            else
            {             
                if (distanceFromTarget > 3.0f && lookWhereYoureGoing != transform.rotation)
                {
                    //Stop
                    EmergencyBreak();
                    //Rotate
                    SteeringAlign(heading);
                }
                //Arrive
                else
                {
                    if (distanceFromTarget < arrivalRadiusSteering)
                    {
                        acceleration = (new Vector3(0, 0, 0) - velocity) / t2t;
                    }
                    else 
                    {
                        acceleration = ((direction * maxSpeed) - velocity) / t2t;
                    }

                    SteeringSeek(direction, acceleration);
                }
            }
        }

        isFacing = false;
    }
    //Steering Seek but with ofset X in direction of Target velocity (Seek but for moving target)
    private void SteeringPursue()
    {
        pursuitLeadPos = target.transform.position;

        if (target.GetComponent<Movement>())
        {
            pursuitLeadPos = target.transform.position + (target.GetComponent<Movement>().velocity.normalized * pursuitLeadDistance);
        }

        heading = ChartCourse(pursuitLeadPos);
        distanceFromTarget = heading.magnitude;
        direction = heading / distanceFromTarget;
        speed = velocity.magnitude;
        acceleration = accelMax * direction;
        goalFacing = heading.normalized;
        lookWhereYoureGoing = Quaternion.LookRotation(goalFacing, Vector3.up);

        //Calculate target distance from the line in the direction character is currently facing            
        float angleTarget = Vector3.Angle(heading, transform.forward);

        //Restrict angle as a function of speed
        angleOfView = 20;
        angleOfView = angleOfView / speed;

        //Target is in FOV: rotate while moving
        if (angleTarget < angleOfView)
        {
            SteeringAlign(heading);

            if (distanceFromTarget < arrivalRadiusSteering)
            {
                acceleration = (new Vector3(0, 0, 0) - velocity) / t2t;
            }

            SteeringSeek(direction, acceleration);
        }
        //Target is out of FOV: rotate in place then move
        else
        {
            if (distanceFromTarget > 3.0f && lookWhereYoureGoing != transform.rotation)
            {
                //Stop
                EmergencyBreak();
                //Rotate
                SteeringAlign(heading);
            }
        }

        isFacing = false;
    }
  
    private void SteeringFlee()
    {
        //Requirement 2: C-i, C-ii
        //Heading away from target will produce accel in direction opposite of target
        heading = transform.position - target.transform.position;
        distanceFromTarget = heading.magnitude;
        direction = heading / distanceFromTarget;
        acceleration = accelMax * direction;

        if (distanceFromTarget > 3.0f && !isFacing)
        {
            //Stop
            EmergencyBreak();        
            //Rotate
            SteeringAlign(heading);
        }       
        else if((-velocity.x - direction.x) < 0.5 && (-velocity.z - direction.z) < 0.5)
        {
            //Stop
            EmergencyBreak();
        }
        else
        {
            //Flee
            SteeringSeek(direction, acceleration);
        }
    }

    private void SteeringSeek(Vector3 direction, Vector3 acceleration)
    {
        velocity += acceleration * (Time.deltaTime);

        //clamp velocity to maxSpeed
        if(velocity.magnitude > maxSpeed)
        {
            dragCoefficient = maxSpeed / velocity.magnitude;
            velocity *= dragCoefficient;
        }

        next_pos = transform.position + (velocity * Time.deltaTime);
        next_pos.y = 0.25f;
        transform.position = next_pos;
    }

    private void SteeringAlign(Vector3 heading)
    {
        goalFacing = heading.normalized;
        lookWhereYoureGoing = Quaternion.LookRotation(goalFacing, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookWhereYoureGoing, rotationSpeedRads);

        if(transform.rotation == lookWhereYoureGoing)
        {
            isFacing = true;
        }
        else 
        {
            isFacing = false;
        }
    }

    //For emergencies only
    private void EmergencyBreak()
    {
        acceleration = Vector3.zero;
        velocity = Vector3.zero;
    }
    
    //Heading setter that takes into account torroidal nature of world map.
    private Vector3 ChartCourse(Vector3 targetPosition)
    {
        float xDistance;
        float zDistance;
        float x2TempDistance;
        float z2TempDistance;
        float distanceRatio;
        float tempDistance1;
        float tempDistance2;
        float distanceFromBound;

        heading = targetPosition - transform.position;
        directRouteDistance = heading.magnitude;

       //Distance using horizontal wrap
        if (transform.position.x < 10)
        {
            xDistance = (transform.position.x) + (20 - target.transform.position.x);
            zDistance = target.transform.position.z - transform.position.z;
        }
        else
        {
            xDistance = (20 - transform.position.x) + target.transform.position.x;
            zDistance = target.transform.position.z - transform.position.z;
        }
        
        tempDistance1 = Mathf.Sqrt(Mathf.Pow(xDistance, 2) + Mathf.Pow(zDistance, 2));

        //Distance using vertical wrap
        if(transform.position.z < 10)
        {
            x2TempDistance = target.transform.position.x - transform.position.x;
            z2TempDistance = (transform.position.z) + (20 - target.transform.position.z);
        }
        else 
        {
            x2TempDistance = target.transform.position.x - transform.position.x;
            z2TempDistance = (20 - transform.position.z) + target.transform.position.z;
        }

        tempDistance2 = Mathf.Sqrt(Mathf.Pow(x2TempDistance, 2) + Mathf.Pow(z2TempDistance, 2));

        if (tempDistance2 < tempDistance1)
        {
            toroidRouteDistance = tempDistance2;
            xDistance = x2TempDistance;
            zDistance = z2TempDistance;
        }

        else
        {
            toroidRouteDistance = tempDistance1;
        }

        if(tempDistance2 < tempDistance1 && directRouteDistance > toroidRouteDistance)
        { 
            if (target.transform.position.z < 10)
            {
                intermediateTarget.z = 19.9f;
                distanceFromBound = 20 - transform.position.z;
            }
            else
            {
                intermediateTarget.z = -0.1f;
                distanceFromBound = transform.position.z;
            }

            distanceRatio = (distanceFromBound) / zDistance;
            intermediateTarget.x = (distanceRatio * xDistance) + transform.position.x;
            heading = intermediateTarget - transform.position;
            heading.y = 0;
        }

        else if (directRouteDistance > toroidRouteDistance)
        {
            if (target.transform.position.x < 10)
            {    
                intermediateTarget.x = 19.9f;
                distanceFromBound = 20 - transform.position.x;
            }
            else
            {
                intermediateTarget.x = -0.1f;
                distanceFromBound = transform.position.x;
            }

            distanceRatio = (distanceFromBound) / xDistance;
            intermediateTarget.z = (distanceRatio * zDistance) + transform.position.z;
            heading = intermediateTarget - transform.position;
            heading.y = 0;
        }

        return heading;
    }

    private void Wander()
    {
        speed = 2;
        nextRandomPoint = WanderCirclePoint();
        pointRefreshTimer += 1;
        
        direction = (nextRandomPoint - transform.position).normalized;
        velocity = direction * speed;
        next_pos = transform.position + (velocity * Time.deltaTime);
        transform.position = next_pos;
    }

    private Vector3 WanderCirclePoint()
    {
        Vector3 wanderCircleCenter = transform.position + (Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized * wanderCircleCenterOffset);
        Vector3 wanderCirclePoint = wanderCircleRadius * (new Vector3(Mathf.Cos(Random.Range(maxCircleVariance, Mathf.PI - maxCircleVariance)), 0.0f, Mathf.Cos(Random.Range(maxCircleVariance, Mathf.PI - maxCircleVariance))));
        return wanderCirclePoint + wanderCircleCenter;
    }

    public bool GetWandering()
    {
        return isWandering;
    }

    public void SetWandering(bool b)
    {
        isWandering = b;
    }

    public bool GetIsTagged()
    {
        return isTagged;
    }

    public void SetIsTagged(bool b)
    {
        isTagged = b;
    }

}
