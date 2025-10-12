using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RecipeApp.Shared.Models
{
    public class Recipe : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _title;
        private string _description;
        private string _imageUrl;
        private List<string> _ingredients = new();
        private string _instructions;
        private int _cookingTimeMinutes;
        private string _author;
        private bool _isFavorite;
        private Guid? _categoryId;
        private Category? _category; // Navigation property

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

        public bool IsFavorite
        {
            get => _isFavorite;
            set => SetProperty(ref _isFavorite, value);
        }

        // Foreign key for Category
        public Guid? CategoryId
        {
            get => _categoryId;
            set => SetProperty(ref _categoryId, value);
        }

        // Navigation property
        public Category? Category
        {
            get => _category;
            set
            {
                SetProperty(ref _category, value);
                // Optionally keep CategoryId in sync
                if (value != null)
                    CategoryId = value.Id;
            }
        }

        // Convenience property for getting category name
        public string? CategoryName => Category?.Name;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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