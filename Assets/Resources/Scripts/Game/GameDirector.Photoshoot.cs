using System.Collections;
using UnityEngine;

namespace Viva
{


    public partial class GameDirector : MonoBehaviour
    {

        [SerializeField]
        private Camera photoshootCamera;
        [SerializeField]
        private GameObject photoshootStage;
        [SerializeField]
        private Transform photoshootSun;
        
        public CameraEffectRendererFeature effectFeature;
        
        public class PhotoshootRequest
        {

            public Texture2D texture;
            public Vector2Int resolution;
            public CameraPose cameraPose;
            public Texture2D background;
            public Companion.Animation pose;

            public PhotoshootRequest(Vector2Int _resolution, CameraPose _cameraPose, Texture2D _background, Companion.Animation _pose)
            {
                resolution = _resolution;
                cameraPose = _cameraPose;
                background = _background;
                pose = _pose;
            }
        }

        public IEnumerator RenderPhotoshoot(Companion companion, PhotoshootRequest request)
        {

            photoshootCamera.transform.localPosition = request.cameraPose.position;
            photoshootCamera.transform.localEulerAngles = request.cameraPose.rotation;
            photoshootCamera.fieldOfView = request.cameraPose.fov;

            Vector3 oldPosition = companion.transform.position;
            Quaternion oldRotation = companion.transform.rotation;
            Quaternion oldSunRotation = skyDirector.sun.transform.rotation;

            //must make sure companion is in a proper behavior to override animations!
            //freeze companion in place without logic momentarily to simulate clothing and hair
            companion.Teleport(photoshootStage.transform.position, photoshootStage.transform.rotation);
            characters.Remove(companion);
            companion.puppetMaster.SetEnableGravity(false);

            photoshootStage.SetActive(true);

            companion.ResetEyeUniforms();
            Debug.Log($"Pose: {request.pose.ToString()}");
            companion.ForceImmediatePose(request.pose);
            yield return new WaitForSeconds(1f);
            GameDirector.skyDirector.OverrideDayNightCycleLighting(GameDirector.skyDirector.defaultDayNightPhase, photoshootSun.rotation);
            GameDirector.skyDirector.sun.color = Color.black;
            Material effectMat = effectFeature.GetEffectMaterial();
            if (effectMat != null)
            {
                effectMat.SetTexture("_Background", request.background);
            }
            request.texture = RenderPhotoshootTexture(photoshootCamera, request.resolution);
            yield return new WaitForSeconds(0.5f);
            GameDirector.skyDirector.RestoreDayNightCycleLighting();

            //restore unfrozen settings
            characters.Add(companion);
            companion.puppetMaster.SetEnableGravity(true);
            companion.Teleport(oldPosition, oldRotation);
            companion.ForceImmediatePose(companion.GetLastReturnableIdleAnimation());

            photoshootStage.SetActive(false);
        }

        private Texture2D RenderPhotoshootTexture(Camera camera, Vector2Int resolution)
        {
            RenderTexture renderTexture = new RenderTexture(resolution.x, resolution.y, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            camera.targetTexture = renderTexture;

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;
            camera.Render();

            //render native size 1x
            Texture2D newTexture = new Texture2D(Steganography.PACK_SIZE, Steganography.CARD_HEIGHT, TextureFormat.RGB24, false, true);
            newTexture.ReadPixels(new Rect(0, 0, Steganography.PACK_SIZE, Steganography.CARD_HEIGHT), 0, 0, false);
            newTexture.Apply(false, false);

            RenderTexture.active = currentRT;
            renderTexture.Release();    //destroy RT memory

            return newTexture;
        }

    }

}