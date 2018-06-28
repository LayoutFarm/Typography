//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
using System;
using System.Collections.Generic;
using PixelFarm.BitmapBufferEx;

namespace WinFormGdiPlus
{

    static class Colors
    {
        public static ColorInt White = ColorInt.FromArgb(255, 255, 255, 255);
        public static ColorInt Black = ColorInt.FromArgb(255, 0, 0, 0);
        public static ColorInt Red = ColorInt.FromArgb(255, 255, 0, 0);
        public static ColorInt Blue = ColorInt.FromArgb(255, 0, 0, 255);
    }
    public class ParticleEmitter
    {

        public PointD Center { get; set; }
        public List<Particle> Particles = new List<Particle>();
        Random rand = new Random();
        public BitmapBuffer TargetBitmap;
        public BitmapBuffer ParticleBitmap;
        RectD sourceRect = new RectD(0, 0, 32, 32);
        double elapsedRemainder;
        double updateInterval = .003;
        HslColor particleColor = new HslColor();



        public ParticleEmitter()
        {
            particleColor = HslColor.FromColor(Colors.Red);
            particleColor.L *= .75;
        }



        void CreateParticle()
        {
            Particle p = new Particle();
            double speed = rand.Next(20) + 140;
            double angle = Math.PI * 2 * rand.Next(10000) / 10000;
            p.Velocity.X = Math.Sin(angle) * speed;
            p.Velocity.Y = Math.Cos(angle) * speed;
            p.Position = new PointD(Center.X - 16, Center.Y - 16);
            p.Color = particleColor.ToColor();
            p.Lifespan = .5 + rand.Next(200) / 1000d;
            p.Initiailize();
            Particles.Add(p);
        }

        public void Update(double elapsedSeconds)
        {
            elapsedRemainder += elapsedSeconds;
            while (elapsedRemainder > updateInterval)
            {
                elapsedRemainder -= updateInterval;
                CreateParticle();
                particleColor.H += .1;
                particleColor.H = particleColor.H % 255;
                for (int i = Particles.Count - 1; i >= 0; i--)
                {
                    Particle p = Particles[i];
                    p.Update(updateInterval);
                    if (p.Color.A == 0) Particles.Remove(p);
                }
            }
            using (TargetBitmap.GetBitmapContext())
            {
                using (ParticleBitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
                {
                    for (int i = 0; i < Particles.Count; i++)
                    {
                        Particle p = Particles[i];
                        TargetBitmap.Blit(p.Position, ParticleBitmap, sourceRect, p.Color, BitmapBufferExtensions.BlendMode.Additive);
                    }
                }
            }
        }
    }

}