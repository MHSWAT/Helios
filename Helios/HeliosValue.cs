﻿//  Copyright 2014 Craig Courtney
//  Copyright 2020 Helios Contributors
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using GadrocsWorkshop.Helios.Windows.Controls;

namespace GadrocsWorkshop.Helios
{
    public class HeliosValue : NotificationObject, IBindingAction, IBindingTrigger, IHeliosValue, INamedBindingElement
    {
        private string _device;
        private string _name;

        private readonly WeakReference _owner;
        private WeakReference _context = new WeakReference(null);

        public HeliosValue(HeliosObject owner, BindingValue initialValue, string device, string name,
            string description, string valueDescription, BindingValueUnit unit)
        {
            _device = device;
            _name = name;
            ActionDescription = description;
            ActionValueDescription = valueDescription;
            _owner = new WeakReference(owner);
            Value = initialValue;
            Unit = unit;

            UpdateId();
            RecalculateName();
        }

        public void RecalculateName()
        {
            string ownerName = _owner.IsAlive ? Owner.Name : "";
            ActionBindingDescription = "set " + (Device.Length > 0 ? _device + " " : "") + _name + " on " +
                                       ownerName + " to  %value%";
        }

        private void UpdateId()
        {
            ValueID = "";
            ActionID = "";
            TriggerID = "";
            if (!string.IsNullOrEmpty(_device))
            {
                ValueID += _device + ".";
                ActionID += _device + ".";
                TriggerID += _device + ".";
            }

            TriggerID += _name + ".changed";
            ActionID += "set." + _name;
            ValueID += _name;
        }

        /// <summary>
        /// Fired when a set action is called on this value object.
        /// </summary>
        public event HeliosActionHandler Execute;

        // XXX this is gross... we churn HeliosTriggerEventArgs and HeliosActionEventArgs objects for no reason.  let's have one EventArgs object that represents a value change
        // XXX and then we can reuse it through the action/trigger/action/trigger stack frames and even use it for tracing
        protected void OnFireTrigger(BindingValue value)
        {
            HeliosTriggerEventArgs args = new HeliosTriggerEventArgs(value);
            HeliosTriggerHandler handler = TriggerFired;
            handler?.Invoke(this, args);
        }

        /// <summary>
        /// Sets a new value for this helios value object
        /// </summary>
        /// <param name="value">Value to be sent to bindings.</param>
        /// <param name="bypassCascadingTriggers">True if bindings should not trigger further triggers.</param>
        public void SetValue(BindingValue value, bool bypassCascadingTriggers)
        {
            if ((Value == null && value != null)
                || (Value != null && !Value.Equals(value)))
            {
                Value = value;
                if (!bypassCascadingTriggers)
                {
                    OnFireTrigger(value);
                }
            }
        }

        #region IHeliosValue Members

        public string ValueID { get; private set; }

        public BindingValue Value { get; private set; }

        public string ValueDescription
        {
            get => ActionValueDescription;
            set
            {
                if ((ActionValueDescription == null && value != null)
                    || (ActionValueDescription != null && !ActionValueDescription.Equals(value)))
                {
                    string oldValue = ActionValueDescription;
                    ActionValueDescription = value;
                    OnPropertyChanged("ValueDescription", oldValue, value, false);
                }
            }
        }

        #endregion

        #region IBindingElement Members

        public object Context
        {
            get => _context.Target;
            set => _context = new WeakReference(value);
        }

        public HeliosObject Owner => _owner.Target as HeliosObject;

        public string Device
        {
            get => _device;
            set
            {
                _device = value;
                UpdateId();
            }
        }

        public string Name
        {
            get => _name;

            set
            {
                string oldValue = _name;
                _name = value;
                OnPropertyChanged("Name", oldValue, value, false);
            }
        }

        public BindingValueUnit Unit { get; }

        #endregion

        #region IBindingAction Members

        public string ActionID { get; private set; }

        public string ActionName => "set " + _name;

        public string ActionVerb => "set";

        public HeliosObject Target => _owner.Target as HeliosObject;

        public string ActionDescription { get; }

        public bool ActionRequiresValue => true;

        public string ActionValueDescription { get; private set; }

        /// <summary>
        /// Executes this action.
        /// </summary>
        /// <param name="value">Value to be processed by this action.</param>
        /// <param name="bypassCascadingTriggers">If true this action will not fire additional triggers.</param>
        public void ExecuteAction(BindingValue value, bool bypassCascadingTriggers)
        {
            HeliosActionEventArgs args = new HeliosActionEventArgs(value, bypassCascadingTriggers);
            Execute?.Invoke(this, args);
        }

        public Type ValueEditorType { get; set; } = typeof(TextStaticEditor);

        public string ActionBindingDescription { get; private set; }

        public string ActionInputBindingDescription => "to %value%";

        #endregion

        #region IBindingTrigger Members

        public event HeliosTriggerHandler TriggerFired;

        public string TriggerID { get; private set; }

        public string TriggerName => _name + " changed";

        public string TriggerVerb => "changed";

        public string TriggerDescription => ActionDescription;

        public string TriggerValueDescription => ActionValueDescription;

        public HeliosObject Source => _owner.Target as HeliosObject;

        public bool TriggerSuppliesValue => true;

        public string TriggerBindingDescription => "when" + (Device.Length > 0 ? " " + _device + " " : " ") + _name +
                                                   " on " + Owner.Name + " changes";

        #endregion
    }
}