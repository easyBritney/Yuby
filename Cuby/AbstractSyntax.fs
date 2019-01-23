module AbstractSyntax

type IPrimitiveType = 
    | TypeInt           
    | TypeChar
    | TypeString    (**)
    | TypeFloat     (**)
    | TypeVoid      (**)
    | TypeStruct of string
    | TypeArray of IPrimitiveType * int option
    | TypePoint of IPrimitiveType
    | Lambda of IPrimitiveType option * (IPrimitiveType * string) list * IStatement (*匿名*)

and IExpression = 
    | Access of IAccess                  (* x,   *x,     x[i]    *)
    | Assign of IAccess * IExpression    (* x=e, *x=e,   x[i]=e  *)
    | Address of IAccess                 
    | ConstInt of int   (*constant int*)
    | ConstString of string (*constant string*)
    | ConstFloat of float32 (*constant float*)
    | ConstChar of char (*constant char*) 
    | ConstNull of int (*default 0*)
    | NullExpression of int (*default 1*)
    | UnaryPrimitiveOperator of string * IExpression
    | BinaryPrimitiveOperator of string * IExpression * IExpression
    | TernaryPrimitiveOperator of IExpression * IExpression * IExpression
    | AndOperator of IExpression * IExpression
    | OrOperator of IExpression * IExpression
    | CallOperator of string * IExpression list

and IAccess = 
    | AccessVariable of string
    | AccessDeclareReference of IExpression
    | AccessIndex of IAccess * IExpression
    | AccessMember of IAccess * IAccess  (**)

and IStatement =
    | If of IExpression * IStatement * IStatement
    | While of IExpression * IStatement
    | DoWhile of IStatement * IExpression
    | Expression of IExpression
    | Return of IExpression option
    | Block of StatementORDeclare list
    | For of IExpression * IExpression * IExpression * IStatement (* normal for *)
    | Case of IExpression * IStatement
    | Switch of IExpression * IStatement list
    | Range of IExpression * IExpression * IExpression * IStatement 
    | Throw of IException
    | Try of IStatement * IStatement list
    | Catch of IException * IStatement
    | Break
    | Continue

and IException = 
    | Exception of string

and StatementORDeclare = 
    | Declare of IPrimitiveType * string
    | DeclareAndAssign of IPrimitiveType * string * IExpression
    | Statement of IStatement

and TopDeclare =
    | FunctionDeclare of IPrimitiveType option * string * (IPrimitiveType * string) list * IStatement
    | VariableDeclare of IPrimitiveType * string 
    | StructDeclare of  string * (IPrimitiveType * string) list 
    | VariableDeclareAndAssign of IPrimitiveType * string * IExpression

and Program = 
    | Prog of TopDeclare list