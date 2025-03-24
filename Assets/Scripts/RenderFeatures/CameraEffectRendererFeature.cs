using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraEffectRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Material initialEffectMaterial;
    [SerializeField] private LayerMask targetCameraLayer;
    private CameraEffectRenderPass effectRenderPass;

    public override void Create()
    {
        effectRenderPass = new CameraEffectRenderPass(initialEffectMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingSkybox
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Camera currentCamera = renderingData.cameraData.camera;
        if (targetCameraLayer == (targetCameraLayer | (1 << currentCamera.gameObject.layer)))
        {
            renderer.EnqueuePass(effectRenderPass);
        }
    }

    public void SetEffectMaterial(Material newMaterial)
    {
        effectRenderPass.SetEffectMaterial(newMaterial);
    }

    public Material GetEffectMaterial()
    {
        return effectRenderPass.effectMaterial;
    }
}