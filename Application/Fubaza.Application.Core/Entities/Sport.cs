using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Fubaza.Application.Core.Entities
{
    public class Sport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? NameDe { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<PlayingPosition>? PlayingPosition { get; set; }
        public virtual ICollection<EventType>? EventType { get; set; }

        [NotMapped]

        public string? NormalizedName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return null;

                // Split words by spaces
                var words = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length == 0)
                    return null;

                // Make the first word lowercase
                var displayName = words[0].ToLower(CultureInfo.InvariantCulture);

                // Capitalize the rest
                for (int i = 1; i < words.Length; i++)
                {
                    var word = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[i].ToLower());
                    displayName += word;
                }

                return displayName;
            }
        }
    }
}
