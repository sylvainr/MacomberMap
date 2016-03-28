namespace MacomberMapClient.User_Interfaces.Training
{
    partial class MM_Game
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pbTimeLeft = new System.Windows.Forms.ProgressBar();
            this.lblScore = new System.Windows.Forms.Label();
            this.lblAnswer4 = new System.Windows.Forms.Label();
            this.lblAnswer3 = new System.Windows.Forms.Label();
            this.lblAnswer2 = new System.Windows.Forms.Label();
            this.lblAnswer1 = new System.Windows.Forms.Label();
            this.lblQuestion = new System.Windows.Forms.Label();
            this.tmrQuestion = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // pbTimeLeft
            // 
            this.pbTimeLeft.Location = new System.Drawing.Point(53, 16);
            this.pbTimeLeft.Maximum = 30;
            this.pbTimeLeft.Name = "pbTimeLeft";
            this.pbTimeLeft.Size = new System.Drawing.Size(429, 23);
            this.pbTimeLeft.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbTimeLeft.TabIndex = 13;
            this.pbTimeLeft.Value = 30;
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScore.Location = new System.Drawing.Point(514, 16);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(49, 20);
            this.lblScore.TabIndex = 12;
            this.lblScore.Text = "Score:";
            // 
            // lblAnswer4
            // 
            this.lblAnswer4.AutoSize = true;
            this.lblAnswer4.Font = new System.Drawing.Font("Arial Narrow", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnswer4.Location = new System.Drawing.Point(46, 254);
            this.lblAnswer4.Name = "lblAnswer4";
            this.lblAnswer4.Size = new System.Drawing.Size(146, 42);
            this.lblAnswer4.TabIndex = 11;
            this.lblAnswer4.Text = "Answer 4:";
            this.lblAnswer4.MouseLeave += new System.EventHandler(this.Answer_MouseLeave);
            this.lblAnswer4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AnswerQuestion);
            this.lblAnswer4.MouseEnter += new System.EventHandler(this.Answer_MouseEnter);
            // 
            // lblAnswer3
            // 
            this.lblAnswer3.AutoSize = true;
            this.lblAnswer3.Font = new System.Drawing.Font("Arial Narrow", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnswer3.Location = new System.Drawing.Point(46, 198);
            this.lblAnswer3.Name = "lblAnswer3";
            this.lblAnswer3.Size = new System.Drawing.Size(146, 42);
            this.lblAnswer3.TabIndex = 10;
            this.lblAnswer3.Text = "Answer 3:";
            this.lblAnswer3.MouseLeave += new System.EventHandler(this.Answer_MouseLeave);
            this.lblAnswer3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AnswerQuestion);
            this.lblAnswer3.MouseEnter += new System.EventHandler(this.Answer_MouseEnter);
            // 
            // lblAnswer2
            // 
            this.lblAnswer2.AutoSize = true;
            this.lblAnswer2.Font = new System.Drawing.Font("Arial Narrow", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnswer2.Location = new System.Drawing.Point(46, 142);
            this.lblAnswer2.Name = "lblAnswer2";
            this.lblAnswer2.Size = new System.Drawing.Size(146, 42);
            this.lblAnswer2.TabIndex = 9;
            this.lblAnswer2.Text = "Answer 2:";
            this.lblAnswer2.MouseLeave += new System.EventHandler(this.Answer_MouseLeave);
            this.lblAnswer2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AnswerQuestion);
            this.lblAnswer2.MouseEnter += new System.EventHandler(this.Answer_MouseEnter);
            // 
            // lblAnswer1
            // 
            this.lblAnswer1.AutoSize = true;
            this.lblAnswer1.Font = new System.Drawing.Font("Arial Narrow", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnswer1.Location = new System.Drawing.Point(46, 86);
            this.lblAnswer1.Name = "lblAnswer1";
            this.lblAnswer1.Size = new System.Drawing.Size(146, 42);
            this.lblAnswer1.TabIndex = 8;
            this.lblAnswer1.Text = "Answer 1:";
            this.lblAnswer1.MouseLeave += new System.EventHandler(this.Answer_MouseLeave);
            this.lblAnswer1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AnswerQuestion);
            this.lblAnswer1.MouseEnter += new System.EventHandler(this.Answer_MouseEnter);
            // 
            // lblQuestion
            // 
            this.lblQuestion.AutoSize = true;
            this.lblQuestion.Font = new System.Drawing.Font("Arial Narrow", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuestion.Location = new System.Drawing.Point(9, 42);
            this.lblQuestion.Name = "lblQuestion";
            this.lblQuestion.Size = new System.Drawing.Size(140, 42);
            this.lblQuestion.TabIndex = 7;
            this.lblQuestion.Text = "Question:";
            // 
            // tmrQuestion
            // 
            this.tmrQuestion.Interval = 1000;
            this.tmrQuestion.Tick += new System.EventHandler(this.tmrQuestion_Tick);
            // 
            // MM_Game
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(607, 317);
            this.Controls.Add(this.pbTimeLeft);
            this.Controls.Add(this.lblScore);
            this.Controls.Add(this.lblAnswer4);
            this.Controls.Add(this.lblAnswer3);
            this.Controls.Add(this.lblAnswer2);
            this.Controls.Add(this.lblAnswer1);
            this.Controls.Add(this.lblQuestion);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_Game";
            this.Text = "Situation Awareness Game - Macomber Map®";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar pbTimeLeft;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.Label lblAnswer4;
        private System.Windows.Forms.Label lblAnswer3;
        private System.Windows.Forms.Label lblAnswer2;
        private System.Windows.Forms.Label lblAnswer1;
        private System.Windows.Forms.Label lblQuestion;
        private System.Windows.Forms.Timer tmrQuestion;

    }
}