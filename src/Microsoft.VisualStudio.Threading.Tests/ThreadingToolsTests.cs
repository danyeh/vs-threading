﻿namespace Microsoft.VisualStudio.Threading.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using Xunit;
    using GenericParameterHelper = TestTools.UnitTesting.GenericParameterHelper;

    public class ThreadingToolsTests : TestBase
    {
        [Fact]
        public void ApplyChangeOptimistically()
        {
            var n = new GenericParameterHelper(1);
            Assert.True(ThreadingTools.ApplyChangeOptimistically(ref n, i => new GenericParameterHelper(i.Data + 1)));
            Assert.Equal(2, n.Data);
        }

        [Fact]
        public void WithCancellationNull()
        {
            Assert.Throws<ArgumentNullException>(new Action(() =>
                ThreadingTools.WithCancellation(null, CancellationToken.None)));
        }

        [Fact]
        public void WithCancellationOfTNull()
        {
            Assert.Throws<ArgumentNullException>(new Action(() =>
                ThreadingTools.WithCancellation<object>(null, CancellationToken.None)));
        }

        /// <summary>
        /// Verifies that a fast path returns the original task if it has already completed.
        /// </summary>
        [Fact]
        public void WithCancellationOfPrecompletedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            var cts = new CancellationTokenSource();
            Assert.Same(tcs.Task, ((Task)tcs.Task).WithCancellation(cts.Token));
        }

        /// <summary>
        /// Verifies that a fast path returns the original task if it has already completed.
        /// </summary>
        [Fact]
        public void WithCancellationOfPrecompletedTaskOfT()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            var cts = new CancellationTokenSource();
            Assert.Same(tcs.Task, tcs.Task.WithCancellation(cts.Token));
        }

        /// <summary>
        /// Verifies that a fast path returns the original task if it has already completed.
        /// </summary>
        [Fact]
        public void WithCancellationOfPrefaultedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetException(new InvalidOperationException());
            var cts = new CancellationTokenSource();
            Assert.Same(tcs.Task, ((Task)tcs.Task).WithCancellation(cts.Token));
        }

        /// <summary>
        /// Verifies that a fast path returns the original task if it has already completed.
        /// </summary>
        [Fact]
        public void WithCancellationOfPrefaultedTaskOfT()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetException(new InvalidOperationException());
            var cts = new CancellationTokenSource();
            Assert.Same(tcs.Task, tcs.Task.WithCancellation(cts.Token));
        }

        /// <summary>
        /// Verifies that a fast path returns the original task if it has already completed.
        /// </summary>
        [Fact]
        public void WithCancellationOfPrecanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetCanceled();
            var cts = new CancellationTokenSource();
            Assert.Same(tcs.Task, ((Task)tcs.Task).WithCancellation(cts.Token));
        }

        /// <summary>
        /// Verifies that a fast path returns the original task if it has already completed.
        /// </summary>
        [Fact]
        public void WithCancellationOfPrecanceledTaskOfT()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetCanceled();
            var cts = new CancellationTokenSource();
            Assert.Same(tcs.Task, tcs.Task.WithCancellation(cts.Token));
        }

        [SkippableFact]
        public void WithCancellationAndPrecancelledToken()
        {
            Skip.If(TestUtilities.IsNet45Mode, "This test verifies behavior that is only available on .NET 4.6.");

            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var result = ((Task)tcs.Task).WithCancellation(cts.Token);
            Assert.True(result.IsCanceled);

            // Verify that the CancellationToken that led to cancellation is tucked away in the returned Task.
            try
            {
                result.GetAwaiter().GetResult();
            }
            catch (TaskCanceledException ex)
            {
                Assert.Equal(cts.Token, ex.CancellationToken);
            }
        }

        [Fact]
        public void WithCancellationOfTAndPrecancelledToken()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.True(tcs.Task.WithCancellation(cts.Token).IsCanceled);
        }

        [Fact]
        public void WithCancellationOfTCanceled()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var t = tcs.Task.WithCancellation(cts.Token);
            Assert.False(t.IsCompleted);
            cts.Cancel();
            Assert.Throws<OperationCanceledException>(() =>
                t.GetAwaiter().GetResult());
        }

        [Fact]
        public void WithCancellationOfTCompleted()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var t = tcs.Task.WithCancellation(cts.Token);
            tcs.SetResult(new GenericParameterHelper());
            Assert.Same(tcs.Task.Result, t.GetAwaiter().GetResult());
        }

        [Fact]
        public void WithCancellationOfTNoDeadlockFromSyncContext()
        {
            var dispatcher = new DispatcherSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(dispatcher);
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource(AsyncDelay / 4);
            try
            {
                tcs.Task.WithCancellation(cts.Token).Wait(TestTimeout);
                Assert.True(false, "Expected OperationCanceledException not thrown.");
            }
            catch (AggregateException ex)
            {
                ex.Handle(x => x is OperationCanceledException);
            }
        }

        [Fact]
        public void WithCancellationOfTNoncancelableNoDeadlockFromSyncContext()
        {
            var dispatcher = new DispatcherSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(dispatcher);
            var tcs = new TaskCompletionSource<object>();
            Task.Run(async delegate
            {
                await Task.Delay(AsyncDelay);
                tcs.SetResult(null);
            });
            tcs.Task.WithCancellation(CancellationToken.None).Wait(TestTimeout);
        }

        [Fact]
        public void WithCancellationCanceled()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var t = ((Task)tcs.Task).WithCancellation(cts.Token);
            Assert.False(t.IsCompleted);
            cts.Cancel();
            Assert.Throws<OperationCanceledException>(() =>
                t.GetAwaiter().GetResult());
        }

        [Fact]
        public void WithCancellationCompleted()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var t = ((Task)tcs.Task).WithCancellation(cts.Token);
            tcs.SetResult(new GenericParameterHelper());
            t.GetAwaiter().GetResult();
        }

        [Fact]
        public void WithCancellationNoDeadlockFromSyncContext()
        {
            var dispatcher = new DispatcherSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(dispatcher);
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource(AsyncDelay / 4);
            try
            {
                ((Task)tcs.Task).WithCancellation(cts.Token).Wait(TestTimeout);
                Assert.True(false, "Expected OperationCanceledException not thrown.");
            }
            catch (AggregateException ex)
            {
                ex.Handle(x => x is OperationCanceledException);
            }
        }

        [Fact]
        public void WithCancellationNoncancelableNoDeadlockFromSyncContext()
        {
            var dispatcher = new DispatcherSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(dispatcher);
            var tcs = new TaskCompletionSource<object>();
            Task.Run(async delegate
            {
                await Task.Delay(AsyncDelay);
                tcs.SetResult(null);
            });
            ((Task)tcs.Task).WithCancellation(CancellationToken.None).Wait(TestTimeout);
        }
    }
}
