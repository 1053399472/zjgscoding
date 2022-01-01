using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tomasulo
{
    public partial class FuncuinalUnitsUsageForm : Form
    {
        public FuncuinalUnitsUsageForm()
        {
            InitializeComponent();
        }

        private void ResourcesForm_Load(object sender, EventArgs e)
        {
            ResourcesGV.DataSource = FuncuinalUnitsUsage.FUUsageDT();
        }

        public static void updateResourceTable(int iteration, int clockCycle, int instructioNum)
        {
            FuncuinalUnitsUsage.Update(iteration, clockCycle, instructioNum);
        }


    }
}
