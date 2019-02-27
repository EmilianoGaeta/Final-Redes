using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

public class SyncTransform : NetworkBehaviour
{
    [Header("Syncs")]
    [SyncVar(hook = "SyncPositionValues")]
    [SerializeField]
    private Vector3 syncPos;
    [SyncVar(hook = "SyncRotationValues")]
    [SerializeField]
    private Quaternion syncRot;
    [SyncVar(hook = "SyncScaleValues")]
    [SerializeField]
    private Vector3 syncScale;

    [Space(5)]
    [Header("Restrictions")]
    [SerializeField]
    private bool allowSyncPos;
    [SerializeField]
    private bool allowSyncRot;
    [SerializeField]
    private bool allowSyncScale;

    [Space(5)]
    [Header("Propertys")]
    [SerializeField]
    private float lerpRate;
    [SerializeField]
    private float normalLerpRate;
    [SerializeField]
    private float fasterLerpRate;

    [Space(5)]
    [Header("Latency")]
    private List<Vector3> syncPosList;
    private List<Quaternion> syncRotList;
    private List<Vector3> syncScaleList;
    [SerializeField] private bool useHistoricalLerping = false;
    [SerializeField] private float closeEnough = 0.11f;

    void Start()
    {
        syncPosList = new List<Vector3>();
        syncRotList = new List<Quaternion>();
        syncScaleList = new List<Vector3>();

        lerpRate = 8f;
        normalLerpRate = 6f;
        fasterLerpRate = 8f;

        syncPos = transform.position;
        syncRot = transform.rotation;
        syncScale = transform.localScale;
    }

    void Update()
    {
        LerpPosition();
    }

    void LerpPosition()
    {
        if (isServer)
        {
            if(allowSyncPos)
                syncPos = transform.position;
            if (allowSyncRot)
                syncRot = transform.rotation;
            if (allowSyncScale)
                syncScale = transform.localScale;
        }
        else
        {
            if (useHistoricalLerping)
                 HistoricalLerping();          
            else
                OrdinaryLerping();         
        }
    }

    void OrdinaryLerping()
    {
        if (allowSyncPos)
            transform.position = Vector3.Lerp(transform.position, syncPos, Time.deltaTime * lerpRate);
        if (allowSyncRot)
            transform.rotation = Quaternion.Slerp(transform.rotation, syncRot, Time.deltaTime * 15);
        if (allowSyncScale)
            transform.localScale = Vector3.Lerp(transform.localScale, syncScale, Time.deltaTime * lerpRate);
    }

    void HistoricalLerping()
    {
        PositionHistoricalLerping();
        RotationHistoricalLerping();
        ScaleHistoricalLerping();
    }

    void PositionHistoricalLerping()
    {
        if (syncPosList.Count > 0 && allowSyncPos)
        {
            transform.position = Vector3.Lerp(transform.position, syncPosList[0], Time.deltaTime * lerpRate);
         
            if (Vector3.Distance(transform.position, syncPosList[0]) < closeEnough)
            {
                syncPosList.RemoveAt(0);
            }

            if (syncPosList.Count > 10)
            {
                lerpRate = fasterLerpRate;
            }
            else
            {
                lerpRate = normalLerpRate;
            }
        }
    }

    void RotationHistoricalLerping()
    {
        if (syncRotList.Count > 0 && allowSyncRot)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, syncRotList[0], Time.deltaTime * lerpRate);

            if (Vector3.Distance(transform.rotation.eulerAngles, syncRotList[0].eulerAngles) < closeEnough)
            {
                syncRotList.RemoveAt(0);
            }

            if (syncRotList.Count > 10)
            {
                lerpRate = fasterLerpRate;
            }
            else
            {
                lerpRate = normalLerpRate;
            }
        }
    }

    void ScaleHistoricalLerping()
    {
        if (syncScaleList.Count > 0 && allowSyncScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, syncScaleList[0], Time.deltaTime * lerpRate);

            if (Vector3.Distance(transform.localScale, syncScaleList[0]) < closeEnough)
            {
                syncScaleList.RemoveAt(0);
            }

            if (syncScaleList.Count > 10)
            {
                lerpRate = fasterLerpRate;
            }
            else
            {
                lerpRate = normalLerpRate;
            }
        }
    }

    [Client]
    void SyncPositionValues(Vector3 latestPos)
    {
        syncPos = latestPos;
        syncPosList.Add(syncPos);
    }

    [Client]
    void SyncRotationValues(Quaternion latestPos)
    {
        syncRot = latestPos;
        syncRotList.Add(syncRot);
    }

    [Client]
    void SyncScaleValues(Vector3 latestPos)
    {
        syncScale = latestPos;
        syncScaleList.Add(syncScale);
    }
}
