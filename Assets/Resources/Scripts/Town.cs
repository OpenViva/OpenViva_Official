using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Viva.Util;


namespace Viva
{

    using CompanionInit = Tuple<Companion, GameDirector.VivaFile.SerializedCompanion>;

    public class Town : VivaSessionAsset
    {

        [SerializeField]
        private int maxResidents = 8;
        [SerializeField]
        private Waypoints m_mainWaypoints;
        public Waypoints mainWaypoints { get { return m_mainWaypoints; } }
        [SerializeField]
        private List<Service> m_services = new List<Service>();
        public List<Service> services { get { return m_services; } }



        protected override void OnAwake()
        {
            GameDirector.instance.AddOnFinishLoadingCallback(OnPostLoad);
        }

        private void OnPostLoad()
        {
            var cardsAvailable = ModelCustomizer.main.characterCardBrowser.FindAllExistingCardsInFolders();

            if (cardsAvailable.Length > 0)
            {
                BuildTownLolis(cardsAvailable, maxResidents - (GameDirector.characters.Count - 1), null);
            }
        }

        public void BuildTownLolis(string[] cards, int count, Vector3? defaultSpawnPos)
        {
            if (count == 0)
            {
                return;
            }
            List<CompanionInit> startingTownCompanions = new List<CompanionInit>();
            Debug.Log("[Town] Generating " + count + " companion residents...");
            for (int i = 0; i < count; i++)
            {
                var cardFilename = cards[i % cards.Length] + ".png";                                                          
                var serializedCompanion = new GameDirector.VivaFile.SerializedCompanion(cardFilename, new GameDirector.VivaFile.SerializedAsset(cardFilename));
                var targetCompanion = GameDirector.instance.GetCompanionFromPool();
                string path = ModelCustomizer.main.characterCardBrowser.cardFolder + "/" + cardFilename;
                if (!File.Exists(path))
                {
                    return;
                }   
                GameDirector.instance.StartCoroutine(Companion.LoadCompanionFromSerializedCompanion(serializedCompanion.sourceCardFilename, targetCompanion, delegate ()
                {
                    startingTownCompanions.Add(new CompanionInit(targetCompanion, serializedCompanion));
                    if (startingTownCompanions.Count == count)
                    {
                        PrepareTownCompanions(startingTownCompanions, defaultSpawnPos);
                    }
                }
                ));
            }
        }

        private void PrepareTownCompanions(List<CompanionInit> startingTownCompanions, Vector3? defaultSpawnPos)
        {

            int index = startingTownCompanions.Count;
            if (!defaultSpawnPos.HasValue)
            {
                for (int i = 0; i < services.Count; i++)
                {
                    var service = services[i];

                    int employeeInfosAvailable = service.employeeInfosAvailable;
                    while (employeeInfosAvailable-- > 0 && index-- > 0)
                    {
                        var employeeSlot = service.GetEmployeeInfo(employeeInfosAvailable);
                        var worldSpawnPosition = service.transform.TransformPoint(employeeSlot.localPos);
                        var worldSpawnForward = service.transform.TransformDirection(employeeSlot.localRootFacePos);
                        startingTownCompanions[index]._2.serviceIndex = i;
                        startingTownCompanions[index]._2.propertiesAsset.transform.position = worldSpawnPosition;
                        startingTownCompanions[index]._2.propertiesAsset.transform.rotation = Quaternion.LookRotation(worldSpawnForward, Vector3.up);
                    }
                }
            }
            while (index-- > 0)
            {
                int nodeIndex = Random.Range(0, mainWaypoints.nodes.Length - 1);
                Vector3 worldSpawnPosition;
                if (defaultSpawnPos.HasValue)
                {
                    worldSpawnPosition = defaultSpawnPos.Value;
                }
                else
                {
                    worldSpawnPosition = transform.TransformPoint(mainWaypoints.nodes[nodeIndex].position);
                }
                startingTownCompanions[index]._2.propertiesAsset.transform.position = worldSpawnPosition;
            }

            foreach (var pair in startingTownCompanions)
            {
                GameDirector.instance.StartCoroutine(pair._1.InitializeCompanion(pair._2, null));
            }
        }
    }

}