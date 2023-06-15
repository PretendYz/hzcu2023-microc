(* File MicroC/Interp.c
   Interpreter for micro-C, a fraction of the C language
   sestoft@itu.dk * 2010-01-07, 2014-10-18

   A value is an integer; it may represent an integer or a pointer,
   where a pointer is just an address in the store (of a variable or
   pointer or the base address of an array).  The environment maps a
   variable to an address (location), and the store maps a location to
   an integer.  This freely permits pointer arithmetics, as in real C.
   Expressions can have side effects.  A function takes a list of
   typed arguments and may optionally return a result.

   For now, arrays can be one-dimensional only.  For simplicity, we
   represent an array as a variable which holds the address of the
   first array element.  This is consistent with the way array-type
   parameters are handled in C (and the way that array-type variables
   were handled in the B language), but not with the way array-type
   variables are handled in C.

   The store behaves as a stack, so all data are stack allocated:
   variables, function parameters and arrays.

   The return statement is not implemented (for simplicity), so all
   functions should have return type void.  But there is as yet no
   typecheck, so be careful.
 *)

module Interp

open Absyn
open Debug
(* ----------------------------------------------------------------- *)

(* Simple environment operations *)
// 多态类型 env
// 环境 env 是 元组 ("name",data) 的列表 ，名称是字符串 string 值 'data 可以是任意类型
// 名称 ---> 数据 名称与数据绑定关系的 键-值 对  key-value pairs
// [("x",9);("y",8)]: int env

type 'data env = (string * 'data) list

//环境查找函数
//在环境 env上查找名称为 x 的值
let rec lookup env x =
    match env with
    | [] -> failwith (x + " not found")
    | (y, v) :: yr -> if x = y then v else lookup yr x

(* A local variable environment also knows the next unused store location *)

// ([("x",9);("y",8)],10)
// x 在位置9,y在位置8,10--->下一个空闲空间位置10
type locEnv = int env * int

(* A function environment maps a function name to parameter list and body *)
//函数参数例子:
//void func (int a , int *p)
// 参数声明列表为: [(TypI,"a");(TypP(TypI) ,"p")]
type paramdecs = (typ * string) list

(* 函数环境列表
  [("函数名", ([参数元组(类型,"名称")的列表],函数体AST)),....]

  //main (i){
  //  int r;
  //    fac (i, &r);
  //    print r;
  // }
  [ ("main",
   ([(TypI, "i")],
    Block
      [Dec (TypI,"r");
       Stmt (Expr (Call ("fac",[Access (AccVar "i"); Addr (AccVar "r")])));
       Stmt (Expr (Prim1 ("printi",Access (AccVar "r"))))]))]

函数环境 是 多态类型  'data env ---(string * 'data ) list 的一个 具体类型 ⭐⭐⭐
    类型变量 'data  具体化为  (paramdecs * stmt)
    (string * (paramdecs * stmt)) list
*)

type funEnv = (paramdecs * stmt) env

(* A global environment consists of a global variable environment
   and a global function environment
 *)

// 全局环境是 变量声明环境 和 函数声明环境的元组
// 两个列表的元组
// ([var declares...],[fun declares..])
// ( [ ("x" ,1); ("y",2) ], [("main",mainAST);("fac",facAST)] )
// mainAST,facAST 分别是main 与fac 的抽象语法树

type gloEnv = int env * funEnv

(* The store maps addresses (ints) to values (ints): *)

//地址是store上的的索引值
type address = int

// store 是一个 地址到值的映射，是对内存的抽象 ⭐⭐⭐
// store 是可更改的数据结构，特定位置的值可以修改，注意与环境的区别
// map{(0,3);(1,8) }
// 位置 0 保存了值 3
// 位置 1 保存了值 8

