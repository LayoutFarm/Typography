//MIT, 2016-2017, WinterDev
//some code from ICU project with BSD license

namespace Typography.TextBreak
{
    public enum TextBreakKind
    {
        Word,
        Sentence,
    }
     
   
    public abstract class TextBreaker
    {
        public abstract void DoBreakCore(WordVisitor visitor, System.ReadOnlySpan<char> input);
        public TextBreakKind BreakKind
        {
            get;
            set;
        }
        public void DoBreak(WordVisitor visitor, System.ReadOnlySpan<char> input)
        {
            IsCanceled = false;//reset 
            //to end
            DoBreakCore(visitor, input);
        }
       
        protected bool IsCanceled { get; set; }
        /// <summary>
        /// cancel current breaking task
        /// </summary>
        public void Cancel() { IsCanceled = true; }
    }
    public readonly struct SplitBound
    {
        public readonly int startIndex;
        public readonly int length;
        public SplitBound(int startIndex, int length)
        {
            this.startIndex = startIndex;
            this.length = length;
        }
#if DEBUG
        public override string ToString()
        {
            return startIndex + ":" + length;
        }
#endif
    }

}