using RubyCase.Core.Audio;
using RubyCase.Core.GameLoop;
using RubyCase.Core.Level;
using RubyCase.Core.Session;
using RubyCase.Core.UI;
using RubyCase.LevelSystem;
using RubyCase.Pool;
using RubyCase.Testing;
using UnityEngine;
using Zenject;

namespace RubyCase.Core.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [Header("Scriptable Objects")]
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private LevelDatabase levelDatabase;
        [SerializeField] private AddressableGroupConfig addressableConfig;
        [SerializeField] private LevelCreationSettings levelCreationSettings;
        [SerializeField] private PoolSettings poolSettings;
        [SerializeField] private SoundSettings soundSettings;


        [Header("Scene References")]
        [SerializeField] private UIManager uiManager;

        public override void InstallBindings()
        {
            Container.BindInstance(gameSettings).AsSingle();
            Container.BindInstance(levelDatabase).AsSingle();
            Container.BindInstance(addressableConfig).AsSingle();
            Container.BindInstance(levelCreationSettings).AsSingle();
            Container.BindInstance(poolSettings).AsSingle();
            Container.BindInstance(soundSettings).AsSingle();
            Container.Bind<ISoundManager>().To<SoundManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<PoolManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<LevelManager>().AsSingle();
            Container.BindInterfacesTo<LevelInstantiator>().AsSingle().NonLazy();
            Container.BindInterfacesTo<LevelSessionFactory>().AsSingle();
            Container.BindInterfacesTo<CoreLoopService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UIManager>().FromInstance(uiManager).AsSingle();
            Container.BindInterfacesTo<GameManager>().AsSingle();
            Container.BindInterfacesTo<ConveyorManager>().AsSingle();
            Container.BindInterfacesTo<BenchManager>().AsSingle();
            Container.BindInterfacesTo<BoxManager>().AsSingle();
            Container.Bind<IBoxJourneyService>().To<BoxJourneyService>().AsSingle();
        }
    }
}
