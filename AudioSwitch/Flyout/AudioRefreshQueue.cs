using System.Windows.Threading;

namespace AudioSwitch.Flyout;

internal sealed class AudioRefreshQueue : IDisposable
{
    private readonly Dispatcher dispatcher;
    private readonly DispatcherTimer timer;
    private int pending;
    private int disposed;

    internal AudioRefreshQueue(Dispatcher dispatcher, Action refresh)
    {
        this.dispatcher = dispatcher;
        timer = new DispatcherTimer(DispatcherPriority.Background, dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(100),
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            // Notifications arriving during a refresh must schedule another pass.
            Interlocked.Exchange(ref pending, 0);
            if (Volatile.Read(ref disposed) == 0)
            {
                refresh();
            }
        };
    }

    internal void Request()
    {
        if (
            Volatile.Read(ref disposed) != 0
            || dispatcher.HasShutdownStarted
            || Interlocked.Exchange(ref pending, 1) != 0
        )
        {
            return;
        }

        // Windows emits a burst of role/property notifications for one switch.
        // Queue one refresh below input priority, not one rebuild per callback.
        dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                if (Volatile.Read(ref disposed) == 0)
                {
                    timer.Start();
                }
            })
        );
    }

    public void Dispose()
    {
        dispatcher.VerifyAccess();
        Interlocked.Exchange(ref disposed, 1);
        timer.Stop();
    }
}
