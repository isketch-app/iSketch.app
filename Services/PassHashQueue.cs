using iSketch.app.Classes.PassHash;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace iSketch.app.Services
{
    public class PassHashQueue
    {
        public ILogger<PassHashQueue> Logger;
        public PassHashQueue(ILogger<PassHashQueue> Logger)
        {
            this.Logger = Logger;
        }
        public bool QueueRunning = false;
        public Task<PassHashResult> GenerateHash(PassHashRequest Request)
        {
            if (Request.Pass == null || Request.Pass == "") return Task.FromResult<PassHashResult>(null);
            Logger.LogInformation("Password queued.");
            Task<PassHashResult> tsk = new(RunHashAction, Request);
            Queue.Enqueue(tsk);
            if (!QueueRunning) _ = RunQueue();
            return tsk;
        }
        private Task RunQueue()
        {
            return Task.Run(() =>
            {
                QueueRunning = true;
                while (true)
                {
                    if (Queue.Count == 0) break;
                    Task<PassHashResult> tsk;
                    if (Queue.TryDequeue(out tsk))
                    {
                        Logger.LogInformation("Hashing...");
                        tsk.RunSynchronously();
                        Logger.LogInformation("{0} left in the queue.", Queue.Count);
                    }
                }
                QueueRunning = false;
            });
        }
        private ConcurrentQueue<Task<PassHashResult>> Queue = new();
        private Func<object, PassHashResult> RunHashAction = (object Request) =>
        {
            PassHashRequest req = (PassHashRequest)Request;
            if (req.Salt == null)
            {
                req.Salt = new byte[128];
                new Random().NextBytes(req.Salt);
            }
            Argon2i a2 = new(Encoding.ASCII.GetBytes(req.Pass))
            {
                DegreeOfParallelism = 1,
                MemorySize = 4882,
                Salt = req.Salt,
                Iterations = 64
            };
            return new PassHashResult()
            {
                Salt = req.Salt,
                Hash = a2.GetBytes(128)
            };
        };
    }
    public class PassHashResult
    {
        public byte[] Salt;
        public byte[] Hash;
    }
    public class PassHashRequest
    {
        public byte[] Salt;
        public string Pass;
    }
}