using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement.Models
{
    public class ResearcherInterest : IResearcherInterestModel
    {
        public int Id { get; set; }

        [Required]
        public int ResearcherId { get; set; }

        [Required]
        public string Keyword { get; set; } = string.Empty;

        [Required]
        public decimal Weight { get; set; }

        public virtual Researcher Researcher { get; set; } = null!;

        public static ResearcherInterest? Create(ResearcherInterestBindingModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new ResearcherInterest
            {
                Id = model.Id,
                ResearcherId = model.ResearcherId,
                Keyword = model.Keyword,
                Weight = model.Weight
            };
        }

        public void Update(ResearcherInterestBindingModel model)
        {
            if (model == null)
            {
                return;
            }

            ResearcherId = model.ResearcherId;
            Keyword = model.Keyword;
            Weight = model.Weight;
        }

        public ResearcherInterestViewModel GetViewModel => new()
        {
            Id = Id,
            ResearcherId = ResearcherId,
            Keyword = Keyword,
            Weight = Weight,
            ResearcherFullName = Researcher == null
                ? string.Empty
                : string.IsNullOrWhiteSpace(Researcher.MiddleName)
                    ? $"{Researcher.LastName} {Researcher.FirstName}"
                    : $"{Researcher.LastName} {Researcher.FirstName} {Researcher.MiddleName}"
        };
    }
}
