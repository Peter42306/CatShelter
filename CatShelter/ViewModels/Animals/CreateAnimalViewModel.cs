using CatShelter.Models.Animal;
using System.ComponentModel.DataAnnotations;

namespace CatShelter.ViewModels.Animals
{
    public class CreateAnimalViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Sex Sex { get; set; }

        public DateOnly? BirthDate { get; set; }

        public bool IsBirthDateApproximate { get; set; }

        public bool IsSterilized { get; set; }

        public bool IsVaccinated { get; set; }

        [StringLength(5000)]
        public string? ShortDescription { get; set; }

        [StringLength(5000)]
        public string? Story { get; set; }

        [StringLength(5000)]
        public string? Features { get; set; }

        public int? SortOrder { get; set; }

        public Status Status { get; set; } = Status.Available;
    }
}
