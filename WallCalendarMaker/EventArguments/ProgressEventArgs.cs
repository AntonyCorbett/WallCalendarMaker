namespace WallCalendarMaker.EventArguments;

internal sealed class ProgressEventArgs : EventArgs
{
    public ProgressEventArgs(string message, bool isVerbose)
    {
        Message = message;
        IsVerbose = isVerbose;
    }

    public string Message { get; }

    public bool IsVerbose { get; }
}