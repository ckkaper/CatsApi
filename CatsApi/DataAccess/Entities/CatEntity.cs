using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatsApi.DataAccess.Entities
{

    [Table("CatEntity")]
    public class CatEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]

        public string CatId { get; set; }

        [Required]

        public int Width { get; set; }

        [Required]

        public int Height { get; set; }

        public string Image { get; set; }

        [Required]
        [StringLength(100)]
        public DateTime Created { get; set; }

        [Required]
        public List<TagEntity> Tags { get; set; }

    }
}
