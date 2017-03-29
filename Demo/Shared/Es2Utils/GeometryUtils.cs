//
// Copyright (c) 2014 The ANGLE Project Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
//

//            Based on Hello_Triangle.c from
// Book:      OpenGL(R) ES 2.0 Programming Guide
// Authors:   Aaftab Munshi, Dan Ginsburg, Dave Shreiner
// ISBN-10:   0321502795
// ISBN-13:   9780321502797
// Publisher: Addison-Wesley Professional
// URLs:      http://safari.informit.com/9780321563835
//            http://www.opengles-book.com


#region Using Directives

using System;
using System.Collections.Generic;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
#endregion


namespace OpenTK.Graphics.ES20
{
    public class CubeGeometry
    {
        public Vector3[] positions;
        public Vector3[] normals;
        public Vector2[] texcoords;
        public ushort[] indices;
    }

    public static class GeometryUtils
    {
        static Vector3 MakeV3(float x, float y, float z) { return new Vector3(x, y, z); }
        static Vector2 MakeV2(float x, float y) { return new Vector2(x, y); }
        public static void GenerateCubeGeometry(float radius, CubeGeometry result)
        {
            result.positions = new Vector3[24];
            result.positions[0] = MakeV3(-radius, -radius, -radius);
            result.positions[1] = MakeV3(-radius, -radius, radius);
            result.positions[2] = MakeV3(radius, -radius, radius);
            result.positions[3] = MakeV3(radius, -radius, -radius);
            result.positions[4] = MakeV3(-radius, radius, -radius);
            result.positions[5] = MakeV3(-radius, radius, radius);
            result.positions[6] = MakeV3(radius, radius, radius);
            result.positions[7] = MakeV3(radius, radius, -radius);
            result.positions[8] = MakeV3(-radius, -radius, -radius);
            result.positions[9] = MakeV3(-radius, radius, -radius);
            result.positions[10] = MakeV3(radius, radius, -radius);
            result.positions[11] = MakeV3(radius, -radius, -radius);
            result.positions[12] = MakeV3(-radius, -radius, radius);
            result.positions[13] = MakeV3(-radius, radius, radius);
            result.positions[14] = MakeV3(radius, radius, radius);
            result.positions[15] = MakeV3(radius, -radius, radius);
            result.positions[16] = MakeV3(-radius, -radius, -radius);
            result.positions[17] = MakeV3(-radius, -radius, radius);
            result.positions[18] = MakeV3(-radius, radius, radius);
            result.positions[19] = MakeV3(-radius, radius, -radius);
            result.positions[20] = MakeV3(radius, -radius, -radius);
            result.positions[21] = MakeV3(radius, -radius, radius);
            result.positions[22] = MakeV3(radius, radius, radius);
            result.positions[23] = MakeV3(radius, radius, -radius);
            result.normals = new Vector3[24];
            result.normals[0] = MakeV3(0.0f, -1.0f, 0.0f);
            result.normals[1] = MakeV3(0.0f, -1.0f, 0.0f);
            result.normals[2] = MakeV3(0.0f, -1.0f, 0.0f);
            result.normals[3] = MakeV3(0.0f, -1.0f, 0.0f);
            result.normals[4] = MakeV3(0.0f, 1.0f, 0.0f);
            result.normals[5] = MakeV3(0.0f, 1.0f, 0.0f);
            result.normals[6] = MakeV3(0.0f, 1.0f, 0.0f);
            result.normals[7] = MakeV3(0.0f, 1.0f, 0.0f);
            result.normals[8] = MakeV3(0.0f, 0.0f, -1.0f);
            result.normals[9] = MakeV3(0.0f, 0.0f, -1.0f);
            result.normals[10] = MakeV3(0.0f, 0.0f, -1.0f);
            result.normals[11] = MakeV3(0.0f, 0.0f, -1.0f);
            result.normals[12] = MakeV3(0.0f, 0.0f, 1.0f);
            result.normals[13] = MakeV3(0.0f, 0.0f, 1.0f);
            result.normals[14] = MakeV3(0.0f, 0.0f, 1.0f);
            result.normals[15] = MakeV3(0.0f, 0.0f, 1.0f);
            result.normals[16] = MakeV3(-1.0f, 0.0f, 0.0f);
            result.normals[17] = MakeV3(-1.0f, 0.0f, 0.0f);
            result.normals[18] = MakeV3(-1.0f, 0.0f, 0.0f);
            result.normals[19] = MakeV3(-1.0f, 0.0f, 0.0f);
            result.normals[20] = MakeV3(1.0f, 0.0f, 0.0f);
            result.normals[21] = MakeV3(1.0f, 0.0f, 0.0f);
            result.normals[22] = MakeV3(1.0f, 0.0f, 0.0f);
            result.normals[23] = MakeV3(1.0f, 0.0f, 0.0f);
            result.texcoords = new Vector2[24];
            result.texcoords[0] = MakeV2(0.0f, 0.0f);
            result.texcoords[1] = MakeV2(0.0f, 1.0f);
            result.texcoords[2] = MakeV2(1.0f, 1.0f);
            result.texcoords[3] = MakeV2(1.0f, 0.0f);
            result.texcoords[4] = MakeV2(1.0f, 0.0f);
            result.texcoords[5] = MakeV2(1.0f, 1.0f);
            result.texcoords[6] = MakeV2(0.0f, 1.0f);
            result.texcoords[7] = MakeV2(0.0f, 0.0f);
            result.texcoords[8] = MakeV2(0.0f, 0.0f);
            result.texcoords[9] = MakeV2(0.0f, 1.0f);
            result.texcoords[10] = MakeV2(1.0f, 1.0f);
            result.texcoords[11] = MakeV2(1.0f, 0.0f);
            result.texcoords[12] = MakeV2(0.0f, 0.0f);
            result.texcoords[13] = MakeV2(0.0f, 1.0f);
            result.texcoords[14] = MakeV2(1.0f, 1.0f);
            result.texcoords[15] = MakeV2(1.0f, 0.0f);
            result.texcoords[16] = MakeV2(0.0f, 0.0f);
            result.texcoords[17] = MakeV2(0.0f, 1.0f);
            result.texcoords[18] = MakeV2(1.0f, 1.0f);
            result.texcoords[19] = MakeV2(1.0f, 0.0f);
            result.texcoords[20] = MakeV2(0.0f, 0.0f);
            result.texcoords[21] = MakeV2(0.0f, 1.0f);
            result.texcoords[22] = MakeV2(1.0f, 1.0f);
            result.texcoords[23] = MakeV2(1.0f, 0.0f);
            result.indices = new ushort[36];
            result.indices[0] = 0; result.indices[1] = 2; result.indices[2] = 1;
            result.indices[3] = 0; result.indices[4] = 3; result.indices[5] = 2;
            result.indices[6] = 4; result.indices[7] = 5; result.indices[8] = 6;
            result.indices[9] = 4; result.indices[10] = 6; result.indices[11] = 7;
            result.indices[12] = 8; result.indices[13] = 9; result.indices[14] = 10;
            result.indices[15] = 8; result.indices[16] = 10; result.indices[17] = 11;
            result.indices[18] = 12; result.indices[19] = 15; result.indices[20] = 14;
            result.indices[21] = 12; result.indices[22] = 14; result.indices[23] = 13;
            result.indices[24] = 16; result.indices[25] = 17; result.indices[26] = 18;
            result.indices[27] = 16; result.indices[28] = 18; result.indices[29] = 19;
            result.indices[30] = 20; result.indices[31] = 23; result.indices[32] = 22;
            result.indices[33] = 20; result.indices[34] = 22; result.indices[35] = 21;
        }
    }

