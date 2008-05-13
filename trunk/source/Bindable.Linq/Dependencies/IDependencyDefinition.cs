using System;
using System.Collections.Generic;
using Bindable.Linq;
using Bindable.Linq.Collections;
using Bindable.Linq.Dependencies.PathNavigation;

namespace Bindable.Linq.Dependencies
{
    /// <summary>
    /// This interface is implemented by items that represent a dependency.
    /// </summary>
    /// <remarks>
    /// Dependency definitions follow the visitor pattern. All SyncLINQ operations implement the 
    /// <see cref="IDependencyDefinition"/> interface, which allows you to pass dependency definitions to them. 
    /// Depending on the type of the operation, the dependency will ask you to either construct the dependency for 
    /// a collection of items (in the case of Aggregators or Iterators), or for a single object (in the case of 
    /// Operators).
    /// </remarks>
    public interface IDependencyDefinition
    {
        /// <summary>
        /// Determines whether this instance can construct dependencies for a collection.
        /// </summary>
        bool AppliesToCollections();

        /// <summary>
        /// Determines whether this instance can construct dependencies for a single element.
        /// </summary>
        bool AppliesToSingleElement();

        /// <summary>
        /// Constructs the dependency for a collection of elements.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="sourceElements">The source elements.</param>
        /// <returns></returns>
        IDependency ConstructForCollection<TElement>(IBindableCollectionInterceptor<TElement> sourceElements, IPathNavigator pathNavigator);

        /// <summary>
        /// Constructs a dependency for a single element.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="sourceElement">The source element.</param>
        /// <returns></returns>
        IDependency ConstructForElement<TElement>(TElement sourceElement, IPathNavigator pathNavigator);
    }
}