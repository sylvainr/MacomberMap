using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Training;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using System;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    /// <summary>
    /// This class provides a training game layer
    /// </summary>
    public class TrainingGameLayer: RenderLayer
    {
        #region Variable declarations
        /// <summary>Our formats for questions and answers</summary>
        protected TextFormat QuestionFormat, AnswerFormat,KeyIndicatorFormat;
      
        /// <summary>Our brushes for rendering</summary>
        public SolidColorBrush WhiteBrush,LightGreenBrush,RedBrush,QuestionBrush,CorrectBrush,IncorrectBrush,BlackBrush;
        #endregion

        #region Initialization and load/unload
        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        public TrainingGameLayer(Direct3DSurface surface) : base(surface, "Training Layer", 99999)
        {
        }

        /// <summary>
        /// Load our layer content
        /// </summary>
        public override void LoadContent()
        {
            if (WhiteBrush != null)
                WhiteBrush.Dispose();
            WhiteBrush = new SolidColorBrush(Surface.RenderTarget2D, SharpDX.Color.White);

            if (LightGreenBrush != null)
                LightGreenBrush.Dispose();
            LightGreenBrush = new SolidColorBrush(Surface.RenderTarget2D, SharpDX.Color.LightGreen);

            if (RedBrush != null)
                RedBrush.Dispose();
            RedBrush = new SolidColorBrush(Surface.RenderTarget2D, SharpDX.Color.Red);

            if (QuestionBrush != null)
                QuestionBrush.Dispose();
                    QuestionBrush = new SolidColorBrush(Surface.RenderTarget2D,new Color(MM_Repository.Training.QuestionTextColor.R, MM_Repository.Training.QuestionTextColor.G, MM_Repository.Training.QuestionTextColor.B, MM_Repository.Training.QuestionTextColor.A));

            if (CorrectBrush != null)
                CorrectBrush.Dispose();
            CorrectBrush = new SolidColorBrush(Surface.RenderTarget2D, new Color(MM_Repository.Training.CorrectAnswerColor.R, MM_Repository.Training.CorrectAnswerColor.G, MM_Repository.Training.CorrectAnswerColor.B, MM_Repository.Training.CorrectAnswerColor.A));

            if (IncorrectBrush != null)
                IncorrectBrush.Dispose();
            IncorrectBrush = new SolidColorBrush(Surface.RenderTarget2D, new Color(MM_Repository.Training.IncorrectAnswerColor.R, MM_Repository.Training.IncorrectAnswerColor.G, MM_Repository.Training.IncorrectAnswerColor.B, MM_Repository.Training.IncorrectAnswerColor.A));

            if (BlackBrush != null)
                BlackBrush.Dispose();
            BlackBrush = new SolidColorBrush(Surface.RenderTarget2D, SharpDX.Color.Black);


            if (QuestionFormat != null && !QuestionFormat.IsDisposed)
                QuestionFormat.Dispose();
            QuestionFormat = new TextFormat(Surface.FactoryDirectWrite,MM_Repository.Training.QuestionFont.Name, MM_Repository.Training.QuestionFont.Size) { TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Near };

            if (AnswerFormat != null && !AnswerFormat.IsDisposed)
                AnswerFormat.Dispose();
            AnswerFormat = new TextFormat(Surface.FactoryDirectWrite, MM_Repository.Training.AnswerFont.Name, MM_Repository.Training.AnswerFont.Size) { TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Near };


            if (KeyIndicatorFormat != null && !KeyIndicatorFormat.IsDisposed)
                KeyIndicatorFormat.Dispose();
            KeyIndicatorFormat = new TextFormat(Surface.FactoryDirectWrite, MM_Repository.OverallDisplay.KeyIndicatorLabelFont.Name, MM_Repository.OverallDisplay.KeyIndicatorLabelFont.Size) { TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Near };

        }

        /// <summary>
        /// Unload our layer content
        /// </summary>
        public override void UnloadContent()
        {
            if (WhiteBrush != null)
                WhiteBrush.Dispose();
            WhiteBrush = null;

            if (LightGreenBrush != null)
                LightGreenBrush.Dispose();
            LightGreenBrush = null;

            if (RedBrush != null)
                RedBrush.Dispose();
            RedBrush = null;

            if (QuestionBrush != null)
                QuestionBrush.Dispose();
            QuestionBrush = null;

            if (CorrectBrush != null)
                CorrectBrush.Dispose();
            CorrectBrush = null;

            if (IncorrectBrush != null)
                IncorrectBrush.Dispose();
            IncorrectBrush = null;

            if (BlackBrush != null)
                BlackBrush.Dispose();
            BlackBrush = null;

            if (QuestionFormat != null && !QuestionFormat.IsDisposed)
                QuestionFormat.Dispose();
            if (AnswerFormat != null && !AnswerFormat.IsDisposed)
                AnswerFormat.Dispose();
            if (KeyIndicatorFormat != null && !KeyIndicatorFormat.IsDisposed)
                KeyIndicatorFormat.Dispose();
        }
        #endregion


        #region Rendering
        /// <summary>
        /// Render our training layer
        /// </summary>
        /// <param name="renderTime"></param>
        public override void Render(RenderTime renderTime)
        {
            base.Render(renderTime);

            if (MM_Repository.Training == null || MM_Repository.Training.TrainingMode == Data_Elements.Training.MM_Training.enumTrainingMode.NotTraning)
                return;
            else
            {
                MM_Repository.Training.CheckTimes();
                String TextToDraw;
                Brush DrawBrush;

                //Determine our question text and color

                if (MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.UserWon)
                {
                    TextToDraw = "Congratulations! You won!";
                    DrawBrush = LightGreenBrush;
                }
                else if (MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.UserFailed)
                {
                    TextToDraw = "Sorry - please play again!";
                    DrawBrush = RedBrush;
                }
                else
                {
                    TextToDraw = MM_Repository.Training.QuestionText;
                    DrawBrush = QuestionBrush;
                }

                //Now, draw all of our strings
                DrawString(TextToDraw, VerticalAlignment.Top, HorizontalAlignment.Center, DrawBrush, QuestionFormat);
                DrawString("Score: " + MM_Repository.Training.Score.ToString("#,##0"), VerticalAlignment.Top, HorizontalAlignment.Left, DrawBrush, QuestionFormat);
                DrawString("[" + (MM_Repository.Training.CurrentLevel.ID + 1).ToString() + "]: " + MM_Repository.Training.CurrentLevel.Title, VerticalAlignment.Bottom, HorizontalAlignment.Left, DrawBrush, QuestionFormat);

                //If we're in question mode, show our countdown timer
                if (MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.QuestionAsked)
                {
                    double TimeLeft = MM_Repository.Training.CurrentLevel.QuestionTimeout - MM_Repository.Training.TimeSincePresentation;
                    DrawString("Time: " + TimeLeft.ToString("0"), VerticalAlignment.Top, HorizontalAlignment.Right, DrawBrush, QuestionFormat);
                }



                //If we have the incorrect answer, draw accordingly.
                if (MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.AnswerCorrect || MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.AnswerWrong)
                {
                    bool Correct = MM_Repository.Training.TrainingMode == MM_Training.enumTrainingMode.AnswerCorrect;
                    DrawBrush = Correct ? CorrectBrush : IncorrectBrush;
                    DrawString(MM_Repository.Training.AnswerText, VerticalAlignment.Top, HorizontalAlignment.Center, DrawBrush, QuestionFormat, 1, 2);

                    RawVector2 CorrectAnswerXY = MM_Coordinates.LngLatToScreenVector2(MM_Repository.Training.CorrectAnswer, Surface.Coordinates.ZoomLevel);
                    CorrectAnswerXY.X -= Surface.Coordinates.TopLeftXY.X;
                    CorrectAnswerXY.Y -= Surface.Coordinates.TopLeftXY.Y;

                    if (!MM_Repository.Training.UserAnswer.IsEmpty)
                    {
                        RawVector2 UserAnswerXY = MM_Coordinates.LngLatToScreenVector2(MM_Repository.Training.UserAnswer, Surface.Coordinates.ZoomLevel);
                        UserAnswerXY.X -= Surface.Coordinates.TopLeftXY.X;
                        UserAnswerXY.Y -= Surface.Coordinates.TopLeftXY.Y;
                        DrawCrosshairBackground(UserAnswerXY, 8f);
                        Surface.RenderTarget2D.DrawLine(CorrectAnswerXY, UserAnswerXY, BlackBrush, 5f);

                        DrawCrosshairBackground(CorrectAnswerXY, 15f);
                        Surface.RenderTarget2D.DrawLine(CorrectAnswerXY, UserAnswerXY, CorrectBrush,3f);
                        DrawCrosshair(WhiteBrush, UserAnswerXY, 10, "");
                    }

                    if (MM_Repository.Training.TargetElement is MM_Substation)
                    
                        DrawCrosshair(DrawBrush, CorrectAnswerXY, 20, ((MM_Substation)MM_Repository.Training.TargetElement).LongName);
                    else
                        DrawCrosshair(DrawBrush, CorrectAnswerXY, 20, MM_Repository.Training.TargetElement.Name);
                }
            }
        }

        /// <summary>
        /// Draw a black background behind where our crosshair will go
        /// </summary>
        /// <param name="Location"></param>
        /// <param name="Radius"></param>
        private void DrawCrosshairBackground(RawVector2 Location, float Radius)
        {
            Surface.RenderTarget2D.FillEllipse(new Ellipse(Location, Radius + 5f, Radius + 5f), BlackBrush);

        }

        /// <summary>
        /// Draw a crosshair of our target size
        /// </summary>
        /// <param name="DrawBrush"></param>
        /// <param name="Location"></param>
        /// <param name="Radius"></param>
        /// <param name="TextToDraw"></param>
        private void DrawCrosshair(Brush DrawBrush, RawVector2 Location, float Radius,  String TextToDraw)
        {
            //Fill an elipse 5 points bigger to highlight everything
            Surface.RenderTarget2D.FillEllipse(new Ellipse(Location, Radius, Radius), BlackBrush);
            Surface.RenderTarget2D.DrawEllipse(new Ellipse(Location, Radius, Radius), DrawBrush,3f);
            Surface.RenderTarget2D.DrawLine(new RawVector2(Location.X - Radius, Location.Y), new RawVector2(Location.X + Radius, Location.Y), DrawBrush,3f);
            Surface.RenderTarget2D.DrawLine(new RawVector2(Location.X, Location.Y - Radius), new RawVector2(Location.X , Location.Y + Radius), DrawBrush,3f);
            
            //Now, if requested, show the text to draw
            if (!String.IsNullOrEmpty(TextToDraw))
                using (TextLayout t = new TextLayout(Surface.FactoryDirectWrite, TextToDraw, KeyIndicatorFormat, Surface.ClientRectangle.Width, Surface.ClientRectangle.Height))
                {
                    Vector2 TextSize = new Vector2(t.Metrics.Width, t.Metrics.Height);
                    RawVector2 TextLocation = new RawVector2(Location.X  - (TextSize.X / 2f), Location.Y + Radius+5);
                    RawVector2 TextEnd = new RawVector2(TextLocation.X + TextSize.X + 3, TextLocation.Y + TextSize.Y + 3);
                    Surface.RenderTarget2D.FillRectangle(new RawRectangleF(TextLocation.X - 3, TextLocation.Y - 3, TextEnd.X, TextEnd.Y), BlackBrush);
                    Surface.RenderTarget2D.DrawTextLayout(TextLocation, t, DrawBrush);

                }
        }


        /// <summary>
        /// Draw our text on the screen
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="vAlign"></param>
        /// <param name="hAlign"></param>
        /// <param name="DrawBrush"></param>
        /// <param name="Format"></param>
        private void DrawString(string Text, VerticalAlignment vAlign, HorizontalAlignment hAlign, Brush DrawBrush, TextFormat Format, float XMultiplier = 1, float YMultiplier=1)
        {
            using (TextLayout t = new TextLayout(Surface.FactoryDirectWrite, Text, Format, Surface.ClientRectangle.Width, Surface.ClientRectangle.Height))
            {
                Vector2 TextSize = new Vector2(t.Metrics.Width, t.Metrics.Height);
                RawVector2 TextPos = new RawVector2(0, 0);

                if (vAlign == VerticalAlignment.Top)
                    TextPos.Y = TextSize.Y * (YMultiplier - 1);
                else if (vAlign == VerticalAlignment.Center)
                    TextPos.Y = (t.MaxHeight - (TextSize.Y*YMultiplier)) / 2f;
                else if (vAlign == VerticalAlignment.Bottom)
                    TextPos.Y = (t.MaxHeight - (TextSize.Y* YMultiplier));

                if (hAlign == HorizontalAlignment.Left)
                    TextPos.X = TextSize.X * (XMultiplier - 1);
                else if (hAlign == HorizontalAlignment.Center)
                    TextPos.X = (t.MaxWidth - (TextSize.X* XMultiplier)) / 2f;
                else if (hAlign == HorizontalAlignment.Right)
                    TextPos.X = t.MaxWidth - (TextSize.X*XMultiplier);


                Surface.RenderTarget2D.DrawTextLayout(TextPos, t, DrawBrush);
            }
        }
#endregion

    }
}
