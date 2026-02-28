using RubyCase.Core;
using UnityEngine;
using Zenject;

namespace RubyCase.BoxSystem
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