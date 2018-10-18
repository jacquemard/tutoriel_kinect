using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectDemos
{
    public sealed class DataSource
    {
        private static readonly DataSource dataSource = new DataSource();

        private readonly ObservableCollection<DataCollection> _allGroups = new ObservableCollection<DataCollection>();

        private static readonly Uri DarkGrayImage = new Uri("DarkGray.png", UriKind.Relative);
        private static readonly Uri MediumGrayImage = new Uri("MediumGray.png", UriKind.Relative);

        public DataSource()
        {
            const string itemContent = "Content";

            var dataGroup = new DataCollection(
                "Group-1",
                "Group Title: 3",
                "Group Subtitle: 3",
                MediumGrayImage,
                string.Empty);

            dataGroup.Items.Add(
                new DataItem(
                    "Group-1-Item-1",
                    "Custom Controls",
                    string.Empty,
                    DarkGrayImage,
                    "Building custom controls",
                    itemContent,
                    dataGroup,
                    typeof (CustomManipulation)));

            AllGroups.Add(dataGroup);
        }

        public ObservableCollection<DataCollection> AllGroups
        {
            get { return _allGroups; }
        }

        public static DataCollection GetGroup(string uniqueId)
        {
            var matches = dataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));

            if (matches.Count() == 1)
            {
                return matches.First();
            }

            return null;
        }

        public static DataItem GetItem(string uniqueId)
        {
            var matches = dataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));

            if (matches.Count() == 1)
            {
                return matches.First();
            }

            return null;
        }
    }

    public class DataItem : DataCommon
    {
        private string _content = string.Empty;
        private DataCollection _group;
        private Type _navigationPage;

        public DataItem(string uniqueId, string title, string subtitle, Uri imagePath, string description, string content, DataCollection group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
            _navigationPage = null;
        }

        public DataItem(string uniqueId, string title, string subtitle, Uri imagePath, string description, string content, DataCollection group, Type navigationPage)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
            this._navigationPage = navigationPage;
        }

        public string Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }

        public DataCollection Group
        {
            get { return _group; }
            set { SetProperty(ref _group, value); }
        }

        public Type NavigationPage
        {
            get { return _navigationPage; }
            set { SetProperty(ref _navigationPage, value); }
        }
    }

    public class DataCollection : DataCommon, IEnumerable
    {
        private readonly ObservableCollection<DataItem> _items = new ObservableCollection<DataItem>();
        private readonly ObservableCollection<DataItem> _topItems = new ObservableCollection<DataItem>();

        public DataCollection(string uniqueId, string title, string subtitle, Uri imagePath, string description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        public ObservableCollection<DataItem> Items
        {
            get { return _items; }
        }

        public ObservableCollection<DataItem> TopItems
        {
            get { return _topItems; }
        }

        public IEnumerator GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }

                    break;
            }
        }
    }

    public abstract class DataCommon : BindableBase
    {
        private static readonly Uri BaseUri = new Uri("pack://application:,,,/");
        private string _uniqueId = string.Empty;
        private string _title = string.Empty;
        private string _subtitle = string.Empty;
        private string _description = string.Empty;
        private ImageSource _image;
        private Uri _imagePath;

        protected DataCommon(string uniqueId, string title, string subtitle, Uri imagePath, string description)
        {
            _uniqueId = uniqueId;
            _title = title;
            _subtitle = subtitle;
            _description = description;
            _imagePath = imagePath;
        }

        public string UniqueId
        {
            get { return _uniqueId; }
            set { SetProperty(ref _uniqueId, value); }
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public string Subtitle
        {
            get { return _subtitle; }
            set { SetProperty(ref _subtitle, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        public ImageSource Image
        {
            get
            {
                if (_image == null && _imagePath != null)
                {
                    _image = new BitmapImage(new Uri(BaseUri, _imagePath));
                }

                return _image;
            }

            set
            {
                _imagePath = null;
                SetProperty(ref _image, value);
            }
        }

        public void SetImage(Uri path)
        {
            _image = null;
            _imagePath = path;
            OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return Title;
        }
    }
}