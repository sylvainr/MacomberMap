using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;
using Macomber_Map.Data_Connections;
using System.Drawing;
using System.Threading;
using Macomber_Map.User_Interface_Components.GDIPlus;

namespace Macomber_Map.User_Interface_Components.NetworkMap
{
    /// <summary>
    /// This class holds the abstract class for the network map.
    /// </summary>
    public abstract class Network_Map : UserControl
    {
        #region Variable declarations
        /// <summary>The current view on the network map</summary>
        public MM_Coordinates Coordinates;

   
        /// <summary>FPS Count (for debugging)</summary>
        internal int FPS = 0;

        /// <summary>The x/y position of the left mouse press (for panning)</summary>
        internal Point MousePos = Point.Empty;

        /// <summary>The menu handling right-click actions</summary>
        internal MM_Popup_Menu RightClickMenu = new MM_Popup_Menu();

        /// <summary>Wheter or not the map has been moved, and elements need to be recalculated</summary>
        public bool MapMoved = false;

        /// <summary>Whether or not an ongoing zoom/pan action is occurring, requiring constant redraws</summary>
        public bool OngoingZoomPan;

        /// <summary>The timer that handles blinking events</summary>
        internal System.Timers.Timer BlinkTimer = new System.Timers.Timer(400);

        /// <summary>Whether or not the worst state should be shown (for handling blinking events)</summary>
        internal bool IsBlinking = false;

        /// <summary>The timer that handles MW/MVAR flows</summary>
        internal System.Timers.Timer FlowTimer = new System.Timers.Timer(100);
       
        /// <summary>The mini-map associated with the network map (for zoom/pan activity)</summary>
        public Mini_Map miniMap = null;
       
        /// <summary>Whether the map is in the midst of rendering</summary>
        internal bool IsRendering = false;

        /// <summary>Whether the control is resizing</summary>
        public bool Resizing = false;

        /// <summary>FPS at last second interval</summary>
        public int LastFPS = 0;

        /// <summary>The number of pixels to shift the display every 100 ms</summary>
        internal Point PanPoint = Point.Empty;

        /// <summary>Whether the user is holding the shift key down while drawing a lasso</summary>
        internal bool ShiftDown = false;

        /// <summary>The violation viewer associated with the network map</summary>
        internal Violation_Viewer violViewer;

        /// <summary>Debug timer, if requested</summary>
        internal System.Timers.Timer tmrFPS;

        /// <summary>
        /// Initialize a new network map
        /// </summary>
        /// <param name="Coordinates"></param>
        public Network_Map(MM_Coordinates Coordinates)
        {
            this.Coordinates = Coordinates;
        }

        /// <summary>
        /// Return the current mouse position upon request
        /// </summary>
        public PointF MouseLatLng
        {
            get
            {
                if (RectangleToScreen(ClientRectangle).Contains(Cursor.Position))
                    return Coordinates.UnconvertPoint(PointToClient(Cursor.Position));
                else
                    return new PointF(float.NaN, float.NaN);
            }
        }

        /// <summary>
        /// Create a new network map control
        /// </summary>
        /// <param name="Coordinates">The coordinates to be used</param>
        /// <returns></returns>
        public static Network_Map CreateMap(MM_Coordinates Coordinates)
        {
            if (MM_Repository.RenderMode == MM_Repository.enumRenderMode.DirectX)
                throw new InvalidOperationException("DirectX UI has been removed!");     
            else if (MM_Repository.RenderMode == MM_Repository.enumRenderMode.GDIPlus)
                return new Network_Map_GDI(Coordinates);
            else
                throw new InvalidOperationException("WPF Network map is not yet implemented!");            
        }
        #endregion

        #region Network Map functions
        /// <summary>
        /// Reset the display coordinates
        /// </summary>
        public abstract void ResetDisplayCoordinates();

        /// <summary>
        /// Set the map's display coordinates to fit particular top/left and bottom/right components
        /// </summary>
        /// <param name="TopLeft"></param>
        /// <param name="BottomRight"></param>
        public abstract void SetDisplayCoordinates(PointF TopLeft, PointF BottomRight);

        /// <summary>
        /// Recompute critical information about a particular substation.
        /// </summary>
        public abstract void UpdateSubstationInformation();

        /// <summary>
        /// Recompute critical information about a particular substation.
        /// </summary>
        /// <param name="Substation">The substation to be recomputed</param>
        public abstract void UpdateSubstationInformation(MM_Substation Substation);


        /// <summary>
        /// Recompute critical information about a particular line.
        /// </summary>
        public abstract void UpdateLineInformation();
        
        
        /// <summary>
        /// Recompute critical information about a particular line.
        /// </summary>
        /// <param name="Line">The line to be recomputed</param>
        /// <param name="AllowAdd">Whether lines should be added</param>
        public abstract void UpdateLineInformation(MM_Line Line, bool AllowAdd);

        /// <summary>Reset our network map</summary>
        public abstract void ResetMap();

                /// <summary>
        /// Set the controls and data sources associated with the network map
        /// </summary>
        /// <param name="violViewer">The violation viewer</param>
        /// <param name="miniMap">The mini-map to be associated with the network map</param>
        /// <param name="resetZoom">Whether to reset the zoom levels</param>
        public abstract void SetControls(Violation_Viewer violViewer, Mini_Map miniMap, int resetZoom);

        /// <summary>
        /// Determine whether an element is visible on the network map
        /// </summary>
        /// <param name="Element">The substation or line to test</param>
        /// <returns></returns>
        public abstract bool IsVisible(MM_Element Element);

        /// <summary>
        /// Determine whether an element's connector is visible on the network map
        /// </summary>
        /// <param name="Element">The substation or line to test</param>
        /// <returns></returns>
        public abstract bool IsConnectorVisible(MM_Element Element);

        /// <summary>
        /// Report the completion of a resize event
        /// </summary>
        public abstract void ResizeComplete();

        /// <summary>
        /// Handle the elapsing of the FPS timer by resetting and doing background events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void tmrFPS_Elapsed(object sender, EventArgs e)
        {
            LastFPS = FPS;
            FPS = 0;
            Application.DoEvents();
        }
        #endregion
    }
}
