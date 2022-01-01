using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tomasulo
{
    class Execution
    {
        #region members
        private static Dictionary<string, Dictionary<int, int>> addMulTimers;
        private string currentInstType;
        private string prevInstType;
        private string currentInstName;
        private string currentInstFullName;
        private int InstructionsNumberInQueue;
        private bool ROBbusy;
        private bool ResevationStatiosBusy;
        private bool LoadBufferBusy;
        private string Destination;
        private int IssuePhaseCycles;
        private int ExecustionPhaseCycles;
        static int Clock;
        private string result;
        private string CalcResult;
        private string vj;
        private string vk;
        private string qj;
        private string qk;
        private static int FPAdd;
        private static int FPMultiply;
        private static int instructionIndex;
        private static int InstructionLatest;
        private string[] comment;
        private string fullComment;
        private int timer;
        private static int numOfUsedCDBCycles;
        private string[] DependentIns;
        private int[] numOfDependentIns;
        private string nameOfDependentInstruction;
        private int TempNumberOfDependecies;
        private int[] CDBCycle;
        private int f;
        private int CDBNewCycle;
        private bool IsCDBCycle;
        private static int DifferentFUCounter;
        private static int SameFUCounter;
        private static int IssuedInstCounter;
        private static bool IsInstructionIssued;
        private static bool ThreeIssueInstHelperFlag;
        private static int NumOfCommitedIns;
        private bool CommitOrNot;

        #endregion

        #region Ctor
        public Execution()
        {    
            LoadDefaults();
        }


        #endregion

        #region props
        public Dictionary<string ,Dictionary<int,int>> AddMulTimers
        {
            get
            {
                return addMulTimers;
            }
            set
            {
                addMulTimers = value;
            }
        }
        public static int NumOfIssuedInst
        {
            get
            {
                return IssuedInstCounter;
            }
            set
            {
                IssuedInstCounter = value;
            }
        }

        public static int ResetDifferentFU
        {
            get
            {
                return DifferentFUCounter;
            }
            set
            {
                DifferentFUCounter = value;
            }
        }

        public static int ResetSameFU
        {
            get
            {
                return SameFUCounter;
            }
            set
            {
                SameFUCounter = value;
            }
        }

        public static bool SetInsIssued
        {
            get
            {
                return IsInstructionIssued;
            }
            set
            {
                IsInstructionIssued = value;
            }
        }

        public static int SetLastInsNum
        {
            get
            {
                return InstructionLatest;
            }
            set
            {
                InstructionLatest = value;
            }
        }

        public int ClockProp
        {
            get
            {
                return Clock;
            }
            set
            {
                Clock = value;
            }
        }

        public int FPMultProp
        {
            get
            {
                return FPMultiply;
            }
            set
            {
                FPMultiply = value;
            }
        }

        public int FPADdProp
        {
            get
            {
                return FPAdd;
            }
            set
            {
                FPAdd = value;
            }
        }

        public int instructionIndexProp
        {
            get
            {
                return instructionIndex;
            }
            set
            {
                instructionIndex = value;
            }
        }

        public static int SetNumOfCommitedIns
        {
            get
            {
                return NumOfCommitedIns;
            }

            set
            {
                NumOfCommitedIns = value;
            }
        }

        public static int SetNumOfCDBIns
        {
            get
            {
                return numOfUsedCDBCycles;
            }

            set
            {
                numOfUsedCDBCycles = value;
            }
        }

        public static int ClockTick()
        {
            Clock++;
            return Clock;
        }

        #endregion

        #region Methods

        //============================================================
        // Function name   : ResetClock
        // Programmer      : Liron Shamir
        // Description     : reset clock to zero
        // Return type     : static void 
        //============================================================
        public static void ResetClock()
        {
            Clock = 0;
        }


        //============================================================
        // Function name   : GetArrayMaxValue
        // Programmer      : Liron Shamir
        // Description     : return for given array the maximum value
        // Return type     : static int 
        // Argument        : int[] array
        //============================================================
        public static int GetArrayMaxValue(int[] array)
        {
            int maxVal = array[0];

            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] > maxVal)
                {
                    maxVal = array[i];
                }
            }

            return maxVal;
        }


        //============================================================
        // Function name   : Issue
        // Programmer      : Liron Shamir
        // Description     : For given instruction number (ordered in InstructionFromInput)
        //                   and for number of instruction that we can do issue per cycle,
        //                   this method handles the issue process. The method able to handle only 3 states,
        //                   in case of issue 1, 2 or 3 instruction per cycles
        // Return type     : bool 
        // Argument        : int instructionIdx
        // Argument        : int numOfInsPerCycle
        // Argument        : int IterationNum
        // Argument        : int clock
        // Argument        : int? numOfInsToCDB
        // Argument        : int? numofInsToCommit
        //============================================================
        public bool Issue(int instructionIdx, int numOfInsPerCycle, int IterationNum, int clock, int? numOfInsToCDB, int? numofInsToCommit)
        {
            InstructionsNumberInQueue = InstructionFromInput.InstructionsFromInputDT().Rows.Count;
            currentInstName = ConstructInstructionFullName(instructionIdx);
            if (InstructionsNumberInQueue > 0)
            {
                currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString());
                currentInstName = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString();

                if (currentInstType.Equals("LD/SD"))
                {
                    LoadBufferBusy = LoadBuffer.IsBusy();
                }
                else
                {
                    ResevationStatiosBusy = ResevationStation.IsResevationStationBusy();
                }

                ROBbusy = ReorderBuffer.IsROBBusy();
            }

            if (!(ROBbusy && (LoadBufferBusy || (ResevationStatiosBusy
                && (currentInstType.Equals("LD/SD"))))))
            {

                switch (numOfInsPerCycle)
                {
                    case 1:
                        HandleOneInsPerCycle(instructionIdx, IterationNum, clock);
                        break;
                    case 2:
                        HandleIssueTwoInsPerCycle(instructionIdx, numOfInsPerCycle, IterationNum, clock);
                        break;
                    case 3:
                        HandleIssueThreeInsPerCycle(instructionIdx, numOfInsPerCycle, IterationNum, clock);
                        break;
                    default:
                        break;
                }

                return true;
            }
            else
            {
                return false;
            }
        }


        //============================================================
        // Function name   : ConstructInstructionFullName
        // Programmer      : Liron Shamir
        // Description     : constructing the long and full syntax of instruction
        // Return type     : string 
        // Argument        : int instructionIdx
        //============================================================
        private string ConstructInstructionFullName(int instructionIdx)
        {
            return InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString()
                + " " + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString()
                + ", " + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()
                + ", " + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString();
        }


        //============================================================
        // Function name   : HandleThreeInsPerCycle
        // Programmer      : Liron Shamir
        // Description     : for given instruction ,  for each FU , handles the state of 3 issues in on cycle
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int numOfInsPerCycle
        // Argument        : int IterationNum
        // Argument        : int clock
        //============================================================
        private void HandleIssueThreeInsPerCycle(int instructionIdx, int numOfInsPerCycle, int IterationNum, int clock) 
        {
            IsInstructionIssued = false;
            currentInstFullName = ConstructInstructionFullName(instructionIdx);

            if (instructionIdx == 0)
            {
                IssuePhaseCycles = 1;

                if (IssuePhaseCycles == clock)
                {
                    IsInstructionIssued = true;
                    InstructionStatusManager.Insert(IterationNum, currentInstFullName, IssuePhaseCycles, 0, 0, 0, 0, "");
                    InstructionLatest = instructionIdx;
                    IssuedInstCounter++;
                }
            }
            else
            {
                if (instructionIdx == 0)
                {
                    IssuePhaseCycles = 1;

                    if (IssuePhaseCycles == clock)
                    {
                        IsInstructionIssued = true;
                        InstructionStatusManager.Insert(IterationNum, currentInstFullName, IssuePhaseCycles, 0, 0, 0, 0, "");
                        IssuedInstCounter++;
                    }
                }
                else
                {
                    if (instructionIdx > (InstructionLatest) && IssuedInstCounter != numOfInsPerCycle && SameFUCounter == 0)
                    {
                        if (InstructionLatest == -1)
                        {
                            InstructionLatest = 0;
                        }

                        if (InstructionLatest == 0)
                        {
                            if (IssuedInstCounter != 0)
                            {
                                if ((instructionIdx - InstructionLatest) == 1)
                                {
                                    prevInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx - 1]["Instruction Name"].ToString());
                                    currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString());
                                }
                                else
                                {
                                    prevInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx - 2]["Instruction Name"].ToString());
                                    currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString());
                                    ThreeIssueInstHelperFlag = true;
                                }
                            }
                            else
                            {
                                prevInstType = "NoInstructionIssued";
                                currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString());
                            }
                        }
                        else
                        {
                            if (IssuedInstCounter != 0)
                            {

                                if ((instructionIdx - InstructionLatest) == 2)
                                {
                                    prevInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx - 1]["Instruction Name"].ToString());
                                    currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString());
                                }
                                else
                                {
                                    prevInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx - 2]["Instruction Name"].ToString());
                                    currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString());
                                    ThreeIssueInstHelperFlag = true;
                                }
                            }
                            else
                            {
                                prevInstType = "NoInstructionIssued";
                                currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString());
                            }
                        }

                        currentInstName = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString();

                        
                        if (prevInstType.Equals(currentInstType) || currentInstType.Equals("Branch"))
                        {
                            SameFUCounter++;
                            InstructionLatest = instructionIdx - 1;
                            IsInstructionIssued = false;
                            ThreeIssueInstHelperFlag = false;
                        }
                        else
                        {
                            if (ThreeIssueInstHelperFlag) 
                            {
                                prevInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx - 1]["Instruction Name"].ToString());
                                currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString());
                                currentInstName = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString();

                                if (prevInstType.Equals(currentInstType) || currentInstType.Equals("Branch"))
                                {
                                    SameFUCounter++;
                                    InstructionLatest = instructionIdx - 1;
                                    IsInstructionIssued = false;
                                    ThreeIssueInstHelperFlag = false;
                                }
                                else
                                {
                                    DifferentFUCounter++;
                                    IssuePhaseCycles = clock;
                                    IssuedInstCounter++;
                                    IsInstructionIssued = true;
                                    InstructionStatusManager.Insert(IterationNum, currentInstFullName, IssuePhaseCycles, 0, 0, 0, 0, "");

                                    if (instructionIdx == InstructionFromInput.InstructionsFromInputDT().Rows.Count - 1)
                                    {
                                        IssuedInstCounter = numOfInsPerCycle;
                                    }
                                }
                            }
                            else
                            {
                                DifferentFUCounter++;
                                IssuePhaseCycles = clock;
                                IssuedInstCounter++;
                                IsInstructionIssued = true;
                                InstructionStatusManager.Insert(IterationNum, currentInstFullName, IssuePhaseCycles, 0, 0, 0, 0, "");

                                if (instructionIdx == InstructionFromInput.InstructionsFromInputDT().Rows.Count - 1)
                                {
                                    IssuedInstCounter = numOfInsPerCycle;
                                }
                            }
 
                        }


                    }

                    if (IssuedInstCounter == 0 && instructionIdx > InstructionLatest)
                    {
                        IssuePhaseCycles = clock;
                        IssuedInstCounter++;
                        IsInstructionIssued = true;
                        InstructionStatusManager.Insert(IterationNum, currentInstFullName, IssuePhaseCycles, 0, 0, 0, 0, "");

                        if (instructionIdx == InstructionFromInput.InstructionsFromInputDT().Rows.Count - 1)
                        {
                            InstructionLatest = instructionIdx;
                            IssuedInstCounter = numOfInsPerCycle;

                        }

                    }
                }
            }

            if (IssuedInstCounter <= numOfInsPerCycle && IsInstructionIssued)
            {
                if (IssuedInstCounter == numOfInsPerCycle)
                {
                    IsInstructionIssued = false;
                    InstructionLatest = instructionIdx;
                }
                switch (currentInstType)
                {
                    case "LD/SD":
                        if (currentInstName.Equals("LD") || currentInstName.Equals("L.D"))
                        {
                            result = "Mem[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + " + Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]]";
                         //   result = "Mem[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + " + "RegisterResultStatus.RegisterResultStatusDT().Rows[2][];
                            if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                            {
                                Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();
                                if (IssuePhaseCycles == clock)
                                {
                                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, "ROB" + (ReorderBuffer.GetBusyFalseItem()).ToString(), "Yes");
                                }
                            }
                            else
                            {
                                if (IssuePhaseCycles == clock)
                                {
                                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                }
                            }
                            if (IssuePhaseCycles == clock)
                            {
                                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result,"");
                                LoadBuffer.Update(LoadBuffer.GetBusyFalseItem(), true, result);
                            }
                        }
                        else 
                        {
                            result = "Mem[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + " + Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]]";
                            if (IssuePhaseCycles == clock)
                            {
                                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", result, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(),"");
                            }
                        }
                        break;
                    case "FP Add":
                        if (currentInstName.Equals("ADD.D"))
                        {
                            result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "] + Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";

                            CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + " + " + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() +" = "
                                + (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) +  Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();
                            //result = result + " -> " +CalcResult;
                            if (instructionIdx == InstructionFromInput.InstructionsFromInputDT().Rows.Count -1)
                            {
                                if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                                {
                                    Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();

                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][0].ToString(), "Yes");
                                        ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result, CalcResult);
                                    }
                                }
                                else
                                {
                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                    }
                                }
                                
                            }
                            else
                            {
                                if (IssuePhaseCycles == clock)
                                {
                                    ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result, CalcResult);
                                }

                                if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                                {
                                    Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();

                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][0].ToString(), "Yes");
                                    }
                                }
                                else
                                {
                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                    }
                                }

                            }

                        }
                        else
                        {
                            result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "] - Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";

                            CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + " - " + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() + " = "
                                + (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) - Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();
                            
                            if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                            {
                                Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();

                                if (IssuePhaseCycles == clock)
                                {
                                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][0].ToString(), "Yes");
                                }
                            }
                            else
                            {
                                if (IssuePhaseCycles == clock)
                                {
                                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                }
                            }
                            if (IssuePhaseCycles == clock)
                            {
                                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result, CalcResult);
                            }



                        }

                        if (RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()) != "")
                        {
                            qj = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString());
                            vj = string.Empty;
                        }
                        else
                        {
                            qj = string.Empty;
                            vj = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "]";
                        }

                        if (RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()) != "")
                        {
                            qk = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString());
                            vk = string.Empty;
                        }
                        else
                        {
                            qk = string.Empty;
                            vk = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";
                        }

                        Destination = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString());

                        if (IssuePhaseCycles == clock)
                        {
                            ResevationStation.UpdateResevationStations(ResevationStation.GetFirstFreeIndex("FP Add"), 0, true, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString(), vj, vk, qj, qk, Destination, instructionIdx);
                        }
                        break;
                    case "FP Multiply":
                        if (currentInstName.Equals("MUL.D"))
                        {
                            result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "] * Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";

                            CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + " * " + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() + " = "
                              + (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) * Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();
                            

                            if (IssuePhaseCycles == clock)
                            {
                                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result, CalcResult);
                            }

                            if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                            {
                                Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();

                                if (IssuePhaseCycles == clock)
                                {
                                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][0].ToString(), "Yes");
                                }
                            }
                            else
                            {
                                if (IssuePhaseCycles == clock)
                                {
                                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                }
                            }
                        }
                        else
                        {
                            result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "] / Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";

                            CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + " / " + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() + " = "
                      + (Convert.ToDouble(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) / Convert.ToDouble(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();
                    

                            if (IssuePhaseCycles == clock)
                            {
                                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result, CalcResult);
                            }

                            if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                            {
                                Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();

                                if (IssuePhaseCycles == clock)
                                {
                                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][0].ToString(), "Yes");
                                }
                            }
                            else
                            {
                                if (IssuePhaseCycles == clock)
                                {
                                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                }
                            }
                        }

                        if (RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()) != "")
                        {
                            qj = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString());
                            vj = string.Empty;
                        }
                        else
                        {
                            qj = string.Empty;
                            vj = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "]";
                        }

                        if (RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()) != "")
                        {
                            qk = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString());
                            vk = string.Empty;
                        }
                        else
                        {
                            qk = string.Empty;
                            vk = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";
                        }

                        Destination = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString());
                        if (IssuePhaseCycles == clock)
                        {
                            ResevationStation.UpdateResevationStations(ResevationStation.GetFirstFreeIndex("FP Multiply"), 0, true, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString(), vj, vk, qj, qk, Destination,instructionIdx);
                        }

                        break;
                    case "Branch":
                        if (InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString().Equals("BEQ") 
                        || InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString().Equals("BNE"))
                        {
                            result = "Address[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";
                        }
                        else if (InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString().Equals("BEQZ")
                            || InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString().Equals("BNEZ"))
                        {
                            result = "Address[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "]";
                        }

                        if (IssuePhaseCycles == clock)
                        {
                            ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", "", result,"");
                        }
                        break;
                    case "Integer":
                        result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "] + " + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString();

                        CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + " + " + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + " = "
  + (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) + Convert.ToInt32(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString())).ToString();

                        RobResevationsStationInteractions(instructionIdx);

                        if (IssuePhaseCycles == clock)
                        {
                            ResevationStation.UpdateResevationStations(ResevationStation.GetFirstFreeIndex("INTADD"), 0, true, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString(), vj, vk, qj, qk, Destination, instructionIdx);
                        }
                        if (IssuePhaseCycles == clock)
                        {
                            ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result, CalcResult);
                        }

                        if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                        {
                            Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();

                            if (IssuePhaseCycles == clock)
                            {
                                RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 33, "ROB" + (ReorderBuffer.GetBusyFalseItem()).ToString(), "Yes");
                            }
                        }
                        else
                        {
                            if (IssuePhaseCycles == clock)
                            {
                                RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 33, string.Empty, "No");
                            }
                        }

                        if (ReorderBuffer.GetRobNum(Destination) != "None")
                        {
                            Destination = ReorderBuffer.GetRobNum(Destination);
                        }
                        break;
                    
                    default:
                        break;

                }
            }
        }


        //============================================================
        // Function name   : HandleTwoInsPerCycle
        // Programmer      : Liron Shamir
        // Description     : for given instruction ,  for each FU , handles the state of 2 issues in on cycle
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int numOfInsPerCycle
        // Argument        : int IterationNum
        // Argument        : int clock
        //============================================================
        private void HandleIssueTwoInsPerCycle(int instructionIdx, int numOfInsPerCycle, int IterationNum, int clock)
        {
            IsInstructionIssued = false;
            currentInstFullName = ConstructInstructionFullName(instructionIdx);

            if (instructionIdx == 0)
            {
                IssuePhaseCycles = 1;

                if (IssuePhaseCycles == clock)
                {
                    IssuedInstCounter++;
                    IsInstructionIssued = true;
                    InstructionStatusManager.Insert(IterationNum, currentInstFullName, IssuePhaseCycles, 0, 0, 0, 0, "");
                }
            }
            else
            {
                if (instructionIdx == 0)
                {
                    IssuePhaseCycles = 1;

                    if (IssuePhaseCycles == clock)
                    {
                        IssuedInstCounter++;
                        IsInstructionIssued = true;
                        InstructionStatusManager.Insert(IterationNum, currentInstFullName, IssuePhaseCycles, 0, 0, 0, 0, "");
                    }
                }
                else
                {

                    if (instructionIdx > (InstructionLatest + 1) && IssuedInstCounter != numOfInsPerCycle && SameFUCounter == 0)
                    {
                        if (InstructionLatest == -1)
                        {
                            InstructionLatest = 0;


                            for (int i = instructionIdx; i > InstructionLatest; i--)
                            {
                                prevInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["Instruction Name"].ToString());
                                currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[i]["Instruction Name"].ToString());
                                currentInstName = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString();

                                if (prevInstType.Equals(currentInstType) ||currentInstName.Equals("Branch") || prevInstType.Equals("Branch"))
                                {
                                    IssuePhaseCycles = clock;
                                    SameFUCounter++;

                                    if (IssuedInstCounter == 0)
                                    {
                                        IssuedInstCounter++;
                                        IsInstructionIssued = true;
                                        InstructionStatusManager.Insert(IterationNum, currentInstFullName, clock, 0, 0, 0, 0, "");

                                    }
                                    else
                                    {
                                        IsInstructionIssued = false;
                                    }
                                }
                                else if (SameFUCounter == 0)
                                {

                                    DifferentFUCounter++;

                                    IssuePhaseCycles = clock;
                                    IssuedInstCounter++;
                                    IsInstructionIssued = true;
                                    InstructionStatusManager.Insert(IterationNum, currentInstFullName, clock, 0, 0, 0, 0, "");

                                }
                                else
                                {
                                    IssuedInstCounter++;
                                    IsInstructionIssued = true;
                                    InstructionStatusManager.Insert(IterationNum, currentInstFullName, clock, 0, 0, 0, 0, "");
                                }

                            }
                        }
                        else
                        {
                            for (int i = instructionIdx; i > (InstructionLatest + 1); i--)
                            {
                                prevInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["Instruction Name"].ToString());
                                currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[i]["Instruction Name"].ToString());
                                currentInstName = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString();

                                if (string.Compare(prevInstType, currentInstType) == 0 || string.Compare(currentInstType, "Branch") == 0 || string.Compare(prevInstType, "Branch") == 0)
                                {
                                    SameFUCounter++;
                                    InstructionLatest = instructionIdx - 1;
                                    IsInstructionIssued = false;
                                }

                                if (SameFUCounter == 0)
                                {
                                    DifferentFUCounter++;
                                    IssuePhaseCycles = clock;
                                    IssuedInstCounter++;
                                    IsInstructionIssued = true;
                                    InstructionStatusManager.Insert(IterationNum, currentInstFullName, clock, 0, 0, 0, 0, "");
                                }

                            }
                        }
                    }
                }
                    if (IssuedInstCounter == 0 && instructionIdx > InstructionLatest)
                    {
                        IssuePhaseCycles = clock;
                        IssuedInstCounter++;
                        IsInstructionIssued = true;
                        InstructionStatusManager.Insert(IterationNum, currentInstFullName, IssuePhaseCycles, 0, 0, 0, 0, "");

                        if (instructionIdx == InstructionFromInput.InstructionsFromInputDT().Rows.Count - 1)
                        {
                            InstructionLatest = instructionIdx;
                            IssuedInstCounter = numOfInsPerCycle;

                        }

                    }


                }
                if (IssuedInstCounter <= numOfInsPerCycle && IsInstructionIssued)
                {
                    if (IssuedInstCounter == numOfInsPerCycle)
                    {
                        IsInstructionIssued = false;
                        InstructionLatest = instructionIdx;
                    }
                    switch (currentInstType)
                    {
                        case "LD/SD":
                            if (currentInstName.Equals("LD") || currentInstName.Equals("L.D"))
                            {
                                result = "Mem[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + " + Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]]";

                                if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                                {
                                    Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();
                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, "ROB" + (ReorderBuffer.GetBusyFalseItem()).ToString(), "Yes");
                                    }
                                }
                                else
                                {
                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                    }
                                }
                                if (IssuePhaseCycles == clock)
                                {
                                    ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result,"");
                                    LoadBuffer.Update(LoadBuffer.GetBusyFalseItem(), true, result);
                                }
                            }
                            else
                            {
                                result = "Mem[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + " + Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]]";
                                if (IssuePhaseCycles == clock)
                                {
                                    ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", result, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(),"");
                                }
                            }
                            break;
                        case "FP Add":
                            if (currentInstName.Equals("ADD.D"))
                            {
                                result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "] + Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";

                                CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + " + " + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() + " = "
+ (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) + Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();
       

                                if (instructionIdx == InstructionFromInput.InstructionsFromInputDT().Rows.Count - 1)
                                {
                                    if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                                    {
                                        Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();

                                        if (IssuePhaseCycles == clock)
                                        {
                                            RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][0].ToString(), "Yes");
                                            ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result,CalcResult);
                                        }
                                    }
                                    else
                                    {
                                        if (IssuePhaseCycles == clock)
                                        {
                                            RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                        }
                                    }

                                }
                                else
                                {
                                    if (IssuePhaseCycles == clock)
                                    {
                                        ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result,CalcResult);
                                    }

                                    if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                                    {
                                        Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();

                                        if (IssuePhaseCycles == clock)
                                        {
                                            RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][0].ToString(), "Yes");
                                        }
                                    }
                                    else
                                    {
                                        if (IssuePhaseCycles == clock)
                                        {
                                            RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                        }
                                    }

                                }

                            }
                            else
                            {
                                result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "] - Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";
                                CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + " - " + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() + " = "
+ (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) - Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();
       

                                if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                                {
                                    Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();

                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][0].ToString(), "Yes");
                                    }
                                }
                                else
                                {
                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                    }
                                }
                                if (IssuePhaseCycles == clock)
                                {
                                    ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result,CalcResult);
                                }



                            }

                            if (RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()) != "")
                            {
                                qj = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString());
                                vj = string.Empty;
                            }
                            else
                            {
                                qj = string.Empty;
                                vj = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "]";
                            }

                            if (RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()) != "")
                            {
                                qk = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString());
                                vk = string.Empty;
                            }
                            else
                            {
                                qk = string.Empty;
                                vk = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";
                            }

                            Destination = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString());

                            if (IssuePhaseCycles == clock)
                            {
                                ResevationStation.UpdateResevationStations(ResevationStation.GetFirstFreeIndex("FP Add"), 0, true, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString(), vj, vk, qj, qk, Destination,instructionIdx);
                            }
                            break;
                        case "FP Multiply":
                            if (currentInstName.Equals("MUL.D"))
                            {
                                result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "] * Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";

                                CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + " * " + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() + " = "
+ (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) * Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();
       

                                if (IssuePhaseCycles == clock)
                                {
                                    ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result,CalcResult);
                                }

                                if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                                {
                                    Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();

                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][0].ToString(), "Yes");
                                    }
                                }
                                else
                                {
                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                    }
                                }
                            }
                            else
                            {
                                result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "] / Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";

                                CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + " / " + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() + " = "
+ (Convert.ToDouble(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) / Convert.ToDouble(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();
       

                                if (IssuePhaseCycles == clock)
                                {
                                    ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result,CalcResult);
                                }


                                if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                                {
                                    Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();

                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][0].ToString(), "Yes");
                                    }
                                }
                                else
                                {
                                    if (IssuePhaseCycles == clock)
                                    {
                                        RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                                    }
                                }
                            }

                            if (RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()) != "")
                            {
                                qj = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString());
                                vj = string.Empty;
                            }
                            else
                            {
                                qj = string.Empty;
                                vj = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "]";
                            }

                            if (RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()) != "")
                            {
                                qk = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString());
                                vk = string.Empty;
                            }
                            else
                            {
                                qk = string.Empty;
                                vk = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";
                            }

                            Destination = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString());
                            if (IssuePhaseCycles == clock)
                            {
                                ResevationStation.UpdateResevationStations(ResevationStation.GetFirstFreeIndex("FP Multiply"), 0, true, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString(), vj, vk, qj, qk, Destination, instructionIdx);
                            }

                            break;
                        case "Branch":
                            if (InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString().Equals("BEQ")
                            || InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString().Equals("BNE"))
                            {
                                result = "Address[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]";
                            }
                            else if (InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString().Equals("BEQZ")
                                || InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString().Equals("BNEZ"))
                            {
                                result = "Address[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "]";
                            }

                            if (IssuePhaseCycles == clock)
                            {
                                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", "", result,"");
                            }
                            break;
                        case "Integer":
                            result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + "] + " + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString();

                            CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + " + " + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + " = "
      + (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) + Convert.ToInt32(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString())).ToString();
          
                            Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();
                            if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
                            {

                                if (IssuePhaseCycles == clock)
                                {
                                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 33, "ROB" + (ReorderBuffer.GetBusyFalseItem()).ToString(), "Yes");
                                }
                            }
                            else
                            {
                                if (IssuePhaseCycles == clock)
                                {
                                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 33, string.Empty, "No");
                                }
                            }

                            if (ReorderBuffer.GetRobNum(Destination) != "None")
                            {
                                Destination = ReorderBuffer.GetRobNum(Destination);
                            }
                            RobResevationsStationInteractions(instructionIdx);

                            if (IssuePhaseCycles == clock)
                            {
                                ResevationStation.UpdateResevationStations(ResevationStation.GetFirstFreeIndex("INTADD"), 0, true, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString(), vj, vk, qj, qk, Destination, instructionIdx);
                            }
                            if (IssuePhaseCycles == clock)
                            {
                                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result,CalcResult);
                            }
                            break;

                        default:
                            break;

                    }
                }
            
        }

        //============================================================
        // Function name   : HandleOneInsPerCycle
        // Programmer      : Liron Shamir
        // Description     : for given instruction ,  for each FU , handles the state of 1 issues in on cycle
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int IterationNum
        // Argument        : int clock
        //============================================================
        private void HandleOneInsPerCycle(int instructionIdx, int IterationNum, int clock)
        {
            currentInstFullName = ConstructInstructionFullName(instructionIdx);
            if (instructionIdx == 0)
            {
                IssuePhaseCycles = 1;
                if (IssuePhaseCycles == clock)
                {
                    InstructionStatusManager.Insert(IterationNum, currentInstFullName, IssuePhaseCycles, 0, 0, 0, 0, "");
                }
                
            }
            else
            {
                IssuePhaseCycles++;
                if (IssuePhaseCycles == clock)
                {
                    InstructionStatusManager.Insert(IterationNum, currentInstFullName, IssuePhaseCycles, 0, 0, 0, 0, "");
                }
                
            }
            switch (currentInstType)
            {
                case "LD/SD":
                    HandleOneIssueLoadStore(instructionIdx, clock);
                    break;
                case "FP Add":
                    HandleOneIssueAddSubstract(instructionIdx, clock);
                    break;
                case "FP Multiply":
                    HandleOneIssueMult(instructionIdx, clock);
                    break;
                case "Branch":
                    HandleOneIssueBranch(instructionIdx, clock);
                    break;
                case "Integer":
                    HandleOneIssueINT(instructionIdx, clock);
                    break;
                default:
                    break;
            }
        }


        //============================================================
        // Function name   : HandleOneIssueINT
        // Programmer      : Liron Shamir
        // Description     : handling integer operations in one issue per cycle mode
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int clock
        //============================================================
        private void HandleOneIssueINT(int instructionIdx, int clock)
        {
            result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][2].ToString() + "] + " + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][3].ToString();

            CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + " + " + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + " = "
