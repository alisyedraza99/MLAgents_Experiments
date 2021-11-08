using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

public class GrippingRobotAgent : Agent
{
    public GameObject endEffector;
    public GameObject pincher;
    public GameObject cube;
    public GameObject destination;
    public GameObject robot;

    RobotController robotController;
    PincherController pincherController;
    TouchDetector touchDetector;
    TablePositionRandomizer tablePositionRandomizer;


    void Start()
    {
        robotController = robot.GetComponent<RobotController>();
        pincherController = pincher.GetComponent<PincherController>();
        touchDetector = cube.GetComponent<TouchDetector>();
        tablePositionRandomizer = cube.GetComponent<TablePositionRandomizer>();
    }


    // AGENT

    public override void OnEpisodeBegin()
    {
        float[] defaultRotations = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        robotController.ForceJointsToRotations(defaultRotations);
        touchDetector.hasTouchedTarget = false;
        touchDetector.hasReachedDest = false;
        tablePositionRandomizer.Move();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (robotController.joints[0].robotPart == null)
        {
            // No robot is present, no observation should be added
            return;
        }

        // relative target position
        Vector3 destPosition = destination.transform.position - cube.transform.position;
        sensor.AddObservation(destPosition);

        // relative cube position
        Vector3 cubePosition = cube.transform.position - robot.transform.position;
        sensor.AddObservation(cubePosition);

        // relative end position
        Vector3 endPosition = endEffector.transform.position - robot.transform.position;
        sensor.AddObservation(endPosition);
        sensor.AddObservation(cubePosition - endPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // move
        for (int jointIndex = 0; jointIndex < actionBuffers.DiscreteActions.Length; jointIndex ++)
        {
            if (jointIndex < actionBuffers.DiscreteActions.Length - 1)
            {
                RotationDirection rotationDirection = ActionIndexToRotationDirection((int)actionBuffers.DiscreteActions[jointIndex]);
                robotController.RotateJoint(jointIndex, rotationDirection, false);
            }
            else
            {
                pincherController.gripState = GripStateForInput(actionBuffers.DiscreteActions[jointIndex] - 1);

            }
        }

        // Knocked the cube off the table
        if (cube.transform.position.y < -1.0)
        {
            SetReward(-1f);
            EndEpisode();
        }

        // end episode if we touched the cube
        if (touchDetector.hasReachedDest)
        {
            SetReward(1f);
            EndEpisode();
        }


        //reward
        float distanceToCube = Vector3.Distance(endEffector.transform.position, cube.transform.position); // roughly 0.7f
        float cubeDistToDest = Vector3.Distance(cube.transform.position, destination.transform.position);


        var jointHeight = 0f; // This is to reward the agent for keeping high up // max is roughly 3.0f
        for (int jointIndex = 0; jointIndex < robotController.joints.Length; jointIndex ++)
        {
            jointHeight += robotController.joints[jointIndex].robotPart.transform.position.y - cube.transform.position.y;
        }

        var reward = 0f;
        if (touchDetector.hasTouchedTarget)
        {
            touchDetector.hasTouchedTarget = false;
            reward = - Mathf.Min(cubeDistToDest * 0.5f, distanceToCube * 2f) + jointHeight / 100f;
        }
        else
        {
            reward = - (distanceToCube * 2f) + jointHeight / 100f;
        }
        //var reward = - distanceToCube + jointHeight / 100f;
        //Debug.Log(reward * 0.1f);
        SetReward(reward * 0.1f);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int inputVal = 0;
        for (int i = 0; i < actionsOut.DiscreteActions.Length; i++)
        {
            if (i < actionsOut.DiscreteActions.Length - 1)
            {
                inputVal = (int)Input.GetAxis(robotController.joints[i].inputAxis);
            }
            else
            {
                inputVal = (int) Input.GetAxis("Fingers");
            }
            actionsOut.DiscreteActions.Array[i] = inputVal + 1;
        }
    }

    // HELPERS

    static public RotationDirection ActionIndexToRotationDirection(int actionIndex)
    {
        return (RotationDirection)(actionIndex - 1);
    }

    static GripState GripStateForInput(float input)
    {
        if (input > 0)
        {
            return GripState.Closing;
        }
        else if (input < 0)
        {
            return GripState.Opening;
        }
        else
        {
            return GripState.Fixed;
        }
    }



}


