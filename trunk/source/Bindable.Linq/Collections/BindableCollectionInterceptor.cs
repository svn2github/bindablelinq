﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;
using Bindable.Linq.Dependencies;
using Bindable.Linq.Helpers;
using Bindable.Linq.Configuration;

namespace Bindable.Linq.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    internal sealed class BindableCollectionInterceptor<TElement> :
        IBindableCollectionInterceptor<TElement>,
        IBindableQuery<TElement>
    {
        private IBindableCollection<TElement> _inner;
        private List<Action<TElement>> _preYieldSteps;
        private List<Action<TElement>> _postYieldSteps;
        private readonly EventHandler<NotifyCollectionChangedEventArgs> _inner_CollectionChanged;
        private readonly EventHandler<PropertyChangedEventArgs> _inner_PropertyChanged;
        private readonly WeakEventReference<NotifyCollectionChangedEventArgs> _inner_CollectionChangedWeak;
        private readonly WeakEventReference<PropertyChangedEventArgs> _inner_PropertyChangedWeak;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindableCollectionInterceptor&lt;TElement&gt;"/> class.
        /// </summary>
        /// <param name="inner">The inner.</param>
        public BindableCollectionInterceptor(IBindableCollection<TElement> inner)
        {
            _inner = inner;
            _preYieldSteps = new List<Action<TElement>>();
            _postYieldSteps = new List<Action<TElement>>();
            _inner_CollectionChanged = new EventHandler<NotifyCollectionChangedEventArgs>(Inner_CollectionChanged);
            _inner_PropertyChanged = new EventHandler<PropertyChangedEventArgs>(Inner_PropertyChanged);
            _inner_CollectionChangedWeak = new WeakEventReference<NotifyCollectionChangedEventArgs>(_inner_CollectionChanged);
            _inner_PropertyChangedWeak = new WeakEventReference<PropertyChangedEventArgs>(_inner_PropertyChanged);
            _inner.CollectionChanged += _inner_CollectionChangedWeak.WeakEventHandler;
            _inner.PropertyChanged += _inner_PropertyChangedWeak.WeakEventHandler;
        }

        /// <summary>
        /// Gets the count of items in the collection.
        /// </summary>
        public int Count
        {
            get { return _inner.Count; }
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        public TElement this[int index]
        {
            get
            {
                TElement result = default(TElement);
                IBindableQuery<TElement> bindable = _inner as IBindableQuery<TElement>;
                if (bindable != null)
                {
                    result = bindable[index];
                }
                return result;
            }
        }

        public int CurrentCount
        {
            get
            {
                int result = 0;
                IBindableQuery<TElement> bindable = _inner as IBindableQuery<TElement>;
                if (bindable != null)
                {
                    result = bindable.CurrentCount;
                }
                else
                {
                    result = this.Count;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IBindingConfiguration Configuration
        {
            get
            {
                IBindingConfiguration result = BindingConfigurations.Default;
                if (_inner is IConfigurable)
                {
                    result = ((IConfigurable) _inner).Configuration;
                }
                return result;
            }
        }

        public void AcceptDependency(IDependencyDefinition definition)
        {
        }

        public void Refresh()
        {
            IRefreshable refreshable = _inner as IRefreshable;
            if (refreshable != null)
            {
                refreshable.Refresh();
            }
            else
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public bool IsLoading
        {
            get
            {
                bool result = false;
                ILoadable loadable = _inner as ILoadable;
                if (loadable != null)
                {
                    result = loadable.IsLoading;
                }
                return result;
            }
        }

        /// <summary>
        /// Adds the pre yield step.
        /// </summary>
        /// <param name="step">The step.</param>
        public void AddPreYieldStep(Action<TElement> step)
        {
            _preYieldSteps.Add(step);
        }

        /// <summary>
        /// Adds the post yield step.
        /// </summary>
        /// <param name="step">The step.</param>
        public void AddPostYieldStep(Action<TElement> step)
        {
            _postYieldSteps.Add(step);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            foreach (TElement element in _inner)
            {
                foreach (Action<TElement> action in _preYieldSteps)
                {
                    action(element);
                }
                yield return element;
                foreach (Action<TElement> action in _postYieldSteps)
                {
                    action(element);
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void Inner_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        private void Inner_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = this.CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _inner.CollectionChanged -= _inner_CollectionChangedWeak.WeakEventHandler;
            _inner.PropertyChanged -= _inner_PropertyChangedWeak.WeakEventHandler;
        }
    }
}
