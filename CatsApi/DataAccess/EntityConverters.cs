using CatsApi.DataAccess.Entities;

namespace CatsApi.Models
{
    public static class EntityConverters
    {
        public static CatEntity ToDbEntity(CatEntity catEntity)
        {
            return catEntity;
        }

        public static TagEntity ToDbEntity(TagEntity catEntity)
        {
            catEntity.Name = catEntity.Name.ToLower();

            return catEntity;
        }
    }
}
