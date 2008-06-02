using System;
using System.Windows.Threading;

namespace Bindable.Linq.Threading
{
#if SILVERLIGHT
    /// <summary>
    /// This dispatcher is used at runtime by both Windows Forms and WPF. The WPF Dispatcher 
    /// class works within Windows Forms, so this appears to be safe.
    /// </summary>
    public class SilverlightDispatcher : IDispatcher
    {
        private Dispatcher _dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="SilverlightDispatcher"/> class.
        /// </summary>
        public SilverlightDispatcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Invoke(Action actionToInvoke)
        {
            _dispatcher.BeginInvoke(actionToInvoke);
        }
    }
#endif
}