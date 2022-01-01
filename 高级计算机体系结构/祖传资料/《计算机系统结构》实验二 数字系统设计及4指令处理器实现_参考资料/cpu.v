// vsim -novopt work.test_computer
// run -all
module mem(addr,data);
    input [5:0] addr;
    output [7:0] data;
    reg [7:0] data;
    
    always @(addr)
    begin
       case(addr)
          0:data = 8'b00_111111;   //add mem[63]
          1:data = 8'b11_000000;   //inc 
          2:data = 8'b01_111110;   //and mem[62]
          3:data = 8'b10_000000;   //jmp 0
          62:data = 8'b0000_1111;  //mem[62] = 0000_1111
          63:data = 8'd2;          //mem[63] = 2
          default:data = 8'd0;
        endcase 
    end
endmodule

module ar(clk,load,din,dout);
   input clk,load;
   input [5:0] din;
   output [5:0] dout;
   
   reg [5:0] dout;
   
   always @(posedge clk)
   begin
      if(load == 1)
         dout = din;
   end
endmodule

module pc(clk,rst,inc,load,din,dout);
   input clk,load,rst,inc;
   input [5:0] din;
   output [5:0] dout;
   
   reg [5:0] dout;
   
   always @(posedge clk)
   begin
      if(rst == 1)
         dout = 0;
      else if(inc == 1)
         dout = dout + 1;
      else if(load == 1)
         dout = din;
   end
endmodule

module dr(clk,load,din,dout);
   input clk,load;
   input [7:0] din;
   output [7:0] dout;
   
   reg [7:0] dout;
   
   always @(posedge clk)
   begin
      if(load == 1)
         dout = din;
   end
endmodule

module ir(clk,load,din,dout);
   input clk,load;
   input [1:0] din;
   output [1:0] dout;
   
   reg [1:0] dout;
   
   always @(posedge clk)
   begin
      if(load == 1)
         dout = din;
   end
endmodule

module ac(clk,rst,inc,load,din,dout);
   input clk,load,rst,inc;
   input [7:0] din;
   output [7:0] dout;
   
   reg [7:0] dout;
   
   always @(posedge clk)
   begin
      if(rst == 1)
         dout = 0;
      else if(inc == 1)
         dout = dout + 1;
      else if(load == 1)
         dout = din;
   end
endmodule

module alu(op,a,b,c);
   input op;
   input [7:0] a,b;
   output [7:0] c;
   
   assign c = (op == 0)?(a + b):(a & b);
endmodule

module tri6(enable,din,dout);
   input enable;
   input [5:0] din;
   output [5:0] dout;
   
   assign dout = (enable == 1)?din:6'bzzz_zzz;
   
endmodule


module tri8(enable,din,dout);
   input enable;
   input [7:0] din;
   output [7:0] dout;
   
   assign dout = (enable == 1)?din:8'bzzz_zzz;
   
endmodule



`define FETCH1 1
`define FETCH2 2
`define FETCH3 3
`define DECODE 4
`define ADD1 5
`define ADD2 6
`define AND1 7
`define AND2 8
`define JMP1 9
`define INC1 10

`define OP_ADD 2'b00
`define OP_AND 2'b01
`define OP_JMP 2'b10
`define OP_INC 2'b11


module controller(clk,rst,opcode,
   membus,
   arload,
   pcload,pcinc,pcbus,
   drload,drbus,
   alusel,
   acload,acinc,
   irload
);

   input clk,rst;
   input [1:0] opcode;
   
   output membus,
   arload,
   pcload,pcinc,pcbus,
   drload,drbus,
   alusel,
   acload,acinc,
   irload;
   
   reg [3:0] state,next_state;

   assign membus = (state == `FETCH2 || state == `ADD1 || state == `AND1)?1:0;
   
   assign arload = (state == `FETCH1 || state == `FETCH3)?1:0;
   
   assign pcload = (state == `JMP1)?1:0;
   assign pcinc = (state == `FETCH2)?1:0;
   assign pcbus = (state == `FETCH1 )?1:0;   
   
   assign drload = (state == `FETCH2 || state == `ADD1 || state == `AND1)?1:0;
   assign drbus = (state == `FETCH3 || state == `ADD2 || state == `AND2 || state == `JMP1)?1:0;
   
   assign alusel = (state == `AND2)?1:0;
   
   assign acload = (state == `ADD2 || state == `AND2)?1:0;
   assign acinc = (state == `INC1)?1:0;
   
   assign irload = (state == `FETCH3)?1:0;

   
   always @(posedge clk)
   begin
      if(rst == 1)
         state = `FETCH1;
       else
          state = next_state;
   end
   
   always @(state or opcode)
   begin
       case(state)
          `FETCH1:next_state = `FETCH2;
          `FETCH2:next_state = `FETCH3;
          `FETCH3:next_state = `DECODE;
          `DECODE:
          begin
            case(opcode)
               `OP_ADD:next_state = `ADD1;
               `OP_AND:next_state = `AND1;
               `OP_JMP:next_state = `JMP1;
               `OP_INC:next_state = `INC1;
               default:next_state = `FETCH1;
            endcase
          end
          `ADD1:next_state = `ADD2;
          `ADD2:next_state = `FETCH1;
          `AND1:next_state = `AND2;
          `AND2:next_state = `FETCH1;
          `JMP1:next_state = `FETCH1;
          `INC1:next_state = `FETCH1;
          default:next_state = `FETCH1;
       endcase
   end
endmodule

module cpu(clk,rst,mem_addr,mem_data);
    input clk,rst;
    output [5:0] mem_addr;
    input [7:0] mem_data;
    
    wire [7:0] bus;
    wire membus,
         arload,
         pcload,pcinc,pcbus,
         drload,drbus,
         alusel,
         acload,acinc,
         irload;
   
    
   
   tri8 t0(membus,mem_data,bus);
   
   ar ar1(clk,arload,bus[5:0],mem_addr);
   
   wire [5:0] pc_out;
   pc pc1(clk,rst,pcinc,pcload,bus[5:0],pc_out);
   tri6 t1(pcbus,pc_out,bus[5:0]);
   
   wire [7:0] dr_out;
   dr dr1(clk,drload,bus,dr_out);
   tri8 t2(drbus,dr_out,bus);
    
   wire [7:0] ac_out,alu_out;
   alu alu1(alusel,ac_out,bus,alu_out);
   ac ac1(clk,rst,acinc,acload,alu_out,ac_out);
   
   wire [1:0] opcode;
   ir ir1(clk,irload,bus[7:6],opcode);
   
   controller c1(clk,rst,opcode,
      membus,
      arload,
      pcload,pcinc,pcbus,
      drload,drbus,
      alusel,
      acload,acinc,
      irload);
   
endmodule

module computer(clk,rst);
   input clk,rst;
   
   wire [5:0] addr;
   wire [7:0] data;
   
   mem mem1(addr,data);
   cpu cpu1(clk,rst,addr,data);
endmodule

module test_computer();
    
    reg clk,rst;
    
    computer c(clk,rst);
    
    initial
    begin
       clk = 0;
       rst = 0;
       
       #5 
       rst = 1;
       
       #10
       rst = 0;
       
       #1000
       $stop();
           
    end
    
    always
    begin
       #10
       clk = ~clk; 
    end
    
    always @(posedge clk)
    begin
       $display("state=%d,mem_addr=%d,mem_data=%d,bus=%x,dr=%x\n",c.cpu1.c1.state,c.cpu1.mem_addr,c.cpu1.mem_data,c.cpu1.bus,c.cpu1.dr_out); 
    end
    
endmodule