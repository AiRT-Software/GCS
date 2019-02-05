using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml; //Needed for XML functionality
using System.Xml.Serialization; //Needed for XML Functionality


public class ColladaImporter {
    Dictionary<string, GameObject> listGameobjects = new Dictionary<string, GameObject>();
    Dictionary<string, Material> listMaterials = new Dictionary<string, Material>();
    GameObject final;

    Vector3 minBoundingBox, maxBoundingBox;

    Bounds modelBounds;
    //This is the parent of all the objects that are imported, and that will be modified on alignment
    public ColladaImporter(ref GameObject substitute)
    {
        final = substitute;
    }
    /// <summary>
    /// Checks the transformations inside the collada file and applies it to the object and its children
    /// </summary>
    /// <param name="item"></param>
    /// <param name="rotate"></param>
    /// <returns></returns>
    GameObject addToDict(XmlNode item, bool rotate)
    {
        //Recursive function ,gets the matrix, checks if the object has any child, gets the child object, applies the child's matrix to the child and then applies the matrix to the
        var groups = item.ChildNodes[1];
        string value = "";
        string matrixstr;
        //If its a group or not
        if (groups.Attributes.Count == 0)
        {
            matrixstr = item.ChildNodes[0].InnerText;
        }
        else { 
            value = groups.Attributes[0].Value;
            matrixstr = item.ChildNodes[0].InnerText;
        }
        string[] matrixelements = matrixstr.Split(' ');

        if (matrixelements.Length < 16)
        {
            return null;
        }
        Matrix4x4 aux;
        if (rotate)
        {
            //Blender
            aux = new Matrix4x4(new Vector4(float.Parse(matrixelements[0]), float.Parse(matrixelements[1]), float.Parse(matrixelements[2]), float.Parse(matrixelements[3])), new Vector4(float.Parse(matrixelements[4]), float.Parse(matrixelements[5]), float.Parse(matrixelements[6]), float.Parse(matrixelements[7])), new Vector4(float.Parse(matrixelements[8]), float.Parse(matrixelements[9]), float.Parse(matrixelements[10]), float.Parse(matrixelements[11])), new Vector4(float.Parse(matrixelements[12]), float.Parse(matrixelements[13]), float.Parse(matrixelements[14]), float.Parse(matrixelements[15])));

        }
        else
        {
            //Everywhere else
            aux = new Matrix4x4(new Vector4(float.Parse(matrixelements[0]), float.Parse(matrixelements[1]), float.Parse(matrixelements[2]), -float.Parse(matrixelements[3])), new Vector4(float.Parse(matrixelements[4]), float.Parse(matrixelements[5]), float.Parse(matrixelements[6]), float.Parse(matrixelements[7])), new Vector4(float.Parse(matrixelements[8]), float.Parse(matrixelements[9]), float.Parse(matrixelements[10]), float.Parse(matrixelements[11])), new Vector4(float.Parse(matrixelements[12]), float.Parse(matrixelements[13]), float.Parse(matrixelements[14]), float.Parse(matrixelements[15])));

        }
        aux = aux.transpose;
        GameObject gb;
        if (value != "" && listGameobjects.ContainsKey(value))
        {
            gb = listGameobjects[value];
            //For every child of the object
            if (item.ChildNodes.Count > 2)
            {
                for (int j = 2; j < item.ChildNodes.Count; j++)
                {
                    addToDict(item.ChildNodes[j], gb, rotate);
                }
            }
            gb.transform.localScale = aux.lossyScale;
            if (rotate)
            {
                Vector3 angles = gb.transform.eulerAngles;
                angles.x = -aux.rotation.eulerAngles.x;
                angles.y = -aux.rotation.eulerAngles.z;
                angles.z = aux.rotation.eulerAngles.y;
                gb.transform.eulerAngles = angles;

            }
            else
            {
                Vector3 angles = gb.transform.eulerAngles;
                angles.x = aux.rotation.eulerAngles.x;
                angles.y = -aux.rotation.eulerAngles.y;
                angles.z = aux.rotation.eulerAngles.z;
                gb.transform.eulerAngles = angles;
            }

            gb.transform.position = aux.MultiplyPoint3x4(gb.transform.position);
            if (rotate)
            {
                gb.transform.position = new Vector3(gb.transform.position.x, gb.transform.position.z, gb.transform.position.y);
            }
        }
        else
        {
            //This is for groups, which do not have a model so they aren't on the listgameobjects, but have a transform matrix
            gb = new GameObject();
            if (item.ChildNodes.Count > 2)
            {
                for (int j = 2; j < item.ChildNodes.Count; j++)
                {
                    addToDict(item.ChildNodes[j], gb, rotate);
                }
            }
            gb.transform.localScale = aux.lossyScale;
            if (rotate)
            {
                Vector3 angles = gb.transform.eulerAngles;
                angles.x = -aux.rotation.eulerAngles.x;
                angles.y = -aux.rotation.eulerAngles.z;
                angles.z = aux.rotation.eulerAngles.y;
                gb.transform.eulerAngles = angles;

            }
            else
            {
                Vector3 angles = gb.transform.eulerAngles;
                angles.x = aux.rotation.eulerAngles.x;
                angles.y = -aux.rotation.eulerAngles.y;
                angles.z = aux.rotation.eulerAngles.z;
                gb.transform.eulerAngles = angles;
            }

            gb.transform.position = aux.MultiplyPoint3x4(gb.transform.position);
            if (rotate)
            {
                gb.transform.position = new Vector3(gb.transform.position.x, gb.transform.position.z, gb.transform.position.y);
            }
        }
        return gb;
    }
    /// <summary>
    /// This function is for the children of the first object, does the same as above
    /// </summary>
    /// <param name="item"></param>
    /// <param name="parent"></param>
    /// <param name="rotate"></param>
    void addToDict(XmlNode item, GameObject parent, bool rotate)
    {
        if (item.ChildNodes[1] == null)
        {
            return;
        }
        var value = item.ChildNodes[1].Attributes[0].Value;
        string matrixstr = item.ChildNodes[0].InnerText;
        string[] matrixelements = matrixstr.Split(' ');
        Matrix4x4 aux;
        if (rotate)
        {
            aux = new Matrix4x4(new Vector4(float.Parse(matrixelements[0]), float.Parse(matrixelements[1]), float.Parse(matrixelements[2]), float.Parse(matrixelements[3])), new Vector4(float.Parse(matrixelements[4]), float.Parse(matrixelements[5]), float.Parse(matrixelements[6]), float.Parse(matrixelements[7])), new Vector4(float.Parse(matrixelements[8]), float.Parse(matrixelements[9]), float.Parse(matrixelements[10]), float.Parse(matrixelements[11])), new Vector4(float.Parse(matrixelements[12]), float.Parse(matrixelements[13]), float.Parse(matrixelements[14]), float.Parse(matrixelements[15])));

        }
        else
        {
            aux = new Matrix4x4(new Vector4(float.Parse(matrixelements[0]), float.Parse(matrixelements[1]), float.Parse(matrixelements[2]), -float.Parse(matrixelements[3])), new Vector4(float.Parse(matrixelements[4]), float.Parse(matrixelements[5]), float.Parse(matrixelements[6]), float.Parse(matrixelements[7])), new Vector4(float.Parse(matrixelements[8]), float.Parse(matrixelements[9]), float.Parse(matrixelements[10]), float.Parse(matrixelements[11])), new Vector4(float.Parse(matrixelements[12]), float.Parse(matrixelements[13]), float.Parse(matrixelements[14]), float.Parse(matrixelements[15])));

        }
        aux = aux.transpose;

        if (listGameobjects.ContainsKey(value))
        {

            GameObject gb = listGameobjects[value];
            gb.transform.parent = parent.transform;

            if (item.ChildNodes.Count > 2)
            {
                for (int j = 2; j < item.ChildNodes.Count; j++)
                {
                    addToDict(item.ChildNodes[j], gb, rotate);

                }
            }
            gb.transform.localScale = aux.lossyScale;
            if (rotate)
            {
                Vector3 angles = gb.transform.eulerAngles;
                angles.x = -aux.rotation.eulerAngles.x ;
                angles.y = -aux.rotation.eulerAngles.z;
                angles.z = aux.rotation.eulerAngles.y;
                gb.transform.eulerAngles = angles;

            }
            else
            {
                Vector3 angles = gb.transform.eulerAngles;
                angles.x = aux.rotation.eulerAngles.x;
                angles.y = -aux.rotation.eulerAngles.y;
                angles.z = aux.rotation.eulerAngles.z;
                gb.transform.eulerAngles = angles;
            }
            gb.transform.position = aux.MultiplyPoint3x4(gb.transform.position);
            if (rotate)
            {
                gb.transform.position = new Vector3(gb.transform.position.x, gb.transform.position.z, gb.transform.position.y);
            }
            //This creates the bounding box
            modelBounds.Encapsulate(gb.transform.position + Vector3.Cross(gb.GetComponent<MeshFilter>().mesh.bounds.extents, gb.transform.localScale));
            modelBounds.Encapsulate(gb.transform.position - Vector3.Cross(gb.GetComponent<MeshFilter>().mesh.bounds.extents, gb.transform.localScale));
        }


    }
    //main function which constructs the mesh
    public IEnumerator getMesh(XmlDocument xml, Shader sh)
    {
        minBoundingBox = Vector3.positiveInfinity;
        maxBoundingBox = Vector3.negativeInfinity;
        //Gameboject which will be instantiated and will contain each mesh
        GameObject mapModel = GameObject.Find("MapModel");

        Mesh finalMesh = new Mesh();
        //XmlNodeList collada = xml.GetElementsByTagName("COLLADA"); 
        XmlNodeList geometries = xml.GetElementsByTagName("geometry");
        XmlNode tools = xml.GetElementsByTagName("authoring_tool")[0];
        bool rotate = true;
        //Default collada has z axis as up, but the fbx collada exporter changes this. In blender rotate will be true
        if (tools != null && tools.InnerText.Equals("FBX COLLADA exporter"))
        {
             XmlNode axis = xml.GetElementsByTagName("up_axis")[0];
            if (axis != null && axis.InnerText.Equals("Y_UP"))
            {
                rotate = false;
            }
        }

        //We get the geometry here

        CombineInstance[] allMeshes = new CombineInstance[geometries.Count];
        int j = 0;
        foreach (XmlNode item in geometries)
        {
            string name = item.Attributes[0].Value;
            string vertexIndex = "";
            string normalIndex = "";
            string triangles = "";
            string UV = "";
            bool UVbool = false;
            int offset = 1;
            Mesh newMesh = new Mesh();
            XmlNodeList geometry = item.ChildNodes;
            foreach (XmlNode meshList in geometry)
            {
                XmlNodeList eachSource = meshList.ChildNodes;
                int i = 0;
                foreach (XmlNode sources in eachSource)
                {
                    if (i == 0)
                    {
                        vertexIndex = sources.InnerText;
                    }
                    else if (i == 1)
                    {
                        normalIndex = sources.InnerText;
                    }
                    //A file can have only vertex, normals and triangles
                    else if (eachSource.Count > 4 && i == 2)
                    {
                        UV = sources.InnerText;
                        UVbool = true;
                    }
                    else if (i == eachSource.Count-1)
                    {
                        triangles = sources.InnerText;
                        offset = sources.ChildNodes.Count - 1;
                    }
                    i++;
                }

            }
            /*
            if (normalIndex.StartsWith("\r\n"))
                normalIndex = normalIndex.Remove(0, 2);
            if (normalIndex.EndsWith("\r\n"))
                normalIndex = normalIndex.Remove(normalIndex.Length - 2, 2);

            string[] items = normalIndex.Replace("\r", " ").Replace("\n", "").Split(' ');
            Vector3[] normalAux = new Vector3[items.Length / 3];

            for (int i = 0; i < items.Length; i = i + 3)
            {
                normalAux[i / 3] = new Vector3(float.Parse(items[i]), float.Parse(items[i + 1]), float.Parse(items[i + 2]));
            }
            */
            //Now, after getting the text of the index, we apply them to a mesh
            if (vertexIndex.StartsWith("\r\n"))
                vertexIndex = vertexIndex.Remove(0, 2);
            if (vertexIndex.EndsWith("\r\n"))
                vertexIndex = vertexIndex.Remove(vertexIndex.Length - 2, 2);

            string[] items = vertexIndex.Replace("\r", " ").Replace("\n", "").Split(' ');
            Vector3[] vertexArray = new Vector3[items.Length / 3];
            Vector3[] normalArray = new Vector3[items.Length / 3];

            for (int i = 0; i < items.Length; i = i + 3)
            {
                //We fill a vertex array and a normal array
                if (rotate)
                {
                    vertexArray[i / 3] = new Vector3(float.Parse(items[i]), float.Parse(items[i + 2]), float.Parse(items[i + 1]));
                    normalArray[i / 3] = new Vector3(0, 0, 0);
                }
                else
                {
                    vertexArray[i / 3] = new Vector3(float.Parse(items[i]), float.Parse(items[i + 1]), float.Parse(items[i + 2]));
                    normalArray[i / 3] = new Vector3(0, 0, 0);
                }
            }

            Vector2[] UVArray = new Vector2[] { };
            //if there are UV we fill a UV array here
            if (UVbool)
            {

                if (UV.StartsWith("\r\n"))
                    UV = UV.Remove(0, 2);
                if (UV.EndsWith("\r\n"))
                    UV = UV.Remove(UV.Length - 2, 2);
                items = UV.Replace("\r", " ").Replace("\n", "").Split(' ');
                UVArray = new Vector2[items.Length / 2];

                for (int i = 0; i < items.Length; i = i + 2)
                {
                    UVArray[i / 2] = new Vector2(float.Parse(items[i]), float.Parse(items[i + 1]));

                }
            }

            if (triangles.StartsWith("\r\n"))
                triangles = triangles.Remove(0, 2);
            if (triangles.EndsWith("\r\n"))
                triangles = triangles.Remove(triangles.Length - 2, 2);
            if (triangles.StartsWith(" "))
                triangles = triangles.Remove(0, 1);
            items = triangles.Replace("\r", "").Replace("\n", "").Split(' '); ;
            List<int> trianglesArray = new List<int>();
            //int[] normalsTrianglesList = new int[items.Length / offset];
            List<int> UVsTrianglesList = new List<int>();
            //Here we get the triangles index AND if there are UV, we add them to a tuple to order them later. This happens because there can be more or less UV than vertex, but unity needs for both to be the same length
            List<int[]> tupleUV = new List<int[]>();
            for (int i = 0; i < items.Length; i = i + offset)
            {
                if (UVbool)
                {
                    if (i < items.Length -2 && !tupleUV.Contains(new int[] { int.Parse(items[i]), int.Parse(items[i + 2]) }) && !trianglesArray.Contains(int.Parse(items[i])))
                    {
                        UVsTrianglesList.Add(int.Parse(items[i + 2]));
                        tupleUV.Add(new int[] { int.Parse(items[i]), int.Parse(items[i + 2]) });
                    }
                    
                }
                trianglesArray.Add(int.Parse(items[i]));

                //normalsTrianglesList[i / offset] = int.Parse(items[i + 1]);

            }
            if (rotate)
            {
                for (int i = 0; i < trianglesArray.Count; i += 3)
                {
                    int intermediate = trianglesArray[i];
                    trianglesArray[i] = trianglesArray[i + 2];
                    trianglesArray[i + 2] = intermediate;
                }
            }
            //Here we get the UV associated to the indices and at the same length as the vertex array
            Vector2[] DefinitiveUVArray = new Vector2[] { };
            if (UVbool)
            {
                DefinitiveUVArray = new Vector2[tupleUV.Count];
                int i = 0;
                foreach (var UVpos in UVsTrianglesList)
                {
                    DefinitiveUVArray[i] = UVArray[UVpos];
                    i++;
                }

            }
            /*for (int i = 0; i < trianglesArray.Length; i++)
            {
                normalArray[trianglesArray[i]] = normalAux[normalsTrianglesList[i]];

            }*/

            //We assign them to the mesh
            newMesh.vertices = vertexArray;
            newMesh.normals = normalArray;
            newMesh.triangles = trianglesArray.ToArray();
            if (UVbool)
            {
                newMesh.uv = DefinitiveUVArray;
            }
            //Calculate the bounds and the normals (is cheaper as it suffers the same problem as th UV)
            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();
            
            allMeshes[j].subMeshIndex = 0;
            allMeshes[j].mesh = newMesh;
            name = '#' + name;
            //Matrix4x4 aux;
            //transforms.TryGetValue(name,out aux);
            //Instantiate the gameobject and assign it the mesh to it
            GameObject gb = GameObject.Instantiate(mapModel);
            
            gb.GetComponent<MeshFilter>().mesh = newMesh;

            gb.name = name;
            //Adding the gb to a list to assign the transform later, and giving them the material
            listGameobjects.Add(name, gb);
            Material mat = new Material(sh);
            mat.color = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
            gb.GetComponent<Renderer>().material = mat;

            var aux = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));
            allMeshes[j].transform = aux;
            
