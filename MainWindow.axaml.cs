using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Interactivity;
using System.Media;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace TicTacToeGUI
{
    public partial class MainWindow : Window
    {
        private int scoreX = 0;
        private int scoreO = 0;
        private int draws = 0;
        private bool isZeroNow = false;
        private Button[,] buttons;
        private bool gameFinished = false;
        
        private enum GameMode { PlayerVsPlayer, PlayerVsAI }
        private GameMode currentMode = GameMode.PlayerVsPlayer;

        private bool soundEnabled = true;
        
        // Уровень сложности ИИ
        private enum Difficulty { Easy, Medium, Hard }
        private Difficulty aiDifficulty = Difficulty.Medium;
        
        private Random random = new Random();

        public MainWindow()
{
    InitializeComponent();
    buttons = new Button[,]
    {
        { B1, B2, B3 },
        { B4, B5, B6 },
        { B7, B8, B9 }
    };
    
    // Инициализируем звук
    SoundManager.Initialize();
    SoundManager.PlayGameStart();
    
    InitializeGame();
}

        private void InitializeGame()
        {
            foreach (var btn in buttons)
            {
                btn.Click += ButtonClick;
                btn.FontSize = 32;
                btn.FontWeight = Avalonia.Media.FontWeight.Bold;
            }
            
            // Инициализируем тексты
            if (DifficultyButton != null)
            {
                DifficultyButton.Content = $"⚙️ {aiDifficulty switch
                {
                    Difficulty.Easy => "Лёгкий",
                    Difficulty.Medium => "Средний",
                    Difficulty.Hard => "Сложный",
                    _ => "Средний"
                }}";
            }
            
            if (ModeButton != null)
            {
                ModeButton.Content = currentMode == GameMode.PlayerVsPlayer 
                    ? "👥 Игрок" 
                    : "🤖 ИИ";
            }
            
            if (DifficultyLabel != null)
            {
                if (currentMode == GameMode.PlayerVsPlayer)
                {
                    DifficultyLabel.Text = "Режим: Игрок vs Игрок";
                    DifficultyLabel.Foreground = Brushes.Green;
                }
                else
                {
                    string difficultyText = aiDifficulty switch
                    {
                        Difficulty.Easy => "Лёгкий",
                        Difficulty.Medium => "Средний",
                        Difficulty.Hard => "Сложный",
                        _ => "Средний"
                    };
                    DifficultyLabel.Text = $"Режим: Игрок vs ИИ ({difficultyText})";
                    DifficultyLabel.Foreground = Brushes.Purple;
                }
            }
            
            ResetGame();
        }

        private async void ButtonClick(object? sender, RoutedEventArgs e)
{
    if (gameFinished) return;
    if (sender is not Button btn) return;
    if (!string.IsNullOrEmpty(btn.Content?.ToString())) 
    {
        SoundManager.PlayError(); // Звук ошибки при клике на занятую клетку
        return;
    }

    // Звук хода
    SoundManager.PlayMove();
    
    MakeMove(btn);
    
    if (currentMode == GameMode.PlayerVsAI && !gameFinished && isZeroNow)
    {
        await Task.Delay(600);
        MakeAIMove();
    }
}

        private void MakeMove(Button btn)
{
    btn.Content = isZeroNow ? "O" : "X";
    btn.Foreground = isZeroNow ? Brushes.Blue : Brushes.Red;
    
    // Звук установки символа
    SoundManager.PlayClick();

    CheckGameResult();
    
    if (!gameFinished)
    {
        isZeroNow = !isZeroNow;
        UpdateTurnDisplay();
    }
}

        private void MakeAIMove()
{
    if (gameFinished) return;
    
    var move = GetBestMove();
    if (move.row == -1) return;
    
    var btn = buttons[move.row, move.col];
    btn.Content = "O";
    btn.Foreground = Brushes.Blue;
    
    // Звук хода ИИ
    SoundManager.PlayClick();

    CheckGameResult();
    
    if (!gameFinished)
    {
        isZeroNow = false;
        UpdateTurnDisplay();
    }
}

        private void CheckGameResult()
{
    var (winnerFound, winningLine) = CheckWinner();
    
    if (winnerFound)
    {
        gameFinished = true;
        _ = HighlightWinningLine(winningLine);
        
        var winner = isZeroNow ? "Нолики победили!" : "Крестики победили!";
        _ = ShowMessageAsync("Победа!", winner);
        
        if (isZeroNow) scoreO++;
        else scoreX++;
        
        // Звук победы
        SoundManager.PlayWin();
        
        UpdateScoreDisplay();
        return;
    }

    if (IsDraw())
    {
        gameFinished = true;
        _ = ShowMessageAsync("Ничья", "Ничья, поле заполнено!");
        draws++;
        
        // Звук ничьи
        SoundManager.PlayDraw();
        
        UpdateScoreDisplay();
    }
}

        private (int row, int col) GetBestMove()
        {
            // В зависимости от уровня сложности используем разные алгоритмы
            return aiDifficulty switch
            {
                Difficulty.Easy => GetEasyMove(),
                Difficulty.Medium => GetMediumMove(),
                Difficulty.Hard => GetHardMove(),
                _ => GetMediumMove()
            };
        }

        // ЛЕГКИЙ уровень - случайные ходы
        private (int row, int col) GetEasyMove()
        {
            var emptyCells = GetEmptyCells();
            return emptyCells.Count > 0 ? emptyCells[random.Next(emptyCells.Count)] : (-1, -1);
        }

        // СРЕДНИЙ уровень - стратегические ходы
        private (int row, int col) GetMediumMove()
        {
            // 1. Проверим, можем ли выиграть
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (IsCellEmpty(i, j))
                    {
                        buttons[i, j].Content = "O";
                        bool canWin = CheckWinner().winnerFound;
                        buttons[i, j].Content = string.Empty;
                        if (canWin) return (i, j);
                    }
                }
            }

            // 2. Блокировать выигрыш соперника
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (IsCellEmpty(i, j))
                    {
                        buttons[i, j].Content = "X";
                        bool shouldBlock = CheckWinner().winnerFound;
                        buttons[i, j].Content = string.Empty;
                        if (shouldBlock) return (i, j);
                    }
                }
            }

            // 3. Центр
            if (IsCellEmpty(1, 1)) return (1, 1);

            // 4. Углы
            var corners = new (int, int)[] { (0,0), (0,2), (2,0), (2,2) };
            var emptyCorners = corners.Where(c => IsCellEmpty(c.Item1, c.Item2)).ToList();
            if (emptyCorners.Count > 0)
                return emptyCorners[random.Next(emptyCorners.Count)];

            // 5. Любая свободная клетка
            return GetEasyMove();
        }

        // СЛОЖНЫЙ уровень - минимакс алгоритм
        private (int row, int col) GetHardMove()
        {
            int bestScore = int.MinValue;
            (int row, int col) bestMove = (-1, -1);
            
            var emptyCells = GetEmptyCells();
            
            foreach (var cell in emptyCells)
            {
                buttons[cell.row, cell.col].Content = "O";
                int score = Minimax(0, false);
                buttons[cell.row, cell.col].Content = string.Empty;
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = cell;
                }
            }
            
            return bestMove;
        }

        // Упрощенный минимакс-алгоритм
        private int Minimax(int depth, bool isMaximizing)
        {
            var (winnerFound, _) = CheckWinner();
            
            if (winnerFound)
            {
                return isMaximizing ? -10 : 10;
            }
            
            if (IsDraw())
            {
                return 0;
            }
            
            if (isMaximizing) // Ход ИИ (O)
            {
                int bestScore = int.MinValue;
                
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (IsCellEmpty(i, j))
                        {
                            buttons[i, j].Content = "O";
                            int score = Minimax(depth + 1, false);
                            buttons[i, j].Content = string.Empty;
                            
                            bestScore = Math.Max(score, bestScore);
                        }
                    }
                }
                return bestScore;
            }
            else // Ход игрока (X)
            {
                int bestScore = int.MaxValue;
                
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (IsCellEmpty(i, j))
                        {
                            buttons[i, j].Content = "X";
                            int score = Minimax(depth + 1, true);
                            buttons[i, j].Content = string.Empty;
                            
                            bestScore = Math.Min(score, bestScore);
                        }
                    }
                }
                return bestScore;
            }
        }

        // Вспомогательный метод - получает список пустых клеток
        private List<(int row, int col)> GetEmptyCells()
        {
            var emptyCells = new List<(int, int)>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (IsCellEmpty(i, j))
                    {
                        emptyCells.Add((i, j));
                    }
                }
            }
            return emptyCells;
        }

        private bool IsCellEmpty(int row, int col)
        {
            return string.IsNullOrEmpty(buttons[row, col].Content?.ToString());
        }

        private void ResetGame()
        {
            gameFinished = false;
            
            foreach (var btn in buttons)
            {
                btn.Content = string.Empty;
                btn.Foreground = Brushes.Black;
                btn.Background = Brushes.White;
            }

            isZeroNow = false;
            UpdateTurnDisplay();
            if (soundEnabled)
            {
                 Task.Run(() => Console.Beep(523, 100));
             }
        }

        private async void UpdateTurnDisplay()
        {
            if (currentMode == GameMode.PlayerVsAI && isZeroNow)
            {
                string difficultyText = aiDifficulty switch
                {
                    Difficulty.Easy => " (Лёгкий)",
                    Difficulty.Medium => " (Средний)",
                    Difficulty.Hard => " (Сложный)",
                    _ => ""
                };
                TurnLabel.Text = $"Ход ИИ{difficultyText}...";
                TurnLabel.Foreground = Brushes.Orange;
            }
            else
            {
                TurnLabel.Text = isZeroNow ? "Ходят нолики" : "Ходят крестики";
                TurnLabel.Foreground = isZeroNow ? Brushes.Blue : Brushes.Red;
            }
            
           
        }

        private void UpdateScoreDisplay()
        {
            ScoreX.Text = scoreX.ToString();
            ScoreO.Text = scoreO.ToString();
            Draws.Text = draws.ToString();
        }

        private (bool winnerFound, int[,] winningLine) CheckWinner()
        {
            string[,] map = new string[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    map[i, j] = buttons[i, j].Content?.ToString() ?? string.Empty;

            int[][,] winPatterns = {
                new int[,] { {0,0}, {0,1}, {0,2} },
                new int[,] { {1,0}, {1,1}, {1,2} },
                new int[,] { {2,0}, {2,1}, {2,2} },
                new int[,] { {0,0}, {1,0}, {2,0} },
                new int[,] { {0,1}, {1,1}, {2,1} },
                new int[,] { {0,2}, {1,2}, {2,2} },
                new int[,] { {0,0}, {1,1}, {2,2} },
                new int[,] { {0,2}, {1,1}, {2,0} }
            };

            foreach (var pattern in winPatterns)
            {
                string a = map[pattern[0,0], pattern[0,1]];
                string b = map[pattern[1,0], pattern[1,1]];
                string c = map[pattern[2,0], pattern[2,1]];
                
                if (!string.IsNullOrEmpty(a) && a == b && b == c)
                    return (true, pattern);
            }

            return (false, new int[0,0]);
        }

        private async Task HighlightWinningLine(int[,] winningLine)
        {
            for (int i = 0; i < 3; i++)
            {
                var btn = buttons[winningLine[i,0], winningLine[i,1]];
                btn.Background = Brushes.LightGreen;
                await Task.Delay(200);
            }
        }

        private bool IsDraw()
        {
            return buttons.Cast<Button>().All(b => !string.IsNullOrEmpty(b.Content?.ToString()));
        }

        private async Task ShowMessageAsync(string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 350,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 20
            };

            stackPanel.Children.Add(new TextBlock
            {
                Text = message,
                FontSize = 18,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });

            var buttonStack = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 10
            };

            var newGameButton = new Button
            {
                Content = "Новая игра",
                Width = 120,
                Padding = new Thickness(10, 5)
            };

            var exitButton = new Button
            {
                Content = "Выйти",
                Width = 120,
                Padding = new Thickness(10, 5)
            };

            newGameButton.Click += (s, e) => 
            {
                dialog.Close();
                ResetGame();
            };
            
            exitButton.Click += (s, e) => this.Close();

            buttonStack.Children.Add(newGameButton);
            buttonStack.Children.Add(exitButton);
            stackPanel.Children.Add(buttonStack);
            
            dialog.Content = stackPanel;
            await dialog.ShowDialog(this);
        }

        // ==================== ОБРАБОТЧИКИ КНОПОК ====================

        private async void NewGame_Click(object? sender, RoutedEventArgs e)
        {
            SoundManager.PlayClick(); // Звук клика
            if (sender is Button btn)
            {
                btn.Opacity = 0.8;
                await Task.Delay(50);
                btn.Opacity = 1;
            }
            ResetGame();
        }
        
        private async void ResetScore_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.Opacity = 0.8;
                await Task.Delay(50);
                btn.Opacity = 1;
            }
             SoundManager.PlayClick(); // Звук клика
            scoreX = 0;
            scoreO = 0;
            draws = 0;
            UpdateScoreDisplay();
            ResetGame();
        }
        
        private async void DifficultyButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.Opacity = 0.8;
                await Task.Delay(50);
                btn.Opacity = 1;
            }
            SoundManager.PlayClick(); // Звук клика
            
            // Меняем уровень сложности по кругу
            aiDifficulty = aiDifficulty switch
            {
                Difficulty.Easy => Difficulty.Medium,
                Difficulty.Medium => Difficulty.Hard,
                Difficulty.Hard => Difficulty.Easy,
                _ => Difficulty.Medium
            };
            
            // Обновляем текст кнопки и метки
            string difficultyText = aiDifficulty switch
            {
                Difficulty.Easy => "Лёгкий",
                Difficulty.Medium => "Средний", 
                Difficulty.Hard => "Сложный",
                _ => "Средний"
            };
            
            // Обновляем кнопку
            if (DifficultyButton != null)
            {
                DifficultyButton.Content = $"⚙️ {difficultyText}";
            }
            
            // Обновляем метку
            if (DifficultyLabel != null)
            {
                if (currentMode == GameMode.PlayerVsPlayer)
                {
                    DifficultyLabel.Text = "Режим: Игрок vs Игрок";
                }
                else
                {
                    DifficultyLabel.Text = $"Режим: Игрок vs ИИ ({difficultyText})";
                }
            }
            
            // Сбрасываем игру при смене сложности
            ResetGame();
        }
        
        private async void ModeButton_Click(object? sender, RoutedEventArgs e)
        {
            SoundManager.PlayClick(); // Звук клика
            if (sender is Button btn)
            {
                btn.Opacity = 0.8;
                await Task.Delay(50);
                btn.Opacity = 1;
            }
            
            currentMode = currentMode == GameMode.PlayerVsPlayer 
                ? GameMode.PlayerVsAI 
                : GameMode.PlayerVsPlayer;
            
            // Обновляем кнопку
            if (ModeButton != null)
            {
                if (currentMode == GameMode.PlayerVsPlayer)
                {
                    ModeButton.Content = "👥 Игрок";
                    ModeButton.Background = new SolidColorBrush(Color.Parse("#27AE60"));
                }
                else
                {
                    ModeButton.Content = "🤖 ИИ";
                    ModeButton.Background = new SolidColorBrush(Color.Parse("#8E44AD"));
                }
            }
            
            // Обновляем метку сложности
            if (DifficultyLabel != null)
            {
                string difficultyText = aiDifficulty switch
                {
                    Difficulty.Easy => "Лёгкий",
                    Difficulty.Medium => "Средний", 
                    Difficulty.Hard => "Сложный",
                    _ => "Средний"
                };
                
                if (currentMode == GameMode.PlayerVsPlayer)
                {
                    DifficultyLabel.Text = "Режим: Игрок vs Игрок";
                    DifficultyLabel.Foreground = Brushes.Green;
                }
                else
                {
                    DifficultyLabel.Text = $"Режим: Игрок vs ИИ ({difficultyText})";
                    DifficultyLabel.Foreground = Brushes.Purple;
                }
            }
            
            // Сбрасываем игру
            ResetGame();
        }
    
    private void SoundButton_Click(object? sender, RoutedEventArgs e)
{
    soundEnabled = !soundEnabled;
    SoundManager.SoundEnabled = soundEnabled;
    
    var soundButton = sender as Button;
    if (soundButton != null)
    {
        if (soundEnabled)
        {
            soundButton.Content = "🔊 Вкл";
            soundButton.Background = new SolidColorBrush(Color.Parse("#27AE60"));
            SoundManager.PlayClick(); // Проиграем звук при включении
        }
        else
        {
            soundButton.Content = "🔇 Выкл";
            soundButton.Background = new SolidColorBrush(Color.Parse("#E74C3C"));
        }
    }
}
    }

}