// 枚举支持store存储的类型
type enumType =
  | INT of int                  // 整数
  | FLOAT of float              // 浮点数
  | DOUBLE of double            // 双精度浮点数
  | CHAR of char                // 字符
  | STRING of string            // 字符串
  | BOOLEAN of bool             // 布尔值
  | POINTER of int              // 指针
  | ARRAY of typ*int*int        // 二维数组
//   | STRUCT of string*int*int    // 结构体
  member this.int = 
    match this with 
    | INT i -> i
    | POINTER i -> i
    | FLOAT f -> int f
    | DOUBLE d -> int d
    | CHAR c -> int c
    | BOOLEAN b -> if b then 1 else 0
    | ARRAY (typ, i, size) -> i    // 不太理解
    | _ -> failwith("not int")
  
  member this.string = 
    match this with 
    | STRING s -> s
    | _ -> failwith("not string")
  
  member this.char = 
    match this with 
    | CHAR c -> c
    | INT i -> char i
    | _ -> failwith("not char")

  member this.float = 
    match this with 
    | FLOAT f -> f
    | INT i -> float i
    | DOUBLE i -> float i
    | _ -> failwith("not float")

  member this.double = 
    match this with 
    | DOUBLE d -> d
    | INT i -> double i
    | _ -> failwith("not double")

  member this.boolean = 
    match this with 
    | BOOLEAN b -> b
    | _ -> failwith("not boolean")

  member this.pointer = 
    match this with 
    | POINTER i -> i
    | INT i -> i
    | _ -> failwith("not pointer")

  member this.checktype =
    match this with 
    | INT i -> TypI
    | FLOAT f -> TypF
    | DOUBLE d -> TypD
    | CHAR c -> TypC
    | BOOLEAN b -> TypB
    | STRING s -> TypS
    | ARRAY(typ,i,size) ->  TypA(typ,Some size)
    // | STRUCT (s,i,size) -> TypeStruct s
    | _ -> failwith("error")

type store = Map<address, enumType>

// type store = Map<address, int>

//空存储
let emptyStore = Map.empty<address, enumType>

//保存value到存储store
let setSto (store: store) addr value = store.Add(addr, value)

//输入addr 返回存储的值value
let getSto (store: store) addr = store.Item addr

// store上从loc开始分配n个值的空间
// 用于数组分配
let rec initSto loc n store =
    if n = 0 then
        store
    else // 默认值 0
        initSto (loc + 1) (n - 1) (setSto store loc (INT 0))

(* Combined environment and store operations *)

(* Extend local variable environment so it maps x to nextloc
   (the next store location) and set store[nextloc] = v.

locEnv结构是元组 : (绑定环境env,下一个空闲地址nextloc)
store结构是Map<string,int>

扩展环境 (x nextloc) :: env ====> 新环境 (env1,nextloc+1)
变更store (nextloc) = v
 *)

// 绑定一个值 x,v 到环境
// 环境是非更改数据结构，只添加新的绑定（变量名称，存储位置），注意与store 的区别⭐⭐⭐
// 返回新环境 locEnv,更新store,
// nextloc是store上下一个空闲位置
(*

// variable.c
int g ;
int h[3];
void main (int n){
n = 8;
}
上面c程序的解释环境如下：

 环境：locEnv:
    ([(n, 5); (n, 4); (g, 0)], 6)

存储：store:
    (0, 0)  (1, 0)(2, 0)(3, 0)(4, 1)  (5, 8)
     ^^^^    ^^^^^^^^^^^^^^^^^^^^^^    ^^^^
       g               h                n

   变量 地址 值
   n--->5--->8
   h--->4--->1
   g--->0--->0

   下一个待分配位置是 6
*)

//将多个值 xs vs绑定到环境
//遍历 xs vs 列表,然后调用 bindVar实现单个值的绑定
let store2str store =
    String.concat "" (List.map string (Map.toList store))

