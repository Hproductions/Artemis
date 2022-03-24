﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Avalonia;
using Avalonia.Controls.Mixins;
using Avalonia.Media;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public abstract class PinViewModel : ActivatableViewModelBase
{
    private readonly INodeService _nodeService;
    private ReactiveCommand<IPin, Unit>? _removePin;
    private Point _position;
    private Color _pinColor;
    private Color _darkenedPinColor;

    protected PinViewModel(IPin pin, INodeService nodeService)
    {
        _nodeService = nodeService;

        Pin = pin;
        SourceList<IPin> connectedPins = new();
        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => Pin.PinConnected += x, x => Pin.PinConnected -= x)
                .Subscribe(e => connectedPins.Add(e.EventArgs.Value))
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => Pin.PinDisconnected += x, x => Pin.PinDisconnected -= x)
                .Subscribe(e => connectedPins.Remove(e.EventArgs.Value))
                .DisposeWith(d);
            Pin.WhenAnyValue(p => p.Type).Subscribe(_ => UpdatePinColor()).DisposeWith(d);
        });

        Connections = connectedPins.Connect().AsObservableList();
        connectedPins.AddRange(Pin.ConnectedTo);
    }

    private void UpdatePinColor()
    {
        TypeColorRegistration registration = _nodeService.GetTypeColorRegistration(Pin.Type);
        PinColor = registration.Color.ToColor();
        DarkenedPinColor = registration.DarkenedColor.ToColor();
    }

    public IObservableList<IPin> Connections { get; }

    public IPin Pin { get; }

    public Color PinColor
    {
        get => _pinColor;
        set => RaiseAndSetIfChanged(ref _pinColor, value);
    }

    public Color DarkenedPinColor
    {
        get => _darkenedPinColor;
        set => RaiseAndSetIfChanged(ref _darkenedPinColor, value);
    }

    public Point Position
    {
        get => _position;
        set => RaiseAndSetIfChanged(ref _position, value);
    }

    public ReactiveCommand<IPin, Unit>? RemovePin
    {
        get => _removePin;
        set => RaiseAndSetIfChanged(ref _removePin, value);
    }

    public bool IsCompatibleWith(PinViewModel pinViewModel)
    {
        if (pinViewModel.Pin.Direction == Pin.Direction || pinViewModel.Pin.Node == Pin.Node)
            return false;

        return Pin.IsTypeCompatible(pinViewModel.Pin.Type);
    }
}