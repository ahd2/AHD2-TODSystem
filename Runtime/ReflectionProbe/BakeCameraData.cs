using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AHD2TimeOfDay
{
    public class BakeCameraData : ScriptableObject
    {
        [SerializeField, HideInInspector] public Matrix4x4[] faceMatrices = new Matrix4x4[]
        {
            new Matrix4x4(
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(0.0f, -1.0f, 0.0f, 0.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            ),
            new Matrix4x4(
                new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
                new Vector4(0.0f, -1.0f, 0.0f, 0.0f),
                new Vector4(-1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            ),
            new Matrix4x4(
                new Vector4(-1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            ),
            new Matrix4x4(
                new Vector4(-1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
                new Vector4(0.0f, -1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            ),
            new Matrix4x4(
                new Vector4(-1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, -1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            ),
            new Matrix4x4(
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, -1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            )
        };

        [SerializeField, HideInInspector] public Matrix4x4 PMatrix4X4 = new Matrix4x4(
            new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
            new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            new Vector4(0.0f, 0.0f, 0.0003f, -1.0f),
            new Vector4(0.0f, 0.0f, 0.30009f, 0.0f)
        );
    }
}