namespace WpfApp4.Models;

public sealed class GameState
{
    private readonly Random _random = new();

    public int MinimumValue { get; } = 1;
    public int MaximumValue { get; } = 100;

    public int SecretNumber { get; private set; }
    public int AttemptsCount { get; private set; }
    public int CurrentMinimum { get; private set; }
    public int CurrentMaximum { get; private set; }
    public bool IsFinished { get; private set; }

    public GameState()
    {
        Reset();
    }

    public void Reset()
    {
        SecretNumber = _random.Next(MinimumValue, MaximumValue + 1);
        AttemptsCount = 0;
        CurrentMinimum = MinimumValue;
        CurrentMaximum = MaximumValue;
        IsFinished = false;
    }

    public GuessFeedback EvaluateGuess(int guess)
    {
        if (IsFinished)
        {
            throw new InvalidOperationException("Game is already finished.");
        }

        AttemptsCount++;
        int difference = Math.Abs(SecretNumber - guess);

        if (guess < SecretNumber)
        {
            CurrentMinimum = Math.Max(CurrentMinimum, guess + 1);
            return new GuessFeedback(GuessOutcome.TooLow, difference);
        }

        if (guess > SecretNumber)
        {
            CurrentMaximum = Math.Min(CurrentMaximum, guess - 1);
            return new GuessFeedback(GuessOutcome.TooHigh, difference);
        }

        IsFinished = true;
        return new GuessFeedback(GuessOutcome.Correct, 0);
    }
}

public enum GuessOutcome
{
    TooLow,
    TooHigh,
    Correct
}

public readonly record struct GuessFeedback(GuessOutcome Outcome, int Difference);
