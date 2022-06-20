using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class HDRConversionFeature : ScriptableRendererFeature
{
    public enum ETexColorType : ushort
    {
        SRGBScene_SRGBUI = 1 << 0,   //Mark As Default  //0
        SRGBScene_LinearUI = 1 << 1,//2
        LinearScene_SRGBUI = 1 << 2,  //4
                                      //OTHERS TEXFORMAT WITH TYPE  
        e_NONE //5
    }

    //
    public ETexColorType _TexInfo = ETexColorType.SRGBScene_SRGBUI;

    public bool ToBackBuffer = false;


    private bool SRGBSceneSRGBUI => (_TexInfo & ETexColorType.SRGBScene_SRGBUI) != 0;
    private bool SRGBSceneLinearUI => (_TexInfo & ETexColorType.SRGBScene_LinearUI) != 0;
    private bool LinearSceneSRGBUI => (_TexInfo & ETexColorType.LinearScene_SRGBUI) != 0;

    private bool NONE => (_TexInfo & ETexColorType.e_NONE) != 0;



    HDRBlitPass m_BlitPass;
    SRGBSceneRTPass m_SRGBSceneRTPass;
    Material m_SRGBSceneMat;

    [SerializeField]
    private Shader m_SRGBSceneShader;


    //初始化操作
    public override void Create()
    {
        //主要的RenderPassEvent 在渲染一系列物体以及后处理完毕后
        m_BlitPass = new HDRBlitPass(RenderPassEvent.AfterRendering);

        //shader 不为空 创建材质球
        if (m_SRGBSceneShader != null)
            m_SRGBSceneMat = new Material(m_SRGBSceneShader);
        //如果材质球 不为空
        if (m_SRGBSceneMat != null)
            m_SRGBSceneRTPass = new SRGBSceneRTPass(m_SRGBSceneMat);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (NONE)
        {
            //Mark The afterProcessingRT As SRGB 

#if UNITY_EDITOR
            //编辑器下多种情况处理

            //1. 单相机 只在编辑器下
            if (Camera.allCameras.Length == 1 && !Application.isPlaying)
            {

            }
            //2. 单相机 编辑器运行情况下
            else if (Camera.allCameras.Length == 1 && Application.isPlaying)
            {

            }
            //3. 编辑器下 多相机 但是UI相机被关闭
            else if (Camera.allCameras.Length > 1 && !ConatainsUICamera() && Application.isPlaying)
            {

                if (m_SRGBSceneRTPass == null && m_SRGBSceneMat != null)
                {
                    m_SRGBSceneRTPass = new SRGBSceneRTPass(m_SRGBSceneMat);
                }
                m_SRGBSceneRTPass.ConfigureInput(ScriptableRenderPassInput.Color);
                m_SRGBSceneRTPass.SetEnableToBackBuffer(ToBackBuffer);
                m_SRGBSceneRTPass.SetTarget(renderer.cameraColorTarget);
                renderer.EnqueuePass(m_SRGBSceneRTPass);
            }
            //4. 编辑器下 多相机 无UI 不运行
            else if (Camera.allCameras.Length > 1 && !ConatainsUICamera() && !Application.isPlaying)
            {

                if (m_SRGBSceneRTPass == null && m_SRGBSceneMat != null)
                {
                    m_SRGBSceneRTPass = new SRGBSceneRTPass(m_SRGBSceneMat);
                }
                m_SRGBSceneRTPass.ConfigureInput(ScriptableRenderPassInput.Color);
                m_SRGBSceneRTPass.SetEnableToBackBuffer(ToBackBuffer);
                m_SRGBSceneRTPass.SetTarget(renderer.cameraColorTarget);
                renderer.EnqueuePass(m_SRGBSceneRTPass);
            }
#else
            try
            { 
                IStoryModule storyModule = GameHelp.GetGame().StoryModule;
                //
                if (storyModule.GetTimelineState() != Q1Game.Timeline.TimelineState.None)
                { 
                    if (null == m_BlitPass)
                        m_BlitPass = new HDRBlitPass(RenderPassEvent.AfterRendering);

                    renderingData.cameraData.SRGBConversionEnable = false;
                    m_BlitPass.source = renderer.cameraColorTarget;
                    renderer.EnqueuePass(m_BlitPass);
                }
            }
            catch
            {

            }
#endif
            return;
        }

        if (SRGBSceneSRGBUI)
        {
            m_BlitPass.source = renderer.cameraColorTarget;
            renderer.EnqueuePass(m_BlitPass);
        }

       // SRGBSceneLinearUI 并且为game摄像机时
        else if (SRGBSceneLinearUI && m_SRGBSceneRTPass != null && renderingData.cameraData.cameraType == CameraType.Game)
        {
            m_SRGBSceneRTPass.ConfigureInput(ScriptableRenderPassInput.Color);
            m_SRGBSceneRTPass.SetEnableToBackBuffer(ToBackBuffer);
            m_SRGBSceneRTPass.SetTarget(renderer.cameraColorTarget);
            //将Pass加入队列
            renderer.EnqueuePass(m_SRGBSceneRTPass);
        }

        if (SRGBSceneLinearUI && m_SRGBSceneRTPass != null)
        {
            m_SRGBSceneRTPass.ConfigureInput(ScriptableRenderPassInput.Color);
            m_SRGBSceneRTPass.SetEnableToBackBuffer(ToBackBuffer);
            m_SRGBSceneRTPass.SetTarget(renderer.cameraColorTarget);
         //   将Pass加入队列
            renderer.EnqueuePass(m_SRGBSceneRTPass);
        }
    }
#if UNITY_EDITOR
    //Private Helper
    //private static readonly LayerMask _ui_Mask = 1 << LayerMask.NameToLayer("UI"); 
    //只有在编辑器下才有GC 无需处理 防止相机没有开后处理情况

    private bool ConatainsUICamera()
    {
        foreach (var cam in Camera.allCameras)
        {
            if (cam.gameObject.layer == 1 << LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }
#endif

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_SRGBSceneMat);
    }

}


