// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KinectThumb.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   A Thumb control that responds to Kinect events
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.Controls
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using Microsoft.Kinect.Toolkit.Controls;

    /// <summary>
    /// A Thumb class that responds to Kinect events
    /// </summary>
    [TemplateVisualState(Name = "KinectDisabled", GroupName = "KinectCommonStates")]
    [TemplateVisualState(Name = "KinectPressed", GroupName = "KinectCommonStates")]
    [TemplateVisualState(Name = "HandPointerOver", GroupName = "KinectCommonStates")]
    [TemplateVisualState(Name = "KinectNormal", GroupName = "KinectCommonStates")]
    public sealed class KinectThumb : Thumb
    {
        /// <summary>
        /// Set to true when we are in design mode
        /// </summary>
        private static readonly bool IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());

        /// <summary>
        /// Bound to the attached KinectRegion.IsPrimaryHandPointerOverProperty.
        /// We do this to monitor changes to the actual property.
        /// </summary>
        private static readonly DependencyProperty IsPrimaryHandPointerOverProperty = DependencyProperty.Register(
            "IsPrimaryHandPointerOver", 
            typeof(bool),
            typeof(KinectThumb),
            new PropertyMetadata(false, (o, args) => ((KinectThumb)o).ChangeVisualState(true)));

        /// <summary>
        /// The captured hand pointer.  This should only be set when we are
        /// being dragged.
        /// </summary>
        private HandPointer capturedHandPointer;

        /// <summary>
        /// The element we actually listen for grip initiation on
        /// </summary>
        private UIElement gripEventTarget;

        /// <summary>
        /// Slider that this thumb is in
        /// </summary>
        private KinectSlider sliderParent;

        /// <summary>
        /// Initializes static members of the <see cref="KinectThumb"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", 
            Justification = "We need to OverrideMetadata in the static constructor")]
        static KinectThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KinectThumb), new FrameworkPropertyMetadata(typeof(Thumb)));
        }

        /// <summary>
        /// Initializes a new instance of the KinectThumb class.
        /// </summary>
        public KinectThumb()
        {
            this.ClipToBounds = true;

            if (!IsInDesignMode)
            {
                this.InitializeKinectThumb();
            }
        }

        /// <summary>
        /// Called to update where we listen for grip.
        /// </summary>
        internal void ListenForGrip()
        {
            if (this.gripEventTarget != null)
            {
                KinectRegion.RemoveHandPointerGripHandler(this.gripEventTarget, this.OnHandPointerGrip);
                KinectRegion.SetIsGripTarget(this, false);

                BindingOperations.ClearBinding(this, IsPrimaryHandPointerOverProperty);

                this.gripEventTarget.IsEnabledChanged -= this.OnStateChangeEvent;
                this.gripEventTarget.MouseEnter -= this.OnMouseChange;
                this.gripEventTarget.MouseLeave -= this.OnMouseChange;

                this.gripEventTarget = null;
            }

            this.sliderParent = FindAncestor<KinectSlider>(this);

            if (this.sliderParent == null)
            {
                this.gripEventTarget = this;
            }
            else if (this.sliderParent.GripEventTarget != null)
            {
                this.gripEventTarget = this.sliderParent.GripEventTarget;
            }
            else
            {
                this.gripEventTarget = this.sliderParent;
            }

            var binding = new Binding { Source = this.gripEventTarget, Path = new PropertyPath(KinectRegion.IsPrimaryHandPointerOverProperty) };
            BindingOperations.SetBinding(this, IsPrimaryHandPointerOverProperty, binding);

            this.gripEventTarget.IsEnabledChanged += this.OnStateChangeEvent;
            this.gripEventTarget.MouseEnter += this.OnMouseChange;
            this.gripEventTarget.MouseLeave += this.OnMouseChange;

            KinectRegion.AddHandPointerGripHandler(this.gripEventTarget, this.OnHandPointerGrip);
            KinectRegion.SetIsGripTarget(this.gripEventTarget, true);
        }

        /// <summary>
        /// Override called when IsDragging changes (either through mouse or Kinect)
        /// </summary>
        /// <param name="args">event args</param>
        protected override void OnDraggingChanged(DependencyPropertyChangedEventArgs args)
        {
            this.ChangeVisualState(true);
            base.OnDraggingChanged(args);
        }

        /// <summary>
        /// The on visual parent changed.
        /// </summary>
        /// <param name="oldParent">
        /// The old parent.
        /// </param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            this.ListenForGrip();
            base.OnVisualParentChanged(oldParent);
        }

        /// <summary>
        /// Finds the ancestor of type T
        /// </summary>
        /// <typeparam name="T">Type of ancestor</typeparam>
        /// <param name="dependencyObject">object to start the search on</param>
        /// <returns>Found ancestor or null</returns>
        private static T FindAncestor<T>(DependencyObject dependencyObject)
            where T : class
        {
            DependencyObject scan = dependencyObject;
            T ancestor;
            do
            {
                scan = VisualTreeHelper.GetParent(scan);
                ancestor = scan as T;
            }
            while (scan != null && ancestor == null);
            return ancestor;
        }
        
        /// <summary>
        /// Called when the mouse enters or leaves the grip event target.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">Event args</param>
        private void OnMouseChange(object sender, MouseEventArgs args)
        {
            this.ChangeVisualState(true);
        }

        /// <summary>
        /// Called when some state changes that will effect the visual state.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">Event args</param>
        private void OnStateChangeEvent(object sender, DependencyPropertyChangedEventArgs args)
        {
            this.ChangeVisualState(true);
        }

        /// <summary>
        /// Called to initialize the control when we are not in design mode.
        /// </summary>
        private void InitializeKinectThumb()
        {
            KinectRegion.AddHandPointerGotCaptureHandler(this, this.OnHandPointerCaptured);
            KinectRegion.AddQueryInteractionStatusHandler(this, this.OnQueryInteractionStatus);
            KinectRegion.AddHandPointerMoveHandler(this, this.OnHandPointerMove);
            KinectRegion.AddHandPointerGripReleaseHandler(this, this.OnHandPointerGripRelease);
            KinectRegion.AddHandPointerLostCaptureHandler(this, this.OnHandPointerLostCapture);

            KinectRegion.SetIsGripTarget(this, true);
        }

        /// <summary>
        /// Called when the user grips their hand.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="kinectHandPointerEventArgs">The kinect hand pointer event arguments.</param>
        private void OnHandPointerGrip(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            var handPointer = kinectHandPointerEventArgs.HandPointer;

            if (!(handPointer.IsPrimaryUser && handPointer.IsPrimaryHandOfUser && handPointer.IsInteractive))
            {
                return;
            }

            if (handPointer.HandEventType == HandEventType.Grip)
            {
                if (this.capturedHandPointer != null)
                {
                    // We already have a hand pointer captured.  Ignore this one.
                    return;
                }

                Debug.Assert(handPointer.Captured == null, "Shouldn't ever get a grip event from a hand pointer captured by someone else.");

                handPointer.Capture(this);

                kinectHandPointerEventArgs.Handled = true;
            }
        }

        /// <summary>
        /// Capture event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="kinectHandPointerEventArgs">The kinect hand pointer event arguments.</param>
        private void OnHandPointerCaptured(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            // We should only get a capture if we don't already have a hand pointer captured.
            if (this.capturedHandPointer != null)
            {
                return;
            }

            this.capturedHandPointer = kinectHandPointerEventArgs.HandPointer;
            this.IsDragging = true;

            var delta = this.GetDragDelta(kinectHandPointerEventArgs.HandPointer);
            var e = new DragStartedEventArgs(delta.X, delta.Y);
            this.RaiseEvent(e);

            this.ChangeVisualState(true);

            kinectHandPointerEventArgs.Handled = true;
        }

        /// <summary>
        /// Called when the user releases their grip
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="kinectHandPointerEventArgs">The kinect hand pointer event arguments.</param>
        private void OnHandPointerGripRelease(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (this.capturedHandPointer == kinectHandPointerEventArgs.HandPointer)
            {
                kinectHandPointerEventArgs.HandPointer.Capture(null);
                kinectHandPointerEventArgs.Handled = true;
            }
        }

        /// <summary>
        /// Lost capture event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="kinectHandPointerEventArgs">The kinect hand pointer event arguments.</param>
        private void OnHandPointerLostCapture(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (this.capturedHandPointer == kinectHandPointerEventArgs.HandPointer)
            {
                this.capturedHandPointer = null;

                this.IsDragging = false;

                var delta = this.GetDragDelta(kinectHandPointerEventArgs.HandPointer);
                var e = new DragCompletedEventArgs(delta.X, delta.Y, false);
                this.RaiseEvent(e);

                this.ChangeVisualState(true);

                kinectHandPointerEventArgs.Handled = true;
            }
        }

        /// <summary>
        /// Hand Pointer Move event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="kinectHandPointerEventArgs">The kinect hand pointer event arguments.</param>
        private void OnHandPointerMove(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (this.capturedHandPointer == kinectHandPointerEventArgs.HandPointer)
            {
                kinectHandPointerEventArgs.Handled = true;

                var delta = this.GetDragDelta(kinectHandPointerEventArgs.HandPointer);
                var e = new DragDeltaEventArgs(delta.X, delta.Y);
                this.RaiseEvent(e);

                kinectHandPointerEventArgs.Handled = true;
            }
        }

        /// <summary>
        /// Sets the state of the VSM depending on the state of the UI
        /// </summary>
        /// <param name="useTransitions">true to use a System.Windows.VisualTransition object to transition between states; otherwise, false.</param>
        private void ChangeVisualState(bool useTransitions)
        {
            string newStateName = "KinectNormal";
            bool showKinectCursor = true;

            if (!this.IsEnabled)
            {
                newStateName = "KinectDisabled";
            }
            else if (this.IsDragging)
            {
                newStateName = "KinectPressed";
                showKinectCursor = false;
            }
            else if (KinectRegion.GetIsPrimaryHandPointerOver(this.gripEventTarget) || this.gripEventTarget.IsMouseOver)
            {
                newStateName = "HandPointerOver";
            }

            var kinectRegion = KinectRegion.GetKinectRegion(this);
            if (this.sliderParent != null && kinectRegion != null)
            {
                if (!showKinectCursor && !this.sliderParent.HideKinectCursorWhenDragging)
                {
                    showKinectCursor = true;
                }

                kinectRegion.IsCursorVisible = showKinectCursor;
            }   
            
            VisualStateManager.GoToState(this, newStateName, useTransitions);
        }

        /// <summary>
        /// Tells the KinectRegion what state the control is in.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="queryInteractionStatusEventArgs">The query interaction status event arguments.</param>
        private void OnQueryInteractionStatus(object sender, QueryInteractionStatusEventArgs queryInteractionStatusEventArgs)
        {
            if (this.capturedHandPointer == queryInteractionStatusEventArgs.HandPointer)
            {
                queryInteractionStatusEventArgs.IsInGripInteraction = true;
                queryInteractionStatusEventArgs.Handled = true;
            }
        }

        /// <summary>
        /// Helper to get how far the thumb needs to move to be at the
        /// hand pointer position.
        /// </summary>
        /// <param name="kinectHandPointer">Hand pointer to check</param>
        /// <returns>How far the thumb control needs to move</returns>
        private Vector GetDragDelta(HandPointer kinectHandPointer)
        {
            var handPointerRelativeToGripEventTarget = kinectHandPointer.GetPosition(this.gripEventTarget);
            var thumbRelativeToGripEventTarget = this.TranslatePoint(new Point(this.ActualWidth / 2.0, this.ActualHeight / 2.0), this.gripEventTarget);
            return handPointerRelativeToGripEventTarget - thumbRelativeToGripEventTarget;
        }
    }
}