using System;
using System.Collections.Generic;
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
using XLabs.Xamarin.Android.MVVM.Databinding.Adapters;

namespace XLabs.Xamarin.Android.MVVM.Databinding.ValueConverters
{
    public class ListAdapterConverter : IValueConverter
    {
        static readonly Dictionary<string, FieldInfo> adapterDictionary
            = new Dictionary<string, FieldInfo>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            Type valueType = value.GetType();
            if (valueType.IsGenericType)
            {
                Type[] typeArguments = valueType.GetGenericArguments();
                if (typeArguments.Any())
                {
                    if (typeArguments.Count() > 1)
                    {
                        throw new Exception("List contains to many type arguments. Unable to create "
                            + nameof(BindableListAdapter<object>) + " in ListAdapterConverter.");
                    }

                    Type itemType = typeArguments[0];
                    Type listType = typeof(BindableListAdapter<>);
                    Type[] typeArgs = { itemType };
                    Type constructed = listType.MakeGenericType(typeArgs);

                    string layoutName = parameter?.ToString();

                    if (layoutName != null)
                    {
                        var dotIndex = layoutName.LastIndexOf(".", StringComparison.Ordinal);
                        string propertyName = layoutName.Substring(dotIndex + 1, layoutName.Length - (dotIndex + 1));
                        string typeName = layoutName.Substring(0, dotIndex);

                        FieldInfo fieldInfo;
                        if (!adapterDictionary.TryGetValue(layoutName, out fieldInfo))
                        {
                            Type type = Type.GetType(typeName, false, true);
                            if (type == null)
                            {
                                throw new Exception("Unable to locate layout type code for layout " + layoutName
                                                    + " Type could not be resolved.");
                            }

                            fieldInfo = type.GetField(propertyName, BindingFlags.Public | BindingFlags.Static);

                            if (fieldInfo != null)
                            {
                                adapterDictionary[layoutName] = fieldInfo;
                            }
                        }

                        if (fieldInfo == null)
                        {
                            throw new Exception("Unable to locate layout type code for layout "
                                + layoutName + " FieldInfo is null.");
                        }

                        int resourceId = (int)fieldInfo.GetValue(null);

                        var result = Activator.CreateInstance(constructed, value, resourceId);
                        return result;
                    }
                }
            }
            else
            {
                throw new Exception("Value is not a generic collection." + parameter);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}