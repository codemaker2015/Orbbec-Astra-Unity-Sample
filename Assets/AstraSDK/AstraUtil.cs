using UnityEngine;

public static class AstraUtil
{
    // astra vectors are in mm, convert to a good unity scale as this is too big
    public static Vector3 AstraVector3dToUnity(Astra.Vector3D astraVector, float scale = 0.01f)
        => new Vector3(
                        astraVector.X,
                        astraVector.Y,
                        astraVector.Z) * 0.01f;

    public static Vector2 AstraVector2dToUnity(Astra.Vector2D astraVector, float scale = 0.01f)
        => new Vector2(
                        astraVector.X,
                        astraVector.Y) * 0.01f;
    public static bool IsJointOk(Astra.Joint joint)
    => joint != null && joint.Status != Astra.JointStatus.NotTracked;

    public static bool IsBodyOk(Astra.Body body)
        => body != null && body.Status != Astra.BodyStatus.NotTracking;

}
