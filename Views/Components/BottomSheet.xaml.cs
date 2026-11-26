using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace PAGELY.Views.Components;

public partial class BottomSheet : ContentView
{
    public static readonly BindableProperty OptionsProperty = BindableProperty.Create(
        nameof(Options), typeof(ObservableCollection<BottomSheetOption>), typeof(BottomSheet),
        defaultValue: new ObservableCollection<BottomSheetOption>());

    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(BottomSheet), false,
        propertyChanged: OnIsOpenChanged);

    public static readonly BindableProperty SelectCommandProperty = BindableProperty.Create(
        nameof(SelectCommand), typeof(ICommand), typeof(BottomSheet), null);

    public ObservableCollection<BottomSheetOption> Options
    {
        get => (ObservableCollection<BottomSheetOption>)GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public ICommand SelectCommand
    {
        get => (ICommand)GetValue(SelectCommandProperty);
        set => SetValue(SelectCommandProperty, value);
    }

    public double SheetOffset => IsOpen ? 0 : 1000;

    public BottomSheet()
    {
        InitializeComponent();
        Options.CollectionChanged += (_, _) => OnPropertyChanged(nameof(SheetOffset));
    }

    private static void OnIsOpenChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not BottomSheet sheet) return;

        sheet.RootGrid.IsVisible = (bool)newValue || sheet.RootGrid.IsVisible;

        _ = sheet.AnimateSheetAsync((bool)newValue);
    }

    private async System.Threading.Tasks.Task AnimateSheetAsync(bool open)
    {
        var targetY = open ? 0 : 1000;
        var startY = open ? 1000 : 0;

        Sheet.TranslationY = startY;
        Backdrop.Opacity = open ? 0 : 0.4;

        await Task.WhenAll(
            Sheet.TranslateTo(0, targetY, (uint)220, Easing.CubicOut),
            Backdrop.FadeTo(open ? 0.4 : 0, (uint)220, Easing.CubicOut));

        if (!open)
        {
            RootGrid.IsVisible = false;
        }
    }

    private void OnBackdropTapped(object? sender, EventArgs e)
    {
        IsOpen = false;
    }
}
