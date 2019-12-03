using System;
using System.Threading;
using System.Threading.Tasks;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Spans;


namespace SampleConsoleApplication
{
    /// <summary>
    /// Sample program demonstrating the Telemetry SDK for Tracing.  It invokes the following methods, recording most of the work
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
        static void Main(string[] args)
        {

            Console.WriteLine("Welcome to the Telemetry SDK sample Application.  Press <Return> to start.");
            Console.ReadLine();

            DoHomework();
        }


        /// <summary>
        /// This method will be tracked as a span.  Since this is a topmost span, when this work is complete, the SpanBatch
        /// consisting of this span and all of its children will be sentt to New Relic.
        /// </summary>
        private static void DoHomework()
        {
            //Track work as span
            SimpleTracer.TrackWork(() =>
            {
                try
                {
                    Console.WriteLine("DoHomework");
                    SimpleTracer.CurrentSpan(s => s.WithAttribute("Method", "DoHomework"));

                    DoMathHomework(TimeSpan.FromSeconds(2));
                    TakeABreak(TimeSpan.FromSeconds(1));
                    DoScienceHomework();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("an Excpetion has occurred");
                }

            });
        }

        private static void DoMathHomework(TimeSpan forHowLong)
        {
            //Track work as span
            SimpleTracer.TrackWork(() =>
            {
                Console.WriteLine("DoMathHomework");
                SimpleTracer.CurrentSpan(s => s.WithAttribute("Method", "DoMathHomework"));
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
            Console.WriteLine("TakeABreak");
            Thread.Sleep(forHowLong);
        }

        private static void DoScienceHomework()
        {
            //Track work as span
            SimpleTracer.TrackWork(() =>
            {
                Console.WriteLine("DoScienceHomework");
                SimpleTracer.CurrentSpan(s => s.WithAttribute("Method", "DoScienceHomework"));

                DoComputerScienceHomework(TimeSpan.FromSeconds(2)); 

                TakeABreak(TimeSpan.FromSeconds(5));

                try
                {
                    CallMyBestFriend(TimeSpan.FromSeconds(2));
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }

                DoBiologyHomework(TimeSpan.FromSeconds(1));
            });
        }

        private static void DoComputerScienceHomework(TimeSpan forHowLong)
        {
            //Track work as span
            SimpleTracer.TrackWork(() => 
            {
                Console.WriteLine("DoComputerScienceHomework");
                SimpleTracer.CurrentSpan(s => s.WithAttribute("Method", "DoComputerScienceHomework"));
                Thread.Sleep(forHowLong);
            });
        }

        private static void CallMyBestFriend(TimeSpan forHowLong)
        {
            //Track work as span
            SimpleTracer.TrackWork(() =>
            {
                Console.WriteLine("CallMyBestFriend");
                SimpleTracer.CurrentSpan(s => s.WithAttribute("Method", "CallMyBestFriend"));

                throw new Exception("Get back to work!!!");
            });
        }


        private static void DoBiologyHomework(TimeSpan forHowLong)
        {
            //Track work as span
            SimpleTracer.TrackWork(() =>
            {
                Console.WriteLine("DoBiologyHomework");
                SimpleTracer.CurrentSpan(s => s.WithAttribute("Method", "DoBiologyHomework"));
                Thread.Sleep(forHowLong);
            });
        }

    }
}
