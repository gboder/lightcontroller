namespace Common
{
    public interface IEntity<K>
        where K : struct
    {
        K Id { get; set; }
    }

    public class KeyedEntityBase<K> : IEntity<K>
        where K : struct
    {
        protected virtual K NotSavedId => default(K);
        public K Id { get; set; }
    }

    public class EntityBase : KeyedEntityBase<int>
    {
        public EntityBase()
        {

        }
    }
}