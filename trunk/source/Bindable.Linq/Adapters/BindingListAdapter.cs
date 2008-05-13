﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Bindable.Linq.Helpers;
using System.Collections;
using Bindable.Linq.Dependencies;
using Bindable.Linq.Collections;

namespace Bindable.Linq.Adapters
{
#if !SILVERLIGHT
    // Silverlight does not provide an IBindingList interface. This class is unnecessary.
    /// <summary>
    /// Converts Bindable LINQ bindable collection result sets into IBindingList implementations compatible with 
    /// Windows Forms.
    /// </summary>
    internal sealed class BindingListAdapter<TElement> : 
        IBindingList,
        IDisposable
    {
        private readonly IBindableCollectionInterceptor<TElement> _source;
        private readonly EventHandler<NotifyCollectionChangedEventArgs> _eventHandler;
        private readonly WeakEventReference<NotifyCollectionChangedEventArgs> _weakHandler;
        private readonly PropertyChangeObserver _propertyChangeObserver;
        private readonly ElementActioner<TElement> _addActioner;
        private readonly Dictionary<string, PropertyDescriptor> _propertyDescriptors;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingListAdapter&lt;TElement&gt;"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public BindingListAdapter(IBindableCollection<TElement> source)
        {
            source.ShouldNotBeNull("source");
            
            _source = new BindableCollectionInterceptor<TElement>(source);
            _eventHandler = Source_CollectionChanged;
            _weakHandler = new WeakEventReference<NotifyCollectionChangedEventArgs>(_eventHandler);
            _source.CollectionChanged += _weakHandler.WeakEventHandler;

            _propertyChangeObserver = new PropertyChangeObserver(Element_PropertyChanged);
            _addActioner = new ElementActioner<TElement>(_source,
                element => _propertyChangeObserver.Attach(element),
                element => _propertyChangeObserver.Detach(element));

            _propertyDescriptors = new Dictionary<string, PropertyDescriptor>();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof (TElement));
            foreach (PropertyDescriptor descriptor in properties)
            {
                if (descriptor != null && descriptor.Name != null)
                {
                    _propertyDescriptors[descriptor.Name] = descriptor;
                }
            }
        }

        #region IBindingList Members

        /// <summary>
        /// Occurs when the list changes or an item in the list changes.
        /// </summary>
        public event ListChangedEventHandler ListChanged;

        /// <summary>
        /// Adds the <see cref="T:System.ComponentModel.PropertyDescriptor"/> to the indexes used for searching.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to add to the indexes used for searching.</param>
        public void AddIndex(PropertyDescriptor property)
        {
        }

        /// <summary>
        /// Adds a new item to the list.
        /// </summary>
        /// <returns>The item added to the list.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// 	<see cref="P:System.ComponentModel.IBindingList.AllowNew"/> is false. </exception>
        public object AddNew()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets whether you can update items in the list.
        /// </summary>
        /// <value></value>
        /// <returns>true if you can update the items in the list; otherwise, false.</returns>
        public bool AllowEdit
        {
            get { return false; }
        }

        /// <summary>
        /// Gets whether you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew"/>.
        /// </summary>
        /// <value></value>
        /// <returns>true if you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew"/>; otherwise, false.</returns>
        public bool AllowNew
        {
            get { return false; }
        }

        /// <summary>
        /// Gets whether you can remove items from the list, using <see cref="M:System.Collections.IList.Remove(System.Object)"/> or <see cref="M:System.Collections.IList.RemoveAt(System.Int32)"/>.
        /// </summary>
        /// <value></value>
        /// <returns>true if you can remove items from the list; otherwise, false.</returns>
        public bool AllowRemove
        {
            get { return false; }
        }

