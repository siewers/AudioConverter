namespace AudioConverter;

internal sealed class ConsoleAppCancellationTokenSource
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ConsoleAppCancellationTokenSource()
    {
        Console.CancelKeyPress += OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        using var _ = _cancellationTokenSource.Token.Register(() =>
            {
                AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
                Console.CancelKeyPress -= OnCancelKeyPress;
            }
        );
    }

    public CancellationToken Token => _cancellationTokenSource.Token;

    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        // NOTE: cancel event, don't terminate the process
        e.Cancel = true;
        _cancellationTokenSource.Cancel();
    }

    private void OnProcessExit(object? sender, EventArgs e)
    {
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            // NOTE: SIGINT (cancel key was pressed, this shouldn't ever actually hit however, as we remove the event handler upon cancellation of the `cancellationSource`)
            return;
        }

        _cancellationTokenSource.Cancel();
    }
}