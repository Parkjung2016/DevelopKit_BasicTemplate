using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PJDev.DevelopKit.BasicTemplate.Runtime.Tests
{
    [TestFixture]
    public sealed class ComponentManagerTests
    {
        private readonly List<GameObject> createdObjects = new();

        [SetUp]
        public void SetUp() => ComponentManagerTestLog.Calls.Clear();

        [TearDown]
        public void TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                    Object.DestroyImmediate(createdObjects[i]);
            }

            createdObjects.Clear();
            ComponentManagerTestLog.Calls.Clear();
        }

        [Test]
        public void Initialize_UsesComponentOrderForAllCallbacks()
        {
            ComponentManagerTestOwner owner = CreateOwner();
            owner.gameObject.AddComponent<LateManagedComponent>();
            owner.gameObject.AddComponent<EarlyManagedComponent>();
            var manager = new ComponentManager();

            manager.AddComponentToDictionary(owner);
            manager.ComponentInitialize(owner);
            manager.AfterInitialize();
            manager.OnUpdate();

            CollectionAssert.AreEqual(
                new[] { "Early.Initialize", "Late.Initialize", "Early.After", "Late.After", "Early.Update", "Late.Update" },
                ComponentManagerTestLog.Calls);
            Assert.AreEqual(2, manager.Count);
            Assert.IsTrue(manager.IsInitialized);
        }

        [Test]
        public void CollectAgain_ReplacesPreviousCallbackLists()
        {
            ComponentManagerTestOwner owner = CreateOwner();
            owner.gameObject.AddComponent<EarlyManagedComponent>();
            var manager = new ComponentManager();

            InitializeAndTick(manager, owner);
            ComponentManagerTestLog.Calls.Clear();
            InitializeAndTick(manager, owner);

            CollectionAssert.AreEqual(
                new[] { "Early.Initialize", "Early.After", "Early.Update" },
                ComponentManagerTestLog.Calls);
        }

        [Test]
        public void Lookup_CanIncludeDerivedTypes()
        {
            ComponentManagerTestOwner owner = CreateOwner();
            EarlyManagedComponent expected = owner.gameObject.AddComponent<EarlyManagedComponent>();
            var manager = new ComponentManager();
            manager.AddComponentToDictionary(owner);

            Assert.AreSame(expected, manager.GetCompo<EarlyManagedComponent>());
            Assert.AreSame(expected, manager.GetCompo<ManagedComponentBase>(isDerived: true));
            Assert.IsNull(manager.GetCompo<ManagedComponentBase>());
        }

        [Test]
        public void Initialize_SkipsComponentsForDifferentOwnerType()
        {
            ComponentManagerTestOwner owner = CreateOwner();
            owner.gameObject.AddComponent<EarlyManagedComponent>();
            owner.gameObject.AddComponent<OtherOwnerManagedComponent>();
            var manager = new ComponentManager();

            InitializeAndTick(manager, owner);

            CollectionAssert.AreEqual(
                new[] { "Early.Initialize", "Early.After", "Early.Update" },
                ComponentManagerTestLog.Calls);
        }

        [Test]
        public void BaseOwner_EnableComponents_UsesRequestedValue()
        {
            ComponentManagerTestOwner owner = CreateOwner();
            EarlyManagedComponent component = owner.gameObject.AddComponent<EarlyManagedComponent>();
            owner.InitializeManagedComponents();

            owner.EnableComponents(false);
            Assert.IsFalse(component.enabled);

            owner.EnableComponents(true);
            Assert.IsTrue(component.enabled);
        }

        private ComponentManagerTestOwner CreateOwner()
        {
            var gameObject = new GameObject("ComponentManagerTestOwner");
            createdObjects.Add(gameObject);
            return gameObject.AddComponent<ComponentManagerTestOwner>();
        }

        private static void InitializeAndTick(ComponentManager manager, ComponentManagerTestOwner owner)
        {
            manager.AddComponentToDictionary(owner);
            manager.ComponentInitialize(owner);
            manager.AfterInitialize();
            manager.OnUpdate();
        }
    }

    public sealed class ComponentManagerTestOwner : BaseComponentOwner<ComponentManagerTestOwner>
    {
        public void InitializeManagedComponents() => InitComponent(this);
    }

    public abstract class ManagedComponentBase : MonoBehaviour, IObjectComponentBase
    {
    }

    [ComponentOrder(-10)]
    public sealed class EarlyManagedComponent : ManagedComponentBase,
        IObjectComponent<ComponentManagerTestOwner>, IAfterInitable, IUpdatable
    {
        public void Initialize(ComponentManagerTestOwner owner) => ComponentManagerTestLog.Calls.Add("Early.Initialize");
        public void AfterInitialize() => ComponentManagerTestLog.Calls.Add("Early.After");
        public void OnUpdate() => ComponentManagerTestLog.Calls.Add("Early.Update");
    }

    [ComponentOrder(10)]
    public sealed class LateManagedComponent : ManagedComponentBase,
        IObjectComponent<ComponentManagerTestOwner>, IAfterInitable, IUpdatable
    {
        public void Initialize(ComponentManagerTestOwner owner) => ComponentManagerTestLog.Calls.Add("Late.Initialize");
        public void AfterInitialize() => ComponentManagerTestLog.Calls.Add("Late.After");
        public void OnUpdate() => ComponentManagerTestLog.Calls.Add("Late.Update");
    }

    public sealed class OtherComponentOwner : MonoBehaviour
    {
    }

    public sealed class OtherOwnerManagedComponent : ManagedComponentBase,
        IObjectComponent<OtherComponentOwner>, IUpdatable
    {
        public void Initialize(OtherComponentOwner owner) => ComponentManagerTestLog.Calls.Add("Other.Initialize");
        public void OnUpdate() => ComponentManagerTestLog.Calls.Add("Other.Update");
    }

    internal static class ComponentManagerTestLog
    {
        internal static readonly List<string> Calls = new();
    }
}
