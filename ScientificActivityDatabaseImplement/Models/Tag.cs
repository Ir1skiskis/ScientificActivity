using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string NormalizedName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsSelectable { get; set; } = true;

        public virtual List<ResearcherTag> ResearcherTags { get; set; } = new();
        public virtual List<ConferenceTag> ConferenceTags { get; set; } = new();
        public virtual List<GrantTag> GrantTags { get; set; } = new();
        public virtual List<JournalTag> JournalTags { get; set; } = new();

        public static Tag? Create(TagBindingModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new Tag
            {
                Id = model.Id,
                Name = model.Name,
                NormalizedName = model.NormalizedName,
                IsActive = model.IsActive,
                IsSelectable = model.IsSelectable
            };
        }

        public void Update(TagBindingModel model)
        {
            if (model == null)
            {
                return;
            }

            Name = model.Name;
            NormalizedName = model.NormalizedName;
            IsActive = model.IsActive;
            IsSelectable = model.IsSelectable;
        }

        public TagViewModel GetViewModel => new()
        {
            Id = Id,
            Name = Name,
            NormalizedName = NormalizedName,
            IsActive = IsActive,
            IsSelectable = IsSelectable
        };
    }
}
