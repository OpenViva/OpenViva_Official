using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraEffectRenderPass : ScriptableRenderPass
{
    public Material effectMaterial;
    private RenderTargetIdentifier source;
    private RenderTargetHandle tempTexture;

    public CameraEffectRenderPass(Material material)
    {
        effectMaterial = material;
        tempTexture.Init("_TempEffectTexture");
    }

    public void SetEffectMaterial(Material material)
    {
        effectMaterial = material;
    }

    // Set the source render target during Execute, not AddRenderPasses
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (effectMaterial == null) return;

        CommandBuffer cmd = CommandBufferPool.Get("Apply Camera Effect");
        
        RenderTargetIdentifier cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
        RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
        cmd.GetTemporaryRT(tempTexture.id, cameraTextureDesc);

        //blit
        cmd.Blit(cameraColorTarget, tempTexture.Identifier(), effectMaterial);
        cmd.Blit(tempTexture.Identifier(), cameraColorTarget);

        //execute
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        if (tempTexture != RenderTargetHandle.CameraTarget)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }
}