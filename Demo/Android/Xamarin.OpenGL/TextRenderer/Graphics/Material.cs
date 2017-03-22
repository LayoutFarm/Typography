using OpenTK.Graphics.ES30;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CSharpGLES;
using Xamarin.OpenGL;

namespace Xamarin.OpenGL
{
    class Material
    {
        public static Material m = new Material(
            vertexShader: @"
#version 300 es
uniform mat4 ProjMtx;
in vec2 Position;
in vec2 UV;
in vec4 Color;
out vec2 Frag_UV;
out vec4 Frag_Color;
void main()
{
	Frag_UV = UV;
	Frag_Color = Color;
	gl_Position = ProjMtx * vec4(Position.xy,0,1);
}
",
            fragmentShader: @"
#version 300 es
uniform sampler2D Texture;
in vec2 Frag_UV;
in vec4 Frag_Color;
out vec4 Out_Color;
void main()
{
	Out_Color = Frag_Color;
}
"
            );

        public static Material mExtra = new Material(
            vertexShader: @"
#version 300 es
uniform mat4 ProjMtx;

in vec4 Position;
in vec2 UV;
in vec4 Color;

out vec2 Frag_UV;
out vec4 Frag_Color;

void main()
{
	Frag_UV = UV;
	Frag_Color = Color;
	gl_Position = ProjMtx * vec4(Position.xy,0,1);
}
",
            fragmentShader: @"
#version 300 es
in vec2 Frag_UV;
in vec4 Frag_Color;

out vec4 Out_Color;

float inCurve(vec2 uv)
{
	return uv.x * uv.x - uv.y;
}

void main()
{
	float x = inCurve(Frag_UV);

	if(!gl_FrontFacing)
	{
		if (x > 0.) discard;
	}
	else
	{
		if (x < 0.) discard;
	}

	Out_Color = Frag_Color;
}
"
            );

        public static void InitMaterials()
        {
            m.Init();
            mExtra.Init();
        }

        public static void DestroyMaterials()
        {
            m.ShutDown();
            mExtra.ShutDown();
        }

        string vertexShaderSource;
        string fragmentShaderSource;

        public readonly uint[] buffers = { 0, 0 };
        public uint positionVboHandle/*buffers[0]*/, elementsHandle/*buffers[1]*/;

        public readonly uint[] vertexArray = { 0 };
        public uint vaoHandle;

        public uint attributePositon, attributeUV, attributeColor;

        public ShaderProgram program;
        public Dictionary<int, string> attributeMap;

        public readonly uint[] textures = { 0 };

        public Material(string vertexShader, string fragmentShader)
        {
            this.vertexShaderSource = vertexShader;
            this.fragmentShaderSource = fragmentShader;
        }

        public void Init()
        {
            this.CreateShaders();
            this.CreateVBOs();
        }

        public void ShutDown()
        {
            this.DeleteShaders();
            this.DeleteVBOs();
        }

        private void CreateShaders()
        {
            program = new ShaderProgram();
            attributePositon = 0;
            attributeUV = 1;
            attributeColor = 2;
            attributeMap = new Dictionary<int, string>
            {
                {0, "Position"},
                {1, "UV"},
                {2, "Color" }
            };
            program.Create(vertexShaderSource, fragmentShaderSource, attributeMap);

            Utility.CheckGLESError();
        }

        private void CreateVBOs()
        {
            GL.GenBuffers(2, buffers);
            positionVboHandle = buffers[0];
            elementsHandle = buffers[1];
            GL.BindBuffer(All.ArrayBuffer, positionVboHandle);
            GL.BindBuffer(All.ElementArrayBuffer, elementsHandle);

            GL.GenVertexArrays(1, vertexArray);
            vaoHandle = vertexArray[0];
            GL.BindVertexArray(vaoHandle);

            GL.BindBuffer(All.ArrayBuffer, positionVboHandle);

            GL.EnableVertexAttribArray(attributePositon);
            GL.EnableVertexAttribArray(attributeUV);
            GL.EnableVertexAttribArray(attributeColor);

            GL.VertexAttribPointer(attributePositon, 2, All.Float, false, Marshal.SizeOf<DrawVertex>(), Marshal.OffsetOf<DrawVertex>("pos"));
            GL.VertexAttribPointer(attributeUV, 2, All.Float, false, Marshal.SizeOf<DrawVertex>(), Marshal.OffsetOf<DrawVertex>("uv"));
            GL.VertexAttribPointer(attributeColor, 4, All.Float, true, Marshal.SizeOf<DrawVertex>(), Marshal.OffsetOf<DrawVertex>("color"));

            Utility.CheckGLESError();
        }

        private void DeleteShaders()
        {
            if (program != null)
            {
                program.Unbind();
                Utility.CheckGLESError();
                program.Delete();
                Utility.CheckGLESError();
                program = null;
            }
        }

        private void DeleteVBOs()
        {
            GL.DeleteBuffers(1, buffers);
            Utility.CheckGLESError();

            GL.BindBuffer(All.ArrayBuffer, 0);
            Utility.CheckGLESError();
        }

    }
}
