using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParticlePos : MonoBehaviour
{
    public RenderTexture _positionMap;
    public ComputeShader _computeShader;
    private int _mapWidth = 256;
    private int _mapHeight = 256;
    private int[] _shaderThreads = { 8, 8, 1 };

    void Start()
    {
        InitRenderTextures();
        RunShader();
    }

    void Update()
    {
        
    }

    public void Dispose()
    {
        if (_positionMap)
        {
            Object.Destroy(_positionMap);
        }
    }

    private void InitRenderTextures()
    {
        if (!_positionMap)
        {
            _positionMap = new RenderTexture(_mapWidth, _mapHeight, 1);
            _positionMap.enableRandomWrite = true;
            _positionMap.Create();
        } else
        {
            _positionMap.enableRandomWrite = true;
        }
    }

    private void RunShader()
    {
        int kernelHandle = _computeShader.FindKernel("SetParticlePos");
        _computeShader.SetTexture(kernelHandle, "PositionMap", _positionMap);
        _computeShader.Dispatch(
            kernelHandle, _mapWidth / _shaderThreads[0],
            _mapHeight / _shaderThreads[1],
            _shaderThreads[2]);
    }

    private void RetrieveShaderData()
    {

    }
}
