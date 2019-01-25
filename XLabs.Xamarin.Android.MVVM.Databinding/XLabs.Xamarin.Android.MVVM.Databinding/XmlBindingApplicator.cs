using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using XLabs.Xamarin.Android.MVVM.Databinding.Utilities;
using XLabs.Xamarin.Android.MVVM.Databinding.ViewBinding;
using Id = Android.Resource.Id;

namespace XLabs.Xamarin.Android.MVVM.Databinding
{
    internal class ApplicationContextHolder
    {
        internal static Context Context { get; set; }
    }

    public class XmlBindingApplicator
    {
        static readonly XName bindingXmlNamespace
            = XNamespace.Get("http://schemas.android.com/apk/res-auto") + "Binding";

        static readonly ViewBinderRegistry registry = new ViewBinderRegistry();

        static List<Type> valueConverterTypes;

        readonly List<Action> unbindActions = new List<Action>();

        static readonly Dictionary<int, List<XElement>> layoutCache = new Dictionary<int, List<XElement>>();

        Context applicationContext;

        public void RemoveBindings()
        {
            foreach (var unbindAction in unbindActions)
            {
                try
                {
                    unbindAction();
                }
                catch (Exception ex)
                {
                    // Log this.
                    if (Debugger.IsAttached)
                    {
                        throw ex;
                    }
                }
            }

            unbindActions.Clear();
        }

        public void ApplyBindings(Activity activity, string viewModelPropertyName, int layoutResourceId)
        {
            if (ApplicationContextHolder.Context == null)
            {
                ApplicationContextHolder.Context = activity.ApplicationContext;
            }

            List<View> views = null;
            List<XElement> elements = null;

            if (layoutResourceId > -1)
            {
                /* Load the XML elements of the view. */
                elements = GetLayoutAsXmlElements(activity, layoutResourceId);
            }

            /* If there is at least one 'Binding' attribute set in the XML file, 
			 * get the view as objects. */
            if (elements != null && elements.Any(xe => xe.Attribute(bindingXmlNamespace) != null))
            {
                var rootView = activity.Window.DecorView.FindViewById(Id.Content);
                views = GetAllChildrenInView(rootView);

                /* Remove the first views that are not part of the layout file. 
				 * This includes the first FrameView. */
                int elementsCount = elements.Count;
                int difference = views.Count - elementsCount;
                if (difference > 0)
                {
                    views = views.GetRange(difference, views.Count - difference);
                }
            }

            if (elements != null && elements.Any() && views != null && views.Any())
            {
                /* Get all the binding inside the XML file. */
                var bindingInfos = ExtractBindingsFromLayoutFile(elements, views);
                if (bindingInfos == null || !bindingInfos.Any())
                {
                    return;
                }

                /* Load all the converters if there is a binding using a converter. */
                if (bindingInfos.Any(info => !string.IsNullOrWhiteSpace(info.Converter)))
                {
                    if (valueConverterTypes == null)
                    {
                        valueConverterTypes = new List<Type>();
                        var converters = TypeUtility.GetTypes<IValueConverter>().ToList();
                        if (converters.Any())
                        {
                            valueConverterTypes.AddRange(converters);
                        }
                        else if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
                    }
                }

                var binder = new BindingApplicator();

                foreach (var bindingInfo in bindingInfos)
                {
                    IValueConverter valueConverter = null;
                    string valueConverterName = bindingInfo.Converter;

                    if (!string.IsNullOrWhiteSpace(valueConverterName))
                    {
                        var valueConverterType = valueConverterTypes.FirstOrDefault(t => t.Name == valueConverterName);
                        if (valueConverterType != null)
                        {
                            var valueConverterConstructor = valueConverterType.GetConstructor(Type.EmptyTypes);
                            if (valueConverterConstructor != null)
                            {
                                valueConverter = valueConverterConstructor.Invoke(null) as IValueConverter;
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"Value converter {valueConverterName} needs "
                                    + "an empty constructor to be instanciated.");
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"There is no converter named {valueConverterName}.");
                        }
                    }

                    binder.ApplyBinding(bindingInfo, activity, viewModelPropertyName, valueConverter, unbindActions);
                }
            }
        }

