using Netherlands3D.Core;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;
using Netherlands3D.Events;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// Class to represent and parse a CityJSON. Currently based on CityJSON v1.0.3
    /// https://www.cityjson.org/specs/1.0.3/
    /// </summary>
    public class CityJSON : MonoBehaviour
    {
        //nodes defined in a CityJSON and therefore reserved node keys.
        private static string[] definedNodes = { "type", "version", "CityObjects", "vertices", "extensions", "metadata", "transform", "appearance", "geometry-templates" };

        public string Version { get; private set; }
        public JSONNode Extensions { get; private set; }
        public JSONNode Metadata { get; private set; }
        public Vector3Double TransformScale { get; private set; } = new Vector3Double(1d, 1d, 1d);
        public Vector3Double TransformTranslate { get; private set; } = new Vector3Double(0d, 0d, 0d);
        public JSONNode Appearance { get; private set; }
        public JSONNode GeometryTemplates { get; private set; }

        public CityObject[] CityObjects { get; private set; } = new CityObject[0];
        public Vector3Double MinExtent { get; private set; }
        public Vector3Double MaxExtent { get; private set; }
        public CoordinateSystem CoordinateSystem { get; private set; }

        [Tooltip("event that provides a CityJSON to this class.")]
        [SerializeField]
        private StringEvent onCityJSONReceived;
        [Tooltip("A cityObject will be created as a GameObject with a CityObject script. This field can hold a prefab with multiple extra scripts (such as CityObjectVisualizer) to be created instead. This prefab must have a CityObject script attached.")]
        [SerializeField]
        private GameObject cityObjectPrefab; 
        [SerializeField]
        [Tooltip("If checked the CityJSON parsed by this script will set the relative center of the RD coordinate system to the center of this CityJSON")]
        private bool useAsRelativeRDCenter;
        [Tooltip("event that is called when the CityJSON is parsed")]
        [SerializeField]
        private TriggerEvent onAllCityObjectsProcessed;

        private void OnEnable()
        {
            onCityJSONReceived.started.AddListener(ParseCityJSON);
        }

        private void OnDisable()
        {
            onCityJSONReceived.started.RemoveAllListeners();
        }

        public void ParseCityJSON(string cityJson)
        {
            foreach (var co in CityObjects)
            {
                Destroy(co.gameObject);
            }

            var node = JSONNode.Parse(cityJson);
            var type = node["type"];
            Assert.IsTrue(type == "CityJSON");
            Version = node["version"];

            //optional data
            Extensions = node["extensions"];
            Metadata = node["metadata"];
            Appearance = node["appearance"]; //todo: not implemented yet
            GeometryTemplates = node["geometry-templates"]; //todo: not implemented yet
            var transformNode = node["transform"];

            if (transformNode.Count > 0)
            {
                TransformScale = new Vector3Double(transformNode["scale"][0], transformNode["scale"][1], transformNode["scale"][2]);
                TransformTranslate = new Vector3Double(transformNode["translate"][0], transformNode["translate"][1], transformNode["translate"][2]);
            }

            //custom nodes must be added as is to the exporter to ensure they are preserved.
            AddExtensionNodesToExporter(node);

            //vertices
            List<Vector3Double> parsedVertices = new List<Vector3Double>();
            foreach (var vertArray in node["vertices"])
            {
                var vert = new Vector3Double(vertArray.Value.AsArray);
                vert *= TransformScale;
                vert += TransformTranslate;
                parsedVertices.Add(vert);
            }
            if(parsedVertices.Count == 0)
            {
                Debug.LogWarning("Vertex list is empty, nothing can be visualized because empty meshes will be created!");
            }

            //metadata
            if (Metadata.Count > 0)
            {
                var coordinateSystemNode = Metadata["referenceSystem"];
                CoordinateSystem = ParseCoordinateSystem(coordinateSystemNode);

                var geographicalExtent = Metadata["geographicalExtent"];
                if (geographicalExtent.Count > 0)
                {
                    MinExtent = new Vector3Double(geographicalExtent[0].AsDouble, geographicalExtent[1].AsDouble, geographicalExtent[2].AsDouble);
                    MaxExtent = new Vector3Double(geographicalExtent[3].AsDouble, geographicalExtent[4].AsDouble, geographicalExtent[5].AsDouble);
                }
                else if (parsedVertices.Count > 0)
                {
                    var minX = parsedVertices.Min(v => v.x);
                    var minY = parsedVertices.Min(v => v.y);
                    var minZ = parsedVertices.Min(v => v.z);
                    var maxX = parsedVertices.Max(v => v.x);
                    var maxY = parsedVertices.Max(v => v.y);
                    var maxZ = parsedVertices.Max(v => v.z);

                    MinExtent = new Vector3Double(minX, minY, minZ);
                    MaxExtent = new Vector3Double(maxX, maxY, maxZ);
                }
            }

            //CityObjects
            Dictionary<JSONNode, CityObject> cityObjects = new Dictionary<JSONNode, CityObject>();
            foreach (var cityObjectNode in node["CityObjects"])
            {
                GameObject go;
                CityObject co;
                if (cityObjectPrefab == null)
                {
                    go = new GameObject(cityObjectNode.Key);
                    go.transform.SetParent(transform);
                    co = go.AddComponent<CityObject>();
                }
                else
                {
                    go = Instantiate(cityObjectPrefab, transform);
                    go.name = cityObjectNode.Key;
                    co = go.GetComponent<CityObject>();
                }
                co.FromJSONNode(cityObjectNode.Key, cityObjectNode.Value, CoordinateSystem, parsedVertices);
                cityObjects.Add(cityObjectNode.Value, co);
            }

            //after creating all the objects, set the parent/child relations. this can only be done after, since the referenced parent/child might not exist yet during initialization in the previous loop
            foreach (var co in cityObjects)
            {
                var parents = co.Key["parents"];
                var parentObjects = new CityObject[parents.Count];
                for (int i = 0; i < parents.Count; i++)
                {
                    string parentId = parents[i];
                    parentObjects[i] = cityObjects.First(co => co.Value.Id == parentId).Value;
                }
                co.Value.SetParents(parentObjects);
            }

            CityObjects = cityObjects.Values.ToArray();

            if (useAsRelativeRDCenter)
                SetRelativeCenter();

            foreach(var co in CityObjects)
            {
                co.OnCityObjectParseCompleted();
            }

            onAllCityObjectsProcessed.Invoke();
        }

        //currently only RD and WGS84 are supported as coordinate systems.
        private static CoordinateSystem ParseCoordinateSystem(JSONNode coordinateSystemNode)
        {
            Debug.Log("Parsing coordinate system: " + coordinateSystemNode.Value);
            if (coordinateSystemNode.Value == "urn:ogc:def:crs:EPSG::28992")
                return CoordinateSystem.RD;
            if (coordinateSystemNode.Value == "urn:ogc:def:crs:EPSG::4979")
                return CoordinateSystem.WGS84;

            Debug.Log("Parsing coordinateSystem failed, using Unity Coordinate Sytem");
            return CoordinateSystem.Unity;
        }

        //custom nodes must be added as is to the exporter to ensure they are preserved.
        public static void AddExtensionNodesToExporter(JSONNode cityJsonNode)
        {
            foreach (var node in cityJsonNode)
            {
                if (definedNodes.Contains(node.Key))
                    continue;

                CityJSONFormatter.AddExtensionNode(node.Key, node.Value);
            }
        }

        // function to remove Extension nodes for use in future scripts that may need to do so
        public static void RemoveExtensionNodes(JSONNode cityJsonNode)
        {
            foreach (var node in cityJsonNode)
            {
                if (definedNodes.Contains(node.Key))
                    continue;

                CityJSONFormatter.RemoveExtensionNode(node.Key);
            }
        }

        // set the relative RD center to avoid floating point issues of GameObject far from the Unity origin
        private void SetRelativeCenter()
        {
            if (CoordinateSystem == CoordinateSystem.RD)
            {
                var relativeCenterRD = (MinExtent + MaxExtent) / 2;
                Debug.Log("Setting Relative RD Center to: " + relativeCenterRD);
                CoordConvert.zeroGroundLevelY = 0;// (float)relativeCenterRD.z;
                CoordConvert.relativeCenterRD = new Vector2RD(relativeCenterRD.x, relativeCenterRD.y);
            }
        }
    }
}
