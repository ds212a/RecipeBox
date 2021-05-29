using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace RecipeBox
{
    public class RecipeInstruction
    {
        #region Fields
        private string id = string.Empty;
        private uint index = 0;
        private string instruction = string.Empty;
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

        public string Instruction
        {
            get => instruction;
            set
            {
                if (instruction != value)
                {
                    instruction = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Constructors
        public RecipeInstruction()
        {

        }

        public RecipeInstruction(uint index, string instruction)
        {
            Id = Guid.NewGuid().ToString();
            Index = index;
            Instruction = instruction;
        }
        #endregion

        public override string ToString()
        {
            return $"{Index}. {Instruction}";
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
