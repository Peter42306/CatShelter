using System.ComponentModel.DataAnnotations.Schema;

namespace CatShelter.Models.Animal
{
    public class Animal
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Sex Sex { get; set; }

        public DateOnly? BirthDate { get; set; }

        public bool IsBirthDateApproximate { get; set; }

        public bool IsSterilized { get; set; }

        public bool IsVaccinated { get; set; }

        public string? ShortDescription { get; set; }

        public string? Story { get; set; }

        public string? Features { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public int? SortOrder { get; set; }

        public Status Status { get; set; }

        public ICollection<Photo> Photos { get; set; } = [];

        public ICollection<Video> Videos { get; set; } = [];



        [NotMapped]
        public Age? Age => CalculateAge();
        
        private Age? CalculateAge()
        {
            if (BirthDate is null)
            {
                return null;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var birthDate = BirthDate.Value;

            var totalMonths = (today.Year - birthDate.Year) * 12 + today.Month - birthDate.Month;

            if (!IsBirthDateApproximate && today.Day < birthDate.Day)
            {
                totalMonths--;
            }

            totalMonths = Math.Max(0,totalMonths);

            var years = totalMonths / 12;
            var months = totalMonths % 12;

            return new Age(years, months);
        }
    }   
}
