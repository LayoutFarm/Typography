//BSD, 2014-present, WinterDev

using System.Text;
namespace LayoutFarm.WebDom
{
    /// <summary>
    /// css attr selector
    /// </summary>
    public class CssAttributeSelectorExpression
    {
        public string AttributeName;
        public CssAttributeSelectorOperator operatorName;
        public CssCodeValueExpression valueExpression;
        public string SelectorSignature
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append('[');
                sb.Append(AttributeName);
                switch (operatorName)
                {
                    case CssAttributeSelectorOperator.Equalily:
                        {
                            sb.Append('=');
                        }
                        break;
                    case CssAttributeSelectorOperator.Existance:
                        {
                        }
                        break;
                    case CssAttributeSelectorOperator.Hyphen:
                        {
                            sb.Append("|=");
                        }
                        break;
                    case CssAttributeSelectorOperator.Prefix:
                        {
                            sb.Append("^=");
                        }
                        break;
                    case CssAttributeSelectorOperator.WhiteSpace:
                        {
                            sb.Append("~=");
                        }
                        break;
                    case CssAttributeSelectorOperator.Substring:
                        {
                            sb.Append("*=");
                        }
                        break;
                    case CssAttributeSelectorOperator.Suffix:
                        {
                            sb.Append("$=");
                        }
                        break;
                }
                if (valueExpression != null)
                {
                    sb.Append(valueExpression.ToString());
                }
                sb.Append(']');
                return sb.ToString();
            }
        }
    }

    public enum CssAttributeSelectorOperator
    {
        Equalily,
        Existance,
        Hyphen,
        Prefix,
        Substring,
        Suffix,
        WhiteSpace
    }
}