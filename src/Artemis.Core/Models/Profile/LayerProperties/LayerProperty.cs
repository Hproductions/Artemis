﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Storage.Entities.Profile;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a property on a layer. Properties are saved in storage and can optionally be modified from the UI.
    ///     <para>
    ///         Note: You cannot initialize layer properties yourself. If properly placed and annotated, the Artemis core will
    ///         initialize these for you.
    ///     </para>
    /// </summary>
    /// <typeparam name="T">The type of property encapsulated in this layer property</typeparam>
    public class LayerProperty<T> : ILayerProperty
    {
        private bool _disposed;

        /// <summary>
        ///     Creates a new instance of the <see cref="LayerProperty{T}" /> class
        /// </summary>
        protected LayerProperty()
        {
            // These are set right after construction to keep the constructor (and inherited constructs) clean
            ProfileElement = null!;
            LayerPropertyGroup = null!;
            Path = null!;
            Entity = null!;
            PropertyDescription = null!;

            CurrentValue = default!;
            DefaultValue = default!;

            _baseValue = default!;
            _keyframes = new List<LayerPropertyKeyframe<T>>();
        }

        /// <summary>
        ///     Returns the type of the property
        /// </summary>
        public Type GetPropertyType()
        {
            return typeof(T);
        }

        /// <inheritdoc />
        public PropertyDescriptionAttribute PropertyDescription { get; internal set; }

        /// <inheritdoc />
        public string Path { get; private set; }

        /// <inheritdoc />
        public void Update(Timeline timeline)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            CurrentValue = BaseValue;

            UpdateKeyframes(timeline);
            UpdateDataBindings(timeline);

            OnUpdated();
        }

        #region IDisposable

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposed = true;

                foreach (IDataBinding dataBinding in _dataBindings)
                    dataBinding.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Hierarchy

        private bool _isHidden;

        /// <summary>
        ///     Gets or sets whether the property is hidden in the UI
        /// </summary>
        public bool IsHidden
        {
            get => _isHidden;
            set
            {
                _isHidden = value;
                OnVisibilityChanged();
            }
        }

        /// <summary>
        ///     Gets the profile element (such as layer or folder) this property is applied to
        /// </summary>
        public RenderProfileElement ProfileElement { get; internal set; }

        /// <summary>
        ///     The parent group of this layer property, set after construction
        /// </summary>
        public LayerPropertyGroup LayerPropertyGroup { get; internal set; }

        #endregion

        #region Value management

        private T _baseValue;

        /// <summary>
        ///     Called every update (if keyframes are both supported and enabled) to determine the new <see cref="CurrentValue" />
        ///     based on the provided progress
        /// </summary>
        /// <param name="keyframeProgress">The linear current keyframe progress</param>
        /// <param name="keyframeProgressEased">The current keyframe progress, eased with the current easing function</param>
        protected virtual void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets or sets the base value of this layer property without any keyframes applied
        /// </summary>
        public T BaseValue
        {
            get => _baseValue;
            set
            {
                if (Equals(_baseValue, value))
                    return;

                _baseValue = value;
                ReapplyUpdate();
            }
        }

        /// <summary>
        ///     Gets the current value of this property as it is affected by it's keyframes, updated once every frame
        /// </summary>
        public T CurrentValue { get; set; }

        /// <summary>
        ///     Gets or sets the default value of this layer property. If set, this value is automatically applied if the property
        ///     has no  value in storage
        /// </summary>
        public T DefaultValue { get; set; }

        /// <summary>
        ///     Sets the current value, using either keyframes if enabled or the base value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="time">
        ///     An optional time to set the value add, if provided and property is using keyframes the value will be set to an new
        ///     or existing keyframe.
        /// </param>
        public void SetCurrentValue(T value, TimeSpan? time)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (time == null || !KeyframesEnabled || !KeyframesSupported)
            {
                BaseValue = value;
            }
            else
            {
                // If on a keyframe, update the keyframe
                LayerPropertyKeyframe<T>? currentKeyframe = Keyframes.FirstOrDefault(k => k.Position == time.Value);
                // Create a new keyframe if none found
                if (currentKeyframe == null)
                    AddKeyframe(new LayerPropertyKeyframe<T>(value, time.Value, Easings.Functions.Linear, this));
                else
                    currentKeyframe.Value = value;
            }

            // Force an update so that the base value is applied to the current value and
            // keyframes/data bindings are applied using the new base value
            ReapplyUpdate();
        }

        /// <summary>
        ///     Overrides the property value with the default value
        /// </summary>
        public void ApplyDefaultValue()
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            BaseValue = DefaultValue;
            CurrentValue = DefaultValue;
        }

        private void ReapplyUpdate()
        {
            ProfileElement.Timeline.ClearDelta();
            Update(ProfileElement.Timeline);
            OnCurrentValueSet();
        }

        #endregion

        #region Keyframes

        private bool _keyframesEnabled;
        private List<LayerPropertyKeyframe<T>> _keyframes;

        /// <summary>
        ///     Gets whether keyframes are supported on this type of property
        /// </summary>
        public bool KeyframesSupported { get; protected internal set; } = true;

        /// <summary>
        ///     Gets or sets whether keyframes are enabled on this property, has no effect if <see cref="KeyframesSupported" /> is
        ///     False
        /// </summary>
        public bool KeyframesEnabled
        {
            get => _keyframesEnabled;
            set
            {
                if (_keyframesEnabled == value) return;
                _keyframesEnabled = value;
                OnKeyframesToggled();
            }
        }


        /// <summary>
        ///     Gets a read-only list of all the keyframes on this layer property
        /// </summary>
        public ReadOnlyCollection<LayerPropertyKeyframe<T>> Keyframes => _keyframes.AsReadOnly();

        /// <summary>
        ///     Gets the current keyframe in the timeline according to the current progress
        /// </summary>
        public LayerPropertyKeyframe<T>? CurrentKeyframe { get; protected set; }

        /// <summary>
        ///     Gets the next keyframe in the timeline according to the current progress
        /// </summary>
        public LayerPropertyKeyframe<T>? NextKeyframe { get; protected set; }

        /// <summary>
        ///     Adds a keyframe to the layer property
        /// </summary>
        /// <param name="keyframe">The keyframe to add</param>
        public void AddKeyframe(LayerPropertyKeyframe<T> keyframe)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (_keyframes.Contains(keyframe))
                return;

            keyframe.LayerProperty?.RemoveKeyframe(keyframe);
            keyframe.LayerProperty = this;
            _keyframes.Add(keyframe);

            SortKeyframes();
            OnKeyframeAdded();
        }

        /// <summary>
        ///     Removes a keyframe from the layer property
        /// </summary>
        /// <param name="keyframe">The keyframe to remove</param>
        public void RemoveKeyframe(LayerPropertyKeyframe<T> keyframe)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (!_keyframes.Contains(keyframe))
                return;

            _keyframes.Remove(keyframe);
            SortKeyframes();
            OnKeyframeRemoved();
        }

        /// <summary>
        ///     Sorts the keyframes in ascending order by position
        /// </summary>
        internal void SortKeyframes()
        {
            _keyframes = _keyframes.OrderBy(k => k.Position).ToList();
        }

        private void UpdateKeyframes(Timeline timeline)
        {
            if (!KeyframesSupported || !KeyframesEnabled)
                return;

            // The current keyframe is the last keyframe before the current time
            CurrentKeyframe = _keyframes.LastOrDefault(k => k.Position <= timeline.Position);
            // Keyframes are sorted by position so we can safely assume the next keyframe's position is after the current 
            if (CurrentKeyframe != null)
            {
                int nextIndex = _keyframes.IndexOf(CurrentKeyframe) + 1;
                NextKeyframe = _keyframes.Count > nextIndex ? _keyframes[nextIndex] : null;
            }
            else
            {
                NextKeyframe = null;
            }

            // No need to update the current value if either of the keyframes are null
            if (CurrentKeyframe == null)
            {
                CurrentValue = _keyframes.Any() ? _keyframes[0].Value : BaseValue;
            }
            else if (NextKeyframe == null)
            {
                CurrentValue = CurrentKeyframe.Value;
            }
            // Only determine progress and current value if both keyframes are present
            else
            {
                TimeSpan timeDiff = NextKeyframe.Position - CurrentKeyframe.Position;
                float keyframeProgress = (float) ((timeline.Position - CurrentKeyframe.Position).TotalMilliseconds / timeDiff.TotalMilliseconds);
                float keyframeProgressEased = (float) Easings.Interpolate(keyframeProgress, CurrentKeyframe.EasingFunction);
                UpdateCurrentValue(keyframeProgress, keyframeProgressEased);
            }
        }

        #endregion

        #region Data bindings

        // ReSharper disable InconsistentNaming
        internal readonly List<IDataBindingRegistration> _dataBindingRegistrations = new List<IDataBindingRegistration>();

        internal readonly List<IDataBinding> _dataBindings = new List<IDataBinding>();
        // ReSharper restore InconsistentNaming

        /// <summary>
        ///     Gets whether data bindings are supported on this type of property
        /// </summary>
        public bool DataBindingsSupported { get; protected internal set; } = true;

        /// <summary>
        ///     Gets whether the layer has any active data bindings
        /// </summary>
        public bool HasDataBinding => GetAllDataBindingRegistrations().Any(r => r.GetDataBinding() != null);

        /// <summary>
        ///     Gets a data binding registration by the expression used to register it
        ///     <para>Note: The expression must exactly match the one used to register the data binding</para>
        /// </summary>
        public DataBindingRegistration<T, TProperty>? GetDataBindingRegistration<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            return GetDataBindingRegistration<TProperty>(propertyExpression.ToString());
        }

        internal DataBindingRegistration<T, TProperty>? GetDataBindingRegistration<TProperty>(string expression)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            IDataBindingRegistration? match = _dataBindingRegistrations.FirstOrDefault(r => r is DataBindingRegistration<T, TProperty> registration &&
                                                                                            registration.PropertyExpression.ToString() == expression);
            return (DataBindingRegistration<T, TProperty>?) match;
        }

        /// <summary>
        ///     Gets a list containing all data binding registrations of this layer property
        /// </summary>
        /// <returns>A list containing all data binding registrations of this layer property</returns>
        public List<IDataBindingRegistration> GetAllDataBindingRegistrations()
        {
            return _dataBindingRegistrations;
        }

        /// <summary>
        ///     Registers a data binding property so that is available to the data binding system
        /// </summary>
        /// <typeparam name="TProperty">The type of the layer property</typeparam>
        /// <param name="propertyExpression">The expression pointing to the value to register</param>
        /// <param name="converter">The converter to use while applying the data binding</param>
        public void RegisterDataBindingProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression, DataBindingConverter<T, TProperty> converter)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess && propertyExpression.Body.NodeType != ExpressionType.Parameter)
                throw new ArtemisCoreException("Provided expression is invalid, it must be 'value => value' or 'value => value.Property'");

            if (converter.SupportedType != propertyExpression.ReturnType)
                throw new ArtemisCoreException($"Cannot register data binding property for property {PropertyDescription.Name} " +
                                               "because the provided converter does not support the property's type");

            _dataBindingRegistrations.Add(new DataBindingRegistration<T, TProperty>(this, converter, propertyExpression));
        }

        /// <summary>
        ///     Enables a data binding for the provided <paramref name="dataBindingRegistration" />
        /// </summary>
        /// <returns>The newly created data binding</returns>
        public DataBinding<T, TProperty> EnableDataBinding<TProperty>(DataBindingRegistration<T, TProperty> dataBindingRegistration)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (dataBindingRegistration.LayerProperty != this)
                throw new ArtemisCoreException("Cannot enable a data binding using a data binding registration of a different layer property");
            if (dataBindingRegistration.DataBinding != null)
                throw new ArtemisCoreException("Provided data binding registration already has an enabled data binding");

            DataBinding<T, TProperty> dataBinding = new DataBinding<T, TProperty>(dataBindingRegistration);
            _dataBindings.Add(dataBinding);

            OnDataBindingEnabled(new LayerPropertyEventArgs<T>(dataBinding.LayerProperty));
            return dataBinding;
        }

        /// <summary>
        ///     Disables the provided data binding
        /// </summary>
        /// <param name="dataBinding">The data binding to remove</param>
        public void DisableDataBinding<TProperty>(DataBinding<T, TProperty> dataBinding)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            _dataBindings.Remove(dataBinding);

            if (dataBinding.Registration != null)
                dataBinding.Registration.DataBinding = null;
            dataBinding.Dispose();
            OnDataBindingDisabled(new LayerPropertyEventArgs<T>(dataBinding.LayerProperty));
        }

        private void UpdateDataBindings(Timeline timeline)
        {
            // To avoid data bindings applying at non-regular updating (during editing) only update when not overriden
            if (timeline.IsOverridden)
                return;

            foreach (IDataBinding dataBinding in _dataBindings)
            {
                dataBinding.Update(timeline);
                dataBinding.Apply();
            }
        }

        #endregion

        #region Storage

        private bool _isInitialized;

        /// <summary>
        ///     Indicates whether the BaseValue was loaded from storage, useful to check whether a default value must be applied
        /// </summary>
        public bool IsLoadedFromStorage { get; internal set; }

        internal PropertyEntity Entity { get; set; }

        /// <inheritdoc />
        public void Initialize(RenderProfileElement profileElement, LayerPropertyGroup group, PropertyEntity entity, bool fromStorage, PropertyDescriptionAttribute description, string path)
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            _isInitialized = true;

            ProfileElement = profileElement ?? throw new ArgumentNullException(nameof(profileElement));
            LayerPropertyGroup = group ?? throw new ArgumentNullException(nameof(group));
            Path = path;
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            PropertyDescription = description ?? throw new ArgumentNullException(nameof(description));
            IsLoadedFromStorage = fromStorage;

            if (PropertyDescription.DisableKeyframes)
                KeyframesSupported = false;
        }

        /// <inheritdoc />
        public void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (!_isInitialized)
                throw new ArtemisCoreException("Layer property is not yet initialized");

            if (!IsLoadedFromStorage)
                ApplyDefaultValue();
            else
                try
                {
                    if (Entity.Value != null)
                        BaseValue = JsonConvert.DeserializeObject<T>(Entity.Value);
                }
                catch (JsonException)
                {
                    // ignored for now
                }

            CurrentValue = BaseValue;
            KeyframesEnabled = Entity.KeyframesEnabled;

            _keyframes.Clear();
            try
            {
                _keyframes.AddRange(
                    Entity.KeyframeEntities.Where(k => k.Position <= ProfileElement.Timeline.Length)
                        .Select(k => new LayerPropertyKeyframe<T>(JsonConvert.DeserializeObject<T>(k.Value), k.Position, (Easings.Functions) k.EasingFunction, this))
                );
            }
            catch (JsonException)
            {
                // ignored for now
            }

            _dataBindings.Clear();
            foreach (IDataBindingRegistration dataBindingRegistration in _dataBindingRegistrations)
            {
                IDataBinding? dataBinding = dataBindingRegistration.CreateDataBinding();
                if (dataBinding != null)
                    _dataBindings.Add(dataBinding);
            }
        }

        /// <summary>
        ///     Saves the property to the underlying property entity
        /// </summary>
        public void Save()
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            if (!_isInitialized)
                throw new ArtemisCoreException("Layer property is not yet initialized");

            Entity.Value = JsonConvert.SerializeObject(BaseValue);
            Entity.KeyframesEnabled = KeyframesEnabled;
            Entity.KeyframeEntities.Clear();
            Entity.KeyframeEntities.AddRange(Keyframes.Select(k => new KeyframeEntity
            {
                Value = JsonConvert.SerializeObject(k.Value),
                Position = k.Position,
                EasingFunction = (int) k.EasingFunction
            }));

            Entity.DataBindingEntities.Clear();
            foreach (IDataBinding dataBinding in _dataBindings)
                dataBinding.Save();
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs once every frame when the layer property is updated
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs<T>>? Updated;

        /// <summary>
        ///     Occurs when the current value of the layer property was updated by some form of input
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs<T>>? CurrentValueSet;

        /// <summary>
        ///     Occurs when the <see cref="IsHidden" /> value of the layer property was updated
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs<T>>? VisibilityChanged;

        /// <summary>
        ///     Occurs when keyframes are enabled/disabled
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs<T>>? KeyframesToggled;

        /// <summary>
        ///     Occurs when a new keyframe was added to the layer property
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs<T>>? KeyframeAdded;

        /// <summary>
        ///     Occurs when a keyframe was removed from the layer property
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs<T>>? KeyframeRemoved;

        /// <summary>
        ///     Occurs when a data binding has been enabled
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs<T>>? DataBindingEnabled;

        /// <summary>
        ///     Occurs when a data binding has been disabled
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs<T>>? DataBindingDisabled;

        /// <summary>
        ///     Invokes the <see cref="Updated" /> event
        /// </summary>
        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this, new LayerPropertyEventArgs<T>(this));
        }

        /// <summary>
        ///     Invokes the <see cref="CurrentValueSet" /> event
        /// </summary>
        protected virtual void OnCurrentValueSet()
        {
            CurrentValueSet?.Invoke(this, new LayerPropertyEventArgs<T>(this));
            LayerPropertyGroup.OnLayerPropertyOnCurrentValueSet(new LayerPropertyEventArgs(this));
        }

        /// <summary>
        ///     Invokes the <see cref="VisibilityChanged" /> event
        /// </summary>
        protected virtual void OnVisibilityChanged()
        {
            VisibilityChanged?.Invoke(this, new LayerPropertyEventArgs<T>(this));
        }

        /// <summary>
        ///     Invokes the <see cref="KeyframesToggled" /> event
        /// </summary>
        protected virtual void OnKeyframesToggled()
        {
            KeyframesToggled?.Invoke(this, new LayerPropertyEventArgs<T>(this));
        }

        /// <summary>
        ///     Invokes the <see cref="KeyframeAdded" /> event
        /// </summary>
        protected virtual void OnKeyframeAdded()
        {
            KeyframeAdded?.Invoke(this, new LayerPropertyEventArgs<T>(this));
        }

        /// <summary>
        ///     Invokes the <see cref="KeyframeRemoved" /> event
        /// </summary>
        protected virtual void OnKeyframeRemoved()
        {
            KeyframeRemoved?.Invoke(this, new LayerPropertyEventArgs<T>(this));
        }

        /// <summary>
        ///     Invokes the <see cref="DataBindingEnabled" /> event
        /// </summary>
        protected virtual void OnDataBindingEnabled(LayerPropertyEventArgs<T> e)
        {
            DataBindingEnabled?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the <see cref="DataBindingDisabled" /> event
        /// </summary>
        protected virtual void OnDataBindingDisabled(LayerPropertyEventArgs<T> e)
        {
            DataBindingDisabled?.Invoke(this, e);
        }

        #endregion
    }
}