using System;
using Microsoft.Maui.Controls;

#if ANDROID
using AView = Android.Views.View;
using MotionEventActions = Android.Views.MotionEventActions;
#endif

namespace PAGELY.Behaviors;

public class BlockParentSwipe : Behavior<View>
{
    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.HandlerChanged += OnHandlerChanged;
        if (bindable.Handler is not null)
            AttachPlatform(bindable);
    }

    protected override void OnDetachingFrom(View bindable)
    {
        if (bindable.Handler is not null)
            DetachPlatform(bindable);
        bindable.HandlerChanged -= OnHandlerChanged;
        base.OnDetachingFrom(bindable);
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        if (sender is not View bindable) return;

        if (bindable.Handler is not null)
            AttachPlatform(bindable);
        else
            DetachPlatform(bindable);
    }

#if ANDROID
    private void AttachPlatform(View bindable)
    {
        if (bindable.Handler is null) return;
        var platformView = bindable.Handler.PlatformView as AView;
        if (platformView is not null)
            platformView.Touch += OnTouch;
    }

    private void DetachPlatform(View bindable)
    {
        if (bindable.Handler is null) return;
        var platformView = bindable.Handler.PlatformView as AView;
        if (platformView is not null)
            platformView.Touch -= OnTouch;
    }

    private void OnTouch(object? sender, AView.TouchEventArgs e)
    {
        if (sender is not AView view) return;

        var action = e.Event?.Action;
        var parent = view.Parent;

        if (action is MotionEventActions.Down or MotionEventActions.Move)
            parent?.RequestDisallowInterceptTouchEvent(true);
        else if (action is MotionEventActions.Up or MotionEventActions.Cancel)
            parent?.RequestDisallowInterceptTouchEvent(false);

        e.Handled = false;
    }
#else
    private void AttachPlatform(View bindable) { }

    private void DetachPlatform(View bindable) { }
#endif
}
