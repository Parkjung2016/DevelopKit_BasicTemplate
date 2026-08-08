using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PJDev.DevelopKit.BasicTemplate.Runtime.Tests
{
    [TestFixture]
    public sealed class BasicTemplateCompatibilityTests
    {
        private readonly List<GameObject> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                    Object.DestroyImmediate(createdObjects[i]);
            }

            createdObjects.Clear();
        }

        [Test]
        public void ComponentUtil_FindsNestedChildWhenRecursive()
        {
            GameObject root = Create("Root");
            GameObject branch = Create("Branch", root.transform);
            GameObject target = Create("Target", branch.transform);

            Assert.AreSame(target, ComponentUtil.FindChild(root, "Target", recursive: true));
            Assert.IsNull(ComponentUtil.FindChild(root, "Target"));
        }

        [Test]
        public void GetOrAdd_ReturnsExistingComponent()
        {
            GameObject target = Create("Target");

            BoxCollider first = target.GetOrAdd<BoxCollider>();
            BoxCollider second = ComponentUtil.GetOrAddComponent<BoxCollider>(target);

            Assert.AreSame(first, second);
            Assert.AreEqual(1, target.GetComponents<BoxCollider>().Length);
        }

        [Test]
        public void SerializableDictionary_RestoresSerializedEntries()
        {
            var dictionary = new SerializableDictionary<string, int>
            {
                ["Health"] = 100,
                ["Attack"] = 20
            };

            dictionary.OnBeforeSerialize();
            dictionary.Clear();
            dictionary.OnAfterDeserialize();

            Assert.AreEqual(100, dictionary["Health"]);
            Assert.AreEqual(20, dictionary["Attack"]);
        }

        [Test]
        public void CollectionCompatibilityMethods_KeepExpectedContents()
        {
            var values = new List<int> { 1, 2, 3 };
            values.RefreshWith(new[] { 4, 5, 6 });
            values.Shuffle();
            values.Sort();

            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, values);
        }

        private GameObject Create(string name, Transform parent = null)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            createdObjects.Add(gameObject);
            return gameObject;
        }
    }
}
