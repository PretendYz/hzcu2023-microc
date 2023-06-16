// Signature file for parser generated by fsyacc
module CPar
type token = 
  | EOF
  | PLUS_ASSIGN
  | MINUS_ASSIGN
  | TIMES_ASSIGN
  | DIV_ASSIGN
  | MOD_ASSIGN
  | SIZEOF
  | TYPEOF
  | INC
  | DESC
  | QUE
  | COLON
  | SWITCH
  | CASE
  | DEFAULT
  | LPAR
  | RPAR
  | LBRACE
  | RBRACE
  | LBRACK
  | RBRACK
  | SEMI
  | COMMA
  | ASSIGN
  | AMP
  | ASSIGN1
  | NOT
  | SEQOR
  | SEQAND
  | EQ
  | NE
  | GT
  | LT
  | GE
  | LE
  | PLUS
  | MINUS
  | TIMES
  | DIV
  | MOD
  | TOCHAR
  | TOINT
  | TOFLOAT
  | CHAR
  | ELSE
  | IF
  | INT
  | FLOAT
  | DOUBLE
  | BOOL
  | STRING
  | NULL
  | PRINT
  | PRINTLN
  | RETURN
  | VOID
  | WHILE
  | FOR
  | DO
  | UNTIL
  | BREAK
  | CONTINUE
  | CSTSTRING of (string)
  | NAME of (string)
  | CSTBOOL of (bool)
  | CSTDOUBLE of (double)
  | CSTFLOAT of (float32)
  | CSTCHAR of (char)
  | CSTINT of (int)
type tokenId = 
    | TOKEN_EOF
    | TOKEN_PLUS_ASSIGN
    | TOKEN_MINUS_ASSIGN
    | TOKEN_TIMES_ASSIGN
    | TOKEN_DIV_ASSIGN
    | TOKEN_MOD_ASSIGN
    | TOKEN_SIZEOF
    | TOKEN_TYPEOF
    | TOKEN_INC
    | TOKEN_DESC
    | TOKEN_QUE
    | TOKEN_COLON
    | TOKEN_SWITCH
    | TOKEN_CASE
    | TOKEN_DEFAULT
    | TOKEN_LPAR
    | TOKEN_RPAR
    | TOKEN_LBRACE
    | TOKEN_RBRACE
    | TOKEN_LBRACK
    | TOKEN_RBRACK
    | TOKEN_SEMI
    | TOKEN_COMMA
    | TOKEN_ASSIGN
    | TOKEN_AMP
    | TOKEN_ASSIGN1
    | TOKEN_NOT
    | TOKEN_SEQOR
    | TOKEN_SEQAND
    | TOKEN_EQ
    | TOKEN_NE
    | TOKEN_GT
    | TOKEN_LT
    | TOKEN_GE
    | TOKEN_LE
    | TOKEN_PLUS
    | TOKEN_MINUS
    | TOKEN_TIMES
    | TOKEN_DIV
    | TOKEN_MOD
    | TOKEN_TOCHAR
    | TOKEN_TOINT
    | TOKEN_TOFLOAT
    | TOKEN_CHAR
    | TOKEN_ELSE
    | TOKEN_IF
    | TOKEN_INT
    | TOKEN_FLOAT
    | TOKEN_DOUBLE
    | TOKEN_BOOL
    | TOKEN_STRING
    | TOKEN_NULL
    | TOKEN_PRINT
    | TOKEN_PRINTLN
    | TOKEN_RETURN
    | TOKEN_VOID
    | TOKEN_WHILE
    | TOKEN_FOR
    | TOKEN_DO
    | TOKEN_UNTIL
    | TOKEN_BREAK
    | TOKEN_CONTINUE
    | TOKEN_CSTSTRING
    | TOKEN_NAME
    | TOKEN_CSTBOOL
    | TOKEN_CSTDOUBLE
    | TOKEN_CSTFLOAT
    | TOKEN_CSTCHAR
    | TOKEN_CSTINT
    | TOKEN_end_of_input
    | TOKEN_error
type nonTerminalId = 
    | NONTERM__startMain
    | NONTERM_Main
    | NONTERM_Topdecs
    | NONTERM_Topdec
    | NONTERM_Vardec
    | NONTERM_Vardesc
    | NONTERM_Fundec
    | NONTERM_Paramdecs
    | NONTERM_Paramdecs1
    | NONTERM_Block
    | NONTERM_StmtOrDecSeq
    | NONTERM_Stmt
    | NONTERM_StmtM
    | NONTERM_StmtU
    | NONTERM_Expr
    | NONTERM_ExprNotAccess
    | NONTERM_AtExprNotAccess
    | NONTERM_Access
    | NONTERM_Exprs
    | NONTERM_Exprs1
    | NONTERM_StmtCase
    | NONTERM_CstC
    | NONTERM_CstB
    | NONTERM_CstI
    | NONTERM_CstS
    | NONTERM_CstF
    | NONTERM_CstD
    | NONTERM_Type
/// This function maps tokens to integer indexes
val tagOfToken: token -> int

/// This function maps integer indexes to symbolic token ids
val tokenTagToTokenId: int -> tokenId

/// This function maps production indexes returned in syntax errors to strings representing the non terminal that would be produced by that production
val prodIdxToNonTerminal: int -> nonTerminalId

/// This function gets the name of a token as a string
val token_to_string: token -> string
val Main : (FSharp.Text.Lexing.LexBuffer<'cty> -> token) -> FSharp.Text.Lexing.LexBuffer<'cty> -> (Absyn.program) 
