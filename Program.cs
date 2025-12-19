using Avalonia;
using System;

namespace TicTacToeGUI
{
    internal static class Program
    {
        [STAThread]  // Атрибут, который указывает, что приложение должно работать в одном потоке (для UI)
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);  // Старт приложения с классическим жизненным циклом (для настольных приложений)
        }

        // Метод конфигурации приложения
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()  // Указываем, что приложение будет класса App (это главный класс приложения в Avalonia)
                         .UsePlatformDetect()  // Автоматическое определение платформы (Windows, macOS, Linux)
                         .LogToTrace();  // Логирование в вывод trace (можно использовать для отладки)
    }
}

