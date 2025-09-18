using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RecipeApp.Models
{
    public class Recipe : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        private string _title;
        private string _description;
        private string _imageUrl;
        private List<string> _ingredients;
        private string _instructions;
        private int _cookingTimeMinutes;
        private string _author;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public List<string> Ingredients
        {
            get => _ingredients;
            set => SetProperty(ref _ingredients, value);
        }

        public string Instructions
        {
            get => _instructions;
            set => SetProperty(ref _instructions, value);
        }

        public int CookingTimeMinutes
        {
            get => _cookingTimeMinutes;
            set => SetProperty(ref _cookingTimeMinutes, value);
        }

        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Notify UI that a property has changed
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Set the backing field and notify UI if value changed
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}