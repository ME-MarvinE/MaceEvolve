namespace MaceEvolve.Controls
{
    partial class NeuralNetworkViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.DrawTimer = new System.Windows.Forms.Timer(this.components);
            this.lblSelectedNodePreviousOutput = new System.Windows.Forms.Label();
            this.lblSelectedNodeId = new System.Windows.Forms.Label();
            this.lblNetworkConnectionsCount = new System.Windows.Forms.Label();
            this.lblNetworkNodesCount = new System.Windows.Forms.Label();
            this.lblSelectedNodeConnectionCount = new System.Windows.Forms.Label();
            this.lblNodeInputOrAction = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // DrawTimer
            // 
            this.DrawTimer.Enabled = true;
            this.DrawTimer.Interval = 17;
            this.DrawTimer.Tick += new System.EventHandler(this.DrawTimer_Tick);
            // 
            // lblSelectedNodePreviousOutput
            // 
            this.lblSelectedNodePreviousOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSelectedNodePreviousOutput.AutoSize = true;
            this.lblSelectedNodePreviousOutput.BackColor = System.Drawing.Color.Transparent;
            this.lblSelectedNodePreviousOutput.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSelectedNodePreviousOutput.Location = new System.Drawing.Point(15, 411);
            this.lblSelectedNodePreviousOutput.Name = "lblSelectedNodePreviousOutput";
            this.lblSelectedNodePreviousOutput.Size = new System.Drawing.Size(217, 32);
            this.lblSelectedNodePreviousOutput.TabIndex = 4;
            this.lblSelectedNodePreviousOutput.Text = "Previous Output: 0";
            // 
            // lblSelectedNodeId
            // 
            this.lblSelectedNodeId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSelectedNodeId.AutoSize = true;
            this.lblSelectedNodeId.BackColor = System.Drawing.Color.Transparent;
            this.lblSelectedNodeId.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSelectedNodeId.Location = new System.Drawing.Point(15, 362);
            this.lblSelectedNodeId.Name = "lblSelectedNodeId";
            this.lblSelectedNodeId.Size = new System.Drawing.Size(61, 32);
            this.lblSelectedNodeId.TabIndex = 4;
            this.lblSelectedNodeId.Text = "Id: 0";
            // 
            // lblNetworkConnectionsCount
            // 
            this.lblNetworkConnectionsCount.AutoSize = true;
            this.lblNetworkConnectionsCount.BackColor = System.Drawing.Color.Transparent;
            this.lblNetworkConnectionsCount.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblNetworkConnectionsCount.Location = new System.Drawing.Point(15, 11);
            this.lblNetworkConnectionsCount.Name = "lblNetworkConnectionsCount";
            this.lblNetworkConnectionsCount.Size = new System.Drawing.Size(174, 32);
            this.lblNetworkConnectionsCount.TabIndex = 4;
            this.lblNetworkConnectionsCount.Text = "Connections: 0";
            // 
            // lblNetworkNodesCount
            // 
            this.lblNetworkNodesCount.AutoSize = true;
            this.lblNetworkNodesCount.BackColor = System.Drawing.Color.Transparent;
            this.lblNetworkNodesCount.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblNetworkNodesCount.Location = new System.Drawing.Point(15, 62);
            this.lblNetworkNodesCount.Name = "lblNetworkNodesCount";
            this.lblNetworkNodesCount.Size = new System.Drawing.Size(109, 32);
            this.lblNetworkNodesCount.TabIndex = 4;
            this.lblNetworkNodesCount.Text = "Nodes: 0";
            // 
            // lblSelectedNodeConnectionCount
            // 
            this.lblSelectedNodeConnectionCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSelectedNodeConnectionCount.AutoSize = true;
            this.lblSelectedNodeConnectionCount.BackColor = System.Drawing.Color.Transparent;
            this.lblSelectedNodeConnectionCount.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSelectedNodeConnectionCount.Location = new System.Drawing.Point(15, 463);
            this.lblSelectedNodeConnectionCount.Name = "lblSelectedNodeConnectionCount";
            this.lblSelectedNodeConnectionCount.Size = new System.Drawing.Size(174, 32);
            this.lblSelectedNodeConnectionCount.TabIndex = 4;
            this.lblSelectedNodeConnectionCount.Text = "Connections: 0";
            // 
            // lblNodeInputOrAction
            // 
            this.lblNodeInputOrAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblNodeInputOrAction.AutoSize = true;
            this.lblNodeInputOrAction.BackColor = System.Drawing.Color.Transparent;
            this.lblNodeInputOrAction.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblNodeInputOrAction.Location = new System.Drawing.Point(15, 511);
            this.lblNodeInputOrAction.Name = "lblNodeInputOrAction";
            this.lblNodeInputOrAction.Size = new System.Drawing.Size(246, 32);
            this.lblNodeInputOrAction.TabIndex = 4;
            this.lblNodeInputOrAction.Text = "Creature Input: None";
            // 
            // NeuralNetworkViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblNetworkNodesCount);
            this.Controls.Add(this.lblNodeInputOrAction);
            this.Controls.Add(this.lblSelectedNodeConnectionCount);
            this.Controls.Add(this.lblNetworkConnectionsCount);
            this.Controls.Add(this.lblSelectedNodeId);
            this.Controls.Add(this.lblSelectedNodePreviousOutput);
            this.Name = "NeuralNetworkViewer";
            this.Size = new System.Drawing.Size(886, 561);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.NeuralNetworkViewer_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.NeuralNetworkViewer_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.NeuralNetworkViewer_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.NeuralNetworkViewer_MouseUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.Timer DrawTimer;
        public System.Windows.Forms.Label lblSelectedNodePreviousOutput;
        public System.Windows.Forms.Label lblSelectedNodeId;
        public System.Windows.Forms.Label lblNetworkConnectionsCount;
        public System.Windows.Forms.Label lblNetworkNodesCount;
        public System.Windows.Forms.Label lblSelectedNodeConnectionCount;
        public System.Windows.Forms.Label lblNodeInputOrAction;
    }
}
