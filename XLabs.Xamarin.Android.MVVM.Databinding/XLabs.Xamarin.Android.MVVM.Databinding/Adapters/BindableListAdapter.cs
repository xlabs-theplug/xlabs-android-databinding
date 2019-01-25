using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace XLabs.Xamarin.Android.MVVM.Databinding.Adapters
{
    public class BindableListAdapter<TItem> : BaseAdapter<TItem>
    {
        readonly IList<TItem> list;
        readonly int layoutId;
        readonly ObservableCollection<TItem> observableCollection;
        readonly LayoutInflater inflater;

        public BindableListAdapter(IList<TItem> list, int layoutId)
        {
            this.list = list;
            this.layoutId = layoutId;

            observableCollection = list as ObservableCollection<TItem>;
            if (observableCollection != null)
            {
                observableCollection.CollectionChanged += HandleCollectionChanged;
            }

            Context context = ApplicationContextHolder.Context;
            inflater = LayoutInflater.From(context);
        }

        void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyDataSetChanged();
        }

        public override int Count => list.Count;

        public override long GetItemId(int position)
        {
            return position;
        }

        public override TItem this[int index] => list[index];

        readonly Dictionary<View, XmlBindingApplicator> bindingsDictionary
                    = new Dictionary<View, XmlBindingApplicator>();

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView ?? inflater.Inflate(layoutId, parent, false);

            TItem item = this[position];

            XmlBindingApplicator applicator;
            if (!bindingsDictionary.TryGetValue(view, out applicator))
            {
                applicator = new XmlBindingApplicator();
            }
            else
            {
                applicator.RemoveBindings();
            }

            applicator.ApplyBindings(view, item, layoutId);

            return view;
        }

        protected override void Dispose(bool disposing)
        {
            if (observableCollection != null)
            {
                observableCollection.CollectionChanged -= HandleCollectionChanged;
            }

            foreach (var operation in bindingsDictionary.Values)
            {
                operation.RemoveBindings();
            }

            bindingsDictionary.Clear();

            base.Dispose(disposing);
        }

    }
}