let bindVar x v (env, nextloc) store : locEnv * store =
    let env1 = (x, nextloc) :: env
    msg $"bindVar:\n%A{env1}\n"

    //返回新环境，新的待分配位置+1，设置当前存储位置为值 v
    let ret = ((env1, nextloc + 1), setSto store nextloc v)

    msg $"locEnv:\n {fst ret}\n"
    msg $"Store:\n {store2str (snd ret)}\n"

    ret


let rec bindVars xs vs locEnv store : locEnv * store =
    let res =
        match (xs, vs) with
        | ([], []) -> (locEnv, store)
        | (x1 :: xr, v1 :: vr) ->
            let (locEnv1, sto1) = bindVar x1 v1 locEnv store
            bindVars xr vr locEnv1 sto1
        | _ -> failwith "parameter/argument mismatch"

    msg "\nbindVars:\n"
    msg $"\nlocEnv:\n{locEnv}\n"
    msg $"\nStore:\n"
    store2str store |> msg
    res
(* Allocate variable (int or pointer or array): extend environment so
   that it maps variable to next available store location, and
   initialize store location(s).
 *)
//

let rec allocate (typ, x) (env0, nextloc) sto0 : locEnv * store =

    let (nextloc1, v, sto1) =
        match typ with
        //数组 调用 initSto 分配 i 个空间
        | TypA(t, Some i) -> (nextloc + i, nextloc, initSto nextloc i sto0)
        // 常规变量默认值是 0
        | _ -> (nextloc, 0, sto0)

    msg $"\nalloc:\n {((typ, x), (env0, nextloc), sto0)}\n"
    bindVar x (getSto sto0 v) (env0, nextloc1) sto1

(* Build global environment of variables and functions.  For global
   variables, store locations are reserved; for global functions, just
   add to global function environment.
*)

//初始化 解释器环境和store
let initEnvAndStore (topdecs: topdec list) : locEnv * funEnv * store =

    //包括全局函数和全局变量
    msg $"\ntopdecs:\n{topdecs}\n"

    let rec addv decs locEnv funEnv store =
        match decs with
        | [] -> (locEnv, funEnv, store)

        // 全局变量声明  调用allocate 在store上给变量分配空间
        | Vardec(typ, x) :: decr ->
            let (locEnv1, sto1) = allocate (typ, x) locEnv store
            addv decr locEnv1 funEnv sto1

        //全局函数 将声明(f,(xs,body))添加到全局函数环境 funEnv
        | Fundec(_, f, xs, body) :: decr -> addv decr locEnv ((f, (xs, body)) :: funEnv) store

    // ([], 0) []  默认全局环境
    // locEnv ([],0) 变量环境 ，变量定义为空列表[],下一个空闲地址为0
    // ([("n", 1); ("r", 0)], 2)  表示定义了 变量 n , r 下一个可以用的变量索引是 2
    // funEnv []   函数环境，函数定义为空列表[]
    addv topdecs ([], 0) [] emptyStore

(* ------------------------------------------------------------------- *)

(* Interpreting micro-C statements *)

