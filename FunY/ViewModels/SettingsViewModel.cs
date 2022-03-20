using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace FunY.ViewModels
{
    public class SettingsViewModel : SettingsManager
    {
        public SettingsViewModel() { }

        /*
         * Sources:
         * 
         * 0. Local
         * 1. Quotes API
         * 2. JokeAPI
         */ 
        public int Source
        {
            get => Get("Local", nameof(Source), 1);
            set => Set("Local", nameof(Source), value);
        }
    }

    public abstract class SettingsManager : ViewModel
    {
        /// <summary>
        /// Gets an app setting.
        /// </summary>
        /// <param name="store">Setting store name.</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="defaultValue">Default setting value.</param>
        /// <returns>App setting value.</returns>
        /// <remarks>If the store parameter is "Local", a local setting will be returned.</remarks>
        protected T Get<T>(string store, string setting, T defaultValue)
        {
            // If store == "Local", get a local setting
            if (store == "Local")
            {
                // Get app settings
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

                // Check if the setting exists
                if (localSettings.Values[setting] == null)
                {
                    localSettings.Values[setting] = defaultValue;
                }

                object val = localSettings.Values[setting];

                // Return the setting if type matches
                if (val is not T)
                {
                    throw new ArgumentException("Type mismatch for \"" + setting + "\" in local store. Got " + val.GetType());
                }
                return (T)val;
            }

            // Get desired composite value
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)roamingSettings.Values[store];

            // If the store exists, check if the setting does as well
            if (composite == null)
            {
                composite = new ApplicationDataCompositeValue();
            }

            if (composite[setting] == null)
            {
                composite[setting] = defaultValue;
                roamingSettings.Values[store] = composite;
            }

            object value = composite[setting];

            // Return the setting if type matches
            if (value is not T)
            {
                throw new ArgumentException("Type mismatch for \"" + setting + "\" in store \"" + store + "\". Current type is " + value.GetType());
            }
            return (T)value;
        }

        /// <summary>
        /// Sets an app setting.
        /// </summary>
        /// <param name="store">Setting store name.</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="newValue">New setting value.</param>
        /// <remarks>If the store parameter is "Local", a local setting will be set.</remarks>
        protected void Set<T>(string store, string setting, T newValue)
        {
            // Try to get the setting, if types don't match, it'll throw an exception
            _ = Get(store, setting, newValue);

            // If store == "Local", set a local setting
            if (store == "Local")
            {
                // Get app settings
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values[setting] = newValue;
                return;
            }

            // Get desired composite value
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)roamingSettings.Values[store];

            // Store doesn't exist, create it
            if (composite == null)
            {
                composite = new ApplicationDataCompositeValue();
            }

            // Set the setting to the desired value
            composite[setting] = newValue;
            roamingSettings.Values[store] = composite;

            OnPropertyChanged(setting);
        }
    }

    /// <summary>
    /// Base ViewModel implementation, wraps a <typeparamref name="Type"/>
    /// object for the Model-View-ViewModel pattern and contains methods
    /// to handle property changes.
    /// </summary>
    /// <typeparam name="Type">Type of the underlying model.</typeparam>
    public abstract class ViewModel<Type> : ViewModel
    {
        private Type _model;
        /// <summary>
        /// Gets or sets the underlying <see cref="Type"/> object.
        /// </summary>
        public Type Model
        {
            get => _model;
            set
            {
                if (_model == null || !_model.Equals(value))
                {
                    _model = value;

                    // Raise the PropertyChanged event for all properties.
                    OnPropertyChanged(string.Empty);
                }
            }
        }
    }

    /// <summary>
    /// Base ViewModel implementation, contains methods to
    /// handle property changes.
    /// </summary>
    public abstract class ViewModel : INotifyPropertyChanged
    {
        private readonly Dictionary<PropertyChangedEventHandler, SynchronizationContext> PropertyChangedEvents = new();

        /// <summary> 
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                PropertyChangedEvents.Add(value, SynchronizationContext.Current);
            }
            remove
            {
                PropertyChangedEvents.Remove(value);
            }
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
            foreach (KeyValuePair<PropertyChangedEventHandler, SynchronizationContext> @event in PropertyChangedEvents)
            {
                if (@event.Value == null)
                {
                    @event.Key.Invoke(this, args);
                }
                else
                {
                    @event.Value.Post(s => @event.Key.Invoke(s, args), this);
                }
            }
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool Set<T>(ref T storage, T value,
            [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
