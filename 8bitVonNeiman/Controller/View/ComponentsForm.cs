﻿using System;
using System.Windows.Forms;

namespace _8bitVonNeiman.Controller.View {
    public partial class ComponentsForm : Form {

        private IComponentsFormOutput _output;

        public ComponentsForm(IComponentsFormOutput output) {
            _output = output;
            InitializeComponent();
        }

        private void ComponentsForm_FormClosed(object sender, FormClosedEventArgs e) {
            _output.FormClosed();
        }

        private void editorButton_Click(object sender, EventArgs e) {
            _output.EditorButtonClicked();
        }

        private void memoryButton_Click(object sender, EventArgs e) {
            _output.MemoryButtonClicked();
        }
    }
}
