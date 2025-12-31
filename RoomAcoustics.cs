using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
public class RoomAcoustics : MonoBehaviour
{
    public float wallRange; // meters from wall
    private BoxCollider currentRoom;
    [Header("NOTE that Snapshots will override if in scope")]
    [SerializeField] private string fmodParamGlobal;
    [SerializeField] private UnityEvent<float> fmodInstance;
    [Header("Param values - below 1 for non acoustic automation")]
    [SerializeField] private float fmodMin = 1f;
    [SerializeField] private float fmodMax = 2f;
    [Header("Sets the exit value for FMOD, set below the mapped range eg 0f")]
    [SerializeField] private float resetExitValue = 0f;
 

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Acoustic_Room"))
        {
            currentRoom = other as BoxCollider;
            Bounds b = currentRoom.bounds;
            
            Vector3 p = other.transform.lossyScale;
            float maxValueGlobal = Mathf.Max(p.x, p.y, p.z);

            Vector3 lp = transform.localScale;
            float maxValueLocal = Mathf.Max(lp.x, lp.y, lp.z);
            wallRange =  maxValueGlobal /4f;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (currentRoom && other.gameObject == currentRoom.gameObject)
        {
            currentRoom = null;
            SetFmodRaw(resetExitValue); //sets the exit value, below the mapped range
        }
    }

    void Update()
    {
        if (currentRoom == null)
            return;


        Bounds b = currentRoom.bounds;
        Vector3 p = transform.position;

        // Distance to nearest wall on each axis
        float dx = Mathf.Min(p.x - b.min.x, b.max.x - p.x);
        float dz = Mathf.Min(p.z - b.min.z, b.max.z - p.z);

        // Wall normalisation
        float proxX = 1f - Mathf.Clamp01(dx / wallRange);
        float proxZ = 1f - Mathf.Clamp01(dz / wallRange);

        // Nearest wall takes precedence
        float wallProximity = Mathf.Max(proxX, proxZ);
            
        SetWallProximity(wallProximity);
    }

    //mapping of value to 1 -2 
    float lastFmodValue = float.MinValue;
    const float UPDATE_EPSILON = 0.01f;

    void SetWallProximity(float value)
    {
        float fmodValue = Mathf.Lerp(fmodMin, fmodMax, value);

        if (Mathf.Abs(fmodValue - lastFmodValue) < UPDATE_EPSILON)
            return;

        lastFmodValue = fmodValue;

        RuntimeManager.StudioSystem.setParameterByName(fmodParamGlobal, fmodValue);
        fmodInstance.Invoke(fmodValue);
    }
    
    //resets to zero - no mapping
    void SetFmodRaw(float value)
    {
        RuntimeManager.StudioSystem.setParameterByName(fmodParamGlobal, value);
        fmodInstance.Invoke(value);
    }
}