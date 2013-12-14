// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KinectSlider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   Slider that responds to Kinect events
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.Controls
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using Microsoft.Kinect.Toolkit.Controls;

    /// <summary>
    /// Slider that responds to Kinect events
    /// </summary>
    [TemplateVisualState(Name = "KinectNormal", GroupName = "KinectCommonStates")]
    [TemplateVisualState(Name = "HandPointerOver", GroupName = "KinectCommonStates")]
    [TemplateVisualState(Name = "KinectDisabled", GroupName = "KinectCommonStates")]
    [TemplateVisualState(Name = "Focused", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Unfocused", GroupName = "FocusStates")]
    [TemplatePart(Name = "Thumb", Type = typeof(KinectThumb))]
    public class KinectSlider : Slider
    {
        /// <summary>
        /// The grip event target property.  Apps can use this to make the area where we listen
        /// for Kinect events larger.
        /// </summary>
        public static readonly DependencyProperty GripEventTargetProperty = DependencyProperty.Register(
            "GripEventTarget", typeof(UIElement), typeof(KinectSlider), new FrameworkPropertyMetadata(null, (o, args) => ((KinectSlider)o).OnGripEventTargetChanged()));

        /// <summary>
        /// Set this to false to show the Kinect cursor when a drag operation is in progress
        /// </summary>
        public static readonly DependencyProperty HideKinectCursorWhenDraggingProperty = DependencyProperty.Register(
            "HideKinectCursorWhenDragging", typeof(bool), typeof(KinectSlider), new PropertyMetadata(true));

        /// <summary>
        /// Bound to the attached KinectRegion.IsPrimaryHandPointerOverProperty.
        /// We do this to monitor changes to the actual property.
        /// </summary>
        private static readonly DependencyProperty IsPrimaryHandPointerOverProperty = DependencyProperty.Register(
            "IsPrimaryHandPointerOver",
            typeof(bool),
            typeof(KinectSlider),
            new PropertyMetadata(false, (o, args) => ((KinectSlider)o).ChangeVisualState(true)));

        /// <summary>
        /// Initializes static members of the <see cref="KinectSlider" /> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Need to OverrideMetadata in the static constructor")]
        static KinectSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KinectSlider), new FrameworkPropertyMetadata(typeof(KinectSlider)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectSlider"/> class. 
        /// </summary>
        public KinectSlider()
        {
            this.Loaded += (o, args) => this.ChangeVisualState(false);
            this.IsEnabledChanged += (o, args) => this.ChangeVisualState(false);
            this.MouseEnter += (o, args) => this.ChangeVisualState(true);
            this.MouseLeave += (o, args) => this.ChangeVisualState(true);

            var binding = new Binding { Source = this, Path = new PropertyPath(KinectRegion.IsPrimaryHandPointerOverProperty) };
            BindingOperations.SetBinding(this, IsPrimaryHandPointerOverProperty, binding);
        }

        /// <summary>
        /// Retrieve the value of IsPrimaryHandPointerOverProperty
        /// </summary>
        public bool IsPrimaryHandPointerOver
        {
            get
            {
                return (bool)this.GetValue(IsPrimaryHandPointerOverProperty);
            }
        }

        /// <summary>
        /// Set this to true to hide the Kinect Cursor when the user is dragging the slider
        /// </summary>
        public bool HideKinectCursorWhenDragging
        {
            get
            {
                return (bool)this.GetValue(HideKinectCursorWhenDraggingProperty);
            }

            set
            {
                this.SetValue(HideKinectCursorWhenDraggingProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the grip event target.
        /// </summary>
        public UIElement GripEventTarget
        {
            get
            {
                return (UIElement)this.GetValue(GripEventTargetProperty);
            }

            set
            {
                this.SetValue(GripEventTargetProperty, value);
            }
        }

        /// <summary>
        /// Called when the GripEventTarget property changes
        /// </summary>
        private void OnGripEventTargetChanged()
        {
            var track = this.GetTemplateChild("PART_Track") as Track;
            if (track != null)
            {
                var thumb = track.Thumb as KinectThumb;

                if (thumb != null)
                {
                    thumb.ListenForGrip();
                }
            }
        }

        /// <summary>
        /// Updates the visual state of the control
        /// </summary>
        /// <param name="useTransitions">true to use a System.Windows.VisualTransition object to transition between states; otherwise, false.</param>
        private void ChangeVisualState(bool useTransitions)
        {
            string newStateName = "KinectNormal";

            if (!this.IsEnabled)
            {
                newStateName = "KinectDisabled";
            }
            else if (this.IsPrimaryHandPointerOver || this.IsMouseOver)
            {
                newStateName = "HandPointerOver";
            }

            VisualStateManager.GoToState(this, newStateName, useTransitions);
        }
    }
}