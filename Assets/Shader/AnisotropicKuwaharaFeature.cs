using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AnisotropicKuwaharaFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;
        public Material Material; // uses "Hidden/AnisotropicKuwaharaURP"

        // Parameters mirrored to shader
        [Range(2, 64)] public int KernelSize = 8; // even
        [Range(1, 8)]  public int N = 8;
        public float Hardness = 1.0f;
        public float Q = 1.0f;
        public float Alpha = 1.0f;
        [Range(0, 3.14159f)] public float ZeroCrossing = 1.2f;
        public float Zeta = 0.5f;

        // Performance/format knobs
        public bool UseFloatRT = true; // ARGBFloat for intermediates; else ARGBHalf
    }

    class Pass : ScriptableRenderPass
    {
        private readonly Settings settings;
        private readonly string profilerTag = "AnisotropicKuwahara";
        private Material mat;

        // RTs
        RTHandle rtEigen;
        RTHandle rtBlurX;
        RTHandle rtTFM;
        RTHandle rtFinal;

        static readonly int _TFM_ID = Shader.PropertyToID("_TFM");
        static readonly int _MainTex_TexelSize_ID = Shader.PropertyToID("_MainTex_TexelSize");
        static readonly int _KernelSize_ID = Shader.PropertyToID("_KernelSize");
        static readonly int _N_ID = Shader.PropertyToID("_N");
        static readonly int _Hardness_ID = Shader.PropertyToID("_Hardness");
        static readonly int _Q_ID = Shader.PropertyToID("_Q");
        static readonly int _Alpha_ID = Shader.PropertyToID("_Alpha");
        static readonly int _ZeroCrossing_ID = Shader.PropertyToID("_ZeroCrossing");
        static readonly int _Zeta_ID = Shader.PropertyToID("_Zeta");

        public Pass(Settings s)
        {
            settings = s;
            renderPassEvent = s.Event;
            mat = s.Material;

            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Keep pass material in sync with the serialized settings
            mat = settings.Material;

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;
            desc.enableRandomWrite = false;
            desc.graphicsFormat = settings.UseFloatRT
                ? UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat
                : UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat;

            RenderingUtils.ReAllocateIfNeeded(ref rtEigen, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AK_Eigen");
            RenderingUtils.ReAllocateIfNeeded(ref rtBlurX, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AK_BlurX");
            RenderingUtils.ReAllocateIfNeeded(ref rtTFM, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AK_TFM");

            var finalDesc = renderingData.cameraData.cameraTargetDescriptor;
            finalDesc.depthBufferBits = 0;
            finalDesc.msaaSamples = 1;
            RenderingUtils.ReAllocateIfNeeded(ref rtFinal, finalDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AK_Final");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Skip unsupported cameras (fixes errors when inspecting the material)
            var camData = renderingData.cameraData;
            var camType = camData.cameraType;
            if (camData.isPreviewCamera || camType == CameraType.Reflection || camType == CameraType.Preview)
                return;

            // Sync material reference in case it changed in the Inspector
            mat = settings.Material;
            if (mat == null) return;

            var src = camData.renderer.cameraColorTargetHandle;
            if (src.rt == null) return;                         // source must exist
            if (rtEigen == null || rtBlurX == null || rtTFM == null || rtFinal == null) return; // targets 

            var cmd = CommandBufferPool.Get(profilerTag);

            // set shared uniforms
            var w = (float)renderingData.cameraData.cameraTargetDescriptor.width;
            var h = (float)renderingData.cameraData.cameraTargetDescriptor.height;
            cmd.SetGlobalVector(_MainTex_TexelSize_ID, new Vector4(1.0f / w, 1.0f / h, w, h));
            cmd.SetGlobalInt(_KernelSize_ID, settings.KernelSize);
            cmd.SetGlobalInt(_N_ID, settings.N);
            cmd.SetGlobalFloat(_Hardness_ID, settings.Hardness);
            cmd.SetGlobalFloat(_Q_ID, settings.Q);
            cmd.SetGlobalFloat(_Alpha_ID, settings.Alpha);
            cmd.SetGlobalFloat(_ZeroCrossing_ID, settings.ZeroCrossing);
            cmd.SetGlobalFloat(_Zeta_ID, settings.Zeta);

            // --- Pass 0: source -> Eigen
            Blitter.BlitCameraTexture(cmd, src, rtEigen, mat, 0); // "Eigen"

            // --- Pass 1: Eigen -> BlurX
            Blitter.BlitCameraTexture(cmd, rtEigen, rtBlurX, mat, 1); // "BlurX"

            // --- Pass 2: BlurX -> TFM
            Blitter.BlitCameraTexture(cmd, rtBlurX, rtTFM, mat, 2);   // "BlurY_TFM"

            // --- Pass 3: (source + TFM) -> Final
            cmd.SetGlobalTexture(_TFM_ID, rtTFM);
            Blitter.BlitCameraTexture(cmd, src, rtFinal, mat, 3);     // "Kuwahara"

            // Copy back to camera color
            Blitter.BlitCameraTexture(cmd, rtFinal, src);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // nothing per-camera; RTHandles live until feature is disposed
        }

        public void Dispose()
        {
            rtEigen?.Release();
            rtBlurX?.Release();
            rtTFM?.Release();
            rtFinal?.Release();
        }
    }

    public Settings settings = new Settings();
    Pass pass;

    public override void Create()
    {
        pass = new Pass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // reflect inspector changes
        pass.renderPassEvent = settings.Event;

        if (settings.Material == null) return;
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        pass?.Dispose();
    }
}
