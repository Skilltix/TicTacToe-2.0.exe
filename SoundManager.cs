using System;
using System.Media;
using System.Threading.Tasks;

namespace TicTacToeGUI
{
    public class SoundManager
    {
        private static SoundPlayer clickSound = new SoundPlayer();
        private static SoundPlayer winSound = new SoundPlayer();
        private static SoundPlayer drawSound = new SoundPlayer();
        private static SoundPlayer moveSound = new SoundPlayer();
        private static SoundPlayer errorSound = new SoundPlayer();
        
        private static bool isInitialized = false;
        private static bool soundEnabled = true;

        public static bool SoundEnabled
        {
            get => soundEnabled;
            set => soundEnabled = value;
        }

        // Инициализация звуков (можно использовать системные или встроенные)
        public static void Initialize()
        {
            if (isInitialized) return;
            
            try
            {
                // Используем системные звуки
                // Можно заменить на свои WAV файлы, если нужно
                isInitialized = true;
            }
            catch
            {
                // Если системные звуки недоступны, продолжаем без них
                isInitialized = false;
            }
        }

        public static void PlayClick()
{
    if (!soundEnabled) return;
    Task.Run(() => Console.Beep(1000, 60)); // Более высокий и короткий
}

public static void PlayMove()
{
    if (!soundEnabled) return;
    Task.Run(() => Console.Beep(800, 80)); // Средний тон
}

public static void PlayWin()
{
    if (!soundEnabled) return;
    Task.Run(() => 
    {
        Console.Beep(659, 150); // Ми
        Console.Beep(784, 150); // Соль
        Console.Beep(1046, 250); // До (высокая)
    });
}

public static void PlayDraw()
{
    if (!soundEnabled) return;
    Task.Run(() => 
    {
        Console.Beep(523, 200); // До
        Console.Beep(523, 200); // До
        Console.Beep(523, 200); // До
    });
}

public static void PlayError()
{
    if (!soundEnabled) return;
    Task.Run(() => Console.Beep(300, 400)); // Низкий и долгий
}

        public static void PlayGameStart()
        {
            if (!soundEnabled || !isInitialized) return;
            Task.Run(() => 
            {
                // Простая мелодия начала игры
                Console.Beep(523, 150); // До
                Console.Beep(659, 150); // Ми
                Console.Beep(784, 150); // Соль
            });
        }

        public static void PlayGameOver()
        {
            if (!soundEnabled || !isInitialized) return;
            Task.Run(() => 
            {
                // Простая мелодия конца игры
                Console.Beep(784, 200); // Соль
                Console.Beep(659, 200); // Ми
                Console.Beep(523, 200); // До
            });
        }
    }
}