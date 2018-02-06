using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Ratio.UWP.Controls
{
    [TemplatePart(Name = "FiltersContainer", Type = typeof(StackPanel))]
    public sealed class RFilteredWrapGridSelector : ContentControl
    {
        #region Properties

        private StackPanel _filtersContainer;
        #region Dependency Properties
        public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
            "ItemContainerStyle", typeof(Style), typeof(RFilteredWrapGridSelector), new PropertyMetadata(default(Style)));

        public Style ItemContainerStyle
        {
            get => (Style) GetValue(ItemContainerStyleProperty);
            set => SetValue(ItemContainerStyleProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(RFilteredWrapGridSelector), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty FilterListProperty = DependencyProperty.Register(
            "FilterList", typeof(IList), typeof(RFilteredWrapGridSelector), new PropertyMetadata(default(IList), FilterListChanged));

        private static void FilterListChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var filterList = dependencyPropertyChangedEventArgs.NewValue as IList;
            if(filterList == null) return;
            var filterControl = dependencyObject as RFilteredWrapGridSelector;
            if(filterControl == null) return;
            if (filterList.Count > 0)
            {
                filterControl.PopulateFilterContainer(filterList);
                var notifyCollection = dependencyPropertyChangedEventArgs.NewValue as INotifyCollectionChanged;
                if (notifyCollection != null)
                {
                    notifyCollection.CollectionChanged += filterControl.NotifyCollectionOnCollectionChanged;
                }
            }
        }

        private void NotifyCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add || notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                PopulateFilterContainer(FilterList);
            }
        }

        public IList FilterList
        {
            get => (IList) GetValue(FilterListProperty);
            set => SetValue(FilterListProperty, value);
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(RFilteredWrapGridSelector), new PropertyMetadata(default(ICommand)));

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        #endregion
        #endregion

        public RFilteredWrapGridSelector()
        {
            DefaultStyleKey = typeof(RFilteredWrapGridSelector);
            TabFocusNavigation = KeyboardNavigationMode.Local;
            TabNavigation = KeyboardNavigationMode.Local;
        }

        #region Overrides

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _filtersContainer = GetTemplateChild("FiltersContainer") as StackPanel;
            if (_filtersContainer != null && FilterList != null && FilterList.Count > 0)
            {
                PopulateFilterContainer(FilterList);
            }
        }

        #endregion

        #region Support Methods

        private void PopulateFilterContainer(IList filterList)
        {
            if (ItemTemplate == null || _filtersContainer == null) return;
            _filtersContainer.Children.Clear();
            foreach (var filter in filterList)
            {
                var container = new RFilterItem()
                {
                    ContentTemplate = ItemTemplate,
                    DataContext = filter
                };
                if (ItemContainerStyle != null && ItemContainerStyle.TargetType == typeof(RFilterItem))
                    container.Style = ItemContainerStyle;
                container.ItemSelected += ContainerOnItemSelected;

                _filtersContainer.Children.Add(container);
                if (_filtersContainer.Children.Count == 1)
                {
                    container.Selected = true;
                }
            }
            foreach (var uielement in _filtersContainer.Children)
            {
                var filteredItem = uielement as RFilterItem;
                if (filteredItem == null) continue;
                var siblings = _filtersContainer.Children.Where(element => element != filteredItem).ToList();
                filteredItem.Siblings = new List<RFilterItem>(siblings.Cast<RFilterItem>());
            }
        }

        private void ContainerOnItemSelected(object sender, object o)
        {
            if (Command != null && Command.CanExecute(o))
            {
                Command.Execute(o);
            }
        }

        #endregion
    }
}
