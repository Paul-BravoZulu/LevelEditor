﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TestProject;
using System.IO;
using System;

public class test2 : MonoBehaviour
{
    alien_level Result = new alien_level();

    void Start()
    {
        string levelPath = @"G:\SteamLibrary\steamapps\common\Alien Isolation\DATA\ENV\PRODUCTION\BSP_TORRENS";

        Result.ModelsCST = File.ReadAllBytes(levelPath + "/RENDERABLE/LEVEL_MODELS.CST");
        Result.ModelsMTL = TestProject.File_Handlers.Models.ModelsMTL.Load(levelPath + "/RENDERABLE/LEVEL_MODELS.MTL", Result.ModelsCST);
        Result.ModelsBIN = TestProject.File_Handlers.Models.ModelBIN.Load(levelPath + "/RENDERABLE/MODELS_LEVEL.BIN");
        Result.ModelsPAK = TestProject.File_Handlers.Models.ModelPAK.Load(levelPath + "/RENDERABLE/LEVEL_MODELS.PAK");
    }

    private GameObject LoadModel(int EntryIndex)
    {
        alien_pak_model_entry ChunkArray = Result.ModelsPAK.Models[EntryIndex];

        GameObject ThisModel = new GameObject();

        for (int ChunkIndex = 0; ChunkIndex < ChunkArray.Header.ChunkCount; ++ChunkIndex)
        {
            int BINIndex = ChunkArray.ChunkInfos[ChunkIndex].BINIndex;
            alien_model_bin_model_info Model = Result.ModelsBIN.Models[BINIndex];
            if (Model.BlockSize == 0) continue;

            alien_vertex_buffer_format VertexInput = Result.ModelsBIN.VertexBufferFormats[Model.VertexFormatIndex];
            alien_vertex_buffer_format VertexInputLowDetail = Result.ModelsBIN.VertexBufferFormats[Model.VertexFormatIndexLowDetail];

            BinaryReader Stream = new BinaryReader(new MemoryStream(ChunkArray.Chunks[ChunkIndex]));

            //dunno if this bit is right
            int VertexArrayCount = 1;
            List<alien_vertex_buffer_format_element> Elements = VertexInput.Elements; 
            List<int> ElementCounts = new List<int>(new int[VertexInput.Elements.Count]);
            for (int ElementIndex = 0; ElementIndex < VertexInput.ElementCount; ++ElementIndex)
            {
                alien_vertex_buffer_format_element Element = VertexInput.Elements[ElementIndex];
                if (VertexArrayCount - 1 != Element.ArrayIndex)
                {
                    Elements[VertexArrayCount++] = Element;
                }
                ElementCounts[VertexArrayCount - 1]++;
            }
            //--

            List<UInt16> InIndices = new List<UInt16>();
            List<Vector3> InVertices = new List<Vector3>();
            List<Vector3> InNormals = new List<Vector3>();

            for (int VertexArrayIndex = 0; VertexArrayIndex < VertexArrayCount; ++VertexArrayIndex)
            {
                int ElementCount = ElementCounts[VertexArrayIndex];
                alien_vertex_buffer_format_element Inputs = Elements[VertexArrayIndex];
                if (Inputs.ArrayIndex == 0xFF) InIndices = Utilities.ConsumeArray<UInt16>(Stream, Model.IndexCount);
                else
                {
                    for (int VertexIndex = 0; VertexIndex < Model.VertexCount; ++VertexIndex)
                    {
                        for (int ElementIndex = 0; ElementIndex < ElementCount; ++ElementIndex)
                        {
                            alien_vertex_buffer_format_element Input = Elements[VertexArrayIndex + ElementIndex];
                            switch (Input.VariableType)
                            {
                                case alien_vertex_input_type.AlienVertexInputType_v3:
                                    {
                                        V3 Value = Utilities.Consume<V3>(Stream);
                                        switch (Input.ShaderSlot)
                                        {
                                            case alien_vertex_input_slot.AlienVertexInputSlot_N:
                                                InNormals.Add(new Vector3(Value.x, Value.y, Value.z));
                                                break;
                                            case alien_vertex_input_slot.AlienVertexInputSlot_T:
                                                break;
                                            case alien_vertex_input_slot.AlienVertexInputSlot_UV:
                                                break;
                                        };
                                        break;
                                    }

                                case alien_vertex_input_type.AlienVertexInputType_u32_C:
                                    {
                                        int Value = Stream.ReadInt32();
                                        switch (Input.ShaderSlot)
                                        {
                                            case alien_vertex_input_slot.AlienVertexInputSlot_C:
                                                break;
                                        }
                                        break;
                                    }

                                case alien_vertex_input_type.AlienVertexInputType_v4u8_i:
                                    {
                                        V4 Value = new V4(Stream.ReadBytes(4));
                                        switch (Input.ShaderSlot)
                                        {
                                            case alien_vertex_input_slot.AlienVertexInputSlot_BI:
                                                break;
                                        }
                                        break;
                                    }

                                case alien_vertex_input_type.AlienVertexInputType_v4u8_f:
                                    {
                                        V4 Value = new V4(Stream.ReadBytes(4));
                                        Value = Value / 255.0f;

                                        switch (Input.ShaderSlot)
                                        {
                                            case alien_vertex_input_slot.AlienVertexInputSlot_BW:
                                                break;
                                            case alien_vertex_input_slot.AlienVertexInputSlot_UV:
                                                break;
                                        }
                                        break;
                                    }

                                case alien_vertex_input_type.AlienVertexInputType_v2s16_UV:
                                    {
                                        List<Int16> Values = Utilities.ConsumeArray<Int16>(Stream, 2);
                                        V2 Value = new V2(Values[0], Values[1]);
                                        Value = Value / 2048.0f;

                                        switch (Input.ShaderSlot)
                                        {
                                            case alien_vertex_input_slot.AlienVertexInputSlot_UV:
                                                break;
                                        }
                                        break;
                                    }

                                case alien_vertex_input_type.AlienVertexInputType_v4s16_f:
                                    {
                                        List<Int16> Values = Utilities.ConsumeArray<Int16>(Stream, 4);
                                        V4 Value = new V4(Values[0], Values[1], Values[2], Values[3]);
                                        Value = Value / (float)Int16.MaxValue;

                                        switch (Input.ShaderSlot)
                                        {
                                            case alien_vertex_input_slot.AlienVertexInputSlot_P:
                                                InVertices.Add(new Vector3(Value.x, Value.y, Value.z));
                                                break;
                                        }
                                        break;
                                    }

                                case alien_vertex_input_type.AlienVertexInputType_v2s16_f:
                                    {
                                        V4 Value = new V4(Stream.ReadBytes(4));
                                        Value = Value / (float)Byte.MaxValue - 0.5f;
                                        Value.Normalise();

                                        switch (Input.ShaderSlot)
                                        {
                                            case alien_vertex_input_slot.AlienVertexInputSlot_N:
                                                InNormals.Add(new Vector3(Value.x, Value.y, Value.z));
                                                break;
                                            case alien_vertex_input_slot.AlienVertexInputSlot_T:
                                                break;
                                            case alien_vertex_input_slot.AlienVertexInputSlot_B:
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
                Utilities.Align(Stream, 16);
            }

            GameObject ThisModelPart = new GameObject();
            ThisModelPart.transform.parent = ThisModel.transform;
            ThisModel.name = Result.ModelsBIN.ModelFilePaths[BINIndex];
            ThisModelPart.name = Result.ModelsBIN.ModelLODPartNames[BINIndex]; //huh? aren't there multiple part names?

            if (InVertices.Count == 0) continue;

            int maxind = -int.MaxValue;
            for (int i = 0; i < InIndices.Count; i++)
            {
                //Debug.Log(indiciesConv[i]);
                if (InIndices[i] > maxind) maxind = InIndices[i];
            }

            Debug.Log("Indices: " + InIndices.Count + ", Vertices: " + InVertices.Count + ", MaxInd: " + maxind);
            Debug.Log("---");

            Mesh thisMesh = new Mesh();
            thisMesh.SetVertices(InVertices);
            thisMesh.SetNormals(InNormals);
            thisMesh.SetIndices(InIndices, MeshTopology.Triangles, 0); //0??
            thisMesh.RecalculateBounds();
            thisMesh.RecalculateNormals();
            thisMesh.RecalculateTangents();
            ThisModelPart.AddComponent<MeshFilter>().mesh = thisMesh;
            ThisModelPart.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

        }

        return ThisModel;
    }

    // Update is called once per frame
    GameObject currentMesh = null;
    int currentMeshIndex = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (currentMesh != null) Destroy(currentMesh);
            currentMesh = LoadModel(currentMeshIndex);
            currentMeshIndex++;
        }
    }
}
