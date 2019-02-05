using System;
using UnityEngine;
using UnityEngine.UI;

// Include these namespaces to use BinaryFormatter
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Xml;
using System.Text;

namespace GracesGames.SimpleFileBrowser.Scripts {
	// Demo class to illustrate the usage of the FileBrowser script
	// Able to save and load files containing serialized data (e.g. text)
	public class DemoCaller : MonoBehaviour {
        public GameObject Canvas, titletext, textosave, savefile,loadedtext,loadfile, slider, slider2, slider3,slider4,inputfield,inputfield2,inputfield3;
        public Shader shader;


		// Use the file browser prefab
		public GameObject FileBrowserPrefab;

		// Define a file extension
		public string FileExtension;

		// Input field to get text to save
		private GameObject _textToSaveInputField;

		// Label to display loaded text
		private GameObject _loadedText;

		// Variable to save intermediate input result
		private string _textToSave;

		public bool PortraitMode;

		// Find the input field, label objects and add a onValueChanged listener to the input field
		private void Start() {
			_textToSaveInputField = GameObject.Find("TextToSaveInputField");
			_textToSaveInputField.GetComponent<InputField>().onValueChanged.AddListener(UpdateTextToSave);

			_loadedText = GameObject.Find("LoadedText");

			GameObject uiCanvas = GameObject.Find("Canvas");
			if (uiCanvas == null) {
                UnityEngine.Debug.LogError("Make sure there is a canvas GameObject present in the Hierarcy (Create UI/Canvas)");
			}
		}

		// Updates the text to save with the new input (current text in input field)
		public void UpdateTextToSave(string text) {
			_textToSave = text;
		}

		// Open the file browser using boolean parameter so it can be called in GUI
		public void OpenFileBrowser(bool saving) {
			OpenFileBrowser(saving ? FileBrowserMode.Save : FileBrowserMode.Load);
		}

		// Open a file browser to save and load files
		private void OpenFileBrowser(FileBrowserMode fileBrowserMode) {
			// Create the file browser and name it
			GameObject fileBrowserObject = Instantiate(FileBrowserPrefab, transform);
            fileBrowserObject.transform.localScale = fileBrowserObject.transform.localScale * 0.75f;
			fileBrowserObject.name = "FileBrowser";
			// Set the mode to save or load
			FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
			fileBrowserScript.SetupFileBrowser(PortraitMode ? ViewMode.Portrait : ViewMode.Landscape);
			if (fileBrowserMode == FileBrowserMode.Save) {
				fileBrowserScript.SaveFilePanel(this, "SaveFileUsingPath", "DemoText", FileExtension);
			} else {
				fileBrowserScript.OpenFilePanel(this, "LoadFileUsingPath", FileExtension);
			}
		}

		// Saves a file with the textToSave using a path
		private void SaveFileUsingPath(string path) {
			// Make sure path and _textToSave is not null or empty
			if (!String.IsNullOrEmpty(path) && !String.IsNullOrEmpty(_textToSave)) {
				BinaryFormatter bFormatter = new BinaryFormatter();
				// Create a file using the path
				FileStream file = File.Create(path);
				// Serialize the data (textToSave)
				bFormatter.Serialize(file, _textToSave);
				// Close the created file
				file.Close();
			} else {
                UnityEngine.Debug.Log("Invalid path or empty file given");
			}
		}
        IEnumerator LoadModel(string path)
        {
            

            WWW www = new WWW(path);
            yield return www;
            //UnityEngine.Debug.Log(www.assetBundle.ToString());
            //GameObject aux = Instantiate(www.assetBundle.LoadAsset<GameObject>("bb8") as GameObject);
            //Canvas.GetComponent<PlaceModel>().Change(aux);
        }
		// Loads a file using a path
		private void LoadFileUsingPath(string path) {
			if (path.Length != 0) {
				BinaryFormatter bFormatter = new BinaryFormatter();
				// Open the file using the path
				FileStream file = File.OpenRead(path);

                //var xmlWriter = new XmlTextWriter(file, Encoding.UTF8);
                //xmlWriter.
                XmlDocument doc = new XmlDocument();
                string aux;
                using (StreamReader reader = new StreamReader(file))
                {
                    aux = reader.ReadToEnd();
                }
                doc.LoadXml(aux);
                //The xml has been loaded. A collada is a xml file

                GameObject mesh = new GameObject();
                ColladaImporter importer = new ColladaImporter(ref mesh);
                //This calls the class that loads the collada model. We made it a coroutine in order to have a loading screen
                StartCoroutine(importer.getMesh(doc, shader));
                //if (!Directory.Exists(Application.dataPath + "/Resources/" + "bb8.fbx") && Directory.Exists(Application.dataPath))
                //GameObject aux = Instantiate(File.OpenRead(path)) as GameObject;
                Canvas.GetComponent<PlaceModel>().Change(mesh);

                //GameObject aux = Instantiate(AssetBundle.LoadFromFile(path) as GameObject;
				// Convert the file from a byte array into a string
				//string fileData = bFormatter.Deserialize(file) as string;
                //Mesh mesh = FastObjImporter.Instance.ImportFile(path);
                
                titletext.SetActive(false);
                textosave.SetActive(false);
                savefile.SetActive(false);
                loadedtext.SetActive(false);
                loadfile.SetActive(false);
                slider.SetActive(true);
                slider2.SetActive(true);
                slider3.SetActive(true);
                slider4.SetActive(true);
                inputfield.SetActive(true);
                inputfield2.SetActive(true);
                inputfield3.SetActive(true);

                //CombineInstance[] combine = new CombineInstance[aux.Length];
                /*while (i < aux.Length)
                {
                    combine[i].mesh = aux[i];
                    //combine[i].transform = aux[i].transform.localToWorldMatrix;
                    //meshFilters[i].gameObject.active = false;
                    i++;
                }
                //Mesh[] mesh = Resources.LoadAll<Mesh>("mapa");
				// We're done working with the file so we can close it
				//file.Close();
				// Set the LoadedText with the value of the file
				//_loadedText.GetComponent<Text>().text = "Loaded data: \n" + fileData;
                GameObject sderfd = new GameObject();
                sderfd.transform.position = new Vector3(0, 0, 0);
                sderfd.AddComponent<MeshFilter>();

                sderfd.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
                sderfd.AddComponent<MeshRenderer>();*/

			} else {
                UnityEngine.Debug.Log("Invalid path given");
			}
		}
	}
}