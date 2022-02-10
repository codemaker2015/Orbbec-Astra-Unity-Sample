using UnityEngine;
using UnityEngine.Assertions;

/*
 * This script bakes colour data from Astra onto a RenderTexture
 * so that it may be used for VFX Graph
 * Please connect ColourToTexture.compute and ColourToTexture.renderTexture
 * to this script in the inspector to use it
 */ 

public class ColourToTexture : MonoBehaviour
{
    public RenderTexture _colorMap;
    public ComputeShader _computeShader;

    private RenderTexture _tempColorMap;
    private byte[] _colorMapData;
    private float[] _colorMapDataFloat;
    private ComputeBuffer _colorMapBuffer;
    private int[] _shaderThreads = { 8, 8, 1 };
    private int height, width;
    private long _lastFrameIndex = -1;
    private const int numColorChannels = 3;

    void Start()
    {
        Assert.IsTrue(AstraController.Instance.ColorEnabled);
        width = AstraConstants.Width;
        height = AstraConstants.Height;
        AstraController.Instance.OnColorFrameEvent += OnNewFrame;
        _colorMapData = new byte[width * height * numColorChannels];
        _colorMapDataFloat = new float[width * height * numColorChannels];
        _colorMapBuffer = new ComputeBuffer(width * height * numColorChannels, sizeof(float));
    }

    void Update()
    {
        AstraController.Instance.OnColorFrameEvent += OnNewFrame;
        InitRenderTextures();
    }

    public void OnNewFrame(Astra.ColorFrame frame)
    {
        if (frame.Width == 0 ||
            frame.Height == 0)
        {
            return;
        }

        if (_lastFrameIndex == frame.FrameIndex)
        {
            return;
        }

        _lastFrameIndex = frame.FrameIndex;
        frame.CopyData(ref _colorMapData);
        // unfortunately we have to convert from byte[] to float[]
        // as compute shaders don't support byte data type
        for (int i = 0; i < _colorMapDataFloat.Length; i++)
        {
            _colorMapDataFloat[i] = _colorMapData[i];
        }
        _colorMapBuffer.SetData(_colorMapDataFloat);
        RunShader();
        BakeShaderData();
    }

    private void RunShader()
    {
        int kernelHandle = _computeShader.FindKernel("ColourToTexture");
        _computeShader.SetFloat("width", width);
        _computeShader.SetFloat("height", height);
        _computeShader.SetInt("bufferLength", _colorMapBuffer.count);
        _computeShader.SetTexture(kernelHandle, "ColorMap", _tempColorMap);
        _computeShader.SetBuffer(kernelHandle, "ColorBuffer", _colorMapBuffer);
        _computeShader.Dispatch(
            kernelHandle,
            width / _shaderThreads[0],
            height / _shaderThreads[1],
            _shaderThreads[2]);
    }

    private void BakeShaderData()
    {
        Graphics.CopyTexture(_tempColorMap, _colorMap);
    }

    private void InitRenderTextures()
    {
        _tempColorMap = new RenderTexture(
            width,
            height,
            0,
            RenderTextureFormat.ARGB32);
        _tempColorMap.enableRandomWrite = true;
        _tempColorMap.Create();
    }

    private void OnDisable()
    {
        AstraController.Instance.OnColorFrameEvent -= OnNewFrame;
        if (_tempColorMap != null) { Object.Destroy(_tempColorMap); }
        if (_colorMapBuffer != null) { _colorMapBuffer.Dispose(); }
    }
}
