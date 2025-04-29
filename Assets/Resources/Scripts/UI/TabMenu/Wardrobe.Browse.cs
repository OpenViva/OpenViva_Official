using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Viva.Util;

namespace Viva
{


    public partial class Wardrobe : UITabMenu
    {

        public class LoadClothingCardRequest
        {
            public readonly string filename;
            public ClothingPreset clothingPreset;
            public Outfit.ClothingOverride clothingOverride;
            public string error;

            public LoadClothingCardRequest(string _filename)
            {
                filename = _filename;
            }
        }


        [Header("Browse")]
        [SerializeField]
        private GameObject browseTab;
        [SerializeField]
        private Button browseTabButton;
        [SerializeField]
        private CardBrowser clothingCardBrowser;


        private ClothingPreset FindClothingPresetByClothingName(string clothingPieceName)
        {
            for (int i = 0; i < GameDirector.instance.wardrobe.Count; i++)
            {
                if (GameDirector.instance.wardrobe[i].clothePieceName == clothingPieceName)
                {
                    return GameDirector.instance.wardrobe[i];
                }
            }
            return null;
        }

        private void InitializeBrowseTab()
        {
            clothingCardBrowser.Initialize(true, LoadClothingCard);
        }

        public void LoadClothingCard(string name, Button sourceButton)
        {
            GameDirector.instance.StartCoroutine(HandleLoadClothingCard(name, sourceButton));
        }

        private void EndLoadClothingCard(string error)
        {
            if (error != null)
            {
                Debug.LogError(error);
            }
            StopLoadingCycle();
        }

        public IEnumerator HandleLoadClothingCard(LoadClothingCardRequest request, OnGenericCallback onFinished)
        {

            CardBrowser.LoadCardTextureRequest clothingTextureRequest = new CardBrowser.LoadCardTextureRequest(request.filename);
            yield return GameDirector.instance.StartCoroutine(clothingCardBrowser.LoadCardTexture(clothingTextureRequest));
            if (clothingTextureRequest.result == null)
            {
                request.error = clothingTextureRequest.error;
                onFinished?.Invoke();
                yield break;
            }
            Steganography.UnpackClothingTextureRequest clothingCardRequest = new Steganography.UnpackClothingTextureRequest(clothingTextureRequest.result);
            yield return GameDirector.instance.StartCoroutine(Steganography.main.ExecuteUnpackClothingCard(clothingCardRequest));
            if (clothingCardRequest.result == null)
            {
                request.error = clothingTextureRequest.error;
                onFinished?.Invoke();
                yield break;
            }
            request.clothingPreset = Wardrobe.main.FindClothingPresetByClothingName(clothingCardRequest.result.clothingPieceName);
            if (request.clothingPreset == null)
            {
                request.error = "ERROR Could not find ClothingPreset with name: " + request.clothingPreset;
                onFinished?.Invoke();
                yield break;
            }
            byte[] pngShot = ImageConversion.EncodeToPNG(clothingCardRequest.result.texture);
            //File.WriteAllBytes(screengrabfile_path, pngShot);

            request.clothingOverride = new Outfit.ClothingOverride(clothingCardRequest.result.texture, request.filename);
            onFinished?.Invoke();
        }

        public IEnumerator HandleLoadClothingCard(string name, Button sourceButton)
        {

            StartLoadingCycle();
            CardBrowser.LoadCardTextureRequest clothingTextureRequest = new CardBrowser.LoadCardTextureRequest(name);
            yield return GameDirector.instance.StartCoroutine(clothingCardBrowser.LoadCardTexture(clothingTextureRequest));
            if (clothingTextureRequest.result == null)
            {
                EndLoadClothingCard(clothingTextureRequest.error);
                yield break;
            }
            Steganography.UnpackClothingTextureRequest clothingCardRequest = new Steganography.UnpackClothingTextureRequest(clothingTextureRequest.result);
            yield return GameDirector.instance.StartCoroutine(Steganography.main.ExecuteUnpackClothingCard(clothingCardRequest));

            //handle result
            if (clothingCardRequest.result == null)
            {
                EndLoadClothingCard(clothingCardRequest.error);
                yield break;
            }

            ClothingPreset clothingPreset = Wardrobe.main.FindClothingPresetByClothingName(clothingCardRequest.result.clothingPieceName);
            if (clothingPreset == null)
            {
                EndLoadClothingCard("ERROR Could not find ClothingPreset with name: " + clothingPreset);
                yield break;
            }

            foreach (var loli in GameDirector.player.objectFingerPointer.selectedCompanions)
            {
                if (clothingPreset.clothePieceName == "cat ears" || clothingPreset.clothePieceName.Contains("glasses"))
                {
                    if (loli.headModel.name != "shinobu")
                    {
                        SetTab((int)Tab.RESULT);
                        resultProgressText.text = "That clothing piece can only be applied to Shinobu!";
                        yield break;
                    }
                }
                ClothingPreset clothingPiece = GameDirector.instance.FindClothing(clothingPreset.clothePieceName);
                if (clothingPreset == null)
                {
                    EndLoadClothingCard("ERROR Could not find ClothingPiece with name: " + clothingPiece);
                    yield break;
                }

                loli.outfit.WearClothingPiece(loli, clothingPiece, new Outfit.ClothingOverride(clothingCardRequest.result.texture, name));
                loli.SetOutfit(loli.outfit);

                loli.passive.clothing.AttemptReactToOutfitChange();
            }
            EndLoadClothingCard(null);

            GameDirector.instance.StartCoroutine(FlashCardUIElement(sourceButton.GetComponent<Image>()));
            string Path = Application.dataPath.Replace("Assets", "") + "ExportedCards/";
            Debug.Log(Path + clothingCardRequest.result.clothingTextureName.Replace(" ", "").Replace("shoe", "").Replace("spat", "").Replace("pant", "").Replace("-", "").Replace("shir", "").Trim() + ".png".Trim());
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            byte[] pngShot = ImageConversion.EncodeToPNG(clothingCardRequest.result.texture);
            File.WriteAllBytes(Path + clothingCardRequest.result.clothingTextureName.Replace(" ", "").Replace("shoe", "").Replace("spat", "").Replace("pant", "").Replace("-", "").Replace("shir", "").Trim() + ".png".Trim(), pngShot);

            ModelCustomizer.main.PlaySpawnSound();
        }

        private IEnumerator FlashCardUIElement(Image image)
        {
            float timer = 0;
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                if (image == null)
                {   //was destroyed externally
                    yield break;
                }
                float ratio = 0.5f + Mathf.Clamp01(timer / 0.5f) * 0.5f;
                image.transform.localScale = Vector3.LerpUnclamped(Vector3.one * 3.5f, Vector3.one, Tools.EaseOutQuad(ratio));
                image.color = Color.LerpUnclamped(Color.green, Color.white, ratio);

                yield return null;
            }
        }

    }

}