﻿using System;
using System.ComponentModel;
using System.Drawing;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xfx;
using Xfx.Controls.iOS.Controls;
using Xfx.Controls.iOS.Extensions;
using Xfx.Controls.iOS.Renderers;

[assembly: ExportRenderer(typeof(XfxEntry), typeof(XfxEntryRendererTouch))]

namespace Xfx.Controls.iOS.Renderers
{
    public class XfxEntryRendererTouch : ViewRenderer<XfxEntry, FloatLabeledTextField>
    {
        private readonly CGColor _defaultLineColor = Color.FromHex("#666666").ToCGColor();
        private readonly CGColor _editingUnderlineColor = UIColor.Blue.CGColor;
        private UIColor _defaultPlaceholderColor;
        private UIColor _defaultTextColor;
        private bool _hasError;
        private bool _hasFocus;

        private IElementController ElementController => Element as IElementController;
        
        protected override void OnElementChanged(ElementChangedEventArgs<XfxEntry> e)
        {
            base.OnElementChanged(e);
            
            // unsubscribe
            if (e.OldElement != null)
            {
                Control.EditingDidBegin -= OnEditingDidBegin;
                Control.EditingDidEnd -= OnEditingDidEnd;
                Control.EditingChanged -= ViewOnEditingChanged;
            }
            
            if (e.NewElement != null)
            {
                var ctrl = CreateNativeControl();
                SetNativeControl(ctrl);

                if (!string.IsNullOrWhiteSpace(Element.AutomationId))
                    SetAutomationId(Element.AutomationId);

                _defaultTextColor = Control.FloatingLabelTextColor;
                _defaultPlaceholderColor = Control.FloatingLabelTextColor;

                SetIsPassword();
                SetText();
                SetHintText();
                SetTextColor();
                SetBackgroundColor();
                SetPlaceholderColor();
                SetKeyboard();
                SetHorizontalTextAlignment();
                SetErrorText();
                SetFont();
                SetFloatingHintEnabled();

                Control.ErrorTextIsVisible = true;
                Control.EditingDidBegin += OnEditingDidBegin;
                Control.EditingDidEnd += OnEditingDidEnd;
                Control.EditingChanged += ViewOnEditingChanged;
            }
        }

        protected virtual FloatLabeledTextField CreateNativeControl()
        {
            return new FloatLabeledTextField();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Entry.PlaceholderProperty.PropertyName)
                SetHintText();
            else if (e.PropertyName == XfxEntry.ErrorTextProperty.PropertyName)
                SetErrorText();
            else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
                SetTextColor();
            else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
                SetBackgroundColor();
            else if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
                SetIsPassword();
            else if (e.PropertyName == Entry.TextProperty.PropertyName)
                SetText();
            else if (e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
                SetPlaceholderColor();
            else if (e.PropertyName == Xamarin.Forms.InputView.KeyboardProperty.PropertyName)
                SetKeyboard();
            else if (e.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName)
                SetHorizontalTextAlignment();
            else if (e.PropertyName == XfxEntry.FloatingHintEnabledProperty.PropertyName)
                SetFloatingHintEnabled();
            else if ((e.PropertyName == Entry.FontAttributesProperty.PropertyName) ||
                     (e.PropertyName == Entry.FontFamilyProperty.PropertyName) ||
                     (e.PropertyName == Entry.FontSizeProperty.PropertyName))
                SetFont();
        }

        private void OnEditingDidEnd(object sender, EventArgs eventArgs)
        {
            // TODO : Florell, Chase (Contractor) 02/21/17 unfocus
            _hasFocus = false;
            Control.UnderlineColor = GetUnderlineColorForState();
            if (Element is XfxEntry entry)
            {
                entry.RaiseRendererFocusChanged(_hasFocus);
            }
        }

        private void OnEditingDidBegin(object sender, EventArgs eventArgs)
        {
            // TODO : Florell, Chase (Contractor) 02/21/17 focus
            _hasFocus = true;
            Control.UnderlineColor = GetUnderlineColorForState();
            if (Element is XfxEntry entry)
            {
                entry.RaiseRendererFocusChanged(_hasFocus);
            }
        }

        private void ViewOnEditingChanged(object sender, EventArgs eventArgs)
        {
            ElementController?.SetValueFromRenderer(Entry.TextProperty, Control.Text);
        }

        private void SetFloatingHintEnabled()
        {
            Control.FloatingLabelEnabled = Element.FloatingHintEnabled;
        }

        private void SetErrorText()
        {
            _hasError = !string.IsNullOrEmpty(Element.ErrorText);
            Control.UnderlineColor = GetUnderlineColorForState();
            Control.ErrorTextIsVisible = _hasError;
            Control.ErrorText = Element.ErrorText;
        }

        private CGColor GetUnderlineColorForState()
        {
            if (_hasError) return UIColor.Red.CGColor;
            return _hasFocus ? _editingUnderlineColor : _defaultLineColor;
        }

        private void SetBackgroundColor()
        {
            NativeView.BackgroundColor = Element.BackgroundColor.ToUIColor();
            Control.UnderlineColor = _defaultLineColor;
        }

        private void SetText()
        {
            if (Control.Text != Element.Text)
                Control.Text = Element.Text;
        }

        private void SetIsPassword()
        {
            Control.SecureTextEntry = Element.IsPassword;
        }

        private void SetHintText()
        {
            Control.Placeholder = Element.Placeholder;
        }

        private void SetPlaceholderColor()
        {
            Control.FloatingLabelTextColor = Element.PlaceholderColor == Color.Default
                ? _defaultPlaceholderColor
                : Element.PlaceholderColor.ToUIColor();
        }

        private void SetTextColor()
        {
            Control.TextColor = Element.TextColor == Color.Default
                ? _defaultTextColor
                : Element.TextColor.ToUIColor();
        }

        private void SetFont()
        {
            Control.Font = Element.ToUIFont();
        }

        private void SetHorizontalTextAlignment()
        {
            switch (Element.HorizontalTextAlignment)
            {
                case TextAlignment.Center:
                    Control.TextAlignment = UITextAlignment.Center;
                    break;
                case TextAlignment.End:
                    Control.TextAlignment = UITextAlignment.Right;
                    break;
                default:
                    Control.TextAlignment = UITextAlignment.Left;
                    break;
            }
        }

        private void SetKeyboard()
        {
            var kbd = Element.Keyboard.ToNative();
            Control.KeyboardType = kbd;
            Control.InputAccessoryView = kbd == UIKeyboardType.NumberPad ? NumberpadAccessoryView() : null;
            Control.ShouldReturn = InvokeCompleted;
        }

        private UIToolbar NumberpadAccessoryView()
        {
            return new UIToolbar(new RectangleF(0.0f, 0.0f, (float) Control.Frame.Size.Width, 44.0f))
            {
                Items = new[]
                {
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate { InvokeCompleted(null); })
                }
            };
        }

        private bool InvokeCompleted(UITextField textField)
        {
            Control.ResignFirstResponder();
            ((IEntryController) Element).SendCompleted();
            return true;
        }
    }
}