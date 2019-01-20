//BSD, 2014-present, WinterDev

using LayoutFarm.WebLexer;
namespace LayoutFarm.WebDom.Parser
{
#if DEBUG
    partial class MyXmlLexer
    {
        dbugLexerReport dbug_LexerReport;
        int dbug_currentLineCharIndex = -1;
        int dbug_currentLineNumber = 0;
        void dbug_OnStartAnalyze()
        {
        }
        void dbug_OnFinishAnalyze()
        {
        }
        public void dbugStartRecord(System.IO.StreamWriter writer)
        {
            dbug_LexerReport = new dbugLexerReport();
            dbug_LexerReport.Start(writer);
        }

        public void dbugEndRecord()
        {
            dbug_LexerReport.Flush();
            dbug_LexerReport = null;
        }

        void dbugReportChar(char c, int currentState)
        {
            if (dbug_LexerReport != null)
            {
                dbug_LexerReport.WriteLine("[" + dbug_currentLineNumber + " ," +
                    dbug_currentLineCharIndex + "] state=" + currentState + " char=" + c);
            }
        }
    }
#endif
}