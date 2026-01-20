using Konscious.Security.Cryptography;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace iSketch.app.Classes;

public class PassHashQueue
{
    public static PassHashQueue Default = new();
    public ILogger<PassHashQueue> Logger = Program.LoggerFactory.CreateLogger<PassHashQueue>();
    public bool QueueRunning = false;
    public Task<Result> GenerateHash(Request Request)
    {
        if (Request.Pass == null || Request.Pass == "") return Task.FromResult<Result>(null);
        Logger.LogInformation("Password queued.");
        Task<Result> tsk = new(RunHashAction, Request);
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
                Task<Result> tsk;
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
    private ConcurrentQueue<Task<Result>> Queue = new();
    private Func<object, Result> RunHashAction = (object Request) =>
    {
        Request req = (Request)Request;
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
        return new Result()
        {
            Salt = req.Salt,
            Hash = a2.GetBytes(128)
        };
    };
    public class Result
    {
        public byte[] Salt;
        public byte[] Hash;
    }
    public class Request
    {
        public byte[] Salt;
        public string Pass;
    }
}