//BSD, 2014-present, WinterDev

using LayoutFarm.WebLexer;
namespace LayoutFarm.WebDom.Parser
{
    public sealed partial class MyXmlLexer : XmlLexer
    {
        int _readIndex = 0;
        int _lastFlushAt = 0;
        int _appendCount = 0;
        int _firstAppendAt = -1;
        TextSnapshot _textSnapshot;
        public MyXmlLexer()
        {
        }
        public override void BeginLex()
        {
#if DEBUG
            dbug_currentLineNumber = 0;
            dbug_currentLineCharIndex = -1;
            //dbugStartRecord("htmlparse.txt");
#endif

            _readIndex = 0;
            _lastFlushAt = 0;
            _appendCount = 0;
            _firstAppendAt = -1;
        }
        public override void EndLex()
        {
            base.EndLex();
        }
        public override void Analyze(TextSnapshot textSnapshot)
        {
#if DEBUG
            dbug_OnStartAnalyze();
#endif

            _textSnapshot = textSnapshot;
            char[] sourceBuffer = TextSnapshot.UnsafeGetInternalBuffer(textSnapshot);
            int lim = sourceBuffer.Length;
            char strEscapeChar = '"';
            int currentState = 0;
            //-----------------------------

            for (int i = 0; i < lim; i++)
            {
                char c = sourceBuffer[i];
#if DEBUG
                dbug_currentLineCharIndex++;
                dbugReportChar(c, currentState);
#endif
                switch (currentState)
                {
                    default:
                        {
                            //???

                        }
                        break;
                    case 0:  //from content mode 
                        {
                            if (c == '<')
                            {
                                //flush existing content 
                                //switch to content  tag mode 
                                FlushExisingBuffer(i, XmlLexerEvent.FromContentPart);
                                currentState = 1;
                                //not need whitespace in this mode 
                            }
                            else
                            {
                                //in content mode
                                AppendBuffer(c, i);
                            }
                        }
                        break;
                    case 1:
                        {
                            //after open angle
                            switch (c)
                            {
                                case '!':
                                    {
                                        currentState = 11; //<!
                                    }
                                    break;
                                case '?':
                                    {
                                        //process instruction
                                        currentState = 8;
                                    }
                                    break;
                                case ':':
                                    {
                                        //shold not occurs
                                        currentState = 4;
                                    }
                                    break;
                                case '/':
                                    {
                                        //close tag 
                                        RaiseStateChanged(XmlLexerEvent.VisitOpenSlashAngle, i, 1);
                                        currentState = 5;//collect node name 
                                    }
                                    break;
                                default:
                                    {
                                        currentState = 5;
                                        //clear prev buffer 
                                        //then start collect node name

                                        AppendBuffer(c, i);
                                    }
                                    break;
                            }
                        }
                        break;
                    case 2:
                        {
                            //inside comment node
                            if (c == '-')
                            {
                                if (i < lim - 2)
                                {
                                    if (sourceBuffer[i + 1] == '-' && sourceBuffer[i + 2] == '>')
                                    {
                                        //end comment node  
                                        FlushExisingBuffer(i, XmlLexerEvent.CommentContent);
                                        i += 2;
                                        currentState = 0;
                                        continue;
                                    }
                                }
                            }
                            //skip all comment  content ? 
                            AppendBuffer(c, i);
                        }
                        break;
                    case 5:
                        {
                            //inside open angle
                            //name collecting
                            //terminate with... 
                            switch (c)
                            {
                                case '/':
                                    {
                                        currentState = 7;
                                    }
                                    break;
                                case '>':
                                    {
                                        FlushExisingBuffer(i, XmlLexerEvent.NodeNameOrAttribute);
                                        RaiseStateChanged(XmlLexerEvent.VisitCloseAngle, i, 1);
                                        //flush 
                                        currentState = 0;
                                        //goto content mode
                                    }
                                    break;
                                case ':':
                                    {
                                        //flush node name
                                        FlushExisingBuffer(i, XmlLexerEvent.NamePrefix);
                                        //start new node name

                                    }
                                    break;
                                case ' ':
                                    {
                                        //flush node name
                                        FlushExisingBuffer(i, XmlLexerEvent.NodeNameOrAttribute);
                                    }
                                    break;
                                case '=':
                                    {
                                        //flush name
                                        FlushExisingBuffer(i, XmlLexerEvent.Attribute);
                                        RaiseStateChanged(XmlLexerEvent.VisitAttrAssign, i, 1);
                                        //start collect value of attr 
                                    }
                                    break;
                                case '"':
                                    {
                                        //start string escap with " 
                                        currentState = 6;
                                        strEscapeChar = '"';
                                    }
                                    break;
                                case '\'':
                                    {
                                        //start string escap with ' 
                                        currentState = 6;
                                        strEscapeChar = '\'';
                                    }
                                    break;
                                default:
                                    {
                                        //else collect 
                                        //flush nodename

                                        if (char.IsWhiteSpace(c))
                                        {
                                            FlushExisingBuffer(i, XmlLexerEvent.NodeNameOrAttribute);
                                        }
                                        else
                                        {
                                            AppendBuffer(c, i);
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    case 6:
                        {
                            //collect string 
                            if (c == strEscapeChar)
                            {
                                //stop string escape
                                //flush 
                                FlushExisingBuffer(i, XmlLexerEvent.AttributeValueAsLiteralString);
                                currentState = 5;
                            }
                            else
                            {
                                AppendBuffer(c, i);
                            }
                        }
                        break;
                    case 7:
                        {
                            //after /   //must be >
                            if (c == '>')
                            {
                                FlushExisingBuffer(i, XmlLexerEvent.NodeNameOrAttribute);
                                RaiseStateChanged(XmlLexerEvent.VisitCloseSlashAngle, i, 1);
                                currentState = 0;
                            }
                            else
                            {
                                //error ?
                            }
                        }
                        break;
                    case 8:
                        {
                            //enter processing instruction
                            if (c == '?')
                            {
                                //exit
                                currentState = 9;
                            }
                        }
                        break;
                    case 9:
                        {
                            if (c == '>')
                            {
                                //flush xml processing instruction 
                                FlushExisingBuffer(i, XmlLexerEvent.ProcessInstructionContent);
                                currentState = 0; //back to content mode
                            }
                        }
                        break;
                    case 10:
                        {
                            //unknown tag
                            //exit from this tag when found >
                            if (c == '>')
                            {
                                currentState = 0;
                            }
                        }
                        break;
                    case 11:
                        {
                            //open_angle, exlcimation
                            switch (c)
                            {
                                case '-':
                                    {
                                        //looking for next char
                                        if (i < lim)
                                        {
                                            if (sourceBuffer[i + 1] == '-')
                                            {
                                                i++;//consume
                                                currentState = 2;
                                                continue;
                                            }
                                            else
                                            {
                                                //unknown tag?
                                                currentState = 10;
                                            }
                                        }
                                    }
                                    break;
                                case '[':
                                    {
                                        // <![
                                        //
                                        currentState = 10;//not implement,just skip
                                    }
                                    break;
                                default:
                                    {
                                        //doc type?
                                        if (char.IsLetter(sourceBuffer[i + 1]))
                                        {
                                            RaiseStateChanged(XmlLexerEvent.VisitOpenAngleExclimation, i, 2);
                                            AppendBuffer(c, i);
                                            currentState = 5;
                                        }
                                        else
                                        {
                                            currentState = 10;//not implement, just skip
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }

#if DEBUG
            dbug_OnFinishAnalyze();
#endif
        }



        void FlushExisingBuffer(int lastFlushAtIndex, XmlLexerEvent lexerEvent)
        {
            //raise lexer event
            if (_appendCount > 0)
            {
#if DEBUG
                //Console.WriteLine(lexerEvent.ToString() + " : " +
                //    new string(this.textSnapshot.Copy(_firstAppendAt, (_readIndex - _firstAppendAt) + 1)));
#endif

                RaiseStateChanged(lexerEvent, _firstAppendAt, (_readIndex - _firstAppendAt) + 1);
            }

            _lastFlushAt = lastFlushAtIndex;
            _appendCount = 0;
        }

        void AppendBuffer(char c, int index)
        {
            if (_appendCount == 0)
            {
                _firstAppendAt = index;
                _appendCount++;
            }

            _readIndex = index;
        }
    }
}