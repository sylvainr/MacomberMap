using MacomberMapClient.Data_Elements.Physical;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Generic
{
    /// <summary>
    /// (C) 2015, Michael E. legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rightws Reserved.
    /// This class provides a data-bound property linked to a DataGridView
    /// </summary>
    public class MM_Bound_Element : INotifyPropertyChanged
    {
        #region Variable declarations
        /// <summary>Our element on which we're based</summary>
        public MM_Element BaseElement;

        /// <summary>The control that owns this element</summary>
        private Control OwningControl;

        /// <summary>Our event handler for property changes</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new bound element
        /// </summary>
        /// <param name="BaseElement"></param>
        /// <param name="OwningControl"></param>
        public MM_Bound_Element(MM_Element BaseElement, Control OwningControl)
        {
            this.OwningControl = OwningControl;
            this.BaseElement = BaseElement;
            //this.BaseElement.ValuesChanged += BaseElement_ValuesChanged;
        }

        /// <summary>
        /// Handle the destruction of the bound element
        /// </summary>
        ~MM_Bound_Element()
        {
            if (this.BaseElement != null)
                this.BaseElement.ValuesChanged -= BaseElement_ValuesChanged;
        }

        /// <summary>
        /// Handle a value change of our element
        /// </summary>
        /// <param name="Element"></param>
        /// <param name="Property"></param>
        private void BaseElement_ValuesChanged(MM_Element Element, String Property)
        {           
            if (PropertyChanged != null && OwningControl != null && OwningControl.Visible)
                if (OwningControl.InvokeRequired)
                    OwningControl.BeginInvoke(new MM_Element.ValuesChangedDelegate(BaseElement_ValuesChanged), Element);
                else
                    PropertyChanged(this, new PropertyChangedEventArgs(Property));
        }
        #endregion
    }
}