//BSD, 2014-present, WinterDev

using System;
using System.Collections.Generic;

namespace LayoutFarm.WebDom.Parser
{
    public delegate void CssLexerEmitHandler(CssTokenName tkname, int startIndex, int len);
    public class CssLexer
    {
        int _appendLength = 0;
        int _startIndex = 0;
        CssLexerEmitHandler _emitHandler;
        char _latestEscapeChar;
        bool _isCollectionWhitespace;
        public CssLexer(CssLexerEmitHandler emitHandler)
        {
            _emitHandler = emitHandler;
        }
        public void Lex(char[] cssSourceBuffer)
        {
            //----------------------
            //clear previous result
            _appendLength = 0;
            _startIndex = 0;
            _latestEscapeChar = '\0';
            //----------------------

            CssLexState lexState = CssLexState.Init;
            int j = cssSourceBuffer.Length;
            for (int i = 0; i < j; ++i)
            {
                char c = cssSourceBuffer[i];
#if DEBUG
                // Console.Write(c);
#endif
                //-------------------------------------- 
                switch (lexState)
                {
                    default:
                        {
                            throw new NotSupportedException();
                        }
                    case CssLexState.Init:
                        {
                            //-------------------------------------- 
                            //1. first name
                            CssTokenName terminalTokenName = GetTerminalTokenName(c);
                            //--------------------------------------  
                            switch (terminalTokenName)
                            {
                                default:
                                    {
                                        Emit(terminalTokenName, i);
                                    }
                                    break;
                                case CssTokenName.Colon:
                                    {
                                        if (i < j - 1)
                                        {
                                            char c1 = cssSourceBuffer[i + 1];
                                            if (c1 == ':')
                                            {
                                                i++;
                                                Emit(CssTokenName.DoubleColon, i);
                                                continue;
                                            }
                                        }
                                        Emit(terminalTokenName, i);
                                    }
                                    break;
                                case CssTokenName.DoubleQuote:
                                    {
                                        _latestEscapeChar = '"';
                                        lexState = CssLexState.CollectString;
                                    }
                                    break;
                                case CssTokenName.Quote:
                                    {
                                        _latestEscapeChar = '\'';
                                        lexState = CssLexState.CollectString;
                                    }
                                    break;
                                case CssTokenName.Divide:
                                    {
                                        //is open comment or not
                                        if (i < j - 1)
                                        {
                                            if (cssSourceBuffer[i + 1] == '*')
                                            {
                                                i++;
                                                //Emit(CssTokenName.LComment, i);
                                                lexState = CssLexState.Comment;
                                                continue;
                                            }
                                        }
                                        Emit(CssTokenName.Divide, i);
                                    }
                                    break;
                                case CssTokenName.Sharp:
                                    {
                                        AppendBuffer(i);
                                        lexState = CssLexState.Iden;
                                    }
                                    break;
                                case CssTokenName.Dot:
                                    {
                                        if (i < j - 1)
                                        {
                                            char c1 = cssSourceBuffer[i + 1];
                                            if (char.IsNumber(c1))
                                            {
                                                AppendBuffer(i);
                                                i++;
                                                AppendBuffer(i);
                                                lexState = CssLexState.Number;
                                                continue;
                                            }
                                        }
                                        Emit(terminalTokenName, i);
                                    }
                                    break;
                                case CssTokenName.Minus:
                                    {
                                        //as iden
                                        AppendBuffer(i);
                                        lexState = CssLexState.Iden;
                                    }
                                    break;
                                case CssTokenName.Unknown:
                                    {
                                        //this is not terminal  
                                        AppendBuffer(i);
                                        if (char.IsNumber(c))
                                        {
                                            lexState = CssLexState.Number;
                                        }
                                        else
                                        {
                                            lexState = CssLexState.Iden;
                                        }
                                    }
                                    break;
                                case CssTokenName.Whitespace:
                                case CssTokenName.Newline:
                                    {
                                        _isCollectionWhitespace = true;
                                    }
                                    break;
                            }
                        }
                        break;
                    case CssLexState.CollectString:
                        {
                            if (c == _latestEscapeChar)
                            {
                                //exit collect string 
                                lexState = CssLexState.Init;
                                EmitBuffer(i, CssTokenName.LiteralString);
                            }
                            else
                            {
                                AppendBuffer(i);
                            }
                        }
                        break;
                    case CssLexState.Comment:
                        {
                            if (c == '*')
                            {
                                if (i < j - 1)
                                {
                                    char c1 = cssSourceBuffer[i + 1];
                                    if (c1 == '/')
                                    {
                                        i++;
                                        //Emit(CssTokenName.RComment, i);
                                        lexState = CssLexState.Init;
                                        continue;
                                    }
                                }
                            }
                            //skip comment?
                        }
                        break;
                    case CssLexState.Iden:
                        {
                            CssTokenName terminalTokenName = GetTerminalTokenName(c);
                            switch (terminalTokenName)
                            {
                                case CssTokenName.Whitespace:
                                case CssTokenName.Newline:
                                    {
                                        EmitBuffer(i, CssTokenName.Iden);
                                        lexState = CssLexState.Init;
                                    }
                                    break;
                                case CssTokenName.Divide:
                                    {
                                        //is open comment or not
                                        throw new NotSupportedException();
                                    }
                                case CssTokenName.Star:
                                    {
                                        //is close comment or not 
                                        throw new NotSupportedException();
                                    }
                                case CssTokenName.Minus:
                                    {
                                        //iden can contains minus 
                                        AppendBuffer(i);
                                    }
                                    break;
                                default:
                                    {
                                        //flush exising buffer
                                        EmitBuffer(i, CssTokenName.Iden);
                                        Emit(terminalTokenName, i);
                                        lexState = CssLexState.Init;
                                    }
                                    break;
                                case CssTokenName.Unknown:
                                    {
                                        //this is not terminal 
                                        AppendBuffer(i);
                                        lexState = CssLexState.Iden;
                                    }
                                    break;
                            }
                        }
                        break;
                    case CssLexState.Number:
                        {
                            if (char.IsNumber(c))
                            {
                                AppendBuffer(i);
                                continue;
                            }
                            //---------------------------------------------------------- 
                            CssTokenName terminalTokenName = GetTerminalTokenName(c);
                            switch (terminalTokenName)
                            {
                                case CssTokenName.Whitespace:
                                case CssTokenName.Newline:
                                    {
                                        if (_appendLength > 0)
                                        {
                                            EmitBuffer(i, CssTokenName.Number);
                                        }

                                        lexState = CssLexState.Init;
                                    }
                                    break;
                                case CssTokenName.Divide:
                                    {
                                        //is open comment or not
                                        throw new NotSupportedException();
                                    }
                                case CssTokenName.Star:
                                    {   //is close comment or not 
                                        throw new NotSupportedException();
                                    }
                                case CssTokenName.Dot:
                                    {
                                        //after number
                                        if (i < j - 1)
                                        {
                                            char c1 = cssSourceBuffer[i + 1];
                                            if (char.IsNumber(c1))
                                            {
                                                AppendBuffer(i);
                                                i++;
                                                AppendBuffer(i);
                                                lexState = CssLexState.Number;
                                                continue;
                                            }
                                        }
                                        EmitBuffer(i, CssTokenName.Number);
                                        Emit(terminalTokenName, i);
                                    }
                                    break;
                                default:
                                    {
                                        //flush exising buffer
                                        EmitBuffer(i, CssTokenName.Number);
                                        Emit(terminalTokenName, i);
                                        lexState = CssLexState.Init;
                                    }
                                    break;
                                case CssTokenName.Unknown:
                                    {
                                        EmitBuffer(i, CssTokenName.Number);
                                        //iden after number may be unit of number*** 
                                        AppendBuffer(i);
                                        lexState = CssLexState.UnitAfterNumber;
                                    }
                                    break;
                            }
                        }
                        break;
                    case CssLexState.UnitAfterNumber:
                        {
                            if (char.IsLetter(c))
                            {
                                AppendBuffer(i);
                            }
                            else
                            {
                                //terminate
                                //TODO: fix this  ....1.348625e-002 


                                EmitBuffer(i, CssTokenName.NumberUnit);
                                //-------------------------------------------
                                CssTokenName terminalTokenName = GetTerminalTokenName(c);
                                switch (terminalTokenName)
                                {
                                    case CssTokenName.Whitespace:
                                    case CssTokenName.Newline:
                                        {
                                        }
                                        break;
                                    default:
                                        {
                                            Emit(terminalTokenName, i);
                                        }
                                        break;
                                }
                                lexState = CssLexState.Init;
                            }
                        }
                        break;
                }
            }
            if (_appendLength > 0)
            {
                switch (lexState)
                {
                    case CssLexState.UnitAfterNumber:
                        EmitBuffer(cssSourceBuffer.Length - 1, CssTokenName.NumberUnit);
                        break;
                    case CssLexState.Number:
                        EmitBuffer(cssSourceBuffer.Length - 1, CssTokenName.Number);
                        break;
                    case CssLexState.Iden:
                    default:
                        EmitBuffer(cssSourceBuffer.Length - 1, CssTokenName.Iden);
                        break;
                }
            }
        }
        void AppendBuffer(int i)
        {
            if (_appendLength == 0)
            {
                _startIndex = i;
            }
            _appendLength++;
        }
        void EmitBuffer(int i, CssTokenName tokenName)
        {
            //flush existing buffer
            if (_appendLength > 0)
            {
                _emitHandler(tokenName, _startIndex, _appendLength);
            }
            _appendLength = 0;
        }
        void Emit(CssTokenName tkname, int i)
        {
            _emitHandler(tkname, i, 1);
        }

