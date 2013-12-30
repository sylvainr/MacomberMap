using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Macomber_Map.Misc
{
    /// <summary>
    /// This class returns a dynamically created multiple-image GIF file.    
    /// The code for creating a GIF color pallete is based on http://support.microsoft.com/kb/319061
    /// The code for creating an animated GIF is based on http://bloggingabout.net/blogs/rick/archive/2005/05/10/3830.aspx
    /// </summary>
    public class AnimatedGifEncoder
    {
        #region Variable declarations
        /// <summary>The output memory stream for our item</summary>
        private MemoryStream mS = new MemoryStream();
                
        /// <summary>The number of seconds between frame incrementing</summary>
        private int FrameRate;

        /// <summary>The total number of frames in the image</summary>
        private int Frames = 0;

        /// <summary>Whether the image has been terminated by a semicolon</summary>
        private bool Terminated = false;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new Gif Encoder
        /// </summary>
        /// <param name="FrameRate">The frame rate of the image</param>
        public AnimatedGifEncoder(int FrameRate)
        {
            this.FrameRate = FrameRate;
        }
        #endregion

        #region Frame modification
        /// <summary>
        /// Add an additional frame to the GIF image
        /// </summary>
        /// <param name="Img">The image to add to the GIF image</param>
        /// <param name="inColors">Colors used in the image</param>
        public void AddFrame(Bitmap Img, Color[] inColors)
        {
            //First, load in our image as a series of bytes in GIF format
            MemoryStream inImg = new MemoryStream();            
            Img.Save(inImg, ImageFormat.Gif);       
     
            //If we're on the first frame, write out our header and application extension
            if (Frames == 0)
            {
                
                mS.Write(inImg.ToArray(), 0, 781);
                mS.Write(new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, 0, 0, 0 }, 0, 19);
            }
            
            

            //Write our image tail (including frame rate), and the actual image data     
            mS.Write(new byte[] { 33, 249, 4, 9 },0, 4);
            mS.Write(BitConverter.GetBytes((UInt16)FrameRate), 0, 2);
            mS.Write(new byte[] { 255, 0}, 0, 2);            
            mS.Write(inImg.ToArray(), 789, (int)inImg.Length - 790);

            //Increment our frames counter
            Frames++;
        }

        

        /// <summary>
        /// Create a new color pallete consisting of the specified colors
        /// </summary>
        /// <param name="inImage">The incoming image</param>
        /// <returns></returns>
        private ColorPalette GetColorPallete(Bitmap inImage)
        {
            //Build our list of colors
            Dictionary<Color, int> ColorTally = new Dictionary<Color, int>();
            for (int x = 0; x < inImage.Width; x++)
                for (int y = 0; y < inImage.Height; y++)
                    if (ColorTally.ContainsKey(inImage.GetPixel(x, y)))
                        ColorTally[inImage.GetPixel(x, y)]++;
                    else
                        ColorTally.Add(inImage.GetPixel(x, y),1);
            
            Bitmap testBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);                                    
            ColorPalette pal = testBmp.Palette;
            int CurPal = 0;
            foreach (Color ThisColor in ColorTally.Keys)
                pal.Entries[CurPal++] = ThisColor;
            for (int a = CurPal; a < 256; a++)
                pal.Entries[a] = Color.FromArgb(0);
        

            testBmp.Dispose();
            return pal;

        }

        /// <summary>
        /// Retrieve the animated GIF.
        /// </summary>
        public Bitmap Image
        {
            get
            {
                //First, if our image isn't terminated, do so.
                if (!Terminated)
                {
                    mS.WriteByte((byte)';');
                    Terminated = true;
                }

                //Now rewind our stream, load and return the GIF
                mS.Position = 0;
                return (Bitmap)Bitmap.FromStream(mS);
            }
        }

      

      

        #endregion
    }
}
