namespace RubyCase.Pool
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}