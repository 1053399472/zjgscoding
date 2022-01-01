using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Tomasulo
{
    class InstructionSet
    {
        #region Members
        private string instructionName;
        private string functionalUnit;
        private int timeToExecute;
        static DataTable instructionSetDT; 
        #endregion

        #region Ctor
        public InstructionSet()
        {
            LoadDefaults();
        }

 
        #endregion

        #region Properties

        public string InstNameProp
        {
            get
            {
                return instructionName;
            }
            set
            {
                instructionName = value;
            }
        }

        public string FuncUnitProp
        {
            get
            {
                return functionalUnit;
            }
            set
            {
                functionalUnit = value;
            }
        }

        public int TimeToExecProp
        {
            get
            {
                return timeToExecute;
            }
            set
            {
                timeToExecute = value;
            }
        }
        public static DataTable GetInstructionSetDT()
        {
            return instructionSetDT;
        }

        #endregion

        #region Methods
        public static bool Update(string functionalUnit, int timeToExecute)
        {
            for (int i = 0; i < instructionSetDT.Rows.Count; i++)
            {
                if (string.Compare(instructionSetDT.Rows[i][1].ToString(), functionalUnit) == 0)
                {
                    instructionSetDT.Rows[i][2] = timeToExecute;
                }
            }
            return true;
        }


        public static void RefreshSet()
        {
            foreach (DataRow row in instructionSetDT.Rows)
	        {
		        row["Time To Execute"] = 1;
	        }
        }

        private void LoadDefaults()
        {
            instructionName = string.Empty;
            functionalUnit = string.Empty;
            timeToExecute = 1;
            instructionSetDT = new DataTable("Instruction Set table");
            instructionSetDT.Columns.Add("Instruction Name", typeof(string));
            instructionSetDT.Columns.Add("Functional Unit", typeof(string));
            instructionSetDT.Columns.Add("Time To Execute", typeof(int));
            instructionSetDT.Rows.Add("L.D", "LD/SD", 1);
            instructionSetDT.Rows.Add("S.D", "LD/SD", 1);
            instructionSetDT.Rows.Add("LD", "LD/SD", 1);
            instructionSetDT.Rows.Add("SD", "LD/SD", 1);
            instructionSetDT.Rows.Add("DADDDI", "Integer", 1);
            instructionSetDT.Rows.Add("DADDU", "Integer", 1);
            instructionSetDT.Rows.Add("DADDIU", "Integer", 1);
            instructionSetDT.Rows.Add("DSGTIU", "Integer", 1);
            instructionSetDT.Rows.Add("DSUB", "Integer", 1);
            instructionSetDT.Rows.Add("BEQ", "Branch", 1);
            instructionSetDT.Rows.Add("BNE", "Branch", 1);
            instructionSetDT.Rows.Add("BEQZ", "Branch", 1);
            instructionSetDT.Rows.Add("BNEZ", "Branch", 1);
            instructionSetDT.Rows.Add("ADD.D", "FP Add", 1);
            instructionSetDT.Rows.Add("SUB.D", "FP Add", 1);
            instructionSetDT.Rows.Add("MUL.D", "FP Multiply", 1);
            instructionSetDT.Rows.Add("DIV.D", "FP Multiply", 1);
        } 

        public static string GetInstruction(string instructionName)
        {
            for (int i = 0; i < instructionSetDT.Rows.Count; i++)
            {
                if (string.Compare(instructionSetDT.Rows[i][0].ToString(), instructionName) == 0)
                {
                    return instructionSetDT.Rows[i][1].ToString();
                }
            }

            return string.Empty;
        } 
        #endregion

    }
}
