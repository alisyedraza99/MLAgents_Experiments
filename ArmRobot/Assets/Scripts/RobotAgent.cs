using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

public class RobotAgent : Agent
{
    public GameObject endEffector;
    public GameObject cube;
    public GameObject robot;

    RobotController robotController;
    TouchDetector touchDetector;
    TablePositionRandomizer tablePositionRandomizer;


    void Start()
    {
        robotController = robot.GetComponent<RobotController>();
        touchDetector = cube.GetComponent<TouchDetector>();
        tablePositionRandomizer = cube.GetComponent<TablePositionRandomizer>();
    }


    // AGENT

    public override void OnEpisodeBegin()
    {
        float[] defaultRotations = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        robotController.ForceJointsToRotations(defaultRotations);
        touchDetector.hasTouchedTarget = false;
        tablePositionRandomizer.Move();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (robotController.joints[0].robotPart == null)
        {
            // No robot is present, no observation should be added
            return;
        }

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
            RotationDirection rotationDirection = ActionIndexToRotationDirection((int) actionBuffers.DiscreteActions[jointIndex]);
            robotController.RotateJoint(jointIndex, rotationDirection, false);
        }

        // Knocked the cube off the table
        if (cube.transform.position.y < -1.0)
        {
            SetReward(-1f);
            EndEpisode();
        }

        // end episode if we touched the cube
        if (touchDetector.hasTouchedTarget)
        {
            SetReward(1f);
            EndEpisode();
        }


        //reward
        float distanceToCube = Vector3.Distance(endEffector.transform.position, cube.transform.position); // roughly 0.7f


        var jointHeight = 0f; // This is to reward the agent for keeping high up // max is roughly 3.0f
        for (int jointIndex = 0; jointIndex < robotController.joints.Length; jointIndex ++)
        {
            jointHeight += robotController.joints[jointIndex].robotPart.transform.position.y - cube.transform.position.y;
        }
        var reward = - distanceToCube + jointHeight / 100f;

        SetReward(reward * 0.1f);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        for (int i = 0; i < actionsOut.DiscreteActions.Length; i++)
        {
            int inputVal = (int)Input.GetAxis(robotController.joints[i].inputAxis);
            Debug.Log(inputVal);
            actionsOut.DiscreteActions.Array[i] = inputVal + 1;
            //if (Mathf.Abs(inputVal) > 0)
            //{
            //    RotationDirection direction = GetRotationDirection(inputVal);
            //    robotController.RotateJoint(i, direction);
            //    return;
            //}
        }
    }

    // HELPERS

    static public RotationDirection ActionIndexToRotationDirection(int actionIndex)
    {
        return (RotationDirection)(actionIndex - 1);
    }




}


