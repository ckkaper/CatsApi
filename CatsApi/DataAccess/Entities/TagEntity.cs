using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatsApi.DataAccess.Entities
{
    [Table("TagEntity")]
    public class TagEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public List<CatEntity> Cats { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
