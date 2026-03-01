using RubyCase.Core.Level;

namespace RubyCase.Core.Session
{
    public sealed class LevelSessionFactory : ILevelSessionFactory
    {
        public ILevelSession Create(int levelIndex, LevelContext context)
        {
            var boxes = new BoxManager();
            var collectables = new CollectableManager();

            var session = new LevelSession(levelIndex, boxes, collectables);
            session.Initialize(context);
            return session;
        }
    }
}
