using UnityEngine;
using System.Collections;
using Valve.VR;
using System;

public class Pointer : MonoBehaviour
{
    SteamVR_TrackedObject trackedObj;
    public bool connected;
    public int index;

    public GameObject HoldingPenguin = null;
    public GameObject OverPenguin = null;

    public event EventHandler<PenguinSelectArgs> PenguinSelected;
    public event EventHandler<PenguinSelectArgs> PenguinReleased;

    public Transform PenguinParent;

    void Awake()
    {
        trackedObj = transform.GetComponentInParent<SteamVR_TrackedObject>();
    }

    void FixedUpdate()
    {
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
            OnTriggerPressed();
        if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
            OnTriggerReleased();
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Grip))
            GameManager.Instance.ResetPenguinPositions();
    }
    /*
    void OnTriggerEnter(Collider col)
    {
        if (!IsOverPenguin() && col.CompareTag("Penguin"))
            OverPenguin = col.gameObject;
    }

    void OnTriggerExit(Collider col)
    {
        if (!IsOverPenguin())
            return;

        if (col.gameObject.name == OverPenguin.name)
        {
            OverPenguin = null;
            checkForPenguinInRange();
        }      

    }
    */
    void OnTriggerPressed()
    {   
        //if (IsOverPenguin())
        //{
            HoldingPenguin = getClosestPenguinInRange();
            if (HoldingPenguin != null)
            {
                //OverPenguin = null;
                GetComponent<MeshRenderer>().enabled = false;
                HoldingPenguin.transform.SetParent(transform.parent);
                HoldingPenguin.GetComponent<Rigidbody>().isKinematic = true;

                if (PenguinSelected != null)
                    PenguinSelected.Invoke(this, new PenguinSelectArgs(HoldingPenguin));
            }

        //}

    }

    void OnTriggerReleased()
    {
        GetComponent<MeshRenderer>().enabled = true;

        if (IsHoldingPenguin())
        {
            HoldingPenguin.transform.SetParent(PenguinParent);
            HoldingPenguin.GetComponent<Rigidbody>().isKinematic = false;

            if (PenguinReleased != null)
                PenguinReleased.Invoke(this, new PenguinSelectArgs(HoldingPenguin));

            HoldingPenguin = null;
            checkForPenguinInRange();
        }
     
    }

    private void checkForPenguinInRange()
    {
        foreach (Collider newCol in Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius))
        {
            if (newCol.CompareTag("Penguin") && (!IsOverPenguin() ||newCol.gameObject.name != OverPenguin.name) )
            {
                OverPenguin = newCol.gameObject;
            }
        }
    }

    private GameObject getClosestPenguinInRange()
    {
        float closestDistance = 100.0f;
        GameObject closestPenguin = null;
        foreach (Collider col in Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius))
        {
            if (col.CompareTag("Penguin"))
            {
                float distance = Mathf.Abs(Vector3.Distance(transform.position, col.transform.position));
                if (distance < closestDistance)
                {
                    closestPenguin = col.gameObject;
                    closestDistance = distance;
                }
            }
            
        }
        return closestPenguin;
    }

    public bool IsOverPenguin()
    {
        return OverPenguin != null;
    }

    public bool IsHoldingPenguin()
    {
        return HoldingPenguin != null;
    }

    public class PenguinSelectArgs : EventArgs
    {
        private readonly GameObject _penguinSelected;

        public PenguinSelectArgs(GameObject penguinSelected)
        {
            _penguinSelected = penguinSelected;
        }

        public GameObject selectedPenguin
        {
            get { return _penguinSelected; }
        }
    }
}
