using Konscious.Security.Cryptography;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace iSketch.app.Services
{
    public class PassHashQueue
    {
        public bool QueueRunning = false;
        public Task<PassHashResult> GenerateHash(string Password)
        {
            Task<PassHashResult> tsk = new(RunHashAction, Password);
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
                        tsk.RunSynchronously();
                    }
                }
                QueueRunning = false;
            });
        }
        private ConcurrentQueue<Task<PassHashResult>> Queue = new();
        private Func<object, PassHashResult> RunHashAction = (object Password) =>
        {
            byte[] passSalt = new byte[128];
            new Random().NextBytes(passSalt);
            Argon2i a2 = new(Encoding.ASCII.GetBytes((string)Password))
            {
                DegreeOfParallelism = 1,
                MemorySize = 4882,
                Salt = passSalt,
                Iterations = 64
            };
            return new PassHashResult()
            {
                Salt = passSalt,
                Hash = a2.GetBytes(128)
            };
        };
    }
    public class PassHashResult {
        public byte[] Salt;
        public byte[] Hash;
    }
}
