using UnityEngine;

public class CameraController : MonoBehaviour
{
    private int _id;
    public string shaderVariableName = "_DistanceToCamera";
    private Vector3 _distanceObjectToCamera;

    public void Start()
    {
        _id = Shader.PropertyToID(shaderVariableName);
    }

    public void Update()
    {
        _distanceObjectToCamera = Camera.main.transform.position - this.transform.position;
        Shader.SetGlobalFloat(_id, _distanceObjectToCamera.magnitude);
    }
}
