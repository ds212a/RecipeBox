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
    public class Ingredient : INotifyPropertyChanged
    {
        #region Enums
        public enum UnitOfMeasurements
        {
            Teaspoon = 0,
            Tablespoon = 1,
            Cup = 2,
            FluidOunce = 3,
            Pint = 4,
            Quart = 5,
            Gallon = 6,
            Ounce = 7,
            Pound = 8,
            Inch = 9,
            Milligram = 10,
            Gram = 11,
            Kilogram = 12,
            Milliliters = 13,
            Liter = 14,
            Deciliter = 15,
            Bunch = 16,
            Can = 17,
            Packet = 18,
            Pinch = 19,
            Bottle = 20,
            Clove = 21,
            Header = 22,
            Item = 23
        };
        #endregion

        #region Fields
        private string id = string.Empty;
        private uint index = 0;
        private string name = string.Empty;
        private uint quantity = 0;
        private UnitOfMeasurements unitOfMeasurement = UnitOfMeasurements.Item;
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

        [XmlAttribute("Index")]
        public uint Index
        {
            get => index;
            set
            {
                if (index != value)
                {
                    index = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }

        public uint Quantity
        {
            get => quantity;
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public UnitOfMeasurements UnitOfMeasurement
        {
            get => unitOfMeasurement;
            set
            {
                if (unitOfMeasurement != value)
                {
                    unitOfMeasurement = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Constructors
        public Ingredient()
        {

        }

        public Ingredient(string id, uint index, string name, uint quantity, UnitOfMeasurements unitOfMeasurement)
        {
            Id = id;
            Index = index;
            Name = name;
            Quantity = quantity;
            UnitOfMeasurement = unitOfMeasurement;
        }
        #endregion

        #region Unit Conversion Methods
        #endregion

        public override string ToString()
        {
            return $"{Quantity} {UnitOfMeasurement} {Name}";
        }

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
