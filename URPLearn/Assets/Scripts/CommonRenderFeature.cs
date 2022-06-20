using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CommonRenderFeature : ScriptableRendererFeature
{

    public Shader UsedShader;
    public Material m_material;
    public RenderPassEvent PassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    CommonPass m_ScriptablePass;

    class CommonPass : ScriptableRenderPass
    {
        Material m_material;
        RenderTargetIdentifier source;
        RenderTargetHandle m_TemporaryColorTexture;
        static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Test Pass");
        RenderTexture m_TempRT;

        public CommonPass(Material mat)
        {
            base.profilingSampler = new ProfilingSampler(nameof(CommonPass));
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
            var camera = renderingData.cameraData.camera;
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.GetTemporaryRT(m_TemporaryColorTexture.id, renderingData.cameraData.cameraTargetDescriptor, FilterMode.Bilinear);
              //  m_TempRT = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.RGB111110Float);
                cmd.Blit(source, m_TemporaryColorTexture.Identifier(), m_material);
                cmd.Blit(m_TemporaryColorTexture.Identifier(), source);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {

        }
    }

    public override void Create()
    {
        if (m_material == null) return;
        m_ScriptablePass = new CommonPass(m_material);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var dest = RenderTargetHandle.CameraTarget;
        var src = renderer.cameraColorTarget;
        m_ScriptablePass.Setup(src);
        renderer.EnqueuePass(m_ScriptablePass);

    }
}


