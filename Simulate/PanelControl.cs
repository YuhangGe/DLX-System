using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace Simulate
{
    class MemoryPanelControl
    {
        private static MemoryPanelControl instance;

        private UInt32 location;
        private UInt32 currentLine;

        MemoryTree memoryTree;

        protected MemoryPanelControl() 
        {
            this.currentLine = 0;
            memoryTree = new MemoryTree();
        }
        public static MemoryPanelControl getInstance()
        {
            if (instance == null)
                instance = new MemoryPanelControl();
            return instance;
        }
        /* 仅调用memoryTree.ReSet()不足以完成机器的初始化
         * 模拟器初始化还由ConsolePanel中定义的函数ConsolePanel_Loaded协助完成
         * 此函数将DSR所对应的内存地址设为1.
         * 由于这个函数触发时,用户界面应当已经初始化完成,因此可以安全地将这个内存赋值语句
         * 添加在这里.
         * Shore Ray */
        public void ReSet()
        {
            this.memoryTree.ReSet();
            CPUInfo.getInstance().computer.memory[SmallTool.UinttoInt(4294901771)] |= 1;
            ChildFormControl.getInstance().update();
        }
        public void setPcCurrent()
        {
            CPUInfo.getInstance().setPC(SmallTool.UinttoInt(this.currentLine));
        }
        public UInt32 getLocation()
        {
            return this.location;
        }
        public void setLoction(UInt32 loc)
        {
            this.location = loc;
        }
        public void deltaLoction(int off)
        {
            double c = this.location;
            c += (double)off;
            this.location = (UInt32)c;
        }
        public UInt32 getCurrentLint()
        {
            return this.currentLine;
        }
        public void setCurrentLine(UInt32 cl)
        {
            this.currentLine = cl;
        }
        public void deltaCurrentLine(int off, int N)
        {
            double c = this.currentLine;
            c += (double)off;
            this.currentLine = (UInt32)c;
        }
        
        public void addBreakPoint(UInt32 location) { }
        public void removeBreakPoint(UInt32 location) { }

        public StringItem[,] getValue(int N)
        {
            StringItem[,] items = new StringItem[N, 7];
            for (int i = 0; i < N; i++)
            {
                MemoryTreeNode mt= this.memoryTree.search((UInt32)(this.location + i*4));
                MemoryInformation mi = mt.info;
                items[i, 0] = mi.loction;
                items[i, 1] = mi.byteValue[0];
                items[i, 2] = mi.byteValue[1];
                items[i, 3] = mi.byteValue[2];
                items[i, 4] = mi.byteValue[3];
                items[i, 5] = mi.hexValue;
                items[i, 6] = mi.insValue;
            }
            return items;
        }
        public bool[,] getLight(int N)
        {
            List<UInt32> lights = CPUInfo.getInstance().getMemoryLight();
            bool[,] isLight = new bool[N, 4];
            for (UInt32 i = 0; i < N * 4; i++)
            {
                if (lights.IndexOf((this.location + i)) != -1)
                    isLight[i / 4, i % 4] = true;
                else
                    isLight[i / 4, i % 4] = false;
            }
            return isLight;
        }
        public int[] getSign(int N)
        {
            int[] signs = new int[N];
            ArrayList breakpoints = CPUInfo.getInstance().getBreakpoints();
            UInt32 pc = SmallTool.InttoUint((CPUInfo.getInstance().getPC()));
            for (UInt32 i = 0; i < N; i++)
            {
                if (pc == this.location + i * 4)
                    if (breakpoints.IndexOf(SmallTool.UinttoInt(this.location + i * 4)) != -1)
                        signs[i] = 3;
                    else
                        signs[i] = 2;
                else
                    if (breakpoints.IndexOf(SmallTool.UinttoInt(this.location + i * 4)) != -1)
                        signs[i] = 1;
                    else
                        signs[i] = 0;
            }
            return signs;
        }
        public int getChoose(int N) 
        {
            int s = (int)((this.currentLine - this.location)/4);
            if (s >= N)
                s = -1;            
            return s;         
        }        
    }

    class RegisterPanelControl
    {
        private static RegisterPanelControl instance;
        protected RegisterPanelControl()
        {
        }
        public static RegisterPanelControl getInstance()
        {
            if (instance == null)
                instance = new RegisterPanelControl();
            return instance;
        }
        public int[] getValue()
        {
            return CPUInfo.getInstance().getRegisterValue();
        }
        public bool[] getLight()
        {
            int n=40;
            bool[] a = new bool[n];
            List<int> lightRegister = CPUInfo.getInstance().getRegisterLight();
            for (int i = 0; i < n; i++)
                if (lightRegister.IndexOf(i - 2) != -1)
                    a[i] = true;
                else
                    a[i] = false;
            return a;
        }
    }
}
