using RubyCase.Core.Level;

namespace RubyCase.Core.Session
{
    public interface ILevelSessionFactory
    {
        ILevelSession Create(int levelIndex, LevelContext context);
    }
}
