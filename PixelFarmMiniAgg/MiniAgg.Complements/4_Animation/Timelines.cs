//BSD, 2014-2016, WinterDev
//BSD August 2009, 2014, WinterDev

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
//implement simple timeline and animation

namespace PixelFarm.Drawing.Animation
{
    /// <summary>
    /// describe property change in an interval ?
    /// </summary>
    public abstract class TimelineBase
    {
        //-------------------------------------------- 
        internal int stateFlags;
        int startFrame;
        int endFrame;
        //--------------------------------------------

        //first 2 bits
        //1. can have 'by' value without 'from'(start value) value & to (destination value)
        protected const int SPECIFIC_BY = 0;
        protected const int ASSIGN_DEST_VALUE = 1 << (1 - 1);
        protected const int ASSIGN_BEGIN_VALUE = 1 << (2 - 1);
        //easier if specific both values
        protected const int ASSIGN_BOTH = ASSIGN_DEST_VALUE | ASSIGN_BEGIN_VALUE;
        //next 2 bits 
        //sometimes specific 'to' value => make it destination value
        //but may not specific 'from' value so use 'from' value from that value
        //in this case we must calculate 'by' value, depends on the init states
        protected const int SPECIFIC_FROM_VALUE = 1 << (3 - 1);
        protected const int SPECIFIC_TO_VALUE = 1 << (4 - 1);
        //5. fillstyle (at end of timeline) 
        protected const int FILL_SUSTAIN = 0; // sustain at destination value
        protected const int FILL_LOOP = 1 << (5 - 1); //fill with loop
        protected const int FILL_BEGIN = 0x0;//
        protected const int FILL_REVERSE = 1 << (6 - 1);//
        public int StartFrame
        {
            get
            {
                return startFrame;
            }
            set
            {
                int diff = endFrame - startFrame;
                if (diff <= 0)
                {
                    diff = 1;
                }
                startFrame = value;
                endFrame = startFrame + diff;
                OnDurationChanged();
            }
        }

        public int FrameDuration
        {
            get
            {
                return endFrame - startFrame + 1;
            }
            set
            {
                if (value > 0)
                {
                    endFrame = startFrame + value - 1;
                    OnDurationChanged();
                }
            }
        }
        public int EndFrame
        {
            get
            {
                return endFrame;
            }
            set
            {
                if (value > startFrame)
                {
                    endFrame = value;
                    OnDurationChanged();
                }
            }
        }
        protected virtual void OnDurationChanged()
        {
        }
    }


    /// <summary>
    /// describe double value change
    /// </summary>
    public class DoubleValueTimeline : TimelineBase
    {
        double fromValue; //start value
        double toValue; //end value
        double stepValue;// step up value in each frame
        double[] inbetweenValues;
        public Double FromValue
        {
            get
            {
                return fromValue;
            }
            set
            {
                fromValue = value;
                stateFlags |= ASSIGN_BEGIN_VALUE;
            }
        }
        public Double By
        {
            get
            {
                return stepValue;
            }
            set
            {
                stepValue = value;
            }
        }

        public Double ToValue
        {
            get
            {
                return toValue;
            }
            set
            {
                toValue = value;
                stateFlags |= ASSIGN_DEST_VALUE;
            }
        }



        protected override void OnDurationChanged()
        {
            int frameDuration = FrameDuration;
            if (frameDuration > 2)
            {
                if (stateFlags == ASSIGN_BOTH)
                {
                    stepValue = (toValue - fromValue) / (double)frameDuration;
                    int inbetweenCount = frameDuration - 2;
                    inbetweenValues = new double[inbetweenCount];
                    inbetweenValues[0] = fromValue + stepValue;
                    for (int i = 1; i < inbetweenCount; i++)
                    {
                        inbetweenValues[i] = inbetweenValues[i - 1] + stepValue;
                    }
                }
            }
        }
        public void SetValueRange(Double fromValue, Double toValue)
        {
            this.fromValue = fromValue;
            this.toValue = toValue;
            stateFlags = ASSIGN_BOTH;
            OnDurationChanged();
        }
        public Double GetValueAtFrameOffset(int frameOffset)
        {
            if (frameOffset == 0)
            {
                return fromValue;
            }
            else if (frameOffset >= EndFrame)
            {
                return toValue;
            }
            else
            {
                return inbetweenValues[frameOffset - 1];
            }
        }
    }


