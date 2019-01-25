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
    public enum BindingMode
    {
        OneWay,
        OneTime,
        TwoWay
    }
}