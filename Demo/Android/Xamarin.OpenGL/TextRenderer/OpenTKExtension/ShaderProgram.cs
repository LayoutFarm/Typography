//MIT, 2015-2017, bitzhuwei, https://github.com/bitzhuwei/CSharpGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.ES30;

namespace CSharpGLES
{
    public class ShaderProgram
    {
        private readonly Shader vertexShader = new Shader();
        private readonly Shader fragmentShader = new Shader();

        /// <summary>
        /// Creates the shader program.
        /// </summary>
        /// <param name="vertexShaderSource">The vertex shader source.</param>
        /// <param name="fragmentShaderSource">The fragment shader source.</param>
        /// <param name="attributeLocations">The attribute locations. This is an optional array of
        /// uint attribute locations to their names.</param>
        /// <exception cref="ShaderCompilationException"></exception>
        public void Create(string vertexShaderSource, string fragmentShaderSource,
            Dictionary<int, string> attributeLocations)
        {
            //  Create the shaders.
            vertexShader.Create((int)All.VertexShader, vertexShaderSource);
            fragmentShader.Create((int)All.FragmentShader, fragmentShaderSource);

            //  Create the program, attach the shaders.
            ShaderProgramObject = GL.CreateProgram();
            GL.AttachShader(ShaderProgramObject, vertexShader.ShaderObject);
            GL.AttachShader(ShaderProgramObject, fragmentShader.ShaderObject);

            //  Before we link, bind any vertex attribute locations.
            if (attributeLocations != null)
            {
                foreach (var vertexAttributeLocation in attributeLocations)
                    GL.BindAttribLocation(ShaderProgramObject, vertexAttributeLocation.Key, vertexAttributeLocation.Value);
            }

            //  Now we can link the program.
            GL.LinkProgram(ShaderProgramObject);

            //  Now that we've compiled and linked the shader, check it's link status. If it's not linked properly, we're
            //  going to throw an exception.
            if (GetLinkStatus() == false)
            {
                string log = this.GetInfoLog();
                throw new ShaderCompilationException(
                    string.Format("Failed to link shader program with ID {0}. Log: {1}", ShaderProgramObject, log), 
                    log);
            }
            if (vertexShader.GetCompileStatus() == false)
            {
                string log = vertexShader.GetInfoLog();
                throw new Exception(log);
            }
            if (fragmentShader.GetCompileStatus() == false)
            {
                string log = fragmentShader.GetInfoLog();
                throw new Exception(log);
            }

            GL.DetachShader(ShaderProgramObject, vertexShader.ShaderObject);
            GL.DetachShader(ShaderProgramObject, fragmentShader.ShaderObject);
            vertexShader.Delete();
            fragmentShader.Delete();
        }

        public void Delete()
        {
            //GL.DetachShader(ShaderProgramObject, vertexShader.ShaderObject);
            //GL.DetachShader(ShaderProgramObject, fragmentShader.ShaderObject);
            //vertexShader.Delete();
            //fragmentShader.Delete();
            GL.DeleteProgram(ShaderProgramObject);
            ShaderProgramObject = 0;
        }

        public uint GetAttributeLocation(string attributeName)
        {
            //  If we don't have the attribute name in the dictionary, get it's
            //  location and add it.
            if (attributeNamesToLocations.ContainsKey(attributeName) == false)
            {
                StringBuilder attri = new StringBuilder(attributeName);
                int location = GL.GetAttribLocation(ShaderProgramObject, attri);
                if (location < 0) { throw new Exception(); }

                attributeNamesToLocations[attributeName] = (uint)location;
            }

            //  Return the attribute location.
            return attributeNamesToLocations[attributeName];
        }

        public void BindAttributeLocation(int location, string attribute)
        {
            GL.BindAttribLocation(ShaderProgramObject, location, attribute);
        }

        public void Bind()
        {
            GL.UseProgram(ShaderProgramObject);
        }

        public void Unbind()
        {
            GL.UseProgram(0);
        }

        public bool GetLinkStatus()
        {
            int[] parameters = new int[] { 0 };
            GL.GetProgram(ShaderProgramObject, All.LinkStatus, parameters);
            return parameters[0] == (int)All.True;
        }

        public string GetInfoLog()
        {
            //  Get the info log length.
            int[] infoLength = new int[] { 0 };
            GL.GetProgram(ShaderProgramObject, All.InfoLogLength, infoLength);
            int bufSize = infoLength[0];

            //  Get the compile info.
            StringBuilder il = new StringBuilder(bufSize);
            GL.GetProgramInfoLog(ShaderProgramObject, bufSize, infoLength, il);

            string log = il.ToString();
            return log;
        }
        
        public void SetUniform(string uniformName, int v1)
        {
            GL.Uniform1(GetUniformLocation(uniformName), v1);
        }
        
        public void SetUniform(string uniformName, int v1, int v2)
        {
            GL.Uniform2(GetUniformLocation(uniformName), v1, v2);
        }

        public void SetUniform(string uniformName, int v1, int v2, int v3)
        {
            GL.Uniform3(GetUniformLocation(uniformName), v1, v2, v3);
        }
        
        public void SetUniform(string uniformName, int v1, int v2, int v3, int v4)
        {
            GL.Uniform4(GetUniformLocation(uniformName), v1, v2, v3, v4);
        }

        public void SetUniform(string uniformName, float v1)
        {
            GL.Uniform1(GetUniformLocation(uniformName), v1);
        }
        
        public void SetUniform(string uniformName, float v1, float v2)
        {
            GL.Uniform2(GetUniformLocation(uniformName), v1, v2);
        }

        public void SetUniform(string uniformName, float v1, float v2, float v3)
        {
            GL.Uniform3(GetUniformLocation(uniformName), v1, v2, v3);
        }

        public void SetUniform(string uniformName, float v1, float v2, float v3, float v4)
        {
            GL.Uniform4(GetUniformLocation(uniformName), v1, v2, v3, v4);
        }
        
        public void SetUniformMatrix3(string uniformName, float[] m)
        {
            GL.UniformMatrix3(GetUniformLocation(uniformName), 1, false, m);
        }

        public void SetUniformMatrix4(string uniformName, float[] m)
        {
            GL.UniformMatrix4(GetUniformLocation(uniformName), 1, false, m);
        }

        public int GetUniformLocation(string uniformName)
        {
            //  If we don't have the uniform name in the dictionary, get it's
            //  location and add it.
            if (uniformNamesToLocations.ContainsKey(uniformName) == false)
            {
                StringBuilder unif = new StringBuilder(uniformName);
                uniformNamesToLocations[uniformName] = GL.GetUniformLocation(ShaderProgramObject, unif);
                //  TODO: if it's not found, we should probably throw an exception.
            }

            //  Return the uniform location.
            return uniformNamesToLocations[uniformName];
        }

        /// <summary>
        /// Gets the shader program object.
        /// </summary>
        /// <value>
        /// The shader program object.
        /// </value>
        public int ShaderProgramObject { get; protected set; }


        /// <summary>
        /// A mapping of uniform names to locations. This allows us to very easily specify
        /// uniform data by name, quickly looking up the location first if needed.
        /// </summary>
        private readonly Dictionary<string, int> uniformNamesToLocations = new Dictionary<string, int>();

        /// <summary>
        /// A mapping of attribute names to locations. This allows us to very easily specify
        /// attribute data by name, quickly looking up the location first if needed.
        /// </summary>
        private readonly Dictionary<string, uint> attributeNamesToLocations = new Dictionary<string, uint>();
    }
}
