using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;
using JointKinect = Windows.Kinect.Joint;

public class BodySourceView : MonoBehaviour
{
    public BodySourceManager scriptBodySourceManager;
    public GameObject JointObject;

    private Dictionary<ulong, GameObject> bodies = new Dictionary<ulong, GameObject>();
    private List<JointType> jointsList = new List<JointType>
    {
        JointType.HandLeft,
        JointType.HandRight,
    };
    private void Update()
    {
        Body[] data = scriptBodySourceManager.GetData();
        if (data == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }
            if (body.IsTracked == true)
            {
                trackedIds.Add(body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(bodies.Keys);
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(bodies[trackingId]);
                bodies.Remove(trackingId);
            }
        }

        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }
            if (body.IsTracked == true)
            {
                if (!bodies.ContainsKey(body.TrackingId))
                {
                    bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                    UpdateBodyObject(body, bodies[body.TrackingId]);
                }
            }
        }
    }
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject($"Body: [{id}]");
        foreach (JointType joint in jointsList)
        {
            GameObject newJoint = Instantiate(JointObject);
            newJoint.name = joint.ToString();
            newJoint.transform.parent = body.transform;
        }
        return body;
    }
    private void UpdateBodyObject(Body body, GameObject bodyObject)
    {
        foreach (JointType joint in jointsList)
        {
            JointKinect sourceJoint = body.Joints[joint];
            Vector3 targetPosition = GetVector3FromJoint(sourceJoint);
            targetPosition.z = 0;
            Transform jointObject = bodyObject.transform.Find(joint.ToString());
            jointObject.position = targetPosition;
        }
    }
    private Vector3 GetVector3FromJoint(JointKinect joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
