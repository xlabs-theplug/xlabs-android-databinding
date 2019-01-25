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

namespace XLabs.Xamarin.Android.MVVM.Databinding
{
    public class BindingExpression
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public string Converter { get; set; }
        public string ConverterParameter { get; set; }
        public BindingMode Mode { get; set; }
        public string ViewValueChangedEvent { get; set; }
        
        public View View { get; set; }
    }
}