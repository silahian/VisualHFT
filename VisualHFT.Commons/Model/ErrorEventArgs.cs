namespace VisualHFT.Commons.Model
{
    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public object Context { get; }  // Including context, for example, the OrderBook object that caused the issue

        public ErrorEventArgs(Exception exception, object context)
        {
            Exception = exception;
            Context = context;
        }
    }

}