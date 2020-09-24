using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUInstancingSpawner : MonoBehaviour
{
    #region Public
    [Header("Spawn Settings")] 
    [Range(0,1023)]
    public int amount;

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
    #endregion

    private void Start()
    {
        _sharedMaterial = new Material(gpuInstancedShader);
        _sharedMaterial.enableInstancing = true;
        
        _matrices = new Matrix4x4[amount];
        _color = new Vector4[amount];
        
        _block = new MaterialPropertyBlock();
        
        for (int i = 0; i < amount; i++)
        {
            Vector3 position = Vector3.zero;;
            position.x = Random.Range(-10, 10);
            position.y= Random.Range(-10, 10);
            position.z = Random.Range(-10, 10);
            
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
            
            _matrices[i] = matrix;
        }
    }

    private void Update()
    {
        //break your batch here for more than 1023 instances
        //remember instance amount depends of target device and gpu.
        
        if (animate)
        {
            for (int i = 0; i < amount; i++)
            {
                Vector3 position = _matrices[i].GetColumn(3);
                position.x += Mathf.Sin(Time.time * Random.Range(-1f, 1f)) * 0.01f;
                position.y += Mathf.Sin(Time.time * Random.Range(-1f, 1f)) * 0.01f;
                position.z += Mathf.Sin(Time.time * Random.Range(-1f, 1f)) * 0.01f;
                
                //Rotation Column 2 and 1
                //Scale Column 0 1 2 magnitude
            
                Matrix4x4 matrix =  Matrix4x4.identity;
                matrix.SetTRS(position, Quaternion.Euler(Vector3.zero), Vector3.one);
            
                _matrices[i] = matrix;
            }
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
                    Graphics.DrawMeshInstanced(meshToSpawn,0,  _sharedMaterial, _matrices, amount, _block);
                    break;
        }
    }
}
