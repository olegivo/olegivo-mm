namespace WinampProxyNS {
  partial class WinampUserControl {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      this.label25 = new System.Windows.Forms.Label();
      this.txtTrackName = new System.Windows.Forms.TextBox();
      this.tbVolume = new System.Windows.Forms.TrackBar();
      this.label7 = new System.Windows.Forms.Label();
      this.btnForward = new System.Windows.Forms.Button();
      this.btnJumpForward = new System.Windows.Forms.Button();
      this.btnPlayPause = new System.Windows.Forms.Button();
      this.btnJumpBack = new System.Windows.Forms.Button();
      this.btnBack = new System.Windows.Forms.Button();
      this.pbProgress = new System.Windows.Forms.ProgressBar();
      this.btnEnqueue = new System.Windows.Forms.Button();
      this.timerUpdate = new System.Windows.Forms.Timer(this.components);
      ((System.ComponentModel.ISupportInitialize) (this.tbVolume)).BeginInit();
      this.SuspendLayout();
      // 
      // label25
      // 
      this.label25.AutoSize = true;
      this.label25.Location = new System.Drawing.Point(1, 6);
      this.label25.Name = "label25";
      this.label25.Size = new System.Drawing.Size(38, 13);
      this.label25.TabIndex = 0;
      this.label25.Text = "Track:";
      // 
      // txtTrackName
      // 
      this.txtTrackName.Location = new System.Drawing.Point(48, 3);
      this.txtTrackName.Name = "txtTrackName";
      this.txtTrackName.ReadOnly = true;
      this.txtTrackName.Size = new System.Drawing.Size(165, 20);
      this.txtTrackName.TabIndex = 1;
      // 
      // tbVolume
      // 
      this.tbVolume.Location = new System.Drawing.Point(41, 68);
      this.tbVolume.Maximum = 255;
      this.tbVolume.Name = "tbVolume";
      this.tbVolume.Size = new System.Drawing.Size(179, 45);
      this.tbVolume.TabIndex = 10;
      this.tbVolume.TickFrequency = 10;
      this.tbVolume.Value = 128;
      this.tbVolume.Scroll += new System.EventHandler(this.tbVolume_Scroll);
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(1, 76);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(45, 13);
      this.label7.TabIndex = 9;
      this.label7.Text = "Volume:";
      // 
      // btnForward
      // 
      this.btnForward.Font = new System.Drawing.Font("Webdings", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (2)));
      this.btnForward.Location = new System.Drawing.Point(187, 46);
      this.btnForward.Name = "btnForward";
      this.btnForward.Size = new System.Drawing.Size(27, 23);
      this.btnForward.TabIndex = 8;
      this.btnForward.Text = "";
      this.btnForward.UseVisualStyleBackColor = true;
      this.btnForward.Click += new System.EventHandler(this.btnForward_Click);
      // 
      // btnJumpForward
      // 
      this.btnJumpForward.Font = new System.Drawing.Font("Webdings", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (2)));
      this.btnJumpForward.Location = new System.Drawing.Point(159, 46);
      this.btnJumpForward.Name = "btnJumpForward";
      this.btnJumpForward.Size = new System.Drawing.Size(27, 23);
      this.btnJumpForward.TabIndex = 7;
      this.btnJumpForward.Text = "";
      this.btnJumpForward.UseVisualStyleBackColor = true;
      this.btnJumpForward.Click += new System.EventHandler(this.btnJumpForward_Click);
      // 
      // btnPlayPause
      // 
      this.btnPlayPause.Font = new System.Drawing.Font("Webdings", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (2)));
      this.btnPlayPause.Location = new System.Drawing.Point(131, 46);
      this.btnPlayPause.Name = "btnPlayPause";
      this.btnPlayPause.Size = new System.Drawing.Size(27, 23);
      this.btnPlayPause.TabIndex = 6;
      this.btnPlayPause.Text = "";
      this.btnPlayPause.UseVisualStyleBackColor = true;
      this.btnPlayPause.Click += new System.EventHandler(this.btnPlayPause_Click);
      // 
      // btnJumpBack
      // 
      this.btnJumpBack.Font = new System.Drawing.Font("Webdings", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (2)));
      this.btnJumpBack.Location = new System.Drawing.Point(103, 46);
      this.btnJumpBack.Name = "btnJumpBack";
      this.btnJumpBack.Size = new System.Drawing.Size(27, 23);
      this.btnJumpBack.TabIndex = 5;
      this.btnJumpBack.Text = "";
      this.btnJumpBack.UseVisualStyleBackColor = true;
      this.btnJumpBack.Click += new System.EventHandler(this.btnJumpBack_Click);
      // 
      // btnBack
      // 
      this.btnBack.Font = new System.Drawing.Font("Webdings", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (2)));
      this.btnBack.Location = new System.Drawing.Point(75, 46);
      this.btnBack.Name = "btnBack";
      this.btnBack.Size = new System.Drawing.Size(27, 23);
      this.btnBack.TabIndex = 4;
      this.btnBack.Text = "";
      this.btnBack.UseVisualStyleBackColor = true;
      this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
      // 
      // pbProgress
      // 
      this.pbProgress.Location = new System.Drawing.Point(48, 26);
      this.pbProgress.Name = "pbProgress";
      this.pbProgress.Size = new System.Drawing.Size(165, 18);
      this.pbProgress.Step = 1;
      this.pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      this.pbProgress.TabIndex = 2;
      // 
      // btnEnqueue
      // 
      this.btnEnqueue.Font = new System.Drawing.Font("Webdings", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (2)));
      this.btnEnqueue.Location = new System.Drawing.Point(47, 46);
      this.btnEnqueue.Name = "btnEnqueue";
      this.btnEnqueue.Size = new System.Drawing.Size(27, 23);
      this.btnEnqueue.TabIndex = 3;
      this.btnEnqueue.Text = "";
      this.btnEnqueue.UseVisualStyleBackColor = true;
      this.btnEnqueue.Click += new System.EventHandler(this.btnEnqueue_Click);
      // 
      // timerUpdate
      // 
      this.timerUpdate.Enabled = true;
      this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
      // 
      // WinampUserControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Controls.Add(this.btnEnqueue);
      this.Controls.Add(this.pbProgress);
      this.Controls.Add(this.label25);
      this.Controls.Add(this.txtTrackName);
      this.Controls.Add(this.tbVolume);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.btnForward);
      this.Controls.Add(this.btnJumpForward);
      this.Controls.Add(this.btnPlayPause);
      this.Controls.Add(this.btnJumpBack);
      this.Controls.Add(this.btnBack);
      this.Name = "WinampUserControl";
      this.Size = new System.Drawing.Size(218, 102);
      this.Load += new System.EventHandler(this.WinampUserControl_Load);
      ((System.ComponentModel.ISupportInitialize) (this.tbVolume)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label25;
    private System.Windows.Forms.TextBox txtTrackName;
    private System.Windows.Forms.TrackBar tbVolume;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Button btnForward;
    private System.Windows.Forms.Button btnJumpForward;
    private System.Windows.Forms.Button btnPlayPause;
    private System.Windows.Forms.Button btnJumpBack;
    private System.Windows.Forms.Button btnBack;
    private System.Windows.Forms.ProgressBar pbProgress;
    private System.Windows.Forms.Button btnEnqueue;
    private System.Windows.Forms.Timer timerUpdate;
  }
}
