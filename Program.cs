using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace HttpTitle
{
    class Program
    {
        //static Queue q = new Queue();
       // static FileStream file = new FileStream("result.txt", FileMode.Create, FileAccess.ReadWrite);
        static List<string> dirList = new List<string>();
        static List<string> urls = new List<string>();
        static async Task Main(string[] args)
        {
            string line;
            List<string> domainList = new List<string>();
            List<string> portList = new List<string>();

            TaskFactory fac = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(200));
            //CancellationTokenSource cts = new CancellationTokenSource();
            List<Task> tasks = new List<Task>();
            //Task[] tasks = new Task[] { };
            System.IO.StreamReader file = new System.IO.StreamReader(args[0]);
            while ((line = file.ReadLine()) != null)
            {
                domainList.Add(line);
            }
            //file = new StreamReader(args[3]);
            //while ((line = file.ReadLine()) != null)
            //{
            //    dirList.Add(line);
            //}
            if (args[1].Contains(','))
            {
                string[] ports = args[1].Split(',');
                foreach (string p in ports)
                {
                    if (p.Contains('-'))
                    {
                        string[] portRange = p.Split('-');
                        for (int i = int.Parse(portRange[0]); i <= int.Parse(portRange[1]); i++)
                        {
                            portList.Add(i.ToString());
                        }
                    }
                    else
                        portList.Add(p);
                }
            }
            else
            {
                if (args[1].Contains('-'))
                {
                    string[] portRange = args[1].Split('-');
                    for (int i = int.Parse(portRange[0]); i <= int.Parse(portRange[1]); i++)
                    {
                        portList.Add(i.ToString());
                    }
                }
                else
                    portList.Add(args[1]);
            }
            foreach (string domain in domainList)
            {
                foreach (string port in portList)
                {
                    //tasks.Add(Task.Run(() => HttpTitle(domain + ":" + port)));
                    
                    tasks.Add(fac.StartNew(() =>
                    {
                        
                        HttpTitle(domain + ":" + port);
                    }));
                }
            }

            Task.WaitAll(tasks.ToArray());
            //test();
            // cts.Dispose();
            //tasks.Clear();
            //foreach (string url in urls)
            //{
            //    foreach (string dir in dirList)
            //    {
            //        tasks.Add(Task.Run(() => CheckDir(url, dir)));
            //    }
            //}
            //Task.WaitAll(tasks.ToArray());
            Console.WriteLine("结束");
            Console.ReadKey();

        }
        static async Task HttpTitle(string pars)
        {
            string url = "";
            string title = "";
            int code = 0;
            HttpClient httpClient = new HttpClient();
            var dirTask = new List<Task> ();
            httpClient.Timeout = TimeSpan.FromSeconds(3);
            try
            {
                url = "http://" + pars;
                await httpClient.GetAsync(url).ContinueWith(
                    (requestTask) =>
                    {
                        HttpResponseMessage response = requestTask.Result;
                        code = Convert.ToInt32(response.StatusCode);
                        response.Content.ReadAsStringAsync().ContinueWith(
                            (readTask) =>
                            {
                                title = Regex.Match(readTask.Result, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                                Console.WriteLine("{0}         {1}        {2}", code.ToString(), title, url);
                               // return url;
                            });
                    });
            }
            catch (AggregateException aggex)
            {
                //HttpResponseMessage response = await httpClient.GetAsync("https://" + url);
                try
                {
                    url = "https://" + pars;
                    await httpClient.GetAsync(url).ContinueWith(
                        (requestTask) =>
                        {
                            HttpResponseMessage response = requestTask.Result;
                            code = Convert.ToInt32(response.StatusCode);
                            response.Content.ReadAsStringAsync().ContinueWith(
                                (readTask) =>
                                {
                                    title = Regex.Match(readTask.Result, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                                    Console.WriteLine("{0}         {1}        {2}", code.ToString(), title, url);
                                });
                        });
                }
                catch (TaskCanceledException taskex)
                {
                    //Console.WriteLine(taskex.ToString());
                }
                catch (WebException webex)
                {
                    //Console.WriteLine(webex.ToString());
                }
                catch (HttpRequestException httpex)
                {
                    //Console.WriteLine(httpex.ToString());
                }
                catch (AggregateException aggex1)
                { 
                
                }
                catch(UriFormatException uri)
                { 
                }
                //Console.WriteLine("https " + "\n" + response.Headers);
            }
            catch (TaskCanceledException taskex)
            {
                //Console.WriteLine(taskex.ToString());
            }
            catch (WebException webex)
            {
                //Console.WriteLine(webex.ToString());
            }
            catch (HttpRequestException httpex)
            {
                //Console.WriteLine(httpex.ToString());
            }
            catch (UriFormatException uri)
            {
            }
            

        }
        static async Task CheckDir(string url,string dir)
        {
            using (HttpClient httpClient = new HttpClient())
            {
               // Console.WriteLine(url+dir);
                httpClient.Timeout = TimeSpan.FromSeconds(3);
                try
                {
                    await httpClient.GetAsync(url + "/" + dir).ContinueWith(
                        (requestTask) =>
                        {
                            HttpResponseMessage response = requestTask.Result;
                            int code = Convert.ToInt32(response.StatusCode);
                            response.Content.ReadAsStringAsync().ContinueWith(
                                (readTask) =>
                                {
                                   // string title = Regex.Match(readTask.Result, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                                    //if (code == 200 || code == 401 || code == 302)
                                        Console.WriteLine("{0}         {1}        ", code.ToString(), url+"/"+dir);
                                    // return url;
                                });
                        });
                }
                catch (TaskCanceledException taskex)
                {
                    //Console.WriteLine(taskex.ToString());
                }
                catch (WebException webex)
                {
                    //Console.WriteLine(webex.ToString());
                }
                catch (HttpRequestException httpex)
                {
                    //Console.WriteLine(httpex.ToString());
                }
                catch (UriFormatException uri)
                {
                }
                catch (AggregateException aggex)
                { 
                }
            }
        }

    }
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        /// <summary>Whether the current thread is processing work items.</summary> 
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;
        /// <summary>The list of tasks to be executed.</summary> 
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks) 
        /// <summary>The maximum concurrency level allowed by this scheduler.</summary> 
        private readonly int _maxDegreeOfParallelism;
        /// <summary>Whether the scheduler is currently processing work items.</summary> 
        private int _delegatesQueuedOrRunning = 0; // protected by lock(_tasks) 

        /// <summary> 
        /// Initializes an instance of the LimitedConcurrencyLevelTaskScheduler class with the 
        /// specified degree of parallelism. 
        /// </summary> 
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism provided by this scheduler.</param> 
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>
        /// current executing number;
        /// </summary>
        public int CurrentCount { get; set; }

        /// <summary>Queues a task to the scheduler.</summary> 
        /// <param name="task">The task to be queued.</param> 
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed. If there aren't enough 
            // delegates currently queued or running to process tasks, schedule another. 
            lock (_tasks)
            {
                // Console.WriteLine("Task Count : {0} ", _tasks.Count);
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }
        int executingCount = 0;
        private static object executeLock = new object();
        /// <summary> 
        /// Informs the ThreadPool that there's work to be executed for this scheduler. 
        /// </summary> 
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items. 
                // This is necessary to enable inlining of tasks into this thread. 
                _currentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue. 
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // When there are no more items to be processed, 
                            // note that we're done processing, and get out. 
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;

                                break;
                            }

                            // Get the next item from the queue 
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }


                        // Execute the task we pulled out of the queue 
                        base.TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread 
                finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }

        /// <summary>Attempts to execute the specified task on the current thread.</summary> 
        /// <param name="task">The task to be executed.</param> 
        /// <param name="taskWasPreviouslyQueued"></param> 
        /// <returns>Whether the task could be executed on the current thread.</returns> 
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {

            // If this thread isn't already processing a task, we don't support inlining 
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue 
            if (taskWasPreviouslyQueued) TryDequeue(task);

            // Try to run the task. 
            return base.TryExecuteTask(task);
        }

        /// <summary>Attempts to remove a previously scheduled task from the scheduler.</summary> 
        /// <param name="task">The task to be removed.</param> 
        /// <returns>Whether the task could be found and removed.</returns> 
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary> 
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        /// <summary>Gets an enumerable of the tasks currently scheduled on this scheduler.</summary> 
        /// <returns>An enumerable of the tasks currently scheduled.</returns> 
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks.ToArray();
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }

}
