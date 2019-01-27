using UnityEngine;
using System.Collections;

public class HoldableJoint : MonoBehaviour
{
    public Joint joint;
    [SerializeField] private bool lockPosition;
    [SerializeField] private float maxDistanceDelta = .1f;
    [SerializeField] private bool lockRotation;
    [SerializeField] private float maxDegreesDelta = 1f;
    
    private Holdable holdable;
    private bool connected;

    void LateUpdate()
    {
        if (lockRotation)
        {
            holdable.transform.rotation =  Quaternion.RotateTowards(
                holdable.transform.rotation, 
                transform.rotation, 
                maxDegreesDelta);
        }

        if (lockPosition)
        {
            holdable.transform.position = Vector3.MoveTowards(
                holdable.transform.position, 
                transform.position - holdable.transform.rotation * holdable.LocalAnchor, 
                maxDistanceDelta);
        }
    }

    void OnJointBreak(float breakForce)
    {
        holdable.OnHoldableJointBreak(breakForce);
        Break();
    }

    public void Break()
    {
        Destroy(this.gameObject);
    }

    public void Connect(Holdable holdable)
    {
        this.holdable = holdable;
        transform.localPosition = Vector3.zero;
        
        joint.connectedBody = holdable.GetComponent<Rigidbody>();
        joint.connectedAnchor = holdable.LocalAnchor;

        if (isActiveAndEnabled)
        {
            StartCoroutine(Refresh());
        }
    }
    
    //Hack for joint not updating when changed via script.
    private IEnumerator Refresh()
    {
        yield return new WaitForEndOfFrame();
        
        //Before attaching to the holdable you have to reset it's position.
        holdable.transform.rotation = transform.rotation;
        holdable.transform.position = transform.position - holdable.transform.rotation * holdable.LocalAnchor;

        bool active = gameObject.activeInHierarchy;

        gameObject.SetActive(!active);
        gameObject.SetActive(active);
    }
}
