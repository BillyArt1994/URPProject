using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CommonRenderFeature : ScriptableRendererFeature
{

    public Shader UsedShader;
    public RenderPassEvent PassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    CommonPass m_ScriptablePass;

    class CommonPass : ScriptableRenderPass
    {
        Material m_material;
        RenderTargetIdentifier source;
        RenderTargetIdentifier dst;
        RenderTargetHandle m_temporaryColorTexture;

        public CommonPass(Material mat)
        {
            m_material = mat;
        }

        public void Setup(RenderTargetIdentifier src)
        {
            this.source = src;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            var opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            //    cmd.GetTemporaryRT(m_renderTargetHandle.id, opaqueDesc, FilterMode.Point);
            Blit(cmd, source, m_temporaryColorTexture.Identifier(), m_material);
            Blit(cmd, m_temporaryColorTexture.Identifier(), source);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {

        }
    }

    public override void Create()
    {
        if (UsedShader == null) return;
        var mat = new Material(UsedShader);
        m_ScriptablePass = new CommonPass(mat);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // if (renderingData.cameraData.isSceneViewCamera) return;
        //if (mat!=null)
        var src = renderer.cameraColorTarget;
        m_ScriptablePass.Setup(src);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


