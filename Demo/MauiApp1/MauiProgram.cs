using Microsoft.Extensions.Logging;
using System.Reflection;


namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            MyFontPanel.GetFontResource = (string fontfile) =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                return assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.Fonts.{fontfile}");                 
            };

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}