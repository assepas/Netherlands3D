using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;

namespace Netherlands3D.SensorThings
{
    public class SensorThingsRIVM : MonoBehaviour
    {
        [SerializeField]
        private string baseApiURL = "https://api-samenmeten.rivm.nl/v1.0";

        [Tooltip("Filter by municipality")]
        private string municipalityThingsFilter = "?$filter=contains(properties/codegemeente,'municipalityID')";

        void Start()
        {
            CheckAvailabilityAPI();
        }

        /// <summary>
        /// Call root API to check if it is online
        /// </summary>
        private void CheckAvailabilityAPI()
        {
            StartCoroutine(RequestAPI(baseApiURL,(success,message) => { Debug.Log($"API Available:{success}", this.gameObject); } ));
        }

        /// <summary>
        /// Get all locations, or by thing ID
        /// </summary>
        /// <param name="callback">Returns Locations object</param>
        /// <param name="thingID">Optional ID of the Thing to request specific location</param>
        public void GetLocations(Action<bool, Locations> callback, int thingID = 0)
        {
            var specificThing = (thingID > 0) ? $"/Things({thingID})/" : "/";

            StartCoroutine(RequestAPI($"{baseApiURL}{specificThing}Locations", (success, text) => {
                if (success)
                {
                    Locations things = JSONToLocations(text);
                    callback(true, things);
                }
                else
                {
                    callback(false, null);
                }
            }));
        }

        public void GetDatastreams(Action<bool, Datastreams> callback, int thingID = 0)
        {
            var specificThing = (thingID > 0) ? $"/Things({thingID})/" : "/";
            StartCoroutine(RequestAPI($"{baseApiURL}{specificThing}Datastreams", (success, text) => {
                if (success)
                {
                    Datastreams datastreams = JSONToDatastreams(text);
                    callback(true, datastreams);
                }
                else
                {
                    callback(false, null);
                }
            }));
        }

        /// <summary>
        /// Get all things from API with optional filters
        /// </summary>
        /// <param name="callback">Returns object if data is retrieved</param>
        public void GetThings(Action<bool,Things> callback, int municipalityID = 0)
        {
            var filter = municipalityThingsFilter = municipalityThingsFilter.Replace("municipalityID", municipalityID.ToString());
            StartCoroutine(RequestAPI($"{baseApiURL}/Things{municipalityThingsFilter}", (success,text) =>
            {
                if(success)
                {
                    Things things = JSONToThings(text);
                    callback(true, things);
                }
                else
                {
                    callback(false,null);
                }
            }));
        }
        private static Things JSONToThings(string text)
        {
            var json = JSON.Parse(text);
            var things = new Things();
            things.iotnextLink = json["@iot.nextLink"];

            var valuesJson = json["value"].AsArray;
            Debug.Log(valuesJson.Count);
            var valuesObjects = new List<Things.Value>();
            for (int i = 0; i < valuesJson.Count; i++)
            {

                var jsonValue = valuesJson[i];
                var jsonValueProperties = jsonValue["properties"];
                valuesObjects.Add(new Things.Value
                {
                    iotid = jsonValue["@iot.id"],
                    iotselfLink = jsonValue["@iot.selfLink"],
                    name = jsonValue["name"],
                    description = jsonValue["description"],
                    properties = new Things.Properties()
                    {
                        codegemeente = jsonValueProperties["codegemeente"],
                        knmicode = jsonValueProperties["knmicode"],
                        nh3closecode = jsonValueProperties["nh3closecode"],
                        nh3regiocode = jsonValueProperties["nh3regiocode"],
                        nh3stadcode = jsonValueProperties["nh3stadcode"],
                        no2closecode = jsonValueProperties["no2closecode"],
                        no2regiocode = jsonValueProperties["no2regiocode"],
                        no2stadcode = jsonValueProperties["no2stadcode"],
                        owner = jsonValueProperties["owner"],
                        pm10closecode = jsonValueProperties["pm10closecode"],
                        pm10regiocode = jsonValueProperties["pm10regiocode"],
                        pm10stadcode = jsonValueProperties["pm10stadcode"],
                        pm25closecode = jsonValueProperties["pm25closecode"],
                        pm25regiocode = jsonValueProperties["pm25regiocode"],
                        pm25stadcode = jsonValueProperties["pm25stadcode"],
                        project = jsonValueProperties["project"]
                    },
                    LocationsiotnavigationLink = "Locations@iot.navigationLink",
                    DatastreamsiotnavigationLink = "Datastreams@iot.navigationLink",
                    HistoricalLocationsiotnavigationLink = "HistoricalLocations@iot.navigationLink"
                });
            }
            things.value = valuesObjects.ToArray();
            return things;
        }

