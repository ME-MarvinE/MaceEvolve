namespace MaceEvolve.WinForms.Controls
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
            components = new System.ComponentModel.Container();
            DrawTimer = new System.Windows.Forms.Timer(components);
            lblSelectedNodePreviousOutput = new System.Windows.Forms.Label();
            lblSelectedNodeId = new System.Windows.Forms.Label();
            lblNetworkConnectionsCount = new System.Windows.Forms.Label();
            lblNetworkNodesCount = new System.Windows.Forms.Label();
            lblSelectedNodeConnectionCount = new System.Windows.Forms.Label();
            lblNodeInputOrAction = new System.Windows.Forms.Label();
            nudMaxNodeStaggerLevel = new System.Windows.Forms.NumericUpDown();
            lblMaxNodeStaggerLEvel = new System.Windows.Forms.Label();
            chkShowNodeLabels = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)nudMaxNodeStaggerLevel).BeginInit();
            SuspendLayout();
            // 
            // DrawTimer
            // 
            DrawTimer.Enabled = true;
            DrawTimer.Interval = 17;
            DrawTimer.Tick += DrawTimer_Tick;
            // 
            // lblSelectedNodePreviousOutput
            // 
            lblSelectedNodePreviousOutput.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblSelectedNodePreviousOutput.AutoSize = true;
            lblSelectedNodePreviousOutput.BackColor = System.Drawing.Color.Transparent;
            lblSelectedNodePreviousOutput.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblSelectedNodePreviousOutput.Location = new System.Drawing.Point(15, 411);
            lblSelectedNodePreviousOutput.Name = "lblSelectedNodePreviousOutput";
            lblSelectedNodePreviousOutput.Size = new System.Drawing.Size(217, 32);
            lblSelectedNodePreviousOutput.TabIndex = 4;
            lblSelectedNodePreviousOutput.Text = "Previous Output: 0";
            // 
            // lblSelectedNodeId
            // 
            lblSelectedNodeId.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblSelectedNodeId.AutoSize = true;
            lblSelectedNodeId.BackColor = System.Drawing.Color.Transparent;
            lblSelectedNodeId.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblSelectedNodeId.Location = new System.Drawing.Point(15, 362);
            lblSelectedNodeId.Name = "lblSelectedNodeId";
            lblSelectedNodeId.Size = new System.Drawing.Size(61, 32);
            lblSelectedNodeId.TabIndex = 4;
            lblSelectedNodeId.Text = "Id: 0";
            // 
            // lblNetworkConnectionsCount
            // 
            lblNetworkConnectionsCount.AutoSize = true;
            lblNetworkConnectionsCount.BackColor = System.Drawing.Color.Transparent;
            lblNetworkConnectionsCount.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblNetworkConnectionsCount.Location = new System.Drawing.Point(15, 11);
            lblNetworkConnectionsCount.Name = "lblNetworkConnectionsCount";
            lblNetworkConnectionsCount.Size = new System.Drawing.Size(174, 32);
            lblNetworkConnectionsCount.TabIndex = 4;
            lblNetworkConnectionsCount.Text = "Connections: 0";
            // 
            // lblNetworkNodesCount
            // 
            lblNetworkNodesCount.AutoSize = true;
            lblNetworkNodesCount.BackColor = System.Drawing.Color.Transparent;
            lblNetworkNodesCount.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblNetworkNodesCount.Location = new System.Drawing.Point(15, 62);
            lblNetworkNodesCount.Name = "lblNetworkNodesCount";
            lblNetworkNodesCount.Size = new System.Drawing.Size(109, 32);
            lblNetworkNodesCount.TabIndex = 4;
            lblNetworkNodesCount.Text = "Nodes: 0";
            // 
            // lblSelectedNodeConnectionCount
            // 
            lblSelectedNodeConnectionCount.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblSelectedNodeConnectionCount.AutoSize = true;
            lblSelectedNodeConnectionCount.BackColor = System.Drawing.Color.Transparent;
            lblSelectedNodeConnectionCount.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblSelectedNodeConnectionCount.Location = new System.Drawing.Point(15, 463);
            lblSelectedNodeConnectionCount.Name = "lblSelectedNodeConnectionCount";
            lblSelectedNodeConnectionCount.Size = new System.Drawing.Size(174, 32);
            lblSelectedNodeConnectionCount.TabIndex = 4;
            lblSelectedNodeConnectionCount.Text = "Connections: 0";
            // 
            // lblNodeInputOrAction
            // 
            lblNodeInputOrAction.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblNodeInputOrAction.AutoSize = true;
            lblNodeInputOrAction.BackColor = System.Drawing.Color.Transparent;
            lblNodeInputOrAction.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblNodeInputOrAction.Location = new System.Drawing.Point(15, 511);
            lblNodeInputOrAction.Name = "lblNodeInputOrAction";
            lblNodeInputOrAction.Size = new System.Drawing.Size(246, 32);
            lblNodeInputOrAction.TabIndex = 4;
            lblNodeInputOrAction.Text = "Creature Input: None";
            // 
            // nudMaxNodeStaggerLevel
            // 
            nudMaxNodeStaggerLevel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudMaxNodeStaggerLevel.Location = new System.Drawing.Point(827, 11);
            nudMaxNodeStaggerLevel.Name = "nudMaxNodeStaggerLevel";
            nudMaxNodeStaggerLevel.Size = new System.Drawing.Size(48, 23);
            nudMaxNodeStaggerLevel.TabIndex = 9;
            nudMaxNodeStaggerLevel.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudMaxNodeStaggerLevel.ValueChanged += nudMaxNodeStaggerLevel_ValueChanged;
            // 
            // lblMaxNodeStaggerLEvel
            // 
            lblMaxNodeStaggerLEvel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lblMaxNodeStaggerLEvel.AutoSize = true;
            lblMaxNodeStaggerLEvel.BackColor = System.Drawing.Color.Transparent;
            lblMaxNodeStaggerLEvel.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblMaxNodeStaggerLEvel.ForeColor = System.Drawing.Color.White;
            lblMaxNodeStaggerLEvel.Location = new System.Drawing.Point(642, 11);
            lblMaxNodeStaggerLEvel.Name = "lblMaxNodeStaggerLEvel";
            lblMaxNodeStaggerLEvel.Size = new System.Drawing.Size(179, 21);
            lblMaxNodeStaggerLEvel.TabIndex = 8;
            lblMaxNodeStaggerLEvel.Text = "Max Node Stagger Level";
            // 
            // chkShowNodeLabels
            // 
            chkShowNodeLabels.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            chkShowNodeLabels.AutoSize = true;
            chkShowNodeLabels.BackColor = System.Drawing.Color.Transparent;
            chkShowNodeLabels.Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkShowNodeLabels.ForeColor = System.Drawing.Color.White;
            chkShowNodeLabels.Location = new System.Drawing.Point(756, 534);
            chkShowNodeLabels.Name = "chkShowNodeLabels";
            chkShowNodeLabels.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            chkShowNodeLabels.Size = new System.Drawing.Size(123, 19);
            chkShowNodeLabels.TabIndex = 10;
            chkShowNodeLabels.Text = "Show Node Labels";
            chkShowNodeLabels.UseVisualStyleBackColor = false;
            chkShowNodeLabels.CheckedChanged += chkShowNodeLabels_CheckedChanged;
            // 
            // NeuralNetworkViewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(chkShowNodeLabels);
            Controls.Add(nudMaxNodeStaggerLevel);
            Controls.Add(lblMaxNodeStaggerLEvel);
            Controls.Add(lblNetworkNodesCount);
            Controls.Add(lblNodeInputOrAction);
            Controls.Add(lblSelectedNodeConnectionCount);
            Controls.Add(lblNetworkConnectionsCount);
            Controls.Add(lblSelectedNodeId);
            Controls.Add(lblSelectedNodePreviousOutput);
            Name = "NeuralNetworkViewer";
            Size = new System.Drawing.Size(886, 561);
            Load += NeuralNetworkViewer_Load;
            Paint += NeuralNetworkViewer_Paint;
            MouseDown += NeuralNetworkViewer_MouseDown;
            MouseMove += NeuralNetworkViewer_MouseMove;
            MouseUp += NeuralNetworkViewer_MouseUp;
            ((System.ComponentModel.ISupportInitialize)nudMaxNodeStaggerLevel).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        public System.Windows.Forms.Timer DrawTimer;
        public System.Windows.Forms.Label lblSelectedNodePreviousOutput;
        public System.Windows.Forms.Label lblSelectedNodeId;
        public System.Windows.Forms.Label lblNetworkConnectionsCount;
        public System.Windows.Forms.Label lblNetworkNodesCount;
        public System.Windows.Forms.Label lblSelectedNodeConnectionCount;
        public System.Windows.Forms.Label lblNodeInputOrAction;
        private System.Windows.Forms.NumericUpDown nudMaxNodeStaggerLevel;
        private System.Windows.Forms.Label lblMaxNodeStaggerLEvel;
        private System.Windows.Forms.CheckBox chkShowNodeLabels;
    }
}