        /// <summary>
        /// Sorts the list based on a <see cref="T:System.ComponentModel.PropertyDescriptor"/> and a <see cref="T:System.ComponentModel.ListSortDirection"/>.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to sort by.</param>
        /// <param name="direction">One of the <see cref="T:System.ComponentModel.ListSortDirection"/> values.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {

        }

        /// <summary>
        /// Returns the index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor"/>.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to search on.</param>
        /// <param name="key">The value of the <paramref name="property"/> parameter to search for.</param>
        /// <returns>
        /// The index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSearching"/> is false. </exception>
        public int Find(PropertyDescriptor property, object key)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets whether the items in the list are sorted.
        /// </summary>
        /// <value></value>
        /// <returns>true if <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)"/> has been called and <see cref="M:System.ComponentModel.IBindingList.RemoveSort"/> has not been called; otherwise, false.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
        public bool IsSorted
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the <see cref="T:System.ComponentModel.PropertyDescriptor"/> from the indexes used for searching.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to remove from the indexes used for searching.</param>
        public void RemoveIndex(PropertyDescriptor property)
        {
        }

        /// <summary>
        /// Removes any sort applied using <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
        public void RemoveSort()
        {
        }

        /// <summary>
        /// Gets the direction of the sort.
        /// </summary>
        /// <value></value>
        /// <returns>One of the <see cref="T:System.ComponentModel.ListSortDirection"/> values.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
        public ListSortDirection SortDirection
        {
            get { return ListSortDirection.Ascending; }
        }

        /// <summary>
        /// Gets the <see cref="T:System.ComponentModel.PropertyDescriptor"/> that is being used for sorting.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor"/> that is being used for sorting.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// 	<see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false. </exception>
        public PropertyDescriptor SortProperty
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets whether a <see cref="E:System.ComponentModel.IBindingList.ListChanged"/> event is raised when the list changes or an item in the list changes.
        /// </summary>
        /// <value></value>
        /// <returns>true if a <see cref="E:System.ComponentModel.IBindingList.ListChanged"/> event is raised when the list changes or when an item changes; otherwise, false.</returns>
        public bool SupportsChangeNotification
        {
            get { return true; }
        }

        /// <summary>
        /// Gets whether the list supports searching using the <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)"/> method.
        /// </summary>
        /// <value></value>
        /// <returns>true if the list supports searching using the <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)"/> method; otherwise, false.</returns>
        public bool SupportsSearching
        {
            get { return false; }
        }

        /// <summary>
        /// Gets whether the list supports sorting.
        /// </summary>
        /// <value></value>
        /// <returns>true if the list supports sorting; otherwise, false.</returns>
        public bool SupportsSorting
        {
            get { return false; }
        }

        #endregion

        #region IList Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"/> to add to the <see cref="T:System.Collections.IList"/>.</param>
        /// <returns>
        /// The position into which the new element was inserted.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        public int Add(object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only. </exception>
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IList"/> contains a specific value.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"/> to locate in the <see cref="T:System.Collections.IList"/>.</param>
        /// <returns>
        /// true if the <see cref="T:System.Object"/> is found in the <see cref="T:System.Collections.IList"/>; otherwise, false.
        /// </returns>
        public bool Contains(object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"/> to locate in the <see cref="T:System.Collections.IList"/>.</param>
        /// <returns>
        /// The index of <paramref name="value"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(object value)
        {
            int result = -1;
            if (_source is IList)
            {
                result = ((IList) _source).IndexOf(value);
            }
            return result;
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.IList"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value"/> should be inserted.</param>
        /// <param name="value">The <see cref="T:System.Object"/> to insert into the <see cref="T:System.Collections.IList"/>.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        /// <exception cref="T:System.NullReferenceException">
        /// 	<paramref name="value"/> is null reference in the <see cref="T:System.Collections.IList"/>.</exception>
        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IList"/> has a fixed size; otherwise, false.</returns>
        public bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IList"/> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"/> to remove from the <see cref="T:System.Collections.IList"/>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        public void Remove(object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.IList"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> at the specified index.
        /// </summary>
        /// <value></value>
        public object this[int index]
        {
            get
            {
                if (_source is IList)
                {
                    return ((IList)_source)[index];
                }
                else if (_source is IBindableQuery<TElement>)
                {
                    return ((IBindableQuery<TElement>)_source)[index];
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="array"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="array"/> is multidimensional.-or- <paramref name="index"/> is equal to or greater than the length of <paramref name="array"/>.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>. </exception>
        /// <exception cref="T:System.ArgumentException">The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>. </exception>
        public void CopyTo(Array array, int index)
        {
            foreach (TElement element in _source)
            {
                if (index < array.Length)
                {
                    array.SetValue(element, index);
                    index++;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.ICollection"/>.</returns>
        public int Count
        {
            get { return _source.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <value></value>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.</returns>
        public bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.</returns>
        public object SyncRoot
        {
            get { return new object(); }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        #endregion

        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_propertyDescriptors.ContainsKey(e.PropertyName))
            {
                PropertyDescriptor descriptor = _propertyDescriptors[e.PropertyName];
                int index = this.IndexOf(sender);
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index, descriptor));
            }
        }

        private void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int index = e.NewStartingIndex;
                        foreach (object item in e.NewItems)
                        {
                            ListChangedEventArgs listEvent = new ListChangedEventArgs(ListChangedType.ItemAdded, index);
                            this.OnListChanged(listEvent);
                            index++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        int newIndex = e.NewStartingIndex;
                        int oldIndex = e.OldStartingIndex;
                        foreach (object item in e.NewItems)
                        {
                            ListChangedEventArgs listEvent = new ListChangedEventArgs(ListChangedType.ItemMoved, newIndex, oldIndex);
                            this.OnListChanged(listEvent);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        int index = e.OldStartingIndex;
                        foreach (object item in e.OldItems)
                        {
                            ListChangedEventArgs listEvent = new ListChangedEventArgs(ListChangedType.ItemDeleted, index);
                            this.OnListChanged(listEvent);
                            index++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        int index = e.NewStartingIndex;
                        foreach (object item in e.NewItems)
                        {
                            ListChangedEventArgs listEvent = new ListChangedEventArgs(ListChangedType.ItemChanged, index);
                            this.OnListChanged(listEvent);
                            index++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        ListChangedEventArgs listEvent = new ListChangedEventArgs(ListChangedType.Reset, -1);
                        this.OnListChanged(listEvent);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:ListChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.ListChangedEventArgs"/> instance containing the event data.</param>
        private void OnListChanged(ListChangedEventArgs e)
        {
            ListChangedEventHandler handler = this.ListChanged;
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
            _addActioner.Dispose();
            _propertyChangeObserver.Dispose();
            _weakHandler.Dispose();
        }
    }
#endif
}