        private static Locations JSONToLocations(string text)
        {
            var json = JSON.Parse(text);
            var locations = new Locations();
            locations.iotnextLink = json["@iot.nextLink"];

            var valuesJson = json["value"].AsArray;
            Debug.Log(valuesJson.Count);
            var valuesObjects = new List<Locations.Value>();
            for (int i = 0; i < valuesJson.Count; i++)
            {
                var jsonValue = valuesJson[i];
                var jsonValueLocation = jsonValue["location"];
                valuesObjects.Add(new Locations.Value
                {
                    iotid = jsonValue["@iot.id"],
                    iotselfLink = jsonValue["@iot.selfLink"],
                    name = jsonValue["name"],
                    description = jsonValue["description"],
                    location = new Locations.Location()
                    {
                        coordinates = new float[2] { jsonValueLocation["coordinates"][0].AsFloat, jsonValueLocation["coordinates"][1].AsFloat },
                        type = jsonValueLocation["type"]
                    },
                    HistoricalLocationsiotnavigationLink = "HistoricalLocations@iot.navigationLink"
                });
            }
            locations.value = valuesObjects.ToArray();
            return locations;
        }
        private static Datastreams JSONToDatastreams(string text)
        {
            var json = JSON.Parse(text);
            var datastreams = new Datastreams();
            datastreams.iotnextLink = json["@iot.nextLink"];

            var valuesJson = json["value"].AsArray;
            Debug.Log(valuesJson.Count);
            var valuesObjects = new List<Datastreams.Value>();
            for (int i = 0; i < valuesJson.Count; i++)
            {
                var jsonValue = valuesJson[i];
                var jsonValueUnityOfMeasurement = jsonValue["unitOfMeasurement"];
                valuesObjects.Add(new Datastreams.Value
                {
                    iotid = jsonValue["@iot.id"],
                    iotselfLink = jsonValue["@iot.selfLink"],
                    name = jsonValue["name"],
                    description = jsonValue["description"],
                    unitOfMeasurement = new Datastreams.Unitofmeasurement()
                    {
                        symbol = jsonValueUnityOfMeasurement["symbol"]
                    },
                    ObservationsiotnavigationLink = "Observations@iot.navigationLink"
                });
            }
            datastreams.value = valuesObjects.ToArray();
            return datastreams;
        }

        public void GetDatastreams()
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/Datastreams", (success, text) => { }));
        }
        public void Observations()
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/Observations", (success, text) => { }));
        }
        public void HistoricalLocations()
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/HistoricalLocations", (success, text) => { }));
        }
        public void ObservedProperties()
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/ObservedProperties", (success, text) => { }));
        }
        public void GetSensors()
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/Sensors", (success, text) => { }));
        }

        private IEnumerator RequestAPI(string uri, Action<bool,string> callback)
        {
            Debug.Log(uri);
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(webRequest.error);
                        callback(false, webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(webRequest.error);
                        callback(false, webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        callback(true,webRequest.downloadHandler.text);
                        break;
                }
            }
        }

        public void StopRequests()
        {
            StopAllCoroutines();
        }
    }
}

