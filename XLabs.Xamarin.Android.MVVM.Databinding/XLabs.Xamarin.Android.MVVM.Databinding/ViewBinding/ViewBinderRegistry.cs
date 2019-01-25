using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;

namespace XLabs.Xamarin.Android.MVVM.Databinding.ViewBinding
{
    public class ViewBinderRegistry
    {
        public bool RemoveViewBinder(Type viewType, string propertyName)
        {
            string key = MakeDictionaryKey(viewType, propertyName);
            return binderDictionary.Remove(key);
        }

        public bool TryGetViewBinder(Type viewType, string propertyName, out IViewBinder viewBinder)
        {
            string key = MakeDictionaryKey(viewType, propertyName);

            if (binderDictionary.TryGetValue(key, out viewBinder))
            {
                return true;
            }

            return false;
        }

        static string MakeDictionaryKey(Type viewType, string propertyName)
        {
            return viewType.AssemblyQualifiedName + "." + propertyName;
        }

        public void SetViewBinder<TView>(string propertyName, IViewBinder viewBinder)
        {
            string key = typeof(TView).AssemblyQualifiedName + "." + propertyName;
            binderDictionary[key] = viewBinder;
        }

        public void SetViewBinder(Type viewType, string propertyName, IViewBinder viewBinder)
        {
            string key = MakeDictionaryKey(viewType, propertyName);
            binderDictionary[key] = viewBinder;
        }

        readonly Dictionary<string, IViewBinder> binderDictionary
            = new Dictionary<string, IViewBinder>
            {
				{MakeDictionaryKey(typeof(CalendarView), nameof(CalendarView.Date)), new ViewEventBinder<CalendarView, CalendarView.DateChangeEventArgs, DateTime>(
                    (view, h) => view.DateChange += h, (view, h) => view.DateChange -= h, (view, args) => new DateTime(args.Year, args.Month, args.DayOfMonth))},
                {MakeDictionaryKey(typeof(CheckBox), nameof(CheckBox.Checked)), new ViewEventBinder<CheckBox, CheckBox.CheckedChangeEventArgs, bool>(
                    (view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
                {MakeDictionaryKey(typeof(RadioButton), nameof(RadioButton.Checked)), new ViewEventBinder<RadioButton, RadioButton.CheckedChangeEventArgs, bool>(
                    (view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
                {MakeDictionaryKey(typeof(RatingBar), nameof(RatingBar.Rating)), new ViewEventBinder<RatingBar, RatingBar.RatingBarChangeEventArgs, float>(
                    (view, h) => view.RatingBarChange += h, (view, h) => view.RatingBarChange -= h, (view, args) => args.Rating)},
                {MakeDictionaryKey(typeof(SearchView), nameof(SearchView.Query)), new ViewEventBinder<SearchView, SearchView.QueryTextChangeEventArgs, string>(
                    (view, h) => view.QueryTextChange += h, (view, h) => view.QueryTextChange -= h, (view, args) => args.NewText)},
                {MakeDictionaryKey(typeof(Switch), nameof(Switch.Checked)), new ViewEventBinder<Switch, Switch.CheckedChangeEventArgs, bool>(
                    (view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
                {MakeDictionaryKey(typeof(TimePicker), nameof(TimePicker.Minute)), new ViewEventBinder<TimePicker, TimePicker.TimeChangedEventArgs, int>(
                    (view, h) => view.TimeChanged += h, (view, h) => view.TimeChanged -= h, (view, args) => args.Minute)},
                {MakeDictionaryKey(typeof(TimePicker), nameof(TimePicker.Hour)), new ViewEventBinder<TimePicker, TimePicker.TimeChangedEventArgs, int>(
                    (view, h) => view.TimeChanged += h, (view, h) => view.TimeChanged -= h, (view, args) => args.HourOfDay)},
                {MakeDictionaryKey(typeof(EditText), nameof(EditText.Text)), new ViewEventBinder<EditText, TextChangedEventArgs, string>(
                    (view, h) => view.TextChanged += h, (view, h) => view.TextChanged -= h, (view, args) => args.Text.ToString())},
                {MakeDictionaryKey(typeof(ToggleButton), nameof(ToggleButton.Text)), new ViewEventBinder<ToggleButton, CompoundButton.CheckedChangeEventArgs, bool>(
                    (view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
                {MakeDictionaryKey(typeof(SeekBar), nameof(SeekBar.Progress)), new ViewEventBinder<SeekBar, SeekBar.ProgressChangedEventArgs, int>(
                    (view, h) => view.ProgressChanged += h, (view, h) => view.ProgressChanged -= h, (view, args) => args.Progress)},
			};
    }
}