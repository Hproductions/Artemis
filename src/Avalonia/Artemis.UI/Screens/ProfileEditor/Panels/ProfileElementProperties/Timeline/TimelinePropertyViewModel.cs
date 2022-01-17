﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Timeline
{
    public class TimelinePropertyViewModel<T> : ActivatableViewModelBase, ITimelinePropertyViewModel
    {
        private readonly IProfileEditorService _profileEditorService;
        public LayerProperty<T> LayerProperty { get; }
        public ProfileElementPropertyViewModel ProfileElementPropertyViewModel { get; }
        public ObservableCollection<TimelineKeyframeViewModel<T>> KeyframeViewModels { get; }

        public TimelinePropertyViewModel(LayerProperty<T> layerProperty, ProfileElementPropertyViewModel profileElementPropertyViewModel, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;
            LayerProperty = layerProperty;
            ProfileElementPropertyViewModel = profileElementPropertyViewModel;
            KeyframeViewModels = new ObservableCollection<TimelineKeyframeViewModel<T>>();
        }

        #region Implementation of ITimelinePropertyViewModel

        public List<ITimelineKeyframeViewModel> GetAllKeyframeViewModels()
        {
            return KeyframeViewModels.Cast<ITimelineKeyframeViewModel>().ToList();
        }

        public void WipeKeyframes(TimeSpan? start, TimeSpan? end)
        {
            start ??= TimeSpan.Zero;
            end ??= TimeSpan.MaxValue;


            List<LayerPropertyKeyframe<T>> toShift = LayerProperty.Keyframes.Where(k => k.Position >= start && k.Position < end).ToList();
            foreach (LayerPropertyKeyframe<T> keyframe in toShift)
                LayerProperty.RemoveKeyframe(keyframe);

            UpdateKeyframes();
        }

        public void ShiftKeyframes(TimeSpan? start, TimeSpan? end, TimeSpan amount)
        {
            start ??= TimeSpan.Zero;
            end ??= TimeSpan.MaxValue;

            List<LayerPropertyKeyframe<T>> toShift = LayerProperty.Keyframes.Where(k => k.Position > start && k.Position < end).ToList();
            foreach (LayerPropertyKeyframe<T> keyframe in toShift)
                keyframe.Position += amount;

            UpdateKeyframes();
        }

        #endregion

        private void UpdateKeyframes()
        {
            // Only show keyframes if they are enabled
            if (LayerProperty.KeyframesEnabled)
            {
                List<LayerPropertyKeyframe<T>> keyframes = LayerProperty.Keyframes.ToList();

                List<TimelineKeyframeViewModel<T>> toRemove = KeyframeViewModels.Where(t => !keyframes.Contains(t.LayerPropertyKeyframe)).ToList();
                foreach (TimelineKeyframeViewModel<T> timelineKeyframeViewModel in toRemove)
                    KeyframeViewModels.Remove(timelineKeyframeViewModel);
                List<TimelineKeyframeViewModel<T>> toAdd = keyframes.Where(k => KeyframeViewModels.All(t => t.LayerPropertyKeyframe != k)).Select(k => new TimelineKeyframeViewModel<T>(k, _profileEditorService)).ToList();
                foreach (TimelineKeyframeViewModel<T> timelineKeyframeViewModel in toAdd)
                    KeyframeViewModels.Add(timelineKeyframeViewModel);
            }
            else
                KeyframeViewModels.Clear();

            foreach (TimelineKeyframeViewModel<T> timelineKeyframeViewModel in KeyframeViewModels)
                timelineKeyframeViewModel.Update();
        }
    }
}