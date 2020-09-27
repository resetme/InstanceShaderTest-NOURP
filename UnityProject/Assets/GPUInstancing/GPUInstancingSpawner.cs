using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUInstancingSpawner : MonoBehaviour
{
    #region Public
    [Header("Spawn Settings")] 
    [Range(0,100000)]
    public int amount;

    public float distanceX, distanceY, distanceZ  = 10;

    public bool animate = false;
    
    [Header("Mesh")]
    public Mesh meshToSpawn;
    
    [Header("Render")]
    public Shader gpuInstancedShader;
    
    [Header("Type")]
    public INSTANCETYPE _instancetype;
    
    public enum INSTANCETYPE
    {
        DrawMesh, MeshInstanced
    }

    #endregion
    
    #region Private
    private Material _sharedMaterial;
    private Matrix4x4[] _matrices;
    private MaterialPropertyBlock _block;
    private int _shader_color = Shader.PropertyToID("_Color");
    private Vector4[] _color;
    private const int MAXAMOUNT = 1023;
    private int _currAmount = 0;
    private Vector3[] _currPos;
    #endregion

    private void Start()
    {
        _currAmount = amount;
        _sharedMaterial = new Material(gpuInstancedShader);
        _sharedMaterial.enableInstancing = true;
        
        _matrices = new Matrix4x4[_currAmount];
        _color = new Vector4[_currAmount];
        _currPos = new Vector3[_currAmount];
        
        _block = new MaterialPropertyBlock();
        
        for (int i = 0; i < _currAmount; i++)
        {
            Vector3 position = Vector3.zero;;
            position.x = Random.Range(-1f, 1f) * distanceX;
            position.y = Random.Range(-1f, 1f) * distanceY;
            position.z = Random.Range(-1f, 1f) * distanceZ;
            
            Matrix4x4 matrix =  Matrix4x4.identity;
            matrix.SetTRS(position, Quaternion.Euler(Vector3.zero), Vector3.one);

            float r = Random.Range(0f, 1f);
            float g = Random.Range(0f, 1f);
            float b = Random.Range(0f, 1f);
            _color[i] = new Vector4(r,g,b,1);  
            
            switch (_instancetype)
            {
                case INSTANCETYPE.MeshInstanced:
                    _block.SetVectorArray(_shader_color, _color);
                    break;
            }

            _currPos[i] = position;
            _matrices[i] = matrix;
        }
    }

    private int _indexGPUBath = 0;
    
    private void Update()
    {
        //break your batch here for more than 1023 instances
        //remember instance amount depends of target device and gpu.
        
        if (animate)
        {
            for (int i = 0; i < _currAmount; i++)
            {
                float offset = (float)i / _currAmount;
                Vector3 position = _currPos[i];
                position.x += Mathf.Sin(Time.time * offset) * (0.1f * offset);
                position.y += 0;//Mathf.Cos(Time.time ) * 0.01f;
                position.z += Mathf.Cos(Time.time * offset) * (0.1f * offset);
                
                //Rotation Column 2 and 1
                //Scale Column 0 1 2 magnitude
            
                Matrix4x4 matrix =  Matrix4x4.identity;
                matrix.SetTRS(position, Quaternion.Euler(Vector3.zero), Vector3.one);
            
                _matrices[i] = matrix;
                _currPos[i] = position;
            }
        }
        
        //Distance
        for (int i = 0; i < _currAmount; i++)
        {
            Vector3 position = _currPos[i];
            position.x *= distanceX;
            position.y *= distanceY;
            position.z *= distanceZ;
                
            //Rotation Column 2 and 1
            //Scale Column 0 1 2 magnitude
            
            Matrix4x4 matrix =  Matrix4x4.identity;
            matrix.SetTRS(position, Quaternion.Euler(Vector3.zero), Vector3.one);
            
            _matrices[i] = matrix;
        }

        switch (_instancetype)
        {
                case INSTANCETYPE.DrawMesh:
                    for (int i = 0; i < amount; i++)
                    {
                        _block.SetColor(_shader_color, _color[i]);
                        Graphics.DrawMesh(meshToSpawn, _matrices[i], _sharedMaterial, 0, null, 0, _block);
                    }

                    break;
                case INSTANCETYPE.MeshInstanced:
                    //limit of 1023
                    //batch here
                    if (_currAmount > MAXAMOUNT)
                        return;
                    
                    Graphics.DrawMeshInstanced(meshToSpawn,0,  _sharedMaterial, _matrices, amount, _block);
                    break;
        }
    }
}
