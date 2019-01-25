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
    /// <summary>
	/// This interface represents an extensibility point for adding support 
	/// for different types of views that may, or may not, already be included 
	/// in the <seealso cref="ViewBinderRegistry"/>.
	/// </summary>
	public interface IViewBinder
    {
        Action BindView(PropertyBinding binding, object viewModel);
    }
}