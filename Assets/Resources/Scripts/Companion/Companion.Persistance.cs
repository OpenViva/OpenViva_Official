using System;
using System.Collections;
using UnityEngine;
using Viva.Util;

namespace Viva
{


    public partial class Companion : Character
    {

        public delegate void OnLoadFinish();


        public static IEnumerator LoadCompanionFromSerializedCompanion(string cardFilename, Companion targetCompanion, OnLoadFinish onFinish)
        {

            if (targetCompanion == null)
            {
                Debug.LogError("[COMPANION] targetCompanion is null!");
                yield break;
            }
            ModelCustomizer.LoadCompanionFromCardRequest cardRequest = new ModelCustomizer.LoadCompanionFromCardRequest(cardFilename, targetCompanion);
            yield return GameDirector.instance.StartCoroutine(ModelCustomizer.main.LoadVivaModelCard(cardRequest));
            if (cardRequest.target == null)
            {
                Debug.LogError(cardRequest.error);
                if (onFinish != null)
                {
                    onFinish();
                }
                yield break;
            }

            Debug.Log("[COMPANION] Loaded " + cardFilename);
            if (onFinish != null)
            {
                onFinish();
            }
        }

        public void SetNameTagTexture(Texture2D texture, float yOffset)
        {
            if (texture == null)
            {
                nametagMR.gameObject.SetActive(false);
            }
            else
            {
                nametagMR.gameObject.SetActive(true);
                nametagMR.material.mainTexture = texture;
                nametagMR.material.SetTextureOffset("_MainTex", new Vector2(0, yOffset));
            }
        }

        public override sealed void Save(GameDirector.VivaFile vivaFile)
        {

            //serialize companion self
            var serializedAsset = new GameDirector.VivaFile.SerializedAsset(this);
            if (serializedAsset == null)
            {
                Debug.LogError("[PERSISTANCE] Could not save companion " + name);
                return;
            }
            var selfAsset = new GameDirector.VivaFile.SerializedCompanion(headModel.sourceCardFilename, serializedAsset);
            serializedAsset.transform.position = floorPos;
            serializedAsset.transform.rotation = Quaternion.LookRotation(Tools.FlatForward(spine1RigidBody.transform.forward), Vector3.up);

            selfAsset.activeTaskSession = new GameDirector.VivaFile.SerializedCompanion.SerializedTaskData(active.currentTask);
            selfAsset.serviceIndex = Service.GetServiceIndex(this); //-1 if null
            vivaFile.companionAssets.Add(selfAsset);
        }

        //applies all serialized properties and awakens companion scripts
        public IEnumerator InitializeCompanion(GameDirector.VivaFile.SerializedCompanion serializedCompanion, OnGenericCallback onFinish)
        {

            if (serializedCompanion == null)
            {
                Debug.LogError("[COMPANION] Companion Asset is null!");
                yield break;
            }

            //apply serialized properties
            var cdm = new CoroutineDeserializeManager();
            GameDirector.instance.StartCoroutine(SerializedVivaProperty.Deserialize(serializedCompanion.propertiesAsset.properties, this, cdm));
            transform.position = serializedCompanion.propertiesAsset.transform.position;
            transform.rotation = serializedCompanion.propertiesAsset.transform.rotation;

            while (!cdm.finished)
            {
                yield return null;
            }

            gameObject.SetActive(true); //call character awake

            //employ into service if any
            if (serializedCompanion.serviceIndex >= 0 && serializedCompanion.serviceIndex < GameDirector.instance.town.services.Count)
            {
                var service = GameDirector.instance.town.services[serializedCompanion.serviceIndex];
                if (service.Employ(this))
                {
                    //match task with service
                    serializedCompanion.activeTaskSession.taskIndex = (int)service.targetBehavior;
                }
            }
            //apply active session if any
            if (serializedCompanion.activeTaskSession != null)
            {
                var task = active.GetTask(
                        (ActiveBehaviors.Behavior)serializedCompanion.activeTaskSession.taskIndex);
                    GameDirector.instance.StartCoroutine(
                        SerializedVivaProperty.Deserialize(serializedCompanion.activeTaskSession.properties,
                            task.session, cdm));
                    active.SetTask(task, null);

            }

            while (!cdm.finished)
            {
                yield return null;
            }

            GameDirector.characters.Add(this);

            onFinish?.Invoke();
        }
    }

}