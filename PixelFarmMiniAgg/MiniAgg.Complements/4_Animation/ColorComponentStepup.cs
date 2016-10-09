//BSD, 2014-2016, WinterDev
//BSD (Oct,November) 2008, (Jan 2009), 2014 WinterDev

using System;
namespace PixelFarm.Drawing.Animation
{
    public class ColorComponentStepup
    {
        public int component1;
        public int component2;
        public int distance;
        public int stepType = 0;
        public double stepValuePerPixel = -1;
        public double nPixelsForOneStep = -1;
        public int CalculateValue(double d)
        {
            if (d < distance)
            {
                if (stepType == 1)
                {
                    double npix = d / nPixelsForOneStep;
                    if (component1 + (int)npix < 256)
                    {
                        return component1 + (int)npix;
                    }
                    else
                    {
                        return 255;
                    }
                }
                else if (stepType == 2)
                {
                    int val = (int)(component1 + (stepValuePerPixel * d));
                    if (val < 256)
                    {
                        return val;
                    }
                    else
                    {
                        return 255;
                    }
                }
                else
                {
                    return component1;
                }
            }
            else
            {
                return component2;
            }
        }
        public ColorComponentStepup(int component1, int component2, int distance)
        {
            this.component1 = component1;
            this.component2 = component2;
            this.distance = distance;
            int componentDiff = Math.Abs(component2 - component1);
            if (componentDiff != 0 && distance != 0)
            {
                if (distance > componentDiff)
                {
                    this.stepType = 1;
                    if (component1 < component2)
                    {
                        this.nPixelsForOneStep = (double)distance / (double)componentDiff;
                    }
                    else
                    {
                        this.nPixelsForOneStep = -((double)distance / (double)componentDiff);
                    }
                }
                else
                {
                    this.stepType = 2;
                    if (component1 < component2)
                    {
                        this.stepValuePerPixel = (double)componentDiff / (double)distance;
                    }
                    else
                    {
                        this.stepValuePerPixel = -((double)componentDiff / (double)distance);
                    }
                }
            }
        }
    }
}