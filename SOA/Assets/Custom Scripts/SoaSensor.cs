﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class SensorModality
{
    public string tagString;
    public float RangeP1;
    public float RangeMax;
}

public class SoaSensor : MonoBehaviour 
{
    public SensorModality[] modes;
    public SoaActor soaActor;
    public List<GameObject> possibleDetections;
    //public float UpdateRate = 1f;
	// Use this for initialization
	void Start () 
    {
        soaActor = gameObject.GetComponentInParent<SoaActor>();
	}

	void Update () 
    {
	}

    public void CheckDetections(List<GameObject> targets)
    {
        foreach (GameObject target in targets)
        {
            // The object being detected must be alive
            if (target.GetComponent<SoaActor>().isAlive)
            {
                // Loop through all possible detect modes
                foreach (SensorModality mode in modes)
                {
                    Vector3 delta = transform.position - target.transform.position;
                    float slantRange = delta.magnitude / SimControl.KmToUnity;

                    if (mode.tagString == target.tag)
                    {
                        if (slantRange < mode.RangeMax)
                        {
                            if (slantRange < mode.RangeP1)
                            {
                                // Debug.Log(soaActor.name + " detects " + target.name + " at " + slantRange + "km");
                                LogDetection(target.gameObject);
                                ClassifyTarget(target.gameObject);
                            }
                            else
                            {
                                if (Random.value < (mode.RangeMax - slantRange) / (mode.RangeMax - mode.RangeP1))
                                {
                                    // Debug.Log(soaActor.name + " detects " + target.name + " at " + slantRange + "km");
                                    LogDetection(target.gameObject);
                                    ClassifyTarget(target.gameObject);
                                }
                                else
                                {
                                    // Debug.Log(soaActor.name + " failed detect of " + target.name + " at " + slantRange + "km");
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    public void logKill(SoaActor killedActor)
    {
        soaActor.killDetections.Add(new soa.Belief_Actor(
            killedActor.unique_id, (int)killedActor.affiliation, killedActor.type, false, (int)killedActor.isCarrying, killedActor.isWeaponized,
            killedActor.transform.position.x / SimControl.KmToUnity,
            killedActor.transform.position.y / SimControl.KmToUnity,
            killedActor.transform.position.z / SimControl.KmToUnity));
    }
    /*
    void OnTriggerStay(Collider other)
    {
        foreach(Modality mode in modes)
        {
            if (mode.tagString == other.gameObject.tag)
            {
                Vector3 delta = transform.position - other.transform.position;
                float slantRange = delta.magnitude;

                if (slantRange < mode.RangeMax)
                {
                    if (slantRange < mode.RangeP1)
                    {
                        LogDetection(other.gameObject);
                    }
                    else
                    {
                        LogPossibleDetection(other.gameObject);
                    }
                }
            }
        }
    }*/

    void LogDetection(GameObject detectedObject)
    {
        if (soaActor.Detections.IndexOf(detectedObject) == -1)
        {
            // twupy1
            // Debug.Log("Adding detection to soa actor list " + soaActor.unique_id);
            soaActor.Detections.Add(detectedObject);
        }
    }

    void ClassifyTarget(GameObject targetObject)
    {
        // Save pointer to target's AoaActor
        SoaActor targetActor = targetObject.GetComponent<SoaActor>();

        // Save the target's unique ID
        int targetUniqueId = targetActor.unique_id;

        // Check if target has been classified already
        if(!soaActor.checkClassified(targetUniqueId)){
            // We are in here because the target has not been classified yet, let's see what we can do
            bool classificationSuccessful = false;

            if (soaActor.affiliation == targetActor.affiliation)
            {
                // People on the same team always know one another
                classificationSuccessful = true;
            }
            else if(soaActor.affiliation == Affiliation.BLUE)
            {
                // Target is either red or neutral
                switch (soaActor.type)
                {
                    case (int)SoaActor.ActorType.HEAVY_LIFT:
                        // Blue heavy lift can immediately classify anything it sees
                        classificationSuccessful = true;
                        break;
                    case (int)SoaActor.ActorType.SMALL_UAV:
                        // Blue small UAV probability of classification varies by range
                        Vector3 delta = transform.position - targetObject.transform.position;
                        float slantRange = delta.magnitude / SimControl.KmToUnity;
                        if (slantRange < 0.5)
                        {
                            // Always can classify within 500 m
                            classificationSuccessful = true;
                        }
                        else if(Random.value <= 1.0f/(slantRange + 0.5f))
                        {
                            // Falls off as 1000m/(r+500m) after 500 m
                            classificationSuccessful = true;
                        }
                        break;
                    case (int)SoaActor.ActorType.BALLOON:
                        // Balloon cannot classify
                        classificationSuccessful = false;
                        break;
                    case (int)SoaActor.ActorType.POLICE:
                        // Police can immediately classify anything it sees
                        classificationSuccessful = true;
                        break;
                }
            }else if(soaActor.affiliation == Affiliation.RED)
            {
                // Red can classify anything it sees immediately
                classificationSuccessful = true;
            }

            // Set target as classified if it passed the checks
            if (classificationSuccessful)
            {
                soaActor.setClassified(targetUniqueId);
            }
        }
    }
}
