﻿using System.Collections;
using Bindable.Linq.Interfaces;
using Bindable.Linq.Threading;
using System.ComponentModel;
using Bindable.Linq.Helpers;
using System.Collections.Specialized;

namespace Bindable.Linq.Adapters.Incoming
{
#if !SILVERLIGHT
    /// <summary>
    /// An adapter to convert Windows Forms <see cref="IBindingList"/> collections into <see cref="IBindableCollection"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    internal sealed class BindingListToBindableCollectionAdapter<TElement> : BindableCollectionAdapterBase<TElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindingListToBindableCollectionAdapter&lt;TElement&gt;"/> class.
        /// </summary>
        /// <param name="sourceCollection">The source collection.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        public BindingListToBindableCollectionAdapter(IEnumerable sourceCollection, IDispatcher dispatcher) 
            : base(sourceCollection, dispatcher)
        {
            var observable = (IBindingList)sourceCollection;
            observable.ListChanged += Weak.Event<ListChangedEventArgs>((sender, e) => Dispatcher.Dispatch(() => SourceCollection_ListChanged(sender, e))).KeepAlive(InstanceLifetime).HandlerProxy.Handler;
        }

        private void SourceCollection_ListChanged(object sender, ListChangedEventArgs e)
        {
            var list = sender as IBindingList;
            if (list == null) return;
            
            NotifyCollectionChangedEventArgs argumentToRaise = null;
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    {
                        var array = new object[list.Count];
                        list.CopyTo(array, 0);
                        if (e.NewIndex >= 0 && e.NewIndex < array.Length)
                        {
                            var itemAtIndex = list[e.NewIndex];
                            argumentToRaise = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemAtIndex, e.NewIndex);
                        }
                    }
                    break;
                case ListChangedType.ItemDeleted:
                    {
                        var array = new object[list.Count];
                        list.CopyTo(array, 0);
                        if (e.OldIndex >= 0 && e.OldIndex < array.Length)
                        {
                            var itemAtIndex = list[e.OldIndex];
                            argumentToRaise = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemAtIndex, e.NewIndex);
                        }
                    }
                    break;
                case ListChangedType.ItemMoved:
                    {
                        var array = new object[list.Count];
                        list.CopyTo(array, 0);
                        if (e.OldIndex >= 0 && e.OldIndex < array.Length)
                        {
                            var itemAtIndex = list[e.NewIndex];
                            argumentToRaise = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, itemAtIndex, e.NewIndex, e.OldIndex);
                        }
                    }
                    break;
                case ListChangedType.ItemChanged:
                case ListChangedType.Reset:
                    argumentToRaise = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    break;
            }

            OnCollectionChanged(argumentToRaise);
        }
    }
#endif
}
