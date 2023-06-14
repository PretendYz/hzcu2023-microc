# **2022-23学年第2学期**

## **实 验 报 告**

![zucc](assets/zucc.png){width="1.5208333333333333in" height="1.5208333333333333in"}

- 课程名称: <u>编程语言原理与编译</u>
- 实验项目: <u>期末大作业</u>
- 专业班级: <u>计算机2004</u>
- 学生学号: <u>32003007</u> <u>32004151</u>
- 学生姓名: <u>姜土根</u> <u>徐韩</u>
- 实验指导教师: <u>郭鸣</u>

| 姓名   | 学号     | 班级       | 任务               | 权重 |
| ------ | -------- | ---------- | ------------------ | ---- |
| 姜土根 | 32003007 | 计算机2004 | 解释器、测试、文档 |      |
| 徐韩   | 32004151 | 计算机2004 | 编译器、测试、文档 |      |

成员代码提交日志



### 1. 项目自评等级:(1-5) 请根据自己项目情况填写下表

（完善程度：个人认为实现的完善程度，个人花费的工作量等
	难度：实现的困难程度，工作量大小，老师定义的困难程度等

| 解释器        | 完善程度 | 难度 | 备注                         |
| ------------- | -------- | ---- | ---------------------------- |
| 三元运算符 ?: | ⭐⭐⭐      | 1    | test_interp/threeOperation.c |
| bool类型      | ⭐⭐⭐⭐     | 2    | test_interp/print.c          |
| char类型      | ⭐⭐       | 1    | test_interp/print.c          |
| float类型     | ⭐⭐⭐⭐     | 2    | test_interp/print.c          |
| string类型    | ⭐⭐⭐⭐     | 2    | test_interp/print.c          |
| for语句       | ⭐⭐⭐      | 2    | test_interp/for.c            |
| += 等语法糖   | ⭐⭐⭐⭐⭐    | 1    | test_interp/self_assign.c    |
| dowhile语句   | ⭐⭐⭐      | 1    | test_interp/dowhile.c        |
| dountil语句   | ⭐⭐⭐      | 1    | test_interp/dowhile.c        |
| print格式输出 | ⭐⭐       | 1    | test_interp/print.c          |
| sizeof 计算   | ⭐⭐⭐      | 2    | test_interp/sizeof.c         |



| 编译器 | 完善程度 | 难度 | 备注      |
| ------ | -------- | ---- | --------- |
|        |          |      | test_comp |
|        |          |      | test_comp |
|        |          |      | test_comp |
|        |          |      | test_comp |
|        |          |      | test_comp |
|        |          |      | test_comp |

### 2. 项目说明

- 整体文件架构

  |       文件        |        描述        |
  | :---------------: | :----------------: |
  | test_interp文件夹 |  解释器例子程序集  |
  |  test_comp文件夹  |  编译器例子程序集  |
  |     Absyn.fs      |      抽象语法      |
  |    Backend.fs     |     编译器后端     |
  |     CLex.fsl      |   fslex词法定义    |
  |      Comp.fs      |       编译器       |
  |    Contcomp.fs    |     优化编译器     |
  |     CPar.fsy      |   fsyacc语法定义   |
  |    grammar.txt    |      文法定义      |
  |     Interp.fs     |       解释器       |
  |  interpc.fsproj   |   解释器项目文件   |
  |     machine.c     |     VM 实现 c      |
  |    Machine.cs     |     VM 实现 c#     |
  |  machine.csproj   |    VM 项目文件     |
  |   Machine.java    |    VM 实现 java    |
  |   microc.fsproj   |   编译器项目文件   |
  |  microcc.fsproj   | 优化编译器项目文件 |
  |     Parse.fs      |     语法解析器     |
  |  StackMachine.fs  |    VM 指令定义     |
  |   file_desc.md    |      文件说明      |

  ​    

- 项目运行

  **解释器：**

  ```sh
  # 编译解释器
  dotnet restore interpc.fsproj     # 可选
  dotnet clean interpc.fsproj  	  # 可选
  dotnet build  interpc.fsproj      # 构建
  # dotnet build -v n interpc.fsproj 
  # -v n查看详细生成过程
  
  # 执行解释器
  # ./bin/Debug/net6.0/interpc.exe test_interp/xxx.c [args]
  dotnet run --project interpc.fsproj test_interp/xxx.c [args]
  # dotnet run --project interpc.fsproj -g test_interp/xxx.c [args] 
  # -g 显示token AST 等调试信息  
  ```
  
  **编译器：**
  
  ```sh
  # 构建 microc.exe 编译器程序 
  dotnet restore  microc.fsproj # 可选
  dotnet clean  microc.fsproj   # 可选
  dotnet build  microc.fsproj   # 构建编译器
  
  # 执行编译器，编译 xxx.c，并输出 xxx.out 文件
  # ./bin/Debug/net6.0/microc.exe test_comp/xxx.c [args]
  dotnet run --project microc.fsproj test_comp/xxx.c [args]
  # dotnet run --project microc.fsproj -g test_comp/xxx.c [args]
  # -g 查看调试信息
  ```

  **优化编译器**
  
  ```sh
  dotnet restore  microcc.fsproj # 可选
  dotnet clean  microcc.fsproj   # 可选
  dotnet build  microcc.fsproj   # 构建优化编译器
  
  dotnet run --project microcc.fsproj test_comp/xxx.c [args]    # 执行优化编译器
  ./bin/Debug/net6.0/microcc.exe test_comp/xxx.c [args]         # 直接执行
  ```
  
  **C 虚拟机：**
  
  ```sh
  # 编译 c 虚拟机
  gcc -o machine.exe machine.c
  # 虚拟机执行指令
  machine.exe test_comp/xxx.out [args]
  
  # 调试执行指令
  machine.exe -trace test_comp/xxx.out [args]  # -trace  并查看跟踪信息
  ```
  
  1. 解释器
  2. 编译器

### 3. 解决技术要点说明

#### 3.1 解释器重构



#### 3.2 



### 4. 心得体会（结合自己情况具体说明）

- 大项目开发过程心得

  姜土根

  

  徐韩

  

- 本课程建议
  
  姜土根
  
  
  
  徐韩
  
  
  
  