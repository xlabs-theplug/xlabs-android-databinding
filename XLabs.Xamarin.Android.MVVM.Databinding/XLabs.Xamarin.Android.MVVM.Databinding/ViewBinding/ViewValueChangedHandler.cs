using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace XLabs.Xamarin.Android.MVVM.Databinding.ViewBinding
{
    internal class ViewValueChangedHandler
    {
        internal static void HandleViewValueChanged(
            PropertyBinding propertyBinding,
            object dataContext)
        {
            try
            {
                propertyBinding.PreventUpdateForTargetProperty = true;

                var newValue = propertyBinding.TargetProperty.GetValue(propertyBinding.View);

                UpdateSourceProperty(propertyBinding.SourceProperty, dataContext, newValue,
                    propertyBinding.Converter, propertyBinding.ConverterParameter);
            }
            catch (Exception ex)
            {
                /* TODO: log exception */
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            finally
            {
                propertyBinding.PreventUpdateForTargetProperty = false;
            }
        }

        internal static void HandleViewValueChanged<TView, TArgs, TNewValue>(
            PropertyBinding propertyBinding,
            Func<TView, TArgs, TNewValue> newValueFunc,
            object dataContext,
            TArgs args)
            where TView : View
        {
            try
            {
                propertyBinding.PreventUpdateForTargetProperty = true;
                var rawValue = newValueFunc((TView)propertyBinding.View, args);

                UpdateSourceProperty(propertyBinding.SourceProperty,
                    dataContext,
                    rawValue,
                    propertyBinding.Converter,
                    propertyBinding.ConverterParameter);
            }
            catch (Exception ex)
            {
                /* TODO: log exception */
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            finally
            {
                propertyBinding.PreventUpdateForTargetProperty = false;
            }
        }

        internal static void UpdateSourceProperty<T>(
            PropertyInfo sourceProperty,
            object dataContext,
            T value,
            IValueConverter valueConverter,
            string converterParameter)
        {
            object newValue;

            if (valueConverter != null)
            {
                newValue = valueConverter.ConvertBack(value,
                    sourceProperty.PropertyType,
                    converterParameter,
                    CultureInfo.CurrentCulture);
            }
            else
            {
                newValue = value;
            }

            sourceProperty.SetValue(dataContext, newValue);
        }
    }
}