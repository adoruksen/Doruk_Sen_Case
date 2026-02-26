using RubyCase.Core;
using RubyCase.LevelSystem;
using RubyCase.UI;
using UnityEngine;
using Zenject;

namespace RubyCase.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [Header("Scriptable Objects")]
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private LevelDatabase levelDatabase;
        [SerializeField] private AddressableGroupConfig addressableConfig;

        [Header("Scene References")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private LevelTestManager testManager;

        public override void InstallBindings()
        {
            Container.BindInstance(gameSettings).AsSingle();
            Container.BindInstance(levelDatabase).AsSingle();
            Container.BindInstance(addressableConfig).AsSingle();

            Container.BindInterfacesTo<LevelManager>().AsSingle();

            Container.BindInterfacesTo<LevelInstantiator>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<LevelSessionFactory>().AsSingle();

            Container.BindInterfacesTo<CoreLoopService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<UIManager>()
                .FromInstance(uiManager)
                .AsSingle();

            if (testManager != null)
            {
                Container.Bind<LevelTestManager>()
                    .FromInstance(testManager)
                    .AsSingle();
            }

            Container.BindInterfacesTo<GameManager>().AsSingle();
        }
    }
}
