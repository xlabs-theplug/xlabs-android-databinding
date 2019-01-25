using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace XLabs.Xamarin.Android.MVVM.Databinding
{
    public class PropertyBinding
    {
        public View View { get; set; }
        public PropertyInfo SourceProperty { get; set; }
        public PropertyInfo TargetProperty { get; set; }
        public IValueConverter Converter { get; set; }
        public string ConverterParameter { get; set; }

        internal bool PreventUpdateForSourceProperty;

        internal bool PreventUpdateForTargetProperty;
    }
}