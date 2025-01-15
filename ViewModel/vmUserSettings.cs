using Newtonsoft.Json.Linq;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.Model;

namespace VisualHFT.ViewModel
{
    public class PropertyViewModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public ObservableCollection<PropertyViewModel> Children { get; set; } = new ObservableCollection<PropertyViewModel>();
        public bool IsObject => Children.Count > 0;
    }

    public class vmUserSettings: BindableBase
    {

        private ObservableCollection<PropertyViewModel> _properties = new ObservableCollection<PropertyViewModel>();
        public ObservableCollection<PropertyViewModel> Properties
        {
            get => _properties;
            set
            {
                if (_properties != value)
                {
                    SetProperty(ref _properties, value);
                }
            }
        }

        public void LoadJson(string jsonContent)
        {
            JObject jsonObject = JObject.Parse(jsonContent);
            Properties = ParseJObject(jsonObject);
        }

        private ObservableCollection<PropertyViewModel> ParseJObject(JObject obj)
        {
            var properties = new ObservableCollection<PropertyViewModel>();

            foreach (var prop in obj.Properties())
            {
                var propertyViewModel = new PropertyViewModel { Name = prop.Name };

                if (prop.Value.Type == JTokenType.Object)
                {
                    propertyViewModel.Children = ParseJObject(prop.Value as JObject);
                }
                else
                {
                    propertyViewModel.Value = prop.Value.ToString();
                }

                properties.Add(propertyViewModel);
            }

            return properties;
        }

    }
}
