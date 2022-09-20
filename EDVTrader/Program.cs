using Avalonia;
using Avalonia.ReactiveUI;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;

namespace EDVTrader
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ConfigureNLog();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().UseReactiveUI();

        private static void ConfigureNLog()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            Layout filePath = Layout.FromString("${basedir}/Data/Logs/${date:format=yyyy}/${date:format=MM}/${date:format=dd}.log");
            Layout filePathDebug = Layout.FromString("${basedir}/Data/Logs/${date:format=yyyy}/${date:format=MM}/${date:format=dd}-debug.log");

            Layout logLayout = Layout.FromString(@"[${date:format=yyyy.MM.dd HH\:mm\:ss}] [${level:uppercase=true:padding=-5}] ${message} ${when:when=length('${exception}')>0:Inner=${newline}}${exception:format=toString,Data}");
            Layout logLayoutDebug = Layout.FromString(@"[${date:format=yyyy.MM.dd HH\:mm\:ss}] [${callsite:className=True:includeNamespace=True:fileName=True:includeSourcePath=False:methodName=True:cleanNamesOfAnonymousDelegates=True:cleanNamesOfAsyncContinuations=True}] [${level:uppercase=true:padding=-5}] ${message} ${when:when=length('${exception}')>0:Inner=${newline}}${exception:format=toString,Data}");

            FileTarget file = new FileTarget("file")
            {
                FileName = filePath,
                Layout = logLayout
            };

            FileTarget fileDebug = new FileTarget("fileDebug")
            {
                FileName = filePathDebug,
                Layout = logLayoutDebug
            };

            ColoredConsoleTarget console = new ColoredConsoleTarget("console")
            {
                Layout = logLayout
            };

            config.AddTarget(file);
            config.AddTarget(fileDebug);
            config.AddTarget(console);

            config.AddRuleForAllLevels(file);
            config.AddRuleForAllLevels(fileDebug);
            config.AddRuleForAllLevels(console);

            LogManager.Configuration = config;
        }
    }
}