    public class ColorTimeline : TimelineBase
    {
        Color fromValue;
        Color toValue;
        Color[] inbetweenValues;
        public ColorTimeline()
        {
        }
        public Color FromValue
        {
            get
            {
                return fromValue;
            }
            set
            {
                fromValue = value;
                stateFlags |= ASSIGN_BEGIN_VALUE;
            }
        }
        public Color ToValue
        {
            get
            {
                return toValue;
            }
            set
            {
                toValue = value;
                stateFlags |= ASSIGN_DEST_VALUE;
            }
        }

        public void EvaluateRange()
        {
            int frameDuration = FrameDuration;
            if (frameDuration > 2)
            {
                ColorComponentStepup rCompo = new ColorComponentStepup(fromValue.R, toValue.R, frameDuration);
                ColorComponentStepup gComp = new ColorComponentStepup(fromValue.G, toValue.G, frameDuration);
                ColorComponentStepup bComp = new ColorComponentStepup(fromValue.B, toValue.B, frameDuration);
                ColorComponentStepup aCompo = new ColorComponentStepup(fromValue.A, toValue.A, frameDuration);
                int inbetweenCount = frameDuration - 2;
                inbetweenValues = new Color[inbetweenCount];
                for (int i = 0; i < inbetweenCount; i++)
                {
                    inbetweenValues[i] = Color.FromArgb(
                         aCompo.CalculateValue(i),
                         rCompo.CalculateValue(i),
                         gComp.CalculateValue(i),
                         bComp.CalculateValue(i)
                        );
                }
            }
        }

        protected override void OnDurationChanged()
        {
            EvaluateRange();
        }

        public void SetValueRange(Color fromValue, Color toValue)
        {
            this.fromValue = fromValue;
            this.toValue = toValue;
            stateFlags = ASSIGN_BOTH;
            EvaluateRange();
        }
        public Color GetValueAtFrameOffset(int frameOffset)
        {
            if (frameOffset == 0)
            {
                return fromValue;
            }
            else if (frameOffset > EndFrame - 1)
            {
                return toValue;
            }
            else
            {
                return inbetweenValues[frameOffset - 1];
            }
        }
    }

    //public class GradientColorTimeline : TimelineBase
    //{

    //    ArtGradientColorInfo fromValue;
    //    ArtGradientColorInfo toValue;
    //    ArtGradientColorInfo[] inbetweenValues;

    //    public GradientColorTimeline()
    //    {

    //    }
    //    public ArtGradientColorInfo FromValue
    //    {
    //        get
    //        {
    //            return fromValue;
    //        }
    //        set
    //        {
    //            fromValue = value;
    //            stateFlags |= ASSIGN_BEGIN_VALUE;
    //        }
    //    }
    //    public ArtGradientColorInfo ToValue
    //    {
    //        get
    //        {
    //            return toValue;
    //        }
    //        set
    //        {
    //            toValue = value;
    //            stateFlags |= ASSIGN_DEST_VALUE;
    //        }
    //    }

    //    protected override void OnDurationChanged()
    //    {
    //        EvaluateRange();
    //    }

    //    public void EvaluateRange()
    //    {

    //        int frameDuration = FrameDuration;
    //        if (frameDuration > 2)
    //        {

    //            if (fromValue == null)
    //            {
    //                return;
    //            }

    //            int toColorCount = toValue.ColorCount;
    //            if (toColorCount == 2)
    //            {

    //                int inbetweenCount = frameDuration - 2;
    //                inbetweenValues = new ArtGradientColorInfo[inbetweenCount];

    //                for (int colorPoint_i = 0; colorPoint_i < 2; ++colorPoint_i)
    //                {

    //                    Color fromColor = fromValue.GetColor(colorPoint_i);
    //                    Color toColor = toValue.GetColor(colorPoint_i);
    //                    Point pos = fromValue.GetPosition(colorPoint_i);

    //                    ColorComponentStepup rCompo = new ColorComponentStepup(fromColor.R, toColor.R, frameDuration);
    //                    ColorComponentStepup gComp = new ColorComponentStepup(fromColor.G, toColor.G, frameDuration);
    //                    ColorComponentStepup bComp = new ColorComponentStepup(fromColor.B, toColor.B, frameDuration);
    //                    ColorComponentStepup aCompo = new ColorComponentStepup(fromColor.A, toColor.A, frameDuration);