            //gb.transform.position = aux.
            j++;
            yield return new WaitForEndOfFrame();
        }
        //We get the transform matrix here
        XmlNodeList library_visual_scenes = xml.GetElementsByTagName("library_visual_scenes");
        var library = library_visual_scenes.Item(0);
        
        foreach (XmlNode item in library.ChildNodes[0])
        {
            GameObject part = addToDict(item, rotate);
            //CheckModelBounds()
            
            if (part != null && part.GetComponent<MeshRenderer>() != null)
            {
                //Creating the Bounding box
                modelBounds.Encapsulate(part.transform.localPosition + Vector3.Cross(part.GetComponent<MeshRenderer>().bounds.extents, part.transform.localScale));
                modelBounds.Encapsulate(part.transform.localPosition - Vector3.Cross(part.GetComponent<MeshRenderer>().bounds.extents, part.transform.localScale));
                //modelBounds.Encapsulate(part.transform.position + Vector3.Cross(Vector3.Cross(part.GetComponent<MeshFilter>().mesh.bounds.extents, part.transform.localRotation.eulerAngles), part.transform.localScale));
                //modelBounds.Encapsulate(part.transform.position - Vector3.Cross(Vector3.Cross(part.GetComponent<MeshFilter>().mesh.bounds.extents, part.transform.localRotation.eulerAngles), part.transform.localScale));
                //Ading each object to a parent
                part.transform.parent = final.transform;

            }
            else if(part != null)
            {
                //This is for groups, which don't have a Mesh
                part.transform.parent = final.transform;
            }
            //transforms.Add(, aux )
        }
        
        //UnityEngine.Debug.Log("Size: " + modelBounds.size);
        MissionManager.modelBoundingBox = modelBounds.size;
        MissionManager.modelBoundsCenter = modelBounds.center;
        //final.AddComponent<BoxCollider>().center = new Vector3(-modelBounds.center.x, modelBounds.center.y, -modelBounds.center.z);
        //final.GetComponent<BoxCollider>().size = modelBounds.size;
        //position the object in the middle of the bounding box
        final.transform.position -= modelBounds.center;

        MissionManager.modelBoundingBox = modelBounds.size;

        GameObject finalParent = new GameObject();
        finalParent.name = "DaeModel";
        final.transform.parent = finalParent.transform;
        //If the TRS matrix is valid, we apply it
        if (MissionManager.invMatrix.ValidTRS() && (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Recording || GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.ModelAlignment)) { 
            Matrix4x4 transfMatrix = Matrix4x4.Inverse(MissionManager.invMatrix);
            UnityEngine.Debug.Log("LossyScale: " + transfMatrix.lossyScale);
            finalParent.transform.localScale = transfMatrix.lossyScale * 0.001f;
            finalParent.transform.localRotation = transfMatrix.rotation;
            finalParent.transform.position += new Vector3(transfMatrix.m03 * 0.001f, transfMatrix.m23 * 0.001f, transfMatrix.m13 * 0.001f);
        }

        //finalMesh.CombineMeshes(allMeshes);
        //total = final;
    }
    //Fixes a bounding box, just i ncase
    void CheckModelBounds(Vector3 anchorPos)
    {
        if (anchorPos.x < minBoundingBox.x)
            minBoundingBox.x = Mathf.Abs(anchorPos.x);
        if (anchorPos.y < minBoundingBox.y)
            minBoundingBox.y = Mathf.Abs(anchorPos.y);
        if (anchorPos.z < minBoundingBox.z)
            minBoundingBox.z = Mathf.Abs(anchorPos.z);

        if (anchorPos.x > maxBoundingBox.x)
            maxBoundingBox.x = Mathf.Abs(anchorPos.x);
        if (anchorPos.y > maxBoundingBox.y)
            maxBoundingBox.y = Mathf.Abs(anchorPos.y);
        if (anchorPos.z > maxBoundingBox.z)
            maxBoundingBox.z = Mathf.Abs(anchorPos.z);
    }

}
