module Compile

open System.IO
open AbstractSyntax
open Assembly

type bstmtordec = 
    | BDec of instruction list
    | BStmt of IStatement

let rec addINCSP m1 C : instruction list = 
    match C with
    | INCSP m2              :: C1   -> addINCSP (m1+m2) C1
    | RET m2                :: C1   -> RET (m2-m1) :: C1
    | Label lab :: RET m2   :: _    -> RET (m2-m1) :: C
    | _                             -> if m1=0 then C else INCSP m1 :: C 

let addLabel C : label * instruction list =
    match C with
    | Label lab :: _ -> (lab, C)
    | GOTO lab  :: _ -> (lab, C)
    | _              -> let lab = newLabel()
                        (lab, Label lab :: C)
                        

let makeJump C : instruction * instruction list = 
    match C with
    | RET m                 :: _ -> (RET m, C)
    | Label lab :: RET m    :: _ -> (RET m, C)
    | Label lab             :: _ -> (GOTO lab, C)
    | GOTO lab              :: _ -> (GOTO lab, C)
    | _                          -> let lab = newLabel()
                                    (GOTO lab, Label lab :: C)
                                   
let makeCall m lab C : instruction list = 
    match C with
    | RET n             :: C1 -> TCALL(m, n, lab) :: C1
    | Label _ :: RET n  :: _  -> TCALL(m, n, lab) :: C
    | _                       -> CALL(m, lab) :: C

let rec deadcode C =
    match C with
    | []                -> []
    | Label lab :: _    -> C
    | _         :: C1   -> deadcode C1

let addNOT C = 
    match C with
    | NOT        :: C1 -> C1
    | IFZERO lab :: C1 -> IFNZRO lab :: C1
    | IFNZRO lab :: C1 -> IFZERO lab :: C1
    | _                -> NOT :: C

let addJump jump C =
    let C1 = deadcode C
    match (jump, C1) with
    | (GOTO lab1, Label lab2 :: _)  -> if lab1=lab2 then C1
                                       else GOTO lab1 :: C1
    | _                             -> jump :: C1


let addGOTO lab C =
    addJump (GOTO lab) C

let rec addCST i C = 
    match (i, C) with
    | (0, ADD       :: C1) -> C1
    | (0, SUB       :: C1) -> C1
    | (0, NOT       :: C1) -> addCST 1 C1
    | (_, NOT       :: C1) -> addCST 1 C1
    | (1, MUL       :: C1) -> C1
    | (1, DIV       :: C1) -> C1
    | (0, EQ        :: C1) -> addNOT C1
    | (_, INCSP m   :: C1) -> if m < 0 then addINCSP (m+1) C1
                              else CSTI i :: C 
    | (0, IFZERO lab :: C1) -> addGOTO lab C1
    | (_, IFZERO lab :: C1) -> C1
    | (0, IFNZRO lab :: C1) -> C1
    | (_, IFNZRO lab :: C1) -> addGOTO lab C1
    | _                     -> CSTI i :: C


type 'data Env = (string * 'data) list

let rec lookup env x = 
    match env with
    | []            -> failwith(x + " not found")
    | (y, v)::yr    -> if x=y then v else lookup yr x

type Var = 
    | Glovar of int
    | Locvar of int

type VarEnv = (Var * IPrimitiveType) Env * int

type Paramdecs = (IPrimitiveType * string) list

type FunEnv = (label * IPrimitiveType option * Paramdecs) Env


let allocate (kind : int -> Var) (typ, x) (varEnv : VarEnv) : VarEnv * instruction list =
    let (env, fdepth) = varEnv
    match typ with
    | TypeArray (TypeArray _, _)    -> failwith "Warning: allocate-arrays of arrays not permitted" 
    | TypeArray (t, Some i)         ->
        let newEnv = ((x, (kind (fdepth+i), typ)) :: env, fdepth+i+1)
        let code = [INCSP i; GETSP; CSTI (i-1); SUB]
        (newEnv, code)
    | _     ->
        let newEnv = ((x, (kind (fdepth), typ)) :: env, fdepth+1)
        let code = [INCSP 1]
        (newEnv, code)

let bindParam (env, fdepth) (typ, x) : VarEnv =
    ((x, (Locvar fdepth, typ)) :: env, fdepth+1);

let bindParams paras (env, fdepth) : VarEnv = 
    List.fold bindParam (env, fdepth) paras;





