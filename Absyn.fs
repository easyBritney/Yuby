
module Absyn

type typ =
  | TypI                             
  | TypC                             
  | TypA of typ * int option         (* Array type                  *)
                                                                   
and expr =                                                         
  | CstI of int                      (* Constant                    *)
  | Prim1 of string * expr           (* Unary primitive operator    *)
  | Prim2 of string * expr * expr    (* Binary primitive operator   *)
  | Andalso of expr * expr           (* Sequential and              *)
  | Orelse of expr * expr            (* Sequential or               *)
  | Call of string * expr list       (* Function call f(...)        *)
                                                                   
and stmt =                                                         
  | If of expr * stmt * stmt         (* Conditional                 *)
  | While of expr * stmt             (* While loop                  *)
  | Unless of expr * stmt *stmt   
  | Expr of expr                     (* Expression statement   e;   *)
  | Return of expr option            (* Return from method          *)
  | Block of stmtordec list          (* Block: grouping and scope   *)
                                                                   
and stmtordec =                                                    
  | Dec of typ * string              (* Local variable declaration  *)
  | Stmt of stmt                     (* A statement                 *)

and topdec = 
  | Fundec of typ option * string * (typ * string) list option * stmt  (* 可选参数 函数*)
  | Vardec of typ * string

and program = 
  | Prog of topdec list
