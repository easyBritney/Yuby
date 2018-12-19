module AbstractSyntax

type IPrimitiveType = 
    | TypeInt           
    | TypeChar
    | TypeString    (**)
    | TypeFloat     (**)
    | TypeStruct of 
    | TypeArray of IPrimitiveType * int option
    | TypePoint of IPrimitiveType

and IExpression = 
    | Access of IAccess                  (* x,   *x,     x[i]    *)
    | Assign of IAccess * IExpression    (* x=e, *x=e,   x[i]=e  *)
    | Address of IAccess                 
    | ConstInt of int   (*constant int*)
    | ConstString of string (*constant string*)
    | ConstFloat of float (*constant float*)
    | ConstChar of char (*constant char*) 
    | UnaryPrimitiveOperator of string * IExpression
    | BinaryPrimitiveOperator of string * IExpression * IExpression
    | AndOperator of IExpression * IExpression
    | OrOperator of IExpression * IExpression
    | CallOperator of string * IExpression list

and IAccess = 
    | AccessVariable of string
    | AccessDeclareReference of IExpression
    | AccessIndex of IAccess * IExpression

and IStatement =
    | If of IExpression * IStatement * IStatement
    | While of IExpression * IStatement
    | Expression of IExpression
    | Return of IExpression option
    | Block of StatementDeclare list
    | For of IExpression * IExpression * IExpression * IStatement (* normal for *)
    | Switch of IExpression * IStatement
    | Case of (int * IStatement) list

and IDeclare = 
    | Declare of IPrimitiveType * string

and StatementORDeclare = 
    | Declare of IPrimitiveType * string
    | Statement of IStatement

and TopDeclare =
    | FunctionDeclare of IPrimitiveType option * string * (IPrimitiveType * string) list * IStatement
    | VariableDeclare of IPrimitiveType * string

and Program = 
    | Prog of TopDeclare list