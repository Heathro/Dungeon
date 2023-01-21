using UnityEngine;

namespace Saving
{
    [System.Serializable]
    public class SerializableTransform
    {
        // STATE

        float xPosition, yPosition, zPosition;
        float xRotation, yRotation, zRotation, wRotation;

        // CONSTRUCT

        public SerializableTransform(Transform form)
        {
            xPosition = form.position.x;
            yPosition = form.position.y;
            zPosition = form.position.z;
            
            xRotation = form.rotation.x;
            yRotation = form.rotation.y;
            zRotation = form.rotation.z;
            wRotation = form.rotation.w;
        }

        // PUBLIC

        public Vector3 GetPosition()
        {
            return new Vector3(xPosition, yPosition, zPosition);
        }

        public Quaternion GetRotation()
        {
            return new Quaternion(xRotation, yRotation, zRotation, wRotation);
        }
    }
}