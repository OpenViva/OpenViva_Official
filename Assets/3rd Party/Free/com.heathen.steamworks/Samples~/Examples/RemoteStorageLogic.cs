#if !DISABLESTEAMWORKS && STEAMWORKSNET
using Heathen.SteamworksIntegration;
using System;
using System.Collections.Generic;
using UnityEngine;
using CloudSave = Heathen.SteamworksIntegration.API.RemoteStorage.Client;

namespace Heathen.DEMO
{
    public class RemoteStorageLogic : MonoBehaviour
    {
        [Serializable]
        public struct DemoSaveData
        {
            public string stringData;
            public int intData;
            public bool boolData;
        }

        [SerializeField]
        private TMPro.TMP_InputField saveFileName;
        [SerializeField]
        private TMPro.TMP_InputField stringField;
        [SerializeField]
        private TMPro.TMP_InputField intField;
        [SerializeField]
        private UnityEngine.UI.Toggle boolField;
        [SerializeField]
        private Transform fileListRoot;
        [SerializeField]
        private GameObject displayRecordTemplate;

        private List<GameObject> listedRecords = new List<GameObject>();
        private DemoSaveData loadedData;

        public void OpenKnowledgeBaseUserData()
        {
            Application.OpenURL("https://kb.heathen.group/steam/cloud-save");
        }

        public void Save()
        {
            loadedData.stringData = stringField.text;
            if (int.TryParse(intField.text, out int result))
                loadedData.intData = result;
            loadedData.boolData = boolField.isOn;

            //Make sure we will have a valid name, this will overwrite if the name matches
            if (!string.IsNullOrEmpty(saveFileName.text.ToLower().Replace(".demo", "")))
            {
                CloudSave.FileWrite(saveFileName.text, loadedData);
                Refresh();
            }
        }

        public void Refresh()
        {
            try
            {
                //Remove the old records
                while (listedRecords.Count > 0)
                {
                    var target = listedRecords[0];
                    listedRecords.Remove(target);
                    Destroy(target);
                }

                //Create a record for each file present
                foreach (var file in CloudSave.GetFiles(".demo"))
                {
                    var go = Instantiate(displayRecordTemplate, fileListRoot);
                    var comp = go.GetComponent<DemoDataDisplayRecord>();
                    comp.Initialize(file, LoadFile);
                    listedRecords.Add(go);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void LoadFile(RemoteStorageFile file)
        {
            loadedData = file.ToJson<DemoSaveData>();
            stringField.text = loadedData.stringData;
            intField.text = loadedData.intData.ToString();
            boolField.isOn = loadedData.boolData;
            saveFileName.text = file.name;
        }
    }
}
#endif