+ (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) + Convert.ToInt32(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString())).ToString();
          

            if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
            {
                Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();
                if (IssuePhaseCycles == clock)
                {
                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 33, "ROB" + (ReorderBuffer.GetBusyFalseItem()).ToString(), "Yes");
                }
            }
            else
            {
                if (IssuePhaseCycles == clock)
                {
                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 33, string.Empty, "No");
                }
            }
            RobResevationsStationInteractions(instructionIdx);

            if (IssuePhaseCycles == clock)
            {
                ResevationStation.UpdateResevationStations(ResevationStation.GetFirstFreeIndex("INTADD"), 0, true, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString(), vj, vk, qj, qk, Destination, instructionIdx);
            }
            if (IssuePhaseCycles == clock)
            {
                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][1].ToString(), result,CalcResult);
               // ResevationStation.UpdateResevationStations(ResevationStation.GetFirstFreeIndex("INTADD"), 0, true, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString(), vj, vk, qj, qk, Destination, instructionIdx);
            }
            if (ReorderBuffer.GetRobNum(Destination) != "None")
            {
                Destination = ReorderBuffer.GetRobNum(Destination);
            }
        }


        //============================================================
        // Function name   : HandleOneIssueBranch
        // Programmer      : Liron Shamir
        // Description     : handling branch operations in one issue per cycle mode
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int clock
        //============================================================
        private void HandleOneIssueBranch(int instructionIdx, int clock)
        {
            if (InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString().Equals("BEQ") || 
                InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString().Equals("BNE"))
            {
                result = "Address[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][3].ToString() + "]";
            }
            else if (InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString().Equals("BEQZ") || 
                InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString().Equals("BNEZ"))
            {
                result = "Address[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][2].ToString() + "]";
            }

            if (IssuePhaseCycles == clock)
            {
                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", "", result,"");
            }
        }


        //============================================================
        // Function name   : HandleOneIssueMult
        // Programmer      : Liron Shamir
        // Description     : handling FP Multiply operations in one issue per cycle mode
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int clock
        //============================================================
        private void HandleOneIssueMult(int instructionIdx, int clock)
        {
            RobResevationsStationInteractions(instructionIdx);
            if (string.Compare(currentInstName, "MUL.D") == 0)
            {
                HandleOneIssueMulDiv(instructionIdx, clock, '*');
            }
            else
            {
                HandleOneIssueMulDiv(instructionIdx, clock, '/');
            }

            //RobResevationsStationInteractions(instructionIdx);
        }


        //============================================================
        // Function name   : HandleOneIssueMulDiv
        // Programmer      : Liron Shamir
        // Description     : handling Multiply\Divide operations in one issue per cycle mode
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int clock
        // Argument        : char oper
        //============================================================
        private void HandleOneIssueMulDiv(int instructionIdx, int clock, char oper)
        {
            result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][2].ToString() + "] " + oper + " Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][3].ToString() + "]";

                switch (oper)
	            {
                    case '*':
                        
            CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + oper + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() + " = "
                         + (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) * Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();
            break;
                    case '/':

            CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + oper + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() + " = "
                         + (Convert.ToDouble(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) / Convert.ToDouble(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();
            break;
                    default:
                     break;
	            }

               
       

            Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();
            
            if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
            {
                if (IssuePhaseCycles == clock)
                {
                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, "ROB" + (ReorderBuffer.GetBusyFalseItem()).ToString(), "Yes");
                }
            }
            else
            {
                if (IssuePhaseCycles == clock)
                {
                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                }
            }
            if (IssuePhaseCycles == clock)
            {
                Destination = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString());
                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][1].ToString(), result,CalcResult);
                ResevationStation.UpdateResevationStations(ResevationStation.GetFirstFreeIndex("FP Multiply"), 0, true, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString(), vj, vk, qj, qk, Destination, instructionIdx);
            }
        }


        //============================================================
        // Function name   : HandleOneIssueAddSubstract
        // Programmer      : Liron Shamir
        // Description     : handling FP Add operations in one issue per cycle mode
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int clock
        //============================================================
        private void HandleOneIssueAddSubstract(int instructionIdx, int clock)
        {
            if (currentInstName.Equals("ADD.D"))
            {
                HandleOneIssueAdd(instructionIdx, clock);
            }
            else //Subtruct
            {
                HandleOneIssueSubstract(instructionIdx, clock);
            }

            RobResevationsStationInteractions(instructionIdx);

            if (IssuePhaseCycles == clock)
            {
                ResevationStation.UpdateResevationStations(ResevationStation.GetFirstFreeIndex("FP Add"), 0, true, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][0].ToString(), vj, vk, qj, qk, Destination, instructionIdx);
            }
        }


        //============================================================
        // Function name   : RobResevationsStationInteractions
        // Programmer      : Liron Shamir
        // Description     : updating Resevation stations varaibles
        // Return type     : void 
        // Argument        : int instructionIdx
        //============================================================
        private void RobResevationsStationInteractions(int instructionIdx)
        {
            if (RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][2].ToString()) != "")
            {
                qj = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][2].ToString());
                vj = string.Empty;
            }
            else
            {
                qj = string.Empty;
                vj = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][2].ToString() + "]";
            }

            if (RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][3].ToString()) != "")
            {
                qk = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][3].ToString());
                vk = string.Empty;
            }
            else
            {
                qk = string.Empty;
                vk = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][3].ToString() + "]";
            }
            Destination = RegisterResultStatus.GetROBID(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString());
        }


        //============================================================
        // Function name   : HandleOneIssueSubstract
        // Programmer      : Liron Shamir
        // Description     : handling SUB operations in one issue per cycle mode
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int clock
        //============================================================
        private void HandleOneIssueSubstract(int instructionIdx, int clock)
        {
            result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][2].ToString() + "] - Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][3].ToString() + "]";

                  
            CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + '-' + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() + " = "
                         + (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) - Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();

            Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();
            if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
            {
                if (IssuePhaseCycles == clock)
                {
                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, "ROB" + (ReorderBuffer.GetBusyFalseItem()).ToString(), "Yes");
                }
            }
            else
            {
                if (IssuePhaseCycles == clock)
                {
                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                }
            }
            if (IssuePhaseCycles == clock)
            {
                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][1].ToString(), result,CalcResult);
            }
        }


        //============================================================
        // Function name   : HandleOneIssueAdd
        // Programmer      : Liron Shamir
        // Description     : handling ADD operations in one issue per cycle mode
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int clock
        //============================================================
        private void HandleOneIssueAdd(int instructionIdx, int clock)
        {
            result = "Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][2].ToString() + "] + Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][3].ToString() + "]";

            CalcResult = RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString() + '+' + RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString() + " = "
             + (Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString()].ToString()) + Convert.ToInt32(RegisterResultStatus.RegisterResultStatusDT().Rows[2][InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString()].ToString())).ToString();


            Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();
            if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
            {
                if (IssuePhaseCycles == clock)
                {
                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, "ROB" + (ReorderBuffer.GetBusyFalseItem()).ToString(), "Yes");
                }
            }
            else
            {
                if (IssuePhaseCycles == clock)
                {
                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                }
            }
            if (IssuePhaseCycles == clock)
            {
                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][1].ToString(), result,CalcResult);
            }
        }


        //============================================================
        // Function name   : HandleOneIssueLoadStore
        // Programmer      : Liron Shamir
        // Description     : handling load/store operations in one issue per cycle mode
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int clock
        //============================================================
        private void HandleOneIssueLoadStore(int instructionIdx, int clock)
        {
            if (currentInstName.Equals("LD") || currentInstName.Equals("L.D"))
            {
                HandleOneIssueLoad(instructionIdx, clock);
            }
            else
            {
                HandleOneIssueStore(instructionIdx, clock);
            }
        }


        //============================================================
        // Function name   : HandleOneIssueStore
        // Programmer      : Liron Shamir
        // Description     : handling store operations in one issue per cycle mode
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int clock
        //============================================================
        private void HandleOneIssueStore(int instructionIdx, int clock)
        {
            result = "Mem[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][2].ToString() + " + Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][3].ToString() + "]]";
            if (IssuePhaseCycles == clock)
            {
                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", result, InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][1].ToString(),"");
            }
        }


        //============================================================
        // Function name   : HandleOneIssueLoad
        // Programmer      : Liron Shamir
        // Description     : handling load operations in one issue per cycle mode
        // Return type     : void 
        // Argument        : int instructionIdx
        // Argument        : int clock
        //============================================================
        private void HandleOneIssueLoad(int instructionIdx, int clock)
        {
            result = "Mem[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][2].ToString() + " + Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx][3].ToString() + "]]";
            Destination = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString();
            if (!ReorderBuffer.ReorderBufferDT().Rows[ReorderBuffer.GetBusyFalseItem()][3].ToString().Equals("Commit"))
            {
                if (IssuePhaseCycles == clock)
                {
                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, "ROB" + (ReorderBuffer.GetBusyFalseItem()).ToString(), "Yes");
                }
            }
            else
            {
                if (IssuePhaseCycles == clock)
                {
                    RegisterResultStatus.Update((int.Parse(Destination.Substring(1))) + 1, string.Empty, "No");
                }
            }
            if (IssuePhaseCycles == clock)
            {
                ReorderBuffer.Update(ReorderBuffer.GetBusyFalseItem(), true, currentInstFullName, "Issue", InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["DestReg"].ToString(), result,"");
                LoadBuffer.Update(LoadBuffer.GetBusyFalseItem(), true, result);
            }
        }


        //============================================================
        // Function name   : Execute
        // Programmer      : Liron Shamir
        // Description     : handling the execution phase in the algorithm
        //                   here  ExecutionManager , ROB and resevation stations will be updated
        //                   also , here dependencies will be resolved.
        // Return type     : void 
        // Argument        : int instNo
        // Argument        : int numOfInsPerCycle
        // Argument        : int IterationNum
        // Argument        : int clock
        // Argument        : int? numOfInsToCDB
        // Argument        : int? numofInsToCommit
        //============================================================
        public void Execute(int instNo, int numOfInsPerCycle, int IterationNum, int clock, int? numOfInsToCDB, int? numofInsToCommit)
        {
            string PrevDdest;
            int ROBIndex = 0;
            int dependenciesCounter = 0;
            timer = 0;
            currentInstName = ConstructInstructionFullName(instNo);
            string tempInst = string.Empty;
            string tempInst2 = string.Empty;

            if (instNo == 0)
            {
                if (clock == 2)
                {
                    InstructionStatusManager.Update(instNo, 
                        IterationNum, 
                        currentInstName, 
                        int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString())),
                        2, int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][4].ToString()), 
                        int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][5].ToString()), 
                        int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][6].ToString()), "");
                    
                    ReorderBuffer.Update(0, true, 
                        currentInstName, 
                        "Execute", ReorderBuffer.ReorderBufferDT().Rows[instNo][4].ToString(),
                        ReorderBuffer.ReorderBufferDT().Rows[instNo][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][6].ToString());
                }
                return;
            }
            else if (InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString().Equals("S.D")
                || InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString().Equals("SD"))
            {
                if (numOfInsPerCycle != 1)
                {
                    if (clock >= instNo)
                    {
                        ROBIndex = UpdateExecutionAndRob(instNo, IterationNum, clock);
                    }
                }
                else
                {
                    if (clock > instNo)
                    {
                        ROBIndex = UpdateExecutionAndRob(instNo, IterationNum, clock);
                    }
                }
            }
            else
            {
                if (numOfInsPerCycle == 1)
                {
                    if (InstructionStatusManager.ExecutionManagerDT().Rows.Count > instNo)
                    {
                        if (clock > 2 && clock > (instNo - 1)
                            && InstructionStatusManager.ExecutionCycle(instNo) == 0)
                        {
                            PrevDdest = InitAlgoTempVars(instNo, ref tempInst, ref tempInst2, ref dependenciesCounter);
                            if (dependenciesCounter != 0)
                            {
                                HandleDependencies(instNo, IterationNum, clock, ref ROBIndex, ref dependenciesCounter);
                            }
                            else
                            {
                                
                                if (clock > (instNo))
                                {
                                    if (clock - int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString()) == 1)
                                    {
                                        ExecustionPhaseCycles = clock;
                                        InstructionStatusManager.Update(instNo, IterationNum, currentInstName, int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString())), ExecustionPhaseCycles, int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][4].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][5].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][6].ToString()), string.Empty);
                                        for (int j = 0; j < ReorderBuffer.ReorderBufferDT().Rows.Count; j++)
                                        {
                                            if (ReorderBuffer.ReorderBufferDT().Rows[j][2].ToString() == currentInstName)
                                            {
                                                ROBIndex = j;
                                                ReorderBuffer.Update(ROBIndex, true, currentInstName, "Execute", ReorderBuffer.ReorderBufferDT().Rows[instNo][4].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][6].ToString());
                                                j = ReorderBuffer.ReorderBufferDT().Rows.Count;
                                            }
                                        }
                                        SetNoDependenciesAlgoVars(instNo);

                                        if (InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Add")
                                            || InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Multiply")
                                            || InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Integer"))
                                        {
                                            ResevationStation.UpdateResevationStations(ResevationStation.GetIndexOfIns(instNo), timer, true, InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString(), vj, vk, string.Empty, string.Empty, InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["DestReg"].ToString(), instNo);
                                        }
                                        return;
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    if (InstructionStatusManager.ExecutionManagerDT().Rows.Count > instNo)
                    {
                        if (clock > 2 && clock >= (instNo - 1)
                            && InstructionStatusManager.ExecutionCycle(instNo) == 0)
                        {
                            PrevDdest = InitAlgoTempVars(instNo, ref tempInst, ref tempInst2, ref dependenciesCounter);
                            if (dependenciesCounter != 0)
                            {
                                string tempComment;
                                tempComment = fullComment;
                                if (tempComment != string.Empty)
                                {
                                    tempComment = tempComment.Remove(0, 9).Substring(0, 3);
                                }
                                if (dependenciesCounter == 1 && (tempComment == "SD " || tempComment == "S.D"))
                                {
                                    dependenciesCounter = 0;
                                    InstructionStatusManager.ExecutionManagerDT().Rows[instNo][7] = string.Empty;

                                    if (clock - int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString()) == 1)
                                    {
                                        ExecustionPhaseCycles = clock;
                                        InstructionStatusManager.Update(instNo, IterationNum, currentInstName, int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString())), ExecustionPhaseCycles, int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][4].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][5].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][6].ToString()), string.Empty);
                                        for (int j = 0; j < ReorderBuffer.ReorderBufferDT().Rows.Count; j++)
                                        {
                                            if (ReorderBuffer.ReorderBufferDT().Rows[j][2].ToString() == currentInstName)
                                            {
                                                ROBIndex = j;
                                                ReorderBuffer.Update(ROBIndex, true, currentInstName, "Execute", ReorderBuffer.ReorderBufferDT().Rows[instNo][4].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][6].ToString());
                                                j = ReorderBuffer.ReorderBufferDT().Rows.Count;
                                            }
                                        }
                                        SetNoDependenciesAlgoVars(instNo);

                                        if (InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Add")
                                            || InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Multiply")
                                             || InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Integer"))
                                        {
                                            ResevationStation.UpdateResevationStations(ResevationStation.GetIndexOfIns(instNo), timer, true, InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString(), vj, vk, string.Empty, string.Empty, InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["DestReg"].ToString(), instNo);
                                        }
                                        return;
                                    }

                                }
                                else
                                {
                                    int DependentinstructionIdx;
                                    for (int i = 0; i < dependenciesCounter; i++)
                                    {
                                        numOfDependentIns[i] = ReorderBuffer.GetIndex(DependentIns[i]);
                                    }

                                    TempNumberOfDependecies = dependenciesCounter;

                                    for (int i = 0; i < TempNumberOfDependecies; i++)
                                    {
                                        if (string.Compare(ReorderBuffer.ReorderBufferDT().Rows[numOfDependentIns[i]][3].ToString(), "Write CDB") == 0 || string.Compare(ReorderBuffer.ReorderBufferDT().Rows[numOfDependentIns[i]][3].ToString(), "Commit") == 0)
                                        {
                                            dependenciesCounter--;
                                            nameOfDependentInstruction = DependentIns[i];
                                            DependentinstructionIdx = 0;

                                            for (int j = 0; j < InstructionStatusManager.ExecutionManagerDT().Rows.Count; j++)
                                            {
                                                if (string.Compare(nameOfDependentInstruction, InstructionStatusManager.ExecutionManagerDT().Rows[j][1].ToString()) == 0)
                                                {
                                                    DependentinstructionIdx = j;
                                                    CDBCycle[f] = InstructionStatusManager.GetCDBCycle(DependentinstructionIdx);
                                                    f++;
                                                }
                                            }

                                            DependentIns[i] = string.Empty;
                                            if (dependenciesCounter == 0)
                                            {
                                                if (InstructionStatusManager.ExecutionManagerDT().Rows[instNo][7].ToString() != string.Empty)
                                                {
                                                    for (int t = 0; t < InstructionStatusManager.CommentForInst(instNo).Length; t++)
                                                    {
                                                        if (InstructionStatusManager.CommentForInst(instNo)[t] == "SD" ||
                                                            InstructionStatusManager.CommentForInst(instNo)[t] == "S.D" ||
                                                            InstructionStatusManager.CommentForInst(instNo)[t] == "BEQ" ||
                                                            InstructionStatusManager.CommentForInst(instNo)[t] == "BNE" ||
                                                            InstructionStatusManager.CommentForInst(instNo)[t] == "BEQZ" ||
                                                            InstructionStatusManager.CommentForInst(instNo)[t] == "BNEZ")
                                                        {
                                                            CommitOrNot = true;
                                                        }
                                                    }
                                                }
                                                if (!CommitOrNot)
                                                {
                                                    if (clock > GetArrayMaxValue(CDBCycle))
                                                    {
                                                        for (int e = 0; e < TempNumberOfDependecies; e++)
                                                        {
                                                            CDBCycle[e] = 0;
                                                        }
                                                        
                                                        f = 0;
                                                        ExecustionPhaseCycles = clock;
                                                        InstructionStatusManager.Update(instNo, IterationNum, currentInstName, int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString())), ExecustionPhaseCycles, int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][4].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][5].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][6].ToString()), "");
                                                        for (int j = 0; j < ReorderBuffer.ReorderBufferDT().Rows.Count; j++)
                                                        {
                                                            if (ReorderBuffer.ReorderBufferDT().Rows[j][2].ToString() == currentInstName)
                                                            {
                                                                ROBIndex = j;
                                                                ReorderBuffer.Update(ROBIndex, true, currentInstName, "Execute", ReorderBuffer.ReorderBufferDT().Rows[instNo][4].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][6].ToString());
                                                                j = ReorderBuffer.ReorderBufferDT().Rows.Count;
                                                            }
                                                        }


                                                        if (!InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("LD/SD")
                                                            &&!InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Branch")
                                                            &&!InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Integer"))
                                                        {
                                                            SetReslovedDependenciesAlgoVars(instNo);
                                                            ResevationStation.UpdateResevationStations(ResevationStation.GetIndexOfIns(instNo),
                                                                timer, true, InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()
                                                                , vj, vk, string.Empty, string.Empty,
                                                                ReorderBuffer.ReorderBufferDT().Rows[ROBIndex][0].ToString(), instNo);
                                                            return;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    int[] CommitCycles = new int[10];
                                                    int c = 0;
                                                    int Max = 0;
                                                    CommitOrNot = false;
                                                    for (int q = 0; q < InstructionStatusManager.InstructionWithDepen(instNo).Length; q++)
                                                    {
                                                        for (int j = 0; j < InstructionStatusManager.ExecutionManagerDT().Rows.Count; j++)
                                                        {
                                                            if (InstructionStatusManager.InstructionWithDepen(instNo)[q] == InstructionStatusManager.ExecutionManagerDT().Rows[j][1].ToString())
                                                            {
                                                                CommitCycles[c] = int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[j][6].ToString());
                                                                c++;
                                                            }
                                                        }
                                                    }


                                                    for (int r = 0; r < CommitCycles.Length; r++)
                                                    {
                                                        if (CommitCycles[r] > Max)
                                                        {
                                                            Max = CommitCycles[r]; //Gets the Maximal commit cycle.
                                                        }
                                                    }

                                                    if (clock > Max)
                                                    {
                                                        if (!CommitOrNot)
                                                        {
                                                            ROBIndex = ExecuteMaxClock(instNo, IterationNum, clock, ROBIndex);
                                                        }
                                                        else
                                                        {
                                                            CommitOrNot = false;
                                                            for (int q = 0; q < InstructionStatusManager.InstructionWithDepen(instNo).Length; q++)
                                                            {
                                                                for (int j = 0; j < InstructionStatusManager.ExecutionManagerDT().Rows.Count; j++)
                                                                {
                                                                    if (InstructionStatusManager.InstructionWithDepen(instNo)[q] == InstructionStatusManager.ExecutionManagerDT().Rows[j][1].ToString())
                                                                    {
                                                                        CommitCycles[c] = int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[j][6].ToString());
                                                                        c++;
                                                                    }
                                                                }
                                                            }

                                                            for (int r = 0; r < CommitCycles.Length; r++)
                                                            {
                                                                if (CommitCycles[r] > Max)
                                                                {
                                                                    Max = CommitCycles[r];
                                                                }
                                                            }

                                                            if (clock > Max)
                                                            {
                                                                ROBIndex = ExecuteMaxClock(instNo, IterationNum, clock, ROBIndex);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                for (int i = 0; i < 10; i++)
                                {
                                    DependentIns[i] = string.Empty;
                                    numOfDependentIns[i] = 0;
                                }

                                InstructionStatusManager.UpdateComment(currentInstName, fullComment);
                                return;
                            }
                            else
                            {
                                if (clock >= (instNo - 1))
                                {
                                    if (clock - int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString()) == 1)
                                    {
                                        ExecustionPhaseCycles = clock;
                                        InstructionStatusManager.Update(instNo, IterationNum, currentInstName, int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString())), ExecustionPhaseCycles, int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][4].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][5].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][6].ToString()), string.Empty);
                                        for (int j = 0; j < ReorderBuffer.ReorderBufferDT().Rows.Count; j++)
                                        {
                                            if (ReorderBuffer.ReorderBufferDT().Rows[j][2].ToString() == currentInstName)
                                            {
                                                ROBIndex = j;
                                                ReorderBuffer.Update(ROBIndex, true, currentInstName, "Execute", ReorderBuffer.ReorderBufferDT().Rows[instNo][4].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][6].ToString());
                                                j = ReorderBuffer.ReorderBufferDT().Rows.Count;
                                            }
                                        }
                                        SetNoDependenciesAlgoVars(instNo);

                                        if (InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Add") || InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Multiply")
                                            || InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Integer"))
                                        {
                                            ResevationStation.UpdateResevationStations(ResevationStation.GetIndexOfIns(instNo), 
                                                timer, true, 
                                                InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString(), 
                                                vj, vk, string.Empty, string.Empty,
                                                InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["DestReg"].ToString(), instNo);
                                        }
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

            }
            return;
        }


        //============================================================
        // Function name   : ExecuteMaxClock
        // Programmer      : Liron Shamir
        // Description     : 
        // Return type     : int 
        // Argument        : int instNo
        // Argument        : int IterationNum
        // Argument        : int clock
        // Argument        : int ROBIndex
        //============================================================
        private int ExecuteMaxClock(int instNo, int IterationNum, int clock, int ROBIndex)
        {
            f = 0;
            ExecustionPhaseCycles = clock;
            InstructionStatusManager.Update(instNo, IterationNum, currentInstName, int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString())), ExecustionPhaseCycles, int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][4].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][5].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][6].ToString()), "");
            for (int j = 0; j < ReorderBuffer.ReorderBufferDT().Rows.Count; j++)
            {
                if (ReorderBuffer.ReorderBufferDT().Rows[j][2].ToString() == currentInstName)
                {
                    ROBIndex = j;
                    ReorderBuffer.Update(ROBIndex, true, currentInstName, "Execute", ReorderBuffer.ReorderBufferDT().Rows[instNo][4].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][6].ToString());
                    j = ReorderBuffer.ReorderBufferDT().Rows.Count;
                }
            }


            if (!InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("LD/SD")
                && !InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Branch")
                && !InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Integer"))
            {
                SetReslovedDependenciesAlgoVars(instNo);
                ResevationStation.UpdateResevationStations(ResevationStation.GetIndexOfIns(instNo), timer, true, InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString(), vj, vk, string.Empty, string.Empty, RegisterResultStatus.RegisterResultStatusDT().Rows[0][InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["DestReg"].ToString()].ToString(), instNo);
            }
            return ROBIndex;
        }


        //============================================================
        // Function name   : InitAlgoTempVars
        // Programmer      : Liron Shamir
        // Description     : initialization of execution method variables
        // Return type     : string 
        // Argument        : int instNo
        // Argument        : ref string tempInst
        // Argument        : ref string tempInst2
        // Argument        : ref int dependenciesCounter
        //============================================================
        private string InitAlgoTempVars(int instNo, ref string tempInst, ref string tempInst2, ref int dependenciesCounter)
        {
            string PrevDdest = String.Empty;
            InitHazardDetectionTemps(instNo, ref tempInst, ref tempInst2);

            for (int i = instNo; i > 0; i--)
            {
                string currentTempInst = InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["Instruction Name"].ToString();
                if (currentTempInst.Equals("S.D")
                    || currentTempInst.Equals("SD")
                    || currentTempInst.Equals("BEQZ")
                    || currentTempInst.Equals("BNEZ"))
                {
                    if (currentTempInst.Equals("S.D")
                        || currentTempInst.Equals("SD"))
                    {
                        PrevDdest = InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["SourceK"].ToString();
                    }
                    else
                    {
                        PrevDdest = InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["DestReg"].ToString();
                    }
                    if (tempInst.Equals(PrevDdest))
                    {
                        dependenciesCounter = SaveDependencies(dependenciesCounter, i);
                    }

                }
                else
                {
                    PrevDdest = InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["DestReg"].ToString();
                    if (tempInst.Equals(PrevDdest) || tempInst2.Equals(PrevDdest))
                    {
                        dependenciesCounter = SaveDependencies(dependenciesCounter, i);
                    }
                }
            }

            fullComment = string.Empty;
            for (int i = 0; i < dependenciesCounter; i++)
            {
                if (dependenciesCounter - i != 1)
                {
                    fullComment += comment[i] + "; ";
                    comment[i] = string.Empty;
                }
                else
                {
                    fullComment += comment[i];
                    comment[i] = string.Empty;
                }
            }
            return PrevDdest;
        }


        //============================================================
        // Function name   : HandleDependencies
        // Programmer      : Liron Shamir
        // Description     : in charge of depenencies resolving
        // Return type     : void 
        // Argument        : int instNo
        // Argument        : int IterationNum
        // Argument        : int clock
        // Argument        : ref int ROBIndex
        // Argument        : ref int dependenciesCounter
        //============================================================
        private void HandleDependencies(int instNo, int IterationNum, int clock, ref int ROBIndex, ref int dependenciesCounter)
        {
            string tempComment;
            tempComment = fullComment;

            if (tempComment != string.Empty)
            {
                tempComment = tempComment.Remove(0, 9).Substring(0, 3);
            }

            if (dependenciesCounter == 1 && (tempComment == "SD " || tempComment == "S.D"))
            {
                dependenciesCounter = 0;
                InstructionStatusManager.ExecutionManagerDT().Rows[instNo][7] = string.Empty;

                if (clock - int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString()) == 1)
                {
                    HandleNoDependencies(instNo, IterationNum, clock, ref ROBIndex);
                    return;
                }

            }
            else
            {
                int DependentinstructionIdx;
                for (int i = 0; i < dependenciesCounter; i++)
                {
                    numOfDependentIns[i] = ReorderBuffer.GetIndex(DependentIns[i]);
                }

                TempNumberOfDependecies = dependenciesCounter;

                for (int i = 0; i < TempNumberOfDependecies; i++)
                {
                    if (ReorderBuffer.ReorderBufferDT().Rows[numOfDependentIns[i]][3].ToString().Equals("Write CDB")
                        || ReorderBuffer.ReorderBufferDT().Rows[numOfDependentIns[i]][3].ToString().Equals("Commit"))
                    {
                        dependenciesCounter--;
                        nameOfDependentInstruction = DependentIns[i];
                        DependentinstructionIdx = 0;
                        
                        /////////////////////////Resolving dependency
                        for (int j = 0; j < InstructionStatusManager.ExecutionManagerDT().Rows.Count; j++)
                        {
                            if (nameOfDependentInstruction.Equals(InstructionStatusManager.ExecutionManagerDT().Rows[j][1].ToString()))
                            {
                                DependentinstructionIdx = j;
                                CDBCycle[f] = InstructionStatusManager.GetCDBCycle(DependentinstructionIdx);
                                f++;
                            }
                        }
                        ///////////////////////////
                        
                        DependentIns[i] = string.Empty;
                        if (dependenciesCounter == 0)
                        {
                            if (clock > GetArrayMaxValue(CDBCycle))
                            {
                                for (int e = 0; e < TempNumberOfDependecies; e++)
                                {
                                    CDBCycle[e] = 0;
                                }

                                if (InstructionStatusManager.ExecutionManagerDT().Rows[instNo][7].ToString() != string.Empty)
                                {
                                    for (int y = 0; y < InstructionStatusManager.CommentForInst(instNo).Length; y++)
                                    {
                                        if (InstructionStatusManager.CommentForInst(instNo)[y] == "SD" ||
                                            InstructionStatusManager.CommentForInst(instNo)[y] == "S.D" ||
                                            InstructionStatusManager.CommentForInst(instNo)[y] == "BEQ" ||
                                            InstructionStatusManager.CommentForInst(instNo)[y] == "BNE" ||
                                            InstructionStatusManager.CommentForInst(instNo)[y] == "BEQZ" ||
                                            InstructionStatusManager.CommentForInst(instNo)[y] == "BNEZ")
                                        {
                                            CommitOrNot = true;
                                        }
                                    }
                                }
                                if (!CommitOrNot)
                                {
                                    f = 0;
                                    ExecustionPhaseCycles = clock;
                                    InstructionStatusManager.Update(instNo, IterationNum, currentInstName, int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString())), ExecustionPhaseCycles, int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][4].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][5].ToString()), int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][6].ToString()), "");
                                    for (int j = 0; j < ReorderBuffer.ReorderBufferDT().Rows.Count; j++)
                                    {
                                        if (ReorderBuffer.ReorderBufferDT().Rows[j][2].ToString() == currentInstName)
                                        {
                                            ROBIndex = j;
                                            ReorderBuffer.Update(ROBIndex, true, currentInstName, "Execute", ReorderBuffer.ReorderBufferDT().Rows[instNo][4].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instNo][6].ToString());
                                            j = ReorderBuffer.ReorderBufferDT().Rows.Count;
                                        }
                                    }

                                    if (!InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("LD/SD")
                                        && !InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Branch")
                                        && !InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Integer"))
                                    {
                                        SetReslovedDependenciesAlgoVars(instNo);
                                        ResevationStation.UpdateResevationStations(ResevationStation.GetIndexOfIns(instNo), timer, true, InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString(), vj, vk, string.Empty, string.Empty, RegisterResultStatus.RegisterResultStatusDT().Rows[0][InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["DestReg"].ToString()].ToString(), instNo);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < 10; i++)
            {
                DependentIns[i] = string.Empty;
                numOfDependentIns[i] = 0;
            }

            InstructionStatusManager.UpdateComment(currentInstName, fullComment);
            return;
        }


        //============================================================
        // Function name   : SetReslovedDependenciesAlgoVars
        // Programmer      : Liron Shamir
        // Description     : when dependency resolved the resevation station variables will be updated
        // Return type     : void 
        // Argument        : int instNo
        //============================================================
        private void SetReslovedDependenciesAlgoVars(int instNo)
        {
            if (InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Multiply"))
            {
                timer = this.AddMulTimers["FPMullTimers"][instNo];
            }
            else if (InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Add"))
            {
                timer = this.AddMulTimers["FPAddTimers"][instNo];
            }
            else if (InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Integer"))
            {
                timer = this.AddMulTimers["INTADD"][instNo];
            }

            vk = string.Empty;
            vj = string.Empty;
            for (int j = 0; j < ReorderBuffer.GetBusyFalseItem(); j++)
            {
                if (ReorderBuffer.ReorderBufferDT().Rows[j][4].ToString().Equals(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["SourceJ"].ToString()))
                {
                    vj = ReorderBuffer.ReorderBufferDT().Rows[j][5].ToString();
                }

                if (ReorderBuffer.ReorderBufferDT().Rows[j][4].ToString().Equals(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["SourceK"].ToString()))
                {
                    vk = ReorderBuffer.ReorderBufferDT().Rows[j][5].ToString();
                }
            }

            if (string.IsNullOrEmpty(vj))
            {
                vj = "Reg[" + InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["SourceJ"].ToString() + "]";
            }

            if (string.IsNullOrEmpty(vk))
            {
                vk = "Reg[" + InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["SourceK"].ToString() + "]";
            }
        }


        //============================================================
        // Function name   : HandleNoDependencies
        // Programmer      : Liron Shamir
        // Description     : when  there is no dependency attached to instruction , te algorithm variables will be updated
        // Return type     : void 
        // Argument        : int instNo
        // Argument        : int IterationNum
        // Argument        : int clock
        // Argument        : ref int ROBIndex
        //============================================================
        private void HandleNoDependencies(int instNo, int IterationNum, int clock, ref int ROBIndex)
        {
            ExecustionPhaseCycles = clock;
            InstructionStatusManager.Update(instNo,
                IterationNum,
                currentInstName,
                int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instNo][2].ToString())),
                ExecustionPhaseCycles, int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][4].ToString()),
                int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][5].ToString()),
                int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instNo][6].ToString()),
                string.Empty);

            for (int j = 0; j < ReorderBuffer.ReorderBufferDT().Rows.Count; j++)
            {
                if (ReorderBuffer.ReorderBufferDT().Rows[j][2].ToString() == currentInstName)
                {
                    ROBIndex = j;
                    ReorderBuffer.Update(ROBIndex,
                        true,
                        currentInstName,
                        "Execute",
                        ReorderBuffer.ReorderBufferDT().Rows[instNo][4].ToString(),
                        ReorderBuffer.ReorderBufferDT().Rows[instNo][5].ToString(),
                        ReorderBuffer.ReorderBufferDT().Rows[instNo][6].ToString());

                    j = ReorderBuffer.ReorderBufferDT().Rows.Count;
                }
            }
            SetNoDependenciesAlgoVars(instNo);

            if (InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Add")
                || InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Multiply")
                || InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Integer"))
            {
                ResevationStation.UpdateResevationStations(ResevationStation.GetIndexOfIns(instNo), timer, true, InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString(), vj, vk, string.Empty, string.Empty, InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["DestReg"].ToString(), instNo);
            }
        }


        //============================================================
        // Function name   : SetNoDependenciesAlgoVars
        // Programmer      : Liron Shamir
        // Description     : when there is no dependency attached to the instruction , the resevation station variabls will updated
        // Return type     : void 
        // Argument        : int instNo
        //============================================================
        private void SetNoDependenciesAlgoVars(int instNo)
        {
            if (!InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("LD/SD")
                || !InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Branch"))
            {

                if (InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Multiply"))
                {
                    timer = this.AddMulTimers["FPMullTimers"][instNo];
                }
                else if (InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("FP Add"))
                {
                    timer = this.AddMulTimers["FPAddTimers"][instNo];
                }
                else if (InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString()).Equals("Integer"))
                {
                    timer = this.AddMulTimers["INTADD"][instNo];
                }
                if (string.IsNullOrEmpty(vj))
                {
                    vj = "Reg[" + InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["SourceJ"].ToString() + "]";
                }

                if (string.IsNullOrEmpty(vk))
                {
                    vk = "Reg[" + InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["SourceK"].ToString() + "]";
                }
            }
        }


        //============================================================
        // Function name   : SaveDependencies
        // Programmer      : Liron Shamir
        // Description     : updating depenedncies data structures
        // Return type     : int 
        // Argument        : int dependenciesCounter
        // Argument        : int i
        //============================================================
        private int SaveDependencies(int dependenciesCounter, int i)
        {
            comment[dependenciesCounter] = "Wait for " + InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["Instruction Name"].ToString()
                + " " + InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["DestReg"].ToString()
                + ", " + InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["SourceJ"].ToString()
                + ", " + InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["SourceK"].ToString();
            DependentIns[dependenciesCounter] = InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["Instruction Name"].ToString()
                + " " + InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["DestReg"].ToString()
                + ", " + InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["SourceJ"].ToString()
                + ", " + InstructionFromInput.InstructionsFromInputDT().Rows[i - 1]["SourceK"].ToString();
            dependenciesCounter++;
            return dependenciesCounter;
        }

        private static void InitHazardDetectionTemps(int instNo, ref string tempInst, ref string tempInst2)
        {
             if (InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString().Equals("S.D")
                || InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString().Equals("SD")
                || InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString().Equals("BEQZ")
                || InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString().Equals("BNEZ"))
            {
                tempInst = InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["DestReg"].ToString();
            }
            else if (InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString().Equals("BEQ")
                || InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["Instruction Name"].ToString().Equals("BNE"))
            {
                tempInst = InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["DestReg"].ToString();
                tempInst2 = InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["SourceJ"].ToString();
            }
            else
            {
                tempInst = InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["SourceJ"].ToString();
                tempInst2 = InstructionFromInput.InstructionsFromInputDT().Rows[instNo]["SourceK"].ToString();
            }
        }


        //============================================================
        // Function name   : UpdateExecutionAndRob
        // Programmer      : Liron Shamir
        // Description     : updating Reorder buffer and ExecutionManager
        // Return type     : int 
        // Argument        : int instructionIdx
        // Argument        : int IterationNum
        // Argument        : int clock
        //============================================================
        private int UpdateExecutionAndRob(int instructionIdx, int IterationNum, int clock)
        {
            int ROBIndex = 0;

            if (instructionIdx > InstructionStatusManager.ExecutionManagerDT().Rows.Count -1)
            {
                return ROBIndex;
            }
            if (int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][2].ToString())) != 0
                && InstructionStatusManager.ExecutionCycle(instructionIdx) == 0
                && int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][2].ToString()) != clock)
            {
                InstructionStatusManager.Update(instructionIdx,
                    IterationNum,
                    currentInstName,
                    int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][2].ToString())), clock,
                    int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][4].ToString()),
                    int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][5].ToString()),
                    int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][6].ToString()), "");

                for (int j = 0; j < ReorderBuffer.ReorderBufferDT().Rows.Count; j++)
                {
                    if (ReorderBuffer.ReorderBufferDT().Rows[j][2].ToString() == currentInstName)
                    {
                        ROBIndex = j;
                        ReorderBuffer.Update((int)ROBIndex, true, currentInstName, "Execute", ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][4].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][6].ToString());
                        j = ReorderBuffer.ReorderBufferDT().Rows.Count;
                    }
                }
            }

            return ROBIndex;
        }


        //============================================================
        // Function name   : HandleCommonDataBus
        // Programmer      : Liron Shamir
        // Description     : Handling CDB for given instruction number
        // Return type     : bool 
        // Argument        : int instructionIdx
        // Argument        : int numOfInsPerCycle
        // Argument        : int IterationNum
        // Argument        : int clock
        // Argument        : int numOfInsToCDB
        // Argument        : int? numofInsToCommit
        //============================================================
        public bool HandleCommonDataBus(int instructionIdx, int numOfInsPerCycle, int IterationNum, int clock, int numOfInsToCDB, int? numofInsToCommit)
        {
            int ROBIndex;
            string val;
            val = "Mem[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceJ"].ToString() + " + Regs[" + InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["SourceK"].ToString() + "]]";
            currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString());
            currentInstName = InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString();
            f = 0;

            if (numOfInsPerCycle != 1)
            {
                if (clock >= instructionIdx)
                {
                    ROBIndex = CDBGeneralHandle(instructionIdx, IterationNum, clock, numOfInsToCDB, val);
                }
            }
            else
            {
                if (clock > instructionIdx)
                {

                    ROBIndex = CDBGeneralHandle(instructionIdx, IterationNum, clock, numOfInsToCDB, val);
                }
            }
            return true;
        }


        //============================================================
        // Function name   : CDBGeneralHandle
        // Programmer      : Liron Shamir
        // Description     : handling CDB and updating reorder budder index by given operation type
        // Return type     : int 
        // Argument        : int instructionIdx
        // Argument        : int IterationNum
        // Argument        : int clock
        // Argument        : int numOfInsToCDB
        // Argument        : string val
        //============================================================
        private int CDBGeneralHandle(int instructionIdx, int IterationNum, int clock, int numOfInsToCDB, string val)
        {
            int ROBIndex =0;
            if (numOfUsedCDBCycles < numOfInsToCDB)
            {
                if (currentInstType.Equals("FP Add")
                    || currentInstType.Equals("FP Multiply"))
                {
                    ROBIndex = HandleCDBAddMULT(instructionIdx, IterationNum, clock);
                }
                else if (currentInstType.Equals("LD/SD"))
                {
                    if (currentInstName.Equals("LD")
                        || currentInstName.Equals("L.D"))
                    {
                        ROBIndex = HandleCDBLoad(instructionIdx, IterationNum, clock, val);
                    }
                }
                else if (currentInstType.Equals("Integer"))
                {
                    ROBIndex = HandleCDBINT(instructionIdx, IterationNum, clock);
                }
            }
            return ROBIndex;
        }


        //============================================================
        // Function name   : HandleCDBINT
        // Programmer      : Liron Shamir
        // Description     : handling CDB phase for integer operation
        // Return type     : int 
        // Argument        : int instructionIdx
        // Argument        : int IterationNum
        // Argument        : int clock
        //============================================================
        private int HandleCDBINT(int instructionIdx, int IterationNum, int clock)
        {
            int ROBIndex=0;
            if (clock > int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][3].ToString()) && int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][3].ToString()) != 0 && int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][5].ToString()) == 0)
            {
                InstructionStatusManager.Update(instructionIdx, IterationNum, currentInstFullName, int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][2].ToString())), int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][3].ToString())), int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][4].ToString())), clock, 0, string.Empty);

                for (int j = 0; j < ReorderBuffer.ReorderBufferDT().Rows.Count; j++)
                {
                    if (ReorderBuffer.ReorderBufferDT().Rows[j][2].ToString() == currentInstFullName)
                    {
                        ROBIndex = j;
                        ReorderBuffer.Update(ROBIndex, true, currentInstFullName, "Write CDB", ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][4].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][6].ToString());
                        j = ReorderBuffer.ReorderBufferDT().Rows.Count;
                    }
                }

                numOfUsedCDBCycles++;
            }
            return ROBIndex;
        }


        //============================================================
        // Function name   : HandleCDBLoad
        // Programmer      : Liron Shamir
        // Description     : handling CDB phase for load/store operation
        // Return type     : int 
        // Argument        : int instructionIdx
        // Argument        : int IterationNum
        // Argument        : int clock
        // Argument        : string val
        //============================================================
        private int HandleCDBLoad(int instructionIdx, int IterationNum, int clock, string val)
        {
            int ROBIndex=0;
            if (ReorderBuffer.ReorderBufferDT().Rows[instructionIdx]["State"].ToString().Equals("Memory Read"))
            {
                if (int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][4].ToString()) != 0
                    && clock == (int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][4].ToString()) + 1))
                {
                    CDBNewCycle = clock;
                    while ((InstructionStatusManager.ExecutionManagerDT().Rows.Count) != f)
                    {
                        for (int i = 0; i < InstructionStatusManager.ExecutionManagerDT().Rows.Count; i++)
                        {
                            if (CDBNewCycle == int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[i][5].ToString()))
                            {
                                if (IsCDBCycle)
                                {
                                    f = 0;
                                    IsCDBCycle = false;
                                }
                                CDBNewCycle += 1;
                                break;
                            }
                            else
                            {
                                f++;
                            }
                        }
                    }
                    f = 0;


                    if (CDBNewCycle == clock)
                    {
                        InstructionStatusManager.Update(instructionIdx, IterationNum, currentInstFullName, int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][2].ToString())), int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][3].ToString())), int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][4].ToString())), clock, 0, string.Empty);

                        for (int j = 0; j < ReorderBuffer.ReorderBufferDT().Rows.Count; j++)
                        {
                            if (ReorderBuffer.ReorderBufferDT().Rows[j][2].ToString() == currentInstFullName)
                            {
                                ROBIndex = j;
                                ReorderBuffer.Update(ROBIndex, true, currentInstFullName, "Write CDB", ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][4].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][6].ToString());
                                j = ReorderBuffer.ReorderBufferDT().Rows.Count;

                                for (int t = 0; t < LoadBuffer.LoadBufferDT().Rows.Count; t++)
                                {
                                    if (string.Compare(LoadBuffer.LoadBufferDT().Rows[t][2].ToString(), val) == 0)
                                    {
                                        LoadBuffer.Delete(t);
                                    }
                                }
                            }
                        }

                        numOfUsedCDBCycles++;
                    }
                }
            }
            return ROBIndex;
        }


        //============================================================
        // Function name   : HandleCDBAddMULT
        // Programmer      : Liron Shamir
        // Description     : handling CDB phase for Add/sub operation
        // Return type     : int 
        // Argument        : int instructionIdx
        // Argument        : int IterationNum
        // Argument        : int clock
        //============================================================
        private int HandleCDBAddMULT(int instructionIdx, int IterationNum, int clock)
        {
            int ROBIndex=0;
            if ((ResevationStation.GetTimerOfIns(instructionIdx) - 1 == 0) || (ResevationStation.GetTimerOfIns(instructionIdx) == 0)
            && (ReorderBuffer.ReorderBufferDT().Rows[instructionIdx]["State"].ToString().Equals("Execute")))
            {
                InstructionStatusManager.Update(instructionIdx, IterationNum, currentInstFullName, int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][2].ToString())), int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][3].ToString())), int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][4].ToString())), clock, 0, string.Empty);

                for (int j = 0; j < ReorderBuffer.ReorderBufferDT().Rows.Count; j++)
                {
                    if (ReorderBuffer.ReorderBufferDT().Rows[j][2].ToString() == currentInstFullName)
                    {
                        ROBIndex = j;
                        ReorderBuffer.Update(ROBIndex, true, currentInstFullName, "Write CDB", ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][4].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][6].ToString());
                        j = ReorderBuffer.ReorderBufferDT().Rows.Count;
                    }
                }

                numOfUsedCDBCycles++;
            }
            return ROBIndex;
        }

        private void LoadDefaults()
        {
            ROBbusy = true;
            ResevationStatiosBusy = true;
            LoadBufferBusy = true;
            result = string.Empty;
            Clock = 0;
            InstructionLatest = -1;
            comment = new string[10];
            fullComment = string.Empty;
            numOfUsedCDBCycles = 0;
            DependentIns = new string[10];
            numOfDependentIns = new int[10];
            CDBCycle = new int[10];
            IsCDBCycle = true;
            f = 0;
            DifferentFUCounter = 0;
            SameFUCounter = 0;
            IssuedInstCounter = 0;
            IsInstructionIssued = true;
            ThreeIssueInstHelperFlag = false;
            NumOfCommitedIns = 0;
            CommitOrNot = false;
            IssuePhaseCycles = 0;
            NumOfCommitedIns = 0;
        }


        //============================================================
        // Function name   : Commit
        // Programmer      : Liron Shamir
        // Description     : Handling Commit phase for given instruction number
        // Return type     : bool 
        // Argument        : int instructionIdx
        // Argument        : int numOfInsPerCycle
        // Argument        : int IterationNum
        // Argument        : int clock
        // Argument        : int numOfInsToCDB
        // Argument        : int numofInsToCommit
        //============================================================
        public bool Commit(int instructionIdx, int numOfInsPerCycle, int IterationNum, int clock, int numOfInsToCDB, int numofInsToCommit)
        {
            string Register;
            bool isCommit = false;

            if (instructionIdx == 0)
            {
                isCommit = true;
            }
            else
            {
                if (ReorderBuffer.IsCommitFinished(ReorderBuffer.GetIndex(currentInstFullName)))
                {
                    isCommit = true;
                }
                else
                {
                    isCommit = false;
                }
            }


            if (NumOfCommitedIns < numofInsToCommit)
            {
                if (isCommit)
                {
                    if (ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][3].ToString().Equals("Write CDB"))
                    {
                        if (clock > int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][5].ToString()))
                        {
                            ReorderBuffer.Update(ReorderBuffer.GetIndex(ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][2].ToString()), 
                                false, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][2].ToString(), 
                                "Commit", ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][4].ToString(), 
                                ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][5].ToString(),
                                ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][6].ToString());

                            Register = ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][4].ToString();

                            for (int j = 0; j < RegisterResultStatus.RegisterResultStatusDT().Columns.Count; j++)
                            {
                                if (string.Compare(Register, RegisterResultStatus.RegisterResultStatusDT().Columns[j].ColumnName) == 0)
                                {
                                    RegisterResultStatus.Delete(j);
                                }
                            }

                            InstructionStatusManager.Update(instructionIdx, 
                                IterationNum, 
                                currentInstFullName, 
                                int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][2].ToString())), 
                                int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][3].ToString())), 
                                int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][4].ToString())), 
                                int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][5].ToString())), 
                                clock, 
                                string.Empty);

                            if (ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][6] != DBNull.Value)
                            {
                                RegisterResultStatus.UpdateRegValue(
                                    Register, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][6].ToString().Split(new char[] { '=' })[1].Trim());
                                
                            }
           

                            NumOfCommitedIns++;
                        }
                    }
                    else
                    {
                        currentInstType = InstructionSet.GetInstruction(InstructionFromInput.InstructionsFromInputDT().Rows[instructionIdx]["Instruction Name"].ToString());
                        if (ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][3].ToString().Equals("Execute"))
                        {
                            if (currentInstType.Equals("LD/SD") || currentInstType.Equals("Branch"))
                            {
                                if (clock > int.Parse(InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][3].ToString()) && InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][7].ToString() == string.Empty)
                                {
                                    ReorderBuffer.Update(ReorderBuffer.GetIndex(ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][2].ToString()), false, ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][2].ToString(), "Commit", ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][4].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][5].ToString(), ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][6].ToString());
                                    Register = ReorderBuffer.ReorderBufferDT().Rows[instructionIdx][4].ToString();

                                    for (int j = 0; j < RegisterResultStatus.RegisterResultStatusDT().Columns.Count; j++)
                                    {
                                        if (string.Compare(Register, RegisterResultStatus.RegisterResultStatusDT().Columns[j].ColumnName) == 0)
                                        {
                                            RegisterResultStatus.Delete(j);
                                        }
                                    }

                                    InstructionStatusManager.Update(instructionIdx, IterationNum, currentInstFullName, int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][2].ToString())), int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][3].ToString())), int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][4].ToString())), int.Parse((InstructionStatusManager.ExecutionManagerDT().Rows[instructionIdx][5].ToString())), clock, string.Empty);
                                    NumOfCommitedIns++;
                                }
                            }
                        }
                    }
                }
            }


            return true;
        } 
        #endregion

    }
}
