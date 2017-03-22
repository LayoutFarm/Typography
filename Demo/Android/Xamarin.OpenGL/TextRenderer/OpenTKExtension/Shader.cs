using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.ES30;

namespace CSharpGLES
{
    /// <summary>
    /// This is the base class for all shaders (vertex and fragment). It offers functionality
    /// which is core to all shaders, such as file loading and binding.
    /// </summary>
    public class Shader
    {
        public void Create(uint shaderType, string source)
        {
            //  Create the OpenGL shader object.
            ShaderObject = GL.CreateShader((All) shaderType);

            //  Set the shader source.
            GL.ShaderSource(ShaderObject, 1, new[] { source }, new[] { source.Length });

            //  Compile the shader object.
            GL.CompileShader(ShaderObject);

            //  Now that we've compiled the shader, check it's compilation status. If it's not compiled properly, we're
            //  going to throw an exception.
            if (GetCompileStatus() == false)
            {
                string log = GetInfoLog();
                throw new ShaderCompilationException(string.Format("Failed to compile shader with ID {0}. Log: {1}", ShaderObject, log), log);
            }
        }

        public void Delete()
        {
            GL.DeleteShader(ShaderObject);
            ShaderObject = 0;
        }

        public bool GetCompileStatus()
        {
            int[] parameters = new int[] { 0 };
            GL.GetShader(ShaderObject, All.CompileStatus, parameters);
            return parameters[0] == (int)All.True;
        }

        public string GetInfoLog()
        {
            int[] infoLength = new int[] { 0 };
            GL.GetShader(ShaderObject, All.InfoLogLength, infoLength);
            int bufSize = infoLength[0];

            StringBuilder il = new StringBuilder(bufSize);
            GL.GetShaderInfoLog(ShaderObject, bufSize, infoLength, il);

            string log = il.ToString();
            return log;
        }

        /// <summary>
        /// Gets the shader object.
        /// </summary>
        public int ShaderObject { get; protected set; }
    }
}
