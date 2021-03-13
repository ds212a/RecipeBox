using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RecipeBox
{
    public class RecipeImage
    {
        #region Fields
        private string id = string.Empty;
        private string originalImagePath = string.Empty;
        private string localImagePath = string.Empty;
        #endregion

        #region Properties
        [XmlIgnore]
        public string Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string OriginalImagePath
        {
            get => originalImagePath;
            set
            {
                if (originalImagePath != value)
                {
                    originalImagePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LocalImagePath
        {
            get => localImagePath;
            set
            {
                if (localImagePath != value)
                {
                    localImagePath = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Constructors
        public RecipeImage()
        {

        }

        public RecipeImage(string originalImagePath, string localImagePath)
        {
            Id = Guid.NewGuid().ToString();
            OriginalImagePath = originalImagePath;
            LocalImagePath = localImagePath;
        }
        #endregion

        #region Property Changed methods
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
