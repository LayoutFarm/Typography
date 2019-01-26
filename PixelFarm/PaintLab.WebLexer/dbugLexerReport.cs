//BSD, 2014-present, WinterDev

using System.IO;
namespace LayoutFarm.WebLexer
{
#if DEBUG
    public class dbugLexerReport
    {
        StreamWriter _strmWriter;
        int _lineCount = 0;
        public void Start(StreamWriter strmWriter)
        {
            _strmWriter = strmWriter;
            strmWriter.AutoFlush = true;
        }
        public void WriteLine(string info)
        {
            _strmWriter.WriteLine(_lineCount + " " + info);
            _strmWriter.Flush();
            _lineCount++;
        }
        public void Flush()
        {
            _strmWriter.Flush();
        }
    }
#endif
}