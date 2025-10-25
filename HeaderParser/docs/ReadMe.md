# HeaderParse45 运行原理 & 映射规则全文档

## 1 总体流程
1. 读整个头文件 → **去注释**  
2. **逐行抽简单 `#define`**（跳过函数宏）  
3. **把已识别的宏按整词边界做一次词法替换**  
4. **正则一次性抓**  
   - `typedef struct {…} Name;`  
   - `struct Name {…};`  
   - `typedef enum {…} Name;`  
   - `enum Name {…};`  
5. 对匹配到的 **body 段** 再做二次解析  
   - struct body → 按 `;` 切行 → 每行拆 **类型部** 与 **声明部** → 再拆逗号 → 指针/数组/变量名  
   - enum body → 按顶层逗号切 → 解析 `=` 号 → 字面量或自增  
6. 结果写 XML

---

## 2 能解析什么

| 语法          | 示例                          | 输出字段               |
|---------------|-------------------------------|------------------------|
| typedef 结构体 | `typedef struct { ... } Foo;` | `<Struct name="Foo">`  |
| 匿名结构体     | `struct Bar { ... };`         | `<Struct name="Bar">`  |
| typedef 枚举   | `typedef enum { ... } Baz;`   | `<Enum name="Baz">`    |
| 匿名枚举       | `enum Baz { ... };`           | `<Enum name="Baz">`    |
| 简单宏         | `#define MAX 100`             | `<Define name="MAX" value="100">` |

❌ **不支持的**（会被安全跳过，不报错）  
函数指针、位域、函数式宏、#ifdef、#include、注释里的代码。

---

## 3 输出 XML 格式

```xml
<Header>
  <Defines>
    <Define name="VERSION" value="3"/>
  </Defines>

  <Structs>
    <Struct name="Point">
      <Field type="int" name="x" pointerLevel="0"/>
      <Field type="int" name="y" pointerLevel="0"/>
    </Struct>
  </Structs>

  <Enums>
    <Enum name="Color">
      <Item name="RED"   raw="0"/>
      <Item name="GREEN" raw="1"/>
      <Item name="BLUE"  raw="2"/>
    </Enum>
  </Enums>
</Header>
```

---

## 4 类型系统映射规则

| C 写法            | CType | PointerLevel | ArraySuffix |
|-------------------|-------|--------------|-------------|
| `int* p`          | int   | 1            | null        |
| `char buf[64]`    | char  | 0            | [64]        |
| `float** mat[4]`  | float | 2            | [4]         |

多维数组按原文本保留：`int a[2][3]` → `ArraySuffix="[2][3]"`

---

## 5 枚举值计算策略

- 显式 `=` → 直接记录原始文本 & 尝试解析十进制 / 十六进制 / 字符常量。  
- 未显式赋值 → 自增序列（从上一个值 +1，初始 -1→0）。  
- 复杂表达式（位运算、宏组合）→ 仅保留 `raw`，`parsedValue` 留空。

---

## 6 宏替换机制

只处理“简单宏”：

- 右值是 **字面量** / **字符常量** / **十六进制数**  
- 或 **单个标识符** 且该标识符已被前面规则解析成字面量

替换方式：整词边界 `\bNAME\b` 一次性替换，**只做一层间接**，防止循环引用。

---

## 7 可扩展类型映射器（TypeMapper）

目的用于将 C/C++ 类型映射到目标类型系统（如 PLC 类型）。

---

| 功能 | 说明 |
|---|---|
| 指针统一映射 | 所有 `T*` / `T**` … 归到 `PointerType`（默认 `LONG`） |
| 正则规则表 | 支持外部 `TypeMapRules.txt` 或 `TypeMapRules.xml` |
| 归一化比对 | 去 `const/volatile`、压空白、小写后再匹配 |
| 加载顺序 | ① 同目录 `txt` → ② 同目录 `xml` → ③ 内置默认规则 |

---

### 7.1 规则文件示例（TypeMapRules.txt）

```
# 指针类型
pointer = LONG

# 16/32/64 位无符号
^(unsigned short|uint16_t)$ => UINT
^(unsigned int|uint32_t|dword)$ => UDINT
^(unsigned long long|uint64_t)$ => ULINT

# 有符号
^(char|int8_t)$ => SINT
^(short|int16_t)$ => INT
^(int|long|int32_t)$ => DINT
^(long long|int64_t)$ => LINT

# 浮点/布尔
^float$ => REAL
^double$ => LREAL
^bool$ => BOOL
```

---

### 7.2 XML 规则示例（TypeMapRules.xml）

```xml
<TypeMapping pointer="LONG">
  <Rule pattern="^(unsigned short|uint16_t)$" target="UINT"/>
  <Rule pattern="^(unsigned int|uint32_t|dword)$" target="UDINT"/>
  <Rule pattern="^float$" target="REAL"/>
  <Rule pattern="^double$" target="LREAL"/>
</TypeMapping>
```

---

### 7.3 使用方式

1. 把规则文件放 `主程序` 同目录  
2. 运行即自动加载；无文件则启用内置默认规则  
3. 输出 XML 中 `Field.type` 已是映射后的目标类型，例如：

```xml
<Field type="UDINT" name="id" pointerLevel="0"/>
<Field type="LONG"  name="data" pointerLevel="1"/>
<Field type="USINT" name="buf" pointerLevel="0" array="[64]"/>
```

---

## 8 关键正则 & 实现要点

- 多行匹配：`RegexOptions.Singleline`  
- 字段切分：`[^;{}]+;` （避开花括号，防止嵌套结构）  
- 指针/数组：`(\s*\[[^\]]*\]\s*)+$` 先拎走数组，再数前缀 `*`  
- 宏替换：`\bNAME\b` 只做 **一层间接** 防止循环  
- 全文一次性读入 → 正则 `Matches` 一次性返回所有结果 → 无流式状态机  
- 纯静态方法 + 只读正例 → **无共享状态**，可多线程同时 `Parse(string)`

---

## 9 记忆口诀

> **“先清注释 → 抽宏并替换 → 正则抓块 → 块内二次拆字段 → 规则映射写 XML”**