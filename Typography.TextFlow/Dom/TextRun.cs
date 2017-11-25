//MIT, 2014-2017, WinterDev
using System.Collections.Generic;

namespace Typography.TextLayout
{
    public class TextRun
    {
        char[] _srcTextBuffer;
        int _startAt;
        int _len;

        GlyphPlanListCache _glyphPlanListCache;

        public TextRun(char[] srcTextBuffer, int startAt, int len)
        {
            this._srcTextBuffer = srcTextBuffer;
            this._startAt = startAt;
            this._len = len;
        }
        public void SetGlyphPlan(List<GlyphPlan> glyphPlans, int startAt, int len)
        {
            _glyphPlanListCache = new GlyphPlanListCache(glyphPlans, startAt, len);
        }
        struct GlyphPlanListCache
        {
            public readonly List<GlyphPlan> glyphPlans;
            public readonly int startAt;
            public readonly int len;
            public GlyphPlanListCache(List<GlyphPlan> glyphPlans, int startAt, int len)
            {
                this.glyphPlans = glyphPlans;
                this.startAt = startAt;
                this.len = len;
            }

        }
    }
}