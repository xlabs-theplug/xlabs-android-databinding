using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace XLabs.Xamarin.Android.MVVM.Databinding.ViewBinding
{
    public class ViewEventBinder<TView, TArgs, TNewValue> : IViewBinder where TView : View where TArgs : EventArgs
    {
        readonly Action<TView, EventHandler<TArgs>> addHandler;
        readonly Action<TView, EventHandler<TArgs>> removeHandler;
        readonly Func<TView, TArgs, TNewValue> newValueFunc;

        public ViewEventBinder(
            Action<TView, EventHandler<TArgs>> addHandler,
            Action<TView, EventHandler<TArgs>> removeHandler,
            Func<TView, TArgs, TNewValue> newValueFunc)
        {
            this.addHandler = AssertNotNull(addHandler, nameof(addHandler));
            this.removeHandler = AssertNotNull(removeHandler, nameof(removeHandler));
            this.newValueFunc = AssertNotNull(newValueFunc, nameof(newValueFunc));
        }

        T AssertNotNull<T>(T value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            return value;
        }

        public Action BindView(PropertyBinding propertyBinding, object dataContext)
        {
            EventHandler<TArgs> handler =
                (sender, args) =>
                {
                    ViewValueChangedHandler.HandleViewValueChanged(propertyBinding, newValueFunc, dataContext, args);
                };

            addHandler((TView)propertyBinding.View, handler);

            Action removeAction = () => { removeHandler((TView)propertyBinding.View, handler); };
            return removeAction;
        }
    }
}