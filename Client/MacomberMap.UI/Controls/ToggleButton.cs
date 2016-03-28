///////////////////////////////////////////////////////////////
// Author: www.binaryconstruct.com
// Date:   10/28/2008
// Name:   ToggleButton.cs
// Description: A button that acts like a checkbox
// License: GPL
///////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace MacomberMap.UI
{
    /// <summary>
    /// A button that has checkbox functionality
    /// </summary>
    [DefaultEvent("Click")]
    [ToolboxItem(true), ToolboxBitmap(typeof(ToggleButton), "MacomberMap.UI.Icons.ToggleButtonIco.png")]
    public class ToggleButton : Button
    {
        #region Events
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Action")]
        [Description("Triggers the CheckState is changed.")]
        public event EventHandler CheckStateChanged;
        protected virtual void OnCheckStateChanged(EventArgs e)
        {
            if (CheckStateChanged != null)
                CheckStateChanged(this, e);

            switch (_CheckState)
            {
                case CheckState.Unchecked:
                    if (this.Checked)
                        this.Checked = false;
                    break;
                case CheckState.Checked:
                    if (!this.Checked)
                        this.Checked = true;
                    break;
                case CheckState.Indeterminate:
                    if (!this.Checked)
                        this.Checked = true;
                    break;
            }

        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Action")]
        [Description("Triggers the CheckState is changed.")]
        public event EventHandler CheckedChanged;
        protected virtual void OnCheckedChanged(EventArgs e)
        {
            if (CheckedChanged != null)
                CheckedChanged(this, e);

            if (_Checked)
            {
                if (this.CheckState != CheckState.Checked)
                    this.CheckState = CheckState.Checked;
            }
            else
            {
                if (this.CheckState != CheckState.Unchecked)
                    this.CheckState = CheckState.Unchecked;
            }
        }

        #endregion

        #region Properties
        private bool _Checked = false;
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        [Description("Gets or Sets the if the ToggleButton is Checked.")]
        [DefaultValue(false)]
        public bool Checked
        {
            get { return _Checked; }
            set
            {
                _Checked = value;
                OnCheckedChanged(new EventArgs());
                SetBG();
                this.Invalidate();
            }
        }

        private CheckState _CheckState = CheckState.Unchecked;
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        [Description("Gets or sets the CheckState of the ToggleButton")]
        [DefaultValue(typeof(CheckState), "Unchecked")]
        public CheckState CheckState
        {
            get { return _CheckState; }
            set
            {
                _CheckState = value;
                OnCheckStateChanged(new EventArgs());
                SetBG();
                this.Invalidate();
            }
        }

        private bool _ThreeState = false;
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Behavior")]
        [Description("Gets or Sets the if the ToggleButton is three state.")]
        [DefaultValue(false)]
        public bool ThreeState
        {
            get { return _ThreeState; }
            set
            {
                _ThreeState = value;
                SetBG();
                this.Invalidate();
            }
        }

        #region Appearance

        private Color _ColorChecked;
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        [Description("Gets or Sets the Checked Color")]
        [DefaultValue(typeof(Color), "Gold")]
        public Color ColorChecked
        {
            get { return _ColorChecked; }
            set { _ColorChecked = value; SetBG(); this.Invalidate(); }
        }
        private Color _ColorIntermediate;
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        [Description("Gets or Sets the Intermediate Color")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color ColorIntermediate
        {
            get { return _ColorIntermediate; }
            set { _ColorIntermediate = value; SetBG(); this.Invalidate(); }
        }

        private Color _BackColor;
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        [Description("Gets or Sets the Unchecked Color")]
        [DefaultValue(typeof(Color), "Transparent")]
        public override Color BackColor
        {
            get { return _BackColor; }
            set { _BackColor = value; SetBG(); this.Invalidate(); }
        }
        #endregion
        #endregion

        public ToggleButton()
        {
            SetBG();
        }


        protected virtual void SetBG()
        {
            this.BackgroundImageLayout = ImageLayout.Stretch;

            Rectangle drawArea = new Rectangle(0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height);

            Bitmap bg = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            Graphics g = Graphics.FromImage(bg);
            g.Clear(Color.Transparent);
            switch (this.CheckState)
            {
                case System.Windows.Forms.CheckState.Checked:
                    g.FillRectangle(new LinearGradientBrush(drawArea, SystemColors.ButtonHighlight, _ColorChecked, 90), drawArea);

                    break;
                case System.Windows.Forms.CheckState.Indeterminate:
                    g.FillRectangle(new LinearGradientBrush(drawArea, SystemColors.ButtonHighlight, _ColorIntermediate, 90), drawArea);
                    break;
                default:
                    g.FillRectangle(new LinearGradientBrush(drawArea, SystemColors.ButtonHighlight, this.BackColor, 90), drawArea);
                    break;
            }
            g.Dispose();

            this.BackgroundImage = bg;

            //if (this.Checked)
            //    this.BackgroundImage = bg;
            //else
            //    this.BackgroundImage = null;

        }

        protected override void OnClick(EventArgs e)
        {
            if (_ThreeState)
            {
                switch (_CheckState)
                {
                    case CheckState.Unchecked:
                        this.CheckState = CheckState.Checked;
                        break;
                    case CheckState.Checked:
                        this.CheckState = CheckState.Indeterminate;
                        break;
                    case CheckState.Indeterminate:
                        this.CheckState = CheckState.Unchecked;
                        break;
                    default:
                        this.CheckState = CheckState.Unchecked;
                        break;
                }
            }
            else
            {
                switch (_CheckState)
                {
                    case CheckState.Unchecked:
                        this.CheckState = CheckState.Checked;
                        break;
                    case CheckState.Checked:
                        this.CheckState = CheckState.Unchecked;
                        break;
                    default:
                        this.CheckState = CheckState.Unchecked;
                        break;
                }
            }

            base.OnClick(e);
        }
    }
}