let rec exec stmt (locEnv: locEnv) (gloEnv: gloEnv) (store: store) : store =
    match stmt with
    | If(e, stmt1, stmt2) ->
        let (v, store1) = eval e locEnv gloEnv store

        if v.int <> 0 then
            exec stmt1 locEnv gloEnv store1 //True分支
        else
            exec stmt2 locEnv gloEnv store1 //False分支

    | While(e, body) ->
        //定义 While循环辅助函数 loop
        let rec loop store1 =
            //求值 循环条件,注意变更环境 store
            let (v, store2) = eval e locEnv gloEnv store1
            // 继续循环
            if v.int <> 0 then
                loop (exec body locEnv gloEnv store2)
            else
                store2 //退出循环返回 环境store2

        loop store
    | For (e1, e2, e3, body) ->
        //定义 For循环辅助函数 loop
        let rec loop store1 =
            //求值 循环条件,注意变更环境 store
            let (v, store2) = eval e2 locEnv gloEnv store1
            // 继续循环
            if v.int <> 0 then
                let (_, store3) = eval e3 locEnv gloEnv (exec body locEnv gloEnv store2)
                loop store3
            else
                store2 //退出循环返回 环境store2
        
        let (_, store1) = eval e1 locEnv gloEnv store
        loop store1

    | DoWhile (e, body) ->
        // 先执行一遍body
        let store1=exec body locEnv gloEnv store 
        // 然后根据判定值执行While循环辅助函数 loop
        exec (While(e, body)) locEnv gloEnv store1

    | DoUntil (e, body) ->
        let rec loop store1 =
            let (v, store2) = eval e locEnv gloEnv store1
            if v.int=0 then loop (exec body locEnv gloEnv store2)
            else store2    
        loop (exec body locEnv gloEnv store)
    
    | Break -> failwith "return not implemented" // 解释器没有实现 break

    | Continue -> failwith "return not implemented" // 解释器没有实现 continue

    | Switch(e,body) ->
        let (res, store1) = eval e locEnv gloEnv store
        let rec choose list =
            match list with
            // 自带break的switch
            | Case(e1,body1) :: tail -> 
                let (res2, store2) = eval e1 locEnv gloEnv store1
                if res2=res then   
                    exec body1 locEnv gloEnv store2
                else choose tail
            | [] -> store1
            | Default(body1) :: tail -> 
                let _ = exec body1 locEnv gloEnv store1
                choose tail
            | _ -> failwith "switch only case or default"
        (choose body)

    | Case(e,body) -> exec body locEnv gloEnv store

    | Expr e ->
        // _ 表示丢弃e的值,返回 变更后的环境store1
        let (_, store1) = eval e locEnv gloEnv store
        store1

    | Block stmts ->

        // 语句块 解释辅助函数 loop
        let rec loop ss (locEnv, store) =
            match ss with
            | [] -> store
            //语句块,解释 第1条语句s1
            // 调用loop 用变更后的环境 解释后面的语句 sr.
            | s1 :: sr -> loop sr (stmtordec s1 locEnv gloEnv store)

        loop stmts (locEnv, store)
    | Return _ -> failwith "return not implemented" // 解释器没有实现 return
    | Default(_) -> failwith "default Not Implemented" 

and stmtordec stmtordec locEnv gloEnv store =
    match stmtordec with
    | Stmt stmt -> (locEnv, exec stmt locEnv gloEnv store)
    | Dec(typ, x) -> allocate (typ, x) locEnv store

(* Evaluating micro-C expressions *)

