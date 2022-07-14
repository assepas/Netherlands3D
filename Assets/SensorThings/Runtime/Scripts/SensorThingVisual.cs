using Netherlands3D.Core;
using Netherlands3D.SensorThings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorThingVisual : MonoBehaviour
{
    private SensorThingsRIVM sensorThingsRIVM;
    private Things.Value thingData;

    private void OnEnable()
    {
        //Get latest data
    }

    public void SetData(SensorThingsRIVM sensorThingsRIVM, Things.Value thingData)
    {
        this.sensorThingsRIVM = sensorThingsRIVM;
        this.thingData = thingData;

        this.name = thingData.name;

        sensorThingsRIVM.GetLocations(GotLocation, thingData.iotid);
        sensorThingsRIVM.GetDatastreams(GotDatastreams, thingData.iotid);
    }

    /// <summary>
    /// Received the datastreams.
    /// For now we simple create gameobjects for the streams to inspect them in the editor
    /// </summary>
    private void GotDatastreams(bool success, Datastreams datastreams)
    {
        if (!success) return;
        for (int i = 0; i < datastreams.value.Length; i++)
        {
            var datastream  = datastreams.value[i];

            var datastreamVisual = new GameObject();
            datastreamVisual.transform.SetParent(this.transform);
            datastreamVisual.name = datastream.name + " - " + datastream.unitOfMeasurement.symbol;
        }
    }

    /// <summary>
    /// Received the location of the thing.
    /// It can contain multiple locations so we loop though them, but this should usualy only be one.
    /// </summary>
    private void GotLocation(bool success, Locations locations)
    {
        if (!success) return;
        for (int i = 0; i < locations.value.Length; i++)
        {
            var location = locations.value[i].location.coordinates;
            var unityCoordinate = CoordConvert.WGS84toUnity(location[0], location[1]);

            this.transform.position = unityCoordinate;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(this.transform.position, this.transform.position + Vector3.up * 500);
    }
}
