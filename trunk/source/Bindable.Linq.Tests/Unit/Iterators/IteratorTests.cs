using System;

namespace Bindable.Linq.Tests.Unit.Iterators
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using Bindable.Linq.Dependencies.Definitions;
    using Bindable.Linq.Iterators;
    using Collections;
    using NUnit.Framework;
    using TestHelpers;
    using TestObjectModel;

    [TestFixture]
    public class IteratorTests : TestFixture
    {
        private static void ForcesLoad(BindableCollection<Contact> contacts, Action<SimpleIterator<Contact>> action)
        {
            var sourceCollection = new SourceCollection(contacts);
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            var contactIterator = new SimpleIterator<Contact>(sourceCollection.AsBindable());
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            action(contactIterator);
            Assert.AreEqual(1, sourceCollection.GetEnumeratorCalls);

            contactIterator.Dispose();
            Assert.AreEqual(1, sourceCollection.GetEnumeratorCalls);
        }

        private static void ForcesLoad(BindableCollection<Contact> contacts, Func<SimpleIterator<Contact>, object> action)
        {
            var sourceCollection = new SourceCollection(contacts);
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            var contactIterator = new SimpleIterator<Contact>(sourceCollection.AsBindable());
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            action(contactIterator);
            Assert.AreEqual(1, sourceCollection.GetEnumeratorCalls);

            contactIterator.Dispose();
            Assert.AreEqual(1, sourceCollection.GetEnumeratorCalls);
        }

        private static void DoesNotForceLoad(BindableCollection<Contact> contacts, Action<SimpleIterator<Contact>> action)
        {
            var sourceCollection = new SourceCollection(contacts);
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            var contactIterator = new SimpleIterator<Contact>(sourceCollection.AsBindable());
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            action(contactIterator);
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            contactIterator.Dispose();
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);
        }

        private static void DoesNotForceLoad(BindableCollection<Contact> contacts, Func<SimpleIterator<Contact>, object> action)
        {
            var sourceCollection = new SourceCollection(contacts);
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            var contactIterator = new SimpleIterator<Contact>(sourceCollection.AsBindable());
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            action(contactIterator);
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            contactIterator.Dispose();
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);
        }

        private static void IsNotSupported(Action action)
        {
            bool threw = false;
            try
            {
                action();
            }
            catch (NotSupportedException)
            {
                threw = true;
            }
            if (!threw)
            {
                Assert.Fail("Exception was not thrown.");
            }
        }

        private class SourceCollection : IEnumerable<Contact>
        {
            private readonly BindableCollection<Contact> _inner;
            private int _getEnumeratorCalls;

            public SourceCollection(BindableCollection<Contact> inner)
            {
                _inner = inner;
            }

            public int GetEnumeratorCalls
            {
                get { return _getEnumeratorCalls; }
            }

            #region IEnumerable<Contact> Members
            public IEnumerator<Contact> GetEnumerator()
            {
                _getEnumeratorCalls++;
                return _inner.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }

        private class SimpleIterator<TElement> : Iterator<TElement, TElement>
            where TElement : class
        {
            public SimpleIterator(IBindableCollection<TElement> sourceCollection)
                : base(sourceCollection) {}

            private void EnsureLockIsNotHeld()
            {
                bool isHeld = true;
                var are = new AutoResetEvent(false);
                ThreadPool.QueueUserWorkItem(delegate
                {
                    if (Monitor.TryEnter(IteratorLock, TimeSpan.FromMilliseconds(10)))
                    {
                        Monitor.Exit(IteratorLock);
                        isHeld = false;
                    }
                    are.Set();
                });
                are.WaitOne();
                if (isHeld)
                {
                    Assert.Fail("Lock is held");
                }
            }

            protected override void LoadSourceCollection()
            {
                EnsureLockIsNotHeld();
                ReactToAddRange(0, SourceCollection);
            }

            protected override void ReactToAddRange(int sourceStartingIndex, IEnumerable<TElement> addedItems)
            {
                EnsureLockIsNotHeld();
                ResultCollection.AddOrInsertRange(sourceStartingIndex, addedItems);
            }

            protected override void ReactToMoveRange(int sourceStartingIndex, IEnumerable<TElement> movedItems)
            {
                EnsureLockIsNotHeld();
            }

            protected override void ReactToRemoveRange(IEnumerable<TElement> removedItems)
            {
                EnsureLockIsNotHeld();
            }

            protected override void ReactToReplaceRange(IEnumerable<TElement> oldItems, IEnumerable<TElement> newItems)
            {
                EnsureLockIsNotHeld();
            }

            protected override void ReactToItemPropertyChanged(TElement item, string propertyName)
            {
                EnsureLockIsNotHeld();
            }
        }

        [Test]
        public void IteratorIListMethodsForceLoad()
        {
            ForcesLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.Contains(Tom));
            ForcesLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.GetEnumerator());
            ForcesLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.IndexOf(Tom));
            ForcesLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.Count);
            ForcesLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator[2]);
            ForcesLoad(Given.Collection(Tom, Sam, Sally), iterator => ((IList) iterator)[2]);
            ForcesLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.CopyTo(new Contact[3], 0));
            DoesNotForceLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.Contains(new object()));
            DoesNotForceLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.IndexOf(new object()));
            DoesNotForceLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.CurrentCount);
            DoesNotForceLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.IsReadOnly);
            DoesNotForceLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.IsLoading);
            DoesNotForceLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.IsSynchronized);
            DoesNotForceLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.IsFixedSize);
            DoesNotForceLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.SyncRoot);
            DoesNotForceLoad(Given.Collection(Tom, Sam, Sally), iterator => iterator.ToString());
        }

        [Test]
        public void IteratorUnsubscribesFromItemWhenRemoved()
        {
            BindableCollection<Contact> sourceCollection = Given.Collection(Mike, Tom, Jack);

            var contactIterator = new SimpleIterator<Contact>(sourceCollection);
            Assert.IsFalse(Mike.HasPropertyChangedSubscribers);
            Assert.IsFalse(Tom.HasPropertyChangedSubscribers);
            Assert.IsFalse(Jack.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasCollectionChangedSubscribers);

            contactIterator.AcceptDependency(new ItemDependencyDefinition("Name"));
            foreach (Contact c in contactIterator) {}

            Assert.IsTrue(Mike.HasPropertyChangedSubscribers);
            Assert.IsTrue(Tom.HasPropertyChangedSubscribers);
            Assert.IsTrue(Jack.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasCollectionChangedSubscribers);

            sourceCollection.Remove(Tom);

            Assert.IsTrue(Mike.HasPropertyChangedSubscribers);
            Assert.IsFalse(Tom.HasPropertyChangedSubscribers);
            Assert.IsTrue(Jack.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasCollectionChangedSubscribers);

            contactIterator.Dispose();

            Assert.IsFalse(Mike.HasPropertyChangedSubscribers);
            Assert.IsFalse(Tom.HasPropertyChangedSubscribers);
            Assert.IsFalse(Jack.HasPropertyChangedSubscribers);
            Assert.IsFalse(sourceCollection.HasPropertyChangedSubscribers);
            Assert.IsFalse(sourceCollection.HasCollectionChangedSubscribers);
        }

        [Test]
        public void IteratorWithDependenciesUnsubscribesFromAllSourceEventsWhenDisposed()
        {
            BindableCollection<Contact> sourceCollection = Given.Collection(Mike, Tom, Jack);

            var contactIterator = new SimpleIterator<Contact>(sourceCollection);
            Assert.IsFalse(Mike.HasPropertyChangedSubscribers);
            Assert.IsFalse(Tom.HasPropertyChangedSubscribers);
            Assert.IsFalse(Jack.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasCollectionChangedSubscribers);

            contactIterator.AcceptDependency(new ItemDependencyDefinition("Name"));

            Assert.IsFalse(Mike.HasPropertyChangedSubscribers);
            Assert.IsFalse(Tom.HasPropertyChangedSubscribers);
            Assert.IsFalse(Jack.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasCollectionChangedSubscribers);

            foreach (Contact c in contactIterator) {}

            Assert.IsTrue(Mike.HasPropertyChangedSubscribers);
            Assert.IsTrue(Tom.HasPropertyChangedSubscribers);
            Assert.IsTrue(Jack.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasCollectionChangedSubscribers);

            contactIterator.Dispose();

            Assert.IsFalse(Mike.HasPropertyChangedSubscribers);
            Assert.IsFalse(Tom.HasPropertyChangedSubscribers);
            Assert.IsFalse(Jack.HasPropertyChangedSubscribers);
            Assert.IsFalse(sourceCollection.HasCollectionChangedSubscribers);
            Assert.IsFalse(sourceCollection.HasPropertyChangedSubscribers);
        }

        [Test]
        public void IteratorWithDepenedenciesDoesNotEnumerateSourceUntilResultIsEnumerated()
        {
            var sourceCollection = new SourceCollection(Given.Collection(Tom, Tim, Jack));
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            var contactIterator = new SimpleIterator<Contact>(sourceCollection.AsBindable());
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            contactIterator.AcceptDependency(new ItemDependencyDefinition("Name"));
            Assert.AreEqual(0, sourceCollection.GetEnumeratorCalls);

            foreach (Contact c in contactIterator) {}
            Assert.AreEqual(1, sourceCollection.GetEnumeratorCalls);

            contactIterator.Dispose();
            Assert.AreEqual(1, sourceCollection.GetEnumeratorCalls);
        }

        [Test]
        public void IteratorWithoutDependenciesUnsubscribesFromAllSourceEventsWhenDisposed()
        {
            BindableCollection<Contact> sourceCollection = Given.Collection(Mike, Tom, Jack);

            var contactIterator = new SimpleIterator<Contact>(sourceCollection);
            Assert.IsFalse(Mike.HasPropertyChangedSubscribers);
            Assert.IsFalse(Tom.HasPropertyChangedSubscribers);
            Assert.IsFalse(Jack.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasCollectionChangedSubscribers);

            contactIterator.GetEnumerator();

            Assert.IsFalse(Mike.HasPropertyChangedSubscribers);
            Assert.IsFalse(Tom.HasPropertyChangedSubscribers);
            Assert.IsFalse(Jack.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasPropertyChangedSubscribers);
            Assert.IsTrue(sourceCollection.HasCollectionChangedSubscribers);

            contactIterator.Dispose();

            Assert.IsFalse(Mike.HasPropertyChangedSubscribers);
            Assert.IsFalse(Tom.HasPropertyChangedSubscribers);
            Assert.IsFalse(Jack.HasPropertyChangedSubscribers);
            Assert.IsFalse(sourceCollection.HasPropertyChangedSubscribers);
            Assert.IsFalse(sourceCollection.HasCollectionChangedSubscribers);
        }

        [Test]
        public void IteratorWriteMethodsThrowExceptions()
        {
            BindableCollection<Contact> sourceCollection = Given.Collection(Mike, Tom, Jack);
            var contactIterator = new SimpleIterator<Contact>(sourceCollection);

            IsNotSupported(() => contactIterator.Add(new object()));
            IsNotSupported(() => contactIterator.Clear());
            IsNotSupported(() => contactIterator.Insert(3, new object()));
            IsNotSupported(() => contactIterator.Remove(new object()));
            IsNotSupported(() => contactIterator.RemoveAt(2));
            IsNotSupported(() => contactIterator[0] = new Contact());
            IsNotSupported(() => ((IList) contactIterator)[0] = new Contact());
            Assert.IsTrue(contactIterator.IsReadOnly);
            Assert.IsTrue(contactIterator.IsSynchronized);
            Assert.IsFalse(contactIterator.IsFixedSize);
            Assert.AreNotEqual(contactIterator.SyncRoot, contactIterator.SyncRoot);
        }
    }
}