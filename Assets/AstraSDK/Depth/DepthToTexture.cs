using UnityEngine;
using UnityEngine.Assertions;

/*
 * This script bakes depth data from Astra onto a RenderTexture
 * so that it may be used for VFX Graph
 * Please connect DepthToTexture.compute and DepthToTexture.renderTexture
 * to this script in the inspector to use it
 */

public class DepthToTexture : MonoBehaviour
{
    public RenderTexture _depthMap;
    public ComputeShader _computeShader;
    public int cutOff = 10000;

    private RenderTexture _tempDepthMap;
    private int[] _shaderThreads = { 8, 8, 1 };
    private int height, width;
    private long _lastFrameIndex = -1;
    private short[] _depthFrameData;
    private float[] _depthFrameDataFloat;
    private ComputeBuffer _depthBuffer;

    private void Start()
    {
        Assert.IsTrue(AstraController.Instance.DepthEnabled);
        // subscribe to new frame events
        AstraController.Instance.OnDepthFrameEvent += OnNewDepthFrame;
        width = AstraConstants.Width;
        height = AstraConstants.Height;
        InitRenderTextures();
        _depthFrameData = new short[width * height];
        _depthFrameDataFloat = new float[width * height];
        _depthBuffer = new ComputeBuffer(width * height, sizeof(float));
    }

    public void OnNewDepthFrame(Astra.DepthFrame frame)
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
        frame.CopyData(ref _depthFrameData);
        // unfortunately we have to convert from short[] to float[]
        // as compute shaders don't support short data type
        for(int i = 0; i < _depthFrameDataFloat.Length; i++)
        {
            _depthFrameDataFloat[i] = _depthFrameData[i];
        }
        _depthBuffer.SetData(_depthFrameDataFloat);

        RunShader();
        BakeShaderData();
    }

    private void InitRenderTextures()
    {
        _tempDepthMap = new RenderTexture(
            width,
            height,
            0,
            RenderTextureFormat.ARGB32);
        _tempDepthMap.enableRandomWrite = true;
        _tempDepthMap.Create();
    }

    private void RunShader()
    {
        int kernelHandle = _computeShader.FindKernel("DepthToTexture");
        _computeShader.SetFloat("width", width);
        _computeShader.SetFloat("height", height);
        _computeShader.SetTexture(kernelHandle, "DepthMap", _tempDepthMap);
        _computeShader.SetBuffer(kernelHandle, "DepthBuffer", _depthBuffer);
        _computeShader.SetFloat("maxDistance", AstraConstants.MaxDistance);
        _computeShader.SetFloat("cutOff", cutOff);
        _computeShader.SetInt("bufferLength", _depthBuffer.count);
        _computeShader.Dispatch(
            kernelHandle,
            width / _shaderThreads[0],
            height / _shaderThreads[1],
            _shaderThreads[2]);
    }

    private void BakeShaderData()
    {
        // We can't directly bake these external render textures due to
        // lack of random-write flag, so temporarily bake to the internal
        // render textures.
        Graphics.CopyTexture(_tempDepthMap, _depthMap);
    }

    private void OnDisable()
    {
        AstraController.Instance.OnDepthFrameEvent -= OnNewDepthFrame;
        if (_tempDepthMap != null) { Object.Destroy(_tempDepthMap); }
        if (_depthBuffer != null) { _depthBuffer.Dispose(); }
    }

}
