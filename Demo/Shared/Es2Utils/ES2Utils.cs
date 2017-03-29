//
// Copyright (c) 2014 The ANGLE Project Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
//


namespace OpenTK.Graphics.ES20
{
    public static class ES2Utils
    {
        public static int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            int compileResult;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out compileResult);
            if (compileResult == 0)
            {
                int infoLogLength;
                GL.GetShader(shader, ShaderParameter.InfoLogLength, out infoLogLength);
                string infolog;
                GL.GetShaderInfoLog(shader, out infolog);
                GL.DeleteShader(shader);
                //std::vector<GLchar> infoLog(infoLogLength);
                //glGetShaderInfoLog(shader, infoLog.size(), NULL, &infoLog[0]);

                //std::cerr << "shader compilation failed: " << &infoLog[0];

                //glDeleteShader(shader);
                shader = 0;
            }

            return shader;
        }
        public static int CompileProgram(string vs_source, string fs_source)
        {
            int program = GL.CreateProgram();
            int vs = CompileShader(ShaderType.VertexShader, vs_source);
            int fs = CompileShader(ShaderType.FragmentShader, fs_source);
            //GLuint program = glCreateProgram();

            //GLuint vs = CompileShader(GL_VERTEX_SHADER, vsSource);
            //GLuint fs = CompileShader(GL_FRAGMENT_SHADER, fsSource);

            if (vs == 0 || fs == 0)
            {
                GL.DeleteShader(vs);
                GL.DeleteShader(fs);
                GL.DeleteProgram(program);
                return 0;
            }
            GL.AttachShader(program, vs);
            GL.DeleteShader(vs);
            //glAttachShader(program, vs);
            //glDeleteShader(vs);

            GL.AttachShader(program, fs);
            GL.DeleteShader(fs);
            //glAttachShader(program, fs);
            //glDeleteShader(fs);
            GL.LinkProgram(program);
            //glLinkProgram(program);

            int linkStatus;
            GL.GetProgram(program, ProgramParameter.LinkStatus, out linkStatus);
            //GLint linkStatus;
            //glGetProgramiv(program, GL_LINK_STATUS, &linkStatus);

            if (linkStatus == 0)
            {
                //GLint infoLogLength;
                //glGetProgramiv(program, GL_INFO_LOG_LENGTH, &infoLogLength);
                int infoLogLength;
                GL.GetProgram(program, ProgramParameter.InfoLogLength, out infoLogLength);
                string infoLog;
                GL.GetProgramInfoLog(program, out infoLog);
                //std::vector<GLchar> infoLog(infoLogLength);
                //glGetProgramInfoLog(program, infoLog.size(), NULL, &infoLog[0]);

                //std::cerr << "program link failed: " << &infoLog[0];
                GL.DeleteProgram(program);
                //glDeleteProgram(program);
                return 0;
            }

            return program;
        }
    }
}