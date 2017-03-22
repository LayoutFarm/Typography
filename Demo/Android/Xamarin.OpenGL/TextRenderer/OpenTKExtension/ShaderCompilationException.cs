using System;

namespace CSharpGLES
{
    public class ShaderCompilationException : Exception
    {
        private readonly string compilerOutput;

        public ShaderCompilationException(string compilerOutput)
        {
            this.compilerOutput = compilerOutput;
        }
        public ShaderCompilationException(string message, string compilerOutput)
            : base(message)
        {
            this.compilerOutput = compilerOutput;
        }
        public ShaderCompilationException(string message, Exception inner, string compilerOutput)
            : base(message, inner)
        {
            this.compilerOutput = compilerOutput;
        }

        public string CompilerOutput { get { return compilerOutput; } }

        public override string ToString()
        {
            return string.Format("{0}{1}{2}", base.ToString(), Environment.NewLine, compilerOutput);
        }
    }
}
