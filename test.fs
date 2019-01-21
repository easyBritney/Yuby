open System
open System.Collections.Generic
let a = "a"


let test a (name : string) (name2 : string)= 
    match a with
    | name -> "Hello"
    | name2 -> "World"

test a "b" "a"