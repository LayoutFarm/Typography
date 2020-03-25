//MIT, 2014-present, WinterDev
using System;
using System.Collections.Generic;

namespace Typography.TextLayout
{

    public class SmallLine
    {

        int _caretCharIndex = 0;//default  
        //TODO: temp public, review accessibility here again
        public List<char> _charBuffer = new List<char>();
        public PxScaledGlyphPlan[] _glyphPlans = new PxScaledGlyphPlan[100]; // no writes to this just yet
        public List<UserCodePointToGlyphIndex> _userCodePointToGlyphIndexMap = new List<UserCodePointToGlyphIndex>();

        bool _contentChanged = true;

        /// <summary>
        /// add char at current pos
        /// </summary>
        /// <param name="c"></param>
        public void AddChar(char c)
        {
            //add char at cursor index
            int count = _charBuffer.Count;

            if (_caretCharIndex == count)
            {
                //at the end                
                _charBuffer.Add(c);
                _caretCharIndex++;
            }
            else if (_caretCharIndex < count)
            {
                _charBuffer.Insert(_caretCharIndex, c);
                _caretCharIndex++;
            }
            else
            {
                throw new System.NotSupportedException();
            }
            _contentChanged = true;
        }
        public void DoBackspace()
        {
            if (_caretCharIndex == 0)
            {
                return;
            }
            //
            int count = _charBuffer.Count;
            if (count == 0)
            {
                _caretCharIndex = 0;
                return;
            }

            //end
            _caretCharIndex--;
            _charBuffer.RemoveAt(_caretCharIndex);

            _contentChanged = true;
        }
        public void DoDelete()
        {
            //simulate by do right + backspace
            int count = _charBuffer.Count;
            if (_caretCharIndex == count)
            {
                //caret is on the end
                //just return
                return;
            }
            DoRight();
            DoBackspace();
        }
        public void DoLeft()
        {
            int count = _charBuffer.Count;
            if (count == 0)
            {
                _caretCharIndex = 0;
                return;
            }
            else if (_caretCharIndex > 0)
            {
                //this is on the end
                _caretCharIndex--;

                //check if the caret can rest on this glyph?
                if (_caretCharIndex > 0)
                {
                    //find its mapping to glyph index
                    UserCodePointToGlyphIndex userCodePointToGlyphIndex = _userCodePointToGlyphIndexMap[_caretCharIndex];
                    int mapToGlyphIndex = userCodePointToGlyphIndex.glyphIndexListOffset_plus1;
                    //
                    if (mapToGlyphIndex == 0)
                    {
                        //no map 
                        DoLeft();   //recursive ***
                        return;
                    }
                    //-------------------------
                    //we -1 ***
                    PxScaledGlyphPlan glyphPlan = _glyphPlans[userCodePointToGlyphIndex.glyphIndexListOffset_plus1 - 1];
                    if (!glyphPlan.AdvanceMoveForward)
                    {
                        //caret can't rest here
                        //so
                        DoLeft();   //recursive ***
                        return;
                    }
                    //---------------------
                    // 
                }
            }
            else
            {

            }

        }
        public void DoRight()
        {
            int count = _charBuffer.Count;
            if (count == 0)
            {
                return;
            }
            else if (_caretCharIndex < count)
            {
                //this is on the end
                _caretCharIndex++;

                //check if the caret can rest on this glyph?
                if (_caretCharIndex < count)
                {

                    //find its mapping to glyph index
                    UserCodePointToGlyphIndex userCodePointToGlyphIndex = _userCodePointToGlyphIndexMap[_caretCharIndex];
                    int mapToGlyphIndex = userCodePointToGlyphIndex.glyphIndexListOffset_plus1;
                    //
                    if (mapToGlyphIndex == 0)
                    {
                        //no map 
                        DoRight();   //recursive ***
                        return;
                    }
                    //-------------------------
                    //we -1 ***
                    PxScaledGlyphPlan glyphPlan = _glyphPlans[userCodePointToGlyphIndex.glyphIndexListOffset_plus1 - 1];
                    if (!glyphPlan.AdvanceMoveForward)
                    {
                        //caret can't rest here
                        //so
                        DoRight();   //recursive ***
                        return;
                    }
                }
            }
            else
            {

            }
        }
        public void DoHome()
        {
            _caretCharIndex = 0;
        }
        public void DoEnd()
        {
            _caretCharIndex = _charBuffer.Count;
        }
        public int CharCount
        {
            get { return 0; }
        }
        public bool ContentChanged { get { return _contentChanged; } set { _contentChanged = value; } }
        public int CaretCharIndex { get { return _caretCharIndex; } }
        public void SetCaretCharIndex(int newindex)
        {
            if (newindex >= 0 && newindex <= _charBuffer.Count)
            {
                _caretCharIndex = newindex;
            }
        }

        public void SetCharIndexFromPos(float x, float y)
        {

            int count = _glyphPlans.Length;
            float accum_x = 0;



            for (int i = 0; i < count; ++i)
            {
                float thisGlyphW = _glyphPlans[i].AdvanceX;
                accum_x += thisGlyphW;
                if (accum_x > x)
                {
                    //TODO: review here 
                    //for some glyph that has been substituted 
                    //glyph may not match with actual user char in the _line    

                    float xoffset_on_glyph = (x - (accum_x - thisGlyphW));
                    if (xoffset_on_glyph >= (thisGlyphW / 2))
                    {
                        if (i + 1 >= _userCodePointToGlyphIndexMap.Count)
                        {
                            //break here
                            _caretCharIndex = i + 1;
                            return;
                        }

                        _caretCharIndex = i + 1;
                        //check if the caret can rest on this pos or not

                        UserCodePointToGlyphIndex map = _userCodePointToGlyphIndexMap[_caretCharIndex];
                        if (map.glyphIndexListOffset_plus1 == 0)
                        {
                            //no map
                            //cant rest here
                            if (_caretCharIndex < count)
                            {
                                DoRight();
                            }
                        }
                        else
                        {
                            //has map
                            if (_caretCharIndex < count && !_glyphPlans[map.glyphIndexListOffset_plus1 - 1].AdvanceMoveForward)
                            {
                                //recursive ***
                                DoRight(); //
                            }
                        }
                    }
                    else
                    {
                        _caretCharIndex = i;
                        //check if the caret can rest on this pos or not
                        UserCodePointToGlyphIndex map = _userCodePointToGlyphIndexMap[_caretCharIndex];
                        if (map.glyphIndexListOffset_plus1 == 0)
                        {
                            //no map
                            //cant rest here
                            if (_caretCharIndex > 0)
                            {
                                //recursive ***
                                DoLeft();
                            }
                        }
                        else
                        {
                            //has map
                            if (_caretCharIndex < count && !_glyphPlans[map.glyphIndexListOffset_plus1 - 1].AdvanceMoveForward)
                            {
                                //recursive ***
                                DoLeft();
                            }
                        }

                    }
                    //stop
                    break;
                }
            }
        }
    }

}