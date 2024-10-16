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
        [StringLength(100)]
        public string CatId { get; set; }

        [Required]
        [StringLength(100)]
        public int Width { get; set; }

        [Required]
        [StringLength(100)]
        public int Height { get; set; }

        [Required]
        [StringLength(100)]
        public string Image { get; set; }

        [Required]
        [StringLength(100)]
        public DateTime CreatedAt { get; set; }

        [Required]
        public List<TagEntity> Tags { get; set; }

    }
}
