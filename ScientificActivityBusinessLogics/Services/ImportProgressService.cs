using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.Services
{
    public class ImportProgressService
    {
        private readonly ConcurrentDictionary<string, ImportProgressViewModel> _jobs = new();

        public ImportProgressViewModel CreateJob(string title)
        {
            var job = new ImportProgressViewModel
            {
                JobId = Guid.NewGuid().ToString("N"),
                Title = title,
                StatusText = "Задача поставлена в очередь",
                Percent = 0,
                Current = 0,
                Total = null,
                IsCompleted = false,
                IsFailed = false,
                StartedAt = DateTime.UtcNow
            };

            _jobs[job.JobId] = job;

            return job;
        }

        public ImportProgressViewModel? GetJob(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return null;
            }

            return _jobs.TryGetValue(jobId, out var job) ? job : null;
        }

        public void Update(
            string jobId,
            string statusText,
            int? current = null,
            int? total = null,
            int? percent = null)
        {
            if (!_jobs.TryGetValue(jobId, out var job))
            {
                return;
            }

            job.StatusText = statusText;

            if (current.HasValue)
            {
                job.Current = current.Value;
            }

            if (total.HasValue)
            {
                job.Total = total.Value;
            }

            if (percent.HasValue)
            {
                job.Percent = Math.Clamp(percent.Value, 0, 100);
            }
            else if (job.Current.HasValue && job.Total.HasValue && job.Total.Value > 0)
            {
                job.Percent = Math.Clamp((int)Math.Round(job.Current.Value * 100.0 / job.Total.Value), 0, 100);
            }

            job.EstimatedSecondsLeft = CalculateEstimatedSecondsLeft(job);
        }

        public void Complete(string jobId, string statusText = "Импорт завершен")
        {
            if (!_jobs.TryGetValue(jobId, out var job))
            {
                return;
            }

            job.StatusText = statusText;
            job.Percent = 100;
            job.IsCompleted = true;
            job.IsFailed = false;
            job.FinishedAt = DateTime.UtcNow;
            job.EstimatedSecondsLeft = 0;
        }

        public void Fail(string jobId, Exception ex)
        {
            if (!_jobs.TryGetValue(jobId, out var job))
            {
                return;
            }

            job.StatusText = "Импорт завершился с ошибкой";
            job.IsCompleted = true;
            job.IsFailed = true;
            job.ErrorMessage = ex.Message;
            job.FinishedAt = DateTime.UtcNow;
        }

        private static int? CalculateEstimatedSecondsLeft(ImportProgressViewModel job)
        {
            if (!job.Current.HasValue ||
                !job.Total.HasValue ||
                job.Total.Value <= 0 ||
                job.Current.Value <= 0 ||
                job.Current.Value >= job.Total.Value)
            {
                return null;
            }

            var elapsedSeconds = (DateTime.UtcNow - job.StartedAt).TotalSeconds;
            if (elapsedSeconds <= 0)
            {
                return null;
            }

            var secondsPerItem = elapsedSeconds / job.Current.Value;
            var remainingItems = job.Total.Value - job.Current.Value;
            var estimatedSecondsLeft = (int)Math.Round(secondsPerItem * remainingItems);

            return Math.Max(estimatedSecondsLeft, 0);
        }
    }
}
