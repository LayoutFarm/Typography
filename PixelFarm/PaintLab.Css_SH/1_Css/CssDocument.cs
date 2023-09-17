//BSD, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using System.Text;
namespace LayoutFarm.WebDom
{
    public class CssDocument
    {
        List<CssDocMember> _cssItemCollection = new List<CssDocMember>();
        public CssDocument()
        {
        }
        public void Add(CssDocMember cssitem)
        {
            _cssItemCollection.Add(cssitem);
        }
        public IEnumerable<CssDocMember> GetCssDocMemberIter()
        {
            int j = _cssItemCollection.Count;
            for (int i = 0; i < j; ++i)
            {
                yield return _cssItemCollection[i];
            }
        }
#if DEBUG
        public override string ToString()
        {
            StringBuilder stBuilder = new StringBuilder();
            int j = _cssItemCollection.Count;
            for (int i = 0; i < j; ++i)
            {
                stBuilder.Append(_cssItemCollection[i].ToString());
            }

            return stBuilder.ToString();
        }
#endif

    }
}