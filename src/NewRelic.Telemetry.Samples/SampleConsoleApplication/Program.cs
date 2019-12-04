using System;
using System.Threading;

namespace SampleConsoleApplication
{
    /// <summary>
    /// Sample program demonstrating the Telemetry SDK for Tracing.  It invokes the following methods, recording the work
    /// as spans.
    /// 
    /// DoHomework                                              Tracked as a Span with 2-child spans
    ///     DoMathHomework                                      Tracked as a span
    ///     TakeABreak                                          Time Accounted for in DoHomework, but not tracked as a span
    ///     DoScienceHomework                                   Tracked as a span with 2-child spans
    ///         DoComputerScienceHomework                       Tracked as a span
    ///         TakeABreak                                      Time Accounted for in DoScienceHomework, but not tracked as a span
    ///         CallMyBestFriend                                Tracked as a span in which Exception occurs.  But the exception is handled in DoScienceHomework
    ///         DoBiologyHomework                               Tracked as a span
    ///
    /// </summary>
    class Program
    {

        public static void Main(string[] args)
        {
            SimpleTracer.WithDefaultConfiguration("Your API Key Here");
            SimpleTracer.EnableTracing();

            Console.WriteLine("Welcome to the Telemetry SDK sample Application.");
            Console.WriteLine(new String('-', 100));

            DoHomework();

            SimpleTracer.DisableTracing();
        }

        /// <summary>
        /// This method will be tracked as a span.  Since this is a topmost span, when this work is complete, the SpanBatch
        /// consisting of this span and all of its children will be sentt to New Relic.
        /// </summary>
        private static void DoHomework()
        {
            //Track work as span
            SimpleTracer.TrackWork("Do Homework", () =>
            {
                DoMathHomework(TimeSpan.FromSeconds(2));
                TakeABreak(TimeSpan.FromSeconds(1));
                DoScienceHomework();

            });
        }

        private static void DoMathHomework(TimeSpan forHowLong)
        {
            //Track work as span
            SimpleTracer.TrackWork("Do Math Homework",() =>
            {
                Thread.Sleep(forHowLong);
            });
        }

        /// <summary>
        /// This method introduces a delay that is not tracked by a span.
        /// Its time would be accounted for in the caller's span.
        /// </summary>
        /// <param name="forHowLong"></param>
        private static void TakeABreak(TimeSpan forHowLong)
        {
            SimpleTracer.CurrentSpan((s) => { s.WithAttribute("Length of Break", forHowLong.TotalSeconds); });
            Thread.Sleep(forHowLong);
        }

        private static void DoScienceHomework()
        {
            //Track work as span
            SimpleTracer.TrackWork("Do Science Homework",() =>
            {
                DoComputerScienceHomework(TimeSpan.FromSeconds(1)); 
                TakeABreak(TimeSpan.FromSeconds(2));

                try
                {
                    CallMyBestFriend(TimeSpan.FromSeconds(20));
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine($"Captured Expected Exception - {ex.Message}");
                }

                DoBiologyHomework(TimeSpan.FromSeconds(1));
            });
        }

        private static void DoComputerScienceHomework(TimeSpan forHowLong)
        {
            //Track work as span
            SimpleTracer.TrackWork("Do Computer Science Homework", () =>
             {
                 SimpleTracer.CurrentSpan(s => s.WithAttribute("Method", "DoComputerScienceHomework"));
                 Thread.Sleep(forHowLong);
             });
        }

        private static void CallMyBestFriend(TimeSpan forHowLong)
        {
            //Track work as span
            SimpleTracer.TrackWork("Call My Bestie",() =>
            {
                throw new Exception("Get back to work!!!");
            });
        }


        private static void DoBiologyHomework(TimeSpan forHowLong)
        {
            //Track work as span
            SimpleTracer.TrackWork("Do Biology Homework", () =>
            {
                Thread.Sleep(forHowLong);
            });
        }

    }
}