    public struct MyMat4
    {
        public float[] data;
        public MyMat4(float m00, float m01, float m02, float m03,
                 float m10, float m11, float m12, float m13,
                 float m20, float m21, float m22, float m23,
                 float m30, float m31, float m32, float m33)
        {
            data = new float[16];
            data[0] = m00; data[4] = m01; data[8] = m02; data[12] = m03;
            data[1] = m10; data[5] = m11; data[9] = m12; data[13] = m13;
            data[2] = m20; data[6] = m21; data[10] = m22; data[14] = m23;
            data[3] = m30; data[7] = m31; data[11] = m32; data[15] = m33;
        }
        public static MyMat4 perspective(float fovY, float aspectRatio, float nearZ, float farZ)
        {
            float frustumHeight = (float)(Math.Tan(fovY / 360.0f * Math.PI) * nearZ);
            float frustumWidth = (float)(frustumHeight * aspectRatio);
            return frustum(-frustumWidth, frustumWidth, -frustumHeight, frustumHeight, nearZ, farZ);
        }
        public static MyMat4 scale(float sx, float sy)
        {
            return new MyMat4(
                sx, 0.0f, 0.0f, 0.0f,
                0.0f, sy, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
                );
        }
        static MyMat4 frustum(float l, float r, float b, float t, float n, float f)
        {
            return new MyMat4((2.0f * n) / (r - l), 0.0f, (r + l) / (r - l), 0.0f,
                                           0.0f, (2.0f * n) / (t - b), (t + b) / (t - b), 0.0f,
                                           0.0f, 0.0f, -(f + n) / (f - n), -(2.0f * f * n) / (f - n),
                                           0.0f, 0.0f, -1.0f, 0.0f);
        }
        public static MyMat4 GetIdentityMat()
        {
            return new MyMat4(1.0f, 0.0f, 0.0f, 0.0f,
                           0.0f, 1.0f, 0.0f, 0.0f,
                           0.0f, 0.0f, 1.0f, 0.0f,
                           0.0f, 0.0f, 0.0f, 1.0f);
        }
        static float Vector3length(ref Vector3 vec)
        {
            float lenSquared = Vector3lengthSquared(ref vec);
            return (lenSquared != 0.0f) ? (float)Math.Pow((lenSquared), 0.5) : 0.0f;
        }

