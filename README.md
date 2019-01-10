# Cuby
---
- 课程名称：编程语言原理与编译
- 实验项目：期末大作业
- 专业班级：计算机1603
- 学生学号：31601147，31601149
- 学生姓名：胡煜，江瑜
- 实验指导教师: 郭鸣
---

## 简介
这是一个编译原理大作业，主要基于microC完成的，这个之所以取名为Cuby，主要是在看`《计算的本质》`这本书的时候，发现Ruby是一门非常好玩有趣的语言，相比C++的错综复杂来说，Ruby是一个集成了优雅与复杂的语言，比如在`irb`下：
```ruby
>> 3.times{puts("Hello world")}
Hello world
Hello world
Hello world
=> 3
>>
```
我看到这门语言的时候我就惊呆了，居然语言还可以这样玩。而C++却令人反胃，实在是太恶心了，虽然说C++给你提供了所有你想用的，但是学习成本高，任何东西都感觉不伦不类的。  
Ruby是一门完全面向对象的编程语言，我尝试去实现面向对象的功能，本来取名叫Yuby，奈何实现面向对象实在太难了，我光是看JVM的指令集就很困难了，还要实现一大堆类库，实在太过于困难了，中途也下过Ruby的源代码，是用C写成的。最后还是放弃了实现面向对象的功能，选择结合microC与Ruby的语法方面作为我们大作业的方向。  
我们打算完善microC并加入Ruby 的 语法，最后如果还有时间的话能够完成面向对象的一个类(最后还是没时间了)。


## 结构
- 前端：由`F#`语言编写而成  
  - `CubyLex.fsl`生成的`CubyLex.fs`词法分析器。
  - `CubyPar.fsy`生成的`CubyPar.fs`语法分析器。
  - `AbstractSyntax.fs` 定义了抽象语法树
  - `Assembly.fs`定义了中间表示的生成指令集
  - `Compile.fs`将抽象语法树转化为中间表示

- 后端：由`Java`语言编写而成
  - `Machine.java`生成`Machine.class`虚拟机与`Machinetrace.class`堆栈追踪

- 测试集：测试程序放在`testing`文件夹内

- 库：`.net`支持
  - `FsLexYacc.Runtime.dll`
## 用法

`fslex --unicode CubyLex.fsl`  
生成`CubyLex.fs`词法分析器

`fsyacc --module CubyPar CubyPar.fsy`  
生成`CubyPar.fs`语法分析器与`CubyPar.fsi`  

`javac Machine.java`  
生成虚拟机

`fsi -r FsLexYacc.Runtime.dll AbstractSyntax.fs CubyPar.fs CubyLex.fs Parse.fs Assembly.fs Compile.fs ParseAndComp.fs`  
可以启用`fsi`的运行该编译器。

在`fsi`中输入:  
`open ParseAndComp;;`

之后则可以在`fsi`中使用使用：  

- `fromString`：从字符串中进行编译

- `fromFile`：从文件中进行编译

- `compileToFile`：生成中间表示

例子：

```fsharp
compileToFile (fromFile "ex11.c") "ex11.out";;  
#q;;
// 将文件ex11.c编译，生成中间表示存入文件"ex11.out"

fromString "int a;"
```

生成中间表示之后，便可以使用虚拟机对中间代码进行运行得出结果：



虚拟机功能：
- `java Machine` 运行中间表示
- `java Machinetrace` 追踪堆栈变化

例子：
```bash
java Machine ex11.out 8

java Machinetrace ex9.out 0
```

## 功能实现


## 技术评价

## 心得体会

## 小组分工

- 胡煜
  - 文档编写
  - 主要负责后端和中间代码生成  
- 江瑜
  - 测试程序
  - 语法、词法分析

- 权重分配表：  

|胡煜|江瑜|
|---|---|
|0.95|0.95|