    //                    for (int i = 0; i < inbetweenCount; i++)
    //                    {

    //                        ArtGradientColorInfo inbetweenColor = null;
    //                        if (colorPoint_i == 0)
    //                        {
    //                            inbetweenColor = new ArtGradientColorInfo();
    //                            inbetweenValues[i] = inbetweenColor;
    //                        }
    //                        else
    //                        {

    //                            inbetweenColor = inbetweenValues[i];
    //                        }

    //                        inbetweenColor.AddColor(
    //                        Color.FromArgb(
    //                             aCompo.CalculateValue(i),
    //                             rCompo.CalculateValue(i),
    //                             gComp.CalculateValue(i),
    //                             bComp.CalculateValue(i)
    //                            ), pos);
    //                    }
    //                }
    //            }
    //            else
    //            {


    //            }
    //        }
    //    }

    //    public void SetValueRange(ArtGradientColorInfo fromValue, ArtGradientColorInfo toValue)
    //    {
    //        this.fromValue = fromValue;
    //        this.toValue = toValue;
    //        stateFlags = ASSIGN_BOTH;
    //        EvaluateRange();
    //    }
    //    public ArtGradientColorInfo GetValueAtFrameOffset(int frameOffset)
    //    {

    //        if (frameOffset == 0)
    //        {
    //            return fromValue;
    //        }
    //        else if (frameOffset > EndFrame - 1)
    //        {
    //            return toValue;
    //        }
    //        else
    //        {

    //            return inbetweenValues[frameOffset - 1];
    //        }
    //    }
    //}




    public class DoubleValueTimelineSeries : TimelineSeriesBase
    {
        public DoubleValueTimelineSeries(ArtGfxInstructionInfo prop)
            : base(prop)
        {
        }

        public void AppendLast(DoubleValueTimeline doubleValueAnimation)
        {
            base.InnerAppendLast(doubleValueAnimation);
        }
    }
    public class ColorTimelineSeries : TimelineSeriesBase
    {
        public ColorTimelineSeries(ArtGfxInstructionInfo prop)
            : base(prop)
        {
        }
        public void AppendLast(ColorTimeline colorAnimation)
        {
            base.InnerAppendLast(colorAnimation);
        }
        //public void AppendLast(GradientColorTimeline colorAnimation)
        //{
        //    base.InnerAppendLast(colorAnimation);
        //}
    }


    //-----------------------------------------------------
    /// <summary>
    /// timeline for each property
    /// </summary>
    public abstract class TimelineSeriesBase
    {
        List<TimelineBase> timelines;
        int lastestFrame;
        int lastestTimelineIndex;
        public readonly ArtGfxInstructionInfo targetGfxInfo;
        public readonly int moduleId;
        public TimelineSeriesBase(ArtGfxInstructionInfo gfxInfo)
        {
            this.targetGfxInfo = gfxInfo;
            this.moduleId = gfxInfo.moduleId;
        }
        protected void InnerAdd(TimelineBase timeline)
        {
            if (timelines == null)
            {
                timelines = new List<TimelineBase>();
            }
            timelines.Add(timeline);
        }

        protected void InnerAppendLast(TimelineBase timeline)
        {
            if (timelines == null)
            {
                timelines = new List<TimelineBase>();
            }
            if (timelines.Count > 0)
            {
                timeline.StartFrame = timelines[timelines.Count - 1].EndFrame + 1;
                timelines.Add(timeline);
            }
            else
            {
                timeline.StartFrame = 0;
                timelines.Add(timeline);
            }
        }

        public TimelineBase GetTimelineAtFrame(int frameNumber)
        {
            if (timelines != null)
            {
                TimelineBase timeline = timelines[lastestTimelineIndex];
                if (frameNumber > lastestFrame)
                {
                    while (frameNumber >= timeline.EndFrame
                        && lastestTimelineIndex < timelines.Count - 1)
                    {
                        lastestTimelineIndex++;
                        timeline = timelines[lastestTimelineIndex];
                    }
                    lastestFrame = frameNumber;
                    return timeline;
                }
                else
                {
                    while (frameNumber < timeline.StartFrame
                        && lastestTimelineIndex > 0)
                    {
                        lastestTimelineIndex--;
                        timeline = timelines[lastestTimelineIndex];
                    }
                    lastestFrame = frameNumber;
                    return timeline;
                }
            }

            return null;
        }
    }
}



