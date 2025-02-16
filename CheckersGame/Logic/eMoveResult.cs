namespace CheckersGame.Logic
{
    public enum eMoveResult
    {
        Success,
        InvalidFormat,
        MustCapture,
        InvalidMove,
        AdditionalCaptureRequired,
        MustCaptureAgain
    }
}