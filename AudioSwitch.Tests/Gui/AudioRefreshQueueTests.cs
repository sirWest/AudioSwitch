using System.Runtime.ExceptionServices;
using System.Windows.Threading;
using AudioSwitch.Flyout;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class AudioRefreshQueueTests
{
    [Fact]
    public void NotificationBurstsYieldToInputAndChangesDuringRefreshAreNotLost() =>
        OnDispatcher(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            var calls = new List<string>();
            AudioRefreshQueue? queue = null;
            queue = new AudioRefreshQueue(
                dispatcher,
                () =>
                {
                    calls.Add("refresh");
                    if (calls.Count == 2)
                    {
                        Task.Run(() => Parallel.For(0, 61, _ => queue!.Request()))
                            .GetAwaiter()
                            .GetResult();
                    }
                }
            );
            using (queue)
            {
                Task.Run(() => Parallel.For(0, 61, _ => queue.Request())).GetAwaiter().GetResult();
                dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    new Action(() => calls.Add("input"))
                );
                Pump(TimeSpan.FromMilliseconds(500));
                Assert.Equal(new[] { "input", "refresh", "refresh" }, calls);

                queue.Request();
                Pump(TimeSpan.FromMilliseconds(250));
                Assert.Equal(4, calls.Count);
            }
        });

    [Fact]
    public void DisposalCancelsPendingAndFutureRefreshes() =>
        OnDispatcher(() =>
        {
            var calls = 0;
            var queue = new AudioRefreshQueue(Dispatcher.CurrentDispatcher, () => calls++);
            queue.Request();
            queue.Dispose();
            Task.Run(queue.Request).GetAwaiter().GetResult();
            Pump(TimeSpan.FromMilliseconds(250));
            Assert.Equal(0, calls);
        });

    private static void Pump(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var stop = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = duration };
        stop.Tick += (_, _) => frame.Continue = false;
        stop.Start();
        try
        {
            Dispatcher.PushFrame(frame);
        }
        finally
        {
            stop.Stop();
        }
    }

    private static void OnDispatcher(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