        public void ApplyBindings(View view, object dataContext, int layoutResourceId)
        {
            Context context = view.Context;

            if (ApplicationContextHolder.Context == null)
            {
                ApplicationContextHolder.Context = context.ApplicationContext;
            }

            List<View> views = null;
            List<XElement> elements = null;

            if (layoutResourceId > -1)
            {
                /* Load the XML elements of the view. */
                elements = GetLayoutAsXmlElements(context, layoutResourceId);
            }

            /* If there is at least one 'Binding' attribute set in the XML file, 
			 * get the view as objects. */
            if (elements != null && elements.Any(xe => xe.Attribute(bindingXmlNamespace) != null))
            {
                views = GetAllChildrenInView(view);
            }

            if (elements != null && elements.Any() && views != null && views.Any())
            {
                /* Get all the binding inside the XML file. */
                var bindingInfos = ExtractBindingsFromLayoutFile(elements, views);
                if (bindingInfos == null || !bindingInfos.Any())
                {
                    return;
                }

                /* Load all the converters if there is a binding using a converter. */
                if (bindingInfos.Any(bo => !string.IsNullOrWhiteSpace(bo.Converter)))
                {
                    if (valueConverterTypes == null)
                    {
                        valueConverterTypes = new List<Type>();
                        var converters = TypeUtility.GetTypes<IValueConverter>();
                        valueConverterTypes.AddRange(converters);
                    }
                }

                var binder = new BindingApplicator();

                foreach (var bindingInfo in bindingInfos)
                {
                    IValueConverter valueConverter = null;
                    string valueConverterName = bindingInfo.Converter;

                    if (!string.IsNullOrWhiteSpace(valueConverterName))
                    {
                        var converterType = valueConverterTypes.FirstOrDefault(t => t.Name == valueConverterName);
                        if (converterType != null)
                        {
                            var constructor = converterType.GetConstructor(Type.EmptyTypes);
                            if (constructor != null)
                            {
                                valueConverter = constructor.Invoke(null) as IValueConverter;
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"Value converter {valueConverterName} needs "
                                    + "an empty constructor to be instanciated.");
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"There is no converter named {valueConverterName}.");
                        }
                    }

                    binder.ApplyBinding(bindingInfo, dataContext, valueConverter, unbindActions);
                }
            }
        }

        public static void SetViewBinder(Type viewType, string propertyName, IViewBinder binder)
        {
            registry.SetViewBinder(viewType, propertyName, binder);
        }

        /// <summary>
        /// Returns the current view (activity) as a list of XML element.
        /// </summary>
        /// <param name="activity">The current activity we want 
        /// to get as a list of XML elements.</param>
        /// <param name="layoutResourceId">The id corresponding to the layout.</param>
        /// <returns>A list of XML elements which represent the XML layout of the view.</returns>
        static List<XElement> GetLayoutAsXmlElements(Context activity, int layoutResourceId)
        {
            List<XElement> result;

            if (layoutCache.TryGetValue(layoutResourceId, out result))
            {
                return result;
            }

            using (var viewAsXmlReader = activity.Resources.GetLayout(layoutResourceId))
            {
                var sb = new StringBuilder();
                while (viewAsXmlReader.Read())
                {
                    sb.Append(viewAsXmlReader.ReadOuterXml());
                }
                var viewAsXDocument = XDocument.Parse(sb.ToString());
                result = viewAsXDocument.Descendants().ToList();
            }

            layoutCache[layoutResourceId] = result;

            return result;
        }

        static List<View> GetAllChildrenInView(View rootView)
        {
            List<View> result = new List<View>();
            GetAllChildrenInView(rootView, result);

            return result;
        }

        /// <summary>
        /// Recursive method which returns the list of children contains in a view.
        /// </summary>
        /// <param name="rootView">The start view from which the children will be retrieved.</param>
        /// <param name="children">A cumulative collection of child views.</param>
        static void GetAllChildrenInView(View rootView, List<View> children)
        {
            if (!(rootView is ViewGroup))
            {
                children.Add(rootView);
                return;
            }

            children.Add(rootView);

            var viewGroup = (ViewGroup)rootView;

            for (var i = 0; i < viewGroup.ChildCount; i++)
            {
                View child = viewGroup.GetChildAt(i);

                GetAllChildrenInView(child, children);
            }
        }

        readonly Regex sourceRegex = new Regex(@"Source=(\w+(.\w+)+)", RegexOptions.Compiled);
        readonly Regex targetRegex = new Regex(@"Target=(\w+)", RegexOptions.Compiled);
        readonly Regex converterRegex = new Regex(@"Converter=(\w+)", RegexOptions.Compiled);
        readonly Regex converterParameterRegex = new Regex(@"ConverterParameter=(\w+\s*(.\w+)+)", RegexOptions.Compiled);
        readonly Regex modeRegex = new Regex(@"Mode=(\w+)", RegexOptions.Compiled);
        readonly Regex changedEventRegex = new Regex(@"ChangedEvent=(\w+)", RegexOptions.Compiled);

        /// <summary>
        /// Extracts the binding information represented 
        /// as the Binding="" attribute in the XML file.
        /// Based on code by Thomas Lebrun http://bit.ly/1OQsD8L
        /// </summary>
        /// <param name="xmlElements">The list of XML elements from 
        /// which to extract the binding information.</param>
        /// <param name="viewElements">The elements of the view.</param>
        /// <returns>
        /// A list containing all the binding info objects.
        /// </returns>
        List<BindingExpression> ExtractBindingsFromLayoutFile(
            List<XElement> xmlElements, List<View> viewElements)
        {
            var result = new List<BindingExpression>();

            for (var i = 0; i < xmlElements.Count; i++)
            {
                var xmlElement = xmlElements.ElementAt(i);

                if (!xmlElement.Attributes(bindingXmlNamespace).Any())
                {
                    continue;
                }

                var bindingAttributes = xmlElement.Attributes(bindingXmlNamespace);

                foreach (var attribute in bindingAttributes)
                {
                    string bindingString = attribute.Value;

                    if (!bindingString.StartsWith("{") || !bindingString.EndsWith("}"))
                    {
                        throw new InvalidOperationException(
                            "The following XML binding operation is not well formatted, it should start "
                            + $"with '{{' and end with '}}:'{System.Environment.NewLine}{bindingString}");
                    }

                    string[] bindingStrings = bindingString.Split(';');

                    foreach (var bindingText in bindingStrings)
                    {
                        if (!bindingText.Contains(","))
                        {
                            throw new InvalidOperationException(
                                "The following XML binding operation is not well formatted, "
                                + "it should contains at least one \',\' "
                                + $"between Source and Target:{System.Environment.NewLine}{bindingString}");
                        }

                        var sourceValue = sourceRegex.Match(bindingText).Groups[1].Value;
                        var targetValue = targetRegex.Match(bindingText).Groups[1].Value;
                        var converterValue = converterRegex.Match(bindingText).Groups[1].Value;

                        string converterParameterValue = converterParameterRegex.Match(bindingText).Groups[1].Value;

                        var bindingMode = BindingMode.OneWay;

                        var modeRegexMatch = modeRegex.Match(bindingText);

                        if (modeRegexMatch.Success)
                        {
                            if (!Enum.TryParse(modeRegexMatch.Groups[1].Value, true, out bindingMode))
                            {
                                throw new InvalidOperationException(
                                    "The Mode property of the following XML binding operation "
                                    + "is not well formatted, it should be \'OneWay\' "
                                    + $"or 'TwoWay':{System.Environment.NewLine}{bindingString}");
                            }
                        }

                        var viewValueChangedEvent = changedEventRegex.Match(bindingText).Groups[1].Value;

                        result.Add(new BindingExpression
                        {
                            View = viewElements.ElementAt(i),
                            Source = sourceValue,
                            Target = targetValue,
                            Converter = converterValue,
                            ConverterParameter = converterParameterValue,
                            Mode = bindingMode,
                            ViewValueChangedEvent = viewValueChangedEvent,
                        });
                    }
                }
            }

            return result;
        }
    }
}