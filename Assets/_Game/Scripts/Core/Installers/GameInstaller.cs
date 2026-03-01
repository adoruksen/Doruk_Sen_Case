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

        [Header("Scene References")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private LevelTestManager testManager;

        public override void InstallBindings()
        {
            Container.BindInstance(gameSettings).AsSingle();
            Container.BindInstance(levelDatabase).AsSingle();
            Container.BindInstance(addressableConfig).AsSingle();
            Container.BindInstance(levelCreationSettings).AsSingle();
            Container.BindInstance(poolSettings).AsSingle();
            Container.BindInterfacesTo<PoolManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<LevelManager>().AsSingle();
            Container.BindInterfacesTo<LevelInstantiator>().AsSingle().NonLazy();
            Container.BindInterfacesTo<LevelSessionFactory>().AsSingle();
            Container.BindInterfacesTo<CoreLoopService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UIManager>().FromInstance(uiManager).AsSingle();
            Container.Bind<LevelTestManager>().FromInstance(testManager).AsSingle();
            Container.BindInterfacesTo<GameManager>().AsSingle();
        }
    }
}
