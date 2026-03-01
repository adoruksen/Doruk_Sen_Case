using RubyCase.Core.Session;
using Zenject;

namespace RubyCase.Core.Installers
{
    public class BoxSystemInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ConveyorManager>().AsSingle();
            Container.BindInterfacesTo<BenchManager>().AsSingle();
            Container.BindInterfacesTo<BoxManager>().AsSingle();
            Container.Bind<IBoxJourneyService>().To<BoxJourneyService>().AsSingle();
        }
    }
}