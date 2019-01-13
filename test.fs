open System
open System.Collections.Generic
let a = 'a'


(int32)(System.BitConverter.ToInt16((System.BitConverter.GetBytes(char(a)), 0)))