and eval e locEnv gloEnv store : enumType * store = 
    match e with
    | Sizeof e -> let (res,s1) = eval e locEnv gloEnv store
                  match res with
                //   | ARRAY (typ, i, size) -> (INT size, s1) // 仅支持存储int
                  | STRING s -> (INT s.Length, s1)
                  | _ -> (INT 1,s1)
    | Typeof e -> let (res,s) = eval e locEnv gloEnv store
                  match res.checktype with
                  | TypB   -> (STRING "bool",s)
                  | TypI   -> (STRING "int",s)
                  | TypP i -> (STRING "pointer",s)
                  | TypC   -> (STRING "char",s)
                  | TypS   -> (STRING "string",s)
                  | TypF   -> (STRING "float",s)
                  | TypD   -> (STRING "double",s)
                  | TypA (typ,i) -> (STRING "array",s)
    | Access acc ->
        let (loc, store1) = access acc locEnv gloEnv store
        (getSto store1 loc, store1)
    | Assign(acc, e) ->
        let (loc, store1) = access acc locEnv gloEnv store
        let (res, store2) = eval e locEnv gloEnv store1
        (res, setSto store2 loc res)
    | CstI i -> (INT i, store)
    | CstB b -> (BOOLEAN b, store)
    | CstS s -> (STRING s, store)
    | CstF f -> (FLOAT (float f), store)
    | CstD d -> (DOUBLE (double d), store)
    | CstC c -> (CHAR c, store)
    | Addr acc -> 
        let (acc1,s) = access acc locEnv gloEnv store
        (POINTER acc1, s)
    | Print(op,e1) -> 
        let (i1, store1) = eval e1 locEnv gloEnv store
        let res = 
            match op with
            | "%c"   -> (printf "%c " i1.char; i1)
            | "%d"   -> (printf "%d " i1.int; i1)  
            | "%f"   -> (printf "%f " i1.float; i1 )
            | "%F"   -> (printf "%F " i1.double; i1 )
            | "%s"   -> (printf "%s " i1.string; i1 )
            | _      -> failwith ("wrong format")
        (res, store1)
    | Prim1(ope, e1) ->
        let (i1, store1) = eval e1 locEnv gloEnv store

        let res =
            match ope with
            | "!" -> if i1.int = 0 then BOOLEAN true else BOOLEAN false
            | _ -> failwith ("unknown primitive " + ope)
        (res, store1)

    | Prim2(ope, e1, e2) ->
        let (enumType1, store1) = eval e1 locEnv gloEnv store
        let (enumType2, store2) = eval e2 locEnv gloEnv store1
        let i1 = enumType1.float
        let i2 = enumType2.float
        let res =
            match ope with
            | "*"  -> if enumType1.checktype = TypF || enumType2.checktype = TypF then  FLOAT (float (i1 + i2)) else INT (int (i1 * i2) )
            | "+"  -> if enumType1.checktype = TypF || enumType2.checktype = TypF then  FLOAT (float (i1 + i2)) else INT (int (i1 + i2) )
            | "-"  -> if enumType1.checktype = TypF || enumType2.checktype = TypF then  FLOAT (float (i1 - i2)) else INT (int (i1 - i2) )  
            | "/"  -> if enumType1.checktype = TypF || enumType2.checktype = TypF then  FLOAT (float (i1 / i2)) else INT (int (i1 / i2) )  
            | "%"  -> if enumType1.checktype = TypF || enumType2.checktype = TypF then  FLOAT (float (i1 % i2)) else INT (int (i1 % i2) )  
            | "==" -> if i1 =  i2 then BOOLEAN true else BOOLEAN false
            | "!=" -> if i1 <> i2 then BOOLEAN true else BOOLEAN false
            | "<"  -> if i1 <  i2 then BOOLEAN true else BOOLEAN false
            | "<=" -> if i1 <= i2 then BOOLEAN true else BOOLEAN false
            | ">=" -> if i1 >= i2 then BOOLEAN true else BOOLEAN false
            | ">"  -> if i1 >  i2 then BOOLEAN true else BOOLEAN false
            | _ -> failwith ("unknown primitive " + ope)
        (res, store2)

    | Prim3 (e1, e2, e3) -> 
        let (i1, store1) = eval e1 locEnv gloEnv store
        let (i2, store2) = eval e2 locEnv gloEnv store1
        let (i3, store3) = eval e3 locEnv gloEnv store2
        let res = if i1.int<>0 then i2 else i3
        (res, store3)
    | Andalso(e1, e2) ->
        let (i1, store1) as res = eval e1 locEnv gloEnv store

        if i1.int <> 0 then eval e2 locEnv gloEnv store1 else res
    | Orelse(e1, e2) ->
        let (i1, store1) as res = eval e1 locEnv gloEnv store

        if i1.int <> 0 then res else eval e2 locEnv gloEnv store1
    | Call(f, es) -> callfun f es locEnv gloEnv store
    | Println(op,e1) -> 
        let (i1, store1) = eval e1 locEnv gloEnv store
        let res = 
            match op with
            | "%c"   -> (printfn "%c " i1.char; i1)
            | "%d"   -> (printfn "%d " i1.int; i1)  
            | "%f"   -> (printfn "%f " i1.float; i1 )
            | "%F"   -> (printfn "%F " i1.double; i1 )
            | "%s"   -> (printfn "%s " i1.string; i1 )
            | _      -> failwith ("wrong format")
        (res, store1)


