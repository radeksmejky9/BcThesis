using Unity.Mathematics;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [SerializeField]
    private Transform assets;
    private Transform cam;
    private Transform model;
    private GameObject anchor;

    [SerializeField]
    private GameObject[] objectPrefabs;

    private GameObject currentObject;

    void Start()
    {
        model = assets.parent;
        cam = Camera.main.transform;
    }

    public void AlignModel()
    {
        if (anchor != null && assets.parent != null)
        {
            assets.parent = model;
            Destroy(anchor);
        }

        anchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        anchor.GetComponent<Renderer>().enabled = false;
        anchor.transform.position = cam.position;
        anchor.transform.parent = model;

        assets.parent = anchor.transform;
        Quaternion objectTransformRotation = Quaternion.Euler(0, assets.rotation.y, 0);
        Quaternion anchorY = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);

        assets.SetPositionAndRotation(new Vector3(
                cam.position.x + model.position.x,
                cam.position.y + model.position.y,
                cam.position.z + model.position.z
                ), new quaternion(0, 0, 0, 0));
        anchor.transform.rotation = objectTransformRotation * anchorY;
    }

    public void CreateObject(int objectIndex)
    {
        model.position = Vector3.zero;
        assets.position = Vector3.zero;
        if (currentObject != null)
            Destroy(currentObject);
        var newObject = Instantiate(objectPrefabs[objectIndex], assets);
        currentObject = newObject;
        AlignModel();
    }
}
