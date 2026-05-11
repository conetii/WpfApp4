using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp4.Infrastructure;
using WpfApp4.Models;

namespace WpfApp4.ViewModels;

public sealed class GuessNumberViewModel : INotifyPropertyChanged
{
    private static readonly Brush NeutralBrush = CreateBrush("#334155");
    private static readonly Brush WarningBrush = CreateBrush("#A16207");
    private static readonly Brush ErrorBrush = CreateBrush("#B42318");
    private static readonly Brush SuccessBrush = CreateBrush("#15803D");
    private static readonly Brush ColdBrush = CreateBrush("#2563EB");
    private static readonly Brush WarmBrush = CreateBrush("#D97706");
    private static readonly Brush HotBrush = CreateBrush("#DC2626");

    private readonly GameState _gameState = new();
    private readonly RelayCommand _submitGuessCommand;

    private string _currentGuess = string.Empty;
    private string _statusMessage = "Введите число от 1 до 100.";
    private Brush _statusBrush = NeutralBrush;
    private string _heatMessage = "Нет попыток.";
    private Brush _heatBrush = NeutralBrush;
    private double _heatValue;
    private int? _bestResult;

    public GuessNumberViewModel()
    {
        _submitGuessCommand = new RelayCommand(SubmitGuess, () => IsInputEnabled);
        RestartGameCommand = new RelayCommand(RestartGame);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> GuessHistory { get; } = new();

    public string PromptText => $"Число от {_gameState.MinimumValue} до {_gameState.MaximumValue}";
    public string RangeText => $"{_gameState.CurrentMinimum} - {_gameState.CurrentMaximum}";
    public string AttemptsText => _gameState.AttemptsCount.ToString();
    public string BestResultText => _bestResult is int best ? $"{best}" : "-";
    public bool IsInputEnabled => !_gameState.IsFinished;
    public ICommand SubmitGuessCommand => _submitGuessCommand;
    public ICommand RestartGameCommand { get; }

    public string CurrentGuess
    {
        get => _currentGuess;
        set => SetProperty(ref _currentGuess, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public Brush StatusBrush
    {
        get => _statusBrush;
        private set => SetProperty(ref _statusBrush, value);
    }

    public string HeatMessage
    {
        get => _heatMessage;
        private set => SetProperty(ref _heatMessage, value);
    }

    public Brush HeatBrush
    {
        get => _heatBrush;
        private set => SetProperty(ref _heatBrush, value);
    }

    public double HeatValue
    {
        get => _heatValue;
        private set => SetProperty(ref _heatValue, value);
    }

    private void SubmitGuess()
    {
        if (!TryGetValidatedGuess(out int guess))
        {
            return;
        }

        GuessFeedback feedback = _gameState.EvaluateGuess(guess);

        OnPropertyChanged(nameof(AttemptsText));
        OnPropertyChanged(nameof(RangeText));

        string historyText = $"Попытка {_gameState.AttemptsCount}: {guess}";

        switch (feedback.Outcome)
        {
            case GuessOutcome.TooLow:
                StatusMessage = $"{guess} - слишком маленькое.";
                StatusBrush = WarningBrush;
                UpdateHeat(feedback.Difference);
                GuessHistory.Insert(0, $"{historyText} - меньше");
                break;

            case GuessOutcome.TooHigh:
                StatusMessage = $"{guess} - слишком большое.";
                StatusBrush = WarningBrush;
                UpdateHeat(feedback.Difference);
                GuessHistory.Insert(0, $"{historyText} - больше");
                break;

            case GuessOutcome.Correct:
                StatusMessage = $"Число {guess} угадано за {_gameState.AttemptsCount} попыток.";
                StatusBrush = SuccessBrush;
                HeatMessage = "Точно.";
                HeatBrush = SuccessBrush;
                HeatValue = 100;
                GuessHistory.Insert(0, $"{historyText} - угадано");
                UpdateBestResult();
                OnPropertyChanged(nameof(IsInputEnabled));
                _submitGuessCommand.RaiseCanExecuteChanged();
                break;
        }

        CurrentGuess = string.Empty;
    }

    private bool TryGetValidatedGuess(out int guess)
    {
        guess = 0;

        if (string.IsNullOrWhiteSpace(CurrentGuess))
        {
            ShowValidationError("Введите число от 1 до 100.");
            return false;
        }

        if (!int.TryParse(CurrentGuess, out guess))
        {
            ShowValidationError("Нужно ввести целое число.");
            return false;
        }

        if (guess < _gameState.MinimumValue || guess > _gameState.MaximumValue)
        {
            ShowValidationError($"Число должно быть от {_gameState.MinimumValue} до {_gameState.MaximumValue}.");
            return false;
        }

        if (guess < _gameState.CurrentMinimum || guess > _gameState.CurrentMaximum)
        {
            ShowValidationError($"Число должно быть от {_gameState.CurrentMinimum} до {_gameState.CurrentMaximum}.");
            return false;
        }

        return true;
    }

    private void ShowValidationError(string message)
    {
        StatusMessage = message;
        StatusBrush = ErrorBrush;
    }

    private void RestartGame()
    {
        _gameState.Reset();
        CurrentGuess = string.Empty;
        GuessHistory.Clear();

        StatusMessage = "Введите число от 1 до 100.";
        StatusBrush = NeutralBrush;
        HeatMessage = "Нет попыток.";
        HeatBrush = NeutralBrush;
        HeatValue = 0;

        OnPropertyChanged(nameof(RangeText));
        OnPropertyChanged(nameof(AttemptsText));
        OnPropertyChanged(nameof(IsInputEnabled));
        _submitGuessCommand.RaiseCanExecuteChanged();
    }

    private void UpdateBestResult()
    {
        if (_bestResult is null || _gameState.AttemptsCount < _bestResult.Value)
        {
            _bestResult = _gameState.AttemptsCount;
            OnPropertyChanged(nameof(BestResultText));
        }
    }

    private void UpdateHeat(int difference)
    {
        if (difference <= 2)
        {
            HeatValue = 90;
            HeatMessage = "Очень близко.";
            HeatBrush = HotBrush;
            return;
        }

        if (difference <= 5)
        {
            HeatValue = 70;
            HeatMessage = "Близко.";
            HeatBrush = WarmBrush;
            return;
        }

        if (difference <= 12)
        {
            HeatValue = 45;
            HeatMessage = "Средне.";
            HeatBrush = WarningBrush;
            return;
        }

        HeatValue = 15;
        HeatMessage = "Далеко.";
        HeatBrush = ColdBrush;
    }

    private static Brush CreateBrush(string hex)
    {
        SolidColorBrush brush = new((Color)ColorConverter.ConvertFromString(hex)!);
        brush.Freeze();
        return brush;
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