and access acc locEnv gloEnv store : int * store =
    match acc with
    | AccVar x -> (lookup (fst locEnv) x, store)
    | AccDeref e -> 
        let (res,s) = eval e locEnv gloEnv store
        (res.int,s)
    | AccIndex(acc, idx) ->
        let (a, store1) = access acc locEnv gloEnv store
        let aval = getSto store1 a
        let (i, store2) = eval idx locEnv gloEnv store1
        // 检查下标是否越界
        let size = 
            match aval with
            | ARRAY (name,i,size) -> size
            | INT(_) -> failwith "Not Implemented"
            | FLOAT(_) -> failwith "Not Implemented"
            | DOUBLE(_) -> failwith "Not Implemented"
            | CHAR(_) -> failwith "Not Implemented"
            | STRING(_) -> failwith "Not Implemented"
            | BOOLEAN(_) -> failwith "Not Implemented"
            | POINTER(_) -> failwith "Not Implemented"
        if(i.int >= size || i.int < 0) then
            failwith( "index out of size" )
        else (aval.int + i.int, store2)

and evals es locEnv gloEnv store : enumType list * store =
    match es with
    | [] -> ([], store)
    | e1 :: er ->
        let (v1, store1) = eval e1 locEnv gloEnv store
        let (vr, storer) = evals er locEnv gloEnv store1
        (v1 :: vr, storer)

and callfun f es locEnv gloEnv store : enumType * store =

    msg <| sprintf "callfun: %A\n" (f, locEnv, gloEnv, store)

    let (_, nextloc) = locEnv
    let (varEnv, funEnv) = gloEnv
    let (paramdecs, fBody) = lookup funEnv f
    let (vs, store1) = evals es locEnv gloEnv store

    let (fBodyEnv, store2) =
        bindVars (List.map snd paramdecs) vs (varEnv, nextloc) store1

    let store3 = exec fBody fBodyEnv gloEnv store2
    // (-111, store3)
    let res = store3.TryFind(-1) 
    let restore = store3.Remove(-1)
    match res with
    | None -> ( BOOLEAN false,restore)
    | Some i -> (i,restore)

(* Interpret a complete micro-C program by initializing the store
   and global environments, then invoking its `main' function.
 *)

// run 返回的结果是 代表内存更改的 store 类型
// vs 参数列表 [8,2,...]
// 可以为空 []
let run (Prog topdecs) vs =
    //
    let ((varEnv, nextloc), funEnv, store0) = initEnvAndStore topdecs

    // mainParams 是 main 的参数列表
    //
    let (mainParams, mainBody) = lookup funEnv "main"

    let (mainBodyEnv, store1) =
        bindVars (List.map snd mainParams) vs (varEnv, nextloc) store0


    msg
    <|

    //以ex9.c为例子
    // main的 AST
    sprintf "\nmainBody:\n %A\n" mainBody
    +

    //局部环境
    // 如
    // i 存储在store位置0,store中下个空闲位置是1
    //([("i", 0)], 1)

    sprintf "\nmainBodyEnv:\n %A\n" mainBodyEnv
    +

    //全局环境 (变量,函数定义)
    // fac 的AST
    // main的 AST
    sprintf $"\n varEnv:\n {varEnv} \nfunEnv:\n{funEnv}\n"
    +

    //当前存储
    // store 中 0 号 位置存储值为8
    // map [(0, 8)]
    sprintf "\nstore1:\n %A\n" store1

    let endstore = exec mainBody mainBodyEnv (varEnv, funEnv) store1

    msg $"\nvarEnv:\n{varEnv}\n"
    msg $"\nStore:\n"
    msg <| store2str endstore

    endstore

(* Example programs are found in the files ex1.c, ex2.c, etc *)
