﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bindable.Linq.Dependencies.PathNavigation.Tokens;

namespace Bindable.Linq.Dependencies.PathNavigation.TokenFactories
{
    /// <summary>
    /// A property parser for properties on a Windows Forms control.
    /// </summary>
    public sealed class WindowsFormsMemberTokenFactory : ITokenFactory
    {
        /// <summary>
        /// Creates an appropriate property monitor for the remaining property path string on the target object.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>
        /// An appropriate <see cref="IPropertyMonitor"/> for the property.
        /// </returns>
        public IToken ParseNext(object target, string propertyPath, Action<object, string> callback, IPathNavigator pathNavigator)
        {
            IToken result = null;
#if !SILVERLIGHT
            if (target is System.Windows.Forms.Control)
            {
                string propertyName = propertyPath;
                string remainingPath = null;
                int dotIndex = propertyPath.IndexOf('.');
                if (dotIndex >= 0)
                {
                    propertyName = propertyPath.Substring(0, dotIndex);
                    remainingPath = propertyPath.Substring(dotIndex + 1);
                }
                result = new WindowsFormsMemberToken(target, propertyName, remainingPath, callback, pathNavigator);
            }
#endif
            return result;
        }
    }
}
