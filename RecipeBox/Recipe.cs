using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RecipeBox
{
    public class Recipe : INotifyPropertyChanged
    {
        #region Fields
        private string id = string.Empty;
        private string name = string.Empty;
        private string description = string.Empty;
        private TimeSpan prepTime = new TimeSpan();
        private TimeSpan cookTime = new TimeSpan();
        private TimeSpan totalTime = new TimeSpan();
        private string url = string.Empty;
        private uint servings = 0;
        private double rating = 0;
        private bool needsSaved = false;

        private ObservableCollection<string> categories = new ObservableCollection<string>();
        private ObservableCollection<string> cuisines = new ObservableCollection<string>();
        private ObservableCollection<Ingredient> ingredients = new ObservableCollection<Ingredient>();
        private ObservableCollection<RecipeInstruction> instructions = new ObservableCollection<RecipeInstruction>();
        private ObservableCollection<RecipeNote> notes = new ObservableCollection<RecipeNote>();
        private ObservableCollection<RecipeImage> images = new ObservableCollection<RecipeImage>();
        #endregion

        #region Properties
        public string Id
        {
            get => id;
            set => SetEditingProperty(ref id, value);
        }

        public string Name
        {
            get => name;
            set => SetEditingProperty(ref name, value);
        }

        public string Description
        {
            get => description;
            set => SetEditingProperty(ref description, value);
        }

        public TimeSpan PrepTime
        {
            get => prepTime;
            set => SetEditingProperty(ref prepTime, value);
        }

        public TimeSpan CookTime
        {
            get => cookTime;
            set => SetEditingProperty(ref cookTime, value);
        }

        public TimeSpan TotalTime
        {
            get => totalTime;
            set => SetEditingProperty(ref totalTime, value);
        }

        public string Url
        {
            get => url;
            set => SetEditingProperty(ref url, value);
        }

        public uint Servings
        {
            get => servings;
            set => SetEditingProperty(ref servings, value);
        }

        public double Rating
        {
            get => rating;
            set => SetEditingProperty(ref rating, value);
        }

        [XmlArrayItem("Category")]
        public ObservableCollection<string> Categories
        {
            get => categories;
            set
            {
                if (categories != value)
                {
                    categories = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlArrayItem("Cuisine")]
        public ObservableCollection<string> Cuisines
        {
            get => cuisines;
            set
            {
                if (cuisines != value)
                {
                    cuisines = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Ingredient> Ingredients
        {
            get => ingredients;
            set
            {
                if (ingredients != value)
                {
                    ingredients = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlArrayItem("Instruction")]
        public ObservableCollection<RecipeInstruction> Instructions
        {
            get => instructions;
            set
            {
                if (instructions != value)
                {
                    instructions = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlArrayItem("Note")]
        public ObservableCollection<RecipeNote> Notes
        {
            get => notes;
            set
            {
                if (notes != value)
                {
                    notes = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlArrayItem("Image")]
        public ObservableCollection<RecipeImage> Images
        {
            get => images;
            set
            {
                if (images != value)
                {
                    images = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlIgnore]
        public bool NeedsSaved
        {
            get => needsSaved;
            set => SetProperty(ref needsSaved, value);
        }
        #endregion

        #region Constructors
        public Recipe()
        {
            Categories.CollectionChanged += Categories_CollectionChanged;
            Cuisines.CollectionChanged += Cuisines_CollectionChanged;
            Ingredients.CollectionChanged += Ingredients_CollectionChanged;
            Instructions.CollectionChanged += Instructions_CollectionChanged;
            Notes.CollectionChanged += Notes_CollectionChanged;
            Images.CollectionChanged += Images_CollectionChanged;
        }
        #endregion

        #region Event Handlers
        void Categories_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //list changed - an item was added.
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                NeedsSaved = true;
            }
        }

        void Cuisines_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //list changed - an item was added.
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                NeedsSaved = true;
            }
        }

        void Ingredients_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //list changed - an item was added.
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                NeedsSaved = true;
            }
        }

        void Instructions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //list changed - an item was added.
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                NeedsSaved = true;
            }
        }

        void Notes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //list changed - an item was added.
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                NeedsSaved = true;
            }
        }

        void Images_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //list changed - an item was added.
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                NeedsSaved = true;
            }
        }
        #endregion

        #region Property Changed methods
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetEditingProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (SetProperty(ref storage, value, propertyName))
            {
                if (Name != "" || Description != "" || CookTime.Equals(new TimeSpan()) == false || PrepTime.Equals(new TimeSpan()) == false || TotalTime.Equals(new TimeSpan()) == false || Servings != 0 || Url != "" || Rating != 0)
                {
                    NeedsSaved = true;
                }
                else
                {
                    NeedsSaved = false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }
        #endregion
    }
}
