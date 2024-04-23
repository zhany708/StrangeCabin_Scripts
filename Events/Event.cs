using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Event : MonoBehaviour
{
    public SO_EventData EventData;


    protected EventManager eventManager;
    protected DoorController doorController;

    



    public abstract void StartEvent();

    public void FinishEvent()
    {
        eventManager.IncrementEventCount();     //增加触发过的事件计数

        if (doorController != null)
        {
            doorController.OpenDoors();
        }
    }



    #region Setters
    public void SetEventManager(EventManager manager)
    {
        eventManager = manager;
    }

    public void SetDoor(DoorController thisdoorController)
    {
        doorController = thisdoorController;
    }
    #endregion
}