        static CssTokenName GetTerminalTokenName(char c)
        {
            CssTokenName tokenName;
            if (terminals.TryGetValue(c, out tokenName))
            {
                return tokenName;
            }
            else
            {
                return CssTokenName.Unknown;
            }
        }

        //===============================================================================================
        static readonly Dictionary<char, CssTokenName> terminals = new Dictionary<char, CssTokenName>();
        static readonly Dictionary<string, CssTokenName> multiCharTokens = new Dictionary<string, CssTokenName>();
        static CssLexer()
        {
            //" @+-*/%.:;[](){}"
            terminals.Add(' ', CssTokenName.Whitespace);
            terminals.Add('\r', CssTokenName.Whitespace);
            terminals.Add('\t', CssTokenName.Whitespace);
            terminals.Add('\f', CssTokenName.Whitespace);
            terminals.Add('\n', CssTokenName.Newline);
            terminals.Add('\'', CssTokenName.Quote);
            terminals.Add('"', CssTokenName.DoubleQuote);
            terminals.Add(',', CssTokenName.Comma);
            terminals.Add('@', CssTokenName.At);
            terminals.Add('+', CssTokenName.Plus);
            terminals.Add('-', CssTokenName.Minus);
            terminals.Add('*', CssTokenName.Star);
            terminals.Add('/', CssTokenName.Divide);
            terminals.Add('%', CssTokenName.Percent);
            terminals.Add('#', CssTokenName.Sharp);
            terminals.Add('~', CssTokenName.Tile);
            terminals.Add('.', CssTokenName.Dot);
            terminals.Add(':', CssTokenName.Colon);
            terminals.Add(';', CssTokenName.SemiColon);
            terminals.Add('[', CssTokenName.LBracket);
            terminals.Add(']', CssTokenName.RBracket);
            terminals.Add('(', CssTokenName.LParen);
            terminals.Add(')', CssTokenName.RParen);
            terminals.Add('{', CssTokenName.LBrace);
            terminals.Add('}', CssTokenName.RBrace);
            terminals.Add('<', CssTokenName.LAngle);
            terminals.Add('>', CssTokenName.RAngle);
            terminals.Add('=', CssTokenName.OpEq);
            terminals.Add('|', CssTokenName.OrPipe);
            terminals.Add('$', CssTokenName.Dollar);
            terminals.Add('^', CssTokenName.Cap);
            //----------------------------------- 
            multiCharTokens.Add("|=", CssTokenName.OrPipeAssign);
            multiCharTokens.Add("~=", CssTokenName.TileAssign);
            multiCharTokens.Add("^=", CssTokenName.CapAssign);
            multiCharTokens.Add("$=", CssTokenName.DollarAssign);
            multiCharTokens.Add("*=", CssTokenName.StarAssign);
            //----------------------------------- 
        }
    }
    public enum CssLexState
    {
        Init,
        Comment,
        Iden,
        CollectString,
        Number,
        UnitAfterNumber
    }

