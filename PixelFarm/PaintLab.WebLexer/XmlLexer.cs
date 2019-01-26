//BSD, 2014-present, WinterDev

using LayoutFarm.WebLexer;
namespace LayoutFarm.WebDom.Parser
{
    public enum XmlLexerEvent
    {
        /// <summary>
        /// &lt;a
        /// </summary>
        VisitOpenAngle,        //  <a
        /// <summary>
        /// &lt;/a
        /// </summary>
        VisitOpenSlashAngle,   //  </a
        /// <summary>
        ///  a&gt;
        /// </summary>
        VisitCloseAngle,       //  a>
        /// <summary>
        /// /&gt;
        /// </summary>
        VisitCloseSlashAngle,  //  />        
        /// <summary>
        /// =
        /// </summary>
        VisitAttrAssign,      //=
        /// <summary>
        /// &lt;!
        /// </summary>
        VisitOpenAngleExclimation, //<! eg. doctype node <!doctype
        /// <summary>
        /// &lt;--
        /// </summary>
        OpenComment,           //  <!--
        /// <summary>
        /// --&gt;
        /// </summary>
        CloseComment,          //  -->
        /// <summary>
        /// &lt;?
        /// </summary>
        OpenProcessInstruction,  //  <?
        ProcessInstructionContent,
        /// <summary>
        /// ?&gt;
        /// </summary>
        CloseProcessInstruction, //  ?>
        NodeNameOrAttribute, 
        NamePrefix,
        NodeNameLocal,
        Attribute,
        AttributeNameLocal,
        AttributeNamePrefix,
        AttributeValueAsLiteralString,
        SwitchToContentPart,
        FromContentPart,
        CommentContent,        
    }

    enum XmlLexState
    {
        Init,
        AfterOpenAngle
    }

    public delegate void XmlLexerEventHandler(XmlLexerEvent lexEvent, int startIndex, int len);
    public abstract partial class XmlLexer
    {
        public event XmlLexerEventHandler LexStateChanged;
        protected void RaiseStateChanged(XmlLexerEvent lexEvent, int startIndex, int len)
        {
            LexStateChanged(lexEvent, startIndex, len);
        }
        public virtual void Analyze(TextSnapshot textSnapshot) { }
        public virtual void BeginLex()
        {
        }
        public virtual void EndLex()
        {
        }
        public static XmlLexer CreateLexer()
        {
            return new MyXmlLexer();
        }
    }
}
