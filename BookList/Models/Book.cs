using System.ComponentModel.DataAnnotations;

namespace BookList.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        public bool stateFinished { get; set; }
    }
}