    public enum CssTokenName
    {
        Unknown,
        Newline,
        Whitespace,
        At,
        Comma,
        Plus, //+
        Minus,//-
        Star,//*
        Divide,// /
        Percent,// %
        Dot, // .
        Colon, // :
        Cap, //^
        OpEq,//=
        Dollar,//$
        Tile, //~
        SemiColon,
        Sharp, //#
        OrPipe, //|
        LParen,
        RParen,
        LBracket,
        RBracket,
        LBrace,
        RBrace,
        LAngle, //<
        RAngle,  //>
        Iden,
        Number,
        NumberUnit,
        LiteralString,
        Comment,
        Quote, //  '
        DoubleQuote,  // "
        //------------------
        DoubleColon, //::
        TileAssign, //~=
        StarAssign,//*=
        CapAssign,//^=
        DollarAssign,//$=  
        OrPipeAssign,//|= 
        //------------------


    }
    public enum CssParseState
    {
        Init,
        MoreBlockName,
        ExpectIdenAfterSpecialBlockNameSymbol,
        BlockBody,
        AfterPropertyName,
        ExpectPropertyValue,
        ExpectValueOfHexColor,
        AfterPropertyValue,
        Comment,
        ExpectBlockAttrIden,
        AfterBlockAttrIden,
        AfterAttrName,
        ExpectedBlockAttrValue,
        AfterBlockNameAttr,
        ExpectAtRuleName,
        //@media
        MediaList,
        //@import
        ExpectImportURL,
        ExpectedFuncParameter,
        AfterFuncParameter,
    }
}