let rec cStmt stmt (varEnv : VarEnv) (funEnv : FunEnv) (C : instruction list) : instruction list = 
    match stmt with
    // | Case (_, _)   ->  
    | If(e, stmt1, stmt2) ->
        let (jumpend, C1) = makeJump C
        let (labelse, C2) = addLabel (cStmt stmt2 varEnv funEnv C1)
        cExpr e varEnv funEnv (IFZERO labelse :: cStmt stmt1 varEnv funEnv (addJump jumpend C2))
    | While(e, body) ->
        let labbegin = newLabel()
        let (jumptest, C1) = 
            makeJump (cExpr e varEnv funEnv (IFNZRO labbegin :: C))
        addJump jumptest (Label labbegin :: cStmt body varEnv funEnv C1)

    | For(dec, e, opera,body) ->   
        let labbegin = newLabel()                       //设置label 
        let (jumptest, C2) =                                                
            makeJump (cExpr e varEnv funEnv (IFNZRO labbegin :: C)) 
        let C3 = cExpr opera varEnv funEnv (addINCSP -1 C2)
        let C4 = cStmt body varEnv funEnv C3    
        cExpr dec varEnv funEnv (addINCSP -1 (addJump jumptest  (Label labbegin :: C4) ) ) //dec Label: body  opera  testjumpToBegin 指令的顺序
// compileToFile (fromFile "testing/ex(for).c ") "testing/ex(for).out";;     
    | Range(dec,i1,i2,body) ->
        let rec tmp stat =
            match stat with
            | Access (c) -> c               //get IAccess
        let ass = Assign (tmp dec,i1)
        let judge = BinaryPrimitiveOperator ("<",Access (tmp dec),i2)
        let opera = Assign (tmp dec, BinaryPrimitiveOperator ("+",Access (tmp dec),ConstInt 1))
        cStmt (For (ass,judge,opera,body))    varEnv funEnv C
    | Expression e ->
        cExpr e varEnv funEnv (addINCSP -1 C)
    | Block stmts ->
        let rec pass1 stmts ((_, fdepth) as varEnv) = 
            match stmts with
            | []        -> ([], fdepth)
            | s1::sr    ->
                let (_, varEnv1) as res1 = bStmtordec s1 varEnv
                let (resr, fdepthr) = pass1 sr varEnv1
                (res1 :: resr, fdepthr)
        let (stmtsback, fdepthend) = pass1 stmts varEnv
        let rec pass2 pairs C =
            match pairs with
            | [] -> C            
            | (BDec code, varEnv)  :: sr -> code @ pass2 sr C
            | (BStmt stmt, varEnv) :: sr -> cStmt stmt varEnv funEnv (pass2 sr C)
        pass2 stmtsback (addINCSP(snd varEnv - fdepthend) C)
    | Return None ->
        RET (snd varEnv - 1) :: deadcode C
    | Return (Some e) ->
        cExpr e varEnv funEnv (RET (snd varEnv) :: deadcode C)

and bStmtordec stmtOrDec varEnv : bstmtordec * VarEnv =
    match stmtOrDec with
    | Statement stmt    ->
        (BStmt stmt, varEnv)
    | Declare (typ, x)  ->
        let (varEnv1, code) = allocate Locvar (typ, x) varEnv
        (BDec code, varEnv1)
    | DeclareAndAssign (typ, x, e) ->
        let (varEnv1, code) = allocate Locvar (typ, x) varEnv
        (BDec (cAccess (AccessVariable(x)) varEnv1 [] (cExpr e varEnv1 [] (STI :: (addINCSP -1 code)))), varEnv1)

and cExpr (e : IExpression) (varEnv : VarEnv) (funEnv : FunEnv) (C : instruction list) : instruction list =
    match e with
    | Access acc        -> cAccess acc varEnv funEnv (LDI :: C)
    | Assign(acc, e)    -> cAccess acc varEnv funEnv (cExpr e varEnv funEnv (STI :: C))
    | ConstInt i        -> addCST i C
    // | ConstChar _       -> 
    | Address acc       -> cAccess acc varEnv funEnv C
    | UnaryPrimitiveOperator(ope, e1) ->
        cExpr e1 varEnv funEnv
            (match ope with
            | "!"       -> addNOT C
            | "printi"  -> PRINTI :: C
            | "printc"  -> PRINTC :: C
            | _         -> failwith "Error: unknown unary operator")
    | BinaryPrimitiveOperator(ope, e1, e2)    ->
        cExpr e1 varEnv funEnv
            (cExpr e2 varEnv funEnv
                (match ope with
                | "*"   -> MUL  ::  C
                | "+"   -> ADD  ::  C
                | "-"   -> SUB  ::  C
                | "/"   -> DIV  ::  C
                | "%"   -> MOD  ::  C
                | "=="  -> EQ   ::  C
                | "!="  -> EQ   ::  addNOT C
                | "<"   -> LT   ::  C
                | ">="  -> LT   ::  addNOT C
                | ">"   -> SWAP ::  LT  :: C
                | "<="  -> SWAP ::  LT  :: addNOT C
                | _     -> failwith "Error: unknown binary operator"))

    | AndOperator(e1, e2)   ->
        match C with
        | IFZERO lab :: _ ->
            cExpr e1 varEnv funEnv (IFZERO lab :: cExpr e2 varEnv funEnv C)
        | IFNZRO labthen :: C1 ->
            let (labelse, C2) = addLabel C1
            cExpr e1 varEnv funEnv
                (IFZERO labelse 
                    :: cExpr e2 varEnv funEnv (IFNZRO labthen :: C2))
        | _ ->
            let (jumpend, C1)   = makeJump C
            let (labfalse, C2)  = addLabel (addCST 0 C1)
            cExpr e1 varEnv funEnv
                (IFZERO labfalse
                    :: cExpr e2 varEnv funEnv (addJump jumpend C2))
    | OrOperator(e1, e2)    ->
        match C with
        | IFNZRO lab :: _ ->
            cExpr e1 varEnv funEnv (IFNZRO lab :: cExpr e2 varEnv funEnv C)
        | IFZERO labthen :: C1 ->
            let(labelse, C2) = addLabel C1
            cExpr e1 varEnv funEnv
                (IFNZRO labelse :: cExpr e2 varEnv funEnv
                    (IFZERO labthen :: C2))
        | _ ->
            let (jumpend, C1) = makeJump C
            let (labtrue, C2) = addLabel(addCST 1 C1)
            cExpr e1 varEnv funEnv
                (IFNZRO labtrue
                    :: cExpr e2 varEnv funEnv (addJump jumpend C2))
    | CallOperator(f, es)   -> callfun f es varEnv funEnv C
    // | AccessMember (_, _)


