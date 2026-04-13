using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDataModels.Enums;
using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement.Models
{
    public class Grant : IGrantModel
    {
        public int Id { get; set; }

        [Required]
        public string ContestNumber { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Organization { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal? Amount { get; set; }

        public string? Currency { get; set; }

        public string? SubjectArea { get; set; }

        [Required]
        public GrantStatus Status { get; set; } = GrantStatus.Открыт;

        public string? Url { get; set; }

        public virtual List<GrantTag> GrantTags { get; set; } = new();

        public static Grant? Create(GrantBindingModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new Grant
            {
                Id = model.Id,
                ContestNumber = model.ContestNumber,
                Title = model.Title,
                Description = model.Description,
                Organization = model.Organization,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Amount = model.Amount,
                Currency = model.Currency,
                SubjectArea = model.SubjectArea,
                Status = model.Status,
                Url = model.Url
            };
        }

        public void Update(GrantBindingModel model)
        {
            if (model == null)
            {
                return;
            }

            ContestNumber = model.ContestNumber;
            Title = model.Title;
            Description = model.Description;
            Organization = model.Organization;
            StartDate = model.StartDate;
            EndDate = model.EndDate;
            Amount = model.Amount;
            Currency = model.Currency;
            SubjectArea = model.SubjectArea;
            Status = model.Status;
            Url = model.Url;
        }

        public GrantViewModel GetViewModel => new()
        {
            Id = Id,
            ContestNumber = ContestNumber,
            Title = Title,
            Description = Description,
            Organization = Organization,
            StartDate = StartDate,
            EndDate = EndDate,
            Amount = Amount,
            Currency = Currency,
            SubjectArea = SubjectArea,
            Status = Status,
            Url = Url
        };
    }
}