public class HDRBlitPass : ScriptableRenderPass
{
    public RenderTargetIdentifier source;
    //RT
    RenderTargetHandle m_BlitRT;

    static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("HDR Conversion");

    //初始化操作
    public HDRBlitPass(RenderPassEvent evt)
    {
        base.profilingSampler = new ProfilingSampler(nameof(HDRBlitPass));
        //初始化RenderTargetHandle
        m_BlitRT.Init("_BlitTexture");
        renderPassEvent = evt;
    }

    //相机数据
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        //创建RT
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
        descriptor.msaaSamples = 1;
        descriptor.depthBufferBits = 0;
        cmd.GetTemporaryRT(m_BlitRT.id, descriptor, FilterMode.Point);
    }


    //执行
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            RenderTargetIdentifier LDRRT = m_BlitRT.Identifier();
            cmd.SetRenderTarget(LDRRT);

            cmd.Blit(source, LDRRT);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    //释放临时RT资源
    public override void OnFinishCameraStackRendering(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(m_BlitRT.id);
    }

}


internal class SRGBSceneRTPass : ScriptableRenderPass
{
    ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SRGBSceneDraw");
    Material m_Material;
    RenderTargetIdentifier m_CameraColorTarget;
    private bool _IsToBackBuffer = false;
    public bool IsToBackBuffer => _IsToBackBuffer;
    RenderTexture m_TempRT;


    public SRGBSceneRTPass(Material material)
    {
        m_Material = material;
        renderPassEvent = RenderPassEvent.AfterRendering + 1;
    }

    public void SetTarget(RenderTargetIdentifier colorHandle)
    {
        m_CameraColorTarget = colorHandle;
    }


    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        //ConfigureTarget(new RenderTargetIdentifier(m_CameraColorTarget, 0, CubemapFace.Unknown, -1));
    }


    public void SetEnableToBackBuffer(bool isToBackBuffer)
    {
        _IsToBackBuffer = isToBackBuffer;
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var camera = renderingData.cameraData.camera;
        if (camera.cameraType != CameraType.Game)
            return;

        if (m_Material == null)
            return;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            m_TempRT = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.RGB111110Float);
            cmd.Blit(m_CameraColorTarget, m_TempRT);
            m_Material.SetTexture("_MainTex", m_TempRT);

            if (!IsToBackBuffer)
                cmd.SetRenderTarget(new RenderTargetIdentifier(m_CameraColorTarget, 0, CubemapFace.Unknown, -1));
            else
                cmd.SetRenderTarget(null as RenderTexture, 0, CubemapFace.Unknown, -1);
            //进行最终的绘制线性到gamma空间的转换
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Material);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
        RenderTexture.ReleaseTemporary(m_TempRT);
    }
}