and makeGlobalEnvs(topdecs : TopDeclare list) : VarEnv * FunEnv * instruction list =
    let rec addv decs varEnv funEnv =
        match decs with
        | [] -> (varEnv, funEnv, [])
        | dec::decr ->
            match dec with
            | VariableDeclare (typ, x) -> 
                let (varEnv1, code1) = allocate Glovar (typ, x) varEnv
                let (varEnvr, funEnvr, coder) = addv decr varEnv1 funEnv
                (varEnvr, funEnvr, code1 @ coder)
            | VariableDeclareAndAssign (typ, x, e) -> 
                let (varEnv1, code1) = allocate Glovar (typ, x) varEnv
                let (varEnvr, funEnvr, coder) = addv decr varEnv1 funEnv
                (varEnvr, funEnvr, code1 @ (cAccess (AccessVariable(x)) varEnvr funEnvr (cExpr e varEnvr funEnvr (STI :: (addINCSP -1 coder)))))
            | FunctionDeclare (tyOpt, f, xs, body) ->
                addv decr varEnv ((f, (newLabel(), tyOpt, xs)) :: funEnv)
            
    addv topdecs ([], 0) []
and cAccess access varEnv funEnv C =
    match access with
    | AccessVariable x  ->
        match lookup (fst varEnv) x with
        | Glovar addr, _ -> addCST addr C
        | Locvar addr, _ -> GETBP :: addCST addr (ADD :: C)
    | AccessDeclareReference e ->
        cExpr e varEnv funEnv C
    | AccessIndex(acc, idx)    ->
        cAccess acc varEnv funEnv (LDI :: cExpr idx varEnv funEnv (ADD :: C))


and cExprs es varEnv funEnv C = 
    match es with
    | []        -> C
    | e1::er    -> cExpr e1 varEnv funEnv (cExprs er varEnv funEnv C)


and callfun f es varEnv funEnv C : instruction list =
    let (labf, tyOpt, paramdecs) = lookup funEnv f
    let argc = List.length es
    if argc = List.length paramdecs then
        cExprs es varEnv funEnv (makeCall argc labf C)
    else
        failwith (f + ": parameter/argument mismatch")


let cProgram (Prog topdecs) : instruction list = 
    let _ = resetLabels ()
    let ((globalVarEnv, _), funEnv, globalInit) = makeGlobalEnvs topdecs
    let compilefun (tyOpt, f, xs, body) = 
        let (labf, _, paras)    = lookup funEnv f
        let (envf, fdepthf)     = bindParams paras (globalVarEnv, 0)
        let C0                  = [RET (List.length paras-1)]
        let code                = cStmt body (envf, fdepthf) funEnv C0
        Label labf :: code
    let functions = 
        List.choose (function 
                        | FunctionDeclare (rTy, name, argTy, body)
                                            ->  Some (compilefun (rTy, name, argTy, body))
                        | VariableDeclare _ -> None
                        | VariableDeclareAndAssign _ -> None)
                        topdecs
    let (mainlab, _, mainparams) = lookup funEnv "main"
    let argc = List.length mainparams
    globalInit
    @ [LDARGS; CALL(argc, mainlab); STOP]
    @ List.concat functions



let intsToFile (inss : int list) (fname : string) =
    File.WriteAllText(fname, String.concat " " (List.map string inss))


let contCompileToFile program fname =
    let instrs      = cProgram program
    let bytecode    = code2ints instrs
    intsToFile bytecode fname; instrs
