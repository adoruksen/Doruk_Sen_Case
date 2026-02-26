namespace RubyCase.Core
{
    public interface ILevelSessionFactory
    {
        ILevelSession Create(int levelIndex, LevelContext context);
    }
}
