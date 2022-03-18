﻿using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.ScriptingProviders;
using Artemis.UI.Screens.Header;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions.Event;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions.Static;
using Artemis.UI.Screens.ProfileEditor.LayerProperties;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.LayerEffects;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem;
using Artemis.UI.Screens.ProfileEditor.Visualization;
using Artemis.UI.Screens.ProfileEditor.Visualization.Tools;
using Artemis.UI.Screens.ProfileEditor.Windows;
using Artemis.UI.Screens.Scripting;
using Artemis.UI.Screens.Settings.Device;
using Artemis.UI.Screens.Settings.Device.Tabs;
using Artemis.UI.Screens.Settings.Tabs.Devices;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Screens.Sidebar.Dialogs.ProfileEdit;
using Stylet;

namespace Artemis.UI.Ninject.Factories;

public interface IVmFactory
{
}

public interface ISettingsVmFactory : IVmFactory
{
    PluginSettingsViewModel CreatePluginSettingsViewModel(Plugin plugin);
    PluginFeatureViewModel CreatePluginFeatureViewModel(PluginFeatureInfo pluginFeatureInfo, bool showShield);
    DeviceSettingsViewModel CreateDeviceSettingsViewModel(ArtemisDevice device);
}

public interface IDeviceDebugVmFactory : IVmFactory
{
    DeviceDialogViewModel DeviceDialogViewModel(ArtemisDevice device);
    DevicePropertiesTabViewModel DevicePropertiesTabViewModel(ArtemisDevice device);
    DeviceInfoTabViewModel DeviceInfoTabViewModel(ArtemisDevice device);
    DeviceLedsTabViewModel DeviceLedsTabViewModel(ArtemisDevice device);
    InputMappingsTabViewModel InputMappingsTabViewModel(ArtemisDevice device);
}

public interface IProfileTreeVmFactory : IVmFactory
{
    FolderViewModel FolderViewModel(ProfileElement folder);
    LayerViewModel LayerViewModel(ProfileElement layer);
}

public interface ILayerHintVmFactory : IVmFactory
{
    LayerHintsDialogViewModel LayerHintsDialogViewModel(Layer layer);
    CategoryAdaptionHintViewModel CategoryAdaptionHintViewModel(CategoryAdaptionHint adaptionHint);
    DeviceAdaptionHintViewModel DeviceAdaptionHintViewModel(DeviceAdaptionHint adaptionHint);
    KeyboardSectionAdaptionHintViewModel KeyboardSectionAdaptionHintViewModel(KeyboardSectionAdaptionHint adaptionHint);
}

public interface IHeaderVmFactory : IVmFactory
{
    SimpleHeaderViewModel SimpleHeaderViewModel(string displayName);
}

public interface IProfileLayerVmFactory : IVmFactory
{
    ProfileLayerViewModel Create(Layer layer, PanZoomViewModel panZoomViewModel);
}

public interface IVisualizationToolVmFactory : IVmFactory
{
    ViewpointMoveToolViewModel ViewpointMoveToolViewModel(PanZoomViewModel panZoomViewModel);
    EditToolViewModel EditToolViewModel(PanZoomViewModel panZoomViewModel);
    SelectionToolViewModel SelectionToolViewModel(PanZoomViewModel panZoomViewModel);
    SelectionRemoveToolViewModel SelectionRemoveToolViewModel(PanZoomViewModel panZoomViewModel);
}

public interface ILayerPropertyVmFactory : IVmFactory
{
    LayerPropertyViewModel LayerPropertyViewModel(ILayerProperty layerProperty);

    LayerPropertyGroupViewModel LayerPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup);
    TreeGroupViewModel TreeGroupViewModel(LayerPropertyGroupViewModel layerPropertyGroupViewModel);
    TimelineGroupViewModel TimelineGroupViewModel(LayerPropertyGroupViewModel layerPropertyGroupViewModel);

    TreeViewModel TreeViewModel(LayerPropertiesViewModel layerPropertiesViewModel, IObservableCollection<LayerPropertyGroupViewModel> layerPropertyGroups);
    EffectsViewModel EffectsViewModel(LayerPropertiesViewModel layerPropertiesViewModel);
    TimelineViewModel TimelineViewModel(LayerPropertiesViewModel layerPropertiesViewModel, IObservableCollection<LayerPropertyGroupViewModel> layerPropertyGroups);
    TimelineSegmentViewModel TimelineSegmentViewModel(SegmentViewModelType segment, IObservableCollection<LayerPropertyGroupViewModel> layerPropertyGroups);
}

public interface IConditionVmFactory : IVmFactory
{
    StaticConditionViewModel StaticConditionViewModel(StaticCondition staticCondition);
    EventConditionViewModel EventConditionViewModel(EventCondition eventCondition);
}

public interface IPrerequisitesVmFactory : IVmFactory
{
    PluginPrerequisiteViewModel PluginPrerequisiteViewModel(PluginPrerequisite pluginPrerequisite, bool uninstall);
}

public interface IScriptVmFactory : IVmFactory
{
    ScriptsDialogViewModel ScriptsDialogViewModel(Profile profile);
    ScriptConfigurationViewModel ScriptConfigurationViewModel(ScriptConfiguration scriptConfiguration);
}

public interface ISidebarVmFactory : IVmFactory
{
    SidebarCategoryViewModel SidebarCategoryViewModel(ProfileCategory profileCategory);
    SidebarProfileConfigurationViewModel SidebarProfileConfigurationViewModel(ProfileConfiguration profileConfiguration);
    ProfileConfigurationHotkeyViewModel ProfileConfigurationHotkeyViewModel(ProfileConfiguration profileConfiguration, bool isDisableHotkey);
    ModuleActivationRequirementViewModel ModuleActivationRequirementViewModel(IModuleActivationRequirement activationRequirement);
}

public interface INodeVmFactory : IVmFactory
{
    NodeScriptWindowViewModel NodeScriptWindowViewModel(NodeScript nodeScript);
}

public interface IPropertyVmFactory
{
    ITreePropertyViewModel TreePropertyViewModel(ILayerProperty layerProperty, LayerPropertyViewModel layerPropertyViewModel);
    ITimelinePropertyViewModel TimelinePropertyViewModel(ILayerProperty layerProperty, LayerPropertyViewModel layerPropertyViewModel);
}