        static float Vector3lengthSquared(ref Vector3 vec)
        {
            return vec.X * vec.X +
                   vec.Y * vec.Y +
                   vec.Z * vec.Z;
        }

        static Vector3 NormalizeVector(Vector3 vec)
        {
            Vector3 ret = new Vector3();
            float len = Vector3length(ref vec);
            if (len != 0.0f)
            {
                float invLen = 1.0f / len;
                ret.X = vec.X * invLen;
                ret.Y = vec.Y * invLen;
                ret.Z = vec.Z * invLen;
            }
            return ret;
        }
        public static MyMat4 rotate(float angle, Vector3 p)
        {
            Vector3 u = NormalizeVector(p);
            float theta = (float)(angle * (Math.PI / 180.0f));
            float cos_t = (float)Math.Cos(theta);
            float sin_t = (float)Math.Sin(theta);
            return new MyMat4(cos_t + (u.X * u.X * (1.0f - cos_t)), (u.X * u.Y * (1.0f - cos_t)) - (u.Z * sin_t), (u.X * u.Z * (1.0f - cos_t)) + (u.Y * sin_t), 0.0f,
                           (u.Y * u.X * (1.0f - cos_t)) + (u.Z * sin_t), cos_t + (u.Y * u.Y * (1.0f - cos_t)), (u.Y * u.Z * (1.0f - cos_t)) - (u.X * sin_t), 0.0f,
                           (u.Z * u.X * (1.0f - cos_t)) - (u.Y * sin_t), (u.Z * u.Y * (1.0f - cos_t)) + (u.X * sin_t), cos_t + (u.Z * u.Z * (1.0f - cos_t)), 0.0f,
                                                                   0.0f, 0.0f, 0.0f, 1.0f);
        }

        public static MyMat4 translate(Vector3 t)
        {
            return new MyMat4(1.0f, 0.0f, 0.0f, t.X,
                              0.0f, 1.0f, 0.0f, t.Y,
                              0.0f, 0.0f, 1.0f, t.Z,
                              0.0f, 0.0f, 0.0f, 1.0f);
        }
        public static MyMat4 ortho(float l, float r, float b, float t, float n, float f)
        {
            return new MyMat4(2.0f / (r - l), 0.0f, 0.0f, -(r + l) / (r - l),
                                     0.0f, 2.0f / (t - b), 0.0f, -(t + b) / (t - b),
                                     0.0f, 0.0f, -2.0f / (f - n), -(f + n) / (f - n),
                                     0.0f, 0.0f, 0.0f, 1.0f);
        }

        public static MyMat4 operator *(MyMat4 a, MyMat4 b)
        {
            return new MyMat4(a.data[0] * b.data[0] + a.data[4] * b.data[1] + a.data[8] * b.data[2] + a.data[12] * b.data[3],
                           a.data[0] * b.data[4] + a.data[4] * b.data[5] + a.data[8] * b.data[6] + a.data[12] * b.data[7],
                           a.data[0] * b.data[8] + a.data[4] * b.data[9] + a.data[8] * b.data[10] + a.data[12] * b.data[11],
                           a.data[0] * b.data[12] + a.data[4] * b.data[13] + a.data[8] * b.data[14] + a.data[12] * b.data[15],
                           a.data[1] * b.data[0] + a.data[5] * b.data[1] + a.data[9] * b.data[2] + a.data[13] * b.data[3],
                           a.data[1] * b.data[4] + a.data[5] * b.data[5] + a.data[9] * b.data[6] + a.data[13] * b.data[7],
                           a.data[1] * b.data[8] + a.data[5] * b.data[9] + a.data[9] * b.data[10] + a.data[13] * b.data[11],
                           a.data[1] * b.data[12] + a.data[5] * b.data[13] + a.data[9] * b.data[14] + a.data[13] * b.data[15],
                           a.data[2] * b.data[0] + a.data[6] * b.data[1] + a.data[10] * b.data[2] + a.data[14] * b.data[3],
                           a.data[2] * b.data[4] + a.data[6] * b.data[5] + a.data[10] * b.data[6] + a.data[14] * b.data[7],
                           a.data[2] * b.data[8] + a.data[6] * b.data[9] + a.data[10] * b.data[10] + a.data[14] * b.data[11],
                           a.data[2] * b.data[12] + a.data[6] * b.data[13] + a.data[10] * b.data[14] + a.data[14] * b.data[15],
                           a.data[3] * b.data[0] + a.data[7] * b.data[1] + a.data[11] * b.data[2] + a.data[15] * b.data[3],
                           a.data[3] * b.data[4] + a.data[7] * b.data[5] + a.data[11] * b.data[6] + a.data[15] * b.data[7],
                           a.data[3] * b.data[8] + a.data[7] * b.data[9] + a.data[11] * b.data[10] + a.data[15] * b.data[11],
                           a.data[3] * b.data[12] + a.data[7] * b.data[13] + a.data[11] * b.data[14] + a.data[15] * b.data[15]);
        }
    }
}