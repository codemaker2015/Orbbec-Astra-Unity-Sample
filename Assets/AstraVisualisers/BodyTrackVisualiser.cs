using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTrackVisualiser : MonoBehaviour
{
    private List<Dictionary<Astra.JointType, GameObject>> _bodyObjs = 
        new List<Dictionary<Astra.JointType, GameObject>>();
    private void Start()
    {
        // subscribe to new body data events
        AstraController.Instance.OnBodyTrackEvent += OnBodyTrack;

        var jointTypes = Enum.GetValues(typeof(Astra.JointType));
        for (int i = 0; i < AstraController.MAX_BODIES; i++)
        {
            var bodyObj = new Dictionary<Astra.JointType, GameObject>();
            foreach (Astra.JointType jointType in jointTypes)
            {
                if (jointType == Astra.JointType.Unknown) { continue; }
                var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bodyObj.Add(jointType, obj);
            }
            _bodyObjs.Add(bodyObj);
        }
    }

    private void OnBodyTrack(Astra.Body[] bodies)
    {
        if (bodies == null)
        {
            return;
        }
        int bodyCount = 0;
        foreach (var body in bodies)
        {
            var bodyObj = _bodyObjs[bodyCount];
            bodyCount++;

            if (!AstraUtil.IsBodyOk(body))
            {
                ToggleBodyVisibility(bodyObj, false);
                continue;
            }

            foreach (var joint in body.Joints)
            {
                if (AstraUtil.IsJointOk(joint))
                {
                    var jointObj = bodyObj[joint.Type];
                    ToggleObjVisibility(jointObj, true);
                    jointObj.transform.position = AstraUtil.AstraVector3dToUnity(joint.WorldPosition);
                }
            }
        }
    }

    private void Update()
    {
    }

    private void OnDisable()
    {
        AstraController.Instance.OnBodyTrackEvent -= OnBodyTrack;
    }

    private void ToggleBodyVisibility(Dictionary<Astra.JointType, GameObject> body, bool isVisible)
    {
        foreach(var jointObj in body)
        {
            GameObject obj = jointObj.Value;
            ToggleObjVisibility(obj, isVisible);
        }
    }

    private void ToggleObjVisibility(GameObject gameObject, bool isVisible)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        renderer.enabled = isVisible